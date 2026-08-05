function deleteUser(userId, userName) {
    const confirmed = confirm(`Bạn có chắc muốn xóa người dùng "${userName}" không?`);
    if (!confirmed) return;
    fetch(`/AdminUsers/Delete/${userId}`, {
        method: 'DELETE',
        headers: { 'Content-Type': 'application/json' }
    })
        .then(res => {
            if (res.ok) {
                const row = document.getElementById(`user-row-${userId}`);
                if (row) {
                    row.style.transition = 'opacity 0.3s';
                    row.style.opacity = '0';
                    setTimeout(() => row.remove(), 300);
                }
                showToast(`Đã xóa người dùng "${userName}".`);
            } else {
                return res.json().then(data => {
                    showToast(data.message || 'Xóa thất bại.', true);
                });
            }
        })
        .catch(() => showToast('Lỗi kết nối. Vui lòng thử lại.', true));
}