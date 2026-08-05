function blockSong(songId, title) {
    const confirmed = confirm(`Bạn có chắc muốn chặn bài hát "${title}" không? Người dùng sẽ không thể thấy bài hát này nữa.`);
    if (!confirmed) return;
    fetch(`/AdminSongs/Block/${songId}`, {
        method: 'POST'
    })
        .then(async res => {
            const data = await res.json().catch(() => ({}));
            if (res.ok) {
                applyBlockedUI(songId, title, true);
                showToast(data.message || `Đã chặn "${title}".`);
            } else {
                showToast(data.message || 'Chặn thất bại.', true);
            }
        })
        .catch(() => showToast('Lỗi kết nối.', true));
}

function unblockSong(songId, title) {
    const confirmed = confirm(`Bạn có chắc muốn bỏ chặn bài hát "${title}" không?`);
    if (!confirmed) return;
    fetch(`/AdminSongs/Unblock/${songId}`, {
        method: 'POST'
    })
        .then(async res => {
            const data = await res.json().catch(() => ({}));
            if (res.ok) {
                applyBlockedUI(songId, title, false);
                showToast(data.message || `Đã bỏ chặn "${title}".`);
            } else {
                showToast(data.message || 'Bỏ chặn thất bại.', true);
            }
        })
        .catch(() => showToast('Lỗi kết nối.', true));
}

function applyBlockedUI(songId, title, blocked) {
    const statusCell = document.getElementById(`status-cell-${songId}`);
    if (statusCell) {
        statusCell.innerHTML = blocked
            ? '<span class="badge" style="background:#e5484d33;color:#e5484d;">Đã chặn</span>'
            : '<span class="badge" style="background:#1db95433;color:#1db954;">Bình thường</span>';
    }
    const actionsCell = document.getElementById(`actions-${songId}`);
    if (actionsCell) {
        actionsCell.innerHTML = blocked
            ? `<button class="btn-icon success" title="Bỏ chặn" onclick="unblockSong(${songId}, '${title}')"><i class="ti ti-lock-open"></i></button>`
            : `<button class="btn-icon danger" title="Chặn" onclick="blockSong(${songId}, '${title}')"><i class="ti ti-lock"></i></button>`;
    }
}