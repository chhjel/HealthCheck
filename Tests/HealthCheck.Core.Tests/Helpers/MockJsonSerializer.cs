﻿using HealthCheck.Core.Abstractions;
using System;

namespace HealthCheck.Core.Tests.Helpers
{
    public class MockJsonSerializer : IJsonSerializer
    {
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

        public string Serialize(object obj)
        {
            LastSerializedObject = obj;
            return SerializeResult;
        }
    }
}