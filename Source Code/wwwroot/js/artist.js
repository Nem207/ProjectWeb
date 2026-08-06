const ArtistDetail = (function () {
    const DEFAULT_COVER = '/images/default-cover.png';
    let artistId = null;
    let artistName = '';
    let artistAvatar = '';
    let allAlbums = [];
    let currentTab = 'all';
    function fmtYear(dateStr) {
        if (!dateStr) return '--';
        const d = new Date(dateStr);
        return isNaN(d) ? '--' : d.getFullYear();
    }
    function imgOr(url) {
        return (url && url.trim() !== '') ? url : DEFAULT_COVER;
    }
    async function loadAlbums() {
        const grid = document.getElementById('albumGrid');
        try {
            const res = await fetch(`/api/Artist/${artistId}/albums`);
            allAlbums = res.ok ? await res.json() : [];
        } catch (err) {
            console.error(err);
            grid.innerHTML = `<div class="empty-text">Không thể tải danh sách đĩa nhạc (lỗi kết nối API).</div>`;
            return;
        }
        renderAlbumGrid();
    }
    function albumTabMatch(album, tab) {
        const t = (album.albumType || '').toLowerCase();
        if (tab === 'all') return true;
        if (tab === 'album') return t === 'album';
        if (tab === 'single_ep') return t === 'single' || t === 'ep';
        return true;
    }
    function renderAlbumGrid() {
        const grid = document.getElementById('albumGrid');
        if (!allAlbums || allAlbums.length === 0) {
            grid.innerHTML = `<div class="empty-text">Nghệ sĩ này chưa có đĩa nhạc nào.</div>`;
            return;
        }
        grid.innerHTML = allAlbums.map(album => `
            <div class="album-card" data-type="${(album.albumType || '').toLowerCase()}" onclick="location.href='/Album/Detail/${album.albumID}'">
                <img src="${imgOr(album.coverImage)}" alt="${album.albumName ?? ''}" onerror="this.onerror=null;this.src='${DEFAULT_COVER}'">
                <div class="album-title">${album.albumName ?? 'Không tìm thấy tên đĩa nhạc'}</div>
                <div class="album-meta">${fmtYear(album.releaseDate)} • ${album.albumType ?? 'Đang cập nhật'}</div>
            </div>
        `).join('');
        applyAlbumTabFilter();
    }
    function applyAlbumTabFilter() {
        const grid = document.getElementById('albumGrid');
        const cards = grid.querySelectorAll('.album-card');
        let visibleCount = 0;
        cards.forEach(card => {
            const match = currentTab === 'all'
                || (currentTab === 'album' && card.dataset.type === 'album')
                || (currentTab === 'single_ep' && (card.dataset.type === 'single' || card.dataset.type === 'ep'));
            card.style.display = match ? '' : 'none';
            if (match) visibleCount++;
        });
        let emptyMsg = grid.querySelector('.empty-text');
        if (visibleCount === 0) {
            if (!emptyMsg) {
                emptyMsg = document.createElement('div');
                emptyMsg.className = 'empty-text';
                emptyMsg.textContent = 'Không có đĩa nhạc phù hợp.';
                grid.appendChild(emptyMsg);
            }
        } else if (emptyMsg) {
            emptyMsg.remove();
        }
    }
    function bindAlbumTabs() {
        document.querySelectorAll('#albumTabs button').forEach(btn => {
            btn.addEventListener('click', () => {
                currentTab = btn.dataset.tab;
                document.querySelectorAll('#albumTabs button').forEach(b => b.classList.toggle('active', b === btn));
                applyAlbumTabFilter();
            });
        });
    }
    let isFollowingCache = false;
    function applyFollowButtonState(isFollowing) {
        isFollowingCache = isFollowing;
        const followBtn = document.getElementById('btnFollow');
        followBtn.textContent = isFollowing ? 'Đang theo dõi' : 'Theo dõi';
        followBtn.classList.toggle('following', isFollowing);
    }
    async function refreshFollowButton() {
        if (!window.IS_AUTHENTICATED) {
            applyFollowButtonState(false);
            return;
        }
        try {
            const res = await fetch(`/api/Artist/${artistId}/follow`);
            const data = res.ok ? await res.json() : { following: false };
            applyFollowButtonState(!!data.following);
        } catch (err) {
            console.error('Không thể tải trạng thái theo dõi.', err);
            applyFollowButtonState(false);
        }
    }
    function syncSidebarArtist(isFollowing) {
        if (!window.Playlist) return;
        if (isFollowing) {
            if (typeof Playlist.addArtistToSidebar === 'function') {
                Playlist.addArtistToSidebar({ artistID: artistId, artistName: artistName, avatar: artistAvatar });
            }
        } else {
            if (typeof Playlist.removeArtistFromSidebar === 'function') {
                Playlist.removeArtistFromSidebar(artistId);
            }
        }
    }
    function bindFollow() {
        document.getElementById('btnFollow').addEventListener('click', async function () {
            if (!window.IS_AUTHENTICATED) {
                Toast.info('Vui lòng đăng nhập để theo dõi nghệ sĩ.');
                return;
            }
            try {
                const res = await fetch(`/api/Artist/${artistId}/follow/toggle`, { method: 'POST' });
                if (!res.ok) throw new Error('Toggle follow thất bại');
                const data = await res.json();
                applyFollowButtonState(!!data.following);
                syncSidebarArtist(!!data.following);
            } catch (err) {
                console.error('Không thể cập nhật theo dõi nghệ sĩ.', err);
                Toast.error('Không thể cập nhật theo dõi nghệ sĩ.');
            }
        });
    }
    let isBlockedCache = false;
    async function refreshBlockState() {
        if (!window.IS_AUTHENTICATED) {
            isBlockedCache = false;
            return;
        }
        try {
            const res = await fetch(`/api/Artist/${artistId}/block`);
            const data = res.ok ? await res.json() : { blocked: false };
            isBlockedCache = !!data.blocked;
        } catch (err) {
            console.error('Không thể tải trạng thái chặn nghệ sĩ.', err);
            isBlockedCache = false;
        }
    }
    function refreshBlockUI() {
        const blocked = isBlockedCache;
        const menuBtn = document.getElementById('menuBlockArtist');
        const playBtn = document.getElementById('btnArtistPlay');
        const followBtn = document.getElementById('btnFollow');
        const unblockBtn = document.getElementById('btnUnblock');
        const notice = document.getElementById('blockedNotice');
        menuBtn.textContent = blocked ? 'Bỏ chặn nghệ sĩ này' : 'Không phát nghệ sĩ này';
        playBtn.style.display = blocked ? 'none' : '';
        followBtn.style.display = blocked ? 'none' : '';
        unblockBtn.style.display = blocked ? '' : 'none';
        notice.style.display = blocked ? '' : 'none';
        document.querySelectorAll('#popularSongs .song-row').forEach(row => {
            row.style.pointerEvents = blocked ? 'none' : '';
            row.style.opacity = blocked ? '0.4' : '';
        });
    }
    function bindUnblock() {
        document.getElementById('btnUnblock').addEventListener('click', function () {
            toggleBlockArtist();
        });
    }
    async function toggleBlockArtist() {
        if (!window.IS_AUTHENTICATED) {
            Toast.info('Vui lòng đăng nhập để chặn nghệ sĩ.');
            return;
        }
        const wasBlocked = isBlockedCache;
        try {
            const res = await fetch(`/api/Artist/${artistId}/block/toggle`, { method: 'POST' });
            if (!res.ok) throw new Error('Toggle block thất bại');
            const data = await res.json();
            isBlockedCache = !!data.blocked;
        } catch (err) {
            console.error('Không thể cập nhật trạng thái chặn nghệ sĩ.', err);
            Toast.error('Không thể cập nhật trạng thái chặn nghệ sĩ.');
            return;
        }
        if (isBlockedCache && isFollowingCache) {
            try {
                const res = await fetch(`/api/Artist/${artistId}/follow/toggle`, { method: 'POST' });
                if (res.ok) {
                    const data = await res.json();
                    applyFollowButtonState(!!data.following);
                    syncSidebarArtist(!!data.following);
                }
            } catch (err) {
                console.error('Không thể bỏ theo dõi nghệ sĩ.', err);
            }
        }
        if (window.Player && typeof Player.notifyArtistBlocked === 'function') {
            Player.notifyArtistBlocked(artistId, isBlockedCache);
        }
        refreshBlockUI();
        refreshFollowButton();
        Toast.success(wasBlocked ? `Đã bỏ chặn ${artistName}.` : `Sẽ không phát nhạc từ ${artistName} nữa.`);
    }
    function openReportModal() {
        document.getElementById('reportArtistName').textContent = artistName;
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
        fetch(`/api/artist-report/${artistId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ reason: reason, description: detail })
        })
            .then(async res => {
                const data = await res.json().catch(() => ({}));
                if (res.ok) {
                    closeReportModal();
                    Toast.success(data.message || 'Cảm ơn bạn đã báo cáo. Chúng tôi sẽ xem xét sớm nhất.');
                } else {
                    Toast.error(data.message || 'Gửi báo cáo thất bại.');
                }
            })
            .catch(err => {
                console.error('Không thể gửi báo cáo.', err);
                Toast.error('Lỗi kết nối, vui lòng thử lại.');
            });
    }
    function bindReportModal() {
        document.getElementById('btnReportCancel').addEventListener('click', closeReportModal);
        document.getElementById('btnReportSubmit').addEventListener('click', submitReport);
        document.getElementById('reportOverlay').addEventListener('click', function (e) {
            if (e.target === this) closeReportModal();
        });
    }
    function getRowSongInfo(row) {
        try {
            return JSON.parse(row.dataset.song);
        } catch {
            return null;
        }
    }
    function getPopularRows() {
        return Array.from(document.querySelectorAll('#popularSongs .song-row'));
    }
    function buildPopularSongList() {
        return getPopularRows().map(getRowSongInfo).filter(Boolean);
    }
    function isPopularSongsActiveQueue(current) {
        if (!current) return false;
        return buildPopularSongList().some(s => s.songId === current.songId);
    }
    function playRowAt(index) {
        const list = buildPopularSongList();
        if (!list[index]) return;
        const current = window.Player && Player.getCurrent ? Player.getCurrent() : null;
        if (current && current.songId === list[index].songId) {
            Player.togglePlay();
            return;
        }
        Player.playFromList(list, index);
    }
    function bindSongRows() {
        getPopularRows().forEach((row, index) => {
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
    function bindPlay() {
        document.getElementById('btnArtistPlay').addEventListener('click', function () {
            if (!window.Player) return;
            const list = buildPopularSongList();
            if (list.length === 0) {
                console.warn('Nghệ sĩ chưa có bài hát để phát.');
                return;
            }
            const current = Player.getCurrent();
            if (isPopularSongsActiveQueue(current)) {
                Player.togglePlay();
            } else {
                Player.playFromList(list, 0);
            }
        });
    }
    function updatePlaybackUI() {
        const current = window.Player && Player.getCurrent ? Player.getCurrent() : null;
        const playing = window.Player && Player.isPlaying ? Player.isPlaying() : false;
        const isThisList = isPopularSongsActiveQueue(current);
        const actionBtn = document.getElementById('btnArtistPlay');
        if (actionBtn) {
            actionBtn.textContent = (isThisList && playing) ? '❚❚' : '▶';
        }
        getPopularRows().forEach(row => {
            const songId = Number(row.dataset.songId);
            const isCurrentRow = !!(current && songId === current.songId);
            row.classList.toggle('row-playing', isCurrentRow);
            const playMini = row.querySelector('.play-mini');
            if (playMini) playMini.textContent = (isCurrentRow && playing) ? '❚❚' : '▶';
        });
    }
    function syncScrollLock() {
        const mainContent = document.getElementById('mainContent');
        if (!mainContent) return;
        const anyOpen = !!document.querySelector('#viewArtistDetail .more-menu.open, #viewArtistDetail .submenu.open');
        mainContent.classList.toggle('scroll-locked', anyOpen);
    }
    function closeAllMenus(exceptEl) {
        document.querySelectorAll('#viewArtistDetail .more-menu.open').forEach(menu => {
            if (menu !== exceptEl) menu.classList.remove('open');
        });
        document.querySelectorAll('#viewArtistDetail .submenu.open').forEach(sm => sm.classList.remove('open'));
        document.querySelectorAll('#viewArtistDetail .row-actions.menu-open').forEach(a => a.classList.remove('menu-open'));
        syncScrollLock();
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
    function bindMoreMenu() {
        const btn = document.getElementById('btnArtistMore');
        const menu = document.getElementById('artistMoreMenu');
        btn.addEventListener('click', function (e) {
            e.stopPropagation();
            const willOpen = !menu.classList.contains('open');
            closeAllMenus();
            menu.classList.toggle('open', willOpen);
            if (willOpen) positionMenu(btn, menu);
            syncScrollLock();
        });
        document.getElementById('menuBlockArtist').addEventListener('click', function () {
            menu.classList.remove('open');
            syncScrollLock();
            toggleBlockArtist();
        });
        document.getElementById('menuReport').addEventListener('click', function () {
            menu.classList.remove('open');
            syncScrollLock();
            openReportModal();
        });
        document.getElementById('menuShare').addEventListener('click', function () {
            menu.classList.remove('open');
            syncScrollLock();
            const url = window.location.href;
            if (navigator.clipboard) {
                navigator.clipboard.writeText(url).then(() => Toast.success('Đã sao chép liên kết!'));
            } else {
                Toast.info(url);
            }
        });
        document.addEventListener('click', function () {
            closeAllMenus();
        });
    }
    function bindSongSubmenus() {
        document.querySelectorAll('#viewArtistDetail .submenu-trigger').forEach(trigger => {
            trigger.addEventListener('click', function (e) {
                e.stopPropagation();
                const submenu = this.nextElementSibling;
                const willOpen = !submenu.classList.contains('open');
                document.querySelectorAll('#viewArtistDetail .submenu.open').forEach(sm => sm.classList.remove('open'));
                submenu.classList.toggle('open', willOpen);
                if (willOpen) positionSubmenu(this, submenu);
                syncScrollLock();
            });
        });
    }
    function toggleSongMenu(evt, songId) {
        evt.stopPropagation();
        const menu = document.getElementById('songMenu-' + songId);
        if (!menu) return;
        const btn = evt.currentTarget;
        const willOpen = !menu.classList.contains('open');
        closeAllMenus();
        menu.classList.toggle('open', willOpen);
        if (willOpen) {
            positionMenu(btn, menu);
            const rowActions = btn.closest('.row-actions');
            if (rowActions) rowActions.classList.add('menu-open');
        }
        syncScrollLock();
    }
    function closeAllSongMenus() {
        closeAllMenus();
    }
    function addToPlaylist(songId, songTitle) {
        closeAllSongMenus();
        if (window.handleSongAddButtonClick) {
            window.handleSongAddButtonClick(songId);
        } else if (window.PlaylistPicker) {
            window.PlaylistPicker.open(songId);
        }
    }
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
    function toggleLikeSong(songId, songTitle) {
        if (!window.IS_AUTHENTICATED) {
            Toast.info('Vui lòng đăng nhập để thích bài hát.');
            closeAllSongMenus();
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
            .catch(err => {
                console.error('Không thể cập nhật bài hát yêu thích.', err);
                Toast.error('Không thể cập nhật bài hát yêu thích.');
            })
            .finally(() => closeAllSongMenus());
    }
    function showCredits(songId) {
        closeAllSongMenus();
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
    function shareSong(songId) {
        const url = `${window.location.origin}/Artist/Detail/${artistId}#song-${songId}`;
        if (navigator.clipboard) {
            navigator.clipboard.writeText(url).then(() => Toast.success('Đã sao chép liên kết bài hát!'));
        } else {
            Toast.info(url);
        }
    }
    function syncRowAddButtons() {
        if (!window.IS_AUTHENTICATED || !window.refreshSongAddButtons) return;
        document.querySelectorAll('#viewArtistDetail .row-add-btn').forEach(btn => {
            const songId = Number(btn.dataset.songId);
            if (songId) window.refreshSongAddButtons(songId);
        });
    }
    function init() {
        const root = document.getElementById('viewArtistDetail');
        if (!root) return;
        artistId = parseInt(root.dataset.artistId, 10);
        artistName = root.dataset.artistName || '';
        artistAvatar = root.dataset.artistAvatar || '';
        bindPlay();
        bindSongRows();
        bindFollow();
        bindUnblock();
        bindMoreMenu();
        bindReportModal();
        bindCreditsModal();
        bindSongSubmenus();
        bindAlbumTabs();
        refreshFollowButton();
        refreshBlockState().then(refreshBlockUI);
        loadLikedSongIds();
        syncRowAddButtons();
        loadAlbums();
        updatePlaybackUI();
        if (window.Player && typeof Player.onChange === 'function') {
            Player.onChange(updatePlaybackUI);
        }
    }
    document.addEventListener('DOMContentLoaded', init);
    return {
        addToPlaylist: addToPlaylist,
        toggleSongMenu: toggleSongMenu,
        closeAllSongMenus: closeAllSongMenus,
        toggleLikeSong: toggleLikeSong,
        showCredits: showCredits,
        shareSong: shareSong
    };
})();