using MongoDB.Driver;
using ConsultorioApi.Models;

namespace ConsultorioApi.Services
{
    public class CitaService
    {
        private readonly MongoDbService _mongoDb;
        private static readonly string[] EstadosValidos = ["Pendiente", "Confirmada", "Completada", "Cancelada"];

        public CitaService(MongoDbService mongoDb)
        {
            _mongoDb = mongoDb;
        }

        public async Task<List<CitaResponse>> ObtenerTodas(string? busqueda, string? estado, DateTime? fecha, int pagina, int porPagina)
        {
            var filtro = Builders<Cita>.Filter.Empty;

            if (!string.IsNullOrWhiteSpace(estado) && EstadosValidos.Contains(estado))
            {
                filtro = Builders<Cita>.Filter.Eq(c => c.Estado, estado);
            }

            if (fecha.HasValue)
            {
                var fechaInicio = fecha.Value.Date;
                var fechaFin = fechaInicio.AddDays(1);
                filtro = Builders<Cita>.Filter.And(
                    filtro,
                    Builders<Cita>.Filter.Gte(c => c.Fecha, fechaInicio),
                    Builders<Cita>.Filter.Lt(c => c.Fecha, fechaFin)
                );
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var pacientes = await _mongoDb.Pacientes
                    .Find(Builders<Paciente>.Filter.Or(
                        Builders<Paciente>.Filter.Regex(p => p.Nombre, new MongoDB.Bson.BsonRegularExpression(busqueda, "i")),
                        Builders<Paciente>.Filter.Regex(p => p.Apellido, new MongoDB.Bson.BsonRegularExpression(busqueda, "i"))
                    ))
                    .ToListAsync();

                var ids = pacientes.Select(p => p.Id).ToList();
                if (ids.Any())
                {
                    filtro = Builders<Cita>.Filter.And(
                        filtro,
                        Builders<Cita>.Filter.In(c => c.PacienteId, ids)
                    );
                }
                else
                {
                    return new List<CitaResponse>();
                }
            }

            var citas = await _mongoDb.Citas
                .Find(filtro)
                .Sort(Builders<Cita>.Sort.Ascending(c => c.Fecha))
                .Skip((pagina - 1) * porPagina)
                .Limit(porPagina)
                .ToListAsync();

            var resultado = new List<CitaResponse>();
            foreach (var cita in citas)
            {
                var paciente = await _mongoDb.Pacientes.Find(p => p.Id == cita.PacienteId).FirstOrDefaultAsync();
                resultado.Add(new CitaResponse
                {
                    Id = cita.Id,
                    PacienteId = cita.PacienteId,
                    PacienteNombre = paciente != null ? $"{paciente.Nombre} {paciente.Apellido}" : "Paciente eliminado",
                    Fecha = cita.Fecha.ToString("yyyy-MM-dd"),
                    Hora = cita.Hora,
                    Motivo = cita.Motivo,
                    Estado = cita.Estado
                });
            }

            return resultado;
        }

        public async Task<long> Contar(string? busqueda, string? estado, DateTime? fecha)
        {
            var filtro = Builders<Cita>.Filter.Empty;

            if (!string.IsNullOrWhiteSpace(estado) && EstadosValidos.Contains(estado))
            {
                filtro = Builders<Cita>.Filter.Eq(c => c.Estado, estado);
            }

            if (fecha.HasValue)
            {
                var fechaInicio = fecha.Value.Date;
                var fechaFin = fechaInicio.AddDays(1);
                filtro = Builders<Cita>.Filter.And(
                    filtro,
                    Builders<Cita>.Filter.Gte(c => c.Fecha, fechaInicio),
                    Builders<Cita>.Filter.Lt(c => c.Fecha, fechaFin)
                );
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var pacientes = await _mongoDb.Pacientes
                    .Find(Builders<Paciente>.Filter.Or(
                        Builders<Paciente>.Filter.Regex(p => p.Nombre, new MongoDB.Bson.BsonRegularExpression(busqueda, "i")),
                        Builders<Paciente>.Filter.Regex(p => p.Apellido, new MongoDB.Bson.BsonRegularExpression(busqueda, "i"))
                    ))
                    .ToListAsync();

                var ids = pacientes.Select(p => p.Id).ToList();
                if (ids.Any())
                {
                    filtro = Builders<Cita>.Filter.And(
                        filtro,
                        Builders<Cita>.Filter.In(c => c.PacienteId, ids)
                    );
                }
                else
                {
                    return 0;
                }
            }

            return await _mongoDb.Citas.CountDocumentsAsync(filtro);
        }

        public async Task<Cita?> ObtenerPorId(string id)
        {
            return await _mongoDb.Citas.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task<CitaResponse?> ObtenerPorIdConPaciente(string id)
        {
            var cita = await ObtenerPorId(id);
            if (cita == null) return null;

            var paciente = await _mongoDb.Pacientes.Find(p => p.Id == cita.PacienteId).FirstOrDefaultAsync();
            return new CitaResponse
            {
                Id = cita.Id,
                PacienteId = cita.PacienteId,
                PacienteNombre = paciente != null ? $"{paciente.Nombre} {paciente.Apellido}" : "Paciente eliminado",
                Fecha = cita.Fecha.ToString("yyyy-MM-dd"),
                Hora = cita.Hora,
                Motivo = cita.Motivo,
                Estado = cita.Estado
            };
        }

        public async Task<Cita> Crear(Cita cita)
        {
            await _mongoDb.Citas.InsertOneAsync(cita);
            return cita;
        }

        public async Task<bool> PacienteExiste(string pacienteId)
        {
            return await _mongoDb.Pacientes.CountDocumentsAsync(p => p.Id == pacienteId) > 0;
        }

        public async Task<bool> EstadoValido(string estado)
        {
            return EstadosValidos.Contains(estado);
        }

        public async Task<bool> Actualizar(string id, Cita cita)
        {
            var result = await _mongoDb.Citas.ReplaceOneAsync(c => c.Id == id, cita);
            return result.MatchedCount > 0;
        }

        public async Task<bool> Eliminar(string id)
        {
            var result = await _mongoDb.Citas.DeleteOneAsync(c => c.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
