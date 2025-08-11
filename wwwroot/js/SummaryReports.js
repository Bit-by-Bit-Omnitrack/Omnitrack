document.addEventListener("DOMContentLoaded", function () {
    const totalTicketsEl = document.getElementById("totalTickets");
    const openTicketsEl = document.getElementById("openTickets");
    const closedTicketsEl = document.getElementById("closedTickets");
    const avgResolutionEl = document.getElementById("avgResolution");

    let statusChart, priorityChart, projectChart, userChart, trendChart;

    function fetchReportData() {
        const filters = {
            startDate: document.getElementById("startDate").value,
            endDate: document.getElementById("endDate").value,
            projectId: document.getElementById("projectSelect").value,
            userId: document.getElementById("userSelect").value
        };

        fetch("/Report/GetSummary", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(filters)
        })
            .then(res => res.json())
            .then(data => {
                // Update KPI Cards
                totalTicketsEl.textContent = data.totalTickets || 0;
                openTicketsEl.textContent = data.openTickets || 0;
                closedTicketsEl.textContent = data.closedTickets || 0;
                avgResolutionEl.textContent = data.avgResolutionDays || 0;

                // Render Charts
                renderCharts(data);
            })
            .catch(err => {
                console.error("Error fetching report:", err);
            });
    }

    function renderCharts(data) {
        const chartOptions = { responsive: true, maintainAspectRatio: false };

        if (statusChart) statusChart.destroy();
        statusChart = new Chart(document.getElementById("statusChart"), {
            type: "pie",
            data: {
                labels: ["Open", "In Progress", "Closed"],
                datasets: [{ data: data.statusCounts, backgroundColor: ["#f39c12", "#3498db", "#2ecc71"] }]
            },
            options: chartOptions
        });

        if (priorityChart) priorityChart.destroy();
        priorityChart = new Chart(document.getElementById("priorityChart"), {
            type: "doughnut",
            data: {
                labels: ["Low", "Medium", "High", "Critical"],
                datasets: [{ data: data.priorityCounts, backgroundColor: ["#27ae60", "#f1c40f", "#e67e22", "#e74c3c"] }]
            },
            options: chartOptions
        });

        if (projectChart) projectChart.destroy();
        projectChart = new Chart(document.getElementById("projectChart"), {
            type: "bar",
            data: {
                labels: data.projects.map(p => p.name),
                datasets: [{ label: "Tickets", data: data.projects.map(p => p.count), backgroundColor: "#2980b9" }]
            },
            options: chartOptions
        });

        if (userChart) userChart.destroy();
        userChart = new Chart(document.getElementById("userChart"), {
            type: "bar",
            data: {
                labels: data.users.map(u => u.name),
                datasets: [{ label: "Tickets", data: data.users.map(u => u.count), backgroundColor: "#8e44ad" }]
            },
            options: chartOptions
        });

        if (trendChart) trendChart.destroy();
        trendChart = new Chart(document.getElementById("trendChart"), {
            type: "line",
            data: {
                labels: data.trends.map(t => t.date),
                datasets: [{ label: "Tickets", data: data.trends.map(t => t.count), borderColor: "#16a085", fill: false }]
            },
            options: chartOptions
        });
    }

    document.getElementById("filterForm").addEventListener("submit", function (e) {
        e.preventDefault();
        fetchReportData();
    });

    document.getElementById("clearFilters").addEventListener("click", function () {
        document.getElementById("filterForm").reset();
        fetchReportData();
    });

    // Initial Load
    fetchReportData();
});
