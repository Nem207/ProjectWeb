const Report = (function () {
    let currentSongId = null;
    let currentSongTitle = "";

    const REASONS = [
        { value: "Copyright", label: "Vi phạm bản quyền" },
        { value: "Inappropriate", label: "Nội dung không phù hợp" },
        { value: "WrongInfo", label: "Sai thông tin bài hát" },
        { value: "PlaybackError", label: "Lỗi phát nhạc" },
        { value: "Other", label: "Khác" }
    ];

    function ensureModal() {
        if (document.getElementById("modalReportSong")) return;
        const overlay = document.createElement("div");
        overlay.className = "modal-overlay";
        overlay.id = "modalReportSong";
        overlay.innerHTML = `
            <div class="modal-box">
                <h2 class="modal-title">Báo cáo bài hát</h2>
                <p id="reportSongTitle"></p>
                <div class="report-reason-list" id="reportReasonList">
                    ${REASONS.map((r, i) => `
                        <label class="report-reason-option">
                            <input type="radio" name="reportReason" value="${r.value}" ${i === 0 ? "checked" : ""}>
                            <span>${r.label}</span>
                        </label>`).join("")}
                </div>
                <div class="form-group">
                    <label class="form-label" for="reportDescription">Mô tả thêm (không bắt buộc)</label>
                    <textarea class="form-input" id="reportDescription" rows="3" maxlength="1000" placeholder="Mô tả chi tiết vấn đề bạn gặp phải..."></textarea>
                </div>
                <div class="modal-actions">
                    <button class="btn btn-secondary" onclick="Report.close()">Huỷ</button>
                    <button class="btn btn-primary" id="btnSubmitReport" onclick="Report.submit()">Gửi báo cáo</button>
                </div>
            </div>`;
        document.body.appendChild(overlay);
        overlay.addEventListener("click", function (e) {
            if (e.target === overlay) Report.close();
        });
    }

    function open(songId, songTitle) {
        ensureModal();
        currentSongId = songId;
        currentSongTitle = songTitle || "";
        document.getElementById("reportSongTitle").textContent = currentSongTitle
            ? `Bài hát: "${currentSongTitle}"`
            : "";
        document.getElementById("reportDescription").value = "";
        const firstRadio = document.querySelector('#reportReasonList input[type="radio"]');
        if (firstRadio) firstRadio.checked = true;
        document.getElementById("modalReportSong").classList.add("open");
    }

    function close() {
        const overlay = document.getElementById("modalReportSong");
        if (overlay) overlay.classList.remove("open");
        currentSongId = null;
    }

    function submit() {
        if (!currentSongId) return;
        const selected = document.querySelector('#reportReasonList input[type="radio"]:checked');
        const reason = selected ? selected.value : "Other";
        const description = document.getElementById("reportDescription").value.trim();
        const btn = document.getElementById("btnSubmitReport");
        btn.disabled = true;
        btn.textContent = "Đang gửi...";
        fetch(`/api/song-report/${currentSongId}`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ reason, description })
        })
            .then(async res => {
                const data = await res.json().catch(() => ({}));
                if (res.ok) {
                    if (typeof Toast !== "undefined") Toast.success(data.message || "Đã gửi báo cáo.");
                    close();
                } else {
                    if (typeof Toast !== "undefined") Toast.error(data.message || "Gửi báo cáo thất bại.");
                }
            })
            .catch(() => {
                if (typeof Toast !== "undefined") Toast.error("Lỗi kết nối. Vui lòng thử lại.");
            })
            .finally(() => {
                btn.disabled = false;
                btn.textContent = "Gửi báo cáo";
            });
    }

    document.addEventListener("click", function (e) {
        const trigger = e.target.closest("[data-report-song-id]");
        if (!trigger) return;
        e.preventDefault();
        e.stopPropagation();
        const songId = trigger.getAttribute("data-report-song-id");
        const songTitle = trigger.getAttribute("data-report-song-title") || "";
        open(Number(songId), songTitle);
        document.querySelectorAll(".more-menu.open").forEach(m => m.classList.remove("open"));
    }, true);

    return { open, close, submit };
})();
window.Report = Report;