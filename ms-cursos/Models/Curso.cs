// =====================================================================
// CAPA DE MODELO (entidad) para cursos
// Define los campos de un Curso y sus reglas de validación.
// La unicidad del código debe verificarse en la capa de servicio o
// persistencia; si ya existe, la API debe responder HTTP 409.
// =====================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ms_cursos.Models;

public class Curso
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El código es obligatorio")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, 6, ErrorMessage = "Los créditos deben estar entre 1 y 6")]
    public int Creditos { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El cupo debe ser mayor que 0")]
    public int Cupo { get; set; }

    [Required(ErrorMessage = "El semestre es obligatorio")]
    [RegularExpression(@"^\d{4}-[12]$",
        ErrorMessage = "El semestre debe tener el formato AAAA-1 o AAAA-2")]
    public string Semestre { get; set; } = string.Empty;
}

public sealed class CrearCursoRequest
{
    [JsonPropertyName("codigo")]
    [Required(ErrorMessage = "El código es obligatorio")]
    public string Codigo { get; init; } = string.Empty;

    [JsonPropertyName("nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; init; } = string.Empty;

    [JsonPropertyName("creditos")]
    [Range(1, 6, ErrorMessage = "Los créditos deben estar entre 1 y 6")]
    public int Creditos { get; init; }

    [JsonPropertyName("cupo")]
    [Range(1, int.MaxValue, ErrorMessage = "El cupo debe ser mayor que 0")]
    public int Cupo { get; init; }

    [JsonPropertyName("semestre")]
    [Required(ErrorMessage = "El semestre es obligatorio")]
    [RegularExpression(@"^\d{4}-[12]$",
        ErrorMessage = "El semestre debe tener el formato AAAA-1 o AAAA-2")]
    public string Semestre { get; init; } = string.Empty;
}

public sealed class ActualizarCursoRequest
{
    [JsonPropertyName("codigo")]
    public string? Codigo { get; init; }

    [JsonPropertyName("nombre")]
    public string? Nombre { get; init; }

    [JsonPropertyName("creditos")]
    [Range(1, 6, ErrorMessage = "Los créditos deben estar entre 1 y 6")]
    public int? Creditos { get; init; }

    [JsonPropertyName("cupo")]
    [Range(1, int.MaxValue, ErrorMessage = "El cupo debe ser mayor que 0")]
    public int? Cupo { get; init; }

    [JsonPropertyName("semestre")]
    [RegularExpression(@"^\d{4}-[12]$",
        ErrorMessage = "El semestre debe tener el formato AAAA-1 o AAAA-2")]
    public string? Semestre { get; init; }
}
