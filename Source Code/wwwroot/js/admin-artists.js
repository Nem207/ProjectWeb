async function blockArtist(id, name) {
    if (!confirm(`Bạn có chắc muốn chặn nghệ sĩ "${name}"? Nghệ sĩ và bài hát của họ sẽ không hiển thị với người dùng.`)) return;
    try {
        const response = await fetch(`/AdminArtists/Block/${id}`, {
            method: "POST"
        });
        const data = await response.json().catch(() => ({}));
        if (response.ok) {
            showToast(data.message || `Đã chặn nghệ sĩ "${name}"`);
            setTimeout(() => window.location.reload(), 500);
        } else {
            showToast(data.message || "Chặn thất bại.", true);
        }
    } catch (error) {
        console.error(error);
        showToast("Có lỗi xảy ra khi chặn nghệ sĩ.", true);
    }
}

async function unblockArtist(id, name) {
    if (!confirm(`Bạn có chắc muốn bỏ chặn nghệ sĩ "${name}"?`)) return;
    try {
        const response = await fetch(`/AdminArtists/Unblock/${id}`, {
            method: "POST"
        });
        const data = await response.json().catch(() => ({}));
        if (response.ok) {
            showToast(data.message || `Đã bỏ chặn nghệ sĩ "${name}"`);
            setTimeout(() => window.location.reload(), 500);
        } else {
            showToast(data.message || "Bỏ chặn thất bại.", true);
        }
    } catch (error) {
        console.error(error);
        showToast("Có lỗi xảy ra khi bỏ chặn nghệ sĩ.", true);
    }
}