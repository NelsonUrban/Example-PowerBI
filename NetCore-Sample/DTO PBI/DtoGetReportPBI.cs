using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace PowerBI_API_NetCore_Sample.DTO_PBI
{
    public class DtoGetReportPBIcs
{
        [DataMember(Name ="Id")]
   
        public string ID_Report { get; set; }
        public string name { get; set; }
        public string appId { get; set; }
        public string webUrl { get; set; }
        public string reportType { get; set; }

    }
}
