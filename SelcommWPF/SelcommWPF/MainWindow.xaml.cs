using MaterialDesignColors;
using Microsoft.Win32;
using Newtonsoft.Json;
using RestSharp;
using SelcommWPF.clients;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.bills;
using SelcommWPF.clients.models.contacts;
using SelcommWPF.clients.models.messages;
using SelcommWPF.dialogs;
using SelcommWPF.dialogs.accounts;
using SelcommWPF.dialogs.auths;
using SelcommWPF.dialogs.bills;
using SelcommWPF.dialogs.charge;
using SelcommWPF.dialogs.contacts;
using SelcommWPF.dialogs.identification;
using SelcommWPF.dialogs.order;
using SelcommWPF.dialogs.plans;
using SelcommWPF.dialogs.report;
using SelcommWPF.dialogs.services;
using SelcommWPF.dialogs.trans;
using SelcommWPF.dialogs.users;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using static SelcommWPF.clients.models.contacts.DocumentModel;

namespace SelcommWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private long TimeWatch = 0;
        private bool IsFinished = false;
        private List<DisplayDetails> DisplayDetailsList;
        private List<string> AccountList = new List<string>();
        private string CurrentContactCode = "";
        private string SelectedServicesListsType = "List";
        public string AccountType = "";
        private List<MenuModel.MenuItem> ServiceMenuList;

        private int BillsTotalCount = 0;
        public bool SetAccountFromSearch = false;

        private int NOTE_LOADING_COUNT = 20;
        private int BILL_LOADLING_COUNT = 10;

        public MainWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            // get size, location info from registry
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            double width = key.GetValue("width") == null ? Global.ScreenWidth : Convert.ToDouble(key.GetValue("width"));
            double height = key.GetValue("height") == null ? Global.ScreenHeigth - 30 : Convert.ToDouble(key.GetValue("height"));
            double left = key.GetValue("left") == null ? 0 : Convert.ToDouble(key.GetValue("left"));
            double top = key.GetValue("top") == null ? 0 : Convert.ToDouble(key.GetValue("top"));

            // set size and location
            this.Width = width;
            this.Height = height;
            this.Left = left < 0 ? 0 : left;
            this.Top = top < 0 ? 0 : top;

            // get MRU for account no
            string accountString = (string)key.GetValue("account");
            SelectedServicesListsType = (string)key.GetValue("ServiceType");
            key.Close();

            // set MRU for account no
            List<string> list = accountString == null || accountString == ""? new List<string>() : JsonConvert.DeserializeObject<List<string>>(accountString);
            ComboAccountNo.ItemsSource = list;
            AccountList = list;

            Global.mainWindow = this;
            LoadingPanel.Visibility = Visibility.Visible;

            // set status bar info
            TextStatusSite.Text = Constants.SiteId;
            TextStatusLogin.Text = System.Environment.UserName;
            TextStatusVersion.Text = "Ver : " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            TextStatusDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            TextStatusTime.Text = DateTime.Now.ToString("hh:mm tt");

            // check current language
            int count = MenuLanguage.Items.Count;
            for (int i = 0; i < count; i++)
            {
                MenuItem item = (MenuItem) MenuLanguage.Items[i];
                if (item.Tag.ToString() == Global.LanguageCode)
                {
                    item.IsChecked = true;
                    break;
                }
            }

            count = MenuServiceList.Items.Count;
            SelectedServicesListsType = SelectedServicesListsType ?? "List";

            for (int i = 0; i < count; i++)
            {
                MenuItem item = (MenuItem)MenuServiceList.Items[i];
                item.IsChecked = SelectedServicesListsType == item.Tag.ToString();
            }

            // set status bar info and refresh token every 10 mins
            EasyTimer.SetInterval(() =>
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        TextStatusDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
                        TextStatusTime.Text = DateTime.Now.ToString("hh:mm tt");

                        TimeWatch++;
                        if (TimeWatch % 200 == 0)
                        {
                            if (Global.AccessToken == null || Global.AccessToken.Credentials == null)
                            {
                                new SplashWindow().Show();
                                Close();
                                return;
                            }

                            Dictionary<string, string> header = Global.GetHeader(Global.AccessToken);
                            header.Add("Content-Type", "application/json");
                            Dictionary<string, object> path = Global.BuildDictionary("SiteId", Constants.SiteId);
                            path.Add("identifier", Global.UserID);
                            Dictionary<string, object> query = Global.BuildDictionary("api-version", 3.1);

                            Global.AccessToken = Global.RequestAPI<TokenResponse>(Constants.RefreshTokenURL, Method.Post, header, path, query, string.Format("\"{0}\"", Global.Password));
                            if (Global.AccessToken == null || Global.AccessToken.Credentials == null)
                            {
                                new LoginWindow().Show();
                                Close();
                                return;
                            }

                            Global.CustomerToken = Global.RequestAPI<TokenResponse>(Constants.CustomerTokenURL, Method.Post, Global.GetHeader(Global.AccessToken), null, query, "");
                            if (Global.CustomerToken == null || Global.CustomerToken.Credentials == null)
                            {
                                new LoginWindow().Show();
                                Close();
                                return;
                            }
                        }
                    });
                }
                catch { }
            }, 1000);

            // get user info
            EasyTimer.SetTimeout(() =>
            {
                Global.servicesClient = new ServicesClient();
                Global.financeClient = new FinancialsClient();
                Global.transactionsClient = new TransactionsClient();
                Global.PayMethodClient = new PayMethodClient();
                Global.planClient = new PlanClient();
                Global.reportClient = new ReportClient();
                Global.messageClient = new MessageClient();

                DisplayDetailsList = new List<DisplayDetails>();

                Dictionary<string, string> header = Global.GetHeader(Global.CustomerToken);
                Dictionary<string, object> path = Global.BuildDictionary("userId", Global.UserID);
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);

                Global.LoggedUser = Global.RequestAPI<UserModel>(Constants.SimpleUserURL, Method.Get, header, path, query, "", false);
                if (Global.LoggedUser != null && Global.LoggedUser.Id != null)
                {
                    path["userId"] = Global.LoggedUser.Id;
                    Global.LoggedUser = Global.RequestAPI<UserModel>(Constants.FullUserURL, Method.Get, header, path, query, "", false);
                }

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    ShowHideVerticalScroll(false);
                    if (Global.LoggedUser == null || Global.LoggedUser.Id == null)
                    {
                        TerminateApplication();
                        return;
                    }
                    TextStatusUser.Text = Global.LoggedUser.Name;
                    LabelBusinessUnit.Content = Global.LoggedUser.BusinessUnits[0].Name;
                });

            }, 10);

        }
        
        private void TerminateApplication()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    Application.Current.Shutdown();
                });

            }, 10000);
        }

        // account search button event
        private void Button_Account_Search(object sender, RoutedEventArgs e)
        {
            SearchContactCode();
        }
        private async void Button_Advanced_Search(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new AdvancedSearch(), "MyDialogHost");
        }

        private void Button_Account_Refresh(object sender, RoutedEventArgs e)
        {
            GetDisplayDetails();
        }

        // when press enter key, search account info
        private void TextAccountNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchContactCode();
            } 
            else if (e.Key == Key.Down || e.Key == Key.Tab)
            {
                ComboAccountNo.SelectedIndex = 0;
            }
        }

        private void TextAccountNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SetAccountFromSearch) return;
            List<string> list = new List<string>();
            int count = AccountList.Count;

            if (TextAccountNo.Text == "")
            {
                ComboAccountNo.ItemsSource = AccountList;
                return;
            }

            for (int i = 0; i < count; i++) if (AccountList[i].Contains(TextAccountNo.Text)) list.Add(AccountList[i]);
            ComboAccountNo.ItemsSource = list;
            GridAccountNo.Visibility = list.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void TextAccountNo_ChangeFocus(object sender, RoutedEventArgs e)
        {
            string eventName = e.RoutedEvent.Name;
            if (eventName == "LostFocus") GridAccountNo.Visibility = Visibility.Collapsed;
            else if (eventName == "GotFocus")
            {
                if (SetAccountFromSearch) SetAccountFromSearch = false;
                else GridAccountNo.Visibility = Visibility.Visible;
            }
        }

        private void TextAccountNo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ComboAccountNo.SelectedItem == null) return;
                TextAccountNo.Text = ComboAccountNo.SelectedItem.ToString().Split('-')[0];
                GetDisplayDetails();
                GridAccountNo.Visibility = Visibility.Collapsed;
            }
            catch { }
        }

        // windows close event
        private async void Windows_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsFinished)
            {
                e.Cancel = true;
                bool result = await MessageUtils.ConfirmMessageAsync(Properties.Resources.Alert, Properties.Resources.Finish);
                if (result)
                {
                    IsFinished = true;
                    Application.Current.Shutdown();
                }
            }
        }

        private void GetAccountMenuList()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                MenuModel result = Global.RequestAPI<MenuModel>(Constants.MenuContactURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("ContactCode", CurrentContactCode), Global.BuildDictionary("api-version", 1.0), "", false);
                
                Application.Current.Dispatcher.Invoke(delegate
                {

                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null || result.MenuItems == null || result.MenuItems.Count == 0)
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources1.Menu_List_Error);
                        return;
                    }

                    MenuAccount.IsEnabled = true;
                    MenuAccount.Items.Clear();
                    List<MenuModel.MenuItem> menuList = result.MenuItems;
                    RenderMenuItem(MenuAccount, menuList);
                });

            }, 10);

        }

        private void RenderMenuItem(MenuItem menu, List<MenuModel.MenuItem> list, bool isService = false)
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                MenuModel.MenuItem content = list[i];
                MenuItem item = new MenuItem();
                item.Header = content.Caption;
                Dictionary<string, string> tag = new Dictionary<string, string>();
                tag.Add("link", content.NavigationURL);
                tag.Add("type", isService ? "1" : "0");
                item.Tag = tag;
                if (content.Tooltip != "tooltip") item.ToolTip = content.Tooltip;
                if (content.MenuItems.Count > 0) RenderMenuItem(item, content.MenuItems, isService);
                else item.Click += Menu_Click_Event;
                menu.Items.Add(item);
            }

        }

        private async void SearchContactCode()
        {
            string search = TextAccountNo.Text;
            if (search == "") return;

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new SearchDialog(search), "MyDialogHost");
        }

        // get display detail from account number
        public void GetDisplayDetails()
        {
            DisplayDetailsList = new List<DisplayDetails>();
            ListView[] listViews = new ListView[]{ ListDisplayDetails, ListContactNotes, ListServicesDetail, ListServices, ListServicesType,
                                                ListServicesNotes, ListFinancials, ListTransactions, ListCreditCard, ListBankTransfer, ListEvents,
                                                ListServicesStatus, ListServicesCost, ListServicesChanges, ListServicesPlans, ListTasks, ListContactTasks,
                                                ListDocuments, ListServiceDocuments, ListServicesEvents, ListMessages};
            foreach (ListView listview in listViews) listview.ItemsSource = null;

            string accountNo = TextAccountNo.Text;
            if (accountNo.Contains("-")) accountNo = accountNo.Split('-')[0];
            if (accountNo == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }
            
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                DisplayDetailsList = Global.RequestAPI<List<DisplayDetails>>(Constants.DisplayDetailsURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("ContactCode", accountNo), Global.BuildDictionary("api-version", 1.1), "");

                Application.Current.Dispatcher.Invoke(delegate
                {

                    if (DisplayDetailsList == null) DisplayDetailsList = new List<DisplayDetails>();
                    int count = DisplayDetailsList.Count;
                    string accountName = "";

                    for (int i = 0; i < count; i++)
                    {
                        if (DisplayDetailsList[i].DisplayGroup == null)
                        {
                            DisplayDetailsList.RemoveAt(i);
                            count--;
                            i--;
                        }

                        if (DisplayDetailsList[i].Label == "Name") accountName = DisplayDetailsList[i].Value;
                        if (DisplayDetailsList[i].Label == "Bill Cycle") Global.BillCycle = DisplayDetailsList[i].Value;
                        if (DisplayDetailsList[i].Label == "Type") AccountType = DisplayDetailsList[i].Value;

                        try
                        {
                            if (DisplayDetailsList[i].Label == "Account Balance") DisplayDetailsList[i].Value = StringUtils.ConvertCurrency(Convert.ToDouble(DisplayDetailsList[i].Value));
                            if (DisplayDetailsList[i].Label == "Deposits Held") DisplayDetailsList[i].Value = StringUtils.ConvertCurrency(Convert.ToDouble(DisplayDetailsList[i].Value));
                            if (DisplayDetailsList[i].Label == "current Amount") DisplayDetailsList[i].Value = StringUtils.ConvertCurrency(Convert.ToDouble(DisplayDetailsList[i].Value));
                            if (DisplayDetailsList[i].Label == "31 - 60 Days Overdue") DisplayDetailsList[i].Value = StringUtils.ConvertCurrency(Convert.ToDouble(DisplayDetailsList[i].Value));
                            if (DisplayDetailsList[i].Label == "91 or More Days Overdue") DisplayDetailsList[i].Value = StringUtils.ConvertCurrency(Convert.ToDouble(DisplayDetailsList[i].Value));
                            if (DisplayDetailsList[i].Label == "Transcations In Progress") DisplayDetailsList[i].Value = StringUtils.ConvertCurrency(Convert.ToDouble(DisplayDetailsList[i].Value));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }

                    }

                    // get MRU for account no
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
                    string accountString = (string)key.GetValue("account");
                    List<string> list = accountString == null || accountString == "" ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(accountString);
                    string accountData = string.Format("{0}-{1}", accountNo, accountName);

                    if (DisplayDetailsList.Count > 0)
                    {
                        CurrentContactCode = accountNo;
                        Global.ContactCode = CurrentContactCode;

                        count = list.Count;
                        bool isExist = false;
                        
                        for (int i = 0; i < count; i++)
                        {
                            if (list[i].Contains(accountNo))
                            {
                                if (list[i] != accountData) list[i] = accountData;
                                isExist = true;
                                break;
                            }
                        }

                        if (!isExist)
                        {
                            list.Add(accountData);
                            AccountList.Add(accountData);
                        }

                        list.Sort();
                        AccountList.Sort();

                        ComboAccountNo.ItemsSource = new List<string>();
                        ComboAccountNo.ItemsSource = list;
                        key.SetValue("account", JsonConvert.SerializeObject(list));
                        key.Close();

                        ListDisplayDetails.ItemsSource = DisplayDetailsList;
                        LoadingPanel.Visibility = Visibility.Hidden;
                        LoadTabInfo(); // when load account info, load tab data by selected item
                        LoadServiceLists();
                        GetAccountMenuList();
                    }
                    else
                    {
                        count = list.Count;
                        for (int i = 0; i < count; i++)
                        {
                            if (list[i].Contains(accountNo))
                            {
                                list.RemoveAt(i);
                                break;
                            }
                        }

                        ComboAccountNo.ItemsSource = new List<string>();
                        ComboAccountNo.ItemsSource = list;
                        TextAccountNo.Text = "";
                        key.SetValue("account", JsonConvert.SerializeObject(list));
                        key.Close();
                        LoadingPanel.Visibility = Visibility.Hidden;
                    }
                });

            }, 10);
        }
    
        // windows resize event
        // when resize windows, save width and height in registry
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            key.SetValue("width", e.NewSize.Width);
            key.SetValue("height", e.NewSize.Height);
            key.Close();
        }
        
        // windows relocation event
        // when relocation windows, save width and height in registry
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            key.SetValue("left", ((Window)sender).Left);
            key.SetValue("top", ((Window)sender).Top);
            key.Close();
        }

        // when click option/reset panel size menu, set size to default
        private async void Menu_Reset_Size(object sender, RoutedEventArgs e)
        {
            bool result = await MessageUtils.ConfirmMessageAsync(Properties.Resources.Alert, Properties.Resources.Reset_Factory_Default);
            if (result)
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
                key.DeleteValue("width");
                key.DeleteValue("height");
                key.DeleteValue("left");
                key.DeleteValue("top");
                key.Close();
                this.Width = Global.ScreenWidth;
                this.Height = Global.ScreenHeigth;
                this.Left = 0;
                this.Top = 0;
                key.Close();
            }
        }

        // change password
        private async void Menu_Change_Password(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ChangePassword(), "MyDialogHost");
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DisplayDetails item = (DisplayDetails)ListDisplayDetails.SelectedItem;
            if (item == null) return;
            string link = item.NavigationPath;
            if (link == null)
            {
                MessageUtils.ShowErrorMessage("", Properties.Resources1.Invalid_Link);
                return;
            }

            ListDisplayDetails.SelectedIndex = -1;
            RedirectAccountLink(link);
        }

        private void Grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DisplayDetails seletedItem = new DisplayDetails();
            long menuId = Convert.ToInt64(((Grid)sender).Tag);
            List<DisplayDetails> list = (List<DisplayDetails>)ListDisplayDetails.ItemsSource;

            foreach(DisplayDetails item in list)
            {
                if (item.MenuId == menuId)
                {
                    seletedItem = item;
                    break;
                }
            }

            if (seletedItem.MenuId != 0)
            {
                FrameworkElement fe = e.Source as FrameworkElement;
                ContextMenu cm = fe.ContextMenu ?? new ContextMenu();

                List<MenuModel.MenuItem> menuItems = seletedItem.MenuItems ?? new List<MenuModel.MenuItem>();
                foreach (MenuModel.MenuItem item in menuItems)
                {
                    MenuItem mi4 = new MenuItem();
                    mi4.Header = item.Caption;
                    Dictionary<string, string> tag = new Dictionary<string, string>();
                    tag.Add("link", item.NavigationURL);
                    tag.Add("type", "1");
                    mi4.Tag = tag;
                    mi4.Click += Menu_Click_Event;
                    cm.Items.Add(mi4);
                }

                cm.IsOpen = true;
                cm.Focus();
            }

        }

        // show notes info
        // type - 0 : Contact Notes, 1: Services Notes
        public void ShowNotes(int skip, int take, int type = 0)
        {
            if (CurrentContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            string url = Constants.NotesListURL;
            string code = CurrentContactCode;
            ListView listView = ListContactNotes;
            string search = TextNoteSearch.Text;

            if (type == 1)
            {
                url = Constants.ServicesNoteURL;
                listView = ListServicesNotes;
                ServicesResponse.ServicesModel services = (ServicesResponse.ServicesModel)ListServices.SelectedItem;
                code = services.ServiceReference + "";
                search = TextServicesNoteSearch.Text;
            }

            if (skip == 0) listView.ItemsSource = new List<NoteResponse.NoteModel>();
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", new double[] { 1.1, 1.2 }[type]);
                query.Add("SearchString", search);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("CountRecords", "Y");
                NoteResponse result = Global.RequestAPI<NoteResponse>(url, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("code", code), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<NoteResponse.NoteModel> list = (List<NoteResponse.NoteModel>)listView.ItemsSource;
                    listView.ItemsSource = new List<NoteResponse.NoteModel>();

                    if (list == null) list = new List<NoteResponse.NoteModel>();
                    int count = result == null || result.Notes == null ? 0 : result.Notes.Count;

                    for (int i = 0; i < count; i++)
                    {
                        result.Notes[i].Created = StringUtils.ConvertDateTime(result.Notes[i].Created);
                        result.Notes[i].Body = StringUtils.Remove_Escape_Sequences(result.Notes[i].Body);
                        list.Add(result.Notes[i]);
                    }

                    listView.Tag = result.Count;
                    listView.ItemsSource = list;
                    LoadingPanel.Visibility = Visibility.Hidden;
                    ShowHideVerticalScroll(MenuVScroll.IsChecked);
                });

            }, 10);

        }

        private void ShowBillHistories(int skip, int take)
        {
            if (skip == 0) ListBillHistory.ItemsSource = new List<BillHistoryModel.BillDetail>();
            if (CurrentContactCode == "")
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
                BillHistoryModel result = Global.RequestAPI<BillHistoryModel>(Constants.BillHistoryURL, Method.Get, Global.GetHeader(Global.CustomerToken), 
                    Global.BuildDictionary("contactCode", CurrentContactCode), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<BillHistoryModel.BillDetail> list = (List<BillHistoryModel.BillDetail>)ListBillHistory.ItemsSource;
                    ListBillHistory.ItemsSource = new List<BillHistoryModel.BillDetail>();

                    BillsTotalCount = result.Count;
                    LabelPageSummary.Content = string.Format("{0} ~ {1} of {2}", skip + 1, skip + take, BillsTotalCount);

                    if (list == null) list = new List<BillHistoryModel.BillDetail>();
                    int count = result.Bills.Count;
                    ListBillHistory.Tag = result.Count;

                    for (int i = 0; i < count; i++)
                    {
                        result.Bills[i].Created = StringUtils.ConvertDateTime(result.Bills[i].Created);
                        result.Bills[i].BillDate = StringUtils.ConvertDate(result.Bills[i].BillDate);
                        result.Bills[i].DueDate = StringUtils.ConvertDate(result.Bills[i].DueDate);
                        result.Bills[i].LastUpdated = StringUtils.ConvertDateTime(result.Bills[i].LastUpdated);

                        result.Bills[i].AmountDueText = StringUtils.ConvertCurrency(result.Bills[i].AmountDue);
                        result.Bills[i].NewChargesText = StringUtils.ConvertCurrency(result.Bills[i].NewCharges);
                        result.Bills[i].PaymentAdjustmentAmountText = StringUtils.ConvertCurrency(result.Bills[i].PaymentAdjustmentAmount);
                        result.Bills[i].PreviousBalanceText = StringUtils.ConvertCurrency(result.Bills[i].PreviousBalance);
                        result.Bills[i].BalanceText = StringUtils.ConvertCurrency(result.Bills[i].Balance);
                        result.Bills[i].DisputedAmountText = StringUtils.ConvertCurrency(result.Bills[i].DisputedAmount);
                        result.Bills[i].InstallmentAmountText = StringUtils.ConvertCurrency(result.Bills[i].InstallmentAmount);
                        result.Bills[i].DepositAmountText = StringUtils.ConvertCurrency(result.Bills[i].DepositAmount);

                        for (int j = 0; j < result.Bills[i].FinancialDocuments.Count; j++)
                        {
                            result.Bills[i].FinancialDocuments[j].Date = StringUtils.ConvertDate(result.Bills[i].FinancialDocuments[j].Date);
                            result.Bills[i].FinancialDocuments[j].DueDate = StringUtils.ConvertDate(result.Bills[i].FinancialDocuments[j].DueDate);
                            result.Bills[i].FinancialDocuments[j].Created = StringUtils.ConvertDateTime(result.Bills[i].FinancialDocuments[j].Created);
                            result.Bills[i].FinancialDocuments[j].LastUpdated = StringUtils.ConvertDateTime(result.Bills[i].FinancialDocuments[j].LastUpdated);

                            result.Bills[i].FinancialDocuments[j].AmountText = StringUtils.ConvertCurrency(result.Bills[i].FinancialDocuments[j].Amount);
                            result.Bills[i].FinancialDocuments[j].TaxAmountText = StringUtils.ConvertCurrency(result.Bills[i].FinancialDocuments[j].TaxAmount);
                        }

                        result.Bills[i].Visiblity = "Collapsed";
                        MaterialDesignThemes.Wpf.PackIcon icon = new MaterialDesignThemes.Wpf.PackIcon();
                        icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ChevronRight;
                        result.Bills[i].ButtonIcon = icon;

                        list.Add(result.Bills[i]);
                    }

                    ListBillHistory.ItemsSource = list;
                    ButtonBillEmail.IsEnabled = false;
                    ButtonBillServices.IsEnabled = false;
                    ButtonBillTransactions.IsEnabled = false;
                    ButtonBillCharges.IsEnabled = false;
                    ButtonBillPdf.IsEnabled = false;
                    ButtonBillExcel.IsEnabled = false;
                    ButtonBillDelete.IsEnabled = false;
                    ButtonBillDisputes.IsEnabled = false;
                    LoadingPanel.Visibility = Visibility.Hidden;
                    ShowHideVerticalScroll(MenuVScroll.IsChecked);
                });

            }, 10);
        }

        public void ShowMessageList(int skip, int take)
        {
            if (skip == 0) ListMessages.ItemsSource = new List<MessageModel.Item>();
            if (Global.ContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            string search = TextMessageSearch.Text;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("CountRecords", "Y");
                query.Add("ContactOnly", true);
                MessageModel result = Global.RequestAPI<MessageModel>(Constants.MessagesListURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("ContactCode", CurrentContactCode), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<MessageModel.Item> list = (List<MessageModel.Item>)ListMessages.ItemsSource;
                    ListMessages.ItemsSource = new List<MessageModel.Item>();

                    if (list == null) list = new List<MessageModel.Item>();
                    int count = result == null || result.Messages == null ? 0 : result.Messages.Count;
                    ListMessages.Tag = result.Count;

                    for (int i = 0; i < count; i++)
                    {
                        result.Messages[i].VisiblityNew = result.Messages[i].Status == "NEW" ? "Visible" : "Collapsed";
                        result.Messages[i].VisiblityOther = result.Messages[i].Status != "NEW" ? "Visible" : "Collapsed";
                        result.Messages[i].AddressText = result.Messages[i].Addresses == null || result.Messages[i].Addresses.Count == 0 ? "" : result.Messages[i].Addresses[0].Address;
                        list.Add(result.Messages[i]);
                    }

                    ListMessages.ItemsSource = list;
                    ShowHideVerticalScroll(Global.HasVerticalScroll);
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private void ListMessages_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ListView listView = (ListView)e.Source;
            int totalCount = listView.Items.Count;
            int limit = Convert.ToInt32(listView.Tag);
            if (totalCount == 0 || totalCount >= limit) return;
            if (e.VerticalOffset + e.VerticalChange >= totalCount - 6)
            {
                ShowMessageList(totalCount, 10);
            }
        }

        private void TextMessageSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowMessageList(0, 20);
        }

        private void ShowFinancials(int skip, int take)
        {
            if (skip == 0) ListFinancials.ItemsSource = new List<Financials.TransactionDetail>();
            if (CurrentContactCode == "")
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
                query.Add("countRecords", "Y");
                Financials result = Global.RequestAPI<Financials>(Constants.FinancialsListURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("code", CurrentContactCode), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<Financials.TransactionDetail> list = (List<Financials.TransactionDetail>)ListFinancials.ItemsSource;
                    ListFinancials.ItemsSource = new List<Financials.TransactionDetail>();

                    if (list == null) list = new List<Financials.TransactionDetail>();
                    int count = result == null ? 0 : result.FinancialTransactions.Count;
                    ListFinancials.Tag = result == null ? 0 : result.Count;

                    for (int i = 0; i < count; i++)
                    {
                        result.FinancialTransactions[i].Date = StringUtils.ConvertDate(result.FinancialTransactions[i].Date);

                        result.FinancialTransactions[i].AmountText = StringUtils.ConvertCurrency(result.FinancialTransactions[i].Amount);
                        result.FinancialTransactions[i].OpenAmountText = StringUtils.ConvertCurrency(result.FinancialTransactions[i].OpenAmount);
                        result.FinancialTransactions[i].RunningBalanceText = StringUtils.ConvertCurrency(result.FinancialTransactions[i].RunningBalance);

                        list.Add(result.FinancialTransactions[i]);
                    }

                    ListFinancials.ItemsSource = list;
                    LoadingPanel.Visibility = Visibility.Hidden;
                    ShowHideVerticalScroll(MenuVScroll.IsChecked);
                });

            }, 10);
        }

        private void ShowTransactions(int skip, int take)
        {
            if (skip == 0) ListTransactions.ItemsSource = new List<Transactions.TransactionDetail>();
            if (CurrentContactCode == "")
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
                query.Add("countRecords", "Y");
                Transactions result = Global.RequestAPI<Transactions>(Constants.TransactionsListURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("code", CurrentContactCode), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<Transactions.TransactionDetail> list = (List<Transactions.TransactionDetail>)ListTransactions.ItemsSource;
                    ListTransactions.ItemsSource = new List<Transactions.TransactionDetail>();

                    if (list == null) list = new List<Transactions.TransactionDetail>();
                    int count = result == null ? 0 : result.FinancialTransactions.Count;
                    ListTransactions.Tag = result.Count;

                    for (int i = 0; i < count; i++)
                    {
                        result.FinancialTransactions[i].Created = StringUtils.ConvertDateTime(result.FinancialTransactions[i].Created);
                        result.FinancialTransactions[i].LastUpdated = StringUtils.ConvertDateTime(result.FinancialTransactions[i].LastUpdated);

                        result.FinancialTransactions[i].AmountText = StringUtils.ConvertCurrency(result.FinancialTransactions[i].Amount);
                        result.FinancialTransactions[i].TaxAmountText = StringUtils.ConvertCurrency(result.FinancialTransactions[i].TaxAmount);
                        result.FinancialTransactions[i].OpenAmountText = StringUtils.ConvertCurrency(result.FinancialTransactions[i].OpenAmount);
                        result.FinancialTransactions[i].RoundingAmountText = StringUtils.ConvertCurrency(result.FinancialTransactions[i].RoundingAmount);

                        list.Add(result.FinancialTransactions[i]);
                    }

                    ListTransactions.ItemsSource = list;
                    ButtonTransDetail.IsEnabled = false;
                    LoadingPanel.Visibility = Visibility.Hidden;
                    ShowHideVerticalScroll(MenuVScroll.IsChecked);
                });

            }, 10);
        }

        public void ShowPaymentMethods()
        {
            if (CurrentContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.1);
                query.Add("Open", true);
                query.Add("DefaultOnly", false);
                List<PaymentMethod> result = Global.RequestAPI<List<PaymentMethod>>(Constants.PayMethodListURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("ContactCode", CurrentContactCode), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result == null)
                    {
                        LoadingPanel.Visibility = Visibility.Hidden;
                        return;
                    }

                    int count = result.Count;
                    List<PaymentMethod> creditCardList = new List<PaymentMethod>();
                    List<PaymentMethod> bankTransferList = new List<PaymentMethod>();

                    for (int i = 0; i < count; i++)
                    {
                        result[i].CreditCard = string.Format("{0} ending in {1}", result[i].Description, 
                            result[i].AccountNumber.Substring(result[i].AccountNumber.Length - 4));
                        result[i].Enabled = true;//!result[i].Default;
                        result[i].DefaultText = result[i].Default ? Properties.Resources.Default_Payment_Method : Properties.Resources.Make_Default;
                        if (result[i].CategoryCode == "C") creditCardList.Add(result[i]);
                        else if (result[i].CategoryCode == "D") bankTransferList.Add(result[i]);
                    }

                    ListCreditCard.ItemsSource = creditCardList;
                    ListBankTransfer.ItemsSource = bankTransferList;
                    LoadingPanel.Visibility = Visibility.Hidden;
                    ShowHideVerticalScroll(MenuVScroll.IsChecked);
                });

            }, 10);
        }

        public void ShowDocumentList(bool isService = false)
        {
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("Resource", isService ? "/Services/Documents" : "/Contacts/Documents");
                query.Add("api-version", 1.2);
                HttpStatusCode result = Global.RequestAPI(Constants.AuthrisationURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, null);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result != HttpStatusCode.OK)
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources1.Feature_Error);
                        return;
                    }
                    if (CurrentContactCode == "")
                    {
                        MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                        return;
                    }

                    string code = CurrentContactCode;
                    if (isService)
                    {
                        ServicesResponse.ServicesModel services = (ServicesResponse.ServicesModel)ListServices.SelectedItem;
                        code = services.ServiceReference + "";
                    }

                    string url = isService ? Constants.ServiceDocumentsGetURL : Constants.DocumentsGetURL;
                    ListView listView = isService ? ListServiceDocuments : ListDocuments;
                    LoadingPanel.Visibility = Visibility.Visible;

                    EasyTimer.SetTimeout(() =>
                    {
                        DocumentModel.Account result1 = Global.RequestAPI<DocumentModel.Account>(url, Method.Get, Global.GetHeader(Global.CustomerToken),
                            Global.BuildDictionary(isService ? "ServiceReference" : "ContactCode", code), Global.BuildDictionary("api-version", 2.0), "");
                        List<DocumentModel.Detail> documents = result1.Documents;

                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            if (documents == null)
                            {
                                LoadingPanel.Visibility = Visibility.Hidden;
                                return;
                            }

                            listView.ItemsSource = documents;
                            LoadingPanel.Visibility = Visibility.Hidden;
                            ShowHideVerticalScroll(MenuVScroll.IsChecked);
                        });

                    }, 10);
                });

            }, 10);
        }

        private void ShowEventList(int skip, int take, int type = 0)
        {
            if (CurrentContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            ListView listView = new ListView[] { ListEvents, ListServicesEvents }[type];
            if (skip == 0) listView.ItemsSource = new List<EventResponse.EventModel>();

            string code = CurrentContactCode;
            string url = Constants.EventListURL;
            string search = TextEventSearch.Text;

            if (type == 1)
            {
                ServicesResponse.ServicesModel services = (ServicesResponse.ServicesModel)ListServices.SelectedItem;
                code = services.ServiceReference + "";
                url = Constants.EventServiceListURL;
                search = TextServicesEventSearch.Text;
            }

            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("countRecords", "Y");
                query.Add("ExludeNoteEvents", true);
                query.Add("SearchString", search);
                EventResponse result = Global.RequestAPI<EventResponse>(url, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary(new string[] { "ContactCode", "ServiceReference" }[type], code), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<EventResponse.EventModel> list = (List<EventResponse.EventModel>)listView.ItemsSource;
                    listView.ItemsSource = new List<EventResponse.EventModel>();

                    if (list == null) list = new List<EventResponse.EventModel>();
                    int count = result == null || result.Count == 0 ? 0 : result.Events.Count;
                    listView.Tag = result.Count;

                    for (int i = 0; i < count; i++)
                    {
                        result.Events[i].Note = StringUtils.Remove_Escape_Sequences(result.Events[i].Note);
                        result.Events[i].StatusDateTime = StringUtils.ConvertDateTime(result.Events[i].StatusDateTime);
                        result.Events[i].Due = StringUtils.ConvertDateTime(result.Events[i].Due);
                        result.Events[i].Created = StringUtils.ConvertDateTime(result.Events[i].Created);
                        list.Add(result.Events[i]);
                    }

                    listView.ItemsSource = list;
                    LoadingPanel.Visibility = Visibility.Hidden;
                    ShowHideVerticalScroll(MenuVScroll.IsChecked);
                });

            }, 10);
        }
        
        private void ShowTaskList(int skip, int take, int type = 0)
        {
            if (CurrentContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            ListView listView = new ListView[] { ListContactTasks, ListTasks}[type];
            if (skip == 0) listView.ItemsSource = new List<TaskModel.Item>();

            string code = CurrentContactCode;
            string url = Constants.TaskListURL;
            string search = TextContactTaskSearch.Text;

            if (type == 1)
            {
                ServicesResponse.ServicesModel services = (ServicesResponse.ServicesModel)ListServices.SelectedItem;
                code = services.ServiceReference + "";
                url = Constants.ServiceTaskListURL;
                search = TextTaskSearch.Text;
            }
            
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("CountRecords", "Y");
                query.Add("SearchString", search);
                TaskModel result = Global.RequestAPI<TaskModel>(url, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary(new string[] { "ContactCode", "ServiceReference" }[type], code), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<TaskModel.Item> list = (List<TaskModel.Item>)listView.ItemsSource;
                    listView.ItemsSource = new List<TaskModel.Item>();

                    if (list == null) list = new List<TaskModel.Item>();
                    int count = result == null ? 0 : result.Items.Count;
                    listView.Tag = result.Count;

                    for (int i = 0; i < count; i++)
                    {
                        result.Items[i].Created = StringUtils.ConvertDateTime(result.Items[i].Created);
                        result.Items[i].Updated = StringUtils.ConvertDateTime(result.Items[i].Updated);
                        result.Items[i].RequiredDate = StringUtils.ConvertDateTime(result.Items[i].RequiredDate);
                        result.Items[i].EstimatedDate = StringUtils.ConvertDateTime(result.Items[i].EstimatedDate);
                        result.Items[i].NextFollowupDate = StringUtils.ConvertDateTime(result.Items[i].NextFollowupDate);
                        result.Items[i].QuotedPriceText = result.Items[i].QuotedPrice == null ? "" : StringUtils.ConvertCurrency(result.Items[i].QuotedPrice.Value);

                        foreach (TaskModel.Email item in result.Items[i].Emails) result.Items[i].EmailsText += ", " + item.Address;
                        result.Items[i].EmailsText = result.Items[i].EmailsText.Substring(2);

                        list.Add(result.Items[i]);
                    }

                    listView.ItemsSource = list;
                    LoadingPanel.Visibility = Visibility.Hidden;
                    ShowHideVerticalScroll(MenuVScroll.IsChecked);
                });

            }, 10);
        }

      
     
        // note add event
        private async void Button_Add_Note_Click(object sender, RoutedEventArgs e)
        {

            if (DisplayDetailsList == null || DisplayDetailsList.Count == 0) return;
            if (CurrentContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            string code = CurrentContactCode;
            int type = Convert.ToInt16(((Button)sender).Tag);

            if (type == 1)
            {
                ServicesResponse.ServicesModel services = (ServicesResponse.ServicesModel)ListServices.SelectedItem;
                code = services.ServiceReference + "";
            }

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) MaterialDesignThemes.Wpf.DialogHost.Close("MyDialogHost");
            await MaterialDesignThemes.Wpf.DialogHost.Show(new NoteDialog(type, code, Properties.Resources.Add_New_Note, 0, ""), "MyDialogHost");
        }

        // note edit event
        private void Button_Edit_Note_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            long id = Convert.ToInt32(button.Tag);
            int type = Convert.ToInt16(button.Margin.Bottom);
            EditNote(id, type);
        }

        // when double click note list view item, edit note
        private void ListItemNote_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            int type = 0;
            NoteResponse.NoteModel model = (NoteResponse.NoteModel)((ListViewItem)sender).Content;

            try
            {
                type = Convert.ToInt16(((TextBlock)e.OriginalSource).Tag);
            }
            catch
            {
                type = Convert.ToInt16(((Grid)VisualTreeHelper.GetChild(((Border)e.OriginalSource).Child, 0)).Tag);
            }
            
            EditNote(model.Id, type);
        }

        private async void EditNote(long id, int type)
        {
            if (CurrentContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            ListView listView = new List<ListView>() { ListContactNotes, ListServicesNotes }[type];
            string code = CurrentContactCode;

            if (type == 1)
            {
                ServicesResponse.ServicesModel services = (ServicesResponse.ServicesModel)ListServices.SelectedItem;
                code = services.ServiceReference + "";
            }

            List<NoteResponse.NoteModel> list = (List<NoteResponse.NoteModel>)listView.ItemsSource;
            int count = list.Count;
            NoteResponse.NoteModel item = null;

            for (int i = 0; i < count; i++)
            {
                if (Convert.ToInt64(list[i].Id) == id)
                {
                    item = list[i];
                    break;
                }
            }
          
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) MaterialDesignThemes.Wpf.DialogHost.Close("MyDialogHost");
            await MaterialDesignThemes.Wpf.DialogHost.Show(new NoteDialog(type, code, Properties.Resources.Edit_Note, id, item.Body, item.CreatedBy, item.Created), "MyDialogHost");
        }
        
        // exit menu event
        private async void Menu_Exit_Click(object sender, RoutedEventArgs e)
        {
            bool result = await MessageUtils.ConfirmMessageAsync(Properties.Resources.Alert, Properties.Resources.Finish);
            if (result)
            {
                IsFinished = true;
                Application.Current.Shutdown();
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                string name = ((TabControl)e.Source).Name;
                if (name == "TabAccountInfo") LoadTabInfo();
                else if (name == "TabServicesDetail") LoadTabInfoDetail();
            }
        }

        private void LoadTabInfo()
        {
            if (DisplayDetailsList == null || DisplayDetailsList.Count == 0) return;
            if (Global.contactClient == null) Global.contactClient = new ContactClient();
            int selectedIndex = TabAccountInfo.SelectedIndex;

            switch (selectedIndex)
            {
                case 0:
                    ShowNotes(0, NOTE_LOADING_COUNT);
                    break;
                case 1:
                    ShowBillHistories(0, BILL_LOADLING_COUNT);
                    break;
                case 2:
                    ShowMessageList(0, BILL_LOADLING_COUNT);
                    break;
                case 3:
                    ShowEventList(0, BILL_LOADLING_COUNT);
                    break;
                case 4:
                    ShowFinancials(0, BILL_LOADLING_COUNT);
                    break;
                case 5:
                    ShowTransactions(0, BILL_LOADLING_COUNT);
                    break;
                case 6:
                    ShowPaymentMethods();
                    break;
                case 7:
                    ShowTaskList(0, BILL_LOADLING_COUNT);
                    break;
                case 8:
                    ShowDocumentList();
                    break;
            }
        }

        public void LoadTabInfoDetail()
        {
            if (Global.contactClient == null || DisplayDetailsList == null || DisplayDetailsList.Count == 0 ||
                Global.servicesClient == null || ListServices == null || ListServicesDetail == null || ListServicesType == null) return;

            int selectedIndex = TabServicesDetail.SelectedIndex;
            switch (selectedIndex)
            {
                case 0:
                    ShowServicesDetail();
                    break;
                case 1:
                    ShowNotes(0, NOTE_LOADING_COUNT, 1);
                    break;
                case 2:
                    ShowEventList(0, NOTE_LOADING_COUNT, 1);
                    break;
                case 3:
                    ShowTaskList(0, BILL_LOADLING_COUNT, 1);
                    break;
                case 4:
                    ShowDocumentList(true);
                    break;
            }
        }

        private void LoadServiceLists(int skip = 0)
        {
            switch (SelectedServicesListsType)
            {
                case "List":
                    GridServices.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
                    ShowServicesList(Constants.ServicesListURL, skip, NOTE_LOADING_COUNT);
                    break;
                case "Type":
                    ShowMainListView();
                    ShowServiceListsType(skip, NOTE_LOADING_COUNT);
                    break;
                case "Plans":
                    ShowMainListView();
                    ShowServiceListsPlans(skip, NOTE_LOADING_COUNT);
                    break;
                case "Changes":
                    ShowMainListView();
                    ShowServiceListsChanges(skip, NOTE_LOADING_COUNT);
                    break;
                case "Status":
                    ShowMainListView();
                    ShowServiceListsStatus(skip, NOTE_LOADING_COUNT);
                    break;
            }
        }
        
        private void LabelExpand_MouseDown(object sender, MouseButtonEventArgs e)
        {
            long id = Convert.ToInt64(((Label)sender).Tag);
            List<BillHistoryModel.BillDetail> list = (List<BillHistoryModel.BillDetail>)ListBillHistory.ItemsSource;
            int count = list.Count;

            for (int i = 0; i < count; i++)
            {
                if (Convert.ToInt64(list[i].Id) == id)
                {
                    if (list[i].Visiblity == "Visible")
                    {
                        list[i].Visiblity = "Collapsed";
                        MaterialDesignThemes.Wpf.PackIcon icon = new MaterialDesignThemes.Wpf.PackIcon();
                        icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ChevronRight;
                        list[i].ButtonIcon = icon;
                    } 
                    else
                    {
                        list[i].Visiblity = "Visible";
                        MaterialDesignThemes.Wpf.PackIcon icon = new MaterialDesignThemes.Wpf.PackIcon();
                        icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ChevronDown;
                        list[i].ButtonIcon = icon;
                    }
                    break;
                }
            }

            ListBillHistory.ItemsSource = new List<BillHistoryModel.BillDetail>();
            ListBillHistory.ItemsSource = list;
        }

        private async void Button_BillNow_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentContactCode == "") return;
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) MaterialDesignThemes.Wpf.DialogHost.Close("MyDialogHost");
            await MaterialDesignThemes.Wpf.DialogHost.Show(new BillNowDialog(), "MyDialogHost");
        }

        private void Button_Pagination_Click(object sender, RoutedEventArgs e)
        {
            if (TextGoToPage.Text == "" || ComboRowsPerPage == null || ListBillHistory.ItemsSource == null) return;

            Button button = (Button)sender;
            int page = Convert.ToInt16(TextGoToPage.Text);
            int count = Convert.ToInt16(((ComboBoxItem)ComboRowsPerPage.SelectedItem).Content.ToString());
            int last = BillsTotalCount / count + 1;

            switch (button.Name)
            {
                case "ButtonBillFirst":
                    page = 1;
                    break;
                case "ButtonBillPrev":
                    page = page > 1 ? page - 1 : 1;
                    break;
                case "ButtonBillNext":
                    page = page < last ? page + 1 : last;
                    break;
                case "ButtonBillLast":
                    page = last;
                    break;
                default:
                    return;
            }

            TextGoToPage.Text = page + "";
        }

        private void Text_Pagination_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextGoToPage.Text == "" || ComboRowsPerPage == null || ListBillHistory.ItemsSource == null) return;

            int page = Convert.ToInt16(TextGoToPage.Text);
            int count = Convert.ToInt16(((ComboBoxItem)ComboRowsPerPage.SelectedItem).Content.ToString());
            ShowBillHistories((page - 1) * count, count);

            int last = BillsTotalCount / count + 1;
            if (page == 1)
            {
                ButtonBillFirst.IsEnabled = false;
                ButtonBillPrev.IsEnabled = false;
                ButtonBillNext.IsEnabled = true;
                ButtonBillLast.IsEnabled = true;
            }
            else if (page == last)
            {
                ButtonBillFirst.IsEnabled = true;
                ButtonBillPrev.IsEnabled = true;
                ButtonBillNext.IsEnabled = false;
                ButtonBillLast.IsEnabled = false;
            }
            else
            {
                ButtonBillFirst.IsEnabled = true;
                ButtonBillPrev.IsEnabled = true;
                ButtonBillNext.IsEnabled = true;
                ButtonBillLast.IsEnabled = true;
            }
        }

        private void Text_Pagination_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void ComboRowsPerPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TextGoToPage.Text == "" || ComboRowsPerPage == null || ListBillHistory.ItemsSource == null) return;

            int page = Convert.ToInt16(TextGoToPage.Text);
            int count = Convert.ToInt16(((ComboBoxItem)ComboRowsPerPage.SelectedItem).Content.ToString());
            ShowBillHistories((page - 1) * count, count);
        }

        private void Menu_Vertical_Scroll_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = e.RoutedEvent.Name == "Checked";
            Global.HasVerticalScroll = isChecked;
            ShowHideVerticalScroll(isChecked);
        }

        private void ShowHideVerticalScroll(bool isChecked)
        {
            ListView[] listViews = new ListView[] { ListDisplayDetails, ListContactNotes, ListServicesDetail, ListServices, ListServicesType,
                                                ListServicesNotes, ListFinancials, ListTransactions, ListCreditCard, ListBankTransfer, ListEvents,
                                                ListServicesStatus, ListServicesCost, ListServicesChanges, ListServicesPlans, ListTasks, ListContactTasks, 
                                                ListDocuments, ListServiceDocuments, ListServicesEvents, ListMessages };
            int count = listViews.Length;
            for (int i = 0; i < count; i++) if (listViews[i] == null) return;
            for (int i = 0; i < count; i++)
            {
                int childCount = VisualTreeHelper.GetChildrenCount(listViews[i]);
                if (childCount > 0)
                {
                    ScrollViewer sv = VisualTreeHelper.GetChild(listViews[i], 0) as ScrollViewer;
                    sv.VerticalScrollBarVisibility = isChecked ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
                }
            }

            ScrollViewer[] scrolls = new ScrollViewer[] { ScrollViewBills, ScrollViewPaymentMethod };
            count = scrolls.Length;

            for (int i = 0; i < count; i++) if (scrolls[i] == null) return;
            for (int i = 0; i < count; i++) scrolls[i].VerticalScrollBarVisibility = isChecked ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
        }

        private void ShowServicesList(string url, int skip, int take, string countRecords = "Y", bool isList = true, string serviceCode = "")
        {
            if (skip == 0) ListServices.ItemsSource = new List<ServicesResponse.ServicesModel>();
            if (CurrentContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            key.SetValue("ServiceType", SelectedServicesListsType);
            key.Close();

            string search = TextServiceSearch.Text;
            if (search == "") search = "**";
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> path = Global.BuildDictionary("contactCode", CurrentContactCode);
                if (!isList) path.Add("serviceCode", serviceCode);
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.1);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("countRecords", countRecords);
                query.Add("SearchString", search);
                ServicesResponse result = Global.RequestAPI<ServicesResponse>(url, Method.Get, Global.GetHeader(Global.CustomerToken), path, query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<ServicesResponse.ServicesModel> list = (List<ServicesResponse.ServicesModel>)ListServices.ItemsSource;
                    ListServices.ItemsSource = new List<ServicesResponse.ServicesModel>();

                    if (list == null) list = new List<ServicesResponse.ServicesModel>();
                    int count = result == null || result.Services == null ? 0 : result.Services.Count;

                    for (int i = 0; i < count; i++)
                    {
                        result.Services[i].Connected = StringUtils.ConvertDateTime(result.Services[i].Connected);
                        result.Services[i].ContractEnd = StringUtils.ConvertDateTime(result.Services[i].ContractEnd);
                        result.Services[i].ContractStart = StringUtils.ConvertDateTime(result.Services[i].ContractStart);
                        result.Services[i].Disconnected = StringUtils.ConvertDateTime(result.Services[i].Disconnected);
                        list.Add(result.Services[i]);
                    }

                    ListServices.Tag = count;
                    ListServices.ItemsSource = list;
                    LoadingPanel.Visibility = Visibility.Hidden;

                    if (list.Count > 0)
                    {
                        ListServices.SelectedIndex = 0;
                        if (!MenuServices.IsEnabled) LoadServiceDetails();
                    }
                });

            }, 10);

        }

        private void ShowServicesDetail()
        {
            ServicesResponse.ServicesModel services = (ServicesResponse.ServicesModel)ListServices.SelectedItem;
            if (services == null) return;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                List<DisplayDetails> result = Global.RequestAPI<List<DisplayDetails>>(Constants.ServicesDetailURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("serviceRef", services.ServiceReference), Global.BuildDictionary("api-version", 1.2), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    ListServicesDetail.ItemsSource = new List<DisplayDetails>();
                    ListServicesDetail.ItemsSource = result;
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private void Menu_Hide_Buttons_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = e.RoutedEvent.Name == "Checked";
            StackPanel[] panels = new StackPanel[] { PanelBillButtons, PanelTransactions, PanelCreditCard, PanelBankTransfer };
            foreach (StackPanel panel in panels) panel.Visibility = isChecked ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ListBillHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBillHistory == null || ListBillHistory.Items.Count == 0) return;

            bool isEmpty = ListBillHistory.SelectedItem == null;
            ButtonBillEmail.IsEnabled = !isEmpty;
            ButtonBillServices.IsEnabled = !isEmpty;
            ButtonBillTransactions.IsEnabled = !isEmpty;
            ButtonBillCharges.IsEnabled = !isEmpty;
            ButtonBillPdf.IsEnabled = !isEmpty;
            ButtonBillExcel.IsEnabled = !isEmpty;
            ButtonBillDelete.IsEnabled = !isEmpty;
            ButtonBillDisputes.IsEnabled = !isEmpty;
            
        }

        private void ShowMainListView()
        {
            string[] services = new string[] { "Type", "Plans", "Changes", "Status", "CostCenter", "ServiceGroup", "Sites" };
            ListView[] listViews = new ListView[] { ListServicesType, ListServicesPlans, ListServicesChanges, ListServicesStatus, ListServicesCost,
                                                ListServicesGroup, ListServicesSites};

            int length = services.Length;
            GridServices.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);

            for (int i = 0; i < length; i++)
            {
                if (services[i] == SelectedServicesListsType) listViews[i].Visibility = Visibility.Visible;
                else listViews[i].Visibility = Visibility.Collapsed;
            }

            ListServices.ItemsSource = new List<ServicesResponse.ServicesModel>();
            ListServicesDetail.ItemsSource = new List<ServicesType.ServicesModel>();
            ListServicesNotes.ItemsSource = new List<NoteResponse.NoteModel>();
            ListServicesEvents.ItemsSource = new List<EventResponse.EventModel>();
        }

        private void ShowServiceListsType(int skip, int take, string searchString = "**", string countRecords = "Y")
        {
            if (skip == 0) ListServicesType.ItemsSource = new List<ServicesType.ServicesModel>();
            if (CurrentContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            string search = TextServiceSearch.Text;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.1);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("countRecords", countRecords);
                query.Add("SearchString", searchString);
                ServicesType result = Global.RequestAPI<ServicesType>(Constants.ServicesTypeURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("contactCode", CurrentContactCode), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<ServicesType.ServicesModel> list = (List<ServicesType.ServicesModel>)ListServicesType.ItemsSource;
                    ListServicesType.ItemsSource = new List<ServicesType.ServicesModel>();

                    if (list == null) list = new List<ServicesType.ServicesModel>();
                    int count = result.ServiceTypeNodes.Count;

                    for (int i = 0; i < count; i++)
                    {
                        string statusCount = "";
                        int countStatus = result.ServiceTypeNodes[i].StatusCounts.Count;

                        for (int j = 0; j < countStatus; j++)
                        {
                            statusCount = string.Format("{0}, {1} : {2}", statusCount, result.ServiceTypeNodes[i].StatusCounts[j].Status,
                                result.ServiceTypeNodes[i].StatusCounts[j].Count);
                        }

                        result.ServiceTypeNodes[i].StatusCountsText = statusCount.Substring(2);
                        list.Add(result.ServiceTypeNodes[i]);
                    }

                    ListServicesType.Tag = count;
                    ListServicesType.ItemsSource = list;
                    if (list.Count > 0) ListServicesType.SelectedIndex = 0;
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private void ShowServiceListsPlans(int skip, int take, string searchString = "**", string countRecords = "Y")
        {
            if (skip == 0) ListServicesPlans.ItemsSource = new List<ServicesPlans.PlanModel>();
            if (CurrentContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            string search = TextServiceSearch.Text;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.1);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("countRecords", countRecords);
                query.Add("SearchString", searchString);
                ServicesPlans result = Global.RequestAPI<ServicesPlans>(Constants.ServicesPlansURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("contactCode", CurrentContactCode), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<ServicesPlans.PlanModel> list = (List<ServicesPlans.PlanModel>)ListServicesPlans.ItemsSource;
                    ListServicesPlans.ItemsSource = new List<ServicesPlans.PlanModel>();

                    if (list == null) list = new List<ServicesPlans.PlanModel>();
                    list.AddRange(result.PlanNodes);

                    ListServicesPlans.Tag = result.Count;
                    ListServicesPlans.ItemsSource = list;
                    if (list.Count > 0) ListServicesPlans.SelectedIndex = 0;
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

      
        private void ShowServiceListsChanges(int skip, int take, string searchString = "**", string countRecords = "Y")
        {
            if (skip == 0) ListServicesChanges.ItemsSource = new List<ServicesChanges>();
            if (CurrentContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            string search = TextServiceSearch.Text;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.1);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("countRecords", countRecords);
                query.Add("From", "1990-01-01");
                List<ServicesChanges> result = Global.RequestAPI<List<ServicesChanges>>(Constants.ServicesChangesURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("contactCode", CurrentContactCode), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<ServicesChanges> list = (List<ServicesChanges>)ListServicesChanges.ItemsSource;
                    ListServicesChanges.ItemsSource = new List<ServicesChanges>();

                    if (list == null) list = new List<ServicesChanges>();
                    foreach (ServicesChanges item in result)
                    {
                        item.From = StringUtils.ConvertDate(item.From);
                        list.Add(item);
                    }

                    ListServicesChanges.ItemsSource = list;
                    if (list.Count > 0) ListServicesChanges.SelectedIndex = 0;
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private void ShowServiceListsStatus(int skip, int take, string searchString = "**", string countRecords = "Y")
        {
            if (skip == 0) ListServicesStatus.ItemsSource = new List<ServicesStatus.StatusModel>();
            if (CurrentContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            string search = TextServiceSearch.Text;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.1);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("countRecords", countRecords);
                ServicesStatus result = Global.RequestAPI<ServicesStatus>(Constants.ServicesStatusURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("contactCode", Global.ContactCode), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<ServicesStatus.StatusModel> list = (List<ServicesStatus.StatusModel>)ListServicesStatus.ItemsSource;
                    ListServicesStatus.ItemsSource = new List<ServicesStatus.StatusModel>();

                    if (list == null) list = new List<ServicesStatus.StatusModel>();
                    list.AddRange(result.StatusNodes);

                    ListServicesStatus.Tag = result.Count;
                    ListServicesStatus.ItemsSource = list;
                    if (list.Count > 0) ListServicesStatus.SelectedIndex = 0;
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private void ShowServiceListsCostCenter(int skip, int take, string searchString = "**", string countRecords = "Y")
        {
            if (skip == 0) ListServicesCost.ItemsSource = new List<ServicesCost.CostCenters>();
            if (CurrentContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            string search = TextServiceSearch.Text;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.1);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("countRecords", countRecords);
                ServicesCost result = Global.RequestAPI<ServicesCost>(Constants.ServicesCostURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("contactCode", Global.ContactCode), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<ServicesCost.CostCenters> list = (List<ServicesCost.CostCenters>)ListServicesCost.ItemsSource;
                    ListServicesCost.ItemsSource = new List<ServicesCost.CostCenters>();

                    if (list == null) list = new List<ServicesCost.CostCenters>();
                    foreach (ServicesCost.CostCenters item in result.CostCenterNodes)
                    {
                        string statusCountText = "";
                        int count = item.StatusCounts.Count;
                        for (int i = 0; i < count; i++) statusCountText += item.StatusCounts[i].Status + " : " + item.StatusCounts[i].Count + ", ";
                        item.StatusCountsText = statusCountText == "" ? "" : statusCountText.Substring(0, statusCountText.Length - 2);
                        list.Add(item);
                    }

                    ListServicesCost.Tag = result.Count;
                    ListServicesCost.ItemsSource = list;
                    if (list.Count > 0) ListServicesCost.SelectedIndex = 0;
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private void ShowServiceListsGroup(int skip, int take, string searchString = "**", string countRecords = "Y")
        {
            if (skip == 0) ListServicesGroup.ItemsSource = new List<ServicesGroup.ServiceGroup>();
            if (CurrentContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            string search = TextServiceSearch.Text;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.1);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("countRecords", countRecords);
                ServicesGroup result = Global.RequestAPI<ServicesGroup>(Constants.ServicesGroupURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("contactCode", Global.ContactCode), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<ServicesGroup.ServiceGroup> list = (List<ServicesGroup.ServiceGroup>)ListServicesGroup.ItemsSource;
                    ListServicesGroup.ItemsSource = new List<ServicesGroup.ServiceGroup>();

                    if (list == null) list = new List<ServicesGroup.ServiceGroup>();
                    foreach (ServicesGroup.ServiceGroup item in result.ServiceGroupNodes)
                    {
                        string statusCountText = "";
                        int count = item.StatusCounts.Count;
                        for (int i = 0; i < count; i++) statusCountText += item.StatusCounts[i].Status + " : " + item.StatusCounts[i].Count + ", ";
                        item.StatusCountsText = statusCountText == "" ? "" : statusCountText.Substring(0, statusCountText.Length - 2);
                        list.Add(item);
                    }

                    ListServicesGroup.Tag = result.Count;
                    ListServicesGroup.ItemsSource = list;
                    if (list.Count > 0) ListServicesGroup.SelectedIndex = 0;
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private void ShowServiceListsSites(int skip, int take, string searchString = "**", string countRecords = "Y")
        {
            if (skip == 0) ListServicesSites.ItemsSource = new List<ServicesSites.SitesModel>();
            if (CurrentContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            string search = TextServiceSearch.Text;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.1);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("countRecords", countRecords);
                ServicesSites result = Global.RequestAPI<ServicesSites>(Constants.ServicesSitesURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("contactCode", Global.ContactCode), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<ServicesSites.SitesModel> list = (List<ServicesSites.SitesModel>)ListServicesSites.ItemsSource;
                    ListServicesSites.ItemsSource = new List<ServicesSites.SitesModel>();

                    if (list == null) list = new List<ServicesSites.SitesModel>();
                    list.AddRange(result.SiteNodes);

                    ListServicesSites.Tag = result.Count;
                    ListServicesSites.ItemsSource = list;
                    if (list.Count > 0) ListServicesSites.SelectedIndex = 0;
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private void ListServicesType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListServicesType == null || ListServicesType.ItemsSource == null || ListServicesType.Items.Count == 0 || ListServices == null) return;
            ServicesType.ServicesModel services = (ServicesType.ServicesModel)ListServicesType.SelectedItem;
            ShowServicesList(Constants.ServicesTypeDetailURL, 0, NOTE_LOADING_COUNT, "Y", false, services.ServiceTypeCode);
        }

        private void ListServicesPlans_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListServicesPlans == null || ListServicesPlans.ItemsSource == null || ListServicesPlans.Items.Count == 0 || ListServices == null) return;
            ServicesPlans.PlanModel services = (ServicesPlans.PlanModel)ListServicesPlans.SelectedItem;
            ShowServicesList(Constants.ServicesPlansDetailURL, 0, NOTE_LOADING_COUNT, "Y", false, services.Id + "");
        }

        private void ListServicesChanges_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListServicesChanges == null || ListServicesChanges.ItemsSource == null || ListServicesChanges.Items.Count == 0 || ListServices == null) return;
            ServicesChanges services = (ServicesChanges)ListServicesChanges.SelectedItem;
            ShowServicesList(Constants.ServicesChangesDetailURL, 0, NOTE_LOADING_COUNT, "Y", false, services.ChangeType);
        }

        private void ListServicesStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListServicesStatus == null || ListServicesStatus.ItemsSource == null || ListServicesStatus.Items.Count == 0 || ListServices == null) return;
            ServicesStatus.StatusModel services = (ServicesStatus.StatusModel)ListServicesStatus.SelectedItem;
            ShowServicesList(Constants.ServicesStatusDetailURL, 0, NOTE_LOADING_COUNT, "Y", false, services.Id);
        }

        private void ListServicesCost_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListServicesCost == null || ListServicesCost.ItemsSource == null || ListServicesCost.Items.Count == 0 || ListServices == null) return;
            ServicesCost.CostCenters services = (ServicesCost.CostCenters)ListServicesCost.SelectedItem;
            ShowServicesList(Constants.ServicesCostDetailURL, 0, NOTE_LOADING_COUNT, "Y", false, services.Id + "");
        }

        private void ListServicesGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListServicesGroup == null || ListServicesGroup.ItemsSource == null || ListServicesGroup.Items.Count == 0 || ListServices == null) return;
            ServicesGroup.ServiceGroup services = (ServicesGroup.ServiceGroup)ListServicesGroup.SelectedItem;
            ShowServicesList(Constants.ServicesGroupDetailURL, 0, NOTE_LOADING_COUNT, "Y", false, services.Id + "");
        }

        private void ListServicesSites_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListServicesSites == null || ListServicesSites.ItemsSource == null || ListServicesSites.Items.Count == 0 || ListServices == null) return;
            ServicesSites.SitesModel services = (ServicesSites.SitesModel)ListServicesSites.SelectedItem;
            ShowServicesList(Constants.ServicesSitesDetailURL, 0, NOTE_LOADING_COUNT, "Y", false, services.Id + "");
        }

        private void ListServices_Scroll(object sender, ScrollChangedEventArgs e)
        {
            ListView listView = (ListView)e.Source;
            int max = Convert.ToInt32(listView.Tag);

            int totalCount = listView.Items.Count;
            if (totalCount < 9 || totalCount >= max) return;

            if (e.VerticalOffset + e.VerticalChange >= totalCount - 9)
            {
                bool isList = true;
                string code = "";

                switch (SelectedServicesListsType)
                {
                    case "Type":
                        isList = false;
                        ServicesType.ServicesModel services1 = (ServicesType.ServicesModel)ListServicesType.SelectedItem;
                        code = services1.ServiceTypeCode;
                        break;
                    case "Plans":
                        isList = false;
                        ServicesPlans.PlanModel services2 = (ServicesPlans.PlanModel)ListServicesPlans.SelectedItem;
                        code = services2.Id + "";
                        break;
                    case "Changes":
                        isList = false;
                        ServicesChanges services3 = (ServicesChanges)ListServicesChanges.SelectedItem;
                        code = services3.ChangeType;
                        break;
                    case "Status":
                        isList = false;
                        ServicesStatus.StatusModel services4 = (ServicesStatus.StatusModel)ListServicesStatus.SelectedItem;
                        code = services4.Id;
                        break;
                    case "CostCenter":
                        isList = false;
                        ServicesCost.CostCenters services5 = (ServicesCost.CostCenters)ListServicesCost.SelectedItem;
                        code = services5.Id + "";
                        break;
                    case "ServiceGroup":
                        isList = false;
                        ServicesGroup.ServiceGroup services6 = (ServicesGroup.ServiceGroup)ListServicesGroup.SelectedItem;
                        code = services6.Id + "";
                        break;
                    case "Sites":
                        isList = false;
                        ServicesSites.SitesModel services7 = (ServicesSites.SitesModel)ListServicesGroup.SelectedItem;
                        code = services7.Id + "";
                        break;
                }

                ShowServicesList(Constants.ServicesListURL, totalCount, NOTE_LOADING_COUNT, "Y", isList, code);
            }
        }

        private void ScrollViewBills_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            int totalCount = ListBillHistory.Items.Count;
            int limit = Convert.ToInt32(ListBillHistory.Tag);
            double totalHeight = ((ScrollViewer)sender).ExtentHeight;
            double fixedHeigth = ((ScrollViewer)sender).ActualHeight;
            if (totalCount < 9 || totalCount >= limit) return;
            if (e.VerticalOffset + e.VerticalChange + fixedHeigth >= totalHeight)
            {
                ShowBillHistories(totalCount, BILL_LOADLING_COUNT);
            }
        }

        private void ListFinancials_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListFinancials == null || ListFinancials.ItemsSource == null || ListFinancials.Items.Count == 0) return;
        }

        private void ListTransactions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListTransactions == null || ListTransactions.ItemsSource == null || ListTransactions.Items.Count == 0) return;

            ButtonTransDetail.IsEnabled = true;
        }

        private async void Menu_Change_Language(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            if (item == null || item.Tag == null) return;

            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            key.SetValue("lang", item.Tag.ToString());
            key.Close();

            bool result = await MessageUtils.ConfirmMessageAsync(Properties.Resources.Alert, Properties.Resources.Restart_Application);
            if (result)
            {
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

        private void Button_Delete_PaymentMethod_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            long paymentMethodId = Convert.ToInt64(button.Tag);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> body = Global.BuildDictionary("Status", "CL");
                body.Add("Note", string.Format("Paymenth method deleted by {0} from ServiceDesk", CurrentContactCode));
                HttpStatusCode result = Global.RequestAPI(Constants.DeleteCardURL, Method.Post, Global.GetHeader(Global.CustomerToken), 
                    Global.BuildDictionary("PayMethodId", paymentMethodId), Global.BuildDictionary("api-version", 1.1), body, false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result == HttpStatusCode.OK)
                    {
                        bool isFound = false;
                        List<PaymentMethod> list = (List<PaymentMethod>)ListCreditCard.ItemsSource;
                        int count = list.Count;

                        for (int i = 0; i < count; i++)
                        {
                            if (Convert.ToInt64(list[i].Id) == paymentMethodId)
                            {
                                list.RemoveAt(i);
                                isFound = true;
                                break;
                            }
                        }

                        if (isFound)
                        {
                            ListCreditCard.ItemsSource = new List<PaymentMethod>();
                            ListCreditCard.ItemsSource = list;
                            LoadingPanel.Visibility = Visibility.Hidden;
                            return;
                        }

                        list = (List<PaymentMethod>)ListBankTransfer.ItemsSource;
                        count = list.Count;

                        for (int i = 0; i < count; i++)
                        {
                            if (Convert.ToInt64(list[i].Id) == paymentMethodId)
                            {
                                list.RemoveAt(i);
                                isFound = true;
                                break;
                            }
                        }

                        if (isFound)
                        {
                            ListCreditCard.ItemsSource = new List<PaymentMethod>();
                            ListBankTransfer.ItemsSource = list;
                        }
                    }
                    else
                    {
                        MessageUtils.ShowErrorMessage("", "Payment method delete failed - " + result.ToString());
                    }

                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);

        }

        private void ToggleButton_Default_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = e.RoutedEvent.Name == "Checked";
            long paymentMethodId = Convert.ToInt64(((ToggleButton)sender).Tag);

            List<PaymentMethod> list = (List<PaymentMethod>)ListCreditCard.ItemsSource;
            int count = list.Count;
            int selectedIndex = -1;

            for (int i = 0; i < count; i++)
            {
                if (Convert.ToInt64(list[i].Id) == paymentMethodId)
                {
                    selectedIndex = i;
                    break;
                }
            }

            if (selectedIndex == -1)
            {
                list = (List<PaymentMethod>)ListBankTransfer.ItemsSource;
                count = list.Count;

                for (int i = 0; i < count; i++)
                {
                    if (Convert.ToInt64(list[i].Id) == paymentMethodId)
                    {
                        selectedIndex = i;
                        break;
                    }
                }
            }

            if (selectedIndex == -1)
            {
                MessageUtils.ShowErrorMessage("", "Can't found payment method Id");
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.MakeDetaultURL, Method.Post, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("PayMethodId", paymentMethodId),
                    Global.BuildDictionary("api-version", 1.1), Global.BuildDictionary("Status", list[selectedIndex].StatusCode), false);

                Application.Current.Dispatcher.Invoke(delegate
                {

                    if (result == HttpStatusCode.OK)
                    {
                        list = (List<PaymentMethod>)ListCreditCard.ItemsSource;
                        ListBankTransfer.ItemsSource = new List<PaymentMethod>();
                        count = list.Count;

                        for (int i = 0; i < count; i++)
                        {
                            list[i].Default = Convert.ToInt64(list[i].Id) == paymentMethodId;
                            list[i].Enabled = Convert.ToInt64(list[i].Id) != paymentMethodId;
                            list[i].DefaultText = Convert.ToInt64(list[i].Id) == paymentMethodId ? "Default Payment Method" : "Make Default";
                        }

                        ListCreditCard.ItemsSource = new List<PaymentMethod>();
                        ListCreditCard.ItemsSource = list;

                        list = (List<PaymentMethod>)ListBankTransfer.ItemsSource;
                        count = list.Count;

                        for (int i = 0; i < count; i++)
                        {
                            list[i].Default = Convert.ToInt64(list[i].Id) == paymentMethodId;
                            list[i].Enabled = Convert.ToInt64(list[i].Id) != paymentMethodId;
                            list[i].DefaultText = Convert.ToInt64(list[i].Id) == paymentMethodId ? "Default Payment Method" : "Make Default";
                        }

                        ListBankTransfer.ItemsSource = list;
                    }
                    else
                    {
                        MessageUtils.ShowErrorMessage("", "Payment method can't make to default - " + result.ToString());
                    }

                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);

        }

        private async void Button_CreditCard_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentContactCode == "") return;
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) MaterialDesignThemes.Wpf.DialogHost.Close("MyDialogHost");
            await MaterialDesignThemes.Wpf.DialogHost.Show(new CreditCard(CurrentContactCode), "MyDialogHost");
        }

        private async void Button_Bank_Add_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentContactCode == "") return;

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) MaterialDesignThemes.Wpf.DialogHost.Close("MyDialogHost");
            await MaterialDesignThemes.Wpf.DialogHost.Show(new BankAccount(CurrentContactCode), "MyDialogHost");
        }

        private void Menu_Click_Event(object sender, RoutedEventArgs e)
        {
            MenuItem menu = (MenuItem)sender;
            if (menu == null || menu.Tag == null)
            {
                MessageUtils.ShowErrorMessage("", "Invalid Page, please contact your Administrator");
                return;
            }

            Dictionary<string, string> tag = (Dictionary<string, string>)menu.Tag;
            if (tag["type"] == "0") RedirectAccountLink(tag["link"]);
            else if (tag["type"] == "1") RedirectServiceLink(tag["link"]);
        }

        private async void RedirectAccountLink(string link)
        {
            string dialog = "MyDialogHost";
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen(dialog)) Global.CloseDialog();

            switch (link)
            {
                case "/Contacts/Notes":
                    TabAccountInfo.SelectedIndex = 0;
                    break;
                case "/Contacts/Names":
                    if (AccountType == "") return;
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new NameDialog(AccountType), dialog);
                    break;
                case "/Contacts/Addresses":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new AddressDialog(), dialog);
                    break;
                case "/Contacts/ContactPhones":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new PhoneDialog(), dialog);
                    break;
                case "/Contacts/Emails":
                    break;
                case "/Contacts/PaymentMethods":
                    TabAccountInfo.SelectedIndex = 6;
                    break;
                case "/Contacts/UserDefinedData":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new UserDefinedDialog(), dialog);
                    break;
                case "/Contacts/RelatedContacts":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new RelatedContactsList(), dialog);
                    break;
                case "/Contacts/RelatedContacts/New":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new RelatedContactDialog("", true), dialog);
                    break;
                case "/Messages/Contacts":
                    TabAccountInfo.SelectedIndex = 2;
                    break;
                case "/Accounts/BillOptions":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new BillOptions(), dialog);
                    break;
                case "BillHistory":
                    TabAccountInfo.SelectedIndex = 1;
                    break;
                case "/Bills/Delegations/Accounts":
                    break;
                case "/Bills/Disputes/Accounts":
                    break;
                case "/FinancialTransactions/Transactions":
                    TabAccountInfo.SelectedIndex = 5;
                    break;
                case "/FinancialTransactions/Receipts/New":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new ReceiptDialog(), dialog);
                    break;
                case "/FinancialTransactions/Invoices/New":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new InvoiceDialog(), dialog);
                    break;
                case "/FinancialTransactions/CreditNotes/New":
                    break;
                case "/FinancialTransactions/CreditAdjustments/New":
                    break;
                case "/FinancialTransactions/DebitAdjustments/New":
                    break;
                case "/FinancialTransactions/Allocate":
                    break;
                case "/Charges/AccountCharges":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new dialogs.charge.ChargeHistoryDialog(), dialog);
                    break;
                case "/Charges/AccountCharges/New":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new AccountChargeDialog(), dialog);
                    break;
                case "/Charges/AccountOverrides":
                    break;
                case "/Charges/AccountOverrides/New":
                    break;
                case "/Accounts/Plans/PlanHistory":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new PlanHistory(), dialog);
                    break;
                case "/Accounts/Plans/PlanChange":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new PlanChange(), dialog);
                    break;
                case "/Tasks/Services":
                    TabServicesDetail.SelectedIndex = 3;
                    break;
                case "/Tasks/Services/New":
                    break;
                case "/Events/AccountEventHistory":
                    TabAccountInfo.SelectedIndex = 3;
                    break;
                case "/Events/Accounts/Events/New":
                    break;
                case "/Tasks/Contacts":
                    TabAccountInfo.SelectedIndex = 8;
                    break;
                case "/Tasks/Contacts/New":
                    break;
                case "/Accounts/Usage/UsageHistory":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new UsagesDialog(), dialog);
                    break;
                case "/Accounts/Documents":
                    TabAccountInfo.SelectedIndex = 8;
                    break;
                case "/Services/New":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new NewServiceDialog(), dialog);
                    break;
                case "/Users/Authentication/Account":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new AuthenticationDialog(), dialog);
                    break;
                case "/Contacts/Identifications":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new IdentificationDialog(), dialog);
                    break;
                case "/Contacts/QuestionAndAnswer/Account":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new QADialog(), dialog);
                    break;
                case "/Contacts/EnquiryPassword":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new dialogs.identification.EnquiryPassword(), dialog);
                    break;
                case "/Accounts/CostCenters":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new CostCenterListDialog(CurrentContactCode), dialog);
                    break;
            }
        }

        private async void RedirectServiceLink(string link)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            ServicesResponse.ServicesModel services = (ServicesResponse.ServicesModel)ListServices.SelectedItem;
            string reference = services.ServiceReference + "";
            string serviceId = services.ServiceId + "";

            switch (link)
            {
                case "/Services/New":
                    break;
                case "/Charges/ServiceCharges":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new ChargeHistoryDialog(true, reference), "MyDialogHost");
                    break;
                case "/Charges/ServiceCharges/New":
                    break;
                case "/Charges/ServiceOverrides":
                    break;
                case "/Charges/ServiceOverrides/New":
                    break;
                case "/Services/Attributes":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new ServiceAttributes(reference), "MyDialogHost");
                    break;
                case "/Services/Notes":
                    TabServicesDetail.SelectedIndex = 1;
                    break;
                case "/Services/Documents":
                    TabServicesDetail.SelectedIndex = 4;
                    break;
                case "/Services/Discounts":
                    break;
                case "/Services/Usage/UsageHistory":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new UsagesDialog(true, reference), "MyDialogHost");
                    break;
                case "/Services/Plans/PlanHistory":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new PlanHistory(true, reference), "MyDialogHost");
                    break;
                case "/Services/Plans/PlanChange":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new PlanChange(true, reference), "MyDialogHost");
                    break;
                case "/Events/ServiceEventHistory":
                    TabServicesDetail.SelectedIndex = 2;
                    break;
                case "/Events/Services/Events/New":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new EventDialog(reference, serviceId), "MyDialogHost");
                    break;
                case "/Services/Terminations/Terminate":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new TerminationDialog(reference), "MyDialogHost");
                    break;
                case "/Services/EnquiryPassword":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new dialogs.services.EnquiryPassword(reference), "MyDialogHost");
                    break;
            }
        }

        private async void ButtonBill_Click(object sender, RoutedEventArgs e)
        {
            BillHistoryModel.BillDetail item = (BillHistoryModel.BillDetail)ListBillHistory.SelectedItem;
            if (item == null || item.Id == 0) return;

            Button button = (Button)sender;
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();

            switch (button.Name)
            {
                case "ButtonBillEmail":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new BillEmailDialog(item.Id, item.BillNumber), "MyDialogHost");
                    break;
                case "ButtonBillServices":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new BillServicesDialog(item.Id, item.BillNumber), "MyDialogHost");
                    break;
                case "ButtonBillTransactions":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new BillTransactions(item.Id, item.BillNumber), "MyDialogHost");
                    break;
                case "ButtonBillCharges":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new BillChargeDialog(item.Id, item.BillNumber), "MyDialogHost");
                    break;
                case "ButtonBillDisputes":
                    await MaterialDesignThemes.Wpf.DialogHost.Show(new BillDisputeDialog(item.Id, item.BillNumber), "MyDialogHost");
                    break;
            }
        }

        private void BillDownload_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string name = button.Name;
            string url = name == "ButtonBillPdf" ? Constants.BillPDFURL : Constants.BillExcelURL;

            LoadingPanel.Visibility = Visibility.Visible;
            BillHistoryModel.BillDetail item = (BillHistoryModel.BillDetail)ListBillHistory.SelectedItem;
            if (item == null || item.Id == 0) return;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> result = Global.RequestAPI<Dictionary<string, object>>(url, Method.Get, Global.GetHeader(Global.CustomerToken), 
                    Global.BuildDictionary("BillId", item.Id), Global.BuildDictionary("api-version", 1.0), "", false);

                string fileType = result["FileType"].ToString();
                string fileExt = result["FileType"].ToString() == "PDF" ? "pdf" : "xls";

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result["Content"] == null) {
                        MessageUtils.ShowErrorMessage("", "Bill is not available for download.");
                        return;
                    }

                    try
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.Filter = string.Format("{0} File|*.{1}", fileType, fileExt);
                        saveFileDialog.Title = string.Format("Save As {0} File", fileType);
                        saveFileDialog.FileName = string.Format("Bill # {0}.{1}", result["BillNumber"], fileExt);
                        
                        if (saveFileDialog.ShowDialog().Value)
                        {
                            File.WriteAllBytes(saveFileDialog.FileName, Convert.FromBase64String(result["Content"].ToString()));
                            MessageUtils.ShowMessage("", string.Format("{0} file downloaded successfully", fileType));
                        }

                    }
                    catch { MessageUtils.ShowErrorMessage("", "Bill is not available for download."); }
                });

            }, 10);

        }

        private void ButtonBillDelete_Click(object sender, RoutedEventArgs e)
        {
            LoadingPanel.Visibility = Visibility.Visible;
            BillHistoryModel.BillDetail item = (BillHistoryModel.BillDetail)ListBillHistory.SelectedItem;
            if (item == null || item.Id == 0) return;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> result = Global.RequestAPI<Dictionary<string, object>>(Constants.BillDeleteURL, Method.Delete, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("BillId", item.Id), null, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private void TextSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DisplayDetailsList == null || DisplayDetailsList.Count == 0) return;

            TextBox textBox = (TextBox)sender;
            switch (textBox.Name)
            {
                case "TextTaskSearch":
                    ShowTaskList(0, BILL_LOADLING_COUNT);
                    break;
                case "TextEventSearch":
                    ShowEventList(0, BILL_LOADLING_COUNT, 0);
                    break;
                case "TextServicesEventSearch":
                    ShowEventList(0, BILL_LOADLING_COUNT, 1);
                    break;
                case "TextNoteSearch":
                    ShowNotes(0, NOTE_LOADING_COUNT, 0);
                    break;
                case "TextServicesNoteSearch":
                    ShowNotes(0, NOTE_LOADING_COUNT, 1);
                    break;
            }
        }

        private void ListEvents_Scroll(object sender, ScrollChangedEventArgs e)
        {
            ListView listView = (ListView)e.Source;
            string name = listView.Name;
            int totalCount = listView.Items.Count;
            int limit = Convert.ToInt32(listView.Tag);
            if (totalCount == 0 || totalCount >= limit) return;

            if (e.VerticalOffset + e.VerticalChange >= totalCount - 6)
            {
                switch (name)
                {
                    case "ListTransactions":
                        ShowTransactions(totalCount, BILL_LOADLING_COUNT);
                        break;
                    case "ListEvents":
                        ShowEventList(totalCount, BILL_LOADLING_COUNT, 0);
                        break;
                    case "ListServicesEvents":
                        ShowEventList(totalCount, BILL_LOADLING_COUNT, 1);
                        break;
                    case "ListContactTasks":
                        ShowTaskList(totalCount, BILL_LOADLING_COUNT, 0);
                        break;
                    case "ListTasks":
                        ShowTaskList(totalCount, BILL_LOADLING_COUNT, 1);
                        break;
                    case "ListContactNotes":
                        ShowNotes(totalCount, NOTE_LOADING_COUNT, 0);
                        break;
                    case "ListServicesNotes":
                        ShowNotes(totalCount, NOTE_LOADING_COUNT, 1);
                        break;
                    case "ListFinancials":
                        ShowFinancials(totalCount, BILL_LOADLING_COUNT);
                        break;
                    case "ListServices":
                        ShowServicesList(Constants.ServicesListURL, totalCount, NOTE_LOADING_COUNT);
                        break;
                }
            }
        }

        private async void ButtonTransDetail_Click(object sender, RoutedEventArgs e)
        {
            if (ListTransactions.Items == null || ListTransactions.Items.Count == 0 || ListTransactions.SelectedItem == null) return;

            Transactions.TransactionDetail item = (Transactions.TransactionDetail)ListTransactions.SelectedItem;

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new TransactionDetail(item.Id), "MyDialogHost");
        }

        private async void Menu_Report_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ReportDialog(), "MyDialogHost");
        }

        private void ButtonDocDownload_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string code = button.Tag.ToString();
            string url = Constants.DocumentDownloadURL;

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> path = Global.BuildDictionary("DocumentId", code);
                DocumentModel.FullDetail result = Global.RequestAPI<DocumentModel.FullDetail>(url, Method.Get, Global.GetHeader(Global.CustomerToken), path, 
                    Global.BuildDictionary("api-version", 2.0), "", false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null || result.Content == null)
                    {
                        MessageUtils.ShowErrorMessage("", "Document is not available for download.");
                        return;
                    }

                    try
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.Filter = result.FileType + " files|*." + result.FileType;
                        saveFileDialog.Title = string.Format("Save As a File");
                        saveFileDialog.FileName = result.FileType == "" ? result.Name : result.Name + "." + result.FileType;

                        if (saveFileDialog.ShowDialog().Value)
                        {
                            File.WriteAllBytes(saveFileDialog.FileName, Convert.FromBase64String(result.Content));
                            MessageUtils.ShowMessage("", "Document downloaded successfully");
                        }
                    }
                    catch { MessageUtils.ShowErrorMessage("", "Document is not available for download."); }
                });

            }, 10);
        }

        private void ButtonServiceDocDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string code = button.Tag.ToString();
            DeleteDocument(code, true);
        }

        private void ButtonAccountDocDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string code = button.Tag.ToString();
            DeleteDocument(code, false);
        }

        private void DeleteDocument(string code, bool isService = false)
        {
            string url = Constants.DocumentDeleteURL;

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> path = Global.BuildDictionary("DocumentId", code);
                HttpStatusCode result = Global.RequestAPI(url, Method.Delete, Global.GetHeader(Global.CustomerToken), path,
                    Global.BuildDictionary("api-version", 2.0), null);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK)
                    {
                        ShowDocumentList(isService);
                    }
                    else
                    {
                        MessageUtils.ShowErrorMessage("", "Document delete failed.");
                    }
                });

            }, 10);
        }

        private void ButtonDocUpload_Click(object sender, RoutedEventArgs e)
        {
            bool isService = ((Button)sender).Tag.ToString() == "Service";
            string dialog = "MyDialogHost";
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen(dialog)) Global.CloseDialog();
            ServicesResponse.ServicesModel services = (ServicesResponse.ServicesModel)ListServices.SelectedItem;

            MaterialDesignThemes.Wpf.DialogHost.Show(new NewDocumentDialog(isService, CurrentContactCode, services.ServiceReference), dialog);
        }

        private async void ButtonTransReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ReceiptDialog(), "MyDialogHost");
        }

        private void Menu_Services_List(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            if (ListServices == null) return;
            SelectedServicesListsType = menuItem.Tag.ToString();
            GridServices.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
            ShowServicesList(Constants.ServicesListURL, 0, NOTE_LOADING_COUNT);
        }

        private void Menu_Services_Type(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            if (ListServices == null) return;
            SelectedServicesListsType = menuItem.Tag.ToString();
            ShowMainListView();

            switch (SelectedServicesListsType)
            {
                case "Type":
                    if (ListServicesType != null) ShowServiceListsType(0, NOTE_LOADING_COUNT);
                    break;
                case "Plans":
                    if (ListServicesPlans != null) ShowServiceListsPlans(0, NOTE_LOADING_COUNT);
                    break;
                case "Changes":
                    if (ListServicesChanges != null) ShowServiceListsChanges(0, NOTE_LOADING_COUNT);
                    break;
                case "Status":
                    if (ListServicesStatus != null) ShowServiceListsStatus(0, NOTE_LOADING_COUNT);
                    break;
                case "CostCenter":
                    if (ListServicesCost != null) ShowServiceListsCostCenter(0, NOTE_LOADING_COUNT);
                    break;
                case "ServiceGroup":
                    if (ListServicesGroup != null) ShowServiceListsGroup(0, NOTE_LOADING_COUNT);
                    break;
                case "Sites":
                    if (ListServicesSites != null) ShowServiceListsSites(0, NOTE_LOADING_COUNT);
                    break;
            }
        }

        private async void Menu_Verify_Security(object sender, RoutedEventArgs e)
        {
            if (Global.ContactCode == "") return;

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new VerifySecurity(), "MyDialogHost"); 
        }

        private async void Menu_Login_History(object sender, RoutedEventArgs e)
        {
            if (Global.ContactId == "") return;

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new LoginHistory(), "MyDialogHost");
        }

        private async void Menu_Password_Expiry(object sender, RoutedEventArgs e)
        {
            if (Global.ContactId == "") return;

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new PasswordExpiry(), "MyDialogHost");
        }

        private async void Menu_Register_Details(object sender, RoutedEventArgs e)
        {
            if (Global.ContactId == "") return;

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new RegisterSecurity(), "MyDialogHost");
        }

        private async void Menu_Account_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new NewAccountDialog(), "MyDialogHost");
        }

        private async void Menu_Order_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new OrderDialog(), "MyDialogHost");
        }
        private async void ButtonSMS_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new SMSDialog(), "MyDialogHost");
        }

        private async void ButtonEmail_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new EmailDialog(), "MyDialogHost");
        }

        private async void ButtonAddTask_Click(object sender, RoutedEventArgs e)
        {
            string tag = ((Button)sender).Tag.ToString();
            bool isService = tag == "Service";

            if (isService && ListServices.SelectedItem == null) return;
            ServicesResponse.ServicesModel services = (ServicesResponse.ServicesModel)ListServices.SelectedItem;
            string code = isService ? services.ServiceReference + "" : Global.ContactCode;

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new TaskDialog("", isService, code), "MyDialogHost");
        }

        private async void ButtonEditTask_Click(object sender, RoutedEventArgs e)
        {
            string tag = ((Button)sender).Tag.ToString();
            bool isService = tag == "Service";

            if ((!isService && ListContactTasks.SelectedItem == null) || (isService && ListTasks.SelectedItem == null)) return;
            TaskModel.Item item = isService ? (TaskModel.Item)ListTasks.SelectedItem : (TaskModel.Item)ListContactTasks.SelectedItem;

            if (isService && ListServices.SelectedItem == null) return;
            ServicesResponse.ServicesModel services = (ServicesResponse.ServicesModel)ListServices.SelectedItem;
            string code = isService ? services.ServiceReference + "" : Global.ContactCode;

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new TaskDialog(JsonConvert.SerializeObject(item), isService, code), "MyDialogHost");
        }

        private async void ListItemTask_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListContactTasks.SelectedItem == null) return;
            TaskModel.Item item = (TaskModel.Item)ListContactTasks.SelectedItem;
            
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new TaskDialog(JsonConvert.SerializeObject(item), false, Global.ContactCode), "MyDialogHost");
        }

        private async void Menu_Service_Provider_Users_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ServiceProvidersDialog(), "MyDialogHost");
        }

        private void PackIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MaterialDesignThemes.Wpf.PackIcon packIcon = sender as MaterialDesignThemes.Wpf.PackIcon;
            string account = packIcon.Tag.ToString();

            List<string> list = (List<string>)ComboAccountNo.ItemsSource;
            list.Remove(account);
            AccountList.Remove(account);
            ComboAccountNo.ItemsSource = new List<string>();
            ComboAccountNo.ItemsSource = list;

            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            key.SetValue("account", JsonConvert.SerializeObject(list));
            key.Close();
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Label label = sender as Label;
                TextAccountNo.Text = label.Content.ToString().Split('-')[0];
                GetDisplayDetails();
                GridAccountNo.Visibility = Visibility.Collapsed;
            }catch{}
        }

        private void ListServices_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ListServices == null || ListServices.ItemsSource == null || ListServices.Items.Count == 0 || ListServicesDetail == null) return;
            if (ServiceMenuList == null) return;

            FrameworkElement fe = e.Source as FrameworkElement;
            ContextMenu cm = fe.ContextMenu ?? new ContextMenu();

            List<MenuModel.MenuItem> menuItems = ServiceMenuList;
            int count = menuItems.Count;
            for (int i = 0; i < count; i++)
            {
                MenuModel.MenuItem content = menuItems[i];
                MenuItem item = new MenuItem();
                item.Header = content.Caption;
                Dictionary<string, string> tag = new Dictionary<string, string>();
                tag.Add("link", content.NavigationURL);
                tag.Add("type", "1");
                item.Tag = tag;
                if (content.Tooltip != "tooltip") item.ToolTip = content.Tooltip;
                if (content.MenuItems.Count > 0) RenderContextMenuItem(item, content.MenuItems);
                else item.Click += Menu_Click_Event;
                cm.Items.Add(item);
            }

            cm.IsOpen = true;
            cm.Focus();
        }

        private void RenderContextMenuItem(MenuItem menu, List<MenuModel.MenuItem> list)
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                MenuModel.MenuItem content = list[i];
                MenuItem item = new MenuItem();
                item.Header = content.Caption;
                Dictionary<string, string> tag = new Dictionary<string, string>();
                tag.Add("link", content.NavigationURL);
                tag.Add("type", "1");
                item.Tag = tag;
                if (content.Tooltip != "tooltip") item.ToolTip = content.Tooltip;
                if (content.MenuItems.Count > 0) RenderContextMenuItem(item, content.MenuItems);
                else item.Click += Menu_Click_Event;
                menu.Items.Add(item);
            }

        }

        private void ListServices_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LoadServiceDetails();
        }

        private void LoadServiceDetails()
        {
            if (ListServices == null || ListServices.ItemsSource == null || ListServices.Items.Count == 0 || ListServicesDetail == null) return;
            ServicesResponse.ServicesModel services = (ServicesResponse.ServicesModel)ListServices.SelectedItem;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                MenuModel result = Global.RequestAPI<MenuModel>(Constants.MenuSerivcesURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("ServiceReference", services.ServiceReference), Global.BuildDictionary("api-version", 1.0), "", false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null || result.MenuItems == null || result.MenuItems.Count == 0)
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources1.Menu_List_Error);
                        return;
                    }

                    MenuServices.IsEnabled = true;
                    MenuServices.Items.Clear();
                    List<MenuModel.MenuItem> menuList = result.MenuItems;
                    ServiceMenuList = menuList;
                    RenderMenuItem(MenuServices, menuList, true);
                    LoadTabInfoDetail();
                });

            }, 10);
        }

        private async void ButtonAddEvent_Click(object sender, RoutedEventArgs e)
        {
            ServicesResponse.ServicesModel services = (ServicesResponse.ServicesModel)ListServices.SelectedItem;
            string reference = services.ServiceReference + "";
            string serviceId = services.ServiceId + "";

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new EventDialog(reference, serviceId), "MyDialogHost");
        }

        private void ButtonAddCostCenter_Click(object sender, RoutedEventArgs e)
        {
            string dialog = "MyDialogHost";
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen(dialog)) Global.CloseDialog();
            ServicesResponse.ServicesModel services = (ServicesResponse.ServicesModel)ListServices.SelectedItem;

            MaterialDesignThemes.Wpf.DialogHost.Show(new NewCostCenterDialog(CurrentContactCode), dialog);
        }
    }
}
