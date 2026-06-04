// PWA Installation Interop
window.PWAInterop = {
    deferredPrompt: null,
    _isInstallable: true, // Always true for manual install

    init: function() {
        // Listen for beforeinstallprompt event (Chrome/Edge)
        window.addEventListener('beforeinstallprompt', (e) => {
            e.preventDefault();
            this.deferredPrompt = e;
            console.log('PWA installable via prompt');
        });

        // Listen for app installed event
        window.addEventListener('appinstalled', () => {
            this.deferredPrompt = null;
            this._isInstallable = false;
            console.log('PWA installed');
        });

        // Check if already installed
        if (window.matchMedia('(display-mode: standalone)').matches || window.navigator.standalone === true) {
            this._isInstallable = false;
        }
    },

    isInstallable: function() {
        return this._isInstallable === true && !this.isStandalone();
    },

    install: async function() {
        if (this.deferredPrompt) {
            // Chrome/Edge: use the prompt
            this.deferredPrompt.prompt();
            const { outcome } = await this.deferredPrompt.userChoice;
            console.log(`User response: ${outcome}`);
            this.deferredPrompt = null;
            this._isInstallable = false;
            return outcome === 'accepted';
        } else {
            // Firefox/iOS: show manual instructions
            const browser = this.getBrowserType();
            let instructions = '';
            
            if (browser === 'firefox') {
                instructions = 'To install: Click the menu (≡) > "Install this site as an app"';
            } else if (browser === 'safari') {
                instructions = 'To install: Tap Share (⎙) > "Add to Home Screen"';
            } else {
                instructions = 'To install: Use your browser menu to add to home screen';
            }
            
            alert(instructions);
            return false;
        }
    },

    // Check if running as PWA
    isStandalone: function() {
        return window.matchMedia('(display-mode: standalone)').matches ||
               window.navigator.standalone === true;
    },

    // Get browser type for showing specific instructions
    getBrowserType: function() {
        const userAgent = navigator.userAgent.toLowerCase();
        if (userAgent.includes('firefox')) return 'firefox';
        if (userAgent.includes('safari') && !userAgent.includes('chrome')) return 'safari';
        if (userAgent.includes('chrome')) return 'chrome';
        if (userAgent.includes('edge')) return 'edge';
        return 'unknown';
    }
};

// Initialize on load
window.PWAInterop.init();
