const API = "";

function authHeaders() {
    const token = localStorage.getItem("token");
    return token ? { "Authorization": `Bearer ${token}` } : {};
}

function handleAuthError(res) {
    if (res.status === 401) {
        localStorage.removeItem("token");
        localStorage.removeItem("nombre");
        window.location.href = "/login.html";
        return true;
    }
    return false;
}

const Notificacion = {
    mostrar(mensaje, tipo) {
        const div = document.getElementById("notificacion");
        const span = document.getElementById("notificacion-mensaje");
        span.textContent = mensaje;
        div.className = `notificacion ${tipo} visible`;
        setTimeout(() => {
            div.className = "notificacion";
        }, 3000);
    }
};
