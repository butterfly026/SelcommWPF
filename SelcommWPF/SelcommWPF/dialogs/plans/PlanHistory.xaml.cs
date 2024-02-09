using SelcommWPF.clients.models;
using SelcommWPF.dialogs.plans;
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

namespace SelcommWPF.dialogs
{
    /// <summary>
    /// Interaction logic for PlanHistory.xaml
    /// </summary>
    public partial class PlanHistory : UserControl
    {
        private bool IsService;
        private string ServiceReference;

        public PlanHistory(bool isServices = false, string reference = "")
        {
            IsService = isServices;
            ServiceReference = reference;
            InitializeComponent();
            LabelDialogTitle.Content = isServices ? Properties.Resources.Services_Plan_History : Properties.Resources.Plan_History;
            ShowPlanHistory(isServices, reference);
        }

        private void ShowPlanHistory(bool isServices, string reference)
        {
            if (Global.ContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            if (!isServices) reference = Global.ContactCode;
            string url = isServices ? Constants.ServicesPlanHistoryURL : Constants.PlanHistoryURL;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {

                List<PlanModel> result = Global.planClient.GetPlanHistories(url, reference, isServices);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    List<PlanModel> list = list = new List<PlanModel>();
                    ListPlanHistory.ItemsSource = new List<PlanModel>();

                    int count = result == null ? 0 : result.Count;
                    for (int i = 0; i < count; i++)
                    {
                        result[i].Scheduled = StringUtils.ConvertDateTime(result[i].Scheduled);
                        result[i].From = StringUtils.ConvertDateTime(result[i].From);
                        result[i].To = StringUtils.ConvertDateTime(result[i].To);
                        result[i].LastUpdated = StringUtils.ConvertDateTime(result[i].LastUpdated);
                        list.Add(result[i]);
                    }

                    ListPlanHistory.ItemsSource = list;
                });

            }, 10);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void ButtonDetail_Click(object sender, RoutedEventArgs e)
        {
            if (ListPlanHistory.SelectedItem == null || ServiceReference == "") return;
            PlanModel item = (PlanModel)ListPlanHistory.SelectedItem;
            
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new PlanDetail(Convert.ToInt64(ServiceReference), item.PlanId.Value), "DetailDialog");
        }

        private void ListPlanHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonPackageDetail.IsEnabled = ListPlanHistory.SelectedItem != null;
        }
    }
}
