using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backend.Models.External;

public sealed class ConvocaProcessRecord
{
    [JsonPropertyName("id_del_proceso")]
    public string? ProcessId { get; set; }

    [JsonPropertyName("referencia_del_proceso")]
    public string? Reference { get; set; }

    [JsonPropertyName("entidad")]
    public string? EntityName { get; set; }

    [JsonPropertyName("nit_entidad")]
    public string? EntityNit { get; set; }

    [JsonPropertyName("departamento_entidad")]
    public string? Department { get; set; }

    [JsonPropertyName("ciudad_entidad")]
    public string? City { get; set; }

    [JsonPropertyName("nombre_del_procedimiento")]
    public string? ProcedureName { get; set; }

    [JsonPropertyName("descripci_n_del_procedimiento")]
    public string? Description { get; set; }

    [JsonPropertyName("fase")]
    public string? Phase { get; set; }

    [JsonPropertyName("fecha_de_publicacion_del")]
    public string? PublishedOn { get; set; }

    [JsonPropertyName("fecha_de_ultima_publicaci")]
    public string? LastPublishedOn { get; set; }

    [JsonPropertyName("fecha_de_recepcion_de")]
    public string? OffersDueOn { get; set; }

    [JsonPropertyName("precio_base")]
    public string? BasePrice { get; set; }

    [JsonPropertyName("modalidad_de_contratacion")]
    public string? Modality { get; set; }

    [JsonPropertyName("duracion")]
    public string? Duration { get; set; }

    [JsonPropertyName("unidad_de_duracion")]
    public string? DurationUnit { get; set; }

    [JsonPropertyName("estado_del_procedimiento")]
    public string? Status { get; set; }

    [JsonPropertyName("adjudicado")]
    public string? Awarded { get; set; }

    [JsonPropertyName("fecha_adjudicacion")]
    public string? AwardedOn { get; set; }

    [JsonPropertyName("valor_total_adjudicacion")]
    public string? AwardAmount { get; set; }

    [JsonPropertyName("nombre_del_proveedor")]
    public string? SupplierName { get; set; }

    [JsonPropertyName("nit_del_proveedor_adjudicado")]
    public string? SupplierNit { get; set; }

    [JsonPropertyName("codigo_principal_de_categoria")]
    public string? UnspscCode { get; set; }

    [JsonPropertyName("estado_de_apertura_del_proceso")]
    public string? OpeningStatus { get; set; }

    [JsonPropertyName("tipo_de_contrato")]
    public string? ContractType { get; set; }

    [JsonPropertyName("urlproceso")]
    [JsonConverter(typeof(ConvocaUrlJsonConverter))]
    public string? ProcessUrl { get; set; }
}

public sealed class ConvocaUrlJsonConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.StartObject:
                string? url = null;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "url")
                    {
                        reader.Read();
                        url = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                    }
                    else if (reader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
                    {
                        reader.Skip();
                    }
                }

                return url;
            default:
                reader.Skip();
                return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value);
    }
}
