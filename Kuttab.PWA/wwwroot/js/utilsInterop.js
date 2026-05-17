// Utility JS interop functions for Blazor WASM
window.UtilsInterop = {
    copyToClipboard: async function (text) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch (e) {
            // Fallback
            var textarea = document.createElement('textarea');
            textarea.value = text;
            document.body.appendChild(textarea);
            textarea.select();
            document.execCommand('copy');
            document.body.removeChild(textarea);
            return true;
        }
    },

    share: async function (title, text, url) {
        if (navigator.share) {
            try {
                await navigator.share({ title: title, text: text, url: url || window.location.href });
                return true;
            } catch (e) {
                if (e.name !== 'AbortError') {
                    // Fall through to clipboard copy
                } else {
                    return false;
                }
            }
        }
        // Fallback: copy to clipboard and show a toast
        try {
            var shareText = (title ? title + '\n\n' : '') + (text || '') + (url ? '\n' + url : '');
            await navigator.clipboard.writeText(shareText);
            window.UtilsInterop._showToast('📋 Copied to clipboard');
            return true;
        } catch (e2) {
            // Legacy fallback
            try {
                var textarea = document.createElement('textarea');
                textarea.value = (title ? title + '\n\n' : '') + (text || '');
                textarea.style.position = 'fixed';
                textarea.style.opacity = '0';
                document.body.appendChild(textarea);
                textarea.select();
                document.execCommand('copy');
                document.body.removeChild(textarea);
                window.UtilsInterop._showToast('📋 Copied to clipboard');
                return true;
            } catch (e3) {
                return false;
            }
        }
    },

    _showToast: function (message) {
        var existing = document.getElementById('kuttab-toast');
        if (existing) existing.remove();
        var toast = document.createElement('div');
        toast.id = 'kuttab-toast';
        toast.textContent = message;
        toast.style.cssText = 'position:fixed;bottom:5rem;left:50%;transform:translateX(-50%);background:rgba(27,94,32,0.92);color:#fff;padding:0.5rem 1.2rem;border-radius:20px;font-size:0.9rem;z-index:9999;pointer-events:none;transition:opacity 0.4s;';
        document.body.appendChild(toast);
        setTimeout(function () { toast.style.opacity = '0'; setTimeout(function () { toast.remove(); }, 400); }, 2500);
    },

    vibrate: function (ms) {
        if (navigator.vibrate) {
            navigator.vibrate(ms);
        }
    },

    setDirection: function (dir) {
        document.documentElement.setAttribute('dir', dir);
        document.documentElement.setAttribute('lang', dir === 'rtl' ? 'ar' : 'en');
    },

    scrollToTop: function (elementId) {
        var el = document.getElementById(elementId);
        if (el) el.scrollTop = 0;
    }
};
