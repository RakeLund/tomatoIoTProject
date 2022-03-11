using System;

namespace Storage
{
    public class TableData
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double LightIntensity { get; set; }
        public DateTime IoTHubEnqueuedTime { get; set; }
    }
}