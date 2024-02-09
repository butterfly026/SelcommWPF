using Microsoft.Win32;
using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.global;
using SelcommWPF.utils;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SelcommWPF.dialogs
{
    /// <summary>
    /// Interaction logic for ChangePassword.xaml
    /// </summary>
    public partial class ChangePassword : UserControl
    {

        private bool IsLoginPage = false;
        private ComplexResponse ComplexResult = new ComplexResponse();

        public ChangePassword(bool isLogin = false)
        {
            IsLoginPage = isLogin;
            InitializeComponent();
        }

        private void Suggestion_Password_Click(object sender, MouseButtonEventArgs e)
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> SuggestionPassword = Global.RequestAPI<Dictionary<string, string>>(Constants.SuggestopnURL, Method.Get,
                    Global.GetHeader(Global.CustomerToken), null, Global.BuildDictionary("api-version", 1.0), "");
                if (SuggestionPassword == null || SuggestionPassword["Password"] == null) return;

                Application.Current.Dispatcher.Invoke(async delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    bool result = await MessageUtils.ConfirmMessageAsync(Properties.Resources.Alert, string.Format("{0}\n{1}", Properties.Resources.Suggestion_Password, SuggestionPassword["Password"]));

                    if (result)
                    {
                        TextNewPassword.Password = SuggestionPassword["Password"];
                        TextConfirm.Password = SuggestionPassword["Password"];

                        LabelComplex.Content = "Password is strong";
                        BorderComplex.Visibility = Visibility.Visible;
                        ComplexResult.PasswordStrength = "Strong";
                        ComplexResult.Reason = "";
                        ComplexResult.Result = "SUCCESS";

                        BorderComplex.Background = Brushes.Green;
                        LabelComplex.Foreground = Brushes.Black;
                    }
                });

            }, 10);
        }

        private void TextNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            string password = TextNewPassword.Password;
            if (password != "") CheckPasswordComplexity(password);
            CheckPasswordMatch();
        }

        private void TextOldPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            CheckPasswordMatch();
        }

        private void TextConfirm_PasswordChanged(object sender, RoutedEventArgs e)
        {
            CheckPasswordMatch();
        }

        private void CheckPasswordMatch()
        {
            if (TextOldPassword.Password != "" && TextNewPassword.Password != "" && TextConfirm.Password != "" &&
                TextConfirm.Password == TextNewPassword.Password)
            {
                ButtonChange.IsEnabled = true;
            }
            else
            {
                ButtonChange.IsEnabled = false;
            }
        }

        private void TextNewPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            string password = TextNewPassword.Password;
            if (password != "") CheckPasswordComplexity(password);
        }

        private void CheckPasswordComplexity(string password)
        {
            ProgressComplex.Visibility = Visibility.Visible;
            BorderComplex.Visibility = Visibility.Collapsed;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("password", password);
                query.Add("api-version", 1.0);
                ComplexResult = Global.RequestAPI<ComplexResponse>(Constants.ComplexURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    ProgressComplex.Visibility = Visibility.Collapsed;
                    LabelComplex.Content = ComplexResult.Result == "SUCCESS" ? string.Format(Properties.Resources.Password_Strength, ComplexResult.PasswordStrength) : ComplexResult.Reason;
                    BorderComplex.Visibility = Visibility.Visible;

                    Brush[] brushes = new Brush[] { Brushes.Red, Brushes.Red, Brushes.Yellow, Brushes.Green };
                    Brush[] brushes1 = new Brush[] { Brushes.White, Brushes.White, Brushes.Black, Brushes.White };
                    List<string> type = new List<string> { "Unacceptable", "Weak", "Medium", "Strong"};
                    BorderComplex.Background = brushes[type.IndexOf(ComplexResult.PasswordStrength)];
                    LabelComplex.Foreground = brushes1[type.IndexOf(ComplexResult.PasswordStrength)];
                });

            }, 10);

        }

        private void Button_Change_Click(object sender, RoutedEventArgs e)
        {
            string oldPassword = TextOldPassword.Password;
            string newPassword = TextNewPassword.Password;
            string confirm = TextConfirm.Password;

            if (oldPassword == "")
            {
                MessageUtils.ShowMessage(Properties.Resources.Error, Properties.Resources.Enter_Old_Password);
                TextOldPassword.Focus();
                return;
            }

            if (newPassword == "")
            {
                MessageUtils.ShowMessage(Properties.Resources.Error, Properties.Resources.Enter_New_Password);
                TextNewPassword.Focus();
                return;
            }

            if (newPassword != confirm)
            {
                MessageUtils.ShowMessage(Properties.Resources.Error, Properties.Resources.Not_Match_Password);
                TextConfirm.Focus();
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> path = Global.BuildDictionary("SiteId", Constants.SiteId);
                path.Add("identifier", Global.UserID);
                Dictionary<string, object> body = Global.BuildDictionary("currentPassword", oldPassword);
                body.Add("newPassword", newPassword);
                HttpStatusCode code = Global.RequestAPI(Constants.ChangePasswordURL, Method.Post, Global.GetHeader(Global.AccessToken), path, 
                    Global.BuildDictionary("api-version", 3.1), body);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (code == HttpStatusCode.NoContent)
                    {
                        MessageUtils.ShowMessage(Properties.Resources.Alert, Properties.Resources.Password_Change_Sucess);
                        Global.CloseDialog();
                        if (IsLoginPage) Global.loginWindow.CheckLoginHistory();
                    }
                    else
                    {
                        MessageUtils.ShowErrorMessage(Properties.Resources.Error, string.Format(Properties.Resources.Password_Change_Error, code.ToString()));
                    }
                });

            }, 10);

        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
            if (IsLoginPage) Global.loginWindow.CheckLoginHistory();
        }

    }
}
