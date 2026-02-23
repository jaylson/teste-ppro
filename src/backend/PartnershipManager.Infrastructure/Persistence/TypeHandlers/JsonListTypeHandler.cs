using System.Data;
using System.Text.Json;
using Dapper;

namespace PartnershipManager.Infrastructure.Persistence.TypeHandlers;

/// <summary>
/// Dapper TypeHandler for JSON arrays stored as strings
/// Maps database JSON strings to C# List<string> and vice versa
/// </summary>
public class JsonListTypeHandler : SqlMapper.TypeHandler<List<string>>
{
    public override List<string> Parse(object value)
    {
        if (value == null || value is DBNull)
        {
            return new List<string>();
        }

        var stringValue = value.ToString();
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return new List<string>();
        }

        try
        {
            // Deserialize JSON array to List<string>
            var result = JsonSerializer.Deserialize<List<string>>(stringValue);
            return result ?? new List<string>();
        }
        catch (JsonException)
        {
            // If it's not valid JSON, return empty list
            return new List<string>();
        }
    }

    public override void SetValue(IDbDataParameter parameter, List<string>? value)
    {
        if (value == null || value.Count == 0)
        {
            parameter.Value = "[]";
        }
        else
        {
            // Serialize List<string> to JSON array
            parameter.Value = JsonSerializer.Serialize(value);
        }
        
        parameter.DbType = DbType.String;
    }
}
