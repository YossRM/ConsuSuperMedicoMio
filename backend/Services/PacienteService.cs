using MongoDB.Driver;
using ConsultorioApi.Models;

namespace ConsultorioApi.Services
{
    public class PacienteService
    {
        private readonly MongoDbService _mongoDb;

        public PacienteService(MongoDbService mongoDb)
        {
            _mongoDb = mongoDb;
            _mongoDb.CrearIndiceUnico();
        }

        public async Task<List<Paciente>> ObtenerTodos(string? busqueda, int pagina, int porPagina)
        {
            var filtro = Builders<Paciente>.Filter.Empty;

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                filtro = Builders<Paciente>.Filter.Or(
                    Builders<Paciente>.Filter.Regex(p => p.Nombre, new MongoDB.Bson.BsonRegularExpression(busqueda, "i")),
                    Builders<Paciente>.Filter.Regex(p => p.Apellido, new MongoDB.Bson.BsonRegularExpression(busqueda, "i")),
                    Builders<Paciente>.Filter.Regex(p => p.Correo, new MongoDB.Bson.BsonRegularExpression(busqueda, "i"))
                );
            }

            return await _mongoDb.Pacientes
                .Find(filtro)
                .Skip((pagina - 1) * porPagina)
                .Limit(porPagina)
                .ToListAsync();
        }

        public async Task<long> Contar(string? busqueda)
        {
            var filtro = Builders<Paciente>.Filter.Empty;

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                filtro = Builders<Paciente>.Filter.Or(
                    Builders<Paciente>.Filter.Regex(p => p.Nombre, new MongoDB.Bson.BsonRegularExpression(busqueda, "i")),
                    Builders<Paciente>.Filter.Regex(p => p.Apellido, new MongoDB.Bson.BsonRegularExpression(busqueda, "i")),
                    Builders<Paciente>.Filter.Regex(p => p.Correo, new MongoDB.Bson.BsonRegularExpression(busqueda, "i"))
                );
            }

            return await _mongoDb.Pacientes.CountDocumentsAsync(filtro);
        }

        public async Task<Paciente?> ObtenerPorId(string id)
        {
            return await _mongoDb.Pacientes.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Paciente> Crear(Paciente paciente)
        {
            paciente.FechaRegistro = DateTime.Now;
            await _mongoDb.Pacientes.InsertOneAsync(paciente);
            return paciente;
        }

        public async Task<bool> ExisteCorreo(string correo, string? excluirId = null)
        {
            var filtro = Builders<Paciente>.Filter.Eq(p => p.Correo, correo);
            if (!string.IsNullOrEmpty(excluirId))
            {
                filtro = Builders<Paciente>.Filter.And(
                    filtro,
                    Builders<Paciente>.Filter.Ne(p => p.Id, excluirId)
                );
            }
            return await _mongoDb.Pacientes.CountDocumentsAsync(filtro) > 0;
        }

        public async Task<bool> Actualizar(string id, Dictionary<string, object> campos)
        {
            var update = Builders<Paciente>.Update.Set(campos.First().Key, campos.First().Value);
            foreach (var campo in campos.Skip(1))
            {
                update = update.Set(campo.Key, campo.Value);
            }

            var result = await _mongoDb.Pacientes.UpdateOneAsync(p => p.Id == id, update);
            return result.MatchedCount > 0;
        }

        public async Task<bool> Eliminar(string id)
        {
            var result = await _mongoDb.Pacientes.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task EliminarCitasDelPaciente(string pacienteId)
        {
            await _mongoDb.Citas.DeleteManyAsync(c => c.PacienteId == pacienteId);
        }
    }
}
