using Domain.Primitives;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Domain.Converters;

/// <summary>
/// Resolver customiozado para serialização polimórfica com System.Text.Json.
/// Permite que objetos que implementam IDomainEvent sejam serializados mantendo informações do tipo,
/// similar ao TypeNameHandling.All do Newtonsoft.Json.
/// </summary>
public class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);
        
        if (jsonTypeInfo.Type.IsAssignableTo(typeof(IDomainEvent)))
        {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
            };
        }

        return jsonTypeInfo;
    }
}
