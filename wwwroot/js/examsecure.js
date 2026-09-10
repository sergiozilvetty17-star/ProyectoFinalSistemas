document.addEventListener("DOMContentLoaded", () => {

    /* =========================
       PAGE REVEAL
    ========================= */

    document.querySelectorAll(".reveal").forEach((element, index) => {

        if (!element.classList.contains("reveal-delay-1") &&
            !element.classList.contains("reveal-delay-2") &&
            !element.classList.contains("reveal-delay-3") &&
            !element.classList.contains("reveal-delay-4")) {

            element.style.animationDelay = `${index * 0.06}s`;
        }
    });


    /* =========================
       RIPPLE BUTTONS
    ========================= */

    document.querySelectorAll(".examsecure-btn").forEach(button => {

        button.addEventListener("click", function (event) {

            const rect = this.getBoundingClientRect();

            const ripple = document.createElement("span");

            const size = Math.max(
                rect.width,
                rect.height
            );

            ripple.className = "examsecure-ripple";

            ripple.style.width = `${size}px`;
            ripple.style.height = `${size}px`;

            ripple.style.left =
                `${event.clientX - rect.left - size / 2}px`;

            ripple.style.top =
                `${event.clientY - rect.top - size / 2}px`;

            this.appendChild(ripple);

            setTimeout(() => {
                ripple.remove();
            }, 600);
        });
    });


    /* =========================
       PASSWORD TOGGLE
    ========================= */

    document.querySelectorAll("[data-password-toggle]")
        .forEach(button => {

            button.addEventListener("click", () => {

                const targetId =
                    button.getAttribute("data-password-toggle");

                const input =
                    document.getElementById(targetId);

                if (!input)
                    return;

                if (input.type === "password") {

                    input.type = "text";

                    button.innerHTML = "Ocultar";

                } else {

                    input.type = "password";

                    button.innerHTML = "Mostrar";
                }
            });
        });


    /* =========================
       FORM LOADING
    ========================= */

    document.querySelectorAll("form[data-loading]")
        .forEach(form => {

            form.addEventListener("submit", () => {

                const button =
                    form.querySelector(
                        'button[type="submit"]'
                    );

                if (!button)
                    return;

                if (button.dataset.submitting === "true")
                    return;

                button.dataset.submitting = "true";

                button.dataset.originalText =
                    button.innerHTML;

                button.innerHTML = `
                    <span class="spinner-border spinner-border-sm me-2"
                          aria-hidden="true"></span>
                    Procesando...
                `;

                button.disabled = true;
            });
        });


    /* =========================
       AUTO HIDE TOASTS
    ========================= */

    document.querySelectorAll(".examsecure-toast")
        .forEach(toast => {

            setTimeout(() => {

                toast.style.transition =
                    "opacity .35s ease, transform .35s ease";

                toast.style.opacity = "0";
                toast.style.transform =
                    "translateY(15px)";

                setTimeout(() => {
                    toast.remove();
                }, 350);

            }, 4500);
        });


    /* =========================
       NAVBAR SCROLL
    ========================= */

    const navbar =
        document.querySelector(".examsecure-navbar");

    if (navbar) {

        window.addEventListener("scroll", () => {

            if (window.scrollY > 15) {

                navbar.style.background =
                    "rgba(7, 11, 20, .90)";

            } else {

                navbar.style.background =
                    "rgba(7, 11, 20, .72)";
            }
        });
    }

});
