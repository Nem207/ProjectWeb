const SongDetail = (function () {
    let songId = null;

    function getContextList() {
        return [window.CURRENT_SONG, ...(window.RELATED_SONGS || [])];
    }

    function playCurrent() {
        if (typeof Player === "undefined" || typeof Player.playFromList !== "function") {
            console.error("song-detail.js: Player chưa sẵn sàng.");
            return;
        }
        const current = Player.getCurrent();
        if (current && current.songId === songId) {
            Player.togglePlay();
        } else {
            Player.playFromList(getContextList(), 0);
        }
    }

    function playQuick(index) {
        if (typeof Player === "undefined" || typeof Player.playFromList !== "function") return;
        const list = getContextList();
        Player.playFromList(list, index + 1);
        Toast.success(`Đang phát: ${list[index + 1].title}`);
    }

    function setPlayingUI(isPlaying) {
        const btn = document.getElementById("btnSongPlay");
        if (!btn) return;
        btn.classList.toggle("is-playing", isPlaying);
        btn.innerHTML = isPlaying
            ? '<svg viewBox="0 0 24 24" width="22" height="22" fill="currentColor"><path d="M6 6h4v12H6zm8 0h4v12h-4z"/></svg>'
            : '<svg viewBox="0 0 24 24" width="22" height="22" fill="currentColor"><path d="M8 5v14l11-7z"/></svg>';
    }

    function syncPlayingUI() {
        if (typeof Player === "undefined") return;
        const current = Player.getCurrent();
        const playing = !!current && current.songId === songId && Player.isPlaying();
        setPlayingUI(playing);
    }

    // ── Heart button ──────────────────────────────────────────────────────────

    function getHeartBtn() {
        return document.getElementById("btnSongDetailLike");
    }

    function setHeartUI(liked) {
        const btn = getHeartBtn();
        if (!btn) return;
        btn.classList.toggle("liked", liked);
        btn.title = liked ? "Xóa khỏi Bài hát đã thích" : "Thêm vào Bài hát đã thích";
    }

    // Đọc trạng thái liked từ LikedSongs cache (hoặc fetch nếu cần)
    // và cập nhật UI heart + player bar heart + row-add-btn cùng lúc
    async function syncHeartUI() {
        if (!window.IS_AUTHENTICATED || !window.LikedSongs) {
            setHeartUI(false);
            return;
        }
        try {
            const liked = await LikedSongs.isLiked(songId);
            setHeartUI(liked);
        } catch (e) {
            console.error("song-detail: Không load được trạng thái liked.", e);
        }
    }

    // Khi Player.onChange kích hoạt (bài hát thay đổi hoặc like từ thanh play)
    // → đồng bộ lại heart UI theo cache hiện tại của LikedSongs
    function onPlayerChange() {
        syncPlayingUI();
        // Chỉ sync heart nếu LikedSongs đã có cache (tránh gọi API thừa)
        if (window.IS_AUTHENTICATED && window.LikedSongs && LikedSongs.likedIdsCache) {
            setHeartUI(LikedSongs.likedIdsCache.has(songId));
        }
    }

    async function handleHeartClick() {
        if (!window.IS_AUTHENTICATED) {
            Toast.info("Vui lòng đăng nhập để thích bài hát.");
            return;
        }
        if (!window.LikedSongs) return;
        const btn = getHeartBtn();
        if (btn) btn.disabled = true; // chống double-click
        try {
            if (typeof Player !== "undefined" && typeof Player.isSongInAnyPlaylist === "function") {
                const inAny = await Player.isSongInAnyPlaylist(songId);
                if (inAny) {
                    if (window.PlaylistPicker) window.PlaylistPicker.open(songId);
                    return;
                }
            }
            const nowLiked = await LikedSongs.toggle(songId);
            setHeartUI(nowLiked);
            // Đồng bộ: thanh play + row-add-btn ở mọi trang
            if (window.refreshSongAddButtons) await window.refreshSongAddButtons(songId);
            if (nowLiked) {
                Toast.show("Đã thêm vào Bài hát đã thích", "success", 4000, {
                    actionText: "Thay đổi",
                    onAction: () => { if (window.PlaylistPicker) window.PlaylistPicker.open(songId); }
                });
            } else {
                Toast.show("Đã xoá khỏi Bài hát đã thích");
            }
        } catch (err) {
            console.error("song-detail: Không thể toggle liked.", err);
            Toast.error("Không thể cập nhật Bài hát đã thích.");
        } finally {
            if (btn) btn.disabled = false;
        }
    }

    // ── Bind ─────────────────────────────────────────────────────────────────

    function bindPlay() {
        const playBtn = document.getElementById("btnSongPlay");
        if (playBtn) playBtn.addEventListener("click", playCurrent);
    }

    function bindLibraryButton() {
        const btn = document.getElementById("btnAddLibrarySong");
        if (!btn) return;
        btn.addEventListener("click", function () {
            if (window.handleSongAddButtonClick) {
                window.handleSongAddButtonClick(songId);
            } else if (typeof PlaylistPicker !== "undefined" && typeof PlaylistPicker.open === "function") {
                PlaylistPicker.open(songId);
            }
        });
    }

    function bindHeartButton() {
        const btn = getHeartBtn();
        if (!btn) return;
        btn.addEventListener("click", handleHeartClick);
    }

    function bindMoreMenu() {
        const moreBtn = document.getElementById("btnSongMore");
        const menu = document.getElementById("songMoreMenu");
        if (!moreBtn || !menu) return;
        moreBtn.addEventListener("click", function (e) {
            e.stopPropagation();
            menu.classList.toggle("open");
        });
        const queueBtn = document.getElementById("menuAddQueueSong");
        if (queueBtn) {
            queueBtn.addEventListener("click", function () {
                if (typeof Player !== "undefined" && typeof Player.addToQueue === "function") {
                    Player.addToQueue(window.CURRENT_SONG);
                }
                menu.classList.remove("open");
            });
        }
        document.addEventListener("click", function () {
            menu.classList.remove("open");
        });
    }

    // ── Init ─────────────────────────────────────────────────────────────────

    function init() {
        const root = document.getElementById("viewSongDetail");
        if (!root || !window.CURRENT_SONG) return;
        songId = Number(root.dataset.songId);

        bindPlay();
        bindLibraryButton();
        bindHeartButton();
        bindMoreMenu();

        syncPlayingUI();
        syncHeartUI();

        if (typeof Player !== "undefined" && typeof Player.onChange === "function") {
            Player.onChange(onPlayerChange);
        }

        // Đồng bộ nút + (dấu cộng/tích) theo cache mới nhất
        if (window.IS_AUTHENTICATED && window.refreshSongAddButtons) {
            window.refreshSongAddButtons(songId);
        }
    }

    document.addEventListener("DOMContentLoaded", init);
    return { playQuick };
})();