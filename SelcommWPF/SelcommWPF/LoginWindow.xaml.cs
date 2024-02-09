using Microsoft.Win32;
using RestSharp;
using SelcommWPF.clients;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.auths;
using SelcommWPF.dialogs;
using SelcommWPF.dialogs.auths;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace SelcommWPF
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            Global.userClient = new UserClient();
            this.Width = Global.ScreenWidth;
            this.Height = Global.ScreenHeigth - 30;
            TextUserID.Focus();
        }

        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            DoLogin();
        }

        private async void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            bool result = await MessageUtils.ConfirmMessageAsync(Properties.Resources.Alert, Properties.Resources.Finish);
            if (result) Application.Current.Shutdown();
        }

        private void TextPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoLogin();
            }
        }

        private void DoLogin()
        {
            string userId = TextUserID.Text;
            if (userId == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_User_Id);
                return;
            }

            string password = TextPassword.Password;
            if (password == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Password);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            Global.UserID = userId;
            Global.Password = password;
            Global.loginWindow = this;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> header = new Dictionary<string, string>();
                header.Add("Content-Type", "application/json");
                header.Add("Authorization", Global.AccessToken.Type + " " + Global.AccessToken.Credentials);
                Dictionary<string, object> path = new Dictionary<string, object>();
                path.Add("SiteId", Constants.SiteId);
                path.Add("identifier", userId);
                Dictionary<string, object> query = new Dictionary<string, object>();
                query.Add("api-version", 3.1);
                TokenResponse accessToken = Global.RequestAPI<TokenResponse>(Constants.RefreshTokenURL, Method.Post, header, path, query, string.Format("\"{0}\"", password), false);

                header["Authorization"] = accessToken.Type + " " + accessToken.Credentials;
                Global.CustomerToken = Global.RequestAPI<TokenResponse>(Constants.CustomerTokenURL, Method.Post, header, null, query, "", false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (accessToken == null && accessToken.Credentials == null || Global.CustomerToken == null || Global.CustomerToken.Credentials == null)
                    {
                        MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Login_Fail);
                        LoginDenied();
                        return;
                    }

                    Global.AccessToken = accessToken;
                    JwtSecurityToken jwtToken = new JwtSecurityToken(Global.CustomerToken.Credentials);
                    Global.ContactId = jwtToken.Payload["contact.code"].ToString().Trim();
                    LoginApproved();
                });

            }, 10);

        }

        private void CloseApplication()
        {
            EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    Application.Current.Shutdown();
                });
            }, 1000);
        }

        private async void Button_Forgot_Password_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) MaterialDesignThemes.Wpf.DialogHost.Close("MyDialogHost");
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ResetPassword(), "MyDialogHost");
        }

        private void LoginDenied()
        {
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("LoginId", Global.UserID);
            body.Add("Password", Global.Password);
            body.Add("ip", "");
            body.Add("Location", "");
            body.Add("MFA", true);
            body.Add("OTP", true);
            body.Add("Tor", true);
            body.Add("Proxy", true);
            body.Add("Anonymous", false);
            body.Add("KnownAttacker", false);
            body.Add("KnownAbuser", false);
            body.Add("Threat", false);
            body.Add("Bogon", true);

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> header = new Dictionary<string, string>();
                header.Add("Authorization", Global.AccessToken.Type + " " + Global.AccessToken.Credentials);
                Dictionary<string, object> query = new Dictionary<string, object>();
                query.Add("api-version", 1.0);
                Global.RequestAPI<Dictionary<string, object>>(Constants.LoginDeniedURL, Method.Post, header, null, query, body);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    CloseApplication();
                });
            }, 10);
        }

        private void LoginApproved()
        {
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("LoginId", Global.UserID);
            body.Add("Password", Global.Password);
            body.Add("ip", "");
            body.Add("Location", "");
            body.Add("MFA", true);
            body.Add("OTP", true);
            body.Add("Tor", true);
            body.Add("Proxy", true);
            body.Add("Anonymous", false);
            body.Add("KnownAttacker", false);
            body.Add("KnownAbuser", false);
            body.Add("Threat", false);
            body.Add("Bogon", true);

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> header = new Dictionary<string, string>();
                header.Add("Authorization", Global.CustomerToken.Type + " " + Global.CustomerToken.Credentials);
                Dictionary<string, object> path = new Dictionary<string, object>();
                path.Add("UserId", Global.ContactId);
                Dictionary<string, object> query = new Dictionary<string, object>();
                query.Add("api-version", 1.0);
                Global.RequestAPI<Dictionary<string, object>>(Constants.LoginApprovedURL, Method.Post, header, path, query, body);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    CheckMFAStatus();
                });

            }, 10);
        }

        public async void CheckMFAStatus()
        {
            if (Global.CurrentSites.TwoFactorAuthEmail || Global.CurrentSites.TwoFactorAuthSMS)
            {
                if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
                await MaterialDesignThemes.Wpf.DialogHost.Show(new TwoFactorAuth(), "MyDialogHost");
            }
            else
            {
                CheckPasswordMustChange();
            }
        }

        public void CheckPasswordMustChange()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> header = new Dictionary<string, string>();
                header.Add("Authorization", Global.CustomerToken.Type + " " + Global.CustomerToken.Credentials);
                Dictionary<string, object> path = new Dictionary<string, object>();
                path.Add("ContactCode", Global.ContactId);
                Dictionary<string, object> query = new Dictionary<string, object>();
                query.Add("api-version", 1.0);
                PasswordInfo result = Global.RequestAPI<PasswordInfo>(Constants.PasswordInformationURL, Method.Get, header, path, query, "");

                Application.Current.Dispatcher.Invoke(async delegate
                {
                    DateTime expiryDate = DateTime.Parse(result.ExpiryDate ?? "9999-12-31 12:59:59");
                    if (result.MustChange || expiryDate <= DateTime.Now)
                    {
                        if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
                        await MaterialDesignThemes.Wpf.DialogHost.Show(new ChangePassword(true), "MyDialogHost");
                    }
                    else
                    {
                        CheckPasswordChangeWarning();
                    }
                });

            }, 10);

        }

        public async void CheckPasswordChangeWarning()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            bool isDisplay = Convert.ToBoolean(key.GetValue("PasswordExpiry", false));
            key.Close();

            if (isDisplay)
            {
                CheckLoginHistory();
            }
            else
            {
                if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
                await MaterialDesignThemes.Wpf.DialogHost.Show(new PasswordExpiry(true), "MyDialogHost");
            }
        }

        public async void CheckLoginHistory()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            bool isDisplay = Convert.ToBoolean(key.GetValue("LoginHistory", false));
            key.Close();

            if (isDisplay)
            {
                CheckRegisterDetails();
            }
            else
            {
                if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
                await MaterialDesignThemes.Wpf.DialogHost.Show(new dialogs.auths.LoginHistory(true), "MyDialogHost");
            }
        }

        public async void CheckRegisterDetails()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            bool isDisplay = Convert.ToBoolean(key.GetValue("RegisterDetails", false));
            key.Close();

            if (isDisplay)
            {
                CheckVerifyDetails();
            }
            else
            {
                if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
                await MaterialDesignThemes.Wpf.DialogHost.Show(new RegisterSecurity(true), "MyDialogHost");
            }
        }

        public async void CheckVerifyDetails()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            bool isDisplay = Convert.ToBoolean(key.GetValue("VerifyDetails", false));
            key.Close();

            if (isDisplay)
            {
                new MainWindow().Show();
                this.Close();
            }
            else
            {
                if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
                await MaterialDesignThemes.Wpf.DialogHost.Show(new VerifySecurity(true), "MyDialogHost");
            }
        }



    }

}
