using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web;

namespace JamPaul.Toolbox.Conectify
{
    public sealed class Instance : IDisposable
    {
        /// <summary>
        /// Identification of the instance
        /// </summary>
        public Guid Guid { get; private set; } = Guid.NewGuid();
        #region Events
        public delegate void ConectifyStringEventArg(Instance sender, string arg);
        public delegate void ConectifyExceptionEventArg(Instance sender, Exception exception);
        public delegate Response ConectifyEndpointCallback(Instance instance, Request request);
        /// <summary>
        /// Event that fires when the HTPP Listener starts
        /// </summary>
        public event ConectifyStringEventArg OnStart;
        /// <summary>
        /// Event the fires when the HTTP Listener stops
        /// </summary>
        public event ConectifyStringEventArg OnStop;
        /// <summary>
        /// Events that fires when the HTTP Listener errors
        /// </summary>
        public event ConectifyExceptionEventArg OnError;
        #endregion

        private int port { get; set; }
        private CancellationTokenSource tokenSource { get; set; }
        private Thread thread { get; set; }
        private string host { get; set; }

        private List<Endpoint> endpoints;
        /// <summary>
        /// Create a new Instance of Conectify
        /// </summary>
        /// <param name="port">Port that the listener will listen to</param>
        /// <param name="host">The host IP that the listener will listen to</param>
        public Instance(int port, string host = "http://localhost")
        {
            this.port = port;
            this.host = host;
            this.tokenSource = new CancellationTokenSource();
            this.thread = new Thread(new ThreadStart(this.Run));
            this.endpoints = new List<Endpoint>();
        }
        /// <summary>
        /// Add a new endpoint listener
        /// </summary>
        /// <param name="endpoint">The endpoint configuration</param>
        /// <param name="callback">The callback function</param>
        /// <returns></returns>
        public bool AddEndpoint(Endpoint endpoint, ConectifyEndpointCallback callback)
        {
            if (this.thread.IsAlive) return false;
            if (endpoint == null) return false;
            if (this.endpoints.Any(x => x.Path == endpoint.Path && x.HttpMethod == endpoint.HttpMethod)) return false;
            if (callback == null) return false;
            endpoint.Callback = callback;
            this.endpoints.Add(endpoint);
            return true;
        }
        /// <summary>
        /// Start the HTTP Listener
        /// </summary>
        /// <returns>True if the listener started, false if it was already running</returns>
        public bool Start()
        {
            if (this.thread.IsAlive) return false;
            this.thread.Start();
            return true;
        }
        /// <summary>
        /// Stop the HTTP Listener
        /// </summary>
        /// <returns>True if the listener stopped, false if it was already stopped</returns>
        public bool Stop()
        {
            if (!this.thread.IsAlive) return false;
            this.tokenSource.Cancel();
            return true;

        }
        private async void Run()
        {
            this.OnStart?.Invoke(this, $"Instance started on port {port} at : {DateTime.Now}");
            var listener = new HttpListener();
            listener.Prefixes.Add($"{this.host}:{port}/");
            listener.Start();
            while (!this.tokenSource.IsCancellationRequested)
            {
                try
                {
                    var ctx = listener.GetContext();
                    using HttpListenerResponse resp = ctx.Response;
                    resp.Headers.Set("Content-Type", "text/plain");

                    var path = ctx.Request.Url.LocalPath;

                    var done = true;
                    Response response = new Response(404, "Not found", ResponseType.Text);
                    HttpMethod method = Instance.GetMethod(ctx.Request);
                    var endpoint = this.endpoints.FirstOrDefault(x => x.Path == path && x.HttpMethod == method);
                    if (endpoint != null)
                    {
                        var query = new Dictionary<string, string>();
                        foreach (string key in ctx.Request.QueryString.AllKeys)
                        {
                            if (query.ContainsKey(key)) query.Remove(key);
                            query.Add(key, ctx.Request.QueryString[key]);
                        }
                        var headers = new Dictionary<string, string>();
                        foreach (var x in ctx.Request.Headers.AllKeys)
                        {
                            if (headers.ContainsKey(x)) headers.Remove(x);
                            headers.Add(x, ctx.Request.Headers[x]);
                        }
                        string body = Instance.getRequestBody(ctx.Request);
                        response = endpoint.Callback(this,
                            new Request(
                                this.endpoints.FirstOrDefault(x => x.Path == path),
                                query,
                                headers,
                                body));
                    }
                    var buffer = Encoding.UTF8.GetBytes(response.message);
                    resp.StatusCode = response.code;
                    resp.Headers[HttpResponseHeader.ContentType] =
                        response.type == ResponseType.JSON ? "application/json" :
                        response.type == ResponseType.HTML ? "text/html" : "text/plain";
                    resp.OutputStream.Write(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    this.OnError?.Invoke(this, ex);
                }


            }
            this.OnStop?.Invoke(this, $"Instance stopped on port {port} at : {DateTime.Now}");
        }
        private static string getRequestBody(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                return null;
            }
            using (System.IO.Stream body = request.InputStream) // here we have data
            {
                using (var reader = new System.IO.StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        private static HttpMethod GetMethod(HttpListenerRequest request)
        {
            if (request.HttpMethod == null) return HttpMethod.Get;
            if (request.HttpMethod == "GET") return HttpMethod.Get;
            if (request.HttpMethod == "POST") return HttpMethod.Post;
            if (request.HttpMethod == "DELETE") return HttpMethod.Delete;
            if (request.HttpMethod == "PUT") return HttpMethod.Put;
            if (request.HttpMethod == "HEAD") return HttpMethod.Head;
            if (request.HttpMethod == "OPTIONS") return HttpMethod.Options;
            if (request.HttpMethod == "TRACE") return HttpMethod.Trace;
            return HttpMethod.Get;
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
