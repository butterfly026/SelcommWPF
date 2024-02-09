using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.payment;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients
{
    class PlanClient
    {

        public List<PlanModel> GetPlanHistories(string url, string code, bool isServices)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment(isServices ? "ServiceReference" : "ContactCode", code);
            request.AddQueryParameter("api-version", isServices ? "1.2" : "1.0");

            var response = client.ExecuteGet<List<PlanModel>>(request);
            MessageUtils.ParseResponse(response);

            return response.Data;
        }

        public PlanModel.Detail GetPlanDetailData(string url, long reference, long id)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            if (reference != 0) request.AddUrlSegment("ServiceReference", reference);
            request.AddUrlSegment("Id", id);
            request.AddQueryParameter("api-version", reference == 0 ? "2.0" : "1.2");

            var response = client.ExecuteGet<PlanModel.Detail>(request);
            MessageUtils.ParseResponse(response);

            return response.Data;
        }

        public TransactionRate GetPlanTransactionRates(string url, long id, int skip, int take, string countRecords)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ServiceReference", id);
            request.AddQueryParameter("api-version", "1.2");
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("CountRecords", countRecords);

            var response = client.ExecuteGet<TransactionRate>(request);
            MessageUtils.ParseResponse(response);

            return response.Data;
        }


    }
}
