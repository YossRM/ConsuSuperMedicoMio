using MongoDB.Driver;
using ConsultorioApi.Models;

namespace ConsultorioApi.Services
{
    public class UsuarioService
    {
        private readonly MongoDbService _mongoDb;

        public UsuarioService(MongoDbService mongoDb)
        {
            _mongoDb = mongoDb;
        }

        private IMongoCollection<Usuario> Usuarios => _mongoDb.db.GetCollection<Usuario>("usuarios");

        public async Task CrearIndiceUnico()
        {
            var indexKeys = Builders<Usuario>.IndexKeys.Ascending(u => u.Correo);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var model = new CreateIndexModel<Usuario>(indexKeys, indexOptions);
            await Usuarios.Indexes.CreateOneAsync(model);
        }

        public async Task<Usuario?> ObtenerPorCorreo(string correo)
        {
            return await Usuarios.Find(u => u.Correo == correo).FirstOrDefaultAsync();
        }

        public async Task<Usuario> Crear(Usuario usuario)
        {
            usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasena);
            await Usuarios.InsertOneAsync(usuario);
            return usuario;
        }

        public async Task<bool> ExisteCorreo(string correo)
        {
            return await Usuarios.CountDocumentsAsync(u => u.Correo == correo) > 0;
        }

        public bool VerificarContrasena(string contrasena, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(contrasena, hash);
        }
    }
}
