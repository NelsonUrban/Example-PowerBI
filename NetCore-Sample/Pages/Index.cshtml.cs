using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace PowerBI_API_NetCore_Sample.Pages
{
    public class IndexModel : PageModel
    {
        public static string _accessToken;
        public string EmbedUrl { get; set; }
        public string AccessToken => _accessToken;
        public string ReportId { get; set; }

        public List<Report> Reports { get; set; } = new List<Report>();

        public AppSettings AppSettings { get; }

        public IndexModel(IConfiguration configuration)
        {
            AppSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
        }

        public async void OnGet()
        {
            if (Request.Query.ContainsKey("code"))
            {

                if (Request.Query.TryGetValue("reportId", out var value))
                {
                    ReportId = value;
                }

                if (_accessToken == null)
                {
                    var authCode = Request.Query["code"][0];
                    Task.Run(async () => await GetToken()).Wait();
                    //AccessToken = _accessToken;
                    //Task.Run(async () => await GetReports()).Wait();
                }
            }
            else
            {
                var errorMessage = VerifySettings();
                if (string.IsNullOrEmpty(errorMessage))
                {
                    var accessToken = GetToken().Result;
                    GetReports(accessToken).GetAwaiter().GetResult();
                }
                else
                {
                    // error in settings
                    Response.Redirect($"Error?message={errorMessage}");
                }
            }
        }

        private async Task GetReports(string accessToken)
        {
            using (var client = new PowerBIClient(new Uri(AppSettings.ApiUrl), new TokenCredentials(accessToken, "Bearer")))
            {
                var groups = await client.Groups.GetGroupsAsync();

                foreach (var group in groups.Value)
                {
                    var reportsList = await client.Reports.GetReportsInGroupAsync(group.Id);

                    Reports.AddRange(reportsList.Value);
                }
            }

        }


        private string GetAuthorizationCode()
        {
            var tennat = "";
            var clientId = "";
            var secret = "";
            // El primer parametro es el appId
            var clientCredential = new ClientCredential(clientId,secret);
            // Este es el tennant Id
            AuthenticationContext context = new AuthenticationContext($"https://sts.windows.net/{tennat}/", false);
            AuthenticationResult authenticationResult = context.AcquireTokenAsync(clientId,
               clientCredential).GetAwaiter().GetResult();
          
            return authenticationResult.AccessToken;
        }

        private async Task<string> GetToken()
        {
            using (var httpClient = new HttpClient())
            {
             

                var parameters = new Dictionary<string, string>();
                parameters.Add("grant_type", "password");
                parameters.Add("scope", "openid");
                parameters.Add("resource", "https://analysis.windows.net/powerbi/api");
                parameters.Add("client_id", "");
                parameters.Add("client_secret", "");
                parameters.Add("username", "");
                parameters.Add("password", "");
                parameters.Add("tenant_id", "");

                var request = new HttpRequestMessage(HttpMethod.Post, AppSettings.LoggingRequestUrl)
                {
                    Content = new FormUrlEncodedContent(parameters)
                };

                using (var response = await httpClient.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var result = JObject.Parse(await response.Content.ReadAsStringAsync());
                        _accessToken = result.Value<string>("access_token");
                        return _accessToken;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }
        }
        private async Task GetAccessToken(string authCode)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                var requestBody = $"grant_type=password" +
                                   $"scope=openid" +
                                   $"resource= https://analysis.windows.net/powerbi/api" +
                                   $"client_id=" +
                                   $"client_secret=" +
                                   $"username=" +
                                   $"password=" +
                                   $"tenant_id=";

                using (var response = await httpClient.PostAsync(AppSettings.LoggingRequestUrl,
                    new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded")))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var result = JObject.Parse(await response.Content.ReadAsStringAsync());
                        _accessToken = result.Value<string>("access_token");
                    }
                    else
                    {
                        Response.Redirect("Error?message=Can't get access token");
                    }
                }
            }
        }

        private string VerifySettings()
        {
            string message = null;
            Guid result;

            // Application Id must have a value.
            if (string.IsNullOrWhiteSpace(AppSettings.ApplicationId))
            {
                message = "ApplicationId is empty. please register your application as Native app in https://dev.powerbi.com/apps and fill application Id in appsettings.json";
            }
            else
            {
                // Application Id must be a Guid object.
                if (!Guid.TryParse(AppSettings.ApplicationId, out result))
                {
                    message = "ApplicationId must be a Guid object. please register your application as Native app in https://dev.powerbi.com/apps and fill application Id in appsettings.json";
                }
            }

            // Workspace Id must be a Guid object.
            if (!string.IsNullOrWhiteSpace(AppSettings.GroupId))
            {
                if (!Guid.TryParse(AppSettings.GroupId, out result))
                {
                    message = "GroupId must be a Guid object. Please select a group you own and fill its Id in appsettings.json";
                }
            }

            // All urls must have value
            if (string.IsNullOrWhiteSpace(AppSettings.ApiUrl) || string.IsNullOrWhiteSpace(AppSettings.AuthorityUri) ||
                string.IsNullOrWhiteSpace(AppSettings.RedirectUrl) || string.IsNullOrWhiteSpace(AppSettings.ResourceUrl) || string.IsNullOrWhiteSpace(AppSettings.LoggingRequestUrl))
            {
                message = "One or more of the urls required are missing. Please check appsettings.json file. for more info check sample instructions in https://github.com/Microsoft/PowerBI-Developer-Samples";
            }

            return message;
        }
    }
}
