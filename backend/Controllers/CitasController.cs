using Microsoft.AspNetCore.Mvc;
using ConsultorioApi.Models;
using ConsultorioApi.Services;

namespace ConsultorioApi.Controllers
{
    [ApiController]
    [Route("citas")]
    public class CitasController : ControllerBase
    {
        private readonly CitaService _citaService;

        public CitasController(CitaService citaService)
        {
            _citaService = citaService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodas(
            [FromQuery] string? busqueda = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? fecha = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int por_pagina = 10)
        {
            DateTime? fechaFiltro = null;
            if (!string.IsNullOrEmpty(fecha) && DateTime.TryParse(fecha, out var f))
                fechaFiltro = f;

            var citas = await _citaService.ObtenerTodas(busqueda, estado, fechaFiltro, pagina, por_pagina);
            var total = await _citaService.Contar(busqueda, estado, fechaFiltro);

            return Ok(new
            {
                citas,
                total,
                pagina,
                total_paginas = (int)Math.Ceiling((double)total / por_pagina)
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(string id)
        {
            var cita = await _citaService.ObtenerPorIdConPaciente(id);
            if (cita == null)
                return NotFound(new { error = "Cita no encontrada" });

            return Ok(cita);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Cita cita)
        {
            if (string.IsNullOrWhiteSpace(cita.PacienteId) ||
                string.IsNullOrWhiteSpace(cita.Hora) ||
                string.IsNullOrWhiteSpace(cita.Motivo))
            {
                return BadRequest(new { error = "Paciente, fecha, hora y motivo son obligatorios" });
            }

            if (!await _citaService.PacienteExiste(cita.PacienteId))
                return BadRequest(new { error = "El paciente no existe" });

            if (!string.IsNullOrWhiteSpace(cita.Estado) && !await _citaService.EstadoValido(cita.Estado))
                return BadRequest(new { error = "Estado invalido. Valores permitidos: Pendiente, Confirmada, Completada, Cancelada" });

            var nueva = await _citaService.Crear(cita);
            return StatusCode(201, new { mensaje = "Cita registrada exitosamente", cita = nueva });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(string id, [FromBody] Cita data)
        {
            var existente = await _citaService.ObtenerPorId(id);
            if (existente == null)
                return NotFound(new { error = "Cita no encontrada" });

            if (!string.IsNullOrWhiteSpace(data.PacienteId) && !await _citaService.PacienteExiste(data.PacienteId))
                return BadRequest(new { error = "El paciente no existe" });

            if (!string.IsNullOrWhiteSpace(data.Estado) && !await _citaService.EstadoValido(data.Estado))
                return BadRequest(new { error = "Estado invalido. Valores permitidos: Pendiente, Confirmada, Completada, Cancelada" });

            if (!string.IsNullOrWhiteSpace(data.PacienteId))
                existente.PacienteId = data.PacienteId;

            if (data.Fecha != default)
                existente.Fecha = data.Fecha;

            if (!string.IsNullOrWhiteSpace(data.Hora))
                existente.Hora = data.Hora;

            if (!string.IsNullOrWhiteSpace(data.Motivo))
                existente.Motivo = data.Motivo;

            if (!string.IsNullOrWhiteSpace(data.Estado))
                existente.Estado = data.Estado;

            await _citaService.Actualizar(id, existente);
            var actualizada = await _citaService.ObtenerPorIdConPaciente(id);

            return Ok(new { mensaje = "Cita actualizada", cita = actualizada });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(string id)
        {
            var eliminada = await _citaService.Eliminar(id);
            if (!eliminada)
                return NotFound(new { error = "Cita no encontrada" });

            return Ok(new { mensaje = "Cita eliminada" });
        }
    }
}
