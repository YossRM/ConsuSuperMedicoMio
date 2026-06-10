using Microsoft.AspNetCore.Mvc;
using ConsultorioApi.Models;
using ConsultorioApi.Services;

namespace ConsultorioApi.Controllers
{
    [ApiController]
    [Route("pacientes")]
    public class PacientesController : ControllerBase
    {
        private readonly PacienteService _pacienteService;

        public PacientesController(PacienteService pacienteService)
        {
            _pacienteService = pacienteService;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos(
            [FromQuery] string? busqueda = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int por_pagina = 10)
        {
            var pacientes = await _pacienteService.ObtenerTodos(busqueda, pagina, por_pagina);
            var total = await _pacienteService.Contar(busqueda);

            return Ok(new
            {
                pacientes,
                total,
                pagina,
                total_paginas = (int)Math.Ceiling((double)total / por_pagina)
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(string id)
        {
            var paciente = await _pacienteService.ObtenerPorId(id);
            if (paciente == null)
                return NotFound(new { error = "Paciente no encontrado" });

            return Ok(paciente);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Paciente paciente)
        {
            if (string.IsNullOrWhiteSpace(paciente.Nombre) ||
                string.IsNullOrWhiteSpace(paciente.Apellido) ||
                string.IsNullOrWhiteSpace(paciente.Telefono) ||
                string.IsNullOrWhiteSpace(paciente.Correo))
            {
                return BadRequest(new { error = "Nombre, apellido, telefono y correo son obligatorios" });
            }

            if (paciente.Edad <= 0)
                return BadRequest(new { error = "La edad debe ser mayor a 0" });

            if (await _pacienteService.ExisteCorreo(paciente.Correo))
                return BadRequest(new { error = "Ya existe un paciente con ese correo" });

            var nuevo = await _pacienteService.Crear(paciente);
            return StatusCode(201, new { mensaje = "Paciente registrado exitosamente", paciente = nuevo });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(string id, [FromBody] Paciente data)
        {
            var existente = await _pacienteService.ObtenerPorId(id);
            if (existente == null)
                return NotFound(new { error = "Paciente no encontrado" });

            var campos = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(data.Nombre))
                campos["Nombre"] = data.Nombre;

            if (!string.IsNullOrWhiteSpace(data.Apellido))
                campos["Apellido"] = data.Apellido;

            if (data.Edad > 0)
                campos["Edad"] = data.Edad;

            if (!string.IsNullOrWhiteSpace(data.Telefono))
                campos["Telefono"] = data.Telefono;

            if (!string.IsNullOrWhiteSpace(data.Correo))
            {
                if (await _pacienteService.ExisteCorreo(data.Correo, id))
                    return BadRequest(new { error = "Ya existe otro paciente con ese correo" });
                campos["Correo"] = data.Correo;
            }

            if (campos.Count == 0)
                return BadRequest(new { error = "No hay campos para actualizar" });

            await _pacienteService.Actualizar(id, campos);
            var actualizado = await _pacienteService.ObtenerPorId(id);

            return Ok(new { mensaje = "Paciente actualizado", paciente = actualizado });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(string id)
        {
            var eliminado = await _pacienteService.Eliminar(id);
            if (!eliminado)
                return NotFound(new { error = "Paciente no encontrado" });

            await _pacienteService.EliminarCitasDelPaciente(id);
            return Ok(new { mensaje = "Paciente y sus citas eliminados" });
        }
    }
}
