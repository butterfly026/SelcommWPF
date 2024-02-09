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
    /// Interaction logic for PhoneHistory.xaml
    /// </summary>
    public partial class PhoneHistory : UserControl
    {
        public PhoneHistory()
        {
            InitializeComponent();
            ShowContactPhoneHistory();
        }

        private void ShowContactPhoneHistory()
        {
            if (Global.ContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                List<PhoneModel.History> result = Global.contactClient.GetContactPhoneHistory(Constants.PhoneHistoryURL, Global.ContactCode);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog("DetailDialog");
                        return;
                    }

                    List<PhoneModel.History> list = list = new List<PhoneModel.History>();
                    ListPhoneHistory.ItemsSource = new List<PhoneModel.History>();

                    int count = result == null ? 0 : result.Count;
                    for (int i = 0; i < count; i++)
                    {
                        result[i].FromDateTime = StringUtils.ConvertDateTime(result[i].FromDateTime);
                        result[i].ToDateTime = StringUtils.ConvertDateTime(result[i].ToDateTime);
                        list.Add(result[i]);
                    }

                    ListPhoneHistory.ItemsSource = list;
                    ShowHideVerticalScroll(Global.HasVerticalScroll);
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }

        private void ShowHideVerticalScroll(bool isChecked)
        {
            ListView[] listViews = new ListView[] { ListPhoneHistory };
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
