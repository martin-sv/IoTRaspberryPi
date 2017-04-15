using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace IoTRaspberryPi
{
    class WebServer
    {
        public delegate void NewMessageEventHandler(WebServer sender, string message);
        public event NewMessageEventHandler NewMessage;
        string message;

        public WebServer()
        {

        }

        public void StartServer()
        {
            while (Start() == true)
            { }
        }

        private const uint BufferSize = 8192;
        private bool Start()
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

                    //string query = GetQuery(request);
                    GetQuery(request);

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
            catch (Exception)
            {
                return false;
            }
        }

        private void GetQuery(StringBuilder request)
        {
            var requestLines = request.ToString().Split(' ');

            var url = requestLines.Length > 1
                              ? requestLines[1] : string.Empty;

            //var uri = new Uri("http://localhost" + url);
            //var query = uri.Query;
            //return query;
            string text = url.ToString();
            text = text.Substring(text.IndexOf('=') + 1);
            message = request.ToString();

            if (message.IndexOf("Message=") != -1)
            {
                message = message.Substring(message.IndexOf("Message=") + 8);
                message = message.Trim();
                message = message.Trim('\0');
                NewMessage?.Invoke(this, message.ToString());
                Debug.WriteLine("Message: {0}", message);
            }
        }
    }
}
