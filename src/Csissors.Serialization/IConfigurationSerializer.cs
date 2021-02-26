using Csissors.Schedule;
using Csissors.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Csissors.Serialization {
    public interface IConfigurationSerializer
    {
        JObject Serialize(string taskName, TaskConfiguration configuration);
        (string, TaskConfiguration) Deserialize(JObject configuration);
    }

    public class ScheduleConverter : JsonConverter
    {
        private readonly JsonSerializer _defaultSerializer;

        public ScheduleConverter() {
            _defaultSerializer = new JsonSerializer();
        }
        
        public override bool CanConvert(Type objectType)
        {
            return typeof(ISchedule).IsAssignableFrom(objectType);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            Type scheduleType = Type.GetType(reader.ReadAsString());
            reader.Read();
            var result = _defaultSerializer.Deserialize(reader, scheduleType);
            reader.Read();
            return result;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            writer.WriteStartArray();
            writer.WriteValue(value.GetType().AssemblyQualifiedName);
            _defaultSerializer.Serialize(writer, value);
            writer.WriteEndArray();
        }
    }

    public class ConfigurationSerializer : IConfigurationSerializer
    {
        private readonly JsonSerializer _serializer;
        public ConfigurationSerializer() {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new ScheduleConverter());
            _serializer = serializer;
        }

        public (string, TaskConfiguration) Deserialize(JObject configuration)
        {
            return configuration.ToObject<(string, TaskConfiguration)>(_serializer);
        }

        public JObject Serialize(string taskName, TaskConfiguration configuration)
        {
            return JObject.FromObject((taskName, configuration), _serializer);
        }
    }
}