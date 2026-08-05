const DEFAULT_COVER = "/images/default-cover.png";
window.UI = window.UI || {};
const Playlist = {
    currentPlaylistId: null,
    currentSongs: [],
    _playbackBound: false,
    openCreateModal() {
        if (!window.IS_AUTHENTICATED) {
            if (typeof Toast !== "undefined" && Toast.show) {
                Toast.show("Bạn phải đăng nhập mới sử dụng được chức năng này.", "error");
            } else {
                alert("Bạn phải đăng nhập mới sử dụng được chức năng này.");
            }
            return;
        }
        const overlay = document.getElementById("modalCreatePlaylist");
        if (!overlay) return;
        document.getElementById("playlistNameInput").value = "";
        const desc = document.getElementById("playlistDescInput");
        const img = document.getElementById("playlistCoverInput");
        const preview = document.getElementById("playlistCoverPreview");
        if (desc) desc.value = "";
        if (img) img.value = "";
        if (preview) preview.src = DEFAULT_COVER;
        overlay.classList.add("open");
        document.getElementById("playlistNameInput").focus();
    },
    closeCreateModal() {
        const overlay = document.getElementById("modalCreatePlaylist");
        if (overlay) overlay.classList.remove("open");
    },
    previewCover() {
        const img = document.getElementById("playlistCoverInput");
        const preview = document.getElementById("playlistCoverPreview");
        if (img && preview) {
            preview.src = img.value.trim() || DEFAULT_COVER;
        }
    },
    async createPlaylist() {
        const nameInput = document.getElementById("playlistNameInput");
        const descInput = document.getElementById("playlistDescInput");
        const imgInput = document.getElementById("playlistCoverInput");
        const name = nameInput.value.trim() || "Playlist của tôi";
        const description = descInput ? descInput.value.trim() : "";
        const coverImage = imgInput ? imgInput.value.trim() : "";
        try {
            const res = await fetch("/api/playlists", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    playlistName: name,
                    description: description,
                    coverImage: coverImage,
                    isPublic: true
                })
            });
            if (!res.ok) throw new Error("Tạo playlist thất bại");
            const newPlaylistId = await res.json();
            this.closeCreateModal();
            if (typeof Toast !== "undefined" && Toast.show) {
                Toast.show("Đã tạo playlist mới");
            }
            await this.loadSidebarLibrary();
            window.location.href = `/Playlist/Detail/${newPlaylistId}`;
        } catch (err) {
            console.error(err);
            if (typeof Toast !== "undefined" && Toast.show) {
                Toast.show("Không thể tạo playlist. Vui lòng thử lại.");
            } else {
                alert("Không thể tạo playlist. Vui lòng thử lại.");
            }
        }
    },
    _renderArtistItem(a) {
        return `
            <a class="library-item" data-type="artist" id="sidebarArtist-${a.artistID}" href="/Artist/Detail/${a.artistID}">
                <img class="library-item-img" src="${a.avatar || DEFAULT_COVER}" onerror="this.onerror=null;this.src='${DEFAULT_COVER}'" alt="${escapeHtml(a.artistName)}" style="border-radius:50%;">
                <div class="library-item-info">
                    <div class="library-item-name">${escapeHtml(a.artistName)}</div>
                    <div class="library-item-meta">Nghệ sĩ</div>
                </div>
            </a>`;
    },
    async loadSidebarLibrary() {
        const list = document.getElementById("libraryList");
        if (!list) return;
        try {
            const [playlistsRes, followedRes] = await Promise.all([
                fetch("/api/playlists"),
                window.IS_AUTHENTICATED ? fetch("/api/Artist/followed") : Promise.resolve(null)
            ]);
            if (!playlistsRes.ok) throw new Error("Không tải được thư viện");
            const playlists = await playlistsRes.json();
            const followedArtists = (followedRes && followedRes.ok) ? await followedRes.json() : [];
            if (!playlists.length && !followedArtists.length) {
                list.innerHTML = `
                    <div class="empty-state" style="padding: 24px 16px;">
                        <p style="font-size:13px; text-align:center; color: var(--color-text-muted);">Tạo playlist đầu tiên của bạn!</p>
                    </div>`;
                return;
            }
            const playlistItems = playlists.map(p => {
                const isFavorites = p.playlistName === "Bài hát yêu thích";
                const imgHtml = isFavorites
                    ? `<div class="library-item-img" style="display:flex;align-items:center;justify-content:center;background:linear-gradient(135deg,#450af5,#c4efd9);font-size:18px;">💚</div>`
                    : `<img class="library-item-img" src="${p.coverImage || DEFAULT_COVER}" onerror="this.onerror=null;this.src='${DEFAULT_COVER}'" alt="${escapeHtml(p.playlistName)}">`;
                return `
                <a class="library-item" data-type="playlist" href="/Playlist/Detail/${p.playlistID}">
                    ${imgHtml}
                    <div class="library-item-info">
                        <div class="library-item-name">${escapeHtml(p.playlistName)}</div>
                        <div class="library-item-meta">Playlist • ${p.songCount} bài hát</div>
                    </div>
                </a>`;
            }).join("");
            const artistItems = followedArtists.map(a => this._renderArtistItem(a)).join("");
            list.innerHTML = playlistItems + artistItems;
            if (window.UI && UI.refreshLibraryFilters) UI.refreshLibraryFilters();
        } catch (err) {
            console.error(err);
        }
    },
    addArtistToSidebar(artist) {
        const list = document.getElementById("libraryList");
        if (!list) return;
        if (document.getElementById(`sidebarArtist-${artist.artistID}`)) return;
        const emptyState = list.querySelector(".empty-state");
        if (emptyState) emptyState.remove();
        list.insertAdjacentHTML("beforeend", this._renderArtistItem(artist));
        if (window.UI && UI.refreshLibraryFilters) UI.refreshLibraryFilters();
    },
    removeArtistFromSidebar(artistId) {
        const list = document.getElementById("libraryList");
        const item = document.getElementById(`sidebarArtist-${artistId}`);
        if (item) item.remove();
        if (list && !list.querySelector(".library-item")) {
            list.innerHTML = `
                <div class="empty-state" style="padding: 24px 16px;">
                    <p style="font-size:13px; text-align:center; color: var(--color-text-muted);">Tạo playlist đầu tiên của bạn!</p>
                </div>`;
        }
        if (window.UI && UI.refreshLibraryFilters) UI.refreshLibraryFilters();
    },
    async loadDetail(playlistId) {
        this.currentPlaylistId = playlistId;
        this.bindPlaybackControls();
        try {
            const res = await fetch(`/api/playlists/${playlistId}`);
            if (!res.ok) throw new Error("Không tìm thấy playlist");
            const data = await res.json();
            this.renderDetail(data);
        } catch (err) {
            console.error(err);
            if (typeof Toast !== "undefined" && Toast.show) {
                Toast.show("Không thể tải playlist");
            }
        }
    },
    renderDetail(data) {
        const p = data.playlist;
        this.currentPlaylistData = p;
        document.getElementById("plCoverImg").src = p.coverImage || DEFAULT_COVER;
        document.getElementById("plName").innerText = p.playlistName || "Playlist của tôi";
        const descEl = document.getElementById("plDescription");
        if (p.description && p.description.trim()) {
            descEl.innerText = p.description;
            descEl.style.display = "";
        } else {
            descEl.innerText = "";
            descEl.style.display = "none";
        }
        document.getElementById("plOwner").innerText = p.ownerName || "Bạn";
        const totalMin = Math.floor(p.totalDurationSeconds / 60);
        document.getElementById("plSongCount").innerText = `${p.songCount} bài hát`;
        document.getElementById("plDuration").innerText = totalMin > 0 ? `khoảng ${totalMin} phút` : "";
        document.getElementById("plCreatedAt").innerText = p.createdAt
            ? new Date(p.createdAt).toLocaleDateString("vi-VN")
            : "";
        const isFavoritePlaylist = p.playlistName === "Bài hát yêu thích";
        const editBtn = document.getElementById("btnEditPlaylist");
        const deleteBtn = document.getElementById("btnDeletePlaylist");
        if (editBtn) editBtn.style.display = isFavoritePlaylist ? "none" : "";
        if (deleteBtn) deleteBtn.style.display = isFavoritePlaylist ? "none" : "";
        const listEl = document.getElementById("plSongList");
        this.currentSongs = data.songs || [];
        if (!data.songs || data.songs.length === 0) {
            listEl.innerHTML = `<div class="pl-empty">Playlist này chưa có bài hát nào. Nhấn "Thêm bài hát" để bắt đầu.</div>`;
            this.updatePlaybackUI();
            return;
        }
        listEl.innerHTML = data.songs.map((s, index) => `
            <div class="pl-song-row" data-song-id="${s.songID}">
                <div class="pl-col-idx">
                    <span class="pl-idx-num">${index + 1}</span>
                    <span class="pl-idx-play" title="Phát" data-index="${index}">▶</span>
                </div>
                <div class="pl-col-title">
                    <img src="${s.coverImage || DEFAULT_COVER}" onerror="this.onerror=null;this.src='${DEFAULT_COVER}'" alt="">
                    <div class="pl-song-title-wrap">
                        <div class="pl-song-title"><a href="/Song/Detail/${s.songID}">${escapeHtml(s.title)}</a></div>
                        <div class="pl-song-artist">${s.artistID
                ? `<a href="/Artist/Detail/${s.artistID}">${escapeHtml(s.artist || "")}</a>`
                : escapeHtml(s.artist || "")}</div>
                    </div>
                </div>
                <div class="pl-col-album">${escapeHtml(s.album || "")}</div>
                <div class="pl-col-duration">${formatDuration(s.duration)}</div>
                <div class="pl-col-action">
                    <button class="pl-remove-btn" title="Báo cáo bài hát"
                            data-report-song-id="${s.songID}" data-report-song-title="${escapeHtml(s.title)}">⚑</button>
                    <button class="pl-remove-btn" title="Xoá khỏi playlist" onclick="Playlist.removeSong(${s.songID})">✕</button>
                </div>
            </div>`).join("");
        this.updatePlaybackUI();
    },
    bindPlaybackControls() {
        if (this._playbackBound) return;
        this._playbackBound = true;
        const listEl = document.getElementById("plSongList");
        if (listEl) {
            listEl.addEventListener("click", (e) => {
                const btn = e.target.closest(".pl-idx-play");
                if (!btn) return;
                const row = btn.closest(".pl-song-row");
                if (!row) return;
                const songId = Number(row.dataset.songId);
                const index = this.currentSongs.findIndex(s => s.songID === songId);
                if (index === -1) return;
                this.playSongAt(index);
            });
        }
        const actionBtn = document.getElementById("plActionPlayBtn");
        if (actionBtn) {
            actionBtn.addEventListener("click", () => this.playAll());
        }
    },
    buildPlayerSongList() {
        return (this.currentSongs || []).map(s => ({
            songId: s.songID,
            title: s.title,
            audioUrl: s.audioUrl,
            durationInSeconds: s.duration,
            coverImageUrl: s.coverImage,
            artistName: s.artist
        }));
    },
    playAll() {
        if (!window.Player || !this.currentSongs || this.currentSongs.length === 0) return;
        const current = Player.getCurrent();
        const isThisPlaylist = !!(current && this.currentSongs.some(s => s.songID === current.songId));
        if (isThisPlaylist) {
            Player.togglePlay();
        } else {
            this.playSongAt(0);
        }
    },
    playSongAt(index) {
        if (!window.Player || !this.currentSongs || !this.currentSongs[index]) return;
        const song = this.currentSongs[index];
        const current = Player.getCurrent();
        if (current && current.songId === song.songID) {
            Player.togglePlay();
            return;
        }
        const list = this.buildPlayerSongList();
        Player.playFromList(list, index);
    },
    updatePlaybackUI() {
        const current = window.Player && Player.getCurrent ? Player.getCurrent() : null;
        const playing = window.Player && Player.isPlaying ? Player.isPlaying() : false;
        const isThisPlaylist = !!(current && (this.currentSongs || []).some(s => s.songID === current.songId));
        const actionBtn = document.getElementById("plActionPlayBtn");
        if (actionBtn) {
            actionBtn.textContent = (isThisPlaylist && playing) ? "❚❚" : "▶";
        }
        document.querySelectorAll("#plSongList .pl-song-row").forEach(row => {
            const songId = Number(row.dataset.songId);
            const isCurrentRow = !!(current && songId === current.songId);
            row.classList.toggle("pl-row-playing", isCurrentRow);
            const playBtn = row.querySelector(".pl-idx-play");
            if (playBtn) playBtn.textContent = (isCurrentRow && playing) ? "❚❚" : "▶";
        });
    },
    openEditModal() {
        if (!this.currentPlaylistId) return;
        const p = this.currentPlaylistData || {};
        document.getElementById("editCover").value = p.coverImage || "";
        document.getElementById("editName").value = p.playlistName || "";
        document.getElementById("editDescription").value = p.description || "";
        const overlay = document.getElementById("modalEditPlaylist");
        if (overlay) overlay.classList.add("open");
    },
    closeEditModal() {
        const overlay = document.getElementById("modalEditPlaylist");
        if (overlay) overlay.classList.remove("open");
    },
    async saveEdit() {
        if (!this.currentPlaylistId) return;
        let name = (document.getElementById("editName").value || "").trim();
        if (!name) name = "Playlist của tôi";
        const cover = (document.getElementById("editCover").value || "").trim();
        const description = (document.getElementById("editDescription").value || "").trim();
        const isPublic = this.currentPlaylistData ? !!this.currentPlaylistData.isPublic : true;
        try {
            const res = await fetch(`/api/playlists/${this.currentPlaylistId}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    playlistName: name,
                    coverImage: cover,
                    description: description,
                    isPublic: isPublic
                })
            });
            if (!res.ok) throw new Error("Cập nhật playlist thất bại");
            this.closeEditModal();
            if (typeof Toast !== "undefined" && Toast.show) Toast.show("Đã lưu thay đổi playlist");
            const detailRes = await fetch(`/api/playlists/${this.currentPlaylistId}`);
            if (detailRes.ok) this.renderDetail(await detailRes.json());
            await this.loadSidebarLibrary();
        } catch (err) {
            console.error(err);
            if (typeof Toast !== "undefined" && Toast.show) Toast.show("Không thể lưu thay đổi playlist");
        }
    },
    async deletePlaylist() {
        if (!this.currentPlaylistId) return;
        if (!confirm("Bạn có chắc muốn xoá playlist này?")) return;
        try {
            const res = await fetch(`/api/playlists/${this.currentPlaylistId}`, { method: "DELETE" });
            if (!res.ok) throw new Error("Xoá thất bại");
            await this.loadSidebarLibrary();
            window.location.href = "/";
        } catch (err) {
            console.error(err);
            if (typeof Toast !== "undefined" && Toast.show) Toast.show("Không thể xoá playlist");
        }
    },
    genreListCache: null,
    selectedGenreId: null,
    genreChipColors: [
        "#e91429", "#1e3264", "#e8115b", "#8d67ab", "#158a49",
        "#dc148c", "#777777", "#ba5d07", "#d84000", "#608108",
        "#503750", "#af2896", "#0d73ec", "#e1118c", "#456559"
    ],
    openAddSongModal() {
        const overlay = document.getElementById("modalAddSong");
        if (!overlay) return;
        overlay.classList.add("open");
        document.getElementById("addSongSearchInput").value = "";
        this.selectedGenreId = null;
        this.loadGenreChips();
        this.searchAvailableSongs("");
    },
    closeAddSongModal() {
        const overlay = document.getElementById("modalAddSong");
        if (overlay) overlay.classList.remove("open");
    },
    async loadGenreChips() {
        const row = document.getElementById("genreChipRow");
        if (!row) return;
        try {
            if (!this.genreListCache) {
                const res = await fetch("/api/search/genres");
                this.genreListCache = res.ok ? await res.json() : [];
            }
            row.innerHTML = this.genreListCache.map((g, i) => `
                <button type="button"
                        class="pl-genre-chip"
                        data-genre-id="${g.genreID}"
                        style="background:${this.genreChipColors[i % this.genreChipColors.length]}">
                    ${escapeHtml(g.genreName)}
                </button>`).join("");
            row.querySelectorAll(".pl-genre-chip").forEach(chip => {
                chip.addEventListener("click", () => {
                    const genreId = Number(chip.dataset.genreId);
                    const willSelect = this.selectedGenreId !== genreId;
                    this.selectedGenreId = willSelect ? genreId : null;
                    row.querySelectorAll(".pl-genre-chip").forEach(c =>
                        c.classList.toggle("active", willSelect && Number(c.dataset.genreId) === genreId));
                    this.searchAvailableSongs(document.getElementById("addSongSearchInput").value);
                });
            });
        } catch (err) {
            console.error(err);
        }
    },
    searchTimer: null,
    searchAvailableSongs(keyword) {
        clearTimeout(this.searchTimer);
        this.searchTimer = setTimeout(async () => {
            if (!this.currentPlaylistId) return;
            try {
                const params = new URLSearchParams({ search: keyword || "" });
                if (this.selectedGenreId) params.set("genreId", this.selectedGenreId);
                const url = `/api/playlists/${this.currentPlaylistId}/available-songs?${params.toString()}`;
                const res = await fetch(url);
                const songs = await res.json();
                this.renderAvailableSongs(songs);
            } catch (err) {
                console.error(err);
            }
        }, 250);
    },
    renderAvailableSongs(songs) {
        const container = document.getElementById("addSongResults");
        if (!songs || songs.length === 0) {
            container.innerHTML = `<div class="pl-empty">Không tìm thấy bài hát phù hợp.</div>`;
            return;
        }
        container.innerHTML = songs.map(s => `
            <div class="pl-add-song-item" data-song-id="${s.songID}">
                <img src="${s.coverImage || DEFAULT_COVER}" onerror="this.onerror=null;this.src='${DEFAULT_COVER}'" alt="">
                <div class="pl-add-song-info">
                    <div class="pl-song-title"><a href="/Song/Detail/${s.songID}" onclick="event.stopPropagation();">${escapeHtml(s.title)}</a></div>
                    <div class="pl-song-artist">${s.artistID
                ? `<a href="/Artist/Detail/${s.artistID}" onclick="event.stopPropagation();">${escapeHtml(s.artist || "")}</a>`
                : escapeHtml(s.artist || "")} • ${escapeHtml(s.album || "")}</div>
                </div>
                <button class="pl-add-song-btn${s.isAdded ? " added" : ""}"
                        title="${s.isAdded ? "Xoá khỏi playlist" : "Thêm vào playlist"}"
                        onclick="Playlist.toggleAvailableSong(${s.songID}, this)">${s.isAdded ? "✓" : "+"}</button>
            </div>`).join("");
    },
    async toggleAvailableSong(songId, btnEl) {
        if (!this.currentPlaylistId) return;
        const wasAdded = btnEl && btnEl.classList.contains("added");
        if (btnEl) btnEl.disabled = true;
        try {
            const res = await fetch(`/api/playlists/${this.currentPlaylistId}/songs/${songId}`, {
                method: wasAdded ? "DELETE" : "POST"
            });
            if (!res.ok) {
                const data = await res.json().catch(() => ({}));
                if (res.status === 403 && data.limitReached) {
                    if (typeof Toast !== "undefined" && Toast.error) Toast.error(data.message);
                    else if (typeof Toast !== "undefined" && Toast.show) Toast.show(data.message, "error");
                    return;
                }
                throw new Error(wasAdded ? "Xoá bài hát thất bại" : "Thêm bài hát thất bại");
            }
            if (btnEl) {
                btnEl.classList.toggle("added", !wasAdded);
                btnEl.innerText = wasAdded ? "+" : "✓";
                btnEl.title = wasAdded ? "Thêm vào playlist" : "Xoá khỏi playlist";
            }
            const detailRes = await fetch(`/api/playlists/${this.currentPlaylistId}`);
            const data = await detailRes.json();
            this.renderDetail(data);
            await this.loadSidebarLibrary();
        } catch (err) {
            console.error(err);
            if (typeof Toast !== "undefined" && Toast.show) {
                Toast.show(wasAdded ? "Không thể xoá bài hát" : "Không thể thêm bài hát");
            }
        } finally {
            if (btnEl) btnEl.disabled = false;
        }
    },
    async removeSong(songId) {
        if (!this.currentPlaylistId) return;
        try {
            const res = await fetch(`/api/playlists/${this.currentPlaylistId}/songs/${songId}`, {
                method: "DELETE"
            });
            if (!res.ok) throw new Error("Xoá bài hát thất bại");
            const detailRes = await fetch(`/api/playlists/${this.currentPlaylistId}`);
            const data = await detailRes.json();
            this.renderDetail(data);
            await this.loadSidebarLibrary();
        } catch (err) {
            console.error(err);
            if (typeof Toast !== "undefined" && Toast.show) Toast.show("Không thể xoá bài hát");
        }
    }
};
window.Playlist = Playlist;
window.UI.openCreatePlaylist = () => Playlist.openCreateModal();
window.UI.closeCreatePlaylist = () => Playlist.closeCreateModal();
const PlaylistPicker = {
    currentSongId: null,
    favPlaylistId: null,
    async open(songId) {
        if (!window.IS_AUTHENTICATED) {
            if (typeof Toast !== "undefined" && Toast.show) {
                Toast.show("Bạn phải đăng nhập mới sử dụng được chức năng này.", "error");
            } else {
                alert("Bạn phải đăng nhập mới sử dụng được chức năng này.");
            }
            return;
        }
        this.currentSongId = songId;
        this.favPlaylistId = null;
        const isBulk = Array.isArray(songId);
        const overlay = document.getElementById("modalAddToPlaylist");
        const list = document.getElementById("addToPlaylistList");
        if (!overlay || !list) return;
        list.innerHTML = `<div class="pl-empty">Đang tải...</div>`;
        overlay.classList.add("open");
        try {
            const [plRes, containRes] = await Promise.all([
                fetch("/api/playlists"),
                isBulk ? Promise.resolve(null) : fetch(`/api/playlists/containing/${songId}`)
            ]);
            const playlists = plRes.ok ? await plRes.json() : [];
            const containingIds = (!isBulk && containRes.ok) ? await containRes.json() : [];
            const favPlaylist = playlists.find(p => p.playlistName === "Bài hát yêu thích");
            const others = playlists.filter(p => p.playlistName !== "Bài hát yêu thích");
            this.favPlaylistId = favPlaylist ? favPlaylist.playlistID : null;
            const favAdded = favPlaylist ? containingIds.includes(favPlaylist.playlistID) : false;
            list.innerHTML = this._renderRow({
                fav: true,
                cover: null,
                name: "Bài hát yêu thích",
                added: favAdded
            }) + others.map(p => this._renderRow({
                fav: false,
                playlistId: p.playlistID,
                cover: p.coverImage,
                name: p.playlistName,
                added: containingIds.includes(p.playlistID)
            })).join("");
            list.querySelectorAll(".pl-picker-item").forEach(btn => {
                btn.addEventListener("click", () => {
                    const isAdded = isBulk ? false : btn.classList.contains("pl-picker-added");
                    if (btn.dataset.fav) {
                        PlaylistPicker.toggleFavorites(btn, isAdded);
                    } else {
                        const playlistId = Number(btn.dataset.playlistId);
                        const playlistName = btn.querySelector(".pl-picker-name").textContent;
                        PlaylistPicker.togglePlaylist(playlistId, playlistName, btn, isAdded);
                    }
                });
            });
        } catch (err) {
            console.error(err);
            list.innerHTML = `<div class="pl-empty">Không thể tải danh sách playlist.</div>`;
        }
    },
    _renderRow({ fav, playlistId, cover, name, added }) {
        const coverHtml = fav
            ? `<span class="pl-picker-cover pl-picker-cover-fav">❤</span>`
            : `<img class="pl-picker-cover" src="${cover || DEFAULT_COVER}" onerror="this.onerror=null;this.src='${DEFAULT_COVER}'" alt="">`;
        return `
            <button class="pl-picker-item${added ? " pl-picker-added" : ""}"
                    ${fav ? 'data-fav="1"' : `data-playlist-id="${playlistId}"`}>
                ${coverHtml}
                <span class="pl-picker-name">${escapeHtml(name)}</span>
                <span class="pl-picker-check" aria-hidden="true">
                    <svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor">
                        <path d="M9 16.2 4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4z"/>
                    </svg>
                </span>
            </button>`;
    },
    close() {
        const overlay = document.getElementById("modalAddToPlaylist");
        if (overlay) overlay.classList.remove("open");
    },
    async toggleFavorites(btnEl, isAdded) {
        if (!this.currentSongId) return;
        try {
            if (!this.favPlaylistId) {
                const favRes = await fetch("/api/playlists/favorites", { method: "POST" });
                if (!favRes.ok) throw new Error("Không thể tạo playlist Bài hát yêu thích");
                this.favPlaylistId = await favRes.json();
            }
            await this._toggleSongInPlaylist(this.favPlaylistId, "Bài hát yêu thích", btnEl, isAdded);
        } catch (err) {
            console.error(err);
            if (typeof Toast !== "undefined" && Toast.show) {
                Toast.show("Không thể cập nhật Bài hát yêu thích.", "error");
            }
        }
    },
    async togglePlaylist(playlistId, playlistName, btnEl, isAdded) {
        if (!this.currentSongId) return;
        await this._toggleSongInPlaylist(playlistId, playlistName, btnEl, isAdded);
    },
    async _toggleSongInPlaylist(playlistId, playlistName, btnEl, isAdded) {
        if (!this.currentSongId) return;
        const songIds = Array.isArray(this.currentSongId) ? this.currentSongId : [this.currentSongId];
        if (songIds.length === 0) return;
        try {
            const results = await Promise.all(songIds.map(songId =>
                fetch(`/api/playlists/${playlistId}/songs/${songId}`, {
                    method: isAdded ? "DELETE" : "POST"
                })
            ));
            const limitedRes = results.find(r => r.status === 403);
            if (limitedRes) {
                const data = await limitedRes.json().catch(() => ({}));
                if (data.limitReached) {
                    if (typeof Toast !== "undefined" && Toast.error) Toast.error(data.message);
                    else if (typeof Toast !== "undefined" && Toast.show) Toast.show(data.message, "error");
                    return;
                }
            }
            const failed = results.filter(r => !r.ok).length;
            if (failed === results.length) {
                throw new Error(isAdded ? "Xóa bài hát thất bại" : "Thêm bài hát thất bại");
            }
            if (btnEl) btnEl.classList.toggle("pl-picker-added", !isAdded);
            if (typeof Toast !== "undefined" && Toast.show) {
                const label = songIds.length > 1 ? `${songIds.length} bài hát` : "bài hát";
                Toast.show(isAdded
                    ? `Đã xóa ${label} khỏi "${playlistName}".`
                    : `Đã thêm ${label} vào "${playlistName}".`);
            }
            await Playlist.loadSidebarLibrary();
        } catch (err) {
            console.error(err);
            if (typeof Toast !== "undefined" && Toast.show) {
                Toast.show("Không thể cập nhật playlist.", "error");
            }
        }
    }
};
window.PlaylistPicker = PlaylistPicker;
const LikedSongs = {
    favPlaylistId: null,
    likedIdsCache: null,
    async _peekFavPlaylistId() {
        if (this.favPlaylistId) return this.favPlaylistId;
        try {
            const res = await fetch("/api/playlists/favorites");
            if (!res.ok) return null;
            const id = await res.json();
            if (id) this.favPlaylistId = id;
            return id || null;
        } catch (err) {
            console.error(err);
            return null;
        }
    },
    async _ensureFavPlaylistId() {
        if (this.favPlaylistId) return this.favPlaylistId;
        const res = await fetch("/api/playlists/favorites", { method: "POST" });
        if (!res.ok) throw new Error("Không thể tạo playlist Bài hát yêu thích");
        this.favPlaylistId = await res.json();
        return this.favPlaylistId;
    },
    async loadLikedIds() {
        try {
            const favPlaylistId = await this._peekFavPlaylistId();
            if (!favPlaylistId) {
                this.likedIdsCache = new Set();
                return this.likedIdsCache;
            }
            const res = await fetch(`/api/playlists/${favPlaylistId}`);
            const data = res.ok ? await res.json() : null;
            const songs = data && data.songs ? data.songs : [];
            this.likedIdsCache = new Set(songs.map(s => s.songID));
        } catch (err) {
            console.error(err);
            this.likedIdsCache = new Set();
        }
        return this.likedIdsCache;
    },
    isLikedCached(songId) {
        return !!(this.likedIdsCache && this.likedIdsCache.has(songId));
    },
    async isLiked(songId) {
        if (this.likedIdsCache) return this.isLikedCached(songId);
        try {
            const favPlaylistId = await this._peekFavPlaylistId();
            if (!favPlaylistId) return false;
            const res = await fetch(`/api/playlists/containing/${songId}`);
            const containingIds = res.ok ? await res.json() : [];
            return containingIds.includes(favPlaylistId);
        } catch (err) {
            console.error(err);
            return false;
        }
    },
    async toggle(songId) {
        const wasLiked = await this.isLiked(songId);
        const favPlaylistId = wasLiked
            ? await this._peekFavPlaylistId()
            : await this._ensureFavPlaylistId();
        const toggleRes = await fetch(`/api/playlists/${favPlaylistId}/songs/${songId}`, {
            method: wasLiked ? "DELETE" : "POST"
        });
        if (!toggleRes.ok) throw new Error("Không thể cập nhật Bài hát đã thích");
        if (this.likedIdsCache) {
            if (wasLiked) this.likedIdsCache.delete(songId);
            else this.likedIdsCache.add(songId);
        }
        if (typeof Playlist !== "undefined" && Playlist.loadSidebarLibrary) {
            Playlist.loadSidebarLibrary();
        }
        return !wasLiked;
    }
};
window.LikedSongs = LikedSongs;
function formatDuration(seconds) {
    seconds = seconds || 0;
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m}:${s.toString().padStart(2, "0")}`;
}
function escapeHtml(str) {
    if (!str) return "";
    return str
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;");
}

if (!window.__playlistPlayerChangeBound && window.Player && typeof Player.onChange === "function") {
    window.__playlistPlayerChangeBound = true;
    Player.onChange(() => Playlist.updatePlaybackUI());
}