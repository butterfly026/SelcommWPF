using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.contacts;
using SelcommWPF.clients.models.messages;
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
    class ContactClient
    {

        public SearchModel SearchContactList(string url, string search, int skip, int take, string countRecords = "Y")
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
            request.AddQueryParameter("CountRecords", countRecords);
            request.AddBody(new { SearchString = search }, "application/json");

            var response = client.ExecutePost<SearchModel>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public SearchModel AdvancedSearchList(string url, Dictionary<string, object> body, int skip, int take, string countRecords = "Y")
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
            request.AddQueryParameter("CountRecords", countRecords);
            request.AddBody(body, "application/json");

            var response = client.ExecutePost<SearchModel>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<PhoneModel.History> GetContactPhoneHistory(string url, string code)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ContactCode", code);
            request.AddQueryParameter("api-version", "1.1");

            var response = client.ExecuteGet<List<PhoneModel.History>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<AddressModel.AddressType> GetContactAddressType(string url)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("api-version", "1.1");

            var response = client.ExecuteGet<List<AddressModel.AddressType>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<AddressModel.Country> GetContactAddressCountries(string url)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("api-version", "1.1");

            var response = client.ExecuteGet<List<AddressModel.Country>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<AddressModel.Country> GetAddressStates(string url, string countryCode)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("CountryCode", countryCode);
            request.AddQueryParameter("api-version", "1.1");

            var response = client.ExecuteGet<List<AddressModel.Country>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<Dictionary<string, object>> GetAddressPostCode(string url, string countryCode, string postCode)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("CountryCode", countryCode);
            request.AddUrlSegment("PostCode", postCode);
            request.AddQueryParameter("api-version", "1.1");

            var response = client.ExecuteGet<List<Dictionary<string, object>>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<Dictionary<string, string>> AutoCompleteAustralia(string url, string term, bool closeMatches = true)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("api-version", "1.0");
            request.AddQueryParameter("Term", term);
            request.AddQueryParameter("CloseMatches", closeMatches);

            var response = client.ExecuteGet<List<Dictionary<string, string>>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public Dictionary<string, string> ParseAddressAustralia(string url, string term)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("api-version", "1.0");
            request.AddQueryParameter("Term", term);

            var response = client.ExecuteGet<Dictionary<string, string>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<Dictionary<string, string>> GetAliasTypesAndTitles(string url)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("api-version", "1.1");

            var response = client.ExecuteGet<List<Dictionary<string, string>>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<UserDefined> GetUserDefinedData(string url)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("api-version", "1.1");

            var response = client.ExecuteGet<List<UserDefined>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<Identification> GetIdentificationData(string url)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("api-version", "1.1");

            var response = client.ExecuteGet<List<Identification>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public Dictionary<string, object> GetIdentificationRules(string url, string contactType)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ContactType", contactType);
            request.AddQueryParameter("api-version", "1.1");

            var response = client.ExecuteGet<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<UserDefined> GetUserDefinedData(string url, string code)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ContactCode", code);
            request.AddQueryParameter("api-version", "1.1");

            var response = client.ExecuteGet<List<UserDefined>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public HttpStatusCode CreateUserDefinedData(string url, string code, string id, string value)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ContactCode", code);
            request.AddUrlSegment("Id", id);
            request.AddQueryParameter("api-version", "1.1");
            request.AddBody(new { Value = value }, "application/json");

            var response = client.ExecutePost<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);

            return response.StatusCode;
        }

        public HttpStatusCode UpdateUserDefinedData(string url, string code, string id, string value)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ContactCode", code);
            request.AddUrlSegment("Id", id);
            request.AddQueryParameter("api-version", "1.1");
            request.AddBody(new { Value = value }, "application/json");

            var response = client.ExecutePut<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);

            return response.StatusCode;
        }

        public void DeleteUserDefinedData(string url, string code, string id)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ContactCode", code);
            request.AddUrlSegment("Id", id);
            request.AddQueryParameter("api-version", "1.1");

            var response = client.Execute(request, Method.Delete);
            if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
        }

        public List<RelatedContactModel.Relationship> GetRelationShipList(string url)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("api-version", "1.1");

            var response = client.ExecuteGet<List<RelatedContactModel.Relationship>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<RelatedContactModel.TimeZone> GetTimeZoneList(string url, string search, int skip = 0, int take = 10)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("api-version", "1.1");
            request.AddQueryParameter("SearchString", search);
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);

            var response = client.ExecuteGet<List<RelatedContactModel.TimeZone>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<RelatedContactModel> GetRelatedContactList(string url, string code)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ContactCode", code);
            request.AddQueryParameter("api-version", "1.1");

            var response = client.ExecuteGet<List<RelatedContactModel>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public RelatedContactModel GetRelatedContactDetail(string url, string code, string id)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ContactCode", code);
            request.AddUrlSegment("Id", id);
            request.AddQueryParameter("api-version", "1.1");

            var response = client.ExecuteGet<RelatedContactModel>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public HttpStatusCode CreateRelatedContactData(string url, string code, Dictionary<string, object> body)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ContactCode", code);
            request.AddQueryParameter("api-version", "1.1");
            request.AddBody(body, "application/json");

            var response = client.ExecutePost<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);

            return response.StatusCode;
        }

        public HttpStatusCode UpdateRelatedContactData(string url, string code, string id, Dictionary<string, object> body)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();
            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ContactCode", code);
            request.AddUrlSegment("Id", id);
            request.AddQueryParameter("api-version", "1.1");
            request.AddBody(body, "application/json");

            var response = client.ExecutePut<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);

            return response.StatusCode;
        }

        public List<Dictionary<string, object>> GetTaskParameters(string url, string contactCode = "")
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            if (contactCode != "") request.AddUrlSegment("ContactCode", contactCode);
            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecuteGet<List<Dictionary<string, object>>>(request);
            MessageUtils.ParseResponse(response);

            return response.Data;
        }

        public List<TaskModel.Item> GetTaskSubListData(string url, long id = 0)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            if (id != 0) request.AddUrlSegment("Id", id);
            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecuteGet<List<TaskModel.Item>>(request);
            MessageUtils.ParseResponse(response);

            return response.Data;
        }

        public Dictionary<string, string> GetTaskNextNumber(string url)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecuteGet<Dictionary<string, string>>(request);
            MessageUtils.ParseResponse(response);

            return response.Data;
        }

        public Dictionary<string, object> CreateNewTask(string url, string code, Dictionary<string, object> body, bool isSerivce = false)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment(isSerivce ? "ServiceReference" : "ContactCode", code);
            request.AddQueryParameter("api-version", "1.0");
            request.AddBody(body, "application/json");

            var response = client.ExecutePost<Dictionary<string, long>>(request);
            MessageUtils.ParseResponse(response);

            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("StatusCode", response.StatusCode);
            result.Add("Response", response.Data);

            return result;
        }


        public HttpStatusCode CreateTaskParameters(string url, long id, Dictionary<string, object> body)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("Id", id);
            request.AddQueryParameter("api-version", "1.0");
            request.AddBody(body, "application/json");

            var response = client.ExecutePost<List<Dictionary<string, object>>>(request);
            MessageUtils.ParseResponse(response);

            return response.StatusCode;
        }

        public void UpdateTaskData(string url, long id, Dictionary<string, object> body)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("Id", id);
            request.AddQueryParameter("api-version", "1.0");
            request.AddBody(body, "application/json");

            var response = client.Execute(request, Method.Patch);
            if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
        }

        public MessageModel GetMessageList(string url, long id, int skip, int take, string countRecords = "Y", string search = "", bool contactOnly = true)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("Id", id);
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
    }
}
