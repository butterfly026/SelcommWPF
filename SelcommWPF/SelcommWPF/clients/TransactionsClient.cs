using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.payment;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients
{
    class TransactionsClient
    {
        public Transactions GetTransactionsHistory(string url, string code, int skip, int take, string countRecords)
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

            var response = client.ExecuteGet<Transactions>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public Transactions.Item GetTransactionsDetail(string url, long id)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("TransactionId", id);
            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecuteGet<Transactions.Item>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public UsageModel GetAccountAndServiceUsages(string url, bool isService, string code, string from, string to, bool unInvoiced, 
                                                    int skip, int take, string countRecords, string search)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment(isService ? "ServiceReference" : "ContactCode", code);
            request.AddQueryParameter("api-version", "1.0");
            if (from != "") request.AddQueryParameter("From", from);
            if (to != "") request.AddQueryParameter("To", to);
            request.AddQueryParameter("Uninvoiced", unInvoiced);
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("countRecords", countRecords);
            request.AddQueryParameter("SearchString", search);

            var response = client.ExecuteGet<UsageModel>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public Dictionary<string, string> GetTransactionNumber(string url, string type, bool isOpen = true, bool isDefaulyOnly = false)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("Type", type);
            request.AddQueryParameter("api-version", "1.0");
            request.AddQueryParameter("Open", isOpen);
            request.AddQueryParameter("DefaultOnly", isDefaulyOnly);

            var response = client.ExecuteGet<Dictionary<string, string>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<AllocationModel> GetAllocationTransactionList(string url, string code, int skip, int take, string search, bool isOpenOnly, bool isOpenFirst = true)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("AccountCode", code);
            request.AddQueryParameter("api-version", "1.0");
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("OpenOnly", isOpenOnly);
            request.AddQueryParameter("OpenFirst", isOpenFirst);

            var response = client.ExecuteGet<List<AllocationModel>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public HttpStatusCode CreateReceiptAndInvoice(string url, string code, Dictionary<string, object> body)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("AccountCode", code);
            request.AddQueryParameter("api-version", "1.0");
            request.AddBody(body, "application/json");

            var response = client.ExecutePost<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);
            return response.StatusCode;
        }

        public void ClearAllocation(string url, string code)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("AccountCode", code);
            request.AddQueryParameter("api-version", "1.0");

            var response = client.Execute(request, Method.Delete);
            MessageUtils.ParseResponse(response);
        }

        public List<Dictionary<string, string>> GetTransactionCategories(string url, string type)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            if (type != "") request.AddUrlSegment("Type", type);
            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecuteGet<List<Dictionary<string, string>>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public ProductModel GetAvailableProductList(string url, string search, int skip = 0, int take = 20, string countRecord = "Y")
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
            request.AddQueryParameter("CountRecords", countRecord);
            request.AddQueryParameter("SearchString", search);

            var response = client.ExecuteGet<ProductModel>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

    }
}
