using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;

namespace TomatoAPI.Models
{
    public class TableData : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double LightIntensity { get; set; }
        public DateTime IoTHubEnqueuedTime { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
