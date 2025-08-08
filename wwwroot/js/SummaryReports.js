document.addEventListener("DOMContentLoaded", function () {
    fetch('/Report/GetSummaryReport')
        .then(res => res.json())
        .then(data => {
            //  Status Chart
            new Chart(document.getElementById('statusChart'), {
                type: 'pie',
                data: {
                    labels: Object.keys(data.statusCounts),
                    datasets: [{
                        data: Object.values(data.statusCounts),
                        backgroundColor: ['#36A2EB', '#FF6384', '#FFCE56', '#4BC0C0']
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        title: {
                            display: true,
                            text: 'Tickets by Status'
                        },
                        legend: {
                            position: 'bottom'
                        }
                    }
                }
            });

            //  Project Chart
            new Chart(document.getElementById('projectChart'), {
                type: 'bar',
                data: {
                    labels: Object.keys(data.projectCounts),
                    datasets: [{
                        label: 'Tickets per Project',
                        data: Object.values(data.projectCounts),
                        backgroundColor: '#4BC0C0'
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        title: {
                            display: true,
                            text: 'Tickets per Project'
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });

            //  Priority Chart
            new Chart(document.getElementById('priorityChart'), {
                type: 'doughnut',
                data: {
                    labels: Object.keys(data.priorityCounts),
                    datasets: [{
                        data: Object.values(data.priorityCounts),
                        backgroundColor: ['#FF6384', '#FFCE56', '#36A2EB']
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        title: {
                            display: true,
                            text: 'Tickets by Priority'
                        },
                        legend: {
                            position: 'bottom'
                        }
                    }
                }
            });

            // User Chart
            new Chart(document.getElementById('userChart'), {
                type: 'horizontalBar', // Chart.js v2 syntax; use 'bar' with indexAxis for v3+
                data: {
                    labels: Object.keys(data.userCounts),
                    datasets: [{
                        label: 'Tickets per User',
                        data: Object.values(data.userCounts),
                        backgroundColor: '#FF9F40'
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        title: {
                            display: true,
                            text: 'Tickets per User'
                        }
                    },
                    indexAxis: 'y',
                    scales: {
                        x: {
                            beginAtZero: true
                        }
                    }
                }
            });
        })
        .catch(error => {
            console.error('Error fetching summary report:', error);
        });
});