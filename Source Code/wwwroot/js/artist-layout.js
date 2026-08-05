document.addEventListener("DOMContentLoaded", function () {
    const toggleBtn = document.getElementById("sidebarToggle");
    const sidebar = document.getElementById("artistSidebar");
    const overlay = document.getElementById("artistOverlay");
    function openSidebar() {
        sidebar.classList.add("open");
        overlay.classList.add("show");
    }
    function closeSidebar() {
        sidebar.classList.remove("open");
        overlay.classList.remove("show");
    }
    toggleBtn?.addEventListener("click", function () {
        sidebar.classList.contains("open") ? closeSidebar() : openSidebar();
    });
    overlay?.addEventListener("click", closeSidebar);
    document.querySelectorAll(".nav-item").forEach(function (link) {
        link.addEventListener("click", closeSidebar);
    });
});