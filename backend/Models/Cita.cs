using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ConsultorioApi.Models
{
    public class Cita
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; } = string.Empty;

        [BsonElement("paciente_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("paciente_id")]
        public string PacienteId { get; set; } = string.Empty;

        [BsonElement("fecha")]
        [JsonPropertyName("fecha")]
        public DateTime Fecha { get; set; }

        [BsonElement("hora")]
        [JsonPropertyName("hora")]
        public string Hora { get; set; } = string.Empty;

        [BsonElement("motivo")]
        [JsonPropertyName("motivo")]
        public string Motivo { get; set; } = string.Empty;

        [BsonElement("estado")]
        [JsonPropertyName("estado")]
        public string Estado { get; set; } = "Pendiente";
    }

    public class CitaResponse
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("paciente_id")]
        public string PacienteId { get; set; } = string.Empty;

        [JsonPropertyName("paciente_nombre")]
        public string? PacienteNombre { get; set; }

        [JsonPropertyName("fecha")]
        public string Fecha { get; set; } = string.Empty;

        [JsonPropertyName("hora")]
        public string Hora { get; set; } = string.Empty;

        [JsonPropertyName("motivo")]
        public string Motivo { get; set; } = string.Empty;

        [JsonPropertyName("estado")]
        public string Estado { get; set; } = string.Empty;
    }
}
