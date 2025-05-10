using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NeoFrost.Types.LoginMethods;

namespace NeoFrost.Types.Conversion;

public class LoginConverter : JsonConverter<LoginAuthentication>
{
    public override bool CanConvert(Type type)
    {
        return typeof(LoginAuthentication).IsAssignableFrom(type);
    }

    public override LoginAuthentication? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // return null;
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, LoginAuthentication value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case PasswordLogin password:
                JsonSerializer.Serialize(writer, password);
                break;
            case SessionTokenLogin session:
                JsonSerializer.Serialize(writer, session);
                break;
            default:
                throw new NotSupportedException(value.GetType().Name);
        }
    }
}