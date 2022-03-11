using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventHubs;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System;

namespace Storage
{
    public class Data2Storage
    {
        [FunctionName("Data2Storage")]
        [return: Table("tomatoes")]
        public static TableData Run([IoTHubTrigger("messages/events", Connection = "IoTHubEndpoint")] EventData message, ILogger log)
        {
            log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");
            var telemetry = JsonSerializer.Deserialize<Telemetry>(message.Body);
            return new TableData
            {
                PartitionKey = message.SystemProperties["iothub-connection-device-id"].ToString(),
                RowKey = string.Format("{0:D19}",DateTime.MaxValue.Ticks - message.SystemProperties.EnqueuedTimeUtc.Ticks),
                Temperature = telemetry.Temperature,
                Humidity = telemetry.Humidity,
                LightIntensity = telemetry.LightIntensity,
                IoTHubEnqueuedTime = message.SystemProperties.EnqueuedTimeUtc
            };
        }
    }
}