using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.messages;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
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

namespace SelcommWPF.dialogs.users
{
    /// <summary>
    /// Interaction logic for ResetPassword.xaml
    /// </summary>
    public partial class ResetPassword : UserControl
    {
        private string ContactCode;
        private ComplexResponse ComplexResult = new ComplexResponse();

        public ResetPassword(string code)
        {
            ContactCode = code;
            InitializeComponent();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("Password", TextPassword.Password);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.ResetPassowrdURL, Method.Post, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("UserId", ContactCode), Global.BuildDictionary("api-version", 1.0), body);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK || result == HttpStatusCode.Created)
                    {
                        MessageUtils.ShowMessage("", "Password reset successfully");
                        Global.CloseDialog("PasswordDialog");
                    }
                });

            }, 10);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("PasswordDialog");
        }

        private void TextPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            string password = TextPassword.Password;
            if (password == "") return;

            ProgressComplex.Visibility = Visibility.Visible;
            BorderComplex.Visibility = Visibility.Collapsed;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("password", password);
                ComplexResult = Global.RequestAPI<ComplexResponse>(Constants.ComplexURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, "");
                Application.Current.Dispatcher.Invoke(delegate
                {
                    ProgressComplex.Visibility = Visibility.Collapsed;
                    LabelComplex.Content = ComplexResult.Result == "SUCCESS" ? string.Format(Properties.Resources.Password_Strength, ComplexResult.PasswordStrength) : ComplexResult.Reason;
                    BorderComplex.Visibility = Visibility.Visible;

                    Brush[] brushes = new Brush[] { Brushes.Red, Brushes.Red, Brushes.Yellow, Brushes.Green };
                    Brush[] brushes1 = new Brush[] { Brushes.White, Brushes.White, Brushes.Black, Brushes.Black };
                    List<string> type = new List<string> { "Unacceptable", "Weak", "Medium", "Strong" };
                    BorderComplex.Background = brushes[type.IndexOf(ComplexResult.PasswordStrength)];
                    LabelComplex.Foreground = brushes1[type.IndexOf(ComplexResult.PasswordStrength)];
                });

            }, 10);
        }

        private void Suggestion_Password_Click(object sender, RoutedEventArgs e)
        {
            ProgressComplex.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> SuggestionPassword = Global.RequestAPI<Dictionary<string, string>>(Constants.SuggestopnURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, Global.BuildDictionary("api-version", 1.0), "");
                if (SuggestionPassword == null || SuggestionPassword["Password"] == null) return;

                Application.Current.Dispatcher.Invoke(async delegate
                {
                    ProgressComplex.Visibility = Visibility.Hidden;
                    bool result = await MessageUtils.ConfirmMessageAsync(Properties.Resources.Alert, string.Format("{0}\n{1}", Properties.Resources.Suggestion_Password,
                        SuggestionPassword["Password"]));

                    if (result)
                    {
                        TextPassword.Password = SuggestionPassword["Password"];
                        LabelComplex.Content = string.Format(Properties.Resources.Password_Strength, ComplexResult.PasswordStrength);
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

        private void TextPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ButtonSave.IsEnabled = TextPassword.Password != "";
        }
    }
}
