using SelcommWPF.clients.models.services;
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

namespace SelcommWPF.dialogs.services
{
    /// <summary>
    /// Interaction logic for ServiceAttributes.xaml
    /// </summary>
    public partial class ServiceAttributes : UserControl
    {
        private string ServiceReference;
        public static ServiceAttributes Instance;
        private List<Dictionary<string, object>> ListDefinitionIds;

        public ServiceAttributes(string reference)
        {
            ServiceReference = reference;
            InitializeComponent();
            InitializeControl();
        }

        public void InitializeControl()
        {
            Instance = this;
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                List<AttributeModel> result = Global.servicesClient.GetServiceAttributesData(Constants.ServicesAttributesURL, ServiceReference);
                Dictionary<string, object> serviceBasicData = Global.servicesClient.GetServiceTypeBasic(Constants.ServicesBasicURL, ServiceReference);
                ListDefinitionIds = Global.servicesClient.GetServicesDefinitionId(Constants.ServicesDefinitionsURL, serviceBasicData["ServiceTypeId"].ToString());

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null || serviceBasicData == null || ListDefinitionIds == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    int count = result.Count;
                    for (int i = 0; i < count; i++)
                    {
                        result[i].From = StringUtils.ConvertDateTime(result[i].From);
                        result[i].To = StringUtils.ConvertDateTime(result[i].To);
                        if (result[i].To.Contains("9999-01-01")) result[i].To = "on going";
                        result[i].LastUpdated = StringUtils.ConvertDateTime(result[i].LastUpdated);
                    }

                    ListServiceAttributes.ItemsSource = result;
                });

            }, 10);

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void ListServiceAttributes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isEnable = ListServiceAttributes.SelectedItem != null;
            ButtonUpdate.IsEnabled = isEnable;
            ButtonDelete.IsEnabled = isEnable;

            if (isEnable)
            {
                AttributeModel item = (AttributeModel)ListServiceAttributes.SelectedItem;
                ButtonUpdate.IsEnabled = item.Editable;
                foreach (Dictionary<string, object> obj in ListDefinitionIds)
                {
                    if (Convert.ToInt64(obj["Id"].ToString()) == item.DefinitionId)
                    {
                        ButtonDelete.IsEnabled = !Convert.ToBoolean(obj["Required"].ToString());
                        break;
                    }
                }
            }
        }

        private async void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog("DetailDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(new AttributeDialog(ServiceReference), "DetailDialog");
        }

        private async void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (ListServiceAttributes.SelectedItem == null) return;
            AttributeModel item = (AttributeModel)ListServiceAttributes.SelectedItem;

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog("DetailDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(new AttributeDialog(ServiceReference, item.Id), "DetailDialog");
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ListServiceAttributes.SelectedItem == null) return;
            AttributeModel item = (AttributeModel)ListServiceAttributes.SelectedItem;

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.servicesClient.DeleteServiceAttribute(Constants.ServicesAttributeDetailURL, item.Id);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK) InitializeControl();
                });

            }, 10);
        }
    }
}
