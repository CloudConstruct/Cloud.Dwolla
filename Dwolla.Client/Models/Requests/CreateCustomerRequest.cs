using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Dwolla.Client.Models.Requests
{
    public class CreateCustomerRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Type { get; set; }
        public string IpAddress { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        [JsonConverter(typeof(WriteAsUppercase))]
        public string State { get; set; }
        public string PostalCode { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Ssn { get; set; }
        public string Phone { get; set; }
        public string BusinessName { get; set; }
        public string BusinessType { get; set; }
        public string BusinessClassification { get; set; }
        public string Ein { get; set; }
        public string DoingBusinessAs { get; set; }
        public string Website { get; set; }
        public Controller Controller { get; set; }
    }

    class WriteAsUppercase : JsonConverter<string>
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
