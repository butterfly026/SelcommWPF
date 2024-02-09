using Microsoft.Win32;
using RestSharp;
using SelcommWPF.clients.models.auths;
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
    /// Interaction logic for VerifySecurity.xaml
    /// </summary>
    public partial class VerifySecurity : UserControl
    {
        private bool IsLoadingPage;
        private string Email = "";
        private string Mobile = "";
        private string PIN = "";
        private DateTime PINExpireTime = new DateTime();

        public VerifySecurity(bool isLogin = false)
        {
            IsLoadingPage = isLogin;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            bool isDisplay = Convert.ToBoolean(key.GetValue("VerifyDetails", false));
            key.Close();
            CheckBoxDisplay.IsChecked = isDisplay;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                PasswordInfo result = Global.RequestAPI<PasswordInfo>(Constants.PasswordInformationURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("ContactCode", Global.ContactId), Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    CheckBoxEmail.Visibility = !result.EmailRegistered || result.EmailVerified ? Visibility.Collapsed : Visibility.Visible;
                    LabelEmailVerified.Visibility = result.EmailRegistered && result.EmailVerified ? Visibility.Visible : Visibility.Collapsed;
                    CheckBoxEmail.Content = result.EmailRegistered && !result.EmailVerified ? string.Format("{0} : {1}", Properties.Resources.Email, result.Email) : "";
                    LabelEmailVerified.Content = result.EmailRegistered && result.EmailVerified ? string.Format("{0} : {1}", Properties.Resources.Email, Properties.Resources.Verified) : "";
                    Email = result.Email;

                    CheckBoxMobile.Visibility = !result.MobileRegistered || result.MobileVerified ? Visibility.Collapsed : Visibility.Visible;
                    LabelMobileVerified.Visibility = result.MobileRegistered && result.MobileVerified ? Visibility.Visible : Visibility.Collapsed;
                    CheckBoxMobile.Content = result.MobileRegistered && !result.MobileVerified ? string.Format("{0} : {1}", Properties.Resources.Mobile, result.Mobile) : "";
                    LabelMobileVerified.Content = result.MobileRegistered && result.MobileVerified ? string.Format("{0} : {1}", Properties.Resources.Mobile, Properties.Resources.Verified) : "";
                    Mobile = result.Mobile;
                    ButtonSend.IsEnabled = !(Email == null && Mobile == null);
                });

            }, 10);

        }

        private void Button_Send_Click(object sender, RoutedEventArgs e)
        {
            string email = CheckBoxEmail.IsChecked.Value ? Email : "";
            string mobile = CheckBoxMobile.IsChecked.Value ? Mobile : "";

            LoadingPanel.Visibility = Visibility.Visible;
            Email = email;
            Mobile = mobile;
            GeneratePINCode(1, email, mobile);
        }

        private void GeneratePINCode(int type, string email, string mobile)
        {
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> body = Global.BuildDictionary("Application", new string[] { "EMAIL_VALIDATE_SERVICE_DESK", "SMS_VALIDATION_SERVICE_DESK" }[type - 1]);
                if (type == 1) body.Add("Email", email);
                else if (type == 2) body.Add("Mobile", mobile);
                Dictionary<string, string> result = Global.RequestAPI<Dictionary<string, string>>(Constants.SendPINURL, Method.Post, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("ContactCode", Global.ContactId), Global.BuildDictionary("api-version", 1.0), body);

                if (result != null)
                {
                    PINExpireTime = DateTime.Parse(result["Expiry"]);
                    if (PIN != null && PINExpireTime != null)
                    {
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            if (type == 1)
                            {
                                if (mobile != "") GeneratePINCode(2, email, mobile);
                                else
                                {
                                    LoadingPanel.Visibility = Visibility.Hidden;
                                    LabelExpireTime.Content = DateTime.Now.AddMinutes(4).ToString("h:mm tt");
                                    Grid2FAStep1.Visibility = Visibility.Collapsed;
                                    Grid2FAStep2.Visibility = Visibility.Visible;

                                    TextEmailPINCode.Visibility = Email == "" ? Visibility.Collapsed : Visibility.Visible;
                                    TextMobilePINCode.Visibility = Mobile == "" ? Visibility.Collapsed : Visibility.Visible;
                                }
                            }
                            else if (type == 2)
                            {
                                LoadingPanel.Visibility = Visibility.Hidden;
                                LabelExpireTime.Content = DateTime.Now.AddMinutes(4).ToString("h:mm tt");
                                Grid2FAStep1.Visibility = Visibility.Collapsed;
                                Grid2FAStep2.Visibility = Visibility.Visible;

                                TextEmailPINCode.Visibility = Email == "" ? Visibility.Collapsed : Visibility.Visible;
                                TextMobilePINCode.Visibility = Mobile == "" ? Visibility.Collapsed : Visibility.Visible;
                            }
                        });
                    }
                }

            }, 10);
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
            if (IsLoadingPage)
            {
                new MainWindow().Show();
                Global.loginWindow.Close();
            }
        }

        private void Button_Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (Email != "") ConfirmPIN(1, TextEmailPINCode.Text, "EMAIL_VALIDATE_SERVICE_DESK");
            if (Mobile != "") ConfirmPIN(2, TextMobilePINCode.Text, "SMS_VALIDATE_SERVICE_DESK");
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            Grid2FAStep1.Visibility = Visibility.Visible;
            Grid2FAStep2.Visibility = Visibility.Collapsed;
        }

        private void TextPINCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            string pin = ((TextBox)sender).Text;
            ButtonConfirm.IsEnabled = pin.Length > 0;
        }

        private void ConfirmPIN(int type, string PIN, string application)
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> header = Global.GetHeader(Global.CustomerToken);
                Dictionary<string, object> path = Global.BuildDictionary("PIN", PIN);
                path.Add("ContactCode", Global.ContactId);
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                Dictionary<string, object> body = Global.BuildDictionary("Application", application);
                body.Add("ContactId", Global.ContactId);
                HttpStatusCode result = Global.RequestAPI(Constants.ConsumePINURL, Method.Patch, header, path, query, body, false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (type == 1)
                    {
                        if (Mobile != "") ConfirmPIN(2, TextMobilePINCode.Text, "SMS_VALIDATE_SERVICE_DESK");
                        else
                        {
                            if (result == HttpStatusCode.OK) Global.CloseDialog();
                            else MessageUtils.ShowErrorMessage("", Properties.Resources.Invalid_PIN);
                        }
                    }
                    else if (type == 2)
                    {
                        if (result == HttpStatusCode.OK) Global.CloseDialog();
                        else MessageUtils.ShowErrorMessage("", Properties.Resources.Invalid_PIN);
                    }
                });

            }, 10);
        }

        private void CheckBoxDisplay_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = e.RoutedEvent.Name == "Checked";
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            key.SetValue("VerifyDetails", isChecked);
            key.Close();
        }
    }
}