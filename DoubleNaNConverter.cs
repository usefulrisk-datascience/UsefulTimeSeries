using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UsefulTimeSeries
{
	public class DoubleNaNConverter:JsonConverter<double>
	{
    
        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            if (double.IsNaN(value))
            {
                writer.WriteNullValue();
            }
            else if (double.IsInfinity(value) || double.IsNegativeInfinity(value)){
                writer.WriteNullValue();
            }
            else
            {
                try{
                    writer.WriteNumberValue(value);
                }
                catch( System.ArgumentException ex){
                    Console.WriteLine(ex.Message +"value:"+ value);
                }
                
            }
        }
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return double.NaN;
            }

            if (reader.TryGetDouble(out double value))
            {
                return value;
            }

            throw new JsonException(); // Or handle the exception as appropriate for your scenario
        }
    }
}

