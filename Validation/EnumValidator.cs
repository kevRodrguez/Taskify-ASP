using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Taskify.Validation;

public static class EnumValidator
{
    public static bool IsDefined(Type enumType, object value)
    {
        var underlying = Enum.GetUnderlyingType(enumType);
        var normalized = Convert.ChangeType(value, underlying);
        return Enum.IsDefined(enumType, normalized);
    }

    public static void ValidateTrackedEntities(ChangeTracker changeTracker)
    {
        foreach (var entry in changeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            foreach (var property in entry.Properties)
            {
                var enumType = property.Metadata.ClrType;
                if (!enumType.IsEnum)
                {
                    continue;
                }

                var value = property.CurrentValue;
                if (value is null)
                {
                    continue;
                }

                if (!IsDefined(enumType, value))
                {
                    var entityName = entry.Metadata.ClrType.Name;
                    var propertyName = property.Metadata.Name;
                    var numeric = Convert.ToInt64(value);

                    throw new InvalidOperationException(
                        $"Valor de enum no válido: {entityName}.{propertyName} = {numeric}.");
                }
            }
        }
    }
}
