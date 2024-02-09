using RestSharp;
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

namespace SelcommWPF.dialogs
{
    /// <summary>
    /// Interaction logic for BillNowDialog.xaml
    /// </summary>
    public partial class BillNowDialog : UserControl
    {

        List<Dictionary<string, string>> HotBillsPeriodList;

        public BillNowDialog()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            TextCycle.Text = Global.BillCycle;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                HotBillsPeriodList = Global.RequestAPI<List<Dictionary<string, string>>>(Constants.HotBillsPeriodsURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.0), "");

                List<string> list = new List<string>();
                foreach(Dictionary<string, string> item in HotBillsPeriodList) list.Add(item["Period"]);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (HotBillsPeriodList == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    ComboPeriod.ItemsSource = list;
                });

            }, 10);

        }

        private void Button_BillNow_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void Radio_Range_Checked(object sender, RoutedEventArgs e)
        {
            if (PanelService == null) return;
            RadioButton radio = (RadioButton)sender;
            PanelService.Visibility = radio.Tag.ToString() == "2" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CheckBox_Include_Usage_Checked(object sender, RoutedEventArgs e)
        {
            if (GridIncludeUsage == null) return;
            bool isChecked = e.RoutedEvent.Name == "Checked";
            GridIncludeUsage.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CheckBox_Charges_Checked(object sender, RoutedEventArgs e)
        {
            if (DatePickerDue == null) return;
            bool isChecked = e.RoutedEvent.Name == "Checked";
            GridCharges.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Radio_DueDate_Checked(object sender, RoutedEventArgs e)
        {
            if (DatePickerDue == null) return;
            RadioButton radio = (RadioButton)sender;
            DatePickerDue.IsEnabled = radio.Tag.ToString() == "2";
        }

        private void CheckBox_Usage_Checked(object sender, RoutedEventArgs e)
        {
            if (DatePickerUsage == null) return;
            bool isChecked = e.RoutedEvent.Name == "Checked";
            DatePickerUsage.IsEnabled = isChecked;
        }
    }
}
