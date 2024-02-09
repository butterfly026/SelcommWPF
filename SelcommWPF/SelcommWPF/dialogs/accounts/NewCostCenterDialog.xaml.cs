using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using Newtonsoft.Json;
using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.services;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.PeerToPeer.Collaboration;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SelcommWPF.dialogs.accounts
{
    /// <summary>
    /// Interaction logic for EventDialog.xaml
    /// </summary>
    public partial class NewCostCenterDialog : UserControl
    {
        private string ContactCode;

        public NewCostCenterDialog(string contactCode)
        {
            InitializeComponent();
            InitializeControl();
            ContactCode = contactCode;
        }

        private void InitializeControl()
        {            
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("Resource", "/Contacts/CostCenters/New");
                query.Add("api-version", 1.2);
                HttpStatusCode result = Global.RequestAPI(Constants.AuthrisationURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, null);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result != HttpStatusCode.OK)
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources1.Feature_Error);
                        Global.CloseDialog();
                    }
                    List<string> StatusSource = new List<string>();
                    StatusSource.Add("Open");
                    StatusSource.Add("Closed");
                    StatusSource.Add("Cancelled");
                    StatusSource.Add("OnHold");

                    ComboStatus.ItemsSource = StatusSource;
                    ComboStatus.SelectedIndex = 0;
                });

            }, 10);
        }

        private void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            string email = TextEmail.Text;
            var regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            if(!email.IsNullOrEmpty() && !regex.IsMatch(email))
            {
                MessageUtils.ShowErrorMessage("", "Please input valid email address");
                return;
            }


            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("CustomerReference", TextCustomerRef.Text);
            body.Add("Name", TextName.Text);
            body.Add("AdditionalInformation1", TextInfo1.Text);
            body.Add("AdditionalInformation2", TextInfo2.Text);
            body.Add("AdditionalInformation3", TextInfo3.Text);
            body.Add("GeneralLedgerAccountCode", TextAccCode.Text);
            body.Add("AggregationPoint", CheckAggregationPoint.IsChecked);
            body.Add("Status", ComboStatus.SelectedValue);
            body.Add("Email", TextEmail.Text);
            body.Add("EFXId", TextEFXId.Text);
            
            LoadingPanel.Visibility = Visibility;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.CostCenterCreateURL, Method.Post, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("contactCode", ContactCode), Global.BuildDictionary("api-version", 1.2), body, false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;

                    if (result == HttpStatusCode.Created)
                    {
                        MessageUtils.ShowMessage("", "Cost Center Created Successfully");
                        Global.CostCenterListDialog.ShowCostCenterList(0, 20);
                        Global.CloseDialog("DetailDialog");
                    }
                    else
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources1.Create_Error);
                        Global.CloseDialog("DetailDialog");
                    }
                });
            }, 10);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }

        private void CheckCreatable()
        {
            ButtonCreate.IsEnabled = !TextName.Text.IsNullOrEmpty();
        }

        private void TextName_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckCreatable();
        }
    }
}
