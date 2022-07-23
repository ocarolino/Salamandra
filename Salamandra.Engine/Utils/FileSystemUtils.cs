using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Utils
{
    public static class FileSystemUtils
    {
        public static string GetApplicationCurrentDirectory()
        {
            return AppContext.BaseDirectory;
        }

        // Source: https://stackoverflow.com/questions/1025332/determine-a-strings-encoding-in-c-sharp (2022-07-23 - Berthier Lemieux)
        public static Encoding DetectFileEncoding(Stream fileStream)
        {
            var Utf8EncodingVerifier = Encoding.GetEncoding("utf-8", new EncoderExceptionFallback(), new DecoderExceptionFallback());

            using (var reader = new StreamReader(fileStream, Utf8EncodingVerifier,
                   detectEncodingFromByteOrderMarks: true, leaveOpen: true, bufferSize: 1024))
            {
                string detectedEncoding;

                try
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                    }

                    detectedEncoding = reader.CurrentEncoding.BodyName;
                }
                catch (Exception e)
                {
                    // Failed to decode the file using the BOM/UT8. 
                    // Assume it's local ANSI
                    detectedEncoding = "ISO-8859-1";
                }

                // Rewind the stream
                fileStream.Seek(0, SeekOrigin.Begin);

                return Encoding.GetEncoding(detectedEncoding);
            }
        }
    }
}
