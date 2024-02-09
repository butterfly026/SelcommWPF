using RestSharp;
using SelcommWPF.clients.models.contacts;
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

namespace SelcommWPF.dialogs.contacts
{
    /// <summary>
    /// Interaction logic for AddressHistory.xaml
    /// </summary>
    public partial class AddressHistory : UserControl
    {
        public AddressHistory()
        {
            InitializeComponent();
            ShowContactAddressHistory();
        }

        private void ShowContactAddressHistory()
        {
            if (Global.ContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                List<AddressModel.History> result = Global.RequestAPI<List<AddressModel.History>>(Constants.AddressHistoryURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.1), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog("SearchAddress");
                        return;
                    }

                    List<AddressModel.History> list = list = new List<AddressModel.History>();
                    ListAddressHistory.ItemsSource = new List<AddressModel.History>();

                    int count = result == null ? 0 : result.Count;
                    for (int i = 0; i < count; i++)
                    {
                        result[i].FromDateTime = StringUtils.ConvertDateTime(result[i].FromDateTime);
                        result[i].ToDateTime = StringUtils.ConvertDateTime(result[i].ToDateTime);
                        list.Add(result[i]);
                    }

                    ListAddressHistory.ItemsSource = list;
                    ShowHideVerticalScroll(Global.HasVerticalScroll);
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("SearchAddress");
        }

        private void ShowHideVerticalScroll(bool isChecked)
        {
            ListView[] listViews = new ListView[] { ListAddressHistory };
            int count = listViews.Length;
            for (int i = 0; i < count; i++) if (listViews[i] == null) return;
            for (int i = 0; i < count; i++)
            {
                try
                {
                    ScrollViewer sv = VisualTreeHelper.GetChild(listViews[i], 0) as ScrollViewer;
                    sv.VerticalScrollBarVisibility = isChecked ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }


    }
}
