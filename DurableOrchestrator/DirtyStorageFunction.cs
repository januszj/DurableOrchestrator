using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableOrchestrator
{
    public class DirtyStorageFunction
    {
        [FunctionName("DirtyStorageFunction")]
        public async Task RunAsync([BlobTrigger("dirty-uploads/{name}", Connection = "STORAGE_TEST")]
            Stream myBlob, string name,
            [DurableClient] IDurableOrchestrationClient orchestrationClient,
            ILogger log
            )
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            string instanceId = await orchestrationClient.StartNewAsync(nameof(DurableOrchestrator), null, name);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
        }
    }
}
