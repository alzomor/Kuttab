// Audio playback interop for Blazor WASM
window.AudioInterop = {
    _audio: null,
    _dotNetRef: null,
    _isRepeating: false,

    init: function (dotNetRef) {
        this._dotNetRef = dotNetRef;
    },

    play: function (url) {
        this.stop();
        this._audio = new Audio(url);
        this._audio.onended = () => {
            if (this._isRepeating) {
                this._audio.currentTime = 0;
                this._audio.play();
            } else if (this._dotNetRef) {
                this._dotNetRef.invokeMethodAsync('OnPlaybackEnded');
            }
        };
        this._audio.onerror = (e) => {
            if (this._dotNetRef) {
                this._dotNetRef.invokeMethodAsync('OnPlaybackError', 'Audio playback error');
            }
        };
        return this._audio.play().then(() => true).catch(() => false);
    },

    stop: function () {
        if (this._audio) {
            this._audio.pause();
            this._audio.currentTime = 0;
            this._audio = null;
        }
        this._isRepeating = false;
    },

    pause: function () {
        if (this._audio) {
            this._audio.pause();
        }
    },

    resume: function () {
        if (this._audio) {
            return this._audio.play().then(() => true).catch(() => false);
        }
        return Promise.resolve(false);
    },

    setRepeat: function (repeat) {
        this._isRepeating = repeat;
    },

    isPlaying: function () {
        return this._audio && !this._audio.paused;
    },

    dispose: function () {
        this.stop();
        this._dotNetRef = null;
    }
};
