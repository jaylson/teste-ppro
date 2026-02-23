using System.Data;
using System.Runtime.Serialization;
using Dapper;

namespace PartnershipManager.Infrastructure.Persistence.TypeHandlers;

/// <summary>
/// Dapper TypeHandler for enums with EnumMember attributes
/// Maps database snake_case values to C# PascalCase enum values
/// </summary>
/// <typeparam name="T">Enum type</typeparam>
public class EnumMemberTypeHandler<T> : SqlMapper.TypeHandler<T> where T : struct, Enum
{
    public override T Parse(object value)
    {
        if (value == null || value is DBNull)
        {
            return default;
        }

        var stringValue = value.ToString();
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return default;
        }

        // Try to find enum value by EnumMember attribute
        foreach (var field in typeof(T).GetFields().Where(f => f.IsLiteral))
        {
            var attribute = field.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                .FirstOrDefault() as EnumMemberAttribute;

            if (attribute != null && attribute.Value == stringValue)
            {
                return (T)field.GetValue(null)!;
            }
        }

        // Fallback: try parse by name (case-insensitive)
        if (Enum.TryParse<T>(stringValue, true, out var result))
        {
            return result;
        }

        // If still not found, throw exception with helpful message
        throw new ArgumentException(
            $"Unable to map '{stringValue}' to enum {typeof(T).Name}. " +
            $"Valid values are: {string.Join(", ", GetValidValues())}");
    }

    public override void SetValue(IDbDataParameter parameter, T value)
    {
        // Get the EnumMember value if exists, otherwise use enum name
        var field = typeof(T).GetField(value.ToString()!);
        var attribute = field?.GetCustomAttributes(typeof(EnumMemberAttribute), false)
            .FirstOrDefault() as EnumMemberAttribute;

        parameter.Value = attribute?.Value ?? value.ToString()!.ToLowerInvariant();
        parameter.DbType = DbType.String;
    }

    private static IEnumerable<string> GetValidValues()
    {
        foreach (var field in typeof(T).GetFields().Where(f => f.IsLiteral))
        {
            var attribute = field.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                .FirstOrDefault() as EnumMemberAttribute;

            yield return attribute?.Value ?? field.Name;
        }
    }
}
