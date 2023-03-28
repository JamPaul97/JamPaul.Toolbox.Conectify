using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace JamPaul.Toolbox.Conectify
{
    public class Endpoint
    {
        public HttpMethod HttpMethod { get; private set; }
        public string Path { get; private set; }
        public bool CaptureHeaders { get;private set; }
        public bool CaptureQuery { get; private set; }
        public Instance.ConectifyEndpointCallback Callback { get; internal set; }
        public Endpoint(
            HttpMethod httpMethod, string path,
            bool captureHeaders = false, bool captureQuery = true)
        {
            HttpMethod = httpMethod;
            Path = path;
            CaptureHeaders = captureHeaders;
            CaptureQuery = captureQuery;
        }
    }
}
