using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ConsultorioApi.Models
{
    public class Paciente
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; } = string.Empty;

        [BsonElement("nombre")]
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement("apellido")]
        [JsonPropertyName("apellido")]
        public string Apellido { get; set; } = string.Empty;

        [BsonElement("edad")]
        [JsonPropertyName("edad")]
        public int Edad { get; set; }

        [BsonElement("telefono")]
        [JsonPropertyName("telefono")]
        public string Telefono { get; set; } = string.Empty;

        [BsonElement("correo")]
        [JsonPropertyName("correo")]
        public string Correo { get; set; } = string.Empty;

        [BsonElement("fecha_registro")]
        [JsonPropertyName("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
