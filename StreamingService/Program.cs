using System.Diagnostics;

namespace StreamingService
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            StreamingWebServer streamingService = new();
            await streamingService.StartServer();
        }
    }
}
