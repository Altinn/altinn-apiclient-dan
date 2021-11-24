﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Altinn.ApiClients.Dan.Interfaces;
using Altinn.ApiClients.Dan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SampleWebApp.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class DanClientTestController : ControllerBase
    {
        private readonly IDanClient _danClient;
        public DanClientTestController(IDanClient danClient)
        {
            _danClient = danClient;
        }

        [ActionName("index")]
        public async Task<ActionResult> Index()
        {
            return Content(await System.IO.File.ReadAllTextAsync("Views/index.html"), "text/html; charset=utf-8");
        }

        [HttpGet("{datasetname}/{subject}")]
        [ActionName("get")]
        public async Task<ActionResult> Get(string datasetname, string subject, [FromQuery] Dictionary<string, string> parameters)
        {
            DataSet dataset = await _danClient.GetDataSet(datasetname, subject, null, parameters);
            return Content(dataset.ToHtmlTable(), "text/html; charset=utf-8");
        }

        [HttpGet("{datasetname}/{subject}")]
        [ActionName("auth")]
        public async Task<ActionResult> Auth(string datasetname, string subject, [FromQuery] Dictionary<string, string> parameters)
        {
            var dataSetRequest = GetDataSetRequest(datasetname, parameters);
            Accreditation accreditation = await _danClient.CreateDataSetRequest(dataSetRequest, subject, "991825827");

            return Content(accreditation.ToHtmlTable(), "text/html; charset=utf-8");
        }
        
        [HttpGet("{accreditationId}/{datasetname?}")]
        [ActionName("status")]
        public async Task<ActionResult> Status(string accreditationId, string datasetname)
        {
            List<DataSetRequestStatus> dataSetRequestStatus;
            if (string.IsNullOrEmpty(datasetname))
            {
                dataSetRequestStatus = await _danClient.GetRequestStatus(accreditationId);
            }
            else
            {
                dataSetRequestStatus = await _danClient.GetRequestStatus(accreditationId, datasetname);
            }

            return Content(dataSetRequestStatus.ToHtmlTable(), "text/html; charset=utf-8");
        }


        [HttpGet("{accreditationId}/{datasetname}")]
        [ActionName("getbyaid")]
        public async Task<ActionResult> GetByAid(string accreditationId, string datasetname)
        {
            DataSet dataset = await _danClient.GetDataSetFromAccreditation(accreditationId, datasetname);
            return Content(dataset.ToHtmlTable(), "text/html; charset=utf-8");
        }

        private static DataSetRequest GetDataSetRequest(string datasetname, Dictionary<string, string> parameters)
        {
            DataSetRequest dataSetRequest = new DataSetRequest()
            {
                DataSetName = datasetname,
                Parameters = parameters.Select(kvp => new DataSetParameter()
                {
                    DataSetParamName = kvp.Key, Value = kvp.Value
                }).ToList()
            };

            return dataSetRequest;
        }
    }
    
    internal static class Extensions
    {
        public static string ToHtmlTable(this DataSet ds)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table>");
            foreach (var dsv in ds.Values)
            {
                sb.AppendLine($"<tr><th>{dsv.Name}:</th><td>{dsv.Value}</td></tr>");
            }
            sb.AppendLine("</table>");

            return sb.ToString();
        }

        public static string ToHtmlTable(this Accreditation acr)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table>");
            sb.AppendLine($"<tr><th>Accreditation-ID:</th><td>{acr.AccreditationId}</td></tr>");
            sb.AppendLine($"<tr><th>Requestor:</th><td>{acr.Requestor}</td></tr>");
            sb.AppendLine($"<tr><th>Subject:</th><td>{acr.Subject}</td></tr>");
            sb.AppendLine($"<tr><th>Issued:</th><td>{acr.Issued}</td></tr>");
            sb.AppendLine($"<tr><th>Dataset(s):</th><td>{string.Join(", ", acr.DataSetDefinitions.Select(code => code.DataSetName))}</td></tr>");
            sb.AppendLine("</table>");

            return sb.ToString();
        }

        public static string ToHtmlTable(this List<DataSetRequestStatus> drsList)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table>");
            foreach (var drs in drsList)
            {
                sb.AppendLine($"<tr><th>Dataset:</th><td>{drs.DataSetName}</td></tr>");
                sb.AppendLine($"<tr><th>Status:</th><td>{drs.Status.Description}</td></tr>");
            }
            sb.AppendLine("</table>");

            return sb.ToString();
        }

    }
}
