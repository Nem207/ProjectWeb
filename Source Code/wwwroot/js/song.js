const Song = (() => {
    let _list = [];
    function init() {
        if (window.songPageList && Array.isArray(window.songPageList)) {
            _list = window.songPageList;
        }
        _setupSearchDebounce();
        _setupCustomDropdowns();
    }
    function _setupCustomDropdowns() {
        _setupOneDropdown("artistFilterWrap", "artistFilterBtn", "artistFilterList", "artistFilterValue", "artistFilterLabel");
        _setupOneDropdown("albumFilterWrap", "albumFilterBtn", "albumFilterList", "albumFilterValue", "albumFilterLabel");
        document.addEventListener("click", () => {
            document.querySelectorAll(".pl-dropdown.open").forEach(el => el.classList.remove("open"));
        });
    }
    function _setupOneDropdown(wrapId, btnId, listId, valueId, labelId) {
        const wrap = document.getElementById(wrapId);
        const btn = document.getElementById(btnId);
        const list = document.getElementById(listId);
        const valueInput = document.getElementById(valueId);
        const label = document.getElementById(labelId);
        if (!wrap || !btn || !list || !valueInput || !label) return;
        btn.addEventListener("click", (e) => {
            e.stopPropagation();
            const wasOpen = wrap.classList.contains("open");
            document.querySelectorAll(".pl-dropdown.open").forEach(el => el.classList.remove("open"));
            if (!wasOpen) wrap.classList.add("open");
        });
        list.addEventListener("click", (e) => {
            e.stopPropagation();
            const option = e.target.closest(".pl-dropdown-option");
            if (!option) return;
            valueInput.value = option.dataset.value || "";
            label.textContent = option.textContent;
            wrap.classList.remove("open");
            document.getElementById("songFilterForm").submit();
        });
    }
    function _setupSearchDebounce() {
        const input = document.getElementById("songSearch");
        if (!input) return;
        let timer = null;
        input.addEventListener("input", () => {
            clearTimeout(timer);
            timer = setTimeout(() => {
                document.getElementById("songFilterForm").submit();
            }, 400);
        });
    }
    function playAt(index) {
        if (!_list || _list.length === 0) return;
        if (index < 0 || index >= _list.length) return;
        if (typeof Player === "undefined" || typeof Player.playFromList !== "function") {
            console.error("song.js: Player.playFromList chưa sẵn sàng.");
            return;
        }
        Player.playFromList(_list, index);
    }
    function playAll() {
        playAt(0);
    }
    function addToQueue(index) {
        if (!_list || index < 0 || index >= _list.length) return;
        if (typeof Player === "undefined" || typeof Player.addToQueue !== "function") {
            console.error("song.js: Player.addToQueue chưa sẵn sàng.");
            return;
        }
        Player.addToQueue(_list[index]);
    }
    document.addEventListener("DOMContentLoaded", init);
    return { playAt, playAll, addToQueue };
})();
