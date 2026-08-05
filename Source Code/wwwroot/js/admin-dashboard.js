document.addEventListener("DOMContentLoaded", () => {
    const spotifyGreen = "#1DB954";
    const chartColors = ["#1DB954", "#1ed760", "#535353", "#b3b3b3", "#ffffff", "#3d3d3d"];
    Chart.defaults.color = "#b3b3b3";
    Chart.defaults.font.family = "'Circular Std', Arial, sans-serif";
    new Chart(document.getElementById("streamTrendChart"), {
        type: "line",
        data: {
            labels: dashboardData.trendLabels,
            datasets: [{
                label: "Lượt nghe",
                data: dashboardData.trendValues,
                borderColor: spotifyGreen,
                backgroundColor: "rgba(29, 185, 84, 0.15)",
                fill: true,
                tension: 0.35,
                pointBackgroundColor: spotifyGreen
            }]
        },
        options: {
            plugins: { legend: { display: false } },
            scales: {
                x: { grid: { color: "#282828" } },
                y: { grid: { color: "#282828" }, beginAtZero: true }
            }
        }
    });
    new Chart(document.getElementById("genreChart"), {
        type: "pie",
        data: {
            labels: dashboardData.genreLabels,
            datasets: [{ data: dashboardData.genreValues, backgroundColor: chartColors }]
        },
        options: { plugins: { legend: { position: "bottom" } } }
    });
});