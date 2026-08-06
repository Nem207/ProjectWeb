const AlbumDetail = (function () {
    let albumId = null;
    let albumName = '';
    let mainArtistId = null;
    let likedSongIdsCache = null;
    function loadLikedSongIds() {
        if (!window.LikedSongs || !window.IS_AUTHENTICATED) return Promise.resolve();
        return LikedSongs.loadLikedIds().then(set => {
            likedSongIdsCache = set;
            document.querySelectorAll('[id^="likeToggle-"]').forEach(btn => {
                const songId = parseInt(btn.id.replace('likeToggle-', ''), 10);
                refreshLikeButton(songId);
            });
        }).catch(err => console.error('Không tải được danh sách bài hát đã thích.', err));
    }
    function refreshLikeButton(songId) {
        const btn = document.getElementById('likeToggle-' + songId);
        if (!btn || !likedSongIdsCache) return;
        btn.textContent = likedSongIdsCache.has(songId) ? '✓ Xóa khỏi Bài hát đã thích' : '♡ Thêm vào Bài hát đã thích';
    }
    function toggleLikeSong(songId) {
        if (!window.IS_AUTHENTICATED) {
            Toast.info('Vui lòng đăng nhập để thích bài hát.');
            return;
        }
        LikedSongs.toggle(songId)
            .then(liked => {
                if (!likedSongIdsCache) likedSongIdsCache = new Set();
                if (liked) likedSongIdsCache.add(songId);
                else likedSongIdsCache.delete(songId);
                refreshLikeButton(songId);
                if (window.refreshSongAddButtons) window.refreshSongAddButtons(songId);
                Toast.success(liked ? 'Đã thêm vào Bài hát đã thích.' : 'Đã xóa khỏi Bài hát đã thích.');
            })
            .catch(err => console.error('Không thể cập nhật bài hát yêu thích.', err));
    }
    function copyLink(url, msg) {
        if (navigator.clipboard) {
            navigator.clipboard.writeText(url).then(() => Toast.success(msg));
        } else {
            Toast.info(url);
        }
    }
    function shareAlbum() {
        copyLink(window.location.href, 'Đã sao chép liên kết album!');
    }
    function shareSong(songId) {
        copyLink(`${window.location.origin}${window.location.pathname}#song-${songId}`, 'Đã sao chép liên kết bài hát!');
    }
    function showCredits(songId) {
        const data = (window.SONGS_DATA || {})[songId];
        if (!data) return;
        document.getElementById('creditsSongTitle').textContent = data.title;
        const list = document.getElementById('creditsList');
        list.innerHTML = data.artists.map(a =>
            `<div style="color:#e0e0e0;font-size:14px;">🎤 <a href="/Artist/Detail/${a.id}" style="color:#1ed760;">${a.name}</a></div>`
        ).join('') || '<div style="color:#b3b3b3;font-size:14px;">Chưa có thông tin ghi công.</div>';
        document.getElementById('creditsOverlay').classList.add('open');
    }
    function bindCreditsModal() {
        document.getElementById('btnCreditsClose').addEventListener('click', function () {
            document.getElementById('creditsOverlay').classList.remove('open');
        });
        document.getElementById('creditsOverlay').addEventListener('click', function (e) {
            if (e.target === this) this.classList.remove('open');
        });
    }
    function openReportModal() {
        document.querySelectorAll('input[name="reportReason"]').forEach(r => r.checked = false);
        document.getElementById('reportDetail').value = '';
        document.getElementById('reportOverlay').classList.add('open');
    }
    function closeReportModal() {
        document.getElementById('reportOverlay').classList.remove('open');
    }
    function submitReport() {
        const selected = document.querySelector('input[name="reportReason"]:checked');
        if (!selected) {
            Toast.error('Vui lòng chọn một lý do báo cáo.');
            return;
        }
        const reason = selected.value;
        const detail = document.getElementById('reportDetail').value.trim();
        console.log('Report album gửi:', { albumId, albumName, reason, detail });
        closeReportModal();
        Toast.success('Cảm ơn bạn đã báo cáo. Chúng tôi sẽ xem xét sớm nhất.');
    }
    function bindReportModal() {
        document.getElementById('btnReportCancel').addEventListener('click', closeReportModal);
        document.getElementById('btnReportSubmit').addEventListener('click', submitReport);
        document.getElementById('reportOverlay').addEventListener('click', function (e) {
            if (e.target === this) closeReportModal();
        });
    }
    function bindSongSubmenus() {
        document.querySelectorAll('#viewAlbumDetail .submenu-trigger').forEach(trigger => {
            trigger.addEventListener('click', function (e) {
                e.stopPropagation();
                const submenu = this.nextElementSibling;
                const willOpen = !submenu.classList.contains('open');
                document.querySelectorAll('#viewAlbumDetail .submenu.open').forEach(sm => sm.classList.remove('open'));
                submenu.classList.toggle('open', willOpen);
                if (willOpen) positionSubmenu(this, submenu);
                syncScrollLock();
            });
        });
    }
    function bindLibraryButton() {
        document.getElementById('btnAddLibrary').addEventListener('click', function () {
            if (window.PlaylistPicker) {
                const songIds = buildAlbumSongList().map(s => s.songId);
                window.PlaylistPicker.open(songIds);
            }
        });
    }
    function getRowSongInfo(row) {
        try {
            return JSON.parse(row.dataset.song);
        } catch {
            return null;
        }
    }
    function getAlbumRows() {
        return Array.from(document.querySelectorAll('#albumSongs .song-row'));
    }
    function buildAlbumSongList() {
        return getAlbumRows().map(getRowSongInfo).filter(Boolean);
    }
    function isAlbumActiveQueue(current) {
        if (!current) return false;
        return buildAlbumSongList().some(s => s.songId === current.songId);
    }
    function playRowAt(index) {
        const list = buildAlbumSongList();
        if (!list[index]) return;
        const current = window.Player && Player.getCurrent ? Player.getCurrent() : null;
        if (current && current.songId === list[index].songId) {
            Player.togglePlay();
            return;
        }
        Player.playFromList(list, index);
    }
    function bindSongRows() {
        getAlbumRows().forEach((row, index) => {
            row.addEventListener('click', function () {
                playRowAt(index);
            });
            const playMini = row.querySelector('.play-mini');
            if (playMini) {
                playMini.addEventListener('click', function (e) {
                    e.stopPropagation();
                    playRowAt(index);
                });
            }
        });
    }
    function updatePlaybackUI() {
        const current = window.Player && Player.getCurrent ? Player.getCurrent() : null;
        const playing = window.Player && Player.isPlaying ? Player.isPlaying() : false;
        const isThisAlbum = isAlbumActiveQueue(current);
        const actionBtn = document.getElementById('btnAlbumPlay');
        if (actionBtn) {
            actionBtn.textContent = (isThisAlbum && playing) ? '❚❚' : '▶';
        }
        getAlbumRows().forEach(row => {
            const songId = Number(row.dataset.songId);
            const isCurrentRow = !!(current && songId === current.songId);
            row.classList.toggle('row-playing', isCurrentRow);
            const playMini = row.querySelector('.play-mini');
            if (playMini) playMini.textContent = (isCurrentRow && playing) ? '❚❚' : '▶';
        });
    }
    function bindPlayShuffle() {
        document.getElementById('btnAlbumPlay').addEventListener('click', function () {
            if (!window.Player) return;
            const list = buildAlbumSongList();
            if (list.length === 0) return;
            const current = Player.getCurrent();
            if (isAlbumActiveQueue(current)) {
                Player.togglePlay();
            } else {
                Player.playFromList(list, 0);
            }
        });
    }
    function positionMenu(btn, menu) {
        const margin = 8;
        const btnRect = btn.getBoundingClientRect();
        const menuRect = menu.getBoundingClientRect();
        let top = btnRect.bottom + 4;
        let left = btnRect.right - menuRect.width;
        if (top + menuRect.height > window.innerHeight - margin) {
            top = btnRect.top - menuRect.height - 4;
        }
        if (top < margin) top = margin;
        if (left < margin) left = Math.min(btnRect.left, window.innerWidth - menuRect.width - margin);
        if (left + menuRect.width > window.innerWidth - margin) left = window.innerWidth - menuRect.width - margin;
        if (left < margin) left = margin;
        menu.style.position = 'fixed';
        menu.style.top = top + 'px';
        menu.style.left = left + 'px';
        menu.style.right = 'auto';
    }
    function positionSubmenu(trigger, submenu) {
        const margin = 8;
        const triggerRect = trigger.getBoundingClientRect();
        const subRect = submenu.getBoundingClientRect();
        let left = triggerRect.left - subRect.width - 4;
        let top = triggerRect.top;
        if (left < margin) {
            left = triggerRect.right + 4;
        }
        if (left + subRect.width > window.innerWidth - margin) {
            left = window.innerWidth - subRect.width - margin;
        }
        if (left < margin) left = margin;
        if (top + subRect.height > window.innerHeight - margin) {
            top = window.innerHeight - subRect.height - margin;
        }
        if (top < margin) top = margin;
        submenu.style.position = 'fixed';
        submenu.style.top = top + 'px';
        submenu.style.left = left + 'px';
        submenu.style.right = 'auto';
    }
    function syncScrollLock() {
        const mainContent = document.getElementById('mainContent');
        if (!mainContent) return;
        const anyOpen = !!document.querySelector(
            '#viewAlbumDetail .more-menu.open, #viewAlbumDetail .row-more-menu.open, ' +
            '#viewAlbumDetail .submenu.open, #viewAlbumDetail .view-toggle-menu.open'
        );
        mainContent.classList.toggle('scroll-locked', anyOpen);
    }
    function bindMoreMenu() {
        const moreBtn = document.getElementById('btnAlbumMore');
        const menu = document.getElementById('albumMoreMenu');
        moreBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            const willOpen = !menu.classList.contains('open');
            menu.classList.toggle('open', willOpen);
            if (willOpen) positionMenu(moreBtn, menu);
            syncScrollLock();
        });
        document.getElementById('menuShareAlbum').addEventListener('click', () => { shareAlbum(); menu.classList.remove('open'); syncScrollLock(); });
        document.getElementById('menuReportAlbum').addEventListener('click', () => { openReportModal(); menu.classList.remove('open'); syncScrollLock(); });
        document.addEventListener('click', function () { menu.classList.remove('open'); syncScrollLock(); });
    }
    function bindRowActions() {
        document.querySelectorAll('.row-add-btn').forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.stopPropagation();
                const songId = Number(this.dataset.songId);
                if (window.handleSongAddButtonClick) {
                    window.handleSongAddButtonClick(songId);
                } else if (window.PlaylistPicker) {
                    window.PlaylistPicker.open(songId);
                }
            });
        });
        document.querySelectorAll('.row-more-btn').forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.stopPropagation();
                const menu = this.nextElementSibling;
                const wasOpen = menu.classList.contains('open');
                document.querySelectorAll('.row-more-menu.open').forEach(m => m.classList.remove('open'));
                document.querySelectorAll('.row-actions.menu-open').forEach(a => a.classList.remove('menu-open'));
                document.querySelectorAll('#viewAlbumDetail .submenu.open').forEach(sm => sm.classList.remove('open'));
                if (!wasOpen) {
                    menu.classList.add('open');
                    this.closest('.row-actions').classList.add('menu-open');
                    positionMenu(this, menu);
                }
                syncScrollLock();
            });
        });
        document.querySelectorAll('.row-menu-like').forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.stopPropagation();
                const songId = Number(this.id.replace('likeToggle-', ''));
                toggleLikeSong(songId);
                this.closest('.row-more-menu').classList.remove('open');
                syncScrollLock();
            });
        });
        document.querySelectorAll('.row-menu-credits').forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.stopPropagation();
                const row = this.closest('.song-row');
                const songId = Number(row.dataset.songId);
                showCredits(songId);
                this.closest('.row-more-menu').classList.remove('open');
                syncScrollLock();
            });
        });
        document.querySelectorAll('.row-menu-share').forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.stopPropagation();
                const row = this.closest('.song-row');
                const songId = Number(row.dataset.songId);
                shareSong(songId);
                this.closest('.row-more-menu').classList.remove('open');
                syncScrollLock();
            });
        });
        loadLikedSongIds().then(() => {
            syncRowAddButtons();
        });
    }
    function bindViewToggle() {
        const root = document.getElementById('viewAlbumDetail');
        const toggleBtn = document.getElementById('btnViewToggle');
        const menu = document.getElementById('viewToggleMenu');
        const btnCompact = document.getElementById('btnModeCompact');
        const btnList = document.getElementById('btnModeList');
        function applyMode(mode) {
            root.classList.toggle('compact-mode', mode === 'compact');
            btnCompact.classList.toggle('active', mode === 'compact');
            btnList.classList.toggle('active', mode === 'list');
            btnCompact.innerHTML = mode === 'compact' ? '☰ Rút gọn <span class="check">✓</span>' : '☰ Rút gọn';
            btnList.innerHTML = mode === 'list' ? '☰ Danh sách <span class="check">✓</span>' : '☰ Danh sách';
            localStorage.setItem('albumViewMode:' + albumId, mode);
        }
        toggleBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            const willOpen = !menu.classList.contains('open');
            menu.classList.toggle('open', willOpen);
            if (willOpen) positionMenu(toggleBtn, menu);
            syncScrollLock();
        });
        btnCompact.addEventListener('click', () => { applyMode('compact'); menu.classList.remove('open'); syncScrollLock(); });
        btnList.addEventListener('click', () => { applyMode('list'); menu.classList.remove('open'); syncScrollLock(); });
        const savedMode = localStorage.getItem('albumViewMode:' + albumId) || 'list';
        applyMode(savedMode);
    }
    function syncRowAddButtons() {
        if (!window.IS_AUTHENTICATED || !window.refreshSongAddButtons) return;
        document.querySelectorAll('.row-add-btn').forEach(btn => {
            const songId = Number(btn.dataset.songId);
            if (songId) window.refreshSongAddButtons(songId);
        });
    }
    function init() {
        const root = document.getElementById('viewAlbumDetail');
        if (!root) return;
        albumId = Number(root.dataset.albumId);
        albumName = root.dataset.albumName || '';
        mainArtistId = Number(root.dataset.artistId) || null;
        bindPlayShuffle();
        bindSongRows();
        bindLibraryButton();
        bindMoreMenu();
        bindReportModal();
        bindCreditsModal();
        bindRowActions();
        bindSongSubmenus();
        bindViewToggle();
        document.addEventListener('click', function () {
            document.querySelectorAll('.row-more-menu.open').forEach(m => m.classList.remove('open'));
            document.querySelectorAll('.row-actions.menu-open').forEach(a => a.classList.remove('menu-open'));
            document.querySelectorAll('#viewAlbumDetail .submenu.open').forEach(sm => sm.classList.remove('open'));
            document.getElementById('viewToggleMenu')?.classList.remove('open');
            syncScrollLock();
        });
        updatePlaybackUI();
        if (window.Player && typeof Player.onChange === 'function') {
            Player.onChange(updatePlaybackUI);
        }
    }
    document.addEventListener('DOMContentLoaded', init);
    return {};
})();