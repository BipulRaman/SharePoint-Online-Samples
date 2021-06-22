using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Sas;

namespace SPO.AsyncMetadataRead.Helpers
{
    public static class AzureStorageHelper
    {
        public static async Task<BlobContainerClient> CreateContainerAsync(string connectionString, string containerName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            try
            {
                // Create the container
                BlobContainerClient container = await blobServiceClient.CreateBlobContainerAsync(containerName);

                if (await container.ExistsAsync())
                {
                    Console.WriteLine("Created container {0}", container.Name);
                    return container;
                }
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine("HTTP error code {0}: {1}", e.Status, e.ErrorCode);
                if (e.Status == 409)
                {
                    return blobServiceClient.GetBlobContainerClient(containerName);
                }
                Console.WriteLine(e.Message);
            }
            return null;
        }

        public static async Task<QueueClient> CreateQueueAsync(string connectionString, string queueName)
        {
            try
            {
                // Instantiate a QueueClient which will be used to create and manipulate the queue
                QueueClient queueClient = new QueueClient(connectionString, queueName);

                // Create the queue
                var queue = queueClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();

                if (await queueClient.ExistsAsync())
                {
                    Console.WriteLine("Created queue {0}", queueClient.Name);
                    return queueClient;
                }

            }
            catch (RequestFailedException e)
            {
                Console.WriteLine("HTTP error code {0}: {1}", e.Status, e.ErrorCode);
                Console.WriteLine(e.Message);
            }

            return null;
        }

        public static string GetServiceSasUriForContainer(BlobContainerClient containerClient, string storedPolicyName = null)
        {
            // Check whether this BlobContainerClient object has been authorized with Shared Key.
            if (containerClient.CanGenerateSasUri)
            {
                // Create a SAS token that's valid for one hour.
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = containerClient.Name,
                    Resource = "c"
                };

                if (storedPolicyName == null)
                {
                    sasBuilder.StartsOn = DateTimeOffset.UtcNow.AddDays(-1);
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddDays(7);
                    sasBuilder.SetPermissions(BlobContainerSasPermissions.Write | BlobContainerSasPermissions.Read);
                }
                else
                {
                    sasBuilder.Identifier = storedPolicyName;
                }

                Uri sasUri = containerClient.GenerateSasUri(sasBuilder);
                Console.WriteLine("SAS URI for blob container is: {0}", sasUri);
                Console.WriteLine();

                return sasUri.AbsoluteUri;
            }
            else
            {
                Console.WriteLine(@"BlobContainerClient must be authorized with Shared Key 
                          credentials to create a service SAS.");
                return null;
            }
        }

        public static string GetServiceSasUriForQueue(QueueClient queueClient, string storedPolicyName = null)
        {
            // Check whether this BlobContainerClient object has been authorized with Shared Key.
            if (queueClient.CanGenerateSasUri)
            {
                // Create a SAS token that's valid for one hour.

                QueueSasBuilder sasBuilder = new QueueSasBuilder()
                {
                    QueueName = queueClient.Name                    
                };

                if (storedPolicyName == null)
                {
                    sasBuilder.StartsOn = DateTimeOffset.UtcNow.AddDays(-1);
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddDays(7);
                    sasBuilder.SetPermissions(QueueAccountSasPermissions.Add | QueueAccountSasPermissions.Read);
                }
                else
                {
                    sasBuilder.Identifier = storedPolicyName;
                }

                Uri sasUri = queueClient.GenerateSasUri(sasBuilder);
                Console.WriteLine("SAS URI for queue is: {0}", sasUri);
                Console.WriteLine();

                return sasUri.AbsoluteUri;
            }
            else
            {
                Console.WriteLine(@"BlobContainerClient must be authorized with Shared Key 
                          credentials to create a service SAS.");
                return null;
            }
        }

    }
}
