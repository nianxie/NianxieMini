using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace Nianxie.Riff
{
    public class JsonCodec
    {
        private class TypeBinder: ISerializationBinder
        {
            private readonly Dictionary<string, Type> _typeMappings;
            private readonly Dictionary<Type, string> _reverseMappings;
            public TypeBinder(Dictionary<string, Type> typeMap)
            {
                _typeMappings = typeMap;
                _reverseMappings = _typeMappings.ToDictionary(pair => pair.Value, pair => pair.Key);
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
        private class Factory
        {
            private Func<AbstractRiffJson> ctor;
            private Version version;
            public readonly JsonSerializerSettings dumpSettings;
            public readonly JsonSerializer serializer;
            public Factory(AbstractRiffJson empty, Func<AbstractRiffJson> ctor, Dictionary<string, Type> typeMap) 
            {
                this.ctor = ctor;
                this.version = Version.Parse(empty.version);
                dumpSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    SerializationBinder = typeMap == null ? null : new TypeBinder(typeMap),
                    Formatting = Formatting.None,
                    Converters = new JsonConverter[]
                    {
                        new Vector2IntConverter(),
                    }
                };
                serializer = JsonSerializer.CreateDefault(dumpSettings);
            }

            public AbstractRiffJson Build(Version versionInJson)
            {
                if (version != versionInJson)
                {
                    throw new NotImplementedException("TODO version compatible");
                }
                return ctor();
            }
        }

        private static Dictionary<string, Factory> kindToFactory = new();
        private static Dictionary<Type, Factory> typeToFactory = new();
        private static JsonSerializerSettings loadSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            Converters = new JsonConverter[]
            {
                new RiffJsonConverter(),
            }
        };
        public static void RegisterFactory<TRiffJson>(Dictionary<string, Type> innerTypeMap=null) where TRiffJson:AbstractRiffJson, new()
        {
            var empty = new TRiffJson();
            if (kindToFactory.ContainsKey(empty.kind))
            {
                throw new Exception($"kind={empty.kind} is registered in json converter");
            }

            var type = typeof(TRiffJson);
            if (typeToFactory.ContainsKey(type))
            {
                throw new Exception($"type={type} is registered in json converter");
            }

            var factory = new Factory(empty, () => new TRiffJson(), innerTypeMap);
            kindToFactory[empty.kind] = factory;
            typeToFactory[type] = factory;
        }
        private class RiffJsonConverter : JsonConverter<AbstractRiffJson>
        {

            public override AbstractRiffJson ReadJson(JsonReader reader, Type objectType, AbstractRiffJson existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                // 1. 将 JSON 加载为 JObject
                JObject jo = JObject.Load(reader);

                // 2. 读取判别器字段
                string kind = jo[nameof(AbstractRiffJson.kind)]!.Value<string>();
                string version = jo[nameof(AbstractRiffJson.version)]!.Value<string>();

                // 3. 根据 kind 决定实例化哪个子类
                var factory = kindToFactory[kind];
                AbstractRiffJson target = factory.Build(Version.Parse(version));

                // 4. 将剩余属性填充到实例中
                factory.serializer.Populate(jo.CreateReader(), target);
                return target;
            }

            public override bool CanWrite => false;

            public override void WriteJson(JsonWriter writer, AbstractRiffJson value, JsonSerializer _)
            {
                throw new NotImplementedException("Write Json not implement in converter");
            }
        }

        public static string Dump(AbstractRiffJson json)
        {
            var factory = kindToFactory[json.kind];
            return JsonConvert.SerializeObject(json, factory.dumpSettings);
        }

        public static TRiffJson Load<TRiffJson>(string jsonStr) where TRiffJson:AbstractRiffJson
        {
            return JsonConvert.DeserializeObject<TRiffJson>(jsonStr, loadSettings);
        }
    }
}