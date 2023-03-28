using System;
using System.Collections.Generic;
using System.Text;

namespace JamPaul.Toolbox.Conectify
{
    public sealed class Request
    {
        public Endpoint Endpoint { get; internal set; }
        public Dictionary<string, string> Query { get; internal set; }
        public Dictionary<string, string> Headers { get; internal set; }
        public string Body { get; internal set; }
        internal Request(Endpoint endpoint, Dictionary<string, string> query, Dictionary<string, string> headers, string body)
        {
            Endpoint = endpoint;
            Query = query;
            Headers = headers;
            Body = body;
        }
    }
}
