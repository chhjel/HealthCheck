using EPiServer.Framework.Blobs;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace HealthCheck.Episerver.Extensions
{
    /// <summary>
    /// Extensions for blob related things.
    /// </summary>
    public static class BlobExtensions
    {
        /// <summary>
        /// Checks if the given blob exists.
        /// <para>For FileBlobs file path will be checked, for other blobs OpenRead will be attempted called.</para>
        /// </summary>
        public static bool Exists(this Blob blob)
        {
            try
            {
                if (blob is FileBlob fileBlob)
                {
                    return File.Exists(fileBlob.FilePath);
                }
                else
                {
                    using var stream = blob.OpenRead();
                    return true;
                }
            } catch(Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// Returns null if <c>ReadAllBytes</c> throws an exception.
        /// <para>Returns null at once if blob is null, or if it's a FileBlob without an existing file.</para>
        /// </summary>
        public static byte[] TryReadAllBytes(this Blob blob)
        {
            try
            {
                if (blob == null)
                {
                    return null;
                }
                else if (blob is FileBlob file && !File.Exists(file.FilePath))
                {
                    return null;
                }
                return blob.ReadAllBytes();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
