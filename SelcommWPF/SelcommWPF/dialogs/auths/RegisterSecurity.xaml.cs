using Microsoft.Win32;
using RestSharp;
using SelcommWPF.clients.models.auths;
using SelcommWPF.clients.models.messages;
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

namespace SelcommWPF.dialogs.auths
{
    /// <summary>
    /// Interaction logic for RegisterSecurity.xaml
    /// </summary>
    public partial class RegisterSecurity : UserControl
    {
        private bool IsLoginPage = false;
        private PasswordInfo PasswordInformation;

        public RegisterSecurity(bool isLogin = false)
        {
            IsLoginPage = isLogin;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            bool isDisplay = Convert.ToBoolean(key.GetValue("RegisterDetails", false));
            key.Close();
            CheckBoxDisplay.IsChecked = isDisplay;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                PasswordInformation = Global.RequestAPI<PasswordInfo>(Constants.PasswordInformationURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("ContactCode", Global.ContactId), Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (PasswordInformation == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    PasswordInformation.Email = PasswordInformation.Email ?? "";
                    PasswordInformation.Mobile = PasswordInformation.Mobile ?? "";
                    TextEmailAddress.Text = PasswordInformation.Email;
                    TextMobileNumber.Text = PasswordInformation.Mobile;

                    TextEmailAddress.SelectionStart = TextEmailAddress.Text.Length;
                    ButtonEmailRemove.IsEnabled = PasswordInformation.EmailRegistered;
                    ButtonMobileRemove.IsEnabled = PasswordInformation.MobileRegistered;
                    ButtonEmailRegister.IsEnabled = false;
                    ButtonMobileRegister.IsEnabled = false;
                });

            }, 10);

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
            if (IsLoginPage) Global.loginWindow.CheckVerifyDetails();
        }

        private void CheckBoxDisplay_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = e.RoutedEvent.Name == "Checked";
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            key.SetValue("RegisterDetails", isChecked);
            key.Close();
        }

        private void TextEmailAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            string param = TextEmailAddress.Text;
            if (param == PasswordInformation.Email) return;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                ValidModel result = Global.RequestAPI<ValidModel>(Constants.ValidateEmail, Method.Get, Global.GetHeader(Global.CustomerToken), 
                    Global.BuildDictionary("Param", param), Global.BuildDictionary("api-version", 1.0), "", false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result.Valid) CheckUniqueAuthInfo(Constants.CheckEmailURL, param, Properties.Resources.Authenticate_Email);
                    else
                    {
                        MessageUtils.ShowErrorMessage("", string.Format(Properties.Resources.Invalid_Message, Properties.Resources.Authenticate_Email));
                        LoadingPanel.Visibility = Visibility.Hidden;
                    }
                });

            }, 10);
        }

        private void TextMobileNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            string param = TextMobileNumber.Text;
            if (param == PasswordInformation.Mobile) return;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                ValidModel result = Global.RequestAPI<ValidModel>(Constants.ValidateSMS, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("Param", param), Global.BuildDictionary("api-version", 1.0), "", false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result.Valid) CheckUniqueAuthInfo(Constants.CheckMobileURL, param, Properties.Resources.Authenticate_Mobile);
                    else
                    {
                        MessageUtils.ShowErrorMessage("", string.Format(Properties.Resources.Invalid_Message, Properties.Resources.Authenticate_Mobile));
                        LoadingPanel.Visibility = Visibility.Hidden;
                    }
                });

            }, 10);
        }

        private void CheckUniqueAuthInfo(string url, string param, string tag)
        {
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, bool> result = Global.RequestAPI<Dictionary<string, bool>>(url, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("Parameter", param), Global.BuildDictionary("api-version", 1.0), "", false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null || result["Used"])
                    {
                        MessageUtils.ShowErrorMessage("", string.Format(Properties.Resources.Choose_Another_Message, tag));
                        if (tag == Properties.Resources.Authenticate_Email) ButtonEmailRegister.IsEnabled = false;
                        else if (tag == Properties.Resources.Authenticate_Mobile) ButtonMobileRegister.IsEnabled = false;
                    }
                    else
                    {
                        MessageUtils.ShowMessage("", string.Format(Properties.Resources.Available_Message, tag));
                        if (tag == Properties.Resources.Authenticate_Email) ButtonEmailRegister.IsEnabled = true;
                        else if (tag == Properties.Resources.Authenticate_Mobile) ButtonMobileRegister.IsEnabled = true;
                    }
                });

            }, 10);

        }

        private async void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string param = "", message = "";
            bool isRemove = false;
            Dictionary<string, object> body = new Dictionary<string, object>();

            switch (button.Name)
            {
                case "ButtonEmailRegister":
                    param = "Email";
                    message = Properties.Resources.Register_Email_Success;
                    body.Add("Email", TextEmailAddress.Text);
                    break;
                case "ButtonEmailRemove":
                    param = "Email";
                    body.Add("Email", "");
                    message = Properties.Resources.Remove_Email_Success;
                    isRemove = true;
                    break;
                case "ButtonMobileRegister":
                    param = "Mobile";
                    body.Add("MobileNumber", TextMobileNumber.Text);
                    message = Properties.Resources.Register_Mobile_Success;
                    break;
                case "ButtonMobileRemove":
                    param = "Mobile";
                    body.Add("MobileNumber", "");
                    message = Properties.Resources.Remove_Mobile_Success;
                    isRemove = true;
                    break;
            }

            if (isRemove)
            {
                bool ret = await MessageUtils.ConfirmMessageAsync("", Properties.Resources.Remove_Contact_Message);
                if (!ret) return;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> path = Global.BuildDictionary("Param", param);
                path.Add("UserId", Global.ContactId);
                HttpStatusCode result = Global.RequestAPI(Constants.RegisterEmailOrMobileURL, Method.Put, Global.GetHeader(Global.CustomerToken), path, 
                    Global.BuildDictionary("api-version", 1.0), body);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result == HttpStatusCode.OK)
                    {
                        MessageUtils.ShowMessage("", message);
                        InitializeControl();
                    }
                });

            }, 10);

        }
    }
}
