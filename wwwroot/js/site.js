window.showToast = function (message, type) {
    if (!message) {
        return;
    }

    var container = document.getElementById("app-toasts");
    if (!container || typeof bootstrap === "undefined") {
        return;
    }

    var variant = type === "error" || type === "success" || type === "warning" ? type : "info";
    var toast = document.createElement("div");
    toast.className = "toast align-items-center border-0 toast-brand toast-brand--" + variant;
    toast.setAttribute("role", "alert");
    toast.setAttribute("aria-live", "assertive");
    toast.setAttribute("aria-atomic", "true");
    toast.innerHTML =
        '<div class="d-flex">' +
            '<div class="toast-body"></div>' +
            '<button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast" aria-label="Cerrar"></button>' +
        "</div>";
    toast.querySelector(".toast-body").textContent = message;
    container.appendChild(toast);

    var instance = bootstrap.Toast.getOrCreateInstance(toast, { delay: 5000 });
    toast.addEventListener("hidden.bs.toast", function () {
        toast.remove();
    });
    instance.show();
};

(function () {
    var container = document.getElementById("app-toasts");
    if (!container) {
        return;
    }

    var message = container.getAttribute("data-initial-message");
    if (message) {
        window.showToast(message, container.getAttribute("data-initial-type"));
    }
})();
