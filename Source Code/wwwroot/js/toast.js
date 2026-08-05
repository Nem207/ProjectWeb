(function () {
    'use strict';
    var ICONS = {
        success: '<svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor"><path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-1.5 14.5l-4-4 1.41-1.41L10.5 13.67l6.09-6.09L18 9l-7.5 7.5z"/></svg>',
        error: '<svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor"><path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z"/></svg>',
        info: '<svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor"><path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z"/></svg>'
    };
    function getContainer() {
        var el = document.getElementById('toastContainer');
        if (!el) {
            el = document.createElement('div');
            el.id = 'toastContainer';
            el.className = 'toast-container';
            document.body.appendChild(el);
        }
        return el;
    }
    function show(message, type, duration) {
        type = type || 'success';
        duration = duration || 3000;
        var container = getContainer();
        var toast = document.createElement('div');
        toast.className = 'toast toast-' + type;
        toast.innerHTML = '<span class="toast-icon">' + (ICONS[type] || ICONS.info) + '</span><span class="toast-msg"></span>';
        toast.querySelector('.toast-msg').textContent = message;
        container.appendChild(toast);
        requestAnimationFrame(function () {
            toast.classList.add('show');
        });
        setTimeout(function () {
            toast.classList.remove('show');
            toast.classList.add('hide');
            setTimeout(function () {
                toast.remove();
            }, 250);
        }, duration);
    }
    window.Toast = {
        show: show,
        success: function (msg, duration) { show(msg, 'success', duration); },
        error: function (msg, duration) { show(msg, 'error', duration); },
        info: function (msg, duration) { show(msg, 'info', duration); }
    };
})();