using EPiServer.Framework.Blobs;
using System;
using System.IO;

namespace QoDL.Toolkit.Episerver.Tests.Helpers
{
    public class MockBlob : Blob
    {
        private string _json;

        public int OpenReadCount { get; set; }
        public int OpenWriteCount { get; set; }
        public int WriteCount { get; set; }

        public MockBlob(Uri id, string json) : base(id)
        {
            _json = json;
        }

        public override Stream OpenRead()
        {
            OpenReadCount++;
            return GenerateStreamFromString(_json);
        }

        public override Stream OpenWrite()
        {
            OpenWriteCount++;
            return new MemoryStream();
        }

        public override void Write(Stream data)
        {
            WriteCount++;
            var reader = new StreamReader(data);
            _json = reader.ReadToEnd();
        }

        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
