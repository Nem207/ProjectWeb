const Player = (function () {
    const ICON_PLAY = "M8 5v14l11-7z";
    const ICON_PAUSE = "M6 6h4v12H6zm8 0h4v12h-4z";
    const ICON_VOLUME = "M3 9v6h4l5 5V4L7 9H3zm13.5 3c0-1.77-1.02-3.29-2.5-4.03v8.05c1.48-.73 2.5-2.25 2.5-4.02zM14 3.23v2.06c2.89.86 5 3.54 5 6.71s-2.11 5.85-5 6.71v2.06c4.01-.91 7-4.49 7-8.77s-2.99-7.86-7-8.77z";
    const ICON_MUTED = "M16.5 12c0-1.77-1.02-3.29-2.5-4.03v2.21l2.45 2.45c.03-.2.05-.41.05-.63zm2.5 0c0 .94-.2 1.82-.54 2.64l1.51 1.51C20.63 14.91 21 13.5 21 12c0-4.28-2.99-7.86-7-8.77v2.06c2.89.86 5 3.54 5 6.71zM4.27 3L3 4.27 7.73 9H3v6h4l5 5v-6.73l4.25 4.25c-.67.52-1.42.93-2.25 1.18v2.06c1.38-.31 2.63-.95 3.69-1.81L19.73 21 21 19.73l-9-9L4.27 3zM12 4L9.91 6.09 12 8.18V4z";
    const audio = new Audio();
    function handlePlayError(err) {
        if (err && err.name === "AbortError") return;
        console.error("Player: không thể phát bài hát.", err);
        if (typeof Toast !== "undefined") {
            Toast.error("Không thể phát bài hát này. Vui lòng thử lại.");
        }
    }
    let changeListeners = [];
    function notifyChange() {
        changeListeners.forEach(fn => { try { fn(); } catch (e) { } });
    }
    let current = null;
    let playlist = [];
    let order = [];
    let pos = -1;
    let queue = [];
    let hasCountedPlay = false;
    let hasCountedEarning = false;
    const MIN_LISTEN_SECONDS_FOR_EARNING = 30;
    let isShuffle = false;
    let repeatMode = "off";
    let isMuted = false;
    let volumeBeforeMute = 1;
    let blockedArtistIds = new Set();
    let blockedArtistIdsLoaded = false;
    async function loadBlockedArtistIds() {
        if (!window.IS_AUTHENTICATED) { blockedArtistIds = new Set(); blockedArtistIdsLoaded = true; return; }
        try {
            const res = await fetch("/api/Artist/blocked");
            const ids = res.ok ? await res.json() : [];
            blockedArtistIds = new Set(Array.isArray(ids) ? ids.map(Number) : []);
        } catch { blockedArtistIds = new Set(); }
        blockedArtistIdsLoaded = true;
    }
    function isArtistBlocked(artistId) {
        if (!artistId) return false;
        return blockedArtistIds.has(Number(artistId));
    }
    // Xóa hoàn toàn UI thanh player về trạng thái rỗng khi block nghệ sĩ đang phát
    function clearNowPlayingUI() {
        audio.pause();
        audio.src = "";
        current = null;
        playlist = [];
        order = [];
        pos = -1;
        queue = [];
        try { localStorage.removeItem("spotifyCloneNowPlaying"); } catch (e) { }
        const nameEl = els.songName();
        if (nameEl) nameEl.innerHTML = "";
        const artistEl = els.artist();
        if (artistEl) artistEl.innerHTML = "";
        const totalTimeEl = els.totalTime();
        if (totalTimeEl) totalTimeEl.textContent = "0:00";
        const currentTimeEl = els.currentTime();
        if (currentTimeEl) currentTimeEl.textContent = "0:00";
        const thumb = els.thumb();
        if (thumb) thumb.innerHTML = `<svg viewBox="0 0 24 24" width="28" height="28" fill="var(--color-text-muted)"><path d="M12 3v10.55c-.59-.34-1.27-.55-2-.55-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4V7h4V3h-6z" /></svg>`;
        const progressFill = els.progressFill();
        if (progressFill) progressFill.style.width = "0%";
        if (els.playPauseIcon()) els.playPauseIcon().setAttribute("d", ICON_PLAY);
        notifyChange();
    }
    function notifyArtistBlocked(artistId, isNowBlocked) {
        const id = Number(artistId);
        if (isNowBlocked) {
            blockedArtistIds.add(id);
            if (current && Number(current.mainArtistID) === id) {
                if (typeof Toast !== "undefined") Toast.info("Đã dừng phát vì bạn đã chặn nghệ sĩ này.");
                clearNowPlayingUI();
            }
        } else {
            blockedArtistIds.delete(id);
        }
    }
    let isPremiumUser = false;
    let songsPlayedSinceAd = 0;
    let adThreshold = randomAdThreshold();
    let isAdShowing = false;
    function randomAdThreshold() {
        return Math.random() < 0.5 ? 2 : 3;
    }
    function refreshPremiumStatus() {
        if (!window.IS_AUTHENTICATED) {
            isPremiumUser = false;
            return Promise.resolve(false);
        }
        return fetch("/api/premium/my-status")
            .then(res => res.ok ? res.json() : null)
            .then(data => {
                isPremiumUser = !!(data && data.isPremium);
                if (isPremiumUser) {
                    hidePremiumLimitOverlay();
                }
                return isPremiumUser;
            })
            .catch(() => {
                isPremiumUser = false;
                return false;
            });
    }
    window.PlayerRefreshPremiumStatus = refreshPremiumStatus;
    function ensureAdOverlay() {
        let overlay = document.getElementById("playerAdOverlay");
        if (overlay) return overlay;
        overlay = document.createElement("div");
        overlay.id = "playerAdOverlay";
        overlay.className = "player-ad-overlay";
        overlay.innerHTML = `
            <div class="player-ad-box">
                <div class="player-ad-label">Quảng cáo</div>
                <div class="player-ad-content">
                    <h3>Nâng cấp lên Spotify Premium</h3>
                    <p>Nghe nhạc không quảng cáo, chất lượng cao và tải nhạc offline. Đăng ký ngay hôm nay!</p>
                    <a href="/Premium" class="player-ad-cta">Đăng ký ngay</a>
                </div>
                <button id="btnCloseAd" class="player-ad-close" disabled>Đóng (5)</button>
            </div>`;
        document.body.appendChild(overlay);
        return overlay;
    }
    function ensurePremiumLimitOverlay() {
        let overlay = document.getElementById("playerPremiumLimitOverlay");
        if (overlay) return overlay;
        overlay = document.createElement("div");
        overlay.id = "playerPremiumLimitOverlay";
        overlay.className = "player-ad-overlay";
        overlay.innerHTML = `
            <div class="player-ad-box">
                <div class="player-ad-label">Bài hát Premium</div>
                <div class="player-ad-content">
                    <h3>Bạn đã nghe hết phần dùng thử</h3>
                    <p>Bài hát này chỉ dành cho thuê bao Premium. Nâng cấp ngay để nghe trọn vẹn không giới hạn.</p>
                    <a href="/Premium" class="player-ad-cta">Nâng cấp Premium</a>
                </div>
                <button id="btnClosePremiumLimit" class="player-ad-close">Bỏ qua bài này</button>
            </div>`;
        document.body.appendChild(overlay);
        overlay.querySelector("#btnClosePremiumLimit").onclick = () => {
            overlay.classList.remove("active");
            next();
            notifyChange();
        };
        return overlay;
    }
    function isOnPremiumPage() {
        return /^\/Premium(\/|$)/i.test(window.location.pathname);
    }
    function enforcePremiumPreviewLimit() {
        audio.pause();
        if (isOnPremiumPage()) {
            hidePremiumLimitOverlay();
            return;
        }
        const overlay = ensurePremiumLimitOverlay();
        overlay.classList.add("active");
    }
    function hidePremiumLimitOverlay() {
        const overlay = document.getElementById("playerPremiumLimitOverlay");
        if (overlay) overlay.classList.remove("active");
    }
    function showAd(onComplete) {
        isAdShowing = true;
        songsPlayedSinceAd = 0;
        adThreshold = randomAdThreshold();
        audio.pause();
        const overlay = ensureAdOverlay();
        const btnClose = overlay.querySelector("#btnCloseAd");
        let remaining = 5;
        btnClose.disabled = true;
        btnClose.textContent = `Đóng (${remaining})`;
        overlay.classList.add("active");
        const timer = setInterval(() => {
            remaining--;
            if (remaining <= 0) {
                clearInterval(timer);
                btnClose.disabled = false;
                btnClose.textContent = "Đóng quảng cáo";
            } else {
                btnClose.textContent = `Đóng (${remaining})`;
            }
        }, 1000);
        btnClose.onclick = () => {
            if (btnClose.disabled) return;
            clearInterval(timer);
            overlay.classList.remove("active");
            isAdShowing = false;
            if (typeof onComplete === "function") onComplete();
        };
    }
    const els = {
        thumb: () => document.getElementById("playerThumb"),
        songName: () => document.getElementById("playerSongName"),
        artist: () => document.getElementById("playerArtist"),
        playPauseIcon: () => document.getElementById("playPauseIcon"),
        currentTime: () => document.getElementById("currentTime"),
        totalTime: () => document.getElementById("totalTime"),
        progressTrack: () => document.getElementById("progressTrack"),
        progressFill: () => document.getElementById("progressFill"),
        volumeTrack: () => document.getElementById("volumeTrack"),
        volumeFill: () => document.getElementById("volumeFill"),
        volumeIcon: () => document.getElementById("volumeIcon"),
        btnShuffle: () => document.getElementById("btnShuffle"),
        btnRepeat: () => document.getElementById("btnRepeat"),
        repeatOneBadge: () => document.getElementById("repeatOneBadge"),
        lyricsTitle: () => document.getElementById("lyricsSongTitle"),
        lyricsArtist: () => document.getElementById("lyricsSongArtist"),
        lyricsContent: () => document.getElementById("lyricsContent"),
        modalLyrics: () => document.getElementById("modalLyrics"),
    };
    const STORAGE_KEY = "spotifyCloneNowPlaying";
    function saveState() {
        try {
            localStorage.setItem(STORAGE_KEY, JSON.stringify({
                current,
                playlist,
                order,
                pos,
                queue,
                currentTime: audio.currentTime || 0,
                isPlaying: !audio.paused,
                isShuffle,
                repeatMode,
                volume: audio.volume,
                isMuted,
                hasCountedPlay,
                hasCountedEarning
            }));
        } catch (e) { }
    }
    function restoreState() {
        let saved = null;
        try { saved = JSON.parse(localStorage.getItem(STORAGE_KEY) || "null"); } catch { saved = null; }
        if (!saved || !saved.current) return;
        current = saved.current;
        playlist = Array.isArray(saved.playlist) ? saved.playlist : [];
        order = Array.isArray(saved.order) ? saved.order : [];
        pos = typeof saved.pos === "number" ? saved.pos : -1;
        queue = Array.isArray(saved.queue) ? saved.queue : [];
        isShuffle = !!saved.isShuffle;
        repeatMode = saved.repeatMode === "all" || saved.repeatMode === "one" ? saved.repeatMode : "off";
        isMuted = !!saved.isMuted;
        hasCountedPlay = !!saved.hasCountedPlay;
        hasCountedEarning = !!saved.hasCountedEarning;
        toggleActiveClass(els.btnShuffle(), isShuffle);
        renderRepeatButton();
        audio.volume = typeof saved.volume === "number" ? saved.volume : 1;
        renderNowPlaying(current);
        renderVolume();
        audio.src = current.audioUrl || "";
        function resume() {
            audio.currentTime = saved.currentTime || 0;
            if (saved.isPlaying) {
                audio.play().catch(() => { });
            }
            audio.removeEventListener("loadedmetadata", resume);
        }
        audio.addEventListener("loadedmetadata", resume);
        if (current.songId) {
            fetchSongById(current.songId).then(fullInfo => {
                if (fullInfo && current && current.songId === fullInfo.songId) {
                    current.isPremium = fullInfo.isPremium;
                    current.maxPreviewSeconds = fullInfo.maxPreviewSeconds;
                    if (current.maxPreviewSeconds && audio.currentTime >= current.maxPreviewSeconds) {
                        enforcePremiumPreviewLimit();
                    } else {
                        hidePremiumLimitOverlay();
                        if (saved.isPlaying && audio.paused) {
                            audio.play().catch(() => { });
                        }
                    }
                }
            });
        }
    }
    async function playSong(songInfo) {
        if (!songInfo || !songInfo.songId) return;
        if (isAdShowing) return;
        if (!blockedArtistIdsLoaded) await loadBlockedArtistIds();
        if (isArtistBlocked(songInfo.mainArtistID)) {
            if (typeof Toast !== "undefined") Toast.info("Bạn đã chặn nghệ sĩ này nên không thể phát bài hát của họ.");
            return;
        }
        current = { ...songInfo };
        hasCountedPlay = false;
        hasCountedEarning = false;
        const fullInfo = await fetchSongById(current.songId);
        if (fullInfo) current = { ...current, ...fullInfo };
        if (isArtistBlocked(current.mainArtistID)) {
            if (typeof Toast !== "undefined") Toast.info("Bạn đã chặn nghệ sĩ này nên không thể phát bài hát của họ.");
            return;
        }
        renderNowPlaying(current);
        audio.src = current.audioUrl || "";
        audio.play().catch(handlePlayError);
        saveState();
    }
    function buildOrderForNewList(anchorIndex) {
        if (isShuffle) {
            const rest = playlist.map((_, i) => i).filter(i => i !== anchorIndex);
            order = [anchorIndex, ...shuffleArray(rest)];
            pos = 0;
        } else {
            order = playlist.map((_, i) => i);
            pos = anchorIndex;
        }
    }
    async function playFromList(songs, startIndex) {
        if (!songs || !songs[startIndex]) return;
        playlist = [...songs];
        buildOrderForNewList(startIndex);
        await playSong(songs[startIndex]);
    }
    function addToQueue(songInfo) {
        if (!songInfo || !songInfo.songId) return;
        queue.push({ ...songInfo });
        saveState();
        if (typeof Toast !== "undefined") {
            Toast.success(`Đã thêm "${songInfo.title}" vào danh sách chờ.`);
        }
    }
    async function playPlaylist(playlistId) {
        if (!playlistId) return;
        const response = await fetch(`/api/playlist/${playlistId}/songs`);
        if (!response.ok) return;
        const songs = await response.json();
        if (!songs || songs.length === 0) return;
        await playFromList(songs, 0);
    }
    async function fetchSongById(songId) {
        const response = await fetch(`/api/musicplayer/song/${songId}`);
        if (!response.ok) return null;
        const data = await response.json();
        return {
            songId: data.songID,
            title: data.title,
            audioUrl: data.audioUrl,
            durationInSeconds: data.duration,
            coverImageUrl: data.coverImage,
            artistName: data.artistNames,
            mainArtistID: data.mainArtistID || null,
            isPremium: !!data.isPremium,
            maxPreviewSeconds: data.maxPreviewSeconds || null
        };
    }
    function renderNowPlaying(song) {
        const nameEl = els.songName();
        if (nameEl) {
            nameEl.innerHTML = `<a href="/Song/Detail/${song.songId}" class="song-title-link">${song.title}</a>`;
        }
        const artistEl = els.artist();
        if (artistEl) {
            const artistText = song.artistName || "Nghệ sĩ ẩn danh";
            artistEl.innerHTML = song.mainArtistID
                ? `<a href="/Artist/Detail/${song.mainArtistID}" class="artist-link-inline">${artistText}</a>`
                : artistText;
        }
        if (els.totalTime()) els.totalTime().textContent = formatTime(song.durationInSeconds || 0);
        const thumb = els.thumb();
        if (thumb) {
            thumb.innerHTML = song.coverImageUrl
                ? `<img src="${song.coverImageUrl}" alt="${song.title}" style="width:100%;height:100%;object-fit:cover;border-radius:inherit;" onerror="this.onerror=null;this.src='/images/default-cover.png'" />`
                : `<svg viewBox="0 0 24 24" width="28" height="28" fill="var(--color-text-muted)"><path d="M12 3v10.55c-.59-.34-1.27-.55-2-.55-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4V7h4V3h-6z" /></svg>`;
        }
        updateLikeButtonUI(song.songId);
    }
    async function isSongInAnyPlaylist(songId) {
        try {
            const res = await fetch(`/api/playlists/containing/${songId}`);
            if (!res.ok) return false;
            const ids = await res.json();
            return Array.isArray(ids) && ids.length > 0;
        } catch {
            return false;
        }
    }
    async function isSongLikedOrInPlaylist(songId) {
        const [inPlaylist, isLiked] = await Promise.all([
            isSongInAnyPlaylist(songId),
            (async () => {
                try {
                    if (!window.LikedSongs || !window.IS_AUTHENTICATED) return false;
                    const ids = await LikedSongs.loadLikedIds();
                    return ids instanceof Set ? ids.has(songId) : false;
                } catch { return false; }
            })()
        ]);
        return inPlaylist || isLiked;
    }
    function updateLikeButtonUI(songId) {
        const btn = document.getElementById("btnAddToPlaylist");
        if (!btn || !songId) return;
        isSongLikedOrInPlaylist(songId).then(active => {
            if (current && current.songId === songId) {
                btn.classList.toggle("liked", active);
            }
        }).catch(() => { });
    }
    async function toggleLikeCurrent() {
        if (!current || !current.songId) return;
        if (!window.IS_AUTHENTICATED) {
            if (typeof Toast !== "undefined") {
                Toast.info("Vui lòng đăng nhập để sử dụng chức năng này.");
            }
            return;
        }
        if (!window.LikedSongs) return;
        const songId = current.songId;
        const btn = document.getElementById("btnAddToPlaylist");
        try {
            const inAny = await isSongInAnyPlaylist(songId);
            if (inAny) {
                if (window.PlaylistPicker) window.PlaylistPicker.open(songId);
                return;
            }
            const nowLiked = await LikedSongs.toggle(songId);
            if (btn && current && current.songId === songId) {
                btn.classList.toggle("liked", nowLiked);
            }
            // Đồng bộ heart ở trang song detail + dấu +/✓ ở mọi nơi
            if (window.refreshSongAddButtons) await window.refreshSongAddButtons(songId);
            // Kích onChange để song-detail, artist, album cập nhật UI heart
            notifyChange();
            if (typeof Toast === "undefined" || !Toast.show) return;
            if (nowLiked) {
                Toast.show("Đã thêm vào Bài hát đã thích", "success", 4000, {
                    actionText: "Thay đổi",
                    onAction: () => {
                        if (window.PlaylistPicker) window.PlaylistPicker.open(songId);
                    }
                });
            } else {
                Toast.show("Đã xoá khỏi Bài hát đã thích");
            }
        } catch (err) {
            console.error(err);
            if (typeof Toast !== "undefined") {
                Toast.error("Không thể cập nhật Bài hát đã thích.");
            }
        }
    }
    function refreshLikeButton(songId) {
        updateLikeButtonUI(songId);
    }
    function openAddToPlaylist() {
        if (!current || !current.songId) return;
        if (!window.IS_AUTHENTICATED) {
            if (typeof Toast !== "undefined") {
                Toast.info("Vui lòng đăng nhập để sử dụng chức năng này.");
            }
            return;
        }
        if (!window.PlaylistPicker) return;
        window.PlaylistPicker.open(current.songId);
    }
    function togglePlay() {
        if (!current) return;
        if (isAdShowing) return;
        if (audio.paused) {
            audio.play().catch(handlePlayError);
        } else {
            audio.pause();
        }
    }
    function next() {
        if (repeatMode === "one" && current) {
            if (isArtistBlocked(current.mainArtistID)) {
                pos = (pos + 1) % order.length;
                const song = playlist[order[pos]];
                playSong(song);
                return;
            }
            audio.currentTime = 0;
            audio.play().catch(() => { });
            return;
        }
        if (queue.length > 0) {
            const upcoming = queue.shift();
            if (!isArtistBlocked(upcoming.mainArtistID)) {
                playSong(upcoming);
                return;
            }
        }
        if (order.length === 0) return;
        let tried = 0;
        do {
            pos = (pos + 1) % order.length;
            tried++;
        } while (tried < order.length && isArtistBlocked((playlist[order[pos]] || {}).mainArtistID));
        const song = playlist[order[pos]];
        if (song && !isArtistBlocked(song.mainArtistID)) {
            playSong(song);
        } else {
            audio.pause();
            notifyChange();
        }
    }
    function prev() {
        if (audio.currentTime > 3) {
            audio.currentTime = 0;
            return;
        }
        if (order.length === 0) return;
        pos = (pos - 1 + order.length) % order.length;
        const song = playlist[order[pos]];
        playSong(song);
    }
    function shuffleArray(arr) {
        const a = [...arr];
        for (let i = a.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [a[i], a[j]] = [a[j], a[i]];
        }
        return a;
    }
    function toggleShuffle() {
        isShuffle = !isShuffle;
        const currentPlaylistIndex = (pos >= 0 && order[pos] !== undefined) ? order[pos] : -1;
        if (currentPlaylistIndex !== -1 && playlist.length > 0) {
            if (isShuffle) {
                const rest = playlist.map((_, i) => i).filter(i => i !== currentPlaylistIndex);
                order = [currentPlaylistIndex, ...shuffleArray(rest)];
            } else {
                order = playlist.map((_, i) => i);
            }
            pos = order.indexOf(currentPlaylistIndex);
        }
        toggleActiveClass(els.btnShuffle(), isShuffle);
        if (typeof Toast !== "undefined") {
            Toast.info(isShuffle ? "Đã bật phát ngẫu nhiên." : "Đã tắt phát ngẫu nhiên.");
        }
        saveState();
    }
    function toggleRepeat() {
        repeatMode = repeatMode === "off" ? "all" : repeatMode === "all" ? "one" : "off";
        renderRepeatButton();
        if (typeof Toast !== "undefined") {
            const msg = repeatMode === "all" ? "Đã bật lặp lại danh sách."
                : repeatMode === "one" ? "Đã bật lặp lại 1 bài hát."
                    : "Đã tắt lặp lại.";
            Toast.info(msg);
        }
        saveState();
    }
    function renderRepeatButton() {
        const btn = els.btnRepeat();
        if (!btn) return;
        btn.classList.toggle("active", repeatMode !== "off");
        btn.classList.toggle("repeat-one", repeatMode === "one");
        btn.title = repeatMode === "all" ? "Lặp lại danh sách"
            : repeatMode === "one" ? "Lặp lại 1 bài hát"
                : "Lặp lại";
    }
    function toggleActiveClass(el, active) {
        if (!el) return;
        el.classList.toggle("active", active);
    }
    function toggleMute() {
        if (isMuted) {
            audio.volume = volumeBeforeMute;
            isMuted = false;
        } else {
            volumeBeforeMute = audio.volume || 1;
            audio.volume = 0;
            isMuted = true;
        }
        renderVolume();
        saveState();
    }
    function setVolumeFromClientX(clientX) {
        const track = els.volumeTrack();
        if (!track) return;
        const rect = track.getBoundingClientRect();
        const fraction = Math.min(1, Math.max(0, (clientX - rect.left) / rect.width));
        audio.volume = fraction;
        isMuted = fraction === 0;
        renderVolume();
        saveState();
    }
    function renderVolume() {
        const fill = els.volumeFill();
        const icon = els.volumeIcon();
        if (fill) fill.style.width = `${audio.volume * 100}%`;
        if (icon) icon.setAttribute("d", audio.volume === 0 ? ICON_MUTED : ICON_VOLUME);
    }
    function setSeekFromClientX(clientX) {
        const track = els.progressTrack();
        if (!track || !audio.duration) return;
        const rect = track.getBoundingClientRect();
        const fraction = Math.min(1, Math.max(0, (clientX - rect.left) / rect.width));
        let target = audio.duration * fraction;
        if (current && current.maxPreviewSeconds && target >= current.maxPreviewSeconds) {
            target = Math.max(0, current.maxPreviewSeconds - 1);
        }
        audio.currentTime = target;
    }
    function bindDrag(trackEl, onMove) {
        if (!trackEl) return;
        let dragging = false;
        trackEl.addEventListener("mousedown", (e) => {
            dragging = true;
            onMove(e.clientX);
        });
        window.addEventListener("mousemove", (e) => {
            if (dragging) onMove(e.clientX);
        });
        window.addEventListener("mouseup", () => {
            dragging = false;
        });
    }
    function formatTime(totalSeconds) {
        const minutes = Math.floor(totalSeconds / 60);
        const seconds = Math.floor(totalSeconds % 60);
        return `${minutes}:${seconds.toString().padStart(2, "0")}`;
    }
    function toggleLyrics() {
        const modal = els.modalLyrics();
        if (!modal) return;
        const opening = !modal.classList.contains("active");
        if (opening && current) {
            if (els.lyricsTitle()) els.lyricsTitle().textContent = current.title;
            if (els.lyricsArtist()) els.lyricsArtist().textContent = current.artistName || "Nghệ sĩ ẩn danh";
            if (els.lyricsContent()) els.lyricsContent().textContent = "Chưa có lời bài hát cho bài này.";
        }
        modal.classList.toggle("active", opening);
        modal.style.display = opening ? "flex" : "none";
    }
    function closeLyrics() {
        const modal = els.modalLyrics();
        if (!modal) return;
        modal.classList.remove("active");
        modal.style.display = "none";
    }
    audio.addEventListener("timeupdate", () => {
        if (els.currentTime()) els.currentTime().textContent = formatTime(audio.currentTime);
        if (els.progressFill() && audio.duration) {
            els.progressFill().style.width = `${(audio.currentTime / audio.duration) * 100}%`;
        }
        if (current && current.maxPreviewSeconds && audio.currentTime >= current.maxPreviewSeconds) {
            enforcePremiumPreviewLimit();
            return;
        }
        if (!hasCountedPlay && audio.currentTime >= 3 && current) {
            hasCountedPlay = true;
            fetch(`/api/musicplayer/play/${current.songId}`, { method: "POST" });
            if (!isPremiumUser) {
                songsPlayedSinceAd++;
            }
        }
        const earningThreshold = (audio.duration && audio.duration < MIN_LISTEN_SECONDS_FOR_EARNING)
            ? audio.duration * 0.8
            : MIN_LISTEN_SECONDS_FOR_EARNING;
        if (!hasCountedEarning && current && audio.currentTime >= earningThreshold) {
            hasCountedEarning = true;
            fetch(`/api/musicplayer/earn/${current.songId}`, { method: "POST" });
        }
        if (Math.floor(audio.currentTime) % 3 === 0) {
            saveState();
        }
    });
    audio.addEventListener("loadedmetadata", () => {
        if (els.totalTime() && audio.duration) {
            els.totalTime().textContent = formatTime(audio.duration);
        }
    });
    audio.addEventListener("play", () => {
        if (els.playPauseIcon()) els.playPauseIcon().setAttribute("d", ICON_PAUSE);
        saveState();
        notifyChange();
    });
    audio.addEventListener("pause", () => {
        if (els.playPauseIcon()) els.playPauseIcon().setAttribute("d", ICON_PLAY);
        saveState();
        notifyChange();
    });
    audio.addEventListener("ended", () => {
        if (!isPremiumUser && songsPlayedSinceAd >= adThreshold) {
            showAd(() => {
                next();
                notifyChange();
            });
            return;
        }
        next();
        notifyChange();
    });
    window.addEventListener("beforeunload", saveState);
    document.addEventListener("DOMContentLoaded", () => {
        bindDrag(els.progressTrack(), setSeekFromClientX);
        bindDrag(els.volumeTrack(), setVolumeFromClientX);
        renderVolume();
        renderRepeatButton();
        refreshPremiumStatus();
        loadBlockedArtistIds();
    });
    restoreState();
    function getQueue() { return [...queue]; }
    function getCurrent() { return current ? { ...current } : null; }
    function isPlaying() { return !audio.paused && !audio.ended; }
    function onChange(fn) {
        if (typeof fn === "function") changeListeners.push(fn);
    }
    function goToSongDetail() {
        if (!current || !current.songId) return;
        window.location.href = `/Song/Detail/${current.songId}`;
    }
    return {
        playSong,
        playFromList,
        addToQueue,
        playPlaylist,
        togglePlay,
        next,
        prev,
        toggleShuffle,
        toggleRepeat,
        toggleMute,
        openAddToPlaylist,
        toggleLikeCurrent,
        refreshLikeButton,
        isSongInAnyPlaylist,
        toggleLyrics,
        closeLyrics,
        goToSongDetail,
        getQueue,
        getCurrent,
        isPlaying,
        onChange,
        notifyArtistBlocked
    };
})();
window.Player = Player;

// Đồng bộ trạng thái nút "+" (thêm vào playlist) ở các trang album/song với logic của trái tim
// ở playbar: xanh nếu bài hát đang ở bất kỳ playlist nào (kể cả Bài hát đã thích),
// bấm vào sẽ mở popup nếu đã có ở playlist nào đó, ngược lại thêm thẳng vào Bài hát đã thích.
window.refreshSongAddButtons = async function (songId) {
    if (!songId || !window.Player || typeof Player.isSongInAnyPlaylist !== "function") return;
    let inAny = false;
    let isLiked = false;
    try {
        const [inPlaylist, likedResult] = await Promise.all([
            Player.isSongInAnyPlaylist(songId),
            (async () => {
                try {
                    if (!window.LikedSongs || !window.IS_AUTHENTICATED) return false;
                    const ids = await LikedSongs.loadLikedIds();
                    return ids instanceof Set ? ids.has(songId) : false;
                } catch { return false; }
            })()
        ]);
        isLiked = likedResult;
        inAny = inPlaylist || isLiked;
    } catch (e) {
        return;
    }
    if (typeof Player.refreshLikeButton === "function") Player.refreshLikeButton(songId);
    // Đồng bộ likeToggle button trong more-menu của artist/album pages
    const likeToggleBtn = document.getElementById(`likeToggle-${songId}`);
    if (likeToggleBtn) {
        likeToggleBtn.textContent = isLiked
            ? "✓ Xóa khỏi Bài hát đã thích"
            : "♡ Thêm vào Bài hát đã thích";
    }
    document.querySelectorAll(`.row-add-btn[data-song-id="${songId}"]`).forEach(btn => {
        btn.classList.toggle("added", inAny);
        btn.textContent = inAny ? "✓" : "+";
        btn.title = inAny ? "Đã thêm vào playlist" : "Thêm vào playlist";
    });
    const songDetailRoot = document.getElementById("viewSongDetail");
    if (songDetailRoot && Number(songDetailRoot.dataset.songId) === Number(songId)) {
        const addBtn = document.getElementById("btnAddLibrarySong");
        if (addBtn) addBtn.classList.toggle("added", inAny);
        // Đồng bộ heart button trên trang song detail
        const heartBtn = document.getElementById("btnSongDetailLike");
        if (heartBtn) {
            heartBtn.classList.toggle("liked", isLiked);
            heartBtn.title = isLiked ? "Xóa khỏi Bài hát đã thích" : "Thêm vào Bài hát đã thích";
        }
    }
};

window.handleSongAddButtonClick = async function (songId) {
    if (!songId) return;
    if (!window.IS_AUTHENTICATED) {
        if (typeof Toast !== "undefined") Toast.info("Vui lòng đăng nhập để sử dụng chức năng này.");
        return;
    }
    if (!window.Player || typeof Player.isSongInAnyPlaylist !== "function") {
        if (window.PlaylistPicker) window.PlaylistPicker.open(songId);
        return;
    }
    try {
        const inAny = await Player.isSongInAnyPlaylist(songId);
        if (inAny) {
            if (window.PlaylistPicker) window.PlaylistPicker.open(songId);
            return;
        }
        if (!window.LikedSongs) return;
        const nowLiked = await LikedSongs.toggle(songId);
        await window.refreshSongAddButtons(songId);
        if (typeof Toast === "undefined" || !Toast.show) return;
        if (nowLiked) {
            Toast.show("Đã thêm vào Bài hát đã thích", "success", 4000, {
                actionText: "Thay đổi",
                onAction: () => {
                    if (window.PlaylistPicker) window.PlaylistPicker.open(songId);
                }
            });
        } else {
            Toast.show("Đã xoá khỏi Bài hát đã thích");
        }
    } catch (err) {
        console.error(err);
        if (typeof Toast !== "undefined") Toast.error("Không thể cập nhật Bài hát đã thích.");
    }
};