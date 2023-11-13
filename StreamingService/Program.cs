using System.Diagnostics;

namespace StreamingService
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            StreamingWebServer streamingService = new();
            await streamingService.StartServer();

            var sourceFilePath = @"C:\myfiles\code\StreamingService\samplefiles\Sample-Video-File-For-Testing.mp4";
            var destFilePath = @"C:\myfiles\code\StreamingService\samplefiles\Copied-Sample-Video-File-For-Testing.mp4";

            if (!File.Exists(sourceFilePath))
            {
                Console.WriteLine("The source file doesn't exist.");
            }

            try
            {
                Stopwatch sw = new();
                sw.Start();
                using (FileStream sourceFs = new(sourceFilePath, FileMode.Open, FileAccess.Read))
                {
                    using (FileStream destFs = new(destFilePath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] buffer = new byte[2048];
                        int bytesRead;
                        while ((bytesRead = sourceFs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            destFs.Write(buffer, 0, bytesRead);
                        }
                    }
                }

                sw.Stop();
                Console.WriteLine($"Successfully copied the file, total time taken={sw.ElapsedMilliseconds} milliseconds");
                Console.Read();
            }
            catch (Exception ex)
            {
                Console.WriteLine("File copy error: " + ex.ToString());
            }
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
            return responseStr;
        }
    }
}