const Pacientes = {
    paginaActual: 1,
    busqueda: "",

    async cargar() {
        try {
            const params = new URLSearchParams({
                pagina: this.paginaActual,
                por_pagina: 10,
                busqueda: this.busqueda
            });
            const res = await fetch(`${API}/pacientes?${params}`, { headers: authHeaders() });
            if (handleAuthError(res)) return;
            const data = await res.json();
            this.renderizarTabla(data.pacientes);
            this.renderizarPaginacion(data.total_paginas);
        } catch (error) {
            Notificacion.mostrar("Error al cargar pacientes", "error");
        }
    },

    renderizarTabla(pacientes) {
        const tbody = document.querySelector("#tabla-pacientes tbody");
        if (pacientes.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:#6b7280;">No se encontraron pacientes</td></tr>';
            return;
        }
        tbody.innerHTML = pacientes.map(p => `
            <tr>
                <td>${p.nombre}</td>
                <td>${p.apellido}</td>
                <td>${p.edad}</td>
                <td>${p.telefono}</td>
                <td>${p.correo}</td>
                <td>${p.fecha_registro}</td>
                <td class="acciones">
                    <button class="btn btn-primary btn-sm" onclick="Pacientes.editar('${p._id}')">Editar</button>
                    <button class="btn btn-danger btn-sm" onclick="Pacientes.eliminar('${p._id}')">Eliminar</button>
                </td>
            </tr>
        `).join("");
    },

    renderizarPaginacion(totalPaginas) {
        const div = document.getElementById("paginacion-pacientes");
        if (totalPaginas <= 1) {
            div.innerHTML = "";
            return;
        }
        let html = "";
        for (let i = 1; i <= totalPaginas; i++) {
            html += `<button class="${i === this.paginaActual ? 'activa' : ''}" onclick="Pacientes.irPagina(${i})">${i}</button>`;
        }
        div.innerHTML = html;
    },

    irPagina(pagina) {
        this.paginaActual = pagina;
        this.cargar();
    },

    buscar(valor) {
        this.busqueda = valor;
        this.paginaActual = 1;
        this.cargar();
    },

    abrirFormulario(paciente = null) {
        const modal = document.getElementById("modal-paciente");
        const titulo = document.getElementById("modal-paciente-titulo");
        document.getElementById("form-paciente").reset();
        document.getElementById("paciente-id").value = "";

        if (paciente) {
            titulo.textContent = "Editar Paciente";
            document.getElementById("paciente-id").value = paciente._id;
            document.getElementById("paciente-nombre").value = paciente.nombre;
            document.getElementById("paciente-apellido").value = paciente.apellido;
            document.getElementById("paciente-edad").value = paciente.edad;
            document.getElementById("paciente-telefono").value = paciente.telefono;
            document.getElementById("paciente-correo").value = paciente.correo;
        } else {
            titulo.textContent = "Nuevo Paciente";
        }
        modal.classList.add("abierto");
    },

    cerrarFormulario() {
        document.getElementById("modal-paciente").classList.remove("abierto");
    },

    async guardar(e) {
        e.preventDefault();
        const id = document.getElementById("paciente-id").value;
        const data = {
            nombre: document.getElementById("paciente-nombre").value.trim(),
            apellido: document.getElementById("paciente-apellido").value.trim(),
            edad: parseInt(document.getElementById("paciente-edad").value),
            telefono: document.getElementById("paciente-telefono").value.trim(),
            correo: document.getElementById("paciente-correo").value.trim()
        };

        try {
            const url = id ? `${API}/pacientes/${id}` : `${API}/pacientes`;
            const method = id ? "PUT" : "POST";
            const res = await fetch(url, {
                method,
                headers: { "Content-Type": "application/json", ...authHeaders() },
                body: JSON.stringify(data)
            });
            if (handleAuthError(res)) return;
            const result = await res.json();

            if (!res.ok) {
                Notificacion.mostrar(result.error, "error");
                return;
            }
            Notificacion.mostrar(result.mensaje, "exito");
            this.cerrarFormulario();
            this.cargar();
        } catch (error) {
            Notificacion.mostrar("Error al guardar paciente", "error");
        }
    },

    async editar(id) {
        try {
            const res = await fetch(`${API}/pacientes/${id}`, { headers: authHeaders() });
            if (handleAuthError(res)) return;
            const paciente = await res.json();
            this.abrirFormulario(paciente);
        } catch (error) {
            Notificacion.mostrar("Error al cargar paciente", "error");
        }
    },

    async eliminar(id) {
        if (!confirm("¿Estas seguro de eliminar este paciente? Se eliminaran sus citas asociadas.")) return;
        try {
            const res = await fetch(`${API}/pacientes/${id}`, { method: "DELETE", headers: authHeaders() });
            if (handleAuthError(res)) return;
            const result = await res.json();
            if (!res.ok) {
                Notificacion.mostrar(result.error, "error");
                return;
            }
            Notificacion.mostrar(result.mensaje, "exito");
            this.cargar();
        } catch (error) {
            Notificacion.mostrar("Error al eliminar paciente", "error");
        }
    }
};
