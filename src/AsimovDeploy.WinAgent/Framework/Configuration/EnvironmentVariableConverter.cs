using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;

namespace AsimovDeploy.WinAgent.Framework.Configuration
{
    public class EnvironmentVariableConverter : JsonConverter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(EnvironmentVariableConverter));
        private static readonly Regex EnvVarRegex = new Regex(@"\$\{([^}]+)\}", RegexOptions.Compiled);
        private readonly IEnvironmentVariableProvider _environmentVariableProvider;

        public EnvironmentVariableConverter(IEnvironmentVariableProvider environmentVariableProvider = null)
        {
            _environmentVariableProvider = environmentVariableProvider ?? new DefaultEnvironmentVariableProvider();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var processedToken = SubstituteEnvironmentVariables(token);
            
            // Use a new serializer without this converter to avoid recursion
            var newSerializer = new JsonSerializer();
            foreach (var converter in serializer.Converters)
            {
                if (converter.GetType() != typeof(EnvironmentVariableConverter))
                {
                    newSerializer.Converters.Add(converter);
                }
            }
            
            return processedToken.ToObject(objectType, newSerializer);
        }

        private JToken SubstituteEnvironmentVariables(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.String:
                {
                    var value = token.Value<string>();
                    return new JValue(SubstituteEnvironmentVariables(value));
                }
                case JTokenType.Object:
                {
                    var obj = new JObject();
                    foreach (var property in token.Children<JProperty>())
                    {
                        obj[property.Name] = SubstituteEnvironmentVariables(property.Value);
                    }
                    return obj;
                }
                case JTokenType.Array:
                {
                    var array = new JArray();
                    foreach (var item in token)
                    {
                        array.Add(SubstituteEnvironmentVariables(item));
                    }
                    return array;
                }
                default:
                    return token;
            }
        }

        private string SubstituteEnvironmentVariables(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return EnvVarRegex.Replace(value, match =>
            {
                var envVarName = match.Groups[1].Value;
                var envVarValue = _environmentVariableProvider.GetEnvironmentVariable(envVarName);
                
                if (envVarValue == null)
                {
                    Log.Warn($"Environment variable '{envVarName}' not found");
                    return match.Value;
                }
                
                return envVarValue;
            });
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
} 