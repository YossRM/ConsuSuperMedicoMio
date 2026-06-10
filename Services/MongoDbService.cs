using MongoDB.Driver;
using ConsultorioApi.Models;
using Microsoft.Extensions.Options;

namespace ConsultorioApi.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public IMongoDatabase db => _database;

        public MongoDbService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<Paciente> Pacientes => _database.GetCollection<Paciente>("pacientes");
        public IMongoCollection<Cita> Citas => _database.GetCollection<Cita>("citas");

        public void CrearIndiceUnico()
        {
            var indexKeys = Builders<Paciente>.IndexKeys.Ascending(p => p.Correo);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var model = new CreateIndexModel<Paciente>(indexKeys, indexOptions);
            Pacientes.Indexes.CreateOne(model);
        }
    }
}
