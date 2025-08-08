document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById('filterForm');
    const startDateInput = document.getElementById('startDate');
    const endDateInput = document.getElementById('endDate');
    const projectSelect = document.getElementById('projectSelect');
    const userSelect = document.getElementById('userSelect');
    const exportTrendsBtn = document.getElementById('exportTrendsCsv');
    const exportProjectsBtn = document.getElementById('exportProjectsCsv');

    //  Restore filters from localStorage
    const savedFilters = JSON.parse(localStorage.getItem('reportFilters'));
    if (savedFilters) {
        startDateInput.value = savedFilters.startDate || '';
        endDateInput.value = savedFilters.endDate || '';
        projectSelect.value = savedFilters.project || '';
        userSelect.value = savedFilters.user || '';
    }

    form.addEventListener('submit', function (e) {
        e.preventDefault();
        saveFilters();
        loadCharts();
    });

    exportTrendsBtn.addEventListener('click', exportTrendsCsv);
    exportProjectsBtn.addEventListener('click', exportProjectsCsv);

    loadCharts(); // Initial load

    function saveFilters() {
        const filters = {
            startDate: startDateInput.value,
            endDate: endDateInput.value,
            project: projectSelect.value,
            user: userSelect.value
        };
        localStorage.setItem('reportFilters', JSON.stringify(filters));
    }

    function getQueryParams() {
        return new URLSearchParams({
            startDate: startDateInput.value,
            endDate: endDateInput.value,
            project: projectSelect.value,
            user: userSelect.value
        });
    }

    function loadCharts() {
        fetch(`/Report/GetSummaryReport?${getQueryParams()}`)
            .then(res => res.json())
            .then(data => {
                populateDropdowns(data.projects, data.users);
                renderChart('statusChart', 'Tickets by Status', data.statusCounts, 'pie');
                renderChart('priorityChart', 'Tickets by Priority', data.priorityCounts, 'doughnut');
                renderChart('projectChart', 'Tickets per Project', data.projectCounts, 'bar');
                renderChart('userChart', 'Tickets per User', data.userCounts, 'bar', true);
                renderLineChart('trendChart', 'Tickets Over Time', data.ticketTrends);
            });
    }

    function populateDropdowns(projects, users) {
        projectSelect.innerHTML = '<option value="">All Projects</option>';
        userSelect.innerHTML = '<option value="">All Users</option>';

        projects.forEach(p => {
            const selected = p === projectSelect.value ? 'selected' : '';
            projectSelect.innerHTML += `<option value="${p}" ${selected}>${p}</option>`;
        });

        users.forEach(u => {
            const selected = u === userSelect.value ? 'selected' : '';
            userSelect.innerHTML += `<option value="${u}" ${selected}>${u}</option>`;
        });
    }

    function renderChart(canvasId, title, data, type, horizontal = false) {
        const ctx = document.getElementById(canvasId);
        new Chart(ctx, {
            type: type,
            data: {
                labels: Object.keys(data),
                datasets: [{
                    label: title,
                    data: Object.values(data),
                    backgroundColor: ['#36A2EB', '#FF6384', '#FFCE56', '#4BC0C0', '#9966FF', '#FF9F40']
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    title: {
                        display: true,
                        text: title
                    },
                    legend: {
                        display: type !== 'bar'
                    }
                },
                indexAxis: horizontal ? 'y' : 'x',
                scales: type === 'bar' ? {
                    y: {
                        beginAtZero: true
                    }
                } : {}
            }
        });
    }

    function renderLineChart(canvasId, title, trendData) {
        const ctx = document.getElementById(canvasId);
        new Chart(ctx, {
            type: 'line',
            data: {
                labels: trendData.map(t => t.date),
                datasets: [{
                    label: title,
                    data: trendData.map(t => t.count),
                    borderColor: 'rgba(75, 192, 192, 1)',
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    fill: true,
                    tension: 0.3
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    title: {
                        display: true,
                        text: title
                    }
                },
                scales: {
                    x: {
                        title: {
                            display: true,
                            text: 'Date'
                        }
                    },
                    y: {
                        title: {
                            display: true,
                            text: 'Tickets'
                        },
                        beginAtZero: true
                    }
                }
            }
        });
    }

    function exportTrendsCsv() {
        fetch(`/Report/GetSummaryReport?${getQueryParams()}`)
            .then(res => res.json())
            .then(data => {
                const csv = ['Date,Count', ...data.ticketTrends.map(t => `${t.date},${t.count}`)].join('\n');
                downloadCsv(csv, 'filtered_ticket_trends.csv');
            });
    }

    function exportProjectsCsv() {
        fetch(`/Report/GetSummaryReport?${getQueryParams()}`)
            .then(res => res.json())
            .then(data => {
                const csv = ['Project,Count', ...Object.entries(data.projectCounts).map(([key, val]) => `${key},${val}`)].join('\n');
                downloadCsv(csv, 'filtered_project_counts.csv');
            });
    }

    function downloadCsv(content, filename) {
        const blob = new Blob([content], { type: 'text/csv' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        a.click();
        URL.revokeObjectURL(url);
    }
});