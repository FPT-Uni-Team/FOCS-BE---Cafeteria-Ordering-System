using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace FOCS.Common.Helpers
{
    public class UtcToVietNamTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetDateTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                var vnTime = TimeZoneInfo.ConvertTimeFromUtc(
                    value.Value,
                    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
                );
                writer.WriteStringValue(vnTime);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }

}
