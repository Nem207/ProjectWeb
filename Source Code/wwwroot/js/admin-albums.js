async function deleteAlbum(id, title) {
    if (!confirm(`Bạn có chắc muốn xóa album "${title}"?`)) return;
    try {
        const response = await fetch(`/AdminAlbums/Delete/${id}`, {
            method: "DELETE"
        });
        if (response.ok) {
            const row = document.getElementById(`album-row-${id}`);
            row?.remove();
            showToast(`Đã xóa album "${title}"`);
        } else {
            const err = await response.json();
            showToast(err.message || "Xóa thất bại.", true);
        }
    } catch (error) {
        console.error(error);
        showToast("Có lỗi xảy ra khi xóa album.", true);
    }
}