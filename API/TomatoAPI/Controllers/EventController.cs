using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using TomatoAPI.Models;
using System.Globalization;

namespace TomatoAPI
{
    public class EventController
    {
        private readonly ILogger<EventController> _logger;

        public EventController(ILogger<EventController> log)
        {
            _logger = log;
        }

        [FunctionName("WaterPlant")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "plantId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **plant id**")]
        [OpenApiParameter(name: "mililiter", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The amount of water given")]
        [OpenApiParameter(name: "timeStampUTC", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The **TimeStamp** of this event in UTC. If not specified, the current time is used.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string), Description = "One or more of the parameters had the wrong format")]
        public async Task<IActionResult> WaterPlant(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "events/water")] HttpRequest req,
            [Table("water")] TableClient tableClient)
        {           
            string mililiter = req.Query["mililiter"];
            string timeStamp = req.Query["timeStamp"];
            string plantId = req.Query["plantId"];

            var watering = new Watering
            {
                PartitionKey = plantId,
                WaterMilliliters = mililiter,
            };
            if (timeStamp != null && timeStamp != "")
            {
                DateTime parsedTimeStamp;
                if (DateTime.TryParse(timeStamp, out parsedTimeStamp))
                {
                    watering.RowKey = parsedTimeStamp.ToString("O");
                }
                else
                {
                    var errormessage = $"The given timestamp {timeStamp} is not valid";
                    _logger.LogWarning(errormessage);
                    return new BadRequestObjectResult(errormessage);
                }               
            }
            else
            {
                watering.RowKey = DateTime.UtcNow.ToString("O");
            }
            try
            {
                await tableClient.AddEntityAsync(watering);
                return new OkResult();
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            
        }

        [FunctionName("HarvestTomatoes")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "plantId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **plantId**")]
        [OpenApiParameter(name: "weight", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The weight of the harvested tomato in g")]
        [OpenApiParameter(name: "timeStamp", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The **time** of harvest. If not specified, the current time is used.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> HarvestTomatoes(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "events/harvest")] HttpRequest req,
            [Table("harvest")] TableClient tableClient)
        {
            string plantId = req.Query["plantId"];
            string weight = req.Query["weight"];
            string timeStamp = req.Query["timeStamp"];
            

            var harvest = new Harvest
            {
                PartitionKey = plantId,
                Weight = weight,
            };
            if (timeStamp != null && timeStamp != "")
            {
                DateTime parsedTimeStamp;
                if (DateTime.TryParse(timeStamp, out parsedTimeStamp))
                {
                    harvest.RowKey = parsedTimeStamp.ToString("O");
                }
                else
                {
                    var errormessage = $"The given timestamp {timeStamp} is not valid";
                    _logger.LogWarning(errormessage);
                    return new BadRequestObjectResult(errormessage);
                }
            }
            else
            {
                harvest.RowKey = DateTime.UtcNow.ToString("O");
            }
            try
            {
                await tableClient.AddEntityAsync(harvest);
                return new OkResult();
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}

