using RestSharp;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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

namespace SelcommWPF.dialogs.services
{
    /// <summary>
    /// Interaction logic for EnquiryPassword.xaml
    /// </summary>
    public partial class EnquiryPassword : UserControl
    {

        private string ServiceReference;

        public EnquiryPassword(string serviceReference)
        {
            InitializeComponent();
            InitializeControl();
            ServiceReference = serviceReference;    
        }

        private void InitializeControl()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("Resource", "/Services/EnquiryPassword");
                query.Add("api-version", 1.2);
                HttpStatusCode result = Global.RequestAPI(Constants.AuthrisationURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, null);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result == HttpStatusCode.OK) GetEnquiryPassword();
                    else
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources1.Feature_Error);
                        Global.CloseDialog();
                    }
                });

            }, 10);
        }

        private void GetEnquiryPassword()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> result = Global.RequestAPI<Dictionary<string, string>>(Constants.ServicePasswordURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ServiceReference", ServiceReference), Global.BuildDictionary("api-version", 1.2), "");
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    TextPassword.Password = result["EnquiryPassword"];
                    ButtonSave.IsEnabled = false;
                });

            }, 10);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, object> body = Global.BuildDictionary("EnquiryPassword", TextPassword.Password);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.ServicePasswordURL, Method.Put, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("ServiceReference", ServiceReference), Global.BuildDictionary("api-version", 1.2), body, false);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK || result == HttpStatusCode.Created) CloseDialog();
                    else if (result == HttpStatusCode.Forbidden)
                    {
                        MessageUtils.ShowErrorMessage("", "You do not have permission for this feature. Please contact your Administrator.");
                        Global.CloseDialog();
                    }
                });

            }, 10);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void TextPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ButtonSave.IsEnabled = TextPassword.Password.Length > 0;
        }

        private void TextPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            Dictionary<string, object> body = Global.BuildDictionary("EnquiryPassword", TextPassword.Password);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.ServicePasswordURL, Method.Put, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("ServiceReference", ServiceReference), Global.BuildDictionary("api-version", 1.2), body, false);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK || result == HttpStatusCode.Created) CloseDialog();
                    else if (result == HttpStatusCode.Forbidden)
                    {
                        MessageUtils.ShowErrorMessage("", "You do not have permission for this feature. Please contact your Administrator.");
                        Global.CloseDialog();
                    }
                });

            }, 10);
        }

        private void CloseDialog()
        {
            MessageUtils.ShowMessage("", "Enquiry password changed successfully.");
            EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    Global.CloseDialog();
                    Global.mainWindow.LoadTabInfoDetail();
                });

            }, 4000);
        }

    }
}
