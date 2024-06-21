using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ApplicationStack;
using Newtonsoft.Json;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;

namespace StacksFunction
{
    public class StackReport
    {
        [FunctionName("StackReport")]
        public async void Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // Get Web Apps Data


            // Instantiate the ARM client - https://learn.microsoft.com/en-us/dotnet/azure/sdk/authentication/?tabs=command-line
            ArmClient client = new ArmClient(new DefaultAzureCredential(), Environment.GetEnvironmentVariable("subscriptionId"));


            // Retrieve list of web apps in subscription
            AsyncPageable<WebSiteResource> webapplist = client.GetDefaultSubscription().GetWebSitesAsync();

            //Retrieve list of Application Stacks
            AvailableStackList stacks = GetApplicationStacks(log).Result;

            string language = string.Empty;
            string server = string.Empty;
            string serverVersion = string.Empty;
            string serverMajorVersion = string.Empty;
            string version = string.Empty;
            string fullVersion = string.Empty;

            // Iterate through pages of web apps list
            await foreach (Page<WebSiteResource> page in webapplist.AsPages())
            {
                // Iterate through each web app in the current page
                foreach (var webapp in page.Values)
                {
                    // Determine the kind of the web app - switch handles code apps on linux or windows; could be expanded to cover Function apps etc
                    switch (webapp.Data.Kind)
                    {
                        // Linux web app
                        case "app,linux":
                            // Retrieve full stack information
                            var fullStack = webapp.Data.SiteConfig.LinuxFxVersion;

                            // Detect Java Apps using Tomcat, Java SE, JBoss EAP, WildFly server
                            if (fullStack.Split('|')[0] == "TOMCAT" || fullStack.Split('|')[0] == "JAVASE" || fullStack.Split('|')[0] == "JBOSSEAP" || fullStack.Split('|')[0] == "WILDFLY")
                            {
                                // Extract server, server version, language and version information
                                serverVersion = fullStack.Split('|')[1].Split('-')[0];
                                language = "JAVACONTAINERS";
                                int lastPeriodIndex = serverVersion.LastIndexOf('.');
                                if (lastPeriodIndex != -1)
                                {
                                    serverMajorVersion = serverVersion.Substring(0, lastPeriodIndex);
                                }
                                else
                                {
                                    serverMajorVersion = serverVersion;
                                }

                                version = fullStack.Split('|')[1].Split('-')[1];
                                server = string.Format("tomcat{0}", serverMajorVersion);
                            }
                            // Detect Java Apps using Java
                            else if (fullStack.Split('|')[0] == "JAVA")
                            {
                                // Extract server, server version, language and version information
                                server = "JAVA";
                                serverVersion = fullStack.Split('|')[1].Split('-')[0];
                                language = "JAVA";
                                version = fullStack.Split('|')[1].Split('-')[1];
                            }
                            // Detect .NET Core Apps
                            else if (fullStack.Split('|')[0] == "DOTNETCORE")
                            {
                                // Extract language and version information
                                language = "DOTNET";
                                version = fullStack.Split('|')[1];

                                int lastPeriodIndex = version.LastIndexOf('.');
                                if (lastPeriodIndex != -1)
                                {
                                    int versionNumber = int.Parse(version.Substring(0, lastPeriodIndex));
                                    // Check if major version is less than or equal to 3
                                    if (versionNumber <= 3)
                                    {
                                        fullVersion = string.Format("dotnetcore{0}", versionNumber.ToString());
                                    }
                                    else
                                    {
                                        fullVersion = string.Format("dotnet{0}", versionNumber.ToString());
                                    }
                                }
                                else
                                {
                                    fullVersion = string.Format("dotnet{0}", version);
                                }
                            }
                            // Detect sidecars and sitecontainers
                            else if (fullStack.Split('|')[0] == "sitecontainers" || fullStack.Split('|')[0] == "sidecars")
                            {
                                language = "SideCars";
                            }
                            // Detect all other stacks
                            else
                            {
                                // Extract language and version information
                                language = fullStack.Split('|')[0];
                                version = fullStack.Split('|')[1];
                                fullVersion = fullStack.Split('|')[1];
                            }

                            // Log information - could output to message queue, docs, etc
                            log.LogInformation($"App: {webapp.Data.Name}");
                            log.LogInformation("Linux");
                            log.LogInformation($"Full Stack: {fullStack}");
                            log.LogInformation($"Language: {language}");
                            log.LogInformation($"Version: {version}");

                            // Get detailed stack information
                            if (language.ToLower() == "sidecars" || language.ToLower() == "sitecontainers")
                            {
                                break;
                            }
                            else if (language.ToLower() == "javacontainers")
                            {
                                log.LogInformation($"Server: {server}");
                                log.LogInformation($"Server Version: {serverVersion}");
                                
                                // Retrieve stack information
                                ApplicationStack.Stack stackInfo = stacks.value.Find(stacks => stacks.name.ToUpper() == language);
                                
                                // Retrieve major version information
                                Majorversions majVersion = stackInfo.properties.majorVersions.Find(Majorversions => Majorversions.value == server);

                                // Check if major version is found
                                if (majVersion != null)
                                {
                                    // Retrieve minor version information
                                    Minorversions minVersion = majVersion.minorVersions.Find(Minorversions => Minorversions.value == serverVersion);

                                    // Check if minor version is found
                                    if (minVersion != null)
                                    {
                                        // Check if stack is deprecated
                                        if (minVersion.stackSettings.linuxContainerSettings.isDeprecated == true || minVersion.stackSettings.linuxContainerSettings.endOfLifeDate <= DateTime.UtcNow)
                                        {
                                            log.LogError("stack is deprecated, end of life: {0}", minVersion.stackSettings.linuxContainerSettings.endOfLifeDate.ToString());
                                        }
                                        else
                                        {
                                            log.LogInformation("stack is not deprecated");
                                        }
                                    }
                                    else
                                    {
                                        log.LogInformation("Stack not found");
                                    }

                                }


                                break;
                            }
                            else
                            {
                                // Retrieve stack information
                                ApplicationStack.Stack stackInfo = stacks.value.Find(stacks => stacks.name.ToUpper() == language);

                                // Retrieve major version information
                                Majorversions majVersion;

                                // Check if language is .NET Core
                                if (language.ToLower().StartsWith("dotnet"))
                                {
                                    majVersion = stackInfo.properties.majorVersions.Find(Majorversions => Majorversions.value == fullVersion);
                                }
                                else
                                {
                                    majVersion = stackInfo.properties.majorVersions.Find(Majorversions => Majorversions.value == version);
                                }

                                // Check if major version is found
                                if (majVersion != null)
                                {
                                    // Retrieve minor version information
                                    Minorversions minVersion = majVersion.minorVersions.Find(Minorversions => Minorversions.value == version);

                                    // Check if minor version is found
                                    if (minVersion != null)
                                    {
                                        // Check if stack is deprecated
                                        if (minVersion.stackSettings.linuxRuntimeSettings.isDeprecated == true || minVersion.stackSettings.linuxRuntimeSettings.endOfLifeDate <= DateTime.UtcNow)
                                        {
                                            log.LogError("stack is deprecated, end of life: {0}", minVersion.stackSettings.linuxRuntimeSettings.endOfLifeDate.ToString());
                                        }
                                        else
                                        {
                                            log.LogInformation("stack is not deprecated");
                                        }
                                    }
                                    else
                                    {
                                        log.LogInformation("Stack not found");
                                    }

                                }


                                break;
                            }
                        // Windows web app
                        case "app":
                            log.LogInformation($"App: {webapp.Data.Name}");
                            log.LogInformation("Windows");
                            break;
                        default:
                            break;

                    }
                }
            }

        }

        private async Task<ApplicationStack.AvailableStackList> GetApplicationStacks(ILogger log)
        {
            var credential = new DefaultAzureCredential();
            var tokenRequestContext = new TokenRequestContext(new[] { "https://management.azure.com/.default" });

            var accessToken = await credential.GetTokenAsync(tokenRequestContext);

            using var client = new HttpClient();
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken.Token);

                var requestUrl = "https://management.azure.com/providers/Microsoft.Web/webAppStacks?api-version=2023-12-01&stackOsType=Linux";

                var response = await client.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();


                    AvailableStackList applicationStackList = System.Text.Json.JsonSerializer.Deserialize<AvailableStackList>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString });

                    return applicationStackList;
                }
                else
                {
                    throw new Exception($"Failed to get available stacks. Status code: {response.StatusCode}");
                }
            }
        }

    }
}
