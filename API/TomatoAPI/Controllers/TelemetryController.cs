using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TomatoAPI.Models;

namespace TomatoAPI.Controllers
{

    internal class TelemetryController
    {
        private readonly ILogger<TelemetryController> _logger;

        public TelemetryController(ILogger<TelemetryController> log)
        {
            _logger = log;
        }

        [FunctionName("GetData")]
        [OpenApiOperation(operationId: "GetData", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "plantId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The **plantId**")]
        [OpenApiParameter(name: "rows", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The number of rows you want. If not specified, only the last created record is returned")]
        [OpenApiParameter(name: "startTime", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The start time of the result you want. If not specified the last record is returned")]
        [OpenApiParameter(name: "endTime", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The end time of the result you want. If not specified the current time is used")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> GetData(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "telemetry/{plantId}")] HttpRequest req, string plantId,
           [Table("telemetry", Connection = "DataStorageEndpoint")] TableClient tableClient)
        {
            string queryFilter;
            IEnumerable<TableData> queryResult;
            int takeRows;
            bool rowsRequested = false;
            bool timeSpanRequested = false;

            if (req.Query.TryGetValue("rows", out var rows))
            {
                rowsRequested = true;
                if (!int.TryParse(rows, out takeRows))
                {
                    string errorMessage = "The given parameter \"rows\" is not a valid integer";
                    _logger.LogWarning(errorMessage);
                    return new BadRequestObjectResult(errorMessage);
                }
            }
            else
            {
                takeRows = 1;
            }
            

            // if startTime is given, filter on time
            if (req.Query.TryGetValue("startTime", out var startTime) && DateTime.TryParse(startTime, out DateTime start))
            {
                if (req.Query.TryGetValue("endTime", out var endTime) && DateTime.TryParse(endTime, out DateTime end))
                {
                    queryFilter = TableClient.CreateQueryFilter<TableData>(t => t.PartitionKey == plantId && t.IoTHubEnqueuedTime > start && t.IoTHubEnqueuedTime < end);
                }
                else
                {
                    queryFilter = TableClient.CreateQueryFilter<TableData>(t => t.PartitionKey == plantId && t.IoTHubEnqueuedTime > start);
                }
                timeSpanRequested = true;
            }
            else // if startTime is not given, filter only on plantId(PartitionKey)
            {
                queryFilter = TableClient.CreateQueryFilter<TableData>(t => t.PartitionKey == plantId);
                
            }
            if (!rowsRequested && timeSpanRequested) // no row parameter given
            {
                queryResult = tableClient.Query<TableData>(filter: queryFilter);
            }
            else
            {
                queryResult = tableClient.Query<TableData>(filter: queryFilter).Take(takeRows);
            }

            return new OkObjectResult(queryResult);
        }
    }
}
