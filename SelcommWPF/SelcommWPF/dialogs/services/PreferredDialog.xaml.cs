using MaterialDesignColors;
using RestSharp;
using SelcommWPF.clients.models;
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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SelcommWPF.dialogs.accounts
{
    /// <summary>
    /// Interaction logic for PreferredDialog.xaml
    /// </summary>
    public partial class PreferredDialog : UserControl
    {
        private string ServiceTypeId;
        private string ServiceNumber;

        public PreferredDialog(string type, string number = "")
        {
            ServiceTypeId = type;
            ServiceNumber = number; 
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            if (ServiceNumber == "") return;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> result = Global.servicesClient.GetServiceConfigData(Constants.ServiceAvailableIdURL, ServiceNumber);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog("DetailDialog");
                        return;
                    }

                    ServicesType.Available item = new ServicesType.Available();
                    item.ServiceId = result["ServiceId"].ToString();
                    item.Ranking = result["Ranking"].ToString();
                    item.Fee = Convert.ToDouble(result["Fee"].ToString());
                    item.FeeText = "$" + item.Fee;
                    ListPreferred.ItemsSource = new List<ServicesType.Available>() { item};
                });

            }, 10);
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            NewServiceDialog.Instance.TextServiceNumber.Text = ServiceNumber;
            Global.CloseDialog("DetailDialog");
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            string search = TextPreferredNumber.Text;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                List<ServicesType.Available> result = Global.servicesClient.GetServiceAvailableIds(Constants.ServiceAvailableIdURL, ServiceTypeId, 0, 20, search);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    int count = result.Count;
                    for (int i = 0; i < count; i++) result[i].FeeText = "$" + result[i].Fee;
                    ListPreferred.ItemsSource = result;
                });

            }, 10);

        }

        private void ToggleReserve_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;
            string serviceId = button.Tag as string;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.servicesClient.ReleaseServiceReserve(Constants.ServiceReserveURL, serviceId);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK || result == HttpStatusCode.Created)
                    {
                        MessageUtils.ShowMessage("", "Service reserve successfully.");
                        ServiceNumber = serviceId;
                    }
                    else MessageUtils.ShowErrorMessage("", "Service reserve failed.");
                });

            }, 10);

        }
    }
}
