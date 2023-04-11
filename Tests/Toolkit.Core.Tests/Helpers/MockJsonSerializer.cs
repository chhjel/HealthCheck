using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Models;
using System;

namespace QoDL.Toolkit.Core.Tests.Helpers
{
    public class MockJsonSerializer : IJsonSerializer
    {
        /// <inheritdoc />
        public string LastError { get; set; }

        public string LastDeserializedJson { get; set; }
        public object LastSerializedObject { get; set; }
        public object DeserializeResult { get; }
        public string SerializeResult { get; }

        public MockJsonSerializer(object deserializeResult = null, string serializeResult = "")
        {
            DeserializeResult = deserializeResult;
            SerializeResult = serializeResult;
        }

        public object Deserialize(string json, Type type)
        {
            LastDeserializedJson = json;
            return DeserializeResult;
        }

        public T Deserialize<T>(string json)
        {
            LastDeserializedJson = json;
            return (T)DeserializeResult;
        }

        public TKGenericResult<object> DeserializeExt(string json, Type type)
        {
            LastDeserializedJson = json;
            return TKGenericResult<object>.CreateSuccess(DeserializeResult);
        }

        public string Serialize(object obj, bool pretty = true)
        {
            LastSerializedObject = obj;
            return SerializeResult;
        }
    }
}
