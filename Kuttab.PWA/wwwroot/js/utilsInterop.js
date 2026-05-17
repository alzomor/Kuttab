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
                await navigator.share({ title: title, text: text, url: url });
                return true;
            } catch (e) {
                return false;
            }
        }
        return false;
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
