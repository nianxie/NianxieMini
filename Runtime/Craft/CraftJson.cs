using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Nianxie.Utils;
using UnityEngine;

namespace Nianxie.Craft
{
    public class CraftJson
    {
        public SlotScriptJson root;
        public Vector2Int atlasSize;
        public IntRectangle[] atlasRects;
        
        #region // static items
        private static readonly JsonSerializerSettings settings;
        private static readonly Dictionary<string, Type> _typeMappings;
        private static readonly Dictionary<Type, string> _reverseMappings;
        static CraftJson()
        {
            // 使用反射获取AbstractCraftJson同命名空间、同程序集的派生类
            var absJsonType = typeof(AbstractSlotJson);
            var craftAsm = AppDomain.CurrentDomain.GetAssemblies().First(asm => asm.GetType(absJsonType.FullName) != null);
            var jsonTypes = craftAsm.GetTypes().Where(type => type.Namespace == absJsonType.Namespace && type.IsSubclassOf(absJsonType)).ToArray();
            _typeMappings = jsonTypes.ToDictionary(type => type.Name);
            _reverseMappings = jsonTypes.ToDictionary(type => type, type=>type.Name);
            settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new TypeBinder(),
                Formatting = Formatting.Indented,
                Converters = new JsonConverter[]
                {
                    new Vector2IntConverter(),
                }
            };
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

        private class TypeBinder: ISerializationBinder
        {

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

        public LargeBytes ToLargeBytes()
        {
            var jsonStr = JsonConvert.SerializeObject(this, settings);
            return LargeBytes.FromUtf8String(jsonStr);
        }
        
        public static CraftJson FromLargeBytes(LargeBytes jsonBytes)
        {
            return JsonConvert.DeserializeObject<CraftJson>(jsonBytes.ToUtf8String(), settings);
        }
        #endregion
    }

}
