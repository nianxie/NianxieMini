using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Nianxie.Riff
{
    internal interface IJsonCodec
    {
        string AbstractSerialize(AbstractRiffJson riffJson);
        AbstractRiffJson AbstractDeserialize(string jsonStr);
    }

    public class JsonCodec<TRiffJson> :IJsonCodec where TRiffJson : AbstractRiffJson
    {
        private readonly JsonSerializerSettings settings;

        protected JsonCodec(Type[] bindTypes)
        {
            settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = bindTypes == null ? null : new TypeBinder(bindTypes),
                Formatting = Formatting.Indented,
                Converters = new JsonConverter[]
                {
                    new Vector2IntConverter(),
                }
            };
        }

        public JsonCodec():this(null)
        {
        }

        private class TypeBinder: ISerializationBinder
        {
            private readonly Dictionary<string, Type> _typeMappings;
            private readonly Dictionary<Type, string> _reverseMappings;
            public TypeBinder(Type[] jsonTypes)
            {
                _typeMappings = jsonTypes.ToDictionary(type => type.Name);
                _reverseMappings = jsonTypes.ToDictionary(type => type, type=>type.Name);
            }

            public Type BindToType(string? assemblyName, string typeName)
            {
                if (_typeMappings.TryGetValue(typeName, out Type type))
                {
                    return type;
                }

                return null;
            }

            public void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                assemblyName = null;
                
                if (_reverseMappings.TryGetValue(serializedType, out string customName))
                {
                    typeName = customName;
                }
                else
                {
                    typeName = null;
                }
            }
        }
        private class Vector2IntConverter : JsonConverter<Vector2Int>
        {
            public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("x");
                writer.WriteValue(value.x);
                writer.WritePropertyName("y");
                writer.WriteValue(value.y);
                writer.WriteEndObject();
            }

            public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                int x = 0, y = 0;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        string propertyName = reader.Value.ToString();
                        reader.Read(); // Advance to the value

                        switch (propertyName)
                        {
                            case "x":
                                x = Convert.ToInt32(reader.Value);
                                break;
                            case "y":
                                y = Convert.ToInt32(reader.Value);
                                break;
                        }
                    }
                    else if (reader.TokenType == JsonToken.EndObject)
                    {
                        break;
                    }
                }
                return new Vector2Int(x, y);
            }
        }
        public string Serialize(TRiffJson riffJson)
        {
            var jsonStr = JsonConvert.SerializeObject(riffJson, settings);
            return jsonStr;
        }

        public TRiffJson Deserialize(string jsonStr)
        {
            return JsonConvert.DeserializeObject<TRiffJson>(jsonStr, settings);
        }

        string IJsonCodec.AbstractSerialize(AbstractRiffJson riffJson)
        {
            return Serialize((TRiffJson) riffJson);
        }

        AbstractRiffJson IJsonCodec.AbstractDeserialize(string jsonStr)
        {
            return Deserialize(jsonStr);
        }
    }

    public class JsonCodec<TRiffJson, TContentJson> : JsonCodec<TRiffJson> where TRiffJson : AbstractRiffJson
    {
        public JsonCodec():base(FindBindTypes())
        {
        }

        private static Type[] FindBindTypes()
        {
            var contentType = typeof(TContentJson);
            // 使用反射获取contentType同命名空间、同程序集的派生类
            var asm = AppDomain.CurrentDomain.GetAssemblies().First(asm => asm.GetType(contentType.FullName) != null);
            var jsonTypes = asm.GetTypes().Where(type => type.Namespace == contentType.Namespace && type.IsSubclassOf(contentType)).ToArray();
            return jsonTypes;
        }
    }
}