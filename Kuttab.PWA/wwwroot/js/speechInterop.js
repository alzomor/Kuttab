// Web Speech API interop for Blazor WASM
window.SpeechInterop = {
    _recognition: null,
    _dotNetRef: null,
    _shouldRestart: false,

    isAvailable: function () {
        return 'webkitSpeechRecognition' in window || 'SpeechRecognition' in window;
    },

    init: function (dotNetRef, languageCode) {
        this._dotNetRef = dotNetRef;
        var SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) return false;

        this._recognition = new SpeechRecognition();
        this._recognition.lang = languageCode || 'ar';
        this._recognition.continuous = true;
        this._recognition.interimResults = true;
        this._recognition.maxAlternatives = 3;

        this._recognition.onresult = (event) => {
            var results = [];
            for (var i = event.resultIndex; i < event.results.length; i++) {
                for (var j = 0; j < event.results[i].length; j++) {
                    results.push(event.results[i][j].transcript);
                }
                if (event.results[i].isFinal && this._dotNetRef) {
                    this._dotNetRef.invokeMethodAsync('OnFinalResult', results);
                } else if (this._dotNetRef) {
                    this._dotNetRef.invokeMethodAsync('OnPartialResult', results);
                }
            }
        };

        this._recognition.onerror = (event) => {
            if (this._dotNetRef) {
                this._dotNetRef.invokeMethodAsync('OnSpeechError', event.error);
            }
        };

        this._recognition.onend = () => {
            if (this._shouldRestart && this._recognition) {
                try { this._recognition.start(); } catch (e) { }
            } else if (this._dotNetRef) {
                this._dotNetRef.invokeMethodAsync('OnListeningStopped');
            }
        };

        this._recognition.onstart = () => {
            if (this._dotNetRef) {
                this._dotNetRef.invokeMethodAsync('OnListeningStarted');
            }
        };

        return true;
    },

    start: function () {
        if (!this._recognition) return false;
        this._shouldRestart = true;
        try {
            this._recognition.start();
            return true;
        } catch (e) {
            return false;
        }
    },

    stop: function () {
        this._shouldRestart = false;
        if (this._recognition) {
            try { this._recognition.stop(); } catch (e) { }
        }
    },

    dispose: function () {
        this.stop();
        this._recognition = null;
        this._dotNetRef = null;
    }
};
