using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableOrchestrator
{
    public static class DurableOrchestrator
    {
        [FunctionName("DurableOrchestrator")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var blobName = context.GetInput<string>();
            log.LogInformation($"Start processing file {blobName}.");

            var isFileClean = await context.CallActivityAsync<bool>(nameof(AntivirusScaner), blobName);

            if (isFileClean)
            {
                await context.CallActivityAsync<bool>(nameof(TransferToCleanStorageFuntion), blobName);
            }
            else
            {
                log.LogWarning($"File {blobName} contains virus deleting from dirty storage !!.");
                var connectionString = Environment.GetEnvironmentVariable("STORAGE_TEST");
                var dirtyStorageContainer = new BlobContainerClient(
                    connectionString,
                    "dirty-uploads");
                
                BlobClient dirtyStorageFile = dirtyStorageContainer.GetBlobClient(blobName);
                await dirtyStorageFile.DeleteIfExistsAsync();
            }
        }
    }
}