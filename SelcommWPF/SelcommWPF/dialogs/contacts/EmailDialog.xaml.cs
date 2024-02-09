using MaterialDesignColors;
using RestSharp;
using SelcommWPF.clients.models.contacts;
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
    /// Interaction logic for EmailDialog.xaml
    /// </summary>
    public partial class EmailDialog : UserControl
    {
        private List<string> EmailToList = new List<string>();
        private List<string> EmailBCCList = new List<string>();
        private List<DocumentModel.Item> SelectedDocuments = new List<DocumentModel.Item>();

        public EmailDialog()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            DatePickerDue.SelectedDate = DateTime.Now;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                List<Dictionary<string, string>> result = Global.RequestAPI<List<Dictionary<string, string>>>(Constants.EmailAddressListURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    List<string> address = new List<string>();
                    foreach (Dictionary<string, string> item in result) address.Add(item["Address"]);
                    ComboEmailTo.ItemsSource = address;
                    ComboEmailBCC.ItemsSource = address;
                });

            }, 10);

        }

        private void ShowAvailableDocuments(int skip, int take)
        {
            if (skip == 0) ListDocuments.ItemsSource = new List<DocumentModel.Item>();
            if (Global.ContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("SearchString", "");
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("CountRecords", "Y");
                Dictionary<string, object> result = Global.RequestAPI<Dictionary<string, object>>(Constants.AvailableDocumentsURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result != null && result["Documents"].ToString() != "[]")
                    {
                        List<DocumentModel.Item> list = (List<DocumentModel.Item>)ListDocuments.ItemsSource;
                        ListDocuments.ItemsSource = new List<DocumentModel.Item>();

                        if (list == null) list = new List<DocumentModel.Item>();
                        list.AddRange((List<DocumentModel.Item>)result["Documents"]);
                        ListDocuments.Tag = result["Count"];
                        ListDocuments.ItemsSource = list;
                        CardDocuments.Visibility = Visibility.Visible;
                    }
                });

            }, 10);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void ButtonSendEmail_Click(object sender, RoutedEventArgs e)
        {
            LoadingPanel.Visibility = Visibility.Visible;
            List<Dictionary<string, string>> recipients = new List<Dictionary<string, string>>();

            foreach (string email in EmailToList)
            {
                Dictionary<string, string> value = new Dictionary<string, string>();
                value.Add("Type", "TO");
                value.Add("Address", email);
                recipients.Add(value);
            }

            foreach (string email in EmailBCCList)
            {
                Dictionary<string, string> value = new Dictionary<string, string>();
                value.Add("Type", "BCC");
                value.Add("Address", email);
                recipients.Add(value);
            }

            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("Recipients", recipients);
            body.Add("Subject", TextEmailSubject.Text);
            body.Add("Body", RichTextEmail.Text.Replace("\"", "'"));
            body.Add("BodyFormat", "HTML");
            body.Add("Priority", "MEDIUM");
            body.Add("CorrelationId", null);
            body.Add("RequestDeliveryReceipt", false);
            body.Add("Due", DatePickerDue.SelectedDate.Value.ToString("yyyy-MM-ddThh:mm:ss"));

            if (ToggleDocumment.IsChecked.Value)
            {
                List<Dictionary<string, object>> files = new List<Dictionary<string, object>>();
                foreach(DocumentModel.Item item in SelectedDocuments)
                {
                    Dictionary<string, object> value = new Dictionary<string, object>();
                    value.Add("Type", new string[] { item.Type });
                    value.Add("DocumentId", item.Id);
                    files.Add(value);
                }
                body.Add("Attachments", files);
            }

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> path = Global.BuildDictionary("Type", "Emails");
                path.Add("Contacts", "Contacts");
                path.Add("ContactCode", Global.ContactCode);
                HttpStatusCode result = Global.RequestAPI(Constants.SendEmailOrSMSURL, Method.Post, Global.GetHeader(Global.CustomerToken), path, Global.BuildDictionary("api-version", 1.0), body);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK || result == HttpStatusCode.Created)
                    {
                        Global.mainWindow.ShowMessageList(0, 10);
                        Global.CloseDialog();
                    }
                });
            }, 10);

        }

        private void TextBoxEmailTo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string address = ComboEmailTo.Text;
                if (EmailToList.Contains(address))
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
                button.Click += ButtonRemoveTo_Click;
                panel.Children.Add(label);
                panel.Children.Add(button);
                border.Child = panel;

                PanelTagTo.Children.Add(border);
                ComboEmailTo.Text = "";
                EmailToList.Add(address);
                ButtonSendEmail.IsEnabled = !string.IsNullOrEmpty(TextEmailSubject.Text) && EmailToList.Count > 0;
            }
        }

        private void ButtonRemoveTo_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StackPanel panel = (StackPanel)button.Parent;
            Border border = (Border)panel.Parent;
            PanelTagTo.Children.Remove(border);
            EmailToList.Remove(((Label)panel.Children[0]).Content.ToString());
            ButtonSendEmail.IsEnabled = !string.IsNullOrEmpty(TextEmailSubject.Text) && EmailToList.Count > 0;
        }

        private void TextEmailSubject_TextChanged(object sender, TextChangedEventArgs e)
        {
            ButtonSendEmail.IsEnabled = !string.IsNullOrEmpty(TextEmailSubject.Text) && EmailToList.Count > 0;
        }

        private void TextBoxEmailBCC_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string address = ComboEmailBCC.Text;
                if (EmailBCCList.Contains(address))
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
                button.Click += ButtonRemoveBCC_Click;
                panel.Children.Add(label);
                panel.Children.Add(button);
                border.Child = panel;

                PanelTagBCC.Children.Add(border);
                ComboEmailBCC.Text = "";
                EmailBCCList.Add(address);
            }
        }

        private void ButtonRemoveBCC_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StackPanel panel = (StackPanel)button.Parent;
            Border border = (Border)panel.Parent;
            PanelTagBCC.Children.Remove(border);
            EmailBCCList.Remove(((Label)panel.Children[0]).Content.ToString());
        }

        private void ToggleDocumment_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = e.RoutedEvent.Name == "Checked";
            if (isChecked)
            {
                GridDocuments.Visibility = Visibility.Visible;
                ShowAvailableDocuments(0, 10);
            }
            else
            {
                GridDocuments.Visibility = Visibility.Collapsed;
                CardDocuments.Visibility = Visibility.Collapsed;
                ListDocuments.ItemsSource = new List<DocumentModel.Item>();
            }

        }

        private void TextDocument_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CardDocuments.Visibility = Visibility.Visible;
        }

        private void ListDocuments_LostFocus(object sender, RoutedEventArgs e)
        {
            CardDocuments.Visibility = Visibility.Collapsed;
        }

        private void ListDocuments_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ListView listView = (ListView)e.Source;
            int totalCount = listView.Items.Count;
            int limit = Convert.ToInt32(listView.Tag);
            if (totalCount == 0 || totalCount >= limit) return;
            if (e.VerticalOffset + e.VerticalChange >= totalCount - 6)
            {
                ShowAvailableDocuments(totalCount, 10);
            }
        }

        private void CheckDocument_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = e.RoutedEvent.Name == "Checked";
            CheckBox checkBox = (CheckBox)sender;

            long Id = Convert.ToInt64(checkBox.Tag);
            List<DocumentModel.Item> list = (List<DocumentModel.Item>)ListDocuments.ItemsSource;

            foreach(DocumentModel.Item item in list)
            {
                if (Id == item.Id)
                {
                    if (isChecked) SelectedDocuments.Add(item);
                    else SelectedDocuments.Remove(item);
                    break;
                }
            }

            string text = "";
            foreach (DocumentModel.Item item in SelectedDocuments) text += "," + item.Name;
            text = text == "" ? "" : text.Substring(1);
            TextDocuments.Text = text;
        }

    }
}
