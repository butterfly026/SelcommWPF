using MaterialDesignColors;
using Newtonsoft.Json;
using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.services;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static SelcommWPF.clients.models.contacts.SearchModel;

namespace SelcommWPF.clients
{
    class ServicesClient
    {

        public ServicesResponse GetServiesList(string url, string contactCode, int skip, int take, string countRecords, string search, 
            bool isList = true, string serviceCode = "")
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("contactCode", contactCode);
            if (!isList) request.AddUrlSegment("serviceCode", serviceCode);
            request.AddQueryParameter("api-version", "1.1");
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("countRecords", countRecords);
            request.AddQueryParameter("SearchString", search);

            var response = client.ExecuteGet<ServicesResponse>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<DisplayDetails> GetServicesListDetails(string url, long serviceRef)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("serviceRef", serviceRef);
            request.AddQueryParameter("api-version", "1.2");

            var response = client.ExecuteGet<List<DisplayDetails>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public ServicesType GetServicesType(string url, string contactCode, int skip, int take, string countRecords, string search)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("contactCode", contactCode);
            request.AddQueryParameter("api-version", "1.1");
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("countRecords", countRecords);
            request.AddQueryParameter("SearchString", search);

            var response = client.ExecuteGet<ServicesType>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public ServicesPlans GetServicesPlans(string url, string contactCode, int skip, int take, string countRecords, string search)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("contactCode", contactCode);
            request.AddQueryParameter("api-version", "1.1");
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("countRecords", countRecords);

            var response = client.ExecuteGet<ServicesPlans>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<ServicesChanges> GetServicesChanges(string url, string contactCode, int skip, int take, string countRecords, string from)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("contactCode", contactCode);
            request.AddQueryParameter("api-version", "1.1");
            request.AddQueryParameter("From", from);
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("countRecords", countRecords);

            var response = client.ExecuteGet<List<ServicesChanges>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public ServicesStatus GetServicesStatus(string url, string contactCode, int skip, int take, string countRecords)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("contactCode", contactCode);
            request.AddQueryParameter("api-version", "1.1");
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("countRecords", countRecords);

            var response = client.ExecuteGet<ServicesStatus>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public ServicesCost GetServicesCostCenter(string url, string contactCode, int skip, int take, string countRecords)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("contactCode", contactCode);
            request.AddQueryParameter("api-version", "1.1");
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("countRecords", countRecords);

            var response = client.ExecuteGet<ServicesCost>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public ServicesGroup GetServicesGroup(string url, string contactCode, int skip, int take, string countRecords)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("contactCode", contactCode);
            request.AddQueryParameter("api-version", "1.1");
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("countRecords", countRecords);

            var response = client.ExecuteGet<ServicesGroup>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public ServicesSites GetServicesSites(string url, string contactCode, int skip, int take, string countRecords)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("contactCode", contactCode);
            request.AddQueryParameter("api-version", "1.1");
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("countRecords", countRecords);

            var response = client.ExecuteGet<ServicesSites>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public TaskModel GetServicesTasksList(string url, string code, int skip, int take, string countRecords, string search, bool isService = true)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment(isService ? "ServiceReference" : "ContactCode", code);
            request.AddQueryParameter("api-version", "1.0");
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("CountRecords", countRecords);
            request.AddQueryParameter("SearchString", search);

            var response = client.ExecuteGet<TaskModel>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<AttributeModel> GetServiceAttributesData(string url, string reference)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ServiceReference", reference);
            request.AddQueryParameter("api-version", "1.2");

            var response = client.ExecuteGet<List<AttributeModel>>(request);
            MessageUtils.ParseResponse(response);

            return response.Data;
        }

        public Dictionary<string, object> GetServiceTypeBasic(string url, string reference)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ServiceReference", reference);
            request.AddQueryParameter("api-version", "1.2");

            var response = client.ExecuteGet<Dictionary<string, object>> (request);
            MessageUtils.ParseResponse(response);

            return response.Data;
        }

        public AttributeModel GetServiceAttributeDetail(string url, long id)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("Id", id);
            request.AddQueryParameter("api-version", "1.2");

            var response = client.ExecuteGet<AttributeModel>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<Dictionary<string, object>> GetServicesDefinitionId(string url, string serviceTypeId)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ServiceTypeId", serviceTypeId);
            request.AddQueryParameter("api-version", "1.2");

            var response = client.ExecuteGet<List<Dictionary<string, object>>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public HttpStatusCode CreateServiceAttribute(string url, string reference, Dictionary<string, object> body)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ServiceReference", reference);
            request.AddQueryParameter("api-version", "1.2");
            request.AddBody(body, "application/json");

            var response = client.ExecutePost<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);

            return response.StatusCode;
        }

        public HttpStatusCode UpdateServiceAttribute(string url, long id, Dictionary<string, object> body)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("Id", id);
            request.AddQueryParameter("api-version", "1.2");
            request.AddBody(body, "application/json");

            var response = client.Patch(request);
            MessageUtils.ParseResponse(response);
            return response.StatusCode;
        }

        public HttpStatusCode DeleteServiceAttribute(string url, long id)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("Id", id);
            request.AddQueryParameter("api-version", "1.2");

            var response = client.Delete(request);
            MessageUtils.ParseResponse(response);
            return response.StatusCode;
        }

        public List<ServicesType.Item> GetServiceTypes(string url, bool includePseudo = false)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("IncludePseudo", includePseudo);
            request.AddQueryParameter("api-version", "1.2");

            var response = client.ExecuteGet<List<ServicesType.Item>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public List<Dictionary<string, object>> GetServiceAttrDefinitions(string url, string serviceTypeId = "")
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            if (serviceTypeId != "") request.AddUrlSegment("ServiceTypeId", serviceTypeId);
            request.AddQueryParameter("api-version", "1.2");

            var response = client.ExecuteGet<List<Dictionary<string, object>>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public HttpStatusCode GetAuthrisations(string url, string resource)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("Resource", resource);
            request.AddQueryParameter("api-version", "1.2");

            var response = client.ExecuteGet<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);
            return response.StatusCode;
        }

        public Dictionary<string, object> GetServiceConfigData(string url, string serviceTypeId)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            if (serviceTypeId != "") request.AddUrlSegment("ServiceTypeId", serviceTypeId);
            request.AddQueryParameter("api-version", "1.2");

            var response = client.ExecuteGet<Dictionary<string, object>>(request);
            //MessageUtils.ParseResponse(response);
            if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
            return response.Data;
        }

        public List<Dictionary<string, object>> GetServiceQualifications(string url, int skip, int take, string search, string ver = "1.0")
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("api-version", ver);
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("SearchString", search);

            var response = client.ExecuteGet<List<Dictionary<string, object>>>(request);
            MessageUtils.ParseResponse(response);

            return response.Data;
        }

        public ServicesPlans.Item GetServicePlansData(string url, string contactCode, string serviceTypeId, int skip, int take, string search)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ServiceTypeId", serviceTypeId);
            request.AddUrlSegment("ContactCode", contactCode);
            request.AddQueryParameter("api-version", "1.2");
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("CountRecords", "Y");
            request.AddQueryParameter("SearchString", search);

            var response = client.ExecuteGet<ServicesPlans.Item>(request);
            MessageUtils.ParseResponse(response);

            return response.Data;
        }

        public List<ServicesType.Available> GetServiceAvailableIds(string url, string serviceTypeId,  int skip, int take, string search)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ServiceTypeId", serviceTypeId);
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("SearchString", search);

            var response = client.ExecuteGet<List<ServicesType.Available>>(request);
            MessageUtils.ParseResponse(response);

            return response.Data;
        }

        public HttpStatusCode ReleaseServiceReserve(string url, string serviceId)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("ServiceId", serviceId);
            request.AddQueryParameter("api-version", "1.2");

            var response = client.ExecutePost<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);
            return response.StatusCode;
        }


    }
}
