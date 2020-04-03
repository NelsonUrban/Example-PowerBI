using System;
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


namespace PowerBI_API_NetCore_Sample.Pages
{
    public class ReportController : Controller
    {
        public static string _accessToken;
        public string EmbedUrl { get; set; }
        public string AccessToken { get; set; }
        public string ReportId { get; set; }

        public AppSettings AppSettings { get; }

        public ReportController(IConfiguration configuration)
        {
            AppSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
        }
        public IActionResult Index()
        {
            return View();
        }     

        public async Task<IActionResult> GetReport()
        {
            using (var client = new PowerBIClient(new Uri(AppSettings.ApiUrl), new TokenCredentials(AccessToken, "Bearer")))
            {

                var groupId = (await client.Groups.GetGroupsAsync()).Value.ToArray();

                

                var Report = await (client.Reports.GetReportsInGroupAsync("1"));
            }

            return View();
        }



        
    }
}