using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Dwolla.Client.Models
{
    internal class WriteAsUppercaseJsonConverter : JsonConverter<string>
    {
        public override string ReadJson(JsonReader reader, Type objectType, [AllowNull] string existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.ReadAsString();
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] string value, JsonSerializer serializer)
        {
            if (value == null && serializer.NullValueHandling == NullValueHandling.Include)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(value.ToUpperInvariant());
            }
        }
    }
}
