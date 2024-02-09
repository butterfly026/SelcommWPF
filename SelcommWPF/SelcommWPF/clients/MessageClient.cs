using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.messages;
using SelcommWPF.global;
using SelcommWPF.utils;
using System.Collections.Generic;
using System.Net;

namespace SelcommWPF.clients
{
    class MessageClient
    {
        public ValidModel ValidatePhoneOrEmail(string url, string param)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("Param", param);
            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecuteGet<ValidModel>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public MessageModel GetMessageList(string url, string contactCode, int skip, int take, string countRecords = "Y", string search = "", bool contactOnly = true)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ContactCode", contactCode);
            request.AddQueryParameter("api-version", "1.0");
            request.AddQueryParameter("SearchString", search);
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("CountRecords", countRecords);
            request.AddQueryParameter("ContactOnly", contactOnly);

            var response = client.ExecuteGet<MessageModel>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<Dictionary<string, string>> GetEmailAddressOrSMS(string url, string contactCode)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ContactCode", contactCode);
            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecuteGet<List<Dictionary<string, string>>>(request);
            MessageUtils.ParseResponse(response);

            return response.Data;
        }

        public HttpStatusCode SendEmailOrSMS(string url, string type, string contactCode, Dictionary<string, object> body)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("Type", type);
            request.AddUrlSegment("Contacts", type == "Emails" ? "Contacts" : "ContactCode");
            request.AddUrlSegment("ContactCode", contactCode);
            request.AddQueryParameter("api-version", "1.0");
            request.AddBody(body, "application/json");

            var response = client.ExecutePost(request);
            MessageUtils.ParseResponse(response);

            return response.StatusCode;
        }

        public Dictionary<string, object> GetAvailableDocuments(string url, string contactCode, int skip, int take, string countRecords = "Y", string search = "")
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ContactCode", contactCode);
            request.AddQueryParameter("api-version", "1.0");
            request.AddQueryParameter("SearchString", search);
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("CountRecords", countRecords);

            var response = client.ExecuteGet<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

    }
}
