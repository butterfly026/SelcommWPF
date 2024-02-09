using RestSharp;
using SelcommWPF.clients.models.bills;
using SelcommWPF.global;
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

namespace SelcommWPF.dialogs.bills.options
{
    /// <summary>
    /// Interaction logic for BillCycles.xaml
    /// </summary>
    public partial class BillCycleDialog : UserControl
    {
        public BillCycleDialog()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                List<BillCycle> result = Global.RequestAPI<List<BillCycle>>(Constants.BillCycleURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog("DetailDialog");
                        return;
                    }

                    ListBillCycles.ItemsSource = result;
                    ListBillCycles.SelectedIndex = 0;
                });

            }, 10);

        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            BillCycle item = (BillCycle)ListBillCycles.SelectedItem;
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("CycleId", item.CycleId);
            body.Add("From", item.From);
            body.Add("NoDateCheck", true);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Global.RequestAPI(Constants.BillCycleURL, Method.Put, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode),
                    Global.BuildDictionary("api-version", 1.0), body, false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    Global.BillOptionDialog.LabelBillCycle.Content = item.Cycle;
                    LoadingPanel.Visibility = Visibility.Hidden;
                    Global.CloseDialog("DetailDialog");
                });

            }, 10);

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }

        private void ListBillCycles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonUpdate.IsEnabled = ListBillCycles.SelectedItem != null;
        }
    }
}
