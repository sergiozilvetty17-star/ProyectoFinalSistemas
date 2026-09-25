(function () {
    "use strict";

    class ExamSecureSpeech {
        constructor(button) {
            this.button = button;
            this.targetId = button.dataset.speechTarget;

            this.status = document.querySelector(
                `[data-speech-status-for="${this.targetId}"]`
            );

            this.recognition = null;
            this.isListening = false;

            this.initialize();
        }

        initialize() {
            const SpeechRecognition =
                window.SpeechRecognition ||
                window.webkitSpeechRecognition;

            if (!SpeechRecognition) {
                this.setStatus(
                    "Este navegador no admite reconocimiento de voz.",
                    true
                );

                this.button.disabled = true;
                return;
            }

            this.recognition = new SpeechRecognition();

            /*
             * es-ES es más compatible con SpeechRecognition
             * que es-BO en navegadores basados en Chromium.
             */
            this.recognition.lang = "es-ES";

            this.recognition.continuous = false;
            this.recognition.interimResults = true;
            this.recognition.maxAlternatives = 1;

            this.recognition.addEventListener(
                "start",
                () => {
                    this.isListening = true;

                    this.button.classList.add(
                        "is-recording"
                    );

                    this.setButtonText("Detener");

                    this.setStatus(
                        "Escuchando... habla con claridad."
                    );
                }
            );

            this.recognition.addEventListener(
                "result",
                event => {
                    let finalText = "";
                    let interimText = "";

                    for (
                        let i = event.resultIndex;
                        i < event.results.length;
                        i++
                    ) {
                        const result =
                            event.results[i];

                        if (!result || !result[0]) {
                            continue;
                        }

                        const transcript =
                            result[0].transcript;

                        if (result.isFinal) {
                            finalText += transcript;
                        } else {
                            interimText += transcript;
                        }
                    }

                    if (interimText.trim()) {
                        this.setStatus(
                            "Reconociendo: " +
                            interimText.trim()
                        );
                    }

                    if (finalText.trim()) {
                        this.insertText(
                            finalText.trim()
                        );
                    }
                }
            );

            this.recognition.addEventListener(
                "nomatch",
                () => {
                    this.setStatus(
                        "No se pudo identificar lo que dijiste.",
                        true
                    );
                }
            );

            this.recognition.addEventListener(
                "error",
                event => {
                    console.error(
                        "SpeechRecognition error:",
                        event.error,
                        event
                    );

                    let message =
                        "No fue posible reconocer la voz.";

                    switch (event.error) {

                        case "not-allowed":
                        case "service-not-allowed":
                            message =
                                "El navegador no permitió utilizar el micrófono.";
                            break;

                        case "no-speech":
                            message =
                                "No se detectó voz. Intenta hablar nuevamente.";
                            break;

                        case "audio-capture":
                            message =
                                "No se encontró un micrófono disponible.";
                            break;

                        case "network":
                            message =
                                "El servicio de reconocimiento de voz requiere conexión a Internet.";
                            break;

                        case "language-not-supported":
                            message =
                                "El idioma español no está disponible en este navegador.";
                            break;

                        case "aborted":
                            message =
                                "Reconocimiento detenido.";
                            break;
                    }

                    this.setStatus(
                        message,
                        true
                    );
                }
            );

            this.recognition.addEventListener(
                "end",
                () => {
                    this.isListening = false;

                    this.button.classList.remove(
                        "is-recording"
                    );

                    this.setButtonText("Dictar");

                    console.log(
                        "SpeechRecognition finalizado."
                    );
                }
            );
        }

        toggleRecording() {
            if (!this.recognition) {
                return;
            }

            if (this.isListening) {
                this.stop();
            } else {
                this.start();
            }
        }

        start() {
            try {
                this.setStatus(
                    "Iniciando reconocimiento..."
                );

                this.recognition.start();

            } catch (error) {
                console.error(
                    "Error iniciando SpeechRecognition:",
                    error
                );

                this.setStatus(
                    "No se pudo iniciar el reconocimiento de voz.",
                    true
                );
            }
        }

        stop() {
            if (!this.recognition) {
                return;
            }

            try {
                this.recognition.stop();

                this.setStatus(
                    "Procesando voz..."
                );

            } catch (error) {
                console.error(
                    "Error deteniendo SpeechRecognition:",
                    error
                );
            }
        }

        insertText(text) {
            const target =
                document.getElementById(
                    this.targetId
                );

            if (!target) {
                this.setStatus(
                    "No se encontró el campo de destino.",
                    true
                );
                return;
            }

            const currentValue =
                target.value.trim();

            if (currentValue) {
                target.value =
                    currentValue + " " + text;
            } else {
                target.value = text;
            }

            target.dispatchEvent(
                new Event("input", {
                    bubbles: true
                })
            );

            target.dispatchEvent(
                new Event("change", {
                    bubbles: true
                })
            );

            this.setStatus(
                "Texto agregado correctamente."
            );
        }

        setButtonText(text) {
            const element =
                this.button.querySelector(
                    ".speech-button-text"
                );

            if (element) {
                element.textContent = text;
            }
        }

        setStatus(message, error = false) {
            if (!this.status) {
                return;
            }

            this.status.textContent = message;

            this.status.classList.toggle(
                "speech-status-error",
                error
            );
        }
    }

    document.addEventListener(
        "DOMContentLoaded",
        () => {
            document
                .querySelectorAll(
                    "[data-speech-target]"
                )
                .forEach(button => {

                    const speech =
                        new ExamSecureSpeech(button);

                    button.addEventListener(
                        "click",
                        () => {
                            speech.toggleRecording();
                        }
                    );
                });
        }
    );
})();
