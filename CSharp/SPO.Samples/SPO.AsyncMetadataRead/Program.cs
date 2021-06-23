using Azure.Storage.Blobs;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.SharePoint.Client;
using SPO.AsyncMetadataRead.Helpers;
using System;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SPO.AsyncMetadataRead
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            Uri site = new Uri(config["SiteUrl"]);
            string listUrl = config["ListUrl"];
            string clientId = config["ClientId"];
            string containerName = config["ContainerName"];
            string queueName = config["QueueName"];
            string user = config["UserName"];
            SecureString password = Utilities.ConvertToSecureString(config["Password"]);            
            string azStorageConnectionString = config["StorageConnectionString"];

            // Note: The PnP Sites Core AuthenticationManager class also supports this
            using (var authenticationManager = new UserAuthenticationManager())
            using (var context = authenticationManager.GetContext(site, user, password, clientId))
            {
                context.Load(context.Web);
                await context.ExecuteQueryAsync();
                Console.WriteLine($"Title: {context.Web.Title}");

                var azManifestContainer = await AzureStorageHelper.CreateContainerAsync(azStorageConnectionString, containerName).ConfigureAwait(true);
                var azReportQueue = await AzureStorageHelper.CreateQueueAsync(azStorageConnectionString, queueName).ConfigureAwait(true);

                string azManifestContainerUrl = AzureStorageHelper.GetServiceSasUriForContainer(azManifestContainer);
                string azReportQueueUrl = AzureStorageHelper.GetServiceSasUriForQueue(azReportQueue);

                var output = context.Site.CreateSPAsyncReadJob(
                    listUrl,
                    new AsyncReadOptions
                    {
                        IncludeDirectDescendantsOnly = false,
                        IncludeSecurity = true,
                    },
                    null,
                    azManifestContainerUrl,
                    azReportQueueUrl);
                context.ExecuteQuery();

                QueueMessage message;
                do
                {
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                    message = azReportQueue.ReceiveMessage();

                    if (message != null)
                    {
                        Console.WriteLine(Utilities.Base64Decode(message.MessageText));
                        azReportQueue.DeleteMessage(message.MessageId, message.PopReceipt);
                    }
                } while (message != null);
            }
            Console.WriteLine("Operations Completed");
        }
    }
}
