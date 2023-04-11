#if NETFULL
using System.IO;
using System.Web;

namespace QoDL.Toolkit.WebUI.Models
{
    /// <summary>
    /// Memory version of HttpPostedFileBase.
    /// </summary>
    public class MemoryFile : HttpPostedFileBase
    {
        readonly Stream stream;
        readonly string contentType;
        readonly string fileName;

        /// <summary>
        /// Memory version of HttpPostedFileBase.
        /// </summary>
        public MemoryFile(Stream stream, string contentType, string fileName)
        {
            this.stream = stream;
            this.contentType = contentType;
            this.fileName = fileName;
        }

        /// <summary>
        /// Length of file.
        /// </summary>
        public override int ContentLength
        {
            get { return (int)stream.Length; }
        }

        /// <summary>
        /// Type of file.
        /// </summary>
        public override string ContentType
        {
            get { return contentType; }
        }

        /// <summary>
        /// Name of file.
        /// </summary>
        public override string FileName
        {
            get { return fileName; }
        }

        /// <summary>
        /// The file stream.
        /// </summary>
        public override Stream InputStream
        {
            get { return stream; }
        }

        /// <summary>
        /// Save the file to disk.
        /// </summary>
        public override void SaveAs(string filename)
        {
            using var file = File.Open(filename, FileMode.CreateNew);
            stream.CopyTo(file);
        }
    }
}
#endif
