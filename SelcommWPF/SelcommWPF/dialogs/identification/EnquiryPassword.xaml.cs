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

namespace SelcommWPF.dialogs.identification
{
    /// <summary>
    /// Interaction logic for EnquiryPassword.xaml
    /// </summary>
    public partial class EnquiryPassword : UserControl
    {
        public EnquiryPassword()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("Resource", "/Contacts/EnquiryPassword");
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
                Dictionary<string, string> result = Global.RequestAPI<Dictionary<string, string>>(Constants.EnquiryPasswordURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.1), "");
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

        private void TextBox_TextChanged(object sender, RoutedEventArgs e)
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
                HttpStatusCode result = Global.RequestAPI(Constants.EnquiryPasswordURL, Method.Put, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.1), body, false);
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

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, object> body = Global.BuildDictionary("EnquiryPassword", TextPassword.Password);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.EnquiryPasswordURL, Method.Put, Global.GetHeader(Global.CustomerToken), 
                    Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.1), body, false);
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

        private void CloseDialog()
        {
            MessageUtils.ShowMessage("", "Enquiry password changed successfully.");
            EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    Global.CloseDialog();
                    Global.mainWindow.GetDisplayDetails();
                });

            }, 4000);
        }

    }
}
