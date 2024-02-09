using RestSharp;
using SelcommWPF.clients.models;
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

namespace SelcommWPF.dialogs.contacts
{
    /// <summary>
    /// Interaction logic for AuthenticationDialog.xaml
    /// </summary>
    public partial class AuthenticationDialog : UserControl
    {
        private Dictionary<string, object> AuthenticationInfo;
        private ComplexResponse ComplexResult = new ComplexResponse();
        private bool IsLoadingAuthInfo = false;

        public AuthenticationDialog()
        {
            InitializeComponent();
            InitializeControl();    
        }

        private void InitializeControl()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                AuthenticationInfo = Global.RequestAPI<Dictionary<string, object>>(Constants.AuthenticationAccountURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("Id", Global.ContactId), Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;    
                    if (AuthenticationInfo == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    TextLoginId.Text = AuthenticationInfo["LoginId"].ToString();
                    TextLoginMobile.Text = AuthenticationInfo["Mobile"].ToString();
                    TextLoginEmail.Text = AuthenticationInfo["Email"].ToString();
                    ToggleChangePassword.IsChecked = Convert.ToBoolean(AuthenticationInfo["ChangePasswordOnFirstLogin"].ToString());
                    IsLoadingAuthInfo = true;
                });

            }, 10);
        }

        private void TextPhoneOrEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsLoadingAuthInfo) return;
            TextBox textBox = (TextBox)sender;
            StackPanel panel = (StackPanel)textBox.Parent;
            ProgressBar progressBar = (ProgressBar)panel.Children[1];

            string url = Constants.ValidatePhone;
            string url1 = Constants.CheckEmailURL;
            string param = textBox.Text;
            if (param == "") return;
            string tag = textBox.Tag.ToString();
            progressBar.Visibility = Visibility.Visible;

            if (tag == "Login Id")
            {
                url = Constants.CheckLoginIdURL;
                CheckUniqueAuthInfo(textBox, url, param, tag, progressBar);
                return;
            }
            else if (tag == Properties.Resources.Authenticate_Email)
            {
                url = Constants.ValidateEmail;
                url1 = Constants.CheckEmailURL;
            }
            else if (tag == Properties.Resources.Authenticate_Mobile)
            {
                url = Constants.ValidateSMS;
                url1 = Constants.CheckMobileURL;
            }

            EasyTimer.SetTimeout(() =>
            {

                ValidModel result = Global.messageClient.ValidatePhoneOrEmail(url, param);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result.Valid)
                    {
                        CheckUniqueAuthInfo(textBox, url1, param, tag, progressBar);
                    }
                    else
                    {
                        MessageUtils.ShowErrorMessage("", param + " : " + string.Format(Properties.Resources.Invalid_Message, tag));
                        progressBar.Visibility = Visibility.Collapsed;
                        textBox.Text = "";
                        textBox.Focus();
                    }
                });

            }, 10);
        }

        private void CheckUniqueAuthInfo(TextBox textBox, string url, string param, string tag, ProgressBar progressBar)
        {
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, bool> result = Global.userClient.GetAuthenticateUnique(url, param);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    progressBar.Visibility = Visibility.Collapsed;
                    if (result == null || result["Used"])
                    {
                        MessageUtils.ShowMessage("", param + " : " + string.Format(Properties.Resources.Choose_Another_Message, tag));
                        textBox.Text = "";
                        textBox.Focus();
                    }
                    else MessageUtils.ShowMessage("", param + " : " + string.Format(Properties.Resources.Available_Message, tag));
                });

            }, 10);

        }


        private void ButtonCheck_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string tag = button.Tag.ToString();

            string url = Constants.ValidateEmail;
            string url1 = Constants.CheckEmailURL;
            TextBox textBox = TextLoginEmail;
            ProgressBar progressBar = ProgressLoginEmail;

            if (tag == "Email")
            {
                tag = Properties.Resources.Authenticate_Email;
                url = Constants.ValidateEmail;
                url1 = Constants.CheckEmailURL;
                textBox = TextLoginEmail;
                progressBar = ProgressLoginEmail;
            }
            else if (tag == "Mobile")
            {
                tag = Properties.Resources.Authenticate_Mobile;
                url = Constants.ValidateSMS;
                url1 = Constants.CheckMobileURL;
                textBox = TextLoginMobile;
                progressBar = ProgressLoginMobile;
            }
            else if (tag == "Login")
            {
                tag = Properties.Resources.Login_id;
                url1 = Constants.CheckLoginIdURL;
                textBox = TextLoginId;
                progressBar = ProgressLoginId;
            }

            string param = textBox.Text;
            if (param == "") return;
            progressBar.Visibility = Visibility.Visible;

            if (tag == Properties.Resources.Login_id)
            {
                CheckUniqueAuthInfo(textBox, url1, param, tag, progressBar);
            }
            else
            {
                EasyTimer.SetTimeout(() =>
                {

                    ValidModel result = Global.messageClient.ValidatePhoneOrEmail(url, param);
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        if (result.Valid)
                        {
                            CheckUniqueAuthInfo(textBox, url1, param, tag, progressBar);
                        }
                        else
                        {
                            MessageUtils.ShowErrorMessage("", param + " : " + string.Format(Properties.Resources.Invalid_Message, tag));
                            progressBar.Visibility = Visibility.Collapsed;
                            textBox.Text = "";
                            textBox.Focus();
                        }
                    });

                }, 10);
            }
        }

        private void TextPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsLoadingAuthInfo) return;
            string password = TextPassword.Password;
            if (password == "") return;

            ProgressComplex.Visibility = Visibility.Visible;
            BorderComplex.Visibility = Visibility.Collapsed;

            EasyTimer.SetTimeout(() =>
            {
                ComplexResult = Global.userClient.CheckComplexity(Constants.ComplexURL, password);
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
                Dictionary<string, string> SuggestionPassword = Global.userClient.GetSuggestion(Constants.SuggestopnURL);
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

        private void TextLoginMobile_TextChanged(object sender, TextChangedEventArgs e)
        {
            ButtonSave.IsEnabled = TextLoginId.Text != "" && TextLoginEmail.Text != "" && TextLoginMobile.Text != "";
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            AuthenticationInfo["LoginId"] = TextLoginId.Text;
            AuthenticationInfo["Mobile"] = TextLoginMobile.Text;
            AuthenticationInfo["Email"] = TextLoginEmail.Text;
            AuthenticationInfo["ChangePasswordOnFirstLogin"] = ToggleChangePassword.IsChecked.Value;
            AuthenticationInfo["Password"] = TextPassword.Password;

            Dictionary<string, object> defaultRole = Global.BuildDictionary("Name", "Customer");
            defaultRole.Add("Default", true);
            AuthenticationInfo["Roles"] = new List<Dictionary<string, object>>() { defaultRole };

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> path = Global.BuildDictionary("Id", Global.ContactId);
                path.Add("Category", "Customer");
                HttpStatusCode result = Global.RequestAPI(Constants.AuthenticationCreateURL, Method.Put, Global.GetHeader(Global.CustomerToken), path,
                    Global.BuildDictionary("api-version", 1.0), AuthenticationInfo, false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK || result == HttpStatusCode.Created)
                    {
                        MessageUtils.ShowMessage("", Properties.Resources1.Auth_Success);
                        Global.CloseDialog();
                    }
                    else MessageUtils.ShowErrorMessage("", Properties.Resources1.Auth_Failed);
                });

            }, 10);

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }
    }
}
