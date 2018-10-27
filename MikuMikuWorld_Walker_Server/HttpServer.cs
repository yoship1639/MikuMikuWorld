using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server
{
    class HttpServer
    {
        public string RootDir { get; set; }
        public int Port { get; set; }

        Task listenTask;
        HttpListener listener;

        public HttpServer(string rootDir, int port)
        {
            RootDir = rootDir;
            Port = port;
        }

        public void Start()
        {
            listenTask = Task.Factory.StartNew(() =>
            {
                listener = new HttpListener();
                listener.Prefixes.Add($"http://localhost:{Port}/");
                listener.Start();

                while (true)
                {
                    try
                    {
                        var context = listener.GetContext();

                        Task.Factory.StartNew(() =>
                        {
                            var con = context;
                            var req = con.Request;
                            var res = con.Response;

                            try
                            {
                                var urlPath = req.RawUrl;
                                if (urlPath[0] == '/' || urlPath[0] == '\\') urlPath = urlPath.Remove(0, 1);

                                // ディレクトリトラバーサル対策
                                if (urlPath.Contains("../") || urlPath.Contains("..\\"))
                                {
                                    Error(this, $"<Http> invalid argument [{req.RemoteEndPoint}] ({urlPath})");
                                    throw new ArgumentException();
                                }

                                var path = RootDir + urlPath;
                                if (!File.Exists(path))
                                {
                                    Error(this, $"<Http> file not found [{req.RemoteEndPoint}] ({urlPath})");
                                    throw new FileNotFoundException();
                                }
                                path = path.Replace("/", "\\");

                                res.StatusCode = 200;
                                byte[] content = File.ReadAllBytes(path);
                                res.OutputStream.Write(content, 0, content.Length);
                            }
                            catch (Exception e)
                            {
                                res.StatusCode = 500;
                                byte[] content = Encoding.UTF8.GetBytes(e.Message);
                                res.OutputStream.Write(content, 0, content.Length);
                            }
                        });
                    }
                    catch
                    {
                        break;
                    }
                }
            });
        }
        public void Stop()
        {
            listener.Stop();
            Thread.Sleep(100);
            listenTask.Wait();
        }

        public event EventHandler<string> Error = delegate { };
    }
}
