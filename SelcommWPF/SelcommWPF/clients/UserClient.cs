using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.auths;
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
    class UserClient
    {

        public T GetSimpleUserInfo<T>(string url, string userId)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("userId", userId);
            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecuteGet<T>(request);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Not_Register_User);
                if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
            }
            else MessageUtils.ParseResponse(response);

            return response.Data;
        }

        public UserModel GetFullUserInfo(string url, string userId)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("userId", userId);
            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecuteGet<UserModel>(request);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Not_Register_User);
                if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
            }
            else MessageUtils.ParseResponse(response);

            return response.Data;
        }

        public ComplexResponse CheckComplexity(string url, string password)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("password", password);
            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecuteGet<ComplexResponse>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public Dictionary<string, string> GetSuggestion(string url)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecuteGet<Dictionary<string, string>>(request);
            if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
            return response.Data;
        }

        public Dictionary<string, bool> ResetPasswordConfig(string url)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.AccessToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.AccessToken.Credentials);
            }

            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecuteGet<Dictionary<string, bool>>(request);
            if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
            return response.Data;
        }

        public HttpStatusCode ResetPasswordSMS(string url, string userId, string code)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.AccessToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.AccessToken.Credentials);
            }

            Dictionary<string, string> body = new Dictionary<string, string>();
            body.Add("ContactCode", userId);
            body.Add("MSISDNFragment", code);

            request.AddQueryParameter("api-version", "1.0");
            request.AddBody(body, "application/json");

            var response = client.ExecuteGet<Dictionary<string, bool>>(request);
            if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
            return response.StatusCode;
        }

        public Dictionary<string, bool> GetAuthenticateUnique(string url, string parameter)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.AccessToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("Parameter", parameter);
            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecuteGet<Dictionary<string, bool>>(request);
            if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
            return response.Data;
        }

        public Dictionary<string, string> GetServiceProvidedNextId(string url)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.AccessToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecutePost<Dictionary<string, string>>(request);
            if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
            return response.Data;
        }

        public PasswordInfo GetPasswordInformation(string url, string code)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.AccessToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ContactCode", code);
            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecuteGet<PasswordInfo>(request);
            if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
            return response.Data;
        }

        public HttpStatusCode LoginApprovedOrDenied(string url, string userId, Dictionary<string, object> body)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.AccessToken.Type == "Bearer")
            {
                string token = userId == "" ? Global.AccessToken.Credentials : Global.CustomerToken.Credentials;
                request.AddHeader("Authorization", "Bearer " + token);
            }

            if (userId != "") request.AddUrlSegment("UserId", userId);
            request.AddQueryParameter("api-version", "1.0");
            request.AddBody(body, "application/json");

            var response = client.ExecutePost<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);
            return response.StatusCode;
        }

        public LoginHistory GetLoginHistory(string url, string userId, int skip, int take, string countRecords)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("UserId", userId);
            request.AddQueryParameter("api-version", "1.0");
            request.AddQueryParameter("SkipRecords", skip);
            request.AddQueryParameter("TakeRecords", take);
            request.AddQueryParameter("CountRecords", countRecords);

            var response = client.ExecuteGet<LoginHistory>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public HttpStatusCode LoginHistoryMakeSuspect(string url, long id)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("HistoryId", id);
            request.AddQueryParameter("api-version", "1.0");

            var response = client.ExecutePut<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);
            return response.StatusCode;
        }

        public HttpStatusCode RegisterEmailOrMobile(string url, string param, Dictionary<string, object> body)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("Param", param);
            request.AddUrlSegment("UserId", Global.ContactId);
            request.AddQueryParameter("api-version", "1.0");
            request.AddBody(body, "application/json");

            var response = client.ExecutePut<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);
            return response.StatusCode;
        }

        public Dictionary<string, object> GetServiceProviderUsers(string url, int skip, int take, string search, string countRecords)
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

            var response = client.ExecuteGet<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public HttpStatusCode CreateServiceProviderUsers(string url, Dictionary<string, object> body)
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
            return response.StatusCode;
        }

        public HttpStatusCode UpdateServiceProviderUsers(string url, string userId, Dictionary<string, object> body)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("userId", userId);
            request.AddQueryParameter("api-version", "1.0");
            request.AddBody(body, "application/json");

            var response = client.Execute(request, Method.Patch);
            MessageUtils.ParseResponse(response);
            return response.StatusCode;
        }

        public Dictionary<string, object> GetUsersConfiguration(string url, string contactCode = "")
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddQueryParameter("api-version", "1.0");
            if (contactCode != "") request.AddQueryParameter("ContactCode", contactCode);

            var response = client.ExecuteGet<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public HttpStatusCode CreateAuthentication(string url, string contactCode, string category, Dictionary<string, object> body)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ContactCode", contactCode);
            request.AddUrlSegment("Category", category);
            request.AddQueryParameter("api-version", "1.0");
            request.AddBody(body, "application/json");

            var response = client.ExecutePut<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);
            return response.StatusCode;
        }

        public HttpStatusCode SelcommUserLock(string url, string contactCode, Dictionary<string, object> body)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("Identifier", contactCode);
            request.AddQueryParameter("api-version", "3.1");
            request.AddBody(body, "application/json");

            var response = client.ExecutePut<Dictionary<string, object>>(request);
            MessageUtils.ParseResponse(response);
            return response.StatusCode;
        }

        public HttpStatusCode DeleteUser(string url, string contactCode)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (Global.CustomerToken.Type == "Bearer")
            {
                request.AddHeader("Authorization", "Bearer " + Global.CustomerToken.Credentials);
            }

            request.AddUrlSegment("ContactCode", contactCode);
            request.AddQueryParameter("api-version", "1.0");

            var response = client.Execute(request, Method.Delete);
            MessageUtils.ParseResponse(response);
            return response.StatusCode;
        }

    }
}
