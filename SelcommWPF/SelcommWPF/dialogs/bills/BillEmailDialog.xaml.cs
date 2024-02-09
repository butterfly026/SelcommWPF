using Newtonsoft.Json;
using RestSharp;
using SelcommWPF.clients.models.bills;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace SelcommWPF.dialogs.bills
{
    /// <summary>
    /// Interaction logic for BillEmailDialog.xaml
    /// </summary>
    public partial class BillEmailDialog : UserControl
    {

        public long BillId;
        public string BillNumber;
        public List<string> EmailList = new List<string>();

        public BillEmailDialog(long id, string number)
        {
            BillId = id;
            BillNumber = number;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LabelDialogTitle.Content = string.Format(Properties.Resources.Bill_Email_Title, BillNumber);
            TextEmailSubject.Text = "Bill";
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                List<Dictionary<string, string>> result = Global.RequestAPI<List<Dictionary<string, string>>>(Constants.BillEmailAddressURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("BillId", BillId), Global.BuildDictionary("api-version", 1.0), "");

                List<string> address = new List<string>();
                foreach (Dictionary<string, string> item in result) address.Add(item["Address"]);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }
                    ComboEmailAddress.ItemsSource = address;
                });

            }, 10);

        }

        private void TextBoxEmail_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                string address = ComboEmailAddress.Text;
                if (EmailList.Contains(address))
                {
                    MessageUtils.ShowErrorMessage("", Properties.Resources.Email_Already_Add);
                    return;
                }

                Border border = new Border();
                border.CornerRadius = new CornerRadius(10);
                border.Background = Brushes.LightGray;
                border.Padding = new Thickness(10, 3, 10, 3);
                border.Margin = new Thickness(5);

                StackPanel panel = new StackPanel();
                panel.Orientation = Orientation.Horizontal;

                Label label = new Label();
                label.Content = address;
                label.FontSize = 16;
                label.Padding = new Thickness(0);

                Button button = new Button();
                button.Padding = new Thickness(0);
                button.Background = Brushes.Transparent;
                button.BorderBrush = Brushes.Transparent;
                button.BorderThickness = new Thickness(0);
                button.Height = 15;
                button.Width = 15;
                button.Margin = new Thickness(5, 2, 0, 0);

                Border border1 = new Border();
                border1.Background = Brushes.DarkGray;
                border1.CornerRadius = new CornerRadius(10);

                MaterialDesignThemes.Wpf.PackIcon icon = new MaterialDesignThemes.Wpf.PackIcon();
                icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Close;
                icon.Width = 12;
                icon.Height = 12;

                border1.Child = icon;
                button.Content = border1;
                button.Click += ButtonRemove_Click;
                panel.Children.Add(label);
                panel.Children.Add(button);
                border.Child = panel;

                PanelTag.Children.Add(border);
                ComboEmailAddress.Text = "";
                EmailList.Add(address);
                ButtonSendEmail.IsEnabled = EmailList.Count > 0;
            }
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StackPanel panel = (StackPanel)button.Parent;
            Border border = (Border)panel.Parent;
            PanelTag.Children.Remove(border);
            EmailList.Remove(((Label)panel.Children[0]).Content.ToString());
            ButtonSendEmail.IsEnabled = EmailList.Count > 0;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void ButtonSendEmail_Click(object sender, RoutedEventArgs e)
        {
            List<BillEmailModel.Recipient> recipients = new List<BillEmailModel.Recipient>();
            foreach (string email in EmailList)
            {
                BillEmailModel.Recipient recipient = new BillEmailModel.Recipient();
                recipient.Id = 0;
                recipient.Type = "TO";
                recipient.RequestDeliveryReceipt = true;
                recipient.Address = email;
                recipients.Add(recipient);
            }

            BillEmailModel body = new BillEmailModel();
            body.AttachPDF = true;
            body.AttachXLS = false;
            body.Importance = "MEDIUM";
            body.Subject = TextEmailSubject.Text;
            body.Body = RichTextEmail.Text.Replace("\"", "'");
            body.BodyFormat = "HTML";
            body.Images = new List<BillEmailModel.Image>();
            body.Recipients = recipients;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> result = Global.RequestAPI<Dictionary<string, object>>(Constants.BillEmailSendURL, Method.Post, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("BillId", BillId), Global.BuildDictionary("api-version", 1.0), JsonConvert.SerializeObject(body));

                Application.Current.Dispatcher.Invoke(delegate
                {
                    try
                    {
                        if (result != null && result["Id"] != null)
                        {
                            MessageUtils.ShowMessage("", Properties.Resources.Email_Sent_Success);
                            Global.CloseDialog();
                        }
                    }
                    catch
                    {
                        try
                        {
                            MessageUtils.ShowErrorMessage("", result["ErrorMessage"].ToString());
                        }
                        catch
                        {
                            MessageUtils.ShowErrorMessage("", Properties.Resources.Unknown_Error);
                        }
                    }
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);

        }
    }
}
