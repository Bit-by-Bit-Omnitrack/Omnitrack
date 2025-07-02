document.addEventListener("DOMContentLoaded", function () {
    const toggles = document.querySelectorAll(".password-toggle");

    toggles.forEach(toggle => {
        toggle.addEventListener("click", function () {
            const input = document.getElementById(this.dataset.input);
            const icon = this.querySelector("i");

            if (input.type === "password") {
                input.type = "text";
                icon.classList.remove("bi-eye-slash");
                icon.classList.add("bi-eye");
            } else {
                input.type = "password";
                icon.classList.remove("bi-eye");
                icon.classList.add("bi-eye-slash");
            }
        });
    });
});
