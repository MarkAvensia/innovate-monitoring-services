using inRiver.Remoting;
using inRiver.Remoting.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NitroConnector.Constants;
using NitroConnector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Avensia.Inriver.Status;
using Avensia.IM.Slack;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.IO;
using Avensia.Inriver.Status.Contract;
using Avensia.Inriver.Status.Extensions;

namespace NitroConnector.Controllers
{
    [FormatFilter]
    [Route("[controller]")]
    [ApiController]
    public class ExtensionQueueController : ControllerBase
    {
        RemoteManager manager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExtensionQueueController> _logger;
        IList<CustomerSettings> customerSettings = new List<CustomerSettings>();

        public ExtensionQueueController(IConfiguration configuration, ILogger<ExtensionQueueController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            Initialized();
        }

        // ExtensionQueue/GetStatQueues/Megaflis/Test
        [HttpGet, Route("GetStatQueues/{customerName}/{environment}")]
        public async System.Threading.Tasks.Task<IActionResult> GetStatQueuesAsync(string customerName, string environment)
        {
            List<StatQueue> stats = new List<StatQueue>();
            var customerSettings = GetCustomerSettings(customerName, environment);
            var extensionURL = _configuration.GetConnectionString("InRiverExtensionLink");

            extensionURL = extensionURL.Replace("{ENVIRONMENT_ID}", customerSettings.EnvironmentId);
            extensionURL = extensionURL.Replace("{ENVIRONMENT}", customerSettings.Environment.ToLower());

            if (customerSettings == null)
                return new JsonResult(stats);

            var IDs = customerSettings.StatQueues.Equals("default") ? GetListOfExtension(customerSettings.RestAPIKey) : CheckExtensionIfEnabled(customerSettings.StatQueues, customerSettings.RestAPIKey);

            foreach (var id in IDs)
            {
                if (string.IsNullOrEmpty(id))
                    continue;

                var result = GetStatistics(customerSettings.RestAPIKey, id);
                if (result.events.queuedEventCount > 0 || result.events.errorEventCount > 0)
                    stats.Add(new StatQueue()
                    {
                        Extension = id,
                        QueuedEventCount = result.events.queuedEventCount,
                        ErrorEventCount = result.events.errorEventCount,
                        CurrentSequenceNumber = result.events.currentSequenceNumber,
                        ExtentionUrl = extensionURL.Replace("{EXTENSION}", id)
                    });
            }

            validateDataForSlackNotification(stats, customerName);

            return new JsonResult(stats);
        }

        public void validateDataForSlackNotification(List<StatQueue> stats, string customerName)
        {
            string filePath = @"temp/" + customerName + ".txt";
            var tresHold = _configuration.GetSection("ExtensionErrorTreshold").Value;
            var message = $"<!here> \n" +
                              $"Customer Extension Updates! \n" +
                              $"*{customerName}* \n" +
                              $"-------------------------------------------- \n";
            var sendMessage = false;

            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    var dataFromFile = System.IO.File.ReadAllText(@"temp/" + customerName + ".txt");
                    var lastState = JsonConvert.DeserializeObject<List<StatQueue>>(dataFromFile);

                    foreach (var stat in stats)
                    {
                        var data = lastState.Where(x => x.ErrorEventCount == x.ErrorEventCount && x.Extension == stat.Extension);

                        if (!(data.Count() > 0))
                        {
                            if (stat.ErrorEventCount >= Int32.Parse(tresHold))
                            {
                                message += $"`{stat.Extension}` \n" +
                                       $"Extension Error: {stat.ErrorEventCount} \n" +
                                       $"Extension URL: {stat.ExtentionUrl} \n" +
                                       $"-------------------------------------------- \n";

                                sendMessage = true;
                            }
                        }
                    }

                    System.IO.File.Delete(filePath);
                }
                else
                {
                    foreach (var stat in stats)
                    {
                        if (stat.ErrorEventCount >= Int32.Parse(tresHold))

                            message += $"`{stat.Extension}` \n" +
                                    $"Extension Error: {stat.ErrorEventCount} \n" +
                                    $"Extension URL: {stat.ExtentionUrl} \n" +
                                       $"-------------------------------------------- \n";

                        sendMessage = true;

                    }
                }

                if (sendMessage)
                {
                    SendSlackStatus(message);
                }

                using FileStream createStream = System.IO.File.Create(@"temp/" + customerName + ".txt");
                JsonSerializer.SerializeAsync(createStream, stats);
            }
            catch (Exception e)
            {
                // TO DO: HERE
            }
        }

        public void SendSlackStatus(string message)
        {
            try
            {
                var slackSettings = _configuration.GetSection("SlackSettings").Get(typeof(SlackSettings)) as SlackSettings;
                SlackHandler slackHandler = new SlackHandler(slackSettings.ChannelName, slackSettings.ChannelCode);

                slackHandler.PostMessageWaitForResponse(message);
            }
            catch (Exception e)
            {
                // TO DO: HERE
            }
        }

        // ExtensionQueue/GetStatQueues/Megaflis/Test
        [HttpGet, Route("GetStatQueues/All")]
        public IActionResult GetAllStatQueues()
        {
            List<StatQueueName> stat = new List<StatQueueName>();
            List<StatQueue> statDetail = new List<StatQueue>();

            var custSettings = customerSettings.Where(a => a.RestAPIKey != "").ToList();
            var extensionURL = _configuration.GetConnectionString("InRiverExtensionLink");

            if (custSettings == null)
                return new JsonResult(stat);

            foreach (var settings in custSettings)
            {
                extensionURL = extensionURL.Replace("{ENVIRONMENT_ID}", settings.EnvironmentId);
                extensionURL = extensionURL.Replace("{ENVIRONMENT}", settings.Environment.ToLower());

                var IDs = settings.StatQueues.Equals("default") ? GetListOfExtension(settings.RestAPIKey) : CheckExtensionIfEnabled(settings.StatQueues, settings.RestAPIKey);
                
                foreach (var id in IDs)
                {
                    var result = GetStatistics(settings.RestAPIKey, id);

                    if (result.events.queuedEventCount > 0 || result.events.errorEventCount > 0) {
                        statDetail.Add(new StatQueue()
                        {
                            ID = settings.Name,
                            Extension = id,
                            QueuedEventCount = result.events.queuedEventCount,
                            ErrorEventCount = result.events.errorEventCount,
                            CurrentSequenceNumber = result.events.currentSequenceNumber,
                            ExtentionUrl = extensionURL.Replace("{EXTENSION}", id)
                        });
                    }
                }

                stat.Add(new StatQueueName()
                {
                    Name = settings.Name,
                    statDetails = statDetail.Where(x => x.ID == settings.Name),
                });
            }

            return new JsonResult(stat);
        }

        #region Not Used
        // ExtensionQueue/GetStateQueue/Megaflis/Test
        [HttpGet, Route("GetStateQueue/{customerName}/{environment}")]
        public IActionResult GetStateQueue(string customerName, string environment)
        {
            List<StateQueue> states = new List<StateQueue>();
            var customerSettings = GetCustomerSettings(customerName, environment);

            if (customerSettings == null)
                return new JsonResult(states);

            var IDs = customerSettings.StateQueues.Equals("default") ? GetListOfExtension(customerSettings.RestAPIKey) : CheckExtensionIfEnabled(customerSettings.StateQueues, customerSettings.RestAPIKey);
            var remotingURL = _configuration.GetConnectionString("RemotingURL");
            manager = RemoteManager.CreateInstance(remotingURL, customerSettings.RemotingKey);

            foreach (var id in IDs)
            {
                if (string.IsNullOrEmpty(id))
                    continue;

                var result = CountQueues(id);
                if (result.Error > 0 || result.Queue > 0)
                    states.Add(new StateQueue()
                    {
                        Extension = id,
                        Error = result.Error,
                        Queue = result.Queue
                    });
            }

            return new JsonResult(states);
        }

        // ExtensionQueue/GetEpiQueue/Megaflis/Test
        [HttpGet, Route("GetEpiQueue/{customerName}/{environment}")]
        public IActionResult GetEpiQueue(string customerName, string environment)
        {
            List<EpiQueue> queues = new List<EpiQueue>();
            var customerSettings = GetCustomerSettings(customerName, environment);
            int pendingEntities = 0, pendingLinks = 0;
            if (customerSettings == null)
                return new JsonResult(queues);

            var IDs = customerSettings.EpiQueues.Equals("default") ? GetListOfExtension(customerSettings.RestAPIKey) : CheckExtensionIfEnabled(customerSettings.EpiQueues, customerSettings.RestAPIKey);

            foreach (var id in IDs)
            {
                if (string.IsNullOrEmpty(id))
                    continue;

                var result = ConnectToEpi(customerSettings.RestAPIKey, id);
                pendingEntities = result?.Count > 0 ? result[EpiConstants.PendingEntities] : 0;
                pendingLinks = result?.Count > 0 ? result[EpiConstants.PendingLinks] : 0;
                if(pendingEntities > 0 || pendingLinks > 0)
                    queues.Add(new EpiQueue()
                    {
                        Extension = id,
                        PendingEntities = pendingEntities,
                        PendingLinks = pendingLinks
                    });
            }

            return new JsonResult(queues);
        }
        #endregion

        private StatisticsQueue GetStatistics(string restAPIKey, string extensionId)
        {
            try
            {
                if (string.IsNullOrEmpty(restAPIKey) || string.IsNullOrEmpty(extensionId))
                    return null;

                var result = new StatisticsQueue()
                {
                    events = new StatisticsQueueDetails() { currentSequenceNumber = 0, errorEventCount = 0, queuedEventCount = 0 },
                    timestampUtc = null
                };

                var baseURL = _configuration.GetConnectionString("BaseURL");
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseURL);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-inRiver-APIkey", restAPIKey);

                    baseURL = $"{baseURL}{extensionId}/statistics";
                    var response = client.GetAsync(baseURL).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        string responseString = response.Content.ReadAsStringAsync().Result;
                        var statisticResult = JsonConvert.DeserializeObject<StatisticsQueue>(responseString);
                        result = statisticResult;
                    }
                }

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private StateQueue CountQueues(string extensionName)
        {
            var result = new StateQueue() { Extension = extensionName };
            int queuedCount = 0, errorCount = 0;
            var connectorStates = new List<ConnectorState>();

            try
            {
                connectorStates = manager.UtilityService.GetAllConnectorStatesForConnector(extensionName);
            }
            catch (Exception)
            {
                return result;
            }

            foreach (var state in connectorStates)
            {
                try
                {
                    var data = JsonConvert.DeserializeObject<ConnectorStateDTO>(state.Data);
                    if (data != null)
                    {
                        if (data.ChangeDateUtc != null && data.ChangeDateUtc != default(DateTime))
                        {
                            DateTime changeDateUtc = data.ChangeDateUtc;

                            if (changeDateUtc.AddDays(1) < DateTime.UtcNow)
                            {
                                errorCount += 1;
                            }
                        }

                        queuedCount += data.EntityId != 0 ? 1 : 0;
                    }
                }
                catch (Exception)
                {
                    return result;
                }
            }

            result.Error = errorCount;
            result.Queue = queuedCount;
            return result;
        }

        private Dictionary<string, int> ConnectToEpi(string restAPIKey, string extensionId)
        {
            try
            {
                Dictionary<string, int> result = new Dictionary<string, int>();
                var baseURL = _configuration.GetConnectionString("BaseURL");
                string endpoint = string.Empty;
                string apiKey = string.Empty;

                // get apikey and endpoint from extension settings.
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseURL);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-inRiver-APIkey", restAPIKey);

                    baseURL = $"{baseURL}{extensionId}/settings";
                    var response = client.GetAsync(baseURL).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = response.Content.ReadAsStringAsync().Result;
                        (endpoint, apiKey) = GetEpiEndpointAndApiKey(JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(responseString));
                    }

                }

                using (HttpClient client = new HttpClient())
                {
                    var fullURL = new Uri(new Uri(endpoint), $"/pimdatareciever/Healthcheck");
                    client.BaseAddress = fullURL;
                    var response = client.GetAsync(fullURL).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        var healthcheckResponse = response.Content.ReadAsStringAsync().Result;
                        var pendingCounts = healthcheckResponse.Replace("Status OK!", string.Empty).Split(',').ToDictionary(x => x.Trim().Split(':')[0].ToLowerInvariant(), x => int.Parse(x.Split(':')[1]));
                        result = pendingCounts;
                    }
                }

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private (string endpoint, string apiKey) GetEpiEndpointAndApiKey(List<KeyValuePair<string, string>> keyValuePairs)
        {
            string endpoint = null;
            string apiKey = null;

            foreach (var item in keyValuePairs)
            {
                switch (item.Key)
                {
                    case EpiConstants.EpiPIMApiKey:
                        apiKey = item.Value;
                        break;
                    case EpiConstants.EpiserverEndpoint:
                        endpoint = item.Value;
                        break;
                }

                if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(endpoint))
                    break;
            }

            return (endpoint, apiKey);
        }

        private CustomerSettings GetCustomerSettings(string customerName, string environment)
        {
            return customerSettings.Where(a => a.Name.Equals(customerName) && a.Environment.Equals(environment)).FirstOrDefault();
        }

        private IList<string> GetListOfExtension(string restAPIKey)
        {
            var restURL = _configuration.GetConnectionString("BaseURL");
            var extensions = new List<string>();
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(restURL);
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-inRiver-APIkey", restAPIKey);
                var response = client.GetAsync(restURL).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    string responseResult = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<List<ExtensionModel>>(responseResult);

                    foreach (var item in result)
                    {
                        if (item.status.isEnabled)
                            extensions.Add(item.extensionId);
                    }
                }
            }
            return extensions;
        }

        private IList<string> CheckExtensionIfEnabled(string extensionIDs, string restAPIKey)
        {
            var extensionList = GetListOfExtension(restAPIKey);
            var IDs = extensionIDs.Split(';').ToList<string>();

            return extensionList.Intersect(IDs).ToList<string>();
        }

        private void Initialized()
        {
            var settings = _configuration.GetSection("CustomerSettings").Get(typeof(List<CustomerSettings>));
            if (settings != null)
            {
                customerSettings = settings as List<CustomerSettings>;
            }
        }

        [HttpGet, Route("GetInRiverStatusRR")]
        public string GetInRiverStatusRR()
        {
            try
            {
                var baseURL = _configuration.GetConnectionString("InRiverStatusURL");
                InRiverStatusClient client = new InRiverStatusClient();
                var inRiverStatus = client.GetInRiverStatusAsync().Result;
                var values = inRiverStatus.Data.CurrentStatus
                   .SelectMany(componentGroup => componentGroup.Components, (componentGroup, component) => new { componentGroup, component })
                   .OrderByDescending(x => x.component.Status)
                   .Select(x => x.componentGroup.DisplayName + " - " + x.component.DisplayName + " is" + x.component.Status.GetDisplayName());

                return JsonConvert.SerializeObject(values);

            }
            catch (Exception e)
            {
                return "Something went wrong: " + e.Message;
            }
        }
    }
}
