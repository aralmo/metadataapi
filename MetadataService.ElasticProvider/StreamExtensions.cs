﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetadataService
{
    public static class StreamExtensions
    {
        public static string ReadToEnd(this Stream stream)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static void Write(this Stream stream, string content, Encoding encoding)
        {
            stream.Write(new ReadOnlySpan<byte>(encoding.GetBytes(content)));
        }
    }
}
