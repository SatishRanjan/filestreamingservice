using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StreamingService
{
    public class StreamingWebServer
    {
        private readonly int _serverPort = 8088;
        private Socket? _serverSocket;
        private string _endOfDataDelimiter = "\r\n";

        public async Task StartServer()
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new(IPAddress.Any, _serverPort);

            _serverSocket.Bind(endPoint);
            _serverSocket.Listen(1000);

            Console.WriteLine($"Streaming server started listening for the client connection on server port: {_serverPort}");

            while (true)
            {
                Socket clientSocket = await _serverSocket.AcceptAsync();
                Console.WriteLine($"A new client {((IPEndPoint)clientSocket.RemoteEndPoint)?.Address.ToString()} is connected at {DateTime.UtcNow.ToString("O")}");

                // Send string byte streams
                //clientSocket.Send(Encoding.UTF8.GetBytes("Hello from local streaming server"));

                // Send bytes as http response
                // clientSocket.Send(Encoding.UTF8.GetBytes(GetHttpResponseString(200, "OK", "Hello from local streaming server", "text/html")));

                // send the file on a seperate thread
                _ = Task.Run(() => SendFileStream(clientSocket));
            }
        }

        private void SendFileStream(Socket clientSocket)
        {
            string requestedFileName = GetTheRequestedFileName(clientSocket);
            Console.WriteLine($"requested filename: {requestedFileName}");
         
            var sourceFilePath = GetFileToDownloadPath(requestedFileName);

            if (string.IsNullOrEmpty(requestedFileName) || !File.Exists(sourceFilePath))
            {
                Console.WriteLine($"The source file doesn't exist, at the file path: {sourceFilePath}");
                clientSocket.Send(Encoding.UTF8.GetBytes("The requested file doesn't exists on the server."));
                clientSocket?.Close();
                return;
            }

            try
            {
                Stopwatch sw = new();
                sw.Start();
                using (FileStream sourceFs = new(sourceFilePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[2048];
                    int bytesRead;
                    while ((bytesRead = sourceFs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        clientSocket.Send(buffer, 0, bytesRead, SocketFlags.None);
                    }
                }

                sw.Stop();
                clientSocket?.Close();
                Console.WriteLine($"Successfully sent the file stream, total time taken={sw.ElapsedMilliseconds} milliseconds");
            }
            catch (Exception ex)
            {
                clientSocket?.Close();
                Console.WriteLine("File copy error: " + ex.ToString());
            }
        }

        private string GetTheRequestedFileName(Socket clientSocket)
        {
            StringBuilder sb = new();
            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = clientSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None)) > 0)
            {
                sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                // Stop reading if the end of data delimiter has been received
                if (sb.ToString().EndsWith(_endOfDataDelimiter))
                {
                    break;
                }
            }

            string receivedString = sb.ToString().TrimEnd(_endOfDataDelimiter.ToCharArray());
            return receivedString;
        }

        private static string GetHttpResponseString(int statusCode, string statusName, string content, string contentType)
        {
            string currentDateTime = DateTime.UtcNow.ToString();
            string statusHeader = "HTTP/1.1" + " " + statusCode + " " + statusName + "\r\n";
            string dateHeader = "Date: " + currentDateTime + "\r\n";
            string serverHeader = "Server: Local Streaming Server" + "\r\n";

            string responseStr = statusHeader + dateHeader + serverHeader;
            if (string.IsNullOrEmpty(content))
            {
                return responseStr + "\r\n";
            }

            string contentTypeHeader = $"Content-Type: {contentType}\r\n";
            string contentLengthHeader = "Content-Length: " + content.Length + "\r\n\r\n";
            responseStr = responseStr + contentTypeHeader + contentLengthHeader;
            return responseStr + content;
        }

        private static string GetFileToDownloadPath(string fileToDownload)
        {
            // Get the directory name of executing assembly
            string? currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            while (currentDirectory != null && new DirectoryInfo(currentDirectory).Name != "StreamingService")
            {
                // Traverse up one level
                currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            }
          
            var result = Path.Combine(currentDirectory, "samplefiles", fileToDownload);
            return result;
        }
    }
}
