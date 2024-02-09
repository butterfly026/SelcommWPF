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
    /// Interaction logic for SMSDialog.xaml
    /// </summary>
    public partial class SMSDialog : UserControl
    {
        private List<string> PhoneNumberList = new List<string>();

        public SMSDialog()
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
                List<string> address = new List<string>();
                List<Dictionary<string, string>> result = Global.messageClient.GetEmailAddressOrSMS(Constants.SMSNumberListURL, Global.ContactCode);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    foreach (Dictionary<string, string> item in result) address.Add(item["MSISDN"]);
                    ComboPhones.ItemsSource = address;
                });

            }, 10);

        }


        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void ButtonSendSMS_Click(object sender, RoutedEventArgs e)
        {
            LoadingPanel.Visibility = Visibility.Visible;
            List<Dictionary<string, string>> addresses = new List<Dictionary<string, string>>();

            foreach (string item in PhoneNumberList)
            {
                Dictionary<string, string> value = new Dictionary<string, string>();
                value.Add("MSISDN", item);
                addresses.Add(value);
            }

            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("Addresses", addresses);
            body.Add("Message", TextMessage.Text);
            body.Add("Priority", "LOW");
            body.Add("CorrelationId", null);
            body.Add("RequestDeliveryReceipt", false);
            body.Add("Due", DatePickerDue.SelectedDate.Value.ToString("yyyy-MM-ddThh:mm:ss"));

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.messageClient.SendEmailOrSMS(Constants.SendEmailOrSMSURL, "SMSs", Global.ContactCode, body);
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

        private void TextBoxSMS_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string address = ComboPhones.Text;
                if (PhoneNumberList.Contains(address))
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
                ComboPhones.Text = "";
                PhoneNumberList.Add(address);
                ButtonSendSMS.IsEnabled = !string.IsNullOrEmpty(TextMessage.Text) && PhoneNumberList.Count > 0;
            }
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StackPanel panel = (StackPanel)button.Parent;
            Border border = (Border)panel.Parent;
            PanelTag.Children.Remove(border);
            PhoneNumberList.Remove(((Label)panel.Children[0]).Content.ToString());
            ButtonSendSMS.IsEnabled = !string.IsNullOrEmpty(TextMessage.Text) && PhoneNumberList.Count > 0;
        }

        private void TextMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            ButtonSendSMS.IsEnabled = !string.IsNullOrEmpty(TextMessage.Text) && PhoneNumberList.Count > 0;
        }

    }
}
