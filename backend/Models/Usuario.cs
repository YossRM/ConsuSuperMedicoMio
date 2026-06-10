using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ConsultorioApi.Models
{
    public class Usuario
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; } = string.Empty;

        [BsonElement("correo")]
        [JsonPropertyName("correo")]
        public string Correo { get; set; } = string.Empty;

        [BsonElement("contraseña")]
        [JsonPropertyName("contraseña")]
        public string Contrasena { get; set; } = string.Empty;

        [BsonElement("nombre")]
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement("rol")]
        [JsonPropertyName("rol")]
        public string Rol { get; set; } = "admin";
    }
}
