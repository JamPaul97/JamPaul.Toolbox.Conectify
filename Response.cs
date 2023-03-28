using System;
using System.Collections.Generic;
using System.Text;

namespace JamPaul.Toolbox.Conectify
{
    public enum ResponseType
    {
        Text = 1,
        HTML = 2,
        JSON = 3
    }
    public sealed class Response
    {
        internal int code { get; set; }
        internal string message { get; set; }
        public ResponseType type { get; set; }

        public Response(int code, string message, ResponseType type = ResponseType.Text)
        {
            this.code = code;
            this.message = message;
            this.type = type;
        }

    }
}
