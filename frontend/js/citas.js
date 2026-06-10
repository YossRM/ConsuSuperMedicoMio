const Citas = {
    paginaActual: 1,
    busqueda: "",
    estado: "",

    async cargar() {
        try {
            const params = new URLSearchParams({
                pagina: this.paginaActual,
                por_pagina: 10,
                busqueda: this.busqueda,
                estado: this.estado
            });
            const res = await fetch(`${API}/citas?${params}`, { headers: authHeaders() });
            if (handleAuthError(res)) return;
            const data = await res.json();
            this.renderizarTabla(data.citas);
            this.renderizarPaginacion(data.total_paginas);
        } catch (error) {
            Notificacion.mostrar("Error al cargar citas", "error");
        }
    },

    renderizarTabla(citas) {
        const tbody = document.querySelector("#tabla-citas tbody");
        if (citas.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;color:#6b7280;">No se encontraron citas</td></tr>';
            return;
        }
        tbody.innerHTML = citas.map(c => {
            const badgeClass = `badge-${c.estado.toLowerCase()}`;
            return `
                <tr>
                    <td>${c.paciente_nombre}</td>
                    <td>${c.fecha}</td>
                    <td>${c.hora}</td>
                    <td>${c.motivo}</td>
                    <td><span class="badge ${badgeClass}">${c.estado}</span></td>
                    <td class="acciones">
                        <button class="btn btn-primary btn-sm" onclick="Citas.editar('${c._id}')">Editar</button>
                        <button class="btn btn-danger btn-sm" onclick="Citas.eliminar('${c._id}')">Eliminar</button>
                    </td>
                </tr>
            `;
        }).join("");
    },

    renderizarPaginacion(totalPaginas) {
        const div = document.getElementById("paginacion-citas");
        if (totalPaginas <= 1) {
            div.innerHTML = "";
            return;
        }
        let html = "";
        for (let i = 1; i <= totalPaginas; i++) {
            html += `<button class="${i === this.paginaActual ? 'activa' : ''}" onclick="Citas.irPagina(${i})">${i}</button>`;
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

    filtrarEstado(valor) {
        this.estado = valor;
        this.paginaActual = 1;
        this.cargar();
    },

    async cargarPacientesSelect() {
        try {
            const res = await fetch(`${API}/pacientes?por_pagina=1000`, { headers: authHeaders() });
            if (handleAuthError(res)) return;
            const data = await res.json();
            const select = document.getElementById("cita-paciente");
            select.innerHTML = '<option value="">Seleccionar paciente</option>';
            data.pacientes.forEach(p => {
                select.innerHTML += `<option value="${p._id}">${p.nombre} ${p.apellido}</option>`;
            });
        } catch (error) {
            Notificacion.mostrar("Error al cargar pacientes", "error");
        }
    },

    async abrirFormulario(cita = null) {
        await this.cargarPacientesSelect();
        const modal = document.getElementById("modal-cita");
        const titulo = document.getElementById("modal-cita-titulo");
        document.getElementById("form-cita").reset();
        document.getElementById("cita-id").value = "";

        if (cita) {
            titulo.textContent = "Editar Cita";
            document.getElementById("cita-id").value = cita._id;
            document.getElementById("cita-paciente").value = cita.paciente_id;
            document.getElementById("cita-fecha").value = cita.fecha;
            document.getElementById("cita-hora").value = cita.hora;
            document.getElementById("cita-motivo").value = cita.motivo;
            document.getElementById("cita-estado").value = cita.estado;
        } else {
            titulo.textContent = "Nueva Cita";
        }
        modal.classList.add("abierto");
    },

    cerrarFormulario() {
        document.getElementById("modal-cita").classList.remove("abierto");
    },

    async guardar(e) {
        e.preventDefault();
        const id = document.getElementById("cita-id").value;
        const data = {
            paciente_id: document.getElementById("cita-paciente").value,
            fecha: document.getElementById("cita-fecha").value,
            hora: document.getElementById("cita-hora").value,
            motivo: document.getElementById("cita-motivo").value.trim(),
            estado: document.getElementById("cita-estado").value
        };

        try {
            const url = id ? `${API}/citas/${id}` : `${API}/citas`;
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
            Notificacion.mostrar("Error al guardar cita", "error");
        }
    },

    async editar(id) {
        try {
            const res = await fetch(`${API}/citas/${id}`, { headers: authHeaders() });
            if (handleAuthError(res)) return;
            const cita = await res.json();
            this.abrirFormulario(cita);
        } catch (error) {
            Notificacion.mostrar("Error al cargar cita", "error");
        }
    },

    async eliminar(id) {
        if (!confirm("¿Estas seguro de eliminar esta cita?")) return;
        try {
            const res = await fetch(`${API}/citas/${id}`, { method: "DELETE", headers: authHeaders() });
            if (handleAuthError(res)) return;
            const result = await res.json();
            if (!res.ok) {
                Notificacion.mostrar(result.error, "error");
                return;
            }
            Notificacion.mostrar(result.mensaje, "exito");
            this.cargar();
        } catch (error) {
            Notificacion.mostrar("Error al eliminar cita", "error");
        }
    }
};
