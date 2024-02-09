using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RestSharp;
using SelcommWPF.clients;
using SelcommWPF.clients.models;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SelcommWPF.global
{
    class Global
    {

        public static double ScreenWidth;
        public static double ScreenHeigth;

        public static LoginWindow loginWindow;
        public static MainWindow mainWindow;
        public static dialogs.charge.ChargeHistoryDialog ChargeDialog;
        public static dialogs.report.ReportDetail ReportDetailDialog;
        public static dialogs.contacts.RelatedContactsList RelatedContactDialg;
        public static dialogs.trans.InvoiceDialog NewInvoiceDialog;
        public static dialogs.bills.BillOptions BillOptionDialog;
        public static dialogs.accounts.CostCenterListDialog CostCenterListDialog;

        public static string UserID;
        public static string Password;
        public static string LanguageCode;
        public static string ContactCode;
        public static string ContactId;
        public static string BillCycle;

        public static ContactClient contactClient;
        public static UserClient userClient;
        public static ServicesClient servicesClient;
        public static FinancialsClient financeClient;
        public static TransactionsClient transactionsClient;
        public static PayMethodClient PayMethodClient;
        public static PlanClient planClient;
        public static ReportClient reportClient;
        public static MessageClient messageClient;

        public static TokenResponse AccessToken;
        public static List<SiteResponse> Sites;
        public static SiteResponse CurrentSites;
        public static TokenResponse CustomerToken;
        public static UserModel LoggedUser;
        public static bool HasVerticalScroll;

        public static void CloseDialog(string identify = "MyDialogHost")
        {
            try
            {
                MaterialDesignThemes.Wpf.DialogHost.Close(identify);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static Dictionary<string, string> GetHeader(TokenResponse token)
        {
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Authorization", token.Type + " " + token.Credentials);
            return header;
        }

        public static Dictionary<string, object> BuildDictionary(string key, object value, Dictionary<string, object> dic = null)
        {
            if (dic == null) dic = new Dictionary<string, object>();
            dic.Add(key, value);
            return dic;
        }

        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        public static void WriteLogFile(RestResponse response)
        {
            try
            {
                if (response.ResponseUri == null) return;

                if (!File.Exists(@"logs.txt"))
                {
                    using (StreamWriter sw = File.CreateText(@"logs.txt"))
                    {
                        sw.WriteLine("API Logs");
                    }
                }

                using (StreamWriter sw = File.AppendText(@"logs.txt"))
                {
                    sw.WriteLine("------------------------------");
                    sw.WriteLine("URL - " + response.ResponseUri.AbsolutePath + "#" + response.Request.Method.ToString() + Environment.NewLine +
                    "Request Params - " + JsonConvert.SerializeObject(response.Request.Parameters) + Environment.NewLine +
                    "Response - " + response.StatusCode + " - " + response.Content);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void WriteLogFile(string body)
        {
            try
            {
                if (!File.Exists(@"logs.txt"))
                {
                    using (StreamWriter sw = File.CreateText(@"logs.txt"))
                    {
                        sw.WriteLine("API Logs");
                    }
                }

                using (StreamWriter sw = File.AppendText(@"logs.txt"))
                {
                    sw.WriteLine(body);
                }
            }catch{}
        }

        public static T RequestAPI<T>(string url, Method method, Dictionary<string, string> header, Dictionary<string, object> path, 
            Dictionary<string, object> query, Dictionary<string, object> body, bool showMessage = true)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (header != null) foreach (KeyValuePair<string, string> entry in header) request.AddHeader(entry.Key, entry.Value);
            if (path != null) foreach (KeyValuePair<string, object> entry in path) request.AddUrlSegment(entry.Key, entry.Value.ToString());
            if (query != null) foreach (KeyValuePair<string, object> entry in query) request.AddQueryParameter(entry.Key, entry.Value.ToString());
            if (body != null) request.AddBody(body, "application/json");

            WriteLogFile("URL - " + url + "#" + method.ToString());
            WriteLogFile("Request Params - " + JsonConvert.SerializeObject(request.Parameters));

            var response = client.Execute<T>(request, method);
            WriteLogFile("Response - " + response.StatusCode + " - " + response.Content);
            if (showMessage) MessageUtils.ParseResponse(response);
            return response.Data;
        }

        public static T RequestAPI<T>(string url, Method method, Dictionary<string, string> header, Dictionary<string, object> path,
           Dictionary<string, object> query, string body, bool showMessage = true)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (header != null) foreach (KeyValuePair<string, string> entry in header) request.AddHeader(entry.Key, entry.Value);
            if (path != null) foreach (KeyValuePair<string, object> entry in path) request.AddUrlSegment(entry.Key, entry.Value.ToString());
            if (query != null) foreach (KeyValuePair<string, object> entry in query) request.AddQueryParameter(entry.Key, entry.Value.ToString());
            if (!string.IsNullOrEmpty(body)) request.AddBody(body, "application/json");

            WriteLogFile("URL - " + url + "#" + method.ToString());
            WriteLogFile("Request Params - " + JsonConvert.SerializeObject(request.Parameters));

            var response = client.Execute<T>(request, method);
            WriteLogFile("Response - " + response.StatusCode + " - " + response.Content);

            if (showMessage) MessageUtils.ParseResponse(response);
            if ((url == Constants.SimpleUserURL || url == Constants.FullUserURL) && response.StatusCode == HttpStatusCode.NoContent) 
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Not_Register_User);

            return response.Data;
        }

        public static HttpStatusCode RequestAPI(string url, Method method, Dictionary<string, string> header, Dictionary<string, object> path,
          Dictionary<string, object> query, Dictionary<string, object> body, bool showMessage = true)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (header != null) foreach (KeyValuePair<string, string> entry in header) request.AddHeader(entry.Key, entry.Value);
            if (path != null) foreach (KeyValuePair<string, object> entry in path) request.AddUrlSegment(entry.Key, entry.Value.ToString());
            if (query != null) foreach (KeyValuePair<string, object> entry in query) request.AddQueryParameter(entry.Key, entry.Value.ToString());
            if (body != null) request.AddBody(body, "application/json");

            WriteLogFile("URL - " + url + "#" + method.ToString());
            WriteLogFile("Request Params - " + JsonConvert.SerializeObject(request.Parameters));

            var response = client.Execute(request, method);
            WriteLogFile("Response - " + response.StatusCode + " - " + response.Content);
            if (showMessage) MessageUtils.ParseResponse(response);
            return response.StatusCode;
        }

        public static Dictionary<string, object> RequestAPIWithStatusCode(string url, Method method, Dictionary<string, string> header, Dictionary<string, object> path, Dictionary<string, object> query, string body)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest();

            if (header != null) foreach (KeyValuePair<string, string> entry in header) request.AddHeader(entry.Key, entry.Value);
            if (path != null) foreach (KeyValuePair<string, object> entry in path) request.AddUrlSegment(entry.Key, entry.Value.ToString());
            if (query != null) foreach (KeyValuePair<string, object> entry in query) request.AddQueryParameter(entry.Key, entry.Value.ToString());
            if (!string.IsNullOrEmpty(body)) request.AddBody(body, "application/json");

            WriteLogFile("URL - " + url + "#" + method.ToString());
            WriteLogFile("Request Params - " + JsonConvert.SerializeObject(request.Parameters));

            var response = client.Execute<Dictionary<string, object>>(request, method);
            WriteLogFile("Response - " + response.StatusCode + " - " + response.Content);

            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("data", response.Data);
            result.Add("code", response.StatusCode);

            return result;
        }

    }
}
