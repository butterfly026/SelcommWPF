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
    /// Interaction logic for AliasesHistory.xaml
    /// </summary>
    public partial class AliasesHistory : UserControl
    {

        private string ReuqestURL;
        private ListView CurrentListView;

        public AliasesHistory(bool isAlias)
        {
            InitializeComponent();
            if (isAlias)
            {
                CurrentListView = ListAliasesHistory;
                ListAliasesHistory.Visibility = Visibility.Visible;
                ListNamessHistory.Visibility = Visibility.Collapsed;
                ReuqestURL = Constants.AliasesHistoryURL;
                LabelTitle.Content = Properties.Resources.Contact_Aliases_History;
            }
            else
            {
                CurrentListView = ListNamessHistory;
                ListAliasesHistory.Visibility = Visibility.Collapsed;
                ListNamessHistory.Visibility = Visibility.Visible;
                ReuqestURL = Constants.NamesHistoryURL;
                LabelTitle.Content = Properties.Resources.Contact_Names_History;
            }

            ShowContactAliasesHistory();
        }


        private void ShowContactAliasesHistory()
        {
            if (Global.ContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                List<NameModel.History> result = Global.RequestAPI<List<NameModel.History>>(ReuqestURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.1), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog("DetailDialog");
                        return;
                    }

                    List<NameModel.History> list = new List<NameModel.History>();
                    int count = result == null ? 0 : result.Count;

                    for (int i = 0; i < count; i++)
                    {
                        result[i].FromDateTime = StringUtils.ConvertDateTime(result[i].FromDateTime);
                        result[i].ToDateTime = StringUtils.ConvertDateTime(result[i].ToDateTime);
                        result[i].Created = StringUtils.ConvertDateTime(result[i].Created);
                        list.Add(result[i]);
                    }

                    CurrentListView.ItemsSource = new List<NameModel.History>();
                    CurrentListView.ItemsSource = list;
                    ShowHideVerticalScroll(Global.HasVerticalScroll);
                });

            }, 10);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }

        private void ShowHideVerticalScroll(bool isChecked)
        {
            ListView[] listViews = new ListView[] { ListAliasesHistory, ListNamessHistory};
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
