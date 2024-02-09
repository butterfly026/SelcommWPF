using RestSharp;
using SelcommWPF.clients.models;
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
    class PayMethodClient
    {

        public List<PaymentMethod> GetPaymentMethodList(string url, string contactCode, bool open = true, bool defaultOnly = false)
        {
            RestClient client = new RestClient(url);
            var request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            if (contactCode != "") request.AddUrlSegment("ContactCode", contactCode);
            request.AddQueryParameter("api-version", "1.1");
            request.AddQueryParameter("Open", open);
            request.AddQueryParameter("DefaultOnly", defaultOnly);

            var response = client.ExecuteGet<List<PaymentMethod>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public HttpStatusCode MakeDefaultPayment(string url, long paymentId, string status)
        {
            RestClient client = new RestClient(url);
            var request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("PayMethodId", paymentId);
            request.AddQueryParameter("api-version", "1.1");
            request.AddBody(new { Status = status }, "application/json");

            var response = client.ExecutePost(request);
            if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
            return response.StatusCode;
        }

        public HttpStatusCode DeletePaymentMethod(string url, long paymentId, string status, string note)
        {
            RestClient client = new RestClient(url);
            var request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("PayMethodId", paymentId);
            request.AddQueryParameter("api-version", "1.1");
            request.AddBody(new { Status = status, Node = note }, "application/json");

            var response = client.ExecutePost(request);
            if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
            return response.StatusCode;
        }

        public Dictionary<string, string> ValidateCreditCard(string url, string cardNumber)
        {
            RestClient client = new RestClient(url);
            var request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("CardNumber", cardNumber);
            request.AddQueryParameter("api-version", "1.1");

            var response = client.ExecuteGet<Dictionary<string, string>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public Dictionary<string, object> AddCreditCard(string url, string contactCode, Dictionary<string, object> body)
        {
            RestClient client = new RestClient(url);
            var request = new RestRequest();
            request.AddHeader("Content-Type", "application/json");
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }
            
            request.AddUrlSegment("ContactCode", contactCode);
            request.AddQueryParameter("api-version", "1.1");
            request.AddBody(body, "application/json");

            var response = client.ExecutePost<Dictionary<string, object>>(request);
            if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
            return response.Data;
        }

        public Dictionary<string, object> AddBankAccount(string url, string contactCode, Dictionary<string, object> body)
        {
            RestClient client = new RestClient(url);
            var request = new RestRequest();
            request.AddHeader("Content-Type", "application/json");
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ContactCode", contactCode);
            request.AddQueryParameter("api-version", "1.1");
            request.AddBody(body, "application/json");

            var response = client.ExecutePost<Dictionary<string, object>>(request);
            if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
            return response.Data;
        }

        public Dictionary<string, double> GetSurchargeAmount(string url, string id, double amount, string source = "ServiceDesk")
        {
            RestClient client = new RestClient(url);
            var request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("PaymentMethodId", id);
            request.AddUrlSegment("Amount", amount);
            request.AddUrlSegment("Source", source);
            request.AddQueryParameter("api-version", "1.1");

            var response = client.ExecuteGet<Dictionary<string, double>>(request);
            if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
            return response.Data;
        }


    }
}
