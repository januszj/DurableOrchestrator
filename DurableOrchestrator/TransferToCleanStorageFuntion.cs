using System;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableOrchestrator
{
    public static class TransferToCleanStorageFuntion
    {
        [FunctionName("TransferToCleanStorageFuntion")]
        public static async Task Run(
            [ActivityTrigger] string blobName,
            ILogger log)
        {
            log.LogInformation($"Transfer file to clean storage, file: {blobName}");
            var connectioString = Environment.GetEnvironmentVariable("STORAGE_TEST");
            var dirtyStorageContainer = new BlobContainerClient(
                connectioString,
                "dirty-uploads");
            
            var cleanStorageContainer = new BlobContainerClient(
                connectioString,
                "clean-storage");

            await CopyBlobAsync(dirtyStorageContainer, cleanStorageContainer, blobName, log);
        }

        private static async Task<string> CopyBlobAsync(
            BlobContainerClient dirtyStorageContainer,
            BlobContainerClient cleanStorageContainer,
            string fileName,
            ILogger log)
        {
            BlobLeaseClient lease = null;
            BlobProperties sourceProperties = null;
            try
            {
                BlobClient dirtyStorageFile = dirtyStorageContainer.GetBlobClient(fileName);
                BlobClient cleanStorageFile = cleanStorageContainer.GetBlobClient(fileName);

                if (await dirtyStorageFile.ExistsAsync())
                {
                    lease = dirtyStorageFile.GetBlobLeaseClient();
                    await lease.AcquireAsync(TimeSpan.FromSeconds(-1));

                    var publicStorageFileSasUri = dirtyStorageFile.GenerateSasUri(
                        BlobSasPermissions.Read,
                        DateTimeOffset.UtcNow.AddMinutes(20)
                    );

                    // Initiate Blob Copy from SOURCE to DESTINATION
                    await cleanStorageFile.StartCopyFromUriAsync(publicStorageFileSasUri);
                    sourceProperties = await dirtyStorageFile.GetPropertiesAsync();

                    return "OK";
                }
                return "File Not Found";
            }
            catch (RequestFailedException ex)
            {
                log.LogCritical(ex, $"Function[{nameof(CopyBlobAsync)}] FileName[{fileName}] Exception");
                throw;
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, $"Function[{nameof(CopyBlobAsync)}] FileName[{fileName}] Exception");
                throw;
            }
            finally
            {
                if (sourceProperties != null && sourceProperties.LeaseState == LeaseState.Leased)
                    await lease.BreakAsync();
            }
        }
    }
}
