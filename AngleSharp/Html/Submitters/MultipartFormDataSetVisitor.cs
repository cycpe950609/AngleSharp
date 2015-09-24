﻿namespace AngleSharp.Html.Submitters
{
    using AngleSharp.Dom.Io;
    using AngleSharp.Extensions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    sealed class MultipartFormDataSetVisitor : IFormSubmitter
    {
        readonly Encoding _encoding;
        readonly List<Action<StreamWriter>> _writers;
        readonly String _boundary;

        public MultipartFormDataSetVisitor(Encoding encoding, String boundary)
        {
            _encoding = encoding;
            _writers = new List<Action<StreamWriter>>();
            _boundary = boundary;
        }

        public void Text(FormDataSetEntry entry, String value)
        {
            if (entry.HasName && value != null)
            {
                _writers.Add(stream =>
                {
                    stream.WriteLine(String.Concat("Content-Disposition: form-data; name=\"", entry.Name.HtmlEncode(_encoding), "\""));
                    stream.WriteLine();
                    stream.WriteLine(value.HtmlEncode(_encoding));
                });
            }
        }

        public void File(FormDataSetEntry entry, String fileName, String contentType, IFile content)
        {
            if (entry.HasName)
            {
                _writers.Add(stream =>
                {
                    var hasContent = content != null && content.Name != null && content.Type != null && content.Body != null;

                    stream.WriteLine("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"",
                        entry.Name.HtmlEncode(_encoding), fileName.HtmlEncode(_encoding));

                    stream.WriteLine("Content-Type: " + contentType);
                    stream.WriteLine();

                    if (hasContent)
                    {
                        stream.Flush();
                        content.Body.CopyTo(stream.BaseStream);
                    }

                    stream.WriteLine();
                });
            }
        }

        public void Serialize(StreamWriter stream)
        {
            foreach (var writer in _writers)
            {
                stream.Write("--");
                stream.WriteLine(_boundary);
                writer(stream);
            }

            stream.Write("--");
            stream.Write(_boundary);
            stream.Write("--");
        }
    }
}
