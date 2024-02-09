using Newtonsoft.Json;
using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.messages;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
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

namespace SelcommWPF.dialogs.users
{
    /// <summary>
    /// Interaction logic for ServiceProvidersDialog.xaml
    /// </summary>
    public partial class ServiceProvidersDialog : UserControl
    {
        public static ServiceProvidersDialog Instance;

        public ServiceProvidersDialog()
        {
            InitializeComponent();
            ShowServiceProviderUsers(0, 20);
            Instance = this;
        }

        public void ShowServiceProviderUsers(int skip, int take)
        {
            if (skip == 0) ListUsers.ItemsSource = new List<UserModel>();
            LoadingPanel.Visibility = Visibility.Visible;
            string search = TextSearch.Text;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("SearchString", search);
                query.Add("CountRecords", "Y");
                Dictionary<string, object> result = Global.RequestAPI<Dictionary<string, object>>(Constants.ServiceProviderUsersURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, "");
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null || result.Count == 0)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    List<UserModel> list = (List<UserModel>)ListUsers.ItemsSource;
                    ListUsers.ItemsSource = new List<UserModel>();
                    if (list == null) list = new List<UserModel>();

                    List<UserModel> items = JsonConvert.DeserializeObject<List<UserModel>>(result["Users"].ToString());
                    int count = items.Count;
                    ListUsers.Tag = Convert.ToInt32(result["Count"].ToString());

                    for (int i = 0; i < count; i++)
                    {
                        string roles = "";
                        foreach (UserModel.BusinessUnitModel role in items[i].Roles) roles += "," + role.Name;
                        items[i].RolesText = roles == "" ? "" : roles.Substring(1);
                        list.Add(items[i]);
                    }

                    ListUsers.ItemsSource = list;
                    ShowHideVerticalScroll(Global.HasVerticalScroll);
                });

            }, 10);
        }

        private void ShowHideVerticalScroll(bool isChecked)
        {
            ListView[] listViews = new ListView[] { ListUsers};
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

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();   
        }

        private void TextSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowServiceProviderUsers(0, 20);
        }

        private async void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ServiceProviderUser(), "DetailDialog");
        }

        private void ListUsers_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            int totalCount = ListUsers.Items.Count;
            int limit = Convert.ToInt32(ListUsers.Tag);

            if (totalCount < 9 || totalCount >= limit) return;
            if (e.VerticalOffset + e.VerticalChange >= totalCount - 5)
            {
                ShowServiceProviderUsers(totalCount, 20);
            }
        }

        private async void ListItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            UserModel item = (UserModel)((ListViewItem)sender).Content;
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ServiceProviderUser(item.Id), "DetailDialog");
        }
    }
}
