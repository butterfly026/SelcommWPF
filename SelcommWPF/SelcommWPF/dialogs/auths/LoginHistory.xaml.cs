using Microsoft.Win32;
using RestSharp;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SelcommWPF.dialogs.auths
{
    /// <summary>
    /// Interaction logic for LoginHistory.xaml
    /// </summary>
    public partial class LoginHistory : UserControl
    {

        private bool IsLoginPage = false;

        public LoginHistory(bool isLogin = false)
        {
            IsLoginPage = isLogin;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            bool isDisplay = Convert.ToBoolean(key.GetValue("LoginHistory", false));
            key.Close();
            CheckBoxDisplay.IsChecked = isDisplay;
            GetLoginHistory(0, 20);
        }

        private void GetLoginHistory(int skip, int take)
        {
            if (skip == 0) ListLoginHistory.ItemsSource = new List<clients.models.auths.LoginHistory.Item>();
            if (Global.ContactId == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("CountRecords", "Y");
                clients.models.auths.LoginHistory result = Global.RequestAPI<clients.models.auths.LoginHistory>(Constants.LoginHistoryURL, Method.Get,
                    Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("UserId", Global.ContactId), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    List<clients.models.auths.LoginHistory.Item> list = (List<clients.models.auths.LoginHistory.Item>)ListLoginHistory.ItemsSource;
                    ListLoginHistory.ItemsSource = new List<clients.models.auths.LoginHistory.Item>();

                    if (list == null) list = new List<clients.models.auths.LoginHistory.Item>();
                    int count = result == null ? 0 : result.Items.Count;
                    ListLoginHistory.Tag = result.Count;
                    
                    for (int i = 0; i < count; i++)
                    {
                        result.Items[i].Date = StringUtils.ConvertDateTime(result.Items[i].Date);
                        list.Add(result.Items[i]);
                    }

                    ListLoginHistory.ItemsSource = list;
                    ShowHideVerticalScroll(Global.HasVerticalScroll);
                });

            }, 10);
        }

        private void ShowHideVerticalScroll(bool isChecked)
        {
            try
            {
                ScrollViewer sv = VisualTreeHelper.GetChild(ListLoginHistory, 0) as ScrollViewer;
                sv.VerticalScrollBarVisibility = isChecked ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
            if (IsLoginPage) Global.loginWindow.CheckRegisterDetails();
        }

        private void ListLoginHistory_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ListView listView = (ListView)e.Source;
            int totalCount = listView.Items.Count;
            int limit = Convert.ToInt32(listView.Tag);
            if (totalCount == 0 || totalCount >= limit) return;
            if (e.VerticalOffset + e.VerticalChange >= totalCount - 6)
            {
                GetLoginHistory(totalCount, 20);
            }
        }

        private void CheckBoxDisplay_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = e.RoutedEvent.Name == "Checked";
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            key.SetValue("LoginHistory", isChecked);
            key.Close();
        }

        private void CheckBoxSuspect_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            long id = Convert.ToInt64(checkBox.Tag);

            bool isChecked = e.RoutedEvent.Name == "Checked";
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.LoginMakeSuspeck, Method.Put, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("HistoryId", id),
                    Global.BuildDictionary("api-version", 1.0), null);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK)
                    {
                        List<clients.models.auths.LoginHistory.Item> list = (List<clients.models.auths.LoginHistory.Item>)ListLoginHistory.ItemsSource;
                        int count = list.Count;
                        for (int i = 0; i < count; i++)
                        {
                            if (list[i].Id == id)
                            {
                                list[i].Threat = isChecked;
                                list[i].Status = isChecked ? "Suspect" : "Approved";
                                break;
                            }
                        }
                        ListLoginHistory.ItemsSource = new List<clients.models.auths.LoginHistory.Item>();
                        ListLoginHistory.ItemsSource = list;
                    }
                });

            }, 10);

        }
    }
}
