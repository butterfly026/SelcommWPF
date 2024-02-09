using RestSharp;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace SelcommWPF.dialogs
{
    /// <summary>
    /// Interaction logic for ResetPassword.xaml
    /// </summary>
    public partial class ResetPassword : UserControl
    {
        public ResetPassword()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> header = new Dictionary<string, string>();
                header.Add("Authorization", Global.AccessToken.Type + " " + Global.AccessToken.Credentials);
                Dictionary<string, object> query = new Dictionary<string, object>();
                query.Add("api-version", 1.0);
                Dictionary<string, bool> result = Global.RequestAPI<Dictionary<string, bool>>(Constants.ResetPasswordConfigURL, Method.Get, header, null, query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources.Forgot_Password_Error);
                        Global.CloseDialog();
                        return;
                    }

                    ((TabItem)TabResetType.Items[0]).Visibility = result["EmailLink"] ? Visibility.Visible : Visibility.Collapsed;
                    ((TabItem)TabResetType.Items[1]).Visibility = result["SMSReset"] ? Visibility.Visible : Visibility.Collapsed;
                });

            }, 10);

        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void TextCode_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void TextCode1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TabResetType.SelectedIndex != 1) return;
            string code = TextCode1.Text;
            if (code.Length == 1)
            {
                EnabledSendButton();
                TextCode2.Focus();
            }
            else if (code.Length > 1) TextCode1.Text = code.Substring(0, 1);
            else ButtonSend.IsEnabled = false;
        }

        private void TextCode2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TabResetType.SelectedIndex != 1) return;
            string code = TextCode2.Text;
            if (code.Length == 1)
            {
                EnabledSendButton();
                TextCode3.Focus();
            }
            else if (code.Length > 1) TextCode2.Text = code.Substring(0, 1);
            else ButtonSend.IsEnabled = false;
        }

        private void TextCode3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TabResetType.SelectedIndex != 1) return;
            string code = TextCode3.Text;
            if (code.Length == 1)
            {
                EnabledSendButton();
                TextCode4.Focus();
            }
            else if (code.Length > 1) TextCode2.Text = code.Substring(0, 1);
            else ButtonSend.IsEnabled = false;
        }

        private void TextCode4_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TabResetType.SelectedIndex != 1) return;
            string code = TextCode4.Text;
            if (code.Length == 1)
            {
                EnabledSendButton();
                ButtonSend.Focus();
            }
            else if (code.Length > 1) TextCode4.Text = code.Substring(0, 1);
            else ButtonSend.IsEnabled = false;
        }

        private void EnabledSendButton()
        {
            string code = TextCode1.Text + TextCode2.Text + TextCode3.Text + TextCode4.Text;
            ButtonSend.IsEnabled = code.Length == 4 && TextUserID.Text.Length > 0;
        }

        private void TabResetType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = TabResetType.SelectedIndex;
            if (index == 0) ButtonSend.IsEnabled = TextUserID.Text.Length > 0;
            else if (index == 1) EnabledSendButton();
        }

        private void TextUserID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TabResetType.SelectedIndex == 0) ButtonSend.IsEnabled = TextUserID.Text.Length > 0;
            else if (TabResetType.SelectedIndex == 1) EnabledSendButton();
        }

        private void Button_Send_Click(object sender, RoutedEventArgs e)
        {
            int index = TabResetType.SelectedIndex;
            string userId = TextUserID.Text;
            string code = TextCode1.Text + TextCode2.Text + TextCode3.Text + TextCode4.Text;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result;
                Dictionary<string, string> header = new Dictionary<string, string>();
                header.Add("Authorization", Global.AccessToken.Type + " " + Global.AccessToken.Credentials);
                if (index == 0)
                {
                    Dictionary<string, object> path = new Dictionary<string, object>();
                    path.Add("SiteId", Constants.SiteId);
                    path.Add("identifier", userId);
                    Dictionary<string, object> query = new Dictionary<string, object>();
                    query.Add("api-version", 3.1);
                    result = Global.RequestAPI(Constants.ResetPasswordURL, Method.Post, header, path, query, null);
                }
                else
                {
                    Dictionary<string, object> query = new Dictionary<string, object>();
                    query.Add("api-version", 1.0);
                    Dictionary<string, object> body = new Dictionary<string, object>();
                    body.Add("ContactCode", userId);
                    body.Add("MSISDNFragment", code);
                    result = Global.RequestAPI(Constants.ResetPasswordSMSURL, Method.Post, header, null, query, body);
                }

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.NoContent) MessageUtils.ShowMessage(Properties.Resources.Error, string.Format(Properties.Resources.Reset_Password_Success, new string[] { "Email", "SMS" }[index]));
                    else MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Reset_Password_Error + result.ToString());
                });

            }, 10);

        }
    }
}
