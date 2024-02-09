using RestSharp;
using SelcommWPF.clients.models.report;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models
{
    class ReportClient
    {
        public ReportDefinition GetReportList(string url, int skip, int take, string countRecords, string search, int categoryId = 0,
                                            bool includeParameters = false, bool includeEmails = false)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("api-version", "1.0");
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("SearchString", search);
            request.AddQueryParameter("CountRecords", countRecords);
            if (categoryId == 0) request.AddQueryParameter("CategoryId", categoryId);
            request.AddQueryParameter("IncludeParameters", includeParameters);
            request.AddQueryParameter("IncludeEmails", includeEmails);

            var response = client.ExecuteGet<ReportDefinition>(request);
            MessageUtils.ParseResponse(response);

            return response.Data;
        }

        public ReportDetailModel GetReportDetailList(string url, string definitionId, int skip, int take, string countRecords,
                                            bool includeParameters = true, bool includeEmails = true)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("DefinitionId", definitionId);
            request.AddQueryParameter("api-version", "1.0");
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("CountRecords", countRecords);
            request.AddQueryParameter("IncludeParameters", includeParameters);
            request.AddQueryParameter("IncludeEmails", includeEmails);

            var response = client.ExecuteGet<ReportDetailModel>(request);
            MessageUtils.ParseResponse(response);

            return response.Data;
        }

        public ReportDefinition.Item GetReportDetailData(string url, string id)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("DefinitionId", id);
            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecuteGet<ReportDefinition.Item>(request);
            MessageUtils.ParseResponse(response);

            return response.Data;
        }

        public void DeleteReportDetail(string url, long id)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("Id", id);
            request.AddQueryParameter("api-version", "1.0");

            var response = client.Execute(request, Method.Delete);
            if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
        }

        public Dictionary<string, object> GetDownloadReport(string url, long id)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("Id", id);
            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecuteGet<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);

            return response.Data;
        }

        public Dictionary<string, object> GenerateReport(string url, Dictionary<string, object> body)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("api-version", "1.0");
            request.AddBody(body, "application/json");

            var response = client.ExecutePost<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);

            body = new Dictionary<string, object>();
            body.Add("statusCode", response.StatusCode);
            body.Add("response", response.Data);
            return body;
        }

        public HttpStatusCode UpdateEndSchedule(string url, long id, string dateTime)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("Id", id);
            request.AddQueryParameter("api-version", "1.0");
            request.AddBody(new { End = dateTime }, "application/json");

            var response = client.Execute(request, Method.Patch);
            if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
            return response.StatusCode;
        }

        public List<UserModel.BusinessUnitModel> GetReportParameterList(string url, string search = "")
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            if (search != "") request.AddQueryParameter("SearchString", search);
            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecuteGet<List<UserModel.BusinessUnitModel>>(request);
            MessageUtils.ParseResponse(response);

            return response.Data;
        }

    }
}   
