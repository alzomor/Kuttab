// MediaRecorder API interop for Blazor WASM
window.RecordingInterop = {
    _mediaRecorder: null,
    _audioChunks: [],
    _dotNetRef: null,
    _recordedBlobUrl: null,

    init: function (dotNetRef) {
        this._dotNetRef = dotNetRef;
    },

    isAvailable: function () {
        return navigator.mediaDevices && navigator.mediaDevices.getUserMedia && window.MediaRecorder;
    },

    startRecording: async function () {
        try {
            var stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            this._audioChunks = [];
            this._mediaRecorder = new MediaRecorder(stream);

            this._mediaRecorder.ondataavailable = (event) => {
                if (event.data.size > 0) {
                    this._audioChunks.push(event.data);
                }
            };

            this._mediaRecorder.onstop = () => {
                var blob = new Blob(this._audioChunks, { type: 'audio/webm' });
                if (this._recordedBlobUrl) {
                    URL.revokeObjectURL(this._recordedBlobUrl);
                }
                this._recordedBlobUrl = URL.createObjectURL(blob);
                stream.getTracks().forEach(t => t.stop());
                if (this._dotNetRef) {
                    this._dotNetRef.invokeMethodAsync('OnRecordingStopped', this._recordedBlobUrl);
                }
            };

            this._mediaRecorder.start();
            if (this._dotNetRef) {
                this._dotNetRef.invokeMethodAsync('OnRecordingStarted');
            }
            return true;
        } catch (e) {
            if (this._dotNetRef) {
                this._dotNetRef.invokeMethodAsync('OnRecordingError', e.message);
            }
            return false;
        }
    },

    stopRecording: function () {
        if (this._mediaRecorder && this._mediaRecorder.state === 'recording') {
            this._mediaRecorder.stop();
        }
    },

    getRecordedUrl: function () {
        return this._recordedBlobUrl;
    },

    deleteRecording: function () {
        if (this._recordedBlobUrl) {
            URL.revokeObjectURL(this._recordedBlobUrl);
            this._recordedBlobUrl = null;
        }
        this._audioChunks = [];
    },

    // Aliases for Blazor interop calls
    start: async function () {
        return await this.startRecording();
    },

    stop: async function () {
        this.stopRecording();
        // Wait briefly for onstop to fire and set blob URL
        await new Promise(r => setTimeout(r, 200));
        return this._recordedBlobUrl;
    },

    dispose: function () {
        this.stopRecording();
        this.deleteRecording();
        this._dotNetRef = null;
    }
};
