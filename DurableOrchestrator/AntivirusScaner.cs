using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableOrchestrator
{
    public static class AntivirusScaner
    {
        [FunctionName("AntivirusScaner")]
        public static async Task<bool> Run(
            [ActivityTrigger] string blobName,
            ILogger log)
        {
            log.LogInformation($"C# AntivirusScaner scanning file: {blobName}");

            await Task.Delay(5000);

            Random r = new Random();
            var isFileClean = r.NextBool();

            if (isFileClean)
                log.LogInformation($"File: {blobName} was clean !!");
            else
                log.LogWarning($"File: {blobName} had virus !!");

            return isFileClean;
        }
    }
}
