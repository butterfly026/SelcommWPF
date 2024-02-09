using Newtonsoft.Json;
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
    /// Interaction logic for SearchDialog.xaml
    /// </summary>
    public partial class SearchDialog : UserControl
    {
        private bool IsSimpleSearch = true;
        private string SearchString = "";
        private Dictionary<string, object> SearchBody = new Dictionary<string, object>();

        public SearchDialog(string search, bool isSimple = true)
        {
            IsSimpleSearch = isSimple;
            if (isSimple) SearchString = search;
            else SearchBody = JsonConvert.DeserializeObject<Dictionary<string, object>>(search);

            InitializeComponent();
            ShowContactList(0, 10);
        }

        private void ShowContactList(int skip, int take)
        {
            PanelCode.Visibility = Visibility.Collapsed;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                if (Global.contactClient == null) Global.contactClient = new clients.ContactClient();
                SearchModel result = new SearchModel();
                if (IsSimpleSearch) result = Global.contactClient.SearchContactList(Constants.SimpleSearchURL, SearchString, skip, take);
                else result = Global.contactClient.AdvancedSearchList(Constants.AdvancedSearchURL, SearchBody, skip, take);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    if (result.RecordCount == 0 || result.Items.Count == 0)
                    {
                        MessageUtils.ShowErrorMessage("", "This account not found.");
                        PanelCode.Visibility = Visibility.Visible;
                        return;
                    }

                    List<SearchModel.Item> list = (List<SearchModel.Item>)ListContacts.ItemsSource;
                    ListContacts.ItemsSource = new List<SearchModel.Item>();

                    if (list == null) list = new List<SearchModel.Item>();
                    ListContacts.Tag = result.RecordCount;
                    if (result.Items != null) list.AddRange(result.Items);
                    ListContacts.ItemsSource = list;
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (skip == 0 || list.Count > 0) ShowContactDetail(list.Count > 0 ? list[0] : null);
                    ScrollDetail.Visibility = Visibility.Visible;
                    ShowHideVerticalScroll(Global.HasVerticalScroll);
                });

            }, 10);
        }

        private void ShowHideVerticalScroll(bool isChecked)
        {
            ListView[] listViews = new ListView[] { ListContacts };
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

            ScrollViewer[] scrolls = new ScrollViewer[] { ScrollDetail };
            count = scrolls.Length;

            for (int i = 0; i < count; i++) if (scrolls[i] == null) return;
            for (int i = 0; i < count; i++) scrolls[i].VerticalScrollBarVisibility = isChecked ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
        }

        private void ShowContactDetail(SearchModel.Item item)
        {
            if (item == null)
            {
                PanelAccount.Visibility = Visibility.Collapsed;
                PanelBirthday.Visibility = Visibility.Collapsed;
                return;
            }

            LabelAccountNumber.Content = item.ContactCode;
            LabelBirthday.Content = item.DateOfBirth;
            PanelBirthday.Visibility = item.DateOfBirth == "" ? Visibility.Collapsed : Visibility.Visible;

            string temp = "";
            foreach(SearchModel.AddressModel data in item.Addresses) temp += data.Type + " : " + data.Address + Environment.NewLine;
            TextAddresses.Text = temp == "" ? "" : temp.Substring(0, temp.Length - 1);
            TextAddresses.Visibility = temp == "" ? Visibility.Collapsed : Visibility.Visible;

            temp = "";
            foreach (SearchModel.EmailModel data in item.Emails) temp += data.Email + Environment.NewLine;
            TextEmails.Text = temp == "" ? "" : temp.Substring(0, temp.Length - 1);
            TextEmails.Visibility = temp == "" ? Visibility.Collapsed : Visibility.Visible;

            temp = "";
            foreach (SearchModel.Phone data in item.ContactPhones) temp += data.Type + " : " + data.Number + Environment.NewLine;
            TextPhones.Text = temp == "" ? "" : temp.Substring(0, temp.Length - 1);
            TextPhones.Visibility = temp == "" ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ListContacts_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ListView listView = (ListView)e.Source;
            int totalCount = listView.Items.Count;
            int limit = Convert.ToInt32(listView.Tag);
            if (totalCount == 0 || totalCount >= limit) return;
            if (e.VerticalOffset + e.VerticalChange >= totalCount - 6)
            {
                ShowContactList(totalCount, 10);
            }
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            Border border = (Border)sender;
            string contactCode = border.Tag.ToString();

            List<SearchModel.Item> list = (List<SearchModel.Item>)ListContacts.ItemsSource;
            foreach(SearchModel.Item item in list)
            {
                if (item.ContactCode == contactCode)
                {
                    ShowContactDetail(item);
                    break;
                }
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border border = (Border)sender;
            string contactCode = border.Tag.ToString();
            Global.mainWindow.SetAccountFromSearch = true;
            Global.mainWindow.TextAccountNo.Text = contactCode;
            Global.mainWindow.GetDisplayDetails();
            EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    Global.CloseDialog();
                    //Global.mainWindow.SetAccountFromSearch = false;
                    Global.mainWindow.GridAccountNo.Visibility = Visibility.Hidden;
                });

            }, 500);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            IsSimpleSearch = true;
            SearchString = TextAccountNo.Text;
            if (string.IsNullOrEmpty(SearchString)) return;
            ShowContactList(0, 10);
        }

        private void TextAccountNo_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                IsSimpleSearch = true;
                SearchString = TextAccountNo.Text;
                if (string.IsNullOrEmpty(SearchString)) return;
                ShowContactList(0, 10);
            }
        }
    }
}
