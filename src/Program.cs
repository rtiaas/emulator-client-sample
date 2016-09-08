namespace EmulatorClientSample
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;

    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var authUrl = config["authUrl"];            // the url to the authorization token service
            var authScopes = config["authScopes"];      // a list of required scopes will be provided
            var clientId = config["clientId"];          // the id is assigned to individual clients wishing to access the service
            var clientSecret = config["clientSecret"];  // the secret is never be embedded in code, but securely stored on the client machine with access restricted to the application only

            // the url of the activity service
            var activityBaseUrl = config["activityBaseUrl"];
            var activityServiceUrl = $"{activityBaseUrl}/{clientId}";

            Console.WriteLine($"Calling authorization service at {authUrl}");

            var authHelper = new AuthHelper(authUrl);
            var token = authHelper.GetAuthToken(clientId, clientSecret, authScopes);

            Console.WriteLine("Authorization token received, connecting to activity service");

            var client = new Client(activityServiceUrl);
            client.Connect(token.AccessToken).Wait();

            Console.WriteLine("Connected..waiting for messages..");
            Console.WriteLine("(press ctrl+c to quit)");

            while (true)
            {
                string message;
                while (!client.TryGetReceivedMessage(out message))
                {
                    Task.Delay(100);
                }

                Console.WriteLine("Received message:");
                Console.WriteLine(message);
                Console.WriteLine();

                Console.WriteLine("Sending response");

                // extract the activity id
                var match = Regex.Match(message, @"RemoteActivity-ID: \b[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b", RegexOptions.IgnoreCase);
                var activityId = match.Value.Replace("RemoteActivity-ID: ", String.Empty);

                var responseMessage = BuildResponseMessage(activityId, "Responding to your message, thanks!");

                client.Send(responseMessage);
            }
        }

        private static byte[] BuildResponseMessage(string activityId, string data)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);

            var sb = new StringBuilder();
            sb.AppendLine("RAF/1.0 200 OK");
            sb.AppendLine("Content-Type: text/plain");
            sb.AppendLine($"Content-Length: {dataBytes.Length}");
            sb.AppendLine($"RemoteActivity-ID: {activityId}");
            sb.AppendLine();

            var headerData = Encoding.UTF8.GetBytes(sb.ToString());

            var ms = new MemoryStream();
            ms.Write(headerData, 0, headerData.Length);
            ms.Write(dataBytes, 0, dataBytes.Length);

            return ms.ToArray();
        }
    }
}
