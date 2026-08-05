function resolveReport(reportId, newStatus) {
    const label = newStatus === 'Reviewed' ? 'đánh dấu đã xử lý' : 'bỏ qua';
    const confirmed = confirm(`Bạn có chắc muốn ${label} báo cáo này không?`);
    if (!confirmed) return;
    fetch(`/AdminArtistReports/Resolve/${reportId}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status: newStatus })
    })
        .then(async res => {
            const data = await res.json().catch(() => ({}));
            if (res.ok) {
                const row = document.getElementById(`report-row-${reportId}`);
                if (row) {
                    row.style.transition = 'opacity 0.3s';
                    row.style.opacity = '0';
                    setTimeout(() => row.remove(), 300);
                }
                showToast(data.message || 'Đã cập nhật báo cáo.');
            } else {
                showToast(data.message || 'Thao tác thất bại.', true);
            }
        })
        .catch(() => showToast('Lỗi kết nối.', true));
}