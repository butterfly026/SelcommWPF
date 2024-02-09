using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients
{
    class FinancialsClient
    {
        public Financials GetFinancialsHistory(string url, string code, int skip, int take, string countRecords)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("code", code);
            request.AddQueryParameter("api-version", "1.0");
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("countRecords", countRecords);

            var response = client.ExecuteGet<Financials>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

    }
}
