using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using Windows.Networking.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using System.IO;
using System.Threading.Tasks;
namespace WebServer
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.GetDeferral();
            WebServer server = new WebServer();
            while (server.Start() == true)
            { }
        }
    }
    internal class WebServer
    {
        private const uint BufferSize = 8192;
        public bool Start()
        {
            try
            {
                StreamSocketListener listener = new StreamSocketListener();
                listener.BindServiceNameAsync("80").AsTask();
                listener.ConnectionReceived += async (sender, args) =>
                {
                    StringBuilder request = new StringBuilder();
                    using (Windows.Storage.Streams.IInputStream input = args.Socket.InputStream)
                    {
                        byte[] data = new byte[BufferSize];
                        Windows.Storage.Streams.IBuffer buffer = data.AsBuffer();
                        uint dataRead = BufferSize;
                        while (dataRead == BufferSize)
                        {
                            await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                            request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                            dataRead = buffer.Length;
                        }
                        //In the future, maybe we parse the HTTP request and serve different HTML pages for now we just always push index.html
                    }
                    using (IOutputStream output = args.Socket.OutputStream)
                    {
                        using (System.IO.Stream response = output.AsStreamForWrite())
                        {
                            string page = "";
                            var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                            // acquire file
                            var file = await folder.GetFileAsync("index.html");
                            var readFile = await Windows.Storage.FileIO.ReadLinesAsync(file);
                            foreach (var line in readFile)
                            {
                                page += line;
                            }
                            byte[] bodyArray = Encoding.UTF8.GetBytes(page);
                            var bodyStream = new MemoryStream(bodyArray);
                            //iCount++;

                            var header = "HTTP/1.1 200 OK\r\n" +
                                        $"Content-Length: {bodyStream.Length}\r\n" +
                                            "Connection: close\r\n\r\n";
                            byte[] headerArray = Encoding.UTF8.GetBytes(header);
                            await response.WriteAsync(headerArray, 0, headerArray.Length);
                            await bodyStream.CopyToAsync(response);
                            await response.FlushAsync();
                        }
                    }
                };
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}