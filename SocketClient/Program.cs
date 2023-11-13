using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace SocketClient
{
    internal class Program
    {
        private static int SERVER_PORT = 8088;
        private static string SERVER_HOST = "127.0.0.1";
        private static int BUFFER_SIZE = 4096;
        private static IPEndPoint _endPoint;
        private static string _endOfDataDelimiter = "\r\n";

        static async Task Main(string[] args)
        {
            _endPoint = new(IPAddress.Parse(SERVER_HOST), SERVER_PORT);

            string fileNameToDownload = "samplefile.txt";
            if (args.Length > 0) fileNameToDownload = args[0];

            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();

            // Download the files in parallel
            var tasks = new Task[1];
            for (int i = 0; i < tasks.Length; i++)
            {
                Socket serverSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tasks[i] = Task.Run(() => DownloadFile(serverSocket, fileNameToDownload));
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);
            sw.Stop();

            Console.WriteLine($"Downaloaded all files in {sw.ElapsedMilliseconds} milliseconds, downloaded files location: {GetDestinationFilePath(fileNameToDownload)}");
            Console.Read();
        }

        private static void DownloadFile(Socket serverSocket, string fileNameToDownload)
        {
            try
            {
                serverSocket.Connect(_endPoint);
                serverSocket.Send(Encoding.UTF8.GetBytes($"{fileNameToDownload}{_endOfDataDelimiter}"));

                // File path and name where the file will be downloaded
                var destFilePath = GetDestinationFilePath(fileNameToDownload);

                using (FileStream destFs = new(destFilePath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[BUFFER_SIZE];
                    int bytesRead;
                    while ((bytesRead = serverSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None)) > 0)
                    {
                        destFs.Write(buffer, 0, bytesRead);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                serverSocket.Close();
            }
        }

        private static string GetDestinationFilePath(string fileToDownload)
        {
            // Get the directory name of executing assembly
            string? currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            while (currentDirectory != null && new DirectoryInfo(currentDirectory).Name != "SocketClient")
            {
                // Traverse up one level
                currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            }

            var fileName = Guid.NewGuid().ToString() + $"_{fileToDownload}";
            var result = Path.Combine(currentDirectory, "downloadedfiles", fileName);
            return result;
        }
    }
}