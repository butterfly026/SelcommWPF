using MaterialDesignColors;
using Microsoft.Win32;
using Newtonsoft.Json;
using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.bills;
using SelcommWPF.clients.models.contacts;
using SelcommWPF.clients.models.messages;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
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
using static SelcommWPF.clients.models.contacts.AddressModel;

namespace SelcommWPF.dialogs.accounts
{
    /// <summary>
    /// Interaction logic for NewAccount.xaml
    /// </summary>
    public partial class NewAccountDialog : UserControl
    {
        public static NewAccountDialog Instance;

        private List<string> TitlesList = new List<string>();
        private List<AddressModel.AddressType> AddressType = new List<AddressModel.AddressType>();
        private List<string> AddressTypeList = new List<string>();
        private List<string> AddressTypeCodeList = new List<string>();
        private List<string> CountriesStringList = new List<string>();
        private List<string> CountriesCodeList = new List<string>();
        private List<AddressModel.Country> CountriesList = new List<AddressModel.Country>();
        private List<PhoneModel.PhoneType> PhoneType = new List<PhoneModel.PhoneType>();
        private List<string> PhoneTypeList = new List<string>();
        private List<RelatedContactModel.Relationship> RelationShipList = new List<RelatedContactModel.Relationship>();
        private List<string> RelationTypeList = new List<string>();
        private IDisposable RequestTimer = null;
        private bool SelectedAddressStatus = false;
        private List<UserModel.BusinessUnitModel> BusinessUnitsList;

        private string CheckString = "";
        private string[] FormatTypeList = new string[] { Properties.Resources.Search, Properties.Resources.Manual, Properties.Resources.Extended };

        private List<BillCycle> BillingCycleList = new List<BillCycle>();
        private List<RelatedContactModel.TimeZone> CurrentTimeZoneList = new List<RelatedContactModel.TimeZone>();
        private RelatedContactModel.TimeZone SelectedTimeZone = new RelatedContactModel.TimeZone();
        private List<BillFormat> CurrenciesList = new List<BillFormat>();
        private List<BillTax> TaxRatesList = new List<BillTax>();
        private List<BillTerms.Item> TermsList = new List<BillTerms.Item>();

        private List<UserDefined> UserDefinedType = new List<UserDefined>();

        private bool IsLoadingUI = false;
        private WrapPanel PanelFileList = new WrapPanel();
        private ComplexResponse ComplexResult = new ComplexResponse();
        private List<Identification> IdentificationType = new List<Identification>();
        private Dictionary<string, object> IdentificationRules = new Dictionary<string, object>();

        public NewAccountDialog()
        {
            InitializeComponent();
            InitializeControl();
            Instance = this;
        }

        private void InitializeControl()
        {
            Instance = this;
            ShowHideVerticalScroll(Global.HasVerticalScroll);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("Resource", "/ServiceDesk/Accounts/New");
                query.Add("api-version", 1.2);
                HttpStatusCode result = Global.RequestAPI(Constants.AuthrisationURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, null);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result == HttpStatusCode.OK)
                    {
                        ComboAccountType.ItemsSource = new string[] { "Corporate", "Person" };
                        ShowHideVerticalScroll(Global.HasVerticalScroll);
                        LoadAccountData();
                    }
                    else
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources1.Feature_Error);
                        Global.CloseDialog();
                    }
                });

            }, 10);
        }

        private void LoadAccountData()
        {
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> header = Global.GetHeader(Global.CustomerToken);
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                Dictionary<string, string> accountId = Global.RequestAPI<Dictionary<string, string>>(Constants.AccountNextIdURL, Method.Post, header, null, query, "");
                BusinessUnitsList = Global.RequestAPI<List<UserModel.BusinessUnitModel>>(Constants.BusinessUnitsURL, Method.Get, header, null, query, "");
                
                query["api-version"] = 1.1;
                List<Dictionary<string, string>> titles = Global.RequestAPI<List<Dictionary<string, string>>>(Constants.ContactTitlesURL, Method.Get, header, null, query, "");
                AddressType = Global.RequestAPI<List<AddressType>>(Constants.AddressTypeURL, Method.Get, header, null, query, "");
                CountriesList = Global.RequestAPI<List<AddressModel.Country>>(Constants.AddressCountriesURL, Method.Get, header, null, query, "");
                PhoneType = Global.RequestAPI<List<PhoneModel.PhoneType>>(Constants.PhoneTypeURL, Method.Get, header, null, query, "");
                RelationShipList = Global.RequestAPI<List<RelatedContactModel.Relationship>>(Constants.RelationShipURL, Method.Get, header, null, query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (accountId == null || titles == null || AddressType == null || CountriesList == null || PhoneType == null
                        || RelationShipList == null || BusinessUnitsList == null)
                    {
                        Global.CloseDialog();
                        return;
                    } 

                    foreach (Dictionary<string, string> item in titles) TitlesList.Add(item["Name"]);
                    ComboNameTitles.ItemsSource = TitlesList;

                    List<string> list = new List<string>();
                    foreach (UserModel.BusinessUnitModel item in BusinessUnitsList) list.Add(item.Name);
                    ComboBusinessUnit.ItemsSource = list;
                    if (list.Count == 1) ComboBusinessUnit.SelectedIndex = 0;

                    foreach (AddressModel.AddressType item in AddressType)
                    {
                        AddressTypeList.Add(item.Name);
                        AddressTypeCodeList.Add(item.Code);
                    }

                    foreach (AddressModel.Country item in CountriesList)
                    {
                        CountriesStringList.Add(item.Name);
                        CountriesCodeList.Add(item.Code);
                    }

                    foreach (PhoneModel.PhoneType item in PhoneType) PhoneTypeList.Add(item.Name);
                    foreach (RelatedContactModel.Relationship item in RelationShipList) RelationTypeList.Add(item.Name);

                    if (accountId != null)
                    {
                        TextAccountId.Text = accountId["UserId"];
                        TextLoginId.Text = accountId["UserId"];
                    }
                });

            }, 10);
        }

        private void ShowHideVerticalScroll(bool isChecked)
        {
            ScrollViewer[] scrolls = new ScrollViewer[] { ScrollViewMain};
            int count = scrolls.Length;

            for (int i = 0; i < count; i++) if (scrolls[i] == null) return;
            for (int i = 0; i < count; i++) scrolls[i].VerticalScrollBarVisibility = isChecked ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (TabAccountComponents == null) return;
            bool isChecked = e.RoutedEvent.Name == "Checked";

            CheckBox checkBox = (CheckBox)sender;
            string content = checkBox.Content.ToString();

            int count = TabAccountComponents.Items.Count;
            for (int i = 0; i < count; i++)
            {
                TabItem item = (TabItem)TabAccountComponents.Items[i];
                string header = item.Header.ToString();
                if (header == content)
                {
                    item.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;
                    break;
                }
            }

        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void ComboAccountType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ComboAccountType.SelectedIndex;
            if (index == -1) return;

            if (index == 0)
            {
                TextCoroprationName.Visibility = Visibility.Visible;
                TextCorporationNumber.Visibility = Visibility.Visible;
                ComboNameTitles.Visibility = Visibility.Collapsed;
                TextFirstName.Visibility = Visibility.Collapsed;
                TextFamilyName.Visibility = Visibility.Collapsed;
            }
            else if (index == 1)
            {
                TextCoroprationName.Visibility = Visibility.Collapsed;
                TextCorporationNumber.Visibility = Visibility.Collapsed;
                ComboNameTitles.Visibility = Visibility.Visible;
                TextFirstName.Visibility = Visibility.Visible;
                TextFamilyName.Visibility = Visibility.Visible;
            }

            TextAccountId.Visibility = Visibility.Visible;
            ComboBusinessUnit.Visibility = Visibility.Visible;
            GridFirst.Visibility = Visibility.Visible;
            GridSecond.Visibility = Visibility.Visible;
            TextFamilyName.Visibility = Visibility.Visible;
            BorderEmail.Visibility = Visibility.Visible;
            BorderPhone.Visibility = Visibility.Visible;
            BorderAddress.Visibility = Visibility.Visible;
            BorderContact.Visibility = Visibility.Visible;

            SetConfiguration(ComboAccountType.SelectedItem.ToString());

        }

        private void SetConfiguration(string accountType = "Default")
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> result = Global.RequestAPI<Dictionary<string, object>>(Constants.AccountConfiguURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("AccountType", accountType), Global.BuildDictionary("api-version", 1.0), "", false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    int count = TabAccountComponents.Items.Count;
                    LoadingPanel.Visibility = Visibility.Hidden;

                    if (result == null || result.Count == 0)
                    {
                        if (accountType != "Default") SetConfiguration();
                        else
                        {
                            MessageUtils.ShowErrorMessage("", Properties.Resources1.Config_Error);
                            for (int i = 0; i < count; i++)
                            {
                                TabItem item = TabAccountComponents.Items[i] as TabItem;
                                if (i != 0) item.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                    else
                    {
                        ButtonNext.Visibility = Visibility.Visible;
                        for (int i = 0; i < count; i++)
                        {
                            TabItem item = TabAccountComponents.Items[i] as TabItem;
                            string key = item.Header.ToString().Replace(" ", "").Replace("_", "");
                            if (result.ContainsKey(key))
                            {
                                var value = result[key];
                                if (value == null) continue;
                                Dictionary<string, object> tabInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(value.ToString());
                                bool enabled = Convert.ToBoolean(tabInfo["Enabled"].ToString());
                                item.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;

                                if (key == "PaymentMethods")
                                {
                                    GridCreditCard.Visibility = Convert.ToBoolean(tabInfo["CreditCardEnabled"].ToString()) ? Visibility.Visible : Visibility.Collapsed;
                                    GridBankTransfer.Visibility = Convert.ToBoolean(tabInfo["BankEnabled"].ToString()) ? Visibility.Visible : Visibility.Collapsed;
                                }
                            }
                        }
                    }
                });

            }, 10);
        }

        private void ButtonAddressAdd_Click(object sender, RoutedEventArgs e)
        {
            AddAddressInfo(PanelAddress.Children.Count);
        }

        private void AddAddressInfo(int index, AddressModel.AddressUsage address = null)
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;

            Grid grid = new Grid();
            ColumnDefinition column = new ColumnDefinition();
            column.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column1 = new ColumnDefinition();
            column1.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column2 = new ColumnDefinition();
            column2.Width = new GridLength(70);

            grid.ColumnDefinitions.Add(column);
            grid.ColumnDefinitions.Add(column1);
            grid.ColumnDefinitions.Add(column2);

            ComboBox combo1 = new ComboBox();
            combo1.ItemsSource = FormatTypeList;
            combo1.Margin = new Thickness(10);
            combo1.FontSize = 16;
            combo1.SelectedIndex = 0;
            combo1.SelectionChanged += ComboBox_SelectionChanged;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(combo1, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(combo1, Properties.Resources.Address_Format + " *");
            grid.Children.Add(combo1);
            Grid.SetColumn(combo1, 0);

            ComboBox combo = new ComboBox();
            combo.Margin = new Thickness(10);
            combo.FontSize = 16;
            combo.SelectedIndex = address == null ? -1 : AddressTypeList.IndexOf(address.AddressTypes[0].Name);

            int count = AddressTypeList.Count;
            for (int i = 0; i < count; i++)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = AddressTypeList[i];
                combo.Items.Add(item);
            }

            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(combo, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(combo, Properties.Resources.Address_Type + " *");
            combo.IsEnabled = address == null;
            grid.Children.Add(combo);
            Grid.SetColumn(combo, 1);

            Grid grid1 = new Grid();
            ColumnDefinition column3 = new ColumnDefinition();
            column3.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column4 = new ColumnDefinition();
            column4.Width = new GridLength(70);

            grid1.ColumnDefinitions.Add(column3);
            grid1.ColumnDefinitions.Add(column4);

            StackPanel panel10 = new StackPanel();
            panel10.Orientation = Orientation.Vertical;

            TextBox text = new TextBox();
            text.Text = address == null ? "" : string.Format("{0} {1} {2} {3}", address.AddressLine1, address.AddressLine2, address.Suburb,
                address.State, address.PostCode);
            text.Margin = new Thickness(10, 10, 10, 0);
            text.FontSize = 16;
            text.Name = "TextAddress_" + (index + 1);
            //text.LostFocus += TextBox_LostFocus;
            text.KeyUp += TextBoxSearch_KeyUp;

            /*text.KeyDown += TextBoxSearch_KeyDown;
            
            if (address == null)
            {
                int id = new Random().Next(1000, 9999);
                text.Name = "Address_" + id;
                text.Tag = "" + id;
            }
            else
            {
                text.Name = "Address_" + address.Id;
                text.Tag = "" + address.Id;
            }*/

            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(text, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(text, Properties.Resources.Address + " *");

            ProgressBar progress = new ProgressBar();
            progress.Margin = new Thickness(10, 0, 10, 0);
            progress.IsIndeterminate = true;
            progress.Visibility = Visibility.Collapsed;

            ListView listAddress = new ListView();
            listAddress.FontSize = 16;
            listAddress.MaxHeight = 150;
            listAddress.Tag = text.Name;
            listAddress.KeyUp += ListViewItem_KeyUp;
            listAddress.MouseUp += ListViewItem_MouseUpClick;

            MaterialDesignThemes.Wpf.Card card = new MaterialDesignThemes.Wpf.Card();
            card.Margin = new Thickness(10, 0, 10, 0);
            card.Content = listAddress;
            card.Visibility = Visibility.Collapsed;

            panel10.Children.Add(text);
            panel10.Children.Add(progress);
            panel10.Children.Add(card);

            grid1.Children.Add(panel10);
            Grid.SetColumn(text, 0);

            StackPanel panel1 = new StackPanel();
            panel1.Orientation = Orientation.Vertical;

            TextBox textPostalCode = new TextBox();
            ComboBox comboCountry = new ComboBox();

            for (int i = 0; i < 4; i++)
            {
                Grid grid4 = new Grid();
                ColumnDefinition column7 = new ColumnDefinition();
                column7.Width = new GridLength(1, GridUnitType.Star);
                ColumnDefinition column8 = new ColumnDefinition();
                column8.Width = new GridLength(1, GridUnitType.Star);

                grid4.ColumnDefinitions.Add(column7);
                grid4.ColumnDefinitions.Add(column8);

                if (i == 0)
                {
                    TextBox text1 = new TextBox();
                    text1.Margin = new Thickness(10);
                    text1.FontSize = 16;
                    text1.Text = address == null ? "" : address.AddressLine1;
                    MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(text1, true);
                    MaterialDesignThemes.Wpf.HintAssist.SetHint(text1, Properties.Resources.Address_Line1 + " *");
                    grid4.Children.Add(text1);
                    Grid.SetColumn(text1, 0);

                    TextBox text2 = new TextBox();
                    text2.Margin = new Thickness(10);
                    text2.FontSize = 16;
                    text2.Text = address == null ? "" : address.AddressLine2;
                    MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(text2, true);
                    MaterialDesignThemes.Wpf.HintAssist.SetHint(text2, Properties.Resources.Address_Line2 + " *");
                    grid4.Children.Add(text2);
                    Grid.SetColumn(text2, 1);

                }
                else if (i == 1)
                {
                    TextBox text1 = new TextBox();
                    text1.Margin = new Thickness(10);
                    text1.FontSize = 16;
                    text1.TextChanged += TextBoxPostalCode_TextChanged;
                    MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(text1, true);
                    MaterialDesignThemes.Wpf.HintAssist.SetHint(text1, Properties.Resources.Postal_Code + " *");
                    grid4.Children.Add(text1);
                    Grid.SetColumn(text1, 0);
                    textPostalCode = text1;

                    ComboBox combo2 = new ComboBox();
                    combo2.Margin = new Thickness(10);
                    combo2.FontSize = 16;
                    combo2.Tag = address == null ? "" : address.Suburb;
                    MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(combo2, true);
                    MaterialDesignThemes.Wpf.HintAssist.SetHint(combo2, Properties.Resources.Suburb + " *");
                    grid4.Children.Add(combo2);
                    Grid.SetColumn(combo2, 1);
                }
                else if (i == 2)
                {
                    TextBox text1 = new TextBox();
                    text1.Margin = new Thickness(10);
                    text1.FontSize = 16;
                    text1.Text = address == null ? "" : address.City;
                    MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(text1, true);
                    MaterialDesignThemes.Wpf.HintAssist.SetHint(text1, Properties.Resources.City + " *");
                    grid4.Children.Add(text1);
                    Grid.SetColumn(text1, 0);

                    ComboBox combo2 = new ComboBox();
                    combo2.Margin = new Thickness(10);
                    combo2.FontSize = 16;
                    combo2.Tag = address == null ? "" : address.State;
                    MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(combo2, true);
                    MaterialDesignThemes.Wpf.HintAssist.SetHint(combo2, Properties.Resources.State + " *");
                    grid4.Children.Add(combo2);
                    Grid.SetColumn(combo2, 1);
                }
                else if (i == 3)
                {
                    ComboBox combo2 = new ComboBox();
                    combo2.Margin = new Thickness(10);
                    combo2.FontSize = 16;
                    combo2.ItemsSource = CountriesStringList;
                    combo2.SelectionChanged += ComboBoxCountry_SelectionChanged;
                    MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(combo2, true);
                    MaterialDesignThemes.Wpf.HintAssist.SetHint(combo2, Properties.Resources.Country + " *");
                    grid4.Children.Add(combo2);
                    Grid.SetColumn(combo2, 0);
                    comboCountry = combo2;
                }

                panel1.Children.Add(grid4);

            }

            panel1.Visibility = Visibility.Collapsed;
            grid1.Children.Add(panel1);
            Grid.SetColumn(panel1, 0);

            Button button = new Button();
            button.Margin = new Thickness(10);
            button.VerticalAlignment = VerticalAlignment.Center;
            button.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            MaterialDesignThemes.Wpf.PackIcon icon = new MaterialDesignThemes.Wpf.PackIcon();
            icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Delete;
            button.Content = icon;
            button.Click += ButtonAddressDelete_Click;
            grid1.Children.Add(button);
            Grid.SetColumn(button, 1);

            panel.Children.Add(grid);
            panel.Children.Add(grid1);
            PanelAddress.Children.Add(panel);

            textPostalCode.Text = address == null ? "" : address.PostCode;
            comboCountry.SelectedIndex = address == null ? -1 : CountriesStringList.IndexOf(address.Country);
        }

        private void TextBoxSearch_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox textAddress = (TextBox)sender;
            string search = textAddress.Text;
            StackPanel panel = (StackPanel)textAddress.Parent;
            ProgressBar progress = (ProgressBar)panel.Children[1];
            MaterialDesignThemes.Wpf.Card card = (MaterialDesignThemes.Wpf.Card)panel.Children[2];
            ListView listView = (ListView)card.Content;

            if (search == "" || RequestTimer != null || SelectedAddressStatus)
            {
                listView.ItemsSource = new List<string>();
                card.Visibility = Visibility.Collapsed;
                return;
            }

            if (e.Key == Key.Down)
            {
                listView.Focus();
                listView.SelectedIndex = 0;
                return;
            }

            RequestTimer = EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    search = textAddress.Text;
                    progress.Visibility = Visibility.Visible;
                    card.Visibility = Visibility.Collapsed;
                });

                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("Term", search);
                query.Add("CloseMatches", true);
                List<Dictionary<string, string>> result = Global.RequestAPI<List<Dictionary<string, string>>>(Constants.AutoCompleteURL, Method.Get, 
                    Global.GetHeader(Global.CustomerToken), null, query, "");

                List<string> addresses = new List<string>();
                if (result != null) foreach (Dictionary<string, string> item in result) addresses.Add(item["Address"]);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    progress.Visibility = Visibility.Collapsed;
                    listView.ItemsSource = new List<string>();
                    listView.ItemsSource = addresses;
                    card.Visibility = addresses.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                    RequestTimer.Dispose();
                    RequestTimer = null;
                });

            }, 2000);
        }

        private void ButtonAddressDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            PanelAddress.Children.Remove(((StackPanel)((Grid)button.Parent).Parent));
        }

        private void TextBoxPostalCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textPostalCode = (TextBox)sender;
            string postalCode = textPostalCode.Text;
            if (postalCode.Length < 4) return;

            Grid grid = (Grid)textPostalCode.Parent;
            ComboBox comboSuburb = (ComboBox)grid.Children[1];

            StackPanel panel = (StackPanel)grid.Parent;
            ComboBox comboCountry = (ComboBox)((Grid)panel.Children[3]).Children[0];
            ComboBox comboState = (ComboBox)((Grid)panel.Children[2]).Children[1];

            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> path = Global.BuildDictionary("CountryCode", "AU");
                path.Add("PostCode", postalCode);
                List<Dictionary<string, object>> result = Global.RequestAPI<List<Dictionary<string, object>>>(Constants.PostCodeURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    path, Global.BuildDictionary("api-version", 1.1), "");

                List<string> suburbList = new List<string>();
                foreach (Dictionary<string, object> item in result) suburbList.Add(item["LocalityName"].ToString());

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    comboSuburb.ItemsSource = suburbList;
                    comboSuburb.SelectedIndex = suburbList.IndexOf(comboSuburb.Tag.ToString());
                });

            }, 10);

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            if (combo == null) return;

            Grid grid = (Grid)combo.Parent;
            StackPanel panel = (StackPanel)grid.Parent;
            grid = (Grid)panel.Children[1];

            TextBox textBox = (TextBox)((StackPanel)grid.Children[0]).Children[0];
            StackPanel panel1 = (StackPanel)grid.Children[1];

            if (combo.SelectedIndex == 0)
            {
                textBox.Visibility = Visibility.Visible;
                panel1.Visibility = Visibility.Collapsed;
            }
            else if (combo.SelectedIndex == 1)
            {
                textBox.Visibility = Visibility.Collapsed;
                panel1.Visibility = Visibility.Visible;
            }
        }

        private void ListViewItem_MouseUpClick(object sender, MouseButtonEventArgs e)
        {
            ListView listView = (ListView)sender;
            if (listView == null || listView.SelectedItem == null) return;

            HandleListEvent(listView);

        }

        private void ListViewItem_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ListView listView = (ListView)sender;
                if (listView == null || listView.SelectedItem == null) return;

                HandleListEvent(listView);
            }
        }

        private void HandleListEvent(ListView listView)
        {
            string name = listView.Tag.ToString();
            TextBox textBoxAddress = Global.FindChild<TextBox>(this, name);
            string address = listView.SelectedItem.ToString();
            textBoxAddress.Text = address;

            Grid grid = (Grid)((StackPanel)textBoxAddress.Parent).Parent;
            StackPanel panel = (StackPanel)grid.Children[1];

            Global.CloseDialog("SearchAddress");
            LoadingPanel.Visibility = Visibility.Visible;
            
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("Term", address);
                Dictionary<string, string> result = Global.RequestAPI<Dictionary<string, string>>(Constants.ParseAddressURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    null, query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    MaterialDesignThemes.Wpf.Card card = (MaterialDesignThemes.Wpf.Card)listView.Parent;
                    card.Visibility = Visibility.Collapsed;

                    grid = (Grid)panel.Children[0];
                    ((TextBox)grid.Children[0]).Text = result["StreetLine"];
                    grid = (Grid)panel.Children[1];
                    ((TextBox)grid.Children[0]).Text = result["Postcode"];
                    ((ComboBox)grid.Children[1]).Tag = result["Suburb"];
                    grid = (Grid)panel.Children[2];
                    ComboBox comboState = (ComboBox)grid.Children[1];
                    comboState.Tag = result["State"];
                    grid = (Grid)panel.Children[3];
                    ((ComboBox)grid.Children[0]).SelectedIndex = CountriesCodeList.IndexOf("AU");
                    LoadingPanel.Visibility = Visibility.Hidden;
                    SelectedAddressStatus = true;
                    textBoxAddress.Focus();
                    textBoxAddress.SelectionStart = textBoxAddress.Text.Length;
                });
            }, 10);
        }

        private void ComboBoxCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            if (combo == null) return;

            Grid grid = (Grid)combo.Parent;
            StackPanel panel = (StackPanel)grid.Parent;
            grid = (Grid)panel.Children[2];

            ComboBox ComboState = (ComboBox)grid.Children[1];
            string countryCode = CountriesCodeList[CountriesStringList.IndexOf(combo.SelectedItem.ToString())];
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                List<AddressModel.Country> result = Global.RequestAPI<List<AddressModel.Country>>(Constants.AddressStateURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("CountryCode", countryCode), Global.BuildDictionary("api-version", 1.1), "");
                List<string> stateList = new List<string>();
                foreach (AddressModel.Country item in result) stateList.Add(item.Code);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    ComboState.ItemsSource = stateList;
                    ComboState.SelectedIndex = stateList.IndexOf(ComboState.Tag.ToString());
                });

            }, 10);

        }

        private void ButtonContactAdd_Click(object sender, RoutedEventArgs e)
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;

            Grid grid = new Grid();
            ColumnDefinition column = new ColumnDefinition();
            column.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column1 = new ColumnDefinition();
            column1.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column2 = new ColumnDefinition();
            column2.Width = new GridLength(70);

            grid.ColumnDefinitions.Add(column);
            grid.ColumnDefinitions.Add(column1);
            grid.ColumnDefinitions.Add(column2);

            ComboBox combo1 = new ComboBox();
            combo1.Margin = new Thickness(10);
            combo1.FontSize = 16;
            combo1.ItemsSource = RelationTypeList;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(combo1, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(combo1, Properties.Resources.Type);
            grid.Children.Add(combo1);
            Grid.SetColumn(combo1, 0);

            ComboBox combo2 = new ComboBox();
            combo2.Margin = new Thickness(10);
            combo2.FontSize = 16;
            combo2.ItemsSource = TitlesList;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(combo2, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(combo2, Properties.Resources.Title1);
            grid.Children.Add(combo2);
            Grid.SetColumn(combo2, 1);

            Grid grid1 = new Grid();
            ColumnDefinition column3 = new ColumnDefinition();
            column3.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column4 = new ColumnDefinition();
            column4.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column5 = new ColumnDefinition();
            column5.Width = new GridLength(70);

            grid1.ColumnDefinitions.Add(column3);
            grid1.ColumnDefinitions.Add(column4);
            grid1.ColumnDefinitions.Add(column5);

            TextBox text1 = new TextBox();
            text1.Margin = new Thickness(10);
            text1.FontSize = 16;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(text1, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(text1, Properties.Resources.First_Name1);
            grid1.Children.Add(text1);
            Grid.SetColumn(text1, 0);

            TextBox text2 = new TextBox();
            text2.Margin = new Thickness(10);
            text2.FontSize = 16;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(text2, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(text2, Properties.Resources.Family_Name1);
            grid1.Children.Add(text2);
            Grid.SetColumn(text2, 1);

            Button button = new Button();
            button.Margin = new Thickness(10);
            button.VerticalAlignment = VerticalAlignment.Center;
            button.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            MaterialDesignThemes.Wpf.PackIcon icon = new MaterialDesignThemes.Wpf.PackIcon();
            icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Delete;
            button.Content = icon;
            button.Click += ButtonContactDelete_Click;
            grid1.Children.Add(button);
            Grid.SetColumn(button, 2);

            panel.Children.Add(grid);
            panel.Children.Add(grid1);
            PanelContact.Children.Add(panel);
        }

        private void ButtonContactDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            PanelContact.Children.Remove(((StackPanel)((Grid)button.Parent).Parent));
        }

        private void ButtonPhoneAdd_Click(object sender, RoutedEventArgs e)
        {
            Grid grid = new Grid();
            ColumnDefinition column = new ColumnDefinition();
            column.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column1 = new ColumnDefinition();
            column1.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column2 = new ColumnDefinition();
            column2.Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(column);
            grid.ColumnDefinitions.Add(column1);
            grid.ColumnDefinitions.Add(column2);

            ComboBox combo = new ComboBox();
            combo.Margin = new Thickness(10);
            combo.FontSize = 16;
            //combo.SelectionChanged += ComboBox_SelectionChanged;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(combo, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(combo, Properties.Resources.Phone_Type + " *");
            combo.ItemsSource = PhoneTypeList;
            grid.Children.Add(combo);
            Grid.SetColumn(combo, 0);

            TextBox text = new TextBox();
            text.Margin = new Thickness(10);
            text.FontSize = 16;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(text, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(text, Properties.Resources.Phone_Number + " *");
            grid.Children.Add(text);
            Grid.SetColumn(text, 1);

            Button button = new Button();
            button.Margin = new Thickness(10);
            button.VerticalAlignment = VerticalAlignment.Center;
            button.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            MaterialDesignThemes.Wpf.PackIcon icon = new MaterialDesignThemes.Wpf.PackIcon();
            icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Delete;
            button.Content = icon;
            button.Click += ButtonPhoneDelete_Click;
            grid.Children.Add(button);
            Grid.SetColumn(button, 2);

            PanelPhones.Children.Add(grid);
        }

        private void ButtonPhoneDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            PanelPhones.Children.Remove((Grid)button.Parent);
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            int index = Convert.ToInt32(((Button)sender).Tag);
            TabAccountComponents.SelectedIndex = index + 1;
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
            int index = Convert.ToInt32(((Button)sender).Tag);
            TabAccountComponents.SelectedIndex = index - 1;
        }

        private void ButtonEmailAdd_Click(object sender, RoutedEventArgs e)
        {
            Grid grid = new Grid();
            ColumnDefinition column = new ColumnDefinition();
            column.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column1 = new ColumnDefinition();
            column1.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column2 = new ColumnDefinition();
            column2.Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(column);
            grid.ColumnDefinitions.Add(column1);
            grid.ColumnDefinitions.Add(column2);

            ComboBox combo = new ComboBox();
            combo.Margin = new Thickness(10);
            combo.FontSize = 16;
            //combo.SelectionChanged += ComboBox_SelectionChanged;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(combo, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(combo, Properties.Resources.Email_Type + " *");
            grid.Children.Add(combo);
            Grid.SetColumn(combo, 0);

            TextBox text = new TextBox();
            text.Margin = new Thickness(10);
            text.FontSize = 16;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(text, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(text, Properties.Resources.Email + " *");
            grid.Children.Add(text);
            Grid.SetColumn(text, 1);

            Button button = new Button();
            button.Margin = new Thickness(10);
            button.VerticalAlignment = VerticalAlignment.Center;
            button.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            MaterialDesignThemes.Wpf.PackIcon icon = new MaterialDesignThemes.Wpf.PackIcon();
            icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Delete;
            button.Content = icon;
            button.Click += ButtonEmailDelete_Click;
            grid.Children.Add(button);
            Grid.SetColumn(button, 2);

            PanelEmails.Children.Add(grid);
        }

        private void ButtonEmailDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            PanelEmails.Children.Remove((Grid)button.Parent);
        }

        private void TabAccountComponents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                int index = TabAccountComponents.SelectedIndex;
                TabItem item = (TabItem)TabAccountComponents.Items[index];
                string header = item.Header.ToString();

                switch (header)
                {
                    case "Options":
                        LoadAccountOptionData();
                        break;
                    case "Identification":
                        LoadIdenfiticationData();
                        break;
                    case "Attributes":
                        LoadAttributesData();
                        break;
                    case "Plans":
                        break;
                    case "Contracts":
                        break;
                    case "Documents":
                        LoadDocumentsData();
                        break;
                    case "Notification":
                        LoadNotificationData();
                        break;
                    case "_Payment Methods":
                        LoadPaymentMethodData();
                        break;
                    case "Authentication":
                        break;
                }
            }
        }

        private void LoadAccountOptionData()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.1);
                query.Add("SearchString", "**");
                query.Add("SkipRecords", 0);
                query.Add("TakeRecords", 10);
                List<RelatedContactModel.TimeZone> defaultTimeZone = Global.RequestAPI<List<RelatedContactModel.TimeZone>>(Constants.TimeZoneDefaultURL, Method.Get,
                    Global.GetHeader(Global.CustomerToken), null, query, "");
                BillingCycleList = Global.RequestAPI<List<BillCycle>>(Constants.BillCycleBussinessURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("BusinessUnitCode", Global.LoggedUser.BusinessUnits[0].Id), Global.BuildDictionary("api-version", 1.0), "");
                CurrenciesList = Global.RequestAPI<List<BillFormat>>(Constants.CurrencyList, Method.Get, Global.GetHeader(Global.CustomerToken), null,
                    Global.BuildDictionary("api-version", 1.0), "");
                TaxRatesList = Global.RequestAPI<List<BillTax>>(Constants.TaxRatesURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, 
                    Global.BuildDictionary("api-version", 1.0), "");
                TermsList = Global.RequestAPI<List<BillTerms.Item>>(Constants.BillTermsURL, Method.Get, Global.GetHeader(Global.CustomerToken), null,
                    Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<string> list1 = new List<string>();
                    int count = BillingCycleList.Count;
                    int index = 0;
                    string defaultTax = "";

                    for (int i = 0; i < count; i++)
                    {
                        if (BillingCycleList[i].Default) index = i;
                        list1.Add(BillingCycleList[i].Name);
                    }

                    ComboBillingCycle.ItemsSource = list1;
                    ComboBillingCycle.SelectedIndex = index;

                    List<string> list2 = new List<string>();
                    count = CurrenciesList.Count;
                    index = 0;

                    for (int i = 0; i < count; i++)
                    {
                        list2.Add(CurrenciesList[i].Name);
                        if (CurrenciesList[i].Default)
                        {
                            index = i;
                            defaultTax = CurrenciesList[i].DefaultTax;
                        }
                    }

                    ComboCurrency.ItemsSource = list2;
                    ComboCurrency.SelectedIndex = index;

                    List<string> list3 = new List<string>();
                    foreach (BillTax item in TaxRatesList) list3.Add(item.Tax);
                    ComboTaxRate.ItemsSource = list3;
                    ComboTaxRate.SelectedIndex = list3.IndexOf(defaultTax);

                    List<string> list4 = new List<string>();
                    index = 0;
                    count = TermsList.Count;
                    for (int i = 0; i < count; i++)
                    {
                        if (TermsList[i].Default) index = i;
                        list4.Add(TermsList[i].Name);
                    }

                    ComboCreditTerms.ItemsSource = list4;
                    ComboCreditTerms.SelectedIndex = index;

                    SelectedTimeZone = defaultTimeZone[0];
                    TextTimeZone.Text = SelectedTimeZone.Name;
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private void TextTimeZone_KeyUp(object sender, KeyEventArgs e)
        {
            string search = TextTimeZone.Text;
            if (search == "" || RequestTimer != null)
            {
                ListTimeZone.ItemsSource = new List<string>();
                CardTimeZone.Visibility = Visibility.Collapsed;
                return;
            }

            if (e.Key == Key.Down)
            {
                ListTimeZone.Focus();
                ListTimeZone.SelectedIndex = 0;
                return;
            }

            RequestTimer = EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    search = TextTimeZone.Text;
                    ProgressTimeZone.Visibility = Visibility.Visible;
                    CardTimeZone.Visibility = Visibility.Collapsed;
                });

                List<string> list = new List<string>();
                if (search != "")
                {
                    CurrentTimeZoneList = Global.contactClient.GetTimeZoneList(Constants.TimeZoneListURL, search);
                    foreach (RelatedContactModel.TimeZone item in CurrentTimeZoneList) list.Add(item.Name);
                }

                Application.Current.Dispatcher.Invoke(delegate
                {
                    ProgressTimeZone.Visibility = Visibility.Collapsed;
                    ListTimeZone.ItemsSource = new List<string>();
                    ListTimeZone.ItemsSource = list;
                    CardTimeZone.Visibility = CurrentTimeZoneList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                    RequestTimer.Dispose();
                    RequestTimer = null;
                });

            }, 2000);
        }

        private void ListTimeZone_KeyUp(object sender, KeyEventArgs e)
        {
            if (ListTimeZone.SelectedItem == null || e.Key != Key.Enter) return;
            SelectTimeZone();
        }

        private void ListTimeZone_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ListTimeZone.SelectedItem == null) return;
            SelectTimeZone();
        }

        private void SelectTimeZone()
        {
            RelatedContactModel.TimeZone item = CurrentTimeZoneList[ListTimeZone.SelectedIndex];
            SelectedTimeZone = item;
            TextTimeZone.Text = item.Name;
            ListTimeZone.ItemsSource = new List<string>();
            CardTimeZone.Visibility = Visibility.Collapsed;
            TextTimeZone.Focus();
            TextTimeZone.SelectionStart = TextTimeZone.Text.Length;
        }

        private void TextCreditLimit_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string str = textBox.Text;

            if (str == CheckString)
            {
                textBox.SelectionStart = str.Length;
                CheckString = "";
                return;
            }

            if (str.Length > 19)
            {
                textBox.Text = str.Substring(0, 19);
                textBox.SelectionStart = str.Length;
                CheckString = textBox.Text;
                return;
            }

            if (str.Contains("A$"))
            {
                str = str.Replace("A$", "");
                str = str.Replace(",", "");
                double price = Convert.ToDouble(str) * 10;
                str = StringUtils.ConvertCurrency(price);
            }
            else
            {
                str = StringUtils.ConvertCurrency(Convert.ToDouble(str) / 100);
            }

            CheckString = str;
            textBox.Text = str;
        }

        private void TextCreditLimit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void LoadAttributesData()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                UserDefinedType = Global.RequestAPI<List<UserDefined>>(Constants.UserDefinedURL, Method.Get, Global.GetHeader(Global.CustomerToken), null,
                    Global.BuildDictionary("api-version", 1.1), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private async void ButtonAddCharge_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog("DetailDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(new AccountChargeDialog(false, false, "", false, "", true), "DetailDialog");
        }

        public void InsertChargeData(string data, bool isUpdate)
        {
            Dictionary<string, object> item = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
            List<AccountCharge.History> list = (List<AccountCharge.History>)ListCharges.ItemsSource;
            list = list ?? new List<AccountCharge.History>();

            AccountCharge.History charges = new AccountCharge.History();
            charges.DefinitionId = item["DefinitionId"].ToString();
            charges.Description = item["Description"].ToString();
            charges.From = item["From"].ToString();
            charges.Price = (double)item["UnitPrice"];
            charges.PriceText = StringUtils.ConvertCurrency(charges.Price);
            charges.DiscountType = item["DiscountType"].ToString();
            charges.Cost = (double)item["Amount"];
            charges.CostText = StringUtils.ConvertCurrency(charges.Cost.Value);
            charges.DiscountAmount = (double)item["UnitDiscount"];
            charges.DiscountAmountText = StringUtils.ConvertCurrency(charges.DiscountAmount);
            charges.DiscountPercentage = (double)item["UnitDiscountPercentage"];
            charges.Quantity = Convert.ToInt32(item["Quantity"]);

            if (isUpdate) list[ListCharges.SelectedIndex] = charges;
            else list.Add(charges);

            ListCharges.ItemsSource = new List<AccountCharge.History>();
            ListCharges.ItemsSource = list;
        }

        private async void ListItemCharges_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            AccountCharge.History item = (AccountCharge.History)ListCharges.SelectedItem;
            if (item == null) return;
            string data = JsonConvert.SerializeObject(item);

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog("DetailDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(new AccountChargeDialog(false, false, "", false, data, true), "DetailDialog");
        }

        private async void ButtonEditCharge_Click(object sender, RoutedEventArgs e)
        {
            string id = ((Button)sender).Tag.ToString();
            List<AccountCharge.History> list = (List<AccountCharge.History>)ListCharges.ItemsSource;

            string data = "";
            foreach(AccountCharge.History item in list)
            {
                if (item.DefinitionId == id)
                {
                    data = JsonConvert.SerializeObject(item);
                    break;
                }
            }

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog("DetailDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(new AccountChargeDialog(false, false, "", false, data, true), "DetailDialog");
        }

        private void ButtonDeleteCharge_Click(object sender, RoutedEventArgs e)
        {
            string id = ((Button)sender).Tag.ToString();
            List<AccountCharge.History> list = (List<AccountCharge.History>)ListCharges.ItemsSource;

            foreach (AccountCharge.History item in list)
            {
                if (item.DefinitionId == id)
                {
                    list.Remove(item);
                    break;
                }
            }

            ListCharges.ItemsSource = new List<AccountCharge.History>();
            ListCharges.ItemsSource = list;
        }

        private async void BuildUserDefinedView(UserDefined item = null)
        {

            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;
            panel.Margin = new Thickness(20);
            panel.Width = 600;

            Label label = new Label();
            label.Content = item == null ? Properties.Resources.Add_User_Defined : Properties.Resources.Update_User_Defined;
            label.FontSize = 20;
            label.FontWeight = FontWeight.FromOpenTypeWeight(700);
            label.HorizontalAlignment = HorizontalAlignment.Center;
            panel.Children.Add(label);

            Grid grid = new Grid();
            for (int i = 0; i < 2; i++)
            {
                ColumnDefinition column = new ColumnDefinition();
                column.Width = new GridLength(1, GridUnitType.Star);
                grid.ColumnDefinitions.Add(column);
            }

            List<UserDefined> addedList = (List<UserDefined>)ListUserDefined.ItemsSource ?? new List<UserDefined>();
            List<string> list = new List<string>();

            foreach (UserDefined row in UserDefinedType)
            {
                bool isAdded = false;
                if (item == null)
                {
                    foreach (UserDefined row1 in addedList)
                    {
                        if (row.Id == row1.Id)
                        {
                            isAdded = true;
                            break;
                        }
                    }
                }
                if (!isAdded) list.Add(row.Name);
            }

            ComboBox combo = new ComboBox();
            combo.Margin = new Thickness(10);
            combo.ItemsSource = list;
            if (item != null) combo.SelectedIndex = list.IndexOf(item.Name);
            combo.FontSize = 16;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(combo, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(combo, Properties.Resources.User_Defined + " *");
            combo.Tag = item == null ? "" : item.DefaultValue;
            combo.SelectionChanged += ComboUserDefined_SelectionChanged;
            grid.Children.Add(combo);
            Grid.SetColumn(combo, 0);

            TextBox textValue = new TextBox();
            textValue.Margin = new Thickness(10);
            textValue.FontSize = 16;
            if (item != null) textValue.Text = item.Value;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(textValue, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(textValue, Properties.Resources.Value + " *");
            grid.Children.Add(textValue);
            Grid.SetColumn(textValue, 1);

            StackPanel panel1 = new StackPanel();
            panel1.Orientation = Orientation.Horizontal;
            panel1.HorizontalAlignment = HorizontalAlignment.Right;
            panel1.Margin = new Thickness(0, 10, 20, 0);

            Button buttonAdd = new Button();
            buttonAdd.Content = item == null ? Properties.Resources.Add : Properties.Resources.Update;
            buttonAdd.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            buttonAdd.Margin = new Thickness(0, 0, 10, 0);
            buttonAdd.Width = 100;
            buttonAdd.Click += ButtonAttributesSave_Click;
            panel1.Children.Add(buttonAdd);

            Button buttonClose = new Button();
            buttonClose.Content = Properties.Resources.Close;
            buttonClose.Tag = "DetailDialog";
            buttonClose.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            buttonClose.Margin = new Thickness(0, 0, 10, 0);
            buttonClose.Width = 100;
            buttonClose.Click += ButtonAttributesClose_Click;
            panel1.Children.Add(buttonClose);

            panel.Children.Add(grid);
            panel.Children.Add(panel1);

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog("DetailDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(panel, "DetailDialog");
        }

        private void ButtonAttributesClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }

        private void ButtonAttributesAdd_Click(object sender, RoutedEventArgs e)
        {
            BuildUserDefinedView();
        }

        private void ButtonAttributesUpdate_Click(object sender, RoutedEventArgs e)
        {
            string id = ((Button)sender).Tag.ToString();
            List<UserDefined> list = (List<UserDefined>)ListUserDefined.ItemsSource;

            int index = 0;
            foreach (UserDefined item in list)
            {
                index++;
                if (item.Id == id)
                {
                    BuildUserDefinedView(item);
                    ListUserDefined.SelectedIndex = index - 1;
                    break;
                }
            }
        }

        private void ListItemAttributes_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            UserDefined item = (UserDefined)ListUserDefined.SelectedItem;
            if (item == null) return;
            BuildUserDefinedView(item);
        }

        private void ButtonAttributesDelete_Click(object sender, RoutedEventArgs e)
        {
            string id = ((Button)sender).Tag.ToString();
            List<UserDefined> list = (List<UserDefined>)ListUserDefined.ItemsSource;

            foreach (UserDefined item in list)
            {
                if (item.Id == id)
                {
                    list.Remove(item);
                    break;
                }
            }

            ListUserDefined.ItemsSource = new List<UserDefined>();
            ListUserDefined.ItemsSource = list;
        }

        private void ButtonAttributesSave_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string name = button.Content.ToString();
            StackPanel panel = (StackPanel)button.Parent;
            Grid grid = (Grid)((StackPanel)panel.Parent).Children[1];

            ComboBox combo = (ComboBox)grid.Children[0];
            TextBox text = (TextBox)grid.Children[1];
            if (combo.SelectedIndex == -1 || string.IsNullOrEmpty(text.Text)) return;

            UserDefined item = null;
            foreach (UserDefined row in UserDefinedType)
            {
                if (row.Name == combo.SelectedItem.ToString())
                {
                    item = row;
                    break;
                }
            }

            if (item == null) return;
            item.Value = text.Text;

            List<UserDefined> list = (List<UserDefined>)ListUserDefined.ItemsSource ?? new List<UserDefined>();
            if (name == "Add") list.Add(item);
            else list[ListUserDefined.SelectedIndex] = item;

            ListUserDefined.ItemsSource = new List<UserDefined>();
            ListUserDefined.ItemsSource = list;
            Global.CloseDialog("DetailDialog");
        }

        private void ComboUserDefined_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            Grid panel = (Grid)combo.Parent;
            TextBox textBox = (TextBox)panel.Children[1];
            textBox.Text = combo.Tag.ToString();
        }

        private void LoadIdenfiticationData()
        {
            string contactType = new string[] { "Corporate", "Person" }[ComboAccountType.SelectedIndex];
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> header = Global.GetHeader(Global.CustomerToken);
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.1);
                IdentificationType = Global.RequestAPI<List<Identification>>(Constants.IdentificationTypeURL, Method.Get, header, null, query, "");
                IdentificationRules = Global.RequestAPI<Dictionary<string, object>>(Constants.IdentificationRuleURL, Method.Get, header, 
                    Global.BuildDictionary("ContactType", contactType), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (IdentificationRules != null) LabelPoints.Content = string.Format(Properties.Resources.ID_Point_Alert, Convert.ToInt32(IdentificationRules["MinimumPoints"].ToString()), 0);
                });

            }, 10);
        }

        private void ButtonIDAdd_Click(object sender, RoutedEventArgs e)
        {
            BuildIndentificationView(null);
        }

        private void ButtonIDSave_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string name = button.Content.ToString();
            StackPanel panel = (StackPanel)button.Parent;
            Grid grid = (Grid)((StackPanel)panel.Parent).Children[1];

            ComboBox comboType = (ComboBox)grid.Children[0];
            TextBox textValue = (TextBox)grid.Children[1];
            TextBox textPoints = (TextBox)grid.Children[2];

            panel = (StackPanel)((StackPanel)panel.Parent).Children[2];
            DatePicker dateIssue = (DatePicker)panel.Children[0];
            DatePicker dateExpiry = (DatePicker)panel.Children[1];

            Identification item = null;
            if (comboType.SelectedIndex == -1 || textValue.Text == "") return;
          
            foreach (Identification row in IdentificationType)
            {
                if (row.Name == comboType.SelectedItem.ToString())
                {
                    item = row;
                    break;
                }
            }

            if (item == null) return;
            if (item.HasIssueDate && dateIssue.SelectedDate == null)
            {
                MessageUtils.ShowErrorMessage("", Properties.Resources.Issue_Date_Require);
                return;
            }

            if (item.HasExpiryDate && dateExpiry.SelectedDate == null)
            {
                MessageUtils.ShowErrorMessage("", Properties.Resources.Expiry_Date_Require);
                return;
            }


            item.Value = textValue.Text;
            item.Points = Convert.ToInt32(textPoints.Text);
            item.IssueDate = item.HasIssueDate ? dateIssue.SelectedDate.Value.ToString("yyyy-MM-dd hh:mm:ss") : null;
            item.ExpiryDate = item.HasExpiryDate ? dateExpiry.SelectedDate.Value.ToString("yyyy-MM-dd hh:mm:ss") : null;

            List<Identification> list = (List<Identification>)ListIdetification.ItemsSource ?? new List<Identification>();
            if (IdentificationRules != null)
            {
                int points = 0;
                foreach (Identification row in list) points += row.Points.Value;
                if (name == "Add") points += item.Points.Value;
                LabelPoints.Content = string.Format(Properties.Resources.ID_Point_Alert, Convert.ToInt32(IdentificationRules["MinimumPoints"].ToString()), points);
            }

            if (name == "Add") list.Add(item);
            else list[ListIdetification.SelectedIndex] = item;

            ListIdetification.ItemsSource = new List<Identification>();
            ListIdetification.ItemsSource = list;
            Global.CloseDialog("DetailDialog");
        }

        private void ButtonIDEdit_Click(object sender, RoutedEventArgs e)
        {
            string id = ((Button)sender).Tag.ToString();
            List<Identification> list = (List<Identification>)ListIdetification.ItemsSource;

            int index = 0;
            foreach (Identification item in list)
            {
                index++;
                if (item.Id == id)
                {
                    BuildIndentificationView(item);
                    ListIdetification.SelectedIndex = index - 1;
                    break;
                }
            }
        }

        private void ButtonIDDelete_Click(object sender, RoutedEventArgs e)
        {
            string id = ((Button)sender).Tag.ToString();
            List<Identification> list = (List<Identification>)ListIdetification.ItemsSource;

            foreach (Identification item in list)
            {
                if (item.Id == id)
                {
                    list.Remove(item);
                    break;
                }
            }

            ListIdetification.ItemsSource = new List<Identification>();
            ListIdetification.ItemsSource = list;

            if (IdentificationRules != null)
            {
                int points = 0;
                foreach (Identification item in list) points += item.Points.Value;
                LabelPoints.Content = string.Format(Properties.Resources.ID_Point_Alert, Convert.ToInt32(IdentificationRules["MinimumPoints"].ToString()), points);
            }
        }
        
        private void ListItemID_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            Identification item = (Identification)ListIdetification.SelectedItem;
            if (item == null) return;
            BuildIndentificationView(item);
        }

        private async void BuildIndentificationView(Identification item)
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;
            panel.Margin = new Thickness(10);
            panel.Width = 800;
            IsLoadingUI = false;

            Label label = new Label();
            label.Content = item == null ? Properties.Resources.Add_Identification : Properties.Resources.Update_Identification;
            label.Margin = new Thickness(10);
            label.FontSize = 20;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.FontWeight = FontWeights.Bold;
            panel.Children.Add(label);

            Grid grid = new Grid();
            ColumnDefinition column = new ColumnDefinition();
            column.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column1 = new ColumnDefinition();
            column1.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column2 = new ColumnDefinition();
            column2.Width = new GridLength(1, GridUnitType.Star);

            grid.ColumnDefinitions.Add(column);
            grid.ColumnDefinitions.Add(column1);
            grid.ColumnDefinitions.Add(column2);

            List<Identification> addedList = (List<Identification>)ListIdetification.ItemsSource ?? new List<Identification>();
            List<string> list = new List<string>();

            foreach (Identification row in IdentificationType)
            {
                bool isAdded = false;
                if (item == null && !row.AllowDuplicates)
                {
                    foreach (Identification row1 in addedList)
                    {
                        if (row.Id == row1.Id)
                        {
                            isAdded = true;
                            break;
                        }
                    }
                }
                if (!isAdded) list.Add(row.Name);
            }

            ComboBox combo1 = new ComboBox();
            combo1.Margin = new Thickness(10);
            combo1.FontSize = 16;
            combo1.ItemsSource = list;
            combo1.SelectionChanged += ComboID_SelectionChanged;
            if (item != null) combo1.SelectedIndex = list.IndexOf(item.Name);
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(combo1, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(combo1, Properties.Resources.Identification1);
            grid.Children.Add(combo1);
            Grid.SetColumn(combo1, 0);

            TextBox text1 = new TextBox();
            text1.Margin = new Thickness(10);
            text1.FontSize = 16;
            text1.TextChanged += TextBox_TextChanged;
            if (item != null) text1.Text = item.Value;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(text1, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(text1, Properties.Resources.Value1);
            grid.Children.Add(text1);
            Grid.SetColumn(text1, 1);

            TextBox text2 = new TextBox();
            text2.Margin = new Thickness(10);
            text2.FontSize = 16;
            text2.TextChanged += TextBox_TextChanged;
            if (item != null) text2.Text = item.Points + "";
            text2.PreviewTextInput += TextCreditLimit_PreviewTextInput;
            text2.IsEnabled = false;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(text2, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(text2, Properties.Resources.Points);
            grid.Children.Add(text2);
            Grid.SetColumn(text2, 2);

            StackPanel panel2 = new StackPanel();
            panel2.Orientation = Orientation.Horizontal;

            DatePicker datePicker1 = new DatePicker();
            datePicker1.Margin = new Thickness(10);
            datePicker1.Width = 250;
            datePicker1.FontSize = 16;
            datePicker1.SelectedDateChanged += DatePicker_SelectedDateChanged;
            if (item != null && item.IssueDate != null) datePicker1.SelectedDate = DateTime.Parse(item.IssueDate);
            if (item != null) datePicker1.Visibility = item.HasIssueDate ? Visibility.Visible : Visibility.Collapsed;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(datePicker1, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(datePicker1, Properties.Resources.Issue_Date1);
            panel2.Children.Add(datePicker1);

            DatePicker datePicker2 = new DatePicker();
            datePicker2.Margin = new Thickness(10);
            datePicker2.Width = 250;
            datePicker2.FontSize = 16;
            datePicker2.SelectedDateChanged += DatePicker_SelectedDateChanged;
            if (item != null && item.ExpiryDate != null) datePicker2.SelectedDate = DateTime.Parse(item.ExpiryDate);
            if (item != null) datePicker2.Visibility = item.HasExpiryDate ? Visibility.Visible : Visibility.Collapsed;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(datePicker2, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(datePicker2, Properties.Resources.Expiry_Date1);
            panel2.Children.Add(datePicker2);

            Button buttonUpload = new Button();
            buttonUpload.Content = Properties.Resources.Upload_File;
            buttonUpload.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            buttonUpload.Margin = new Thickness(10);
            buttonUpload.Click += ButtonIDUpload_Click;
            panel2.Children.Add(buttonUpload);

            StackPanel panel1 = new StackPanel();
            panel1.Orientation = Orientation.Horizontal;
            panel1.HorizontalAlignment = HorizontalAlignment.Right;
            panel1.Margin = new Thickness(0, 10, 20, 0);
            
            Button buttonAdd = new Button();
            buttonAdd.Name = "ButtonIdSave";
            buttonAdd.Content = item == null ? Properties.Resources.Add : Properties.Resources.Update;
            buttonAdd.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            buttonAdd.Margin = new Thickness(0, 0, 10, 0);
            buttonAdd.Width = 100;
            if (item == null) buttonAdd.IsEnabled = false;
            buttonAdd.Click += ButtonIDSave_Click;
            panel1.Children.Add(buttonAdd);

            Button buttonClose = new Button();
            buttonClose.Content = Properties.Resources.Close;
            buttonClose.Tag = "DetailDialog";
            buttonClose.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            buttonClose.Margin = new Thickness(0, 0, 10, 0);
            buttonClose.Width = 100;
            buttonClose.Click += ButtonAttributesClose_Click;
            panel1.Children.Add(buttonClose);

            PanelFileList = new WrapPanel();
            PanelFileList.Orientation = Orientation.Horizontal;

            panel.Children.Add(grid);
            panel.Children.Add(panel2);
            panel.Children.Add(PanelFileList);
            panel.Children.Add(panel1);
            IsLoadingUI = true;

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog("DetailDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(panel, "DetailDialog");
        }

        private void AddUploadedFile(string name)
        {
            Border border = new Border();
            border.CornerRadius = new CornerRadius(10);
            border.Background = Brushes.LightGray;
            border.Padding = new Thickness(10, 3, 10, 3);
            border.Margin = new Thickness(5);

            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;

            Label label = new Label();
            label.Content = name;
            label.FontSize = 16;
            label.Padding = new Thickness(0);

            Button button = new Button();
            button.Padding = new Thickness(0);
            button.Background = Brushes.Transparent;
            button.BorderBrush = Brushes.Transparent;
            button.BorderThickness = new Thickness(0);
            button.Height = 15;
            button.Width = 15;
            button.Margin = new Thickness(5, 2, 0, 0);

            Border border1 = new Border();
            border1.Background = Brushes.DarkGray;
            border1.CornerRadius = new CornerRadius(10);

            MaterialDesignThemes.Wpf.PackIcon icon = new MaterialDesignThemes.Wpf.PackIcon();
            icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Close;
            icon.Width = 12;
            icon.Height = 12;

            border1.Child = icon;
            button.Content = border1;
            button.Click += ButtonRemoveTo_Click;
            panel.Children.Add(label);
            panel.Children.Add(button);
            border.Child = panel;
            PanelFileList.Children.Add(border);
        }

        private void ButtonRemoveTo_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StackPanel panel = (StackPanel)button.Parent;
            Border border = (Border)panel.Parent;
            PanelFileList.Children.Remove(border);
        }

        private void ComboID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoadingUI) return;
            ComboBox comboId = (ComboBox)sender;
            Identification item = null;
            foreach (Identification row in IdentificationType)
            {
                if (row.Name == comboId.SelectedItem.ToString())
                {
                    item = row;
                    break;
                }
            }

            Grid grid = (Grid)comboId.Parent;
            TextBox textBox = ((TextBox)grid.Children[2]);
            textBox.Visibility = item.Points == null ? Visibility.Collapsed : Visibility.Visible;
            if (item.Points != null) textBox.Text = item.Points.Value + "";

            StackPanel panel = (StackPanel)grid.Parent;
            Button button = Global.FindChild<Button>(panel, "ButtonIdSave");
            panel = (StackPanel)panel.Children[2];

            DatePicker dateIssue = (DatePicker)panel.Children[0];
            DatePicker dateExpiry = (DatePicker)panel.Children[1];

            dateIssue.Visibility = item.HasIssueDate ? Visibility.Visible : Visibility.Collapsed;
            dateExpiry.Visibility = item.HasExpiryDate ? Visibility.Visible : Visibility.Collapsed;

            button.IsEnabled = comboId.SelectedIndex != -1 && ((TextBox)grid.Children[1]).Text != "" &&
                (dateIssue.Visibility == Visibility.Collapsed || (dateIssue.Visibility == Visibility.Visible && dateIssue.SelectedDate != null)) &&
                (dateExpiry.Visibility == Visibility.Collapsed || (dateExpiry.Visibility == Visibility.Visible && dateExpiry.SelectedDate != null));

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoadingUI) return;
            TextBox textBox = (TextBox)sender;
            Grid grid = (Grid)textBox.Parent;

            ComboBox comboName = (ComboBox)grid.Children[0];
            TextBox textValue = (TextBox)grid.Children[1];

            StackPanel panel = (StackPanel)grid.Parent;
            Button button = Global.FindChild<Button>(panel, "ButtonIdSave");
            panel = (StackPanel)panel.Children[2];

            DatePicker dateIssue = (DatePicker)panel.Children[0];
            DatePicker dateExpiry = (DatePicker)panel.Children[1];

            button.IsEnabled = comboName.SelectedIndex != -1 && textValue.Text != "" &&
                (dateIssue.Visibility == Visibility.Collapsed || (dateIssue.Visibility == Visibility.Visible && dateIssue.SelectedDate != null)) &&
                (dateExpiry.Visibility == Visibility.Collapsed || (dateExpiry.Visibility == Visibility.Visible && dateExpiry.SelectedDate != null));
        }


        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoadingUI) return;
            DatePicker datePicker = (DatePicker)sender;
            StackPanel panel = (StackPanel)datePicker.Parent;

            DatePicker dateIssue = (DatePicker)panel.Children[0];
            DatePicker dateExpiry = (DatePicker)panel.Children[1];

            panel = (StackPanel)panel.Parent;
            Button button = Global.FindChild<Button>(panel, "ButtonIdSave");
            Grid grid = (Grid)panel.Children[1];

            ComboBox comboName = (ComboBox)grid.Children[0];
            TextBox textValue = (TextBox)grid.Children[1];

            button.IsEnabled = comboName.SelectedIndex != -1 && textValue.Text != "" &&
                (dateIssue.Visibility == Visibility.Collapsed || (dateIssue.Visibility == Visibility.Visible && dateIssue.SelectedDate != null)) &&
                (dateExpiry.Visibility == Visibility.Collapsed || (dateExpiry.Visibility == Visibility.Visible && dateExpiry.SelectedDate != null));
        }

        private void ButtonIDUpload_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StackPanel panel = (StackPanel)button.Parent;
            panel = (StackPanel)panel.Parent;
            WrapPanel label = (WrapPanel)panel.Children[3];

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                MessageUtils.ShowMessage("", Properties.Resources.Upload_ID_File);
                string[] names = openFileDialog.FileNames;
                foreach (string s in names)
                {
                    string str = s.Substring(s.LastIndexOf("\\") + 1);
                    AddUploadedFile(str);
                }
            }
        }

        private void LoadDocumentsData()
        {

        }

        private void ButtonDocumentAdd_Click(object sender, RoutedEventArgs e)
        {
            BuildDocumentView();
        }

        private void ButtonDocumentEdit_Click(object sender, RoutedEventArgs e)
        {
            long id = Convert.ToInt64(((Button)sender).Tag.ToString());
            List<DocumentModel.Detail> list = (List<DocumentModel.Detail>)ListDocuments.ItemsSource;

            int index = 0;
            foreach (DocumentModel.Detail item in list)
            {
                index++;
                if (item.Id == id)
                {
                    BuildDocumentView(item);
                    ListDocuments.SelectedIndex = index - 1;
                    break;
                }
            }
        }

        private void ButtonDocumentDelete_Click(object sender, RoutedEventArgs e)
        {
            long id = Convert.ToInt64(((Button)sender).Tag.ToString());
            List<DocumentModel.Detail> list = (List<DocumentModel.Detail>)ListDocuments.ItemsSource;

            foreach (DocumentModel.Detail item in list)
            {
                if (item.Id == id)
                {
                    list.Remove(item);
                    break;
                }
            }

            ListDocuments.ItemsSource = new List<DocumentModel.Detail>();
            ListDocuments.ItemsSource = list;
        }

        private async void BuildDocumentView(DocumentModel.Detail item = null)
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;
            panel.Margin = new Thickness(10);
            panel.Width = 800;

            Label label = new Label();
            label.Content = item == null ? Properties.Resources.Add_Document : Properties.Resources.Update_Document;
            label.Margin = new Thickness(10);
            label.FontSize = 20;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.FontWeight = FontWeights.Bold;
            panel.Children.Add(label);

            Grid grid = new Grid();
            ColumnDefinition column = new ColumnDefinition();
            column.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column1 = new ColumnDefinition();
            column1.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column2 = new ColumnDefinition();
            column2.Width = new GridLength(1, GridUnitType.Star);

            grid.ColumnDefinitions.Add(column);
            grid.ColumnDefinitions.Add(column1);
            grid.ColumnDefinitions.Add(column2);

            TextBox text1 = new TextBox();
            text1.Margin = new Thickness(10);
            text1.FontSize = 16;
            if (item != null) text1.Text = item.Author;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(text1, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(text1, Properties.Resources.Author);
            grid.Children.Add(text1);
            Grid.SetColumn(text1, 0);

            DatePicker datePicker1 = new DatePicker();
            datePicker1.Margin = new Thickness(10);
            datePicker1.FontSize = 16;
            if (item != null) datePicker1.SelectedDate = DateTime.Parse(item.DateAuthored);
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(datePicker1, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(datePicker1, Properties.Resources.Date_Authored);
            Grid.SetColumn(datePicker1, 1);
            grid.Children.Add(datePicker1);

            TextBox text2 = new TextBox();
            text2.Margin = new Thickness(10);
            text2.FontSize = 16;
            if (item != null) text2.Text = item.Note;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(text2, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(text2, Properties.Resources.Note);
            grid.Children.Add(text2);
            Grid.SetColumn(text2, 2);

            Grid grid1 = new Grid();
            ColumnDefinition column3 = new ColumnDefinition();
            column2.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column4 = new ColumnDefinition();
            column3.Width = new GridLength(1, GridUnitType.Star);

            grid1.ColumnDefinitions.Add(column3);
            grid1.ColumnDefinitions.Add(column4);

            TextBox text3 = new TextBox();
            text3.Margin = new Thickness(10);
            text3.FontSize = 16;
            if (item != null)
            {
                text3.Text = item.Name;
                text3.Tag = item.FileType;
            }
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(text3, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(text3, Properties.Resources.Name);
            grid1.Children.Add(text3);
            Grid.SetColumn(text3, 0);

            Button buttonUpload = new Button();
            buttonUpload.Content = Properties.Resources.Upload_File;
            buttonUpload.HorizontalAlignment = HorizontalAlignment.Left;
            buttonUpload.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            buttonUpload.Margin = new Thickness(10);
            buttonUpload.Click += ButtonDocumentUpload_Click;
            grid1.Children.Add(buttonUpload);
            Grid.SetColumn(buttonUpload, 1);

            StackPanel panel1 = new StackPanel();
            panel1.Orientation = Orientation.Horizontal;
            panel1.HorizontalAlignment = HorizontalAlignment.Right;
            panel1.Margin = new Thickness(0, 10, 20, 0);

            Button buttonAdd = new Button();
            buttonAdd.Content = item == null ? Properties.Resources.Add : Properties.Resources.Update;
            buttonAdd.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            buttonAdd.Margin = new Thickness(0, 0, 10, 0);
            buttonAdd.Width = 100;
            buttonAdd.Click += ButtonDocumentSave_Click;
            panel1.Children.Add(buttonAdd);

            Button buttonClose = new Button();
            buttonClose.Content = Properties.Resources.Close;
            buttonClose.Tag = "DetailDialog";
            buttonClose.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            buttonClose.Margin = new Thickness(0, 0, 10, 0);
            buttonClose.Width = 100;
            buttonClose.Click += ButtonAttributesClose_Click;
            panel1.Children.Add(buttonClose);

            panel.Children.Add(grid);
            panel.Children.Add(grid1);
            panel.Children.Add(panel1);

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog("DetailDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(panel, "DetailDialog");
        }

        private void ButtonDocumentUpload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                MessageUtils.ShowMessage("", Properties.Resources.Upload_Doc_File);
                Button button = (Button)sender;
                string fileName = openFileDialog.FileName;
                fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                Grid grid = (Grid)button.Parent;
                TextBox textBox = (TextBox)grid.Children[0];
                textBox.Text = fileName.LastIndexOf(".") == -1 ? fileName : fileName.Substring(0, fileName.LastIndexOf("."));
                textBox.Tag = fileName.LastIndexOf(".") == -1 ?  "" : fileName.Substring(fileName.LastIndexOf(".") + 1);
            }
        }

        private void ButtonDocumentSave_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string name = button.Content.ToString();
            StackPanel panel = (StackPanel)button.Parent;
            panel = (StackPanel)panel.Parent;

            DocumentModel.Detail item = new DocumentModel.Detail();
            item.Id = new Random().Next(1000, 9999);

            Grid grid = (Grid)panel.Children[1];
            TextBox textAuthor = (TextBox)grid.Children[0];
            DatePicker dateAuthor = (DatePicker)grid.Children[1];
            TextBox textNote = (TextBox)grid.Children[2];

            if (textAuthor.Text == "" || dateAuthor.SelectedDate == null || textNote.Text == "") return;
            item.Author = textAuthor.Text;
            item.DateAuthored = dateAuthor.SelectedDate.Value.ToString("yyyy-MM-dd hh:mm:ss");
            item.Note = textNote.Text;

            grid = (Grid)panel.Children[2];
            TextBox textName = (TextBox)grid.Children[0];
            if (textName.Text == "") return;
            item.Name = textName.Text;
            item.FileType = textName.Tag == null ? "" : textName.Tag.ToString();

            item.Category = "User";
            item.Created = item.DateAuthored;
            item.Updated = item.DateAuthored;
            item.CreatedBy = "40013040";
            item.UpdatedBy = "40013040";

            List<DocumentModel.Detail> list = (List<DocumentModel.Detail>)ListDocuments.ItemsSource ?? new List<DocumentModel.Detail>();

            if (name == "Add") list.Add(item);
            else list[ListDocuments.SelectedIndex] = item;

            ListDocuments.ItemsSource = new List<DocumentModel.Detail>();
            ListDocuments.ItemsSource = list;
            Global.CloseDialog("DetailDialog");

        }

        private void LoadNotificationData()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, bool> result = Global.RequestAPI<Dictionary<string, bool>>(Constants.NotificationConfigURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    null, Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    ToggleBills.IsChecked = result["Bills"];
                    ToggleUsage.IsChecked = result["Usage"];
                    ToggleLogin.IsChecked = result["Login"];
                    ToggleOffers.IsChecked = result["Offers"];
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private void TextPhoneOrEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            StackPanel panel = (StackPanel)textBox.Parent;
            ProgressBar progressBar = (ProgressBar)panel.Children[1];

            string url = Constants.ValidatePhone;
            string url1 = Constants.CheckEmailURL;
            string param = textBox.Text;
            if (param == "") return;
            string tag = textBox.Tag.ToString();
            progressBar.Visibility = Visibility.Visible;

            if (tag == "Login Id")
            {
                url = Constants.CheckLoginIdURL;
                CheckUniqueAuthInfo(textBox, url, param, tag, progressBar);
                return;
            }
            else if (tag == Properties.Resources.Authenticate_Email)
            {
                url = Constants.ValidateEmail;
                url1 = Constants.CheckEmailURL;
            }
            else if (tag == Properties.Resources.Authenticate_Mobile)
            {
                url = Constants.ValidateSMS;
                url1 = Constants.CheckMobileURL;
            }

            EasyTimer.SetTimeout(() =>
            {

                ValidModel result = Global.RequestAPI<ValidModel>(url, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("Parameter", param),
                     Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result.Valid)
                    {
                        CheckUniqueAuthInfo(textBox, url1, param, tag, progressBar);
                    }
                    else
                    {
                        MessageUtils.ShowErrorMessage("", param + " : " + string.Format(Properties.Resources.Invalid_Message, tag));
                        progressBar.Visibility = Visibility.Collapsed;
                        textBox.Text = "";
                        textBox.Focus();
                    }
                });

            }, 10);
        }

        private void CheckUniqueAuthInfo(TextBox textBox, string url, string param, string tag, ProgressBar progressBar)
        {
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, bool> result = Global.RequestAPI<Dictionary<string, bool>>(url, Method.Get, Global.GetHeader(Global.CustomerToken), 
                    Global.BuildDictionary("Parameter", param), Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    progressBar.Visibility = Visibility.Collapsed;
                    if (result == null || result["Used"])
                    {
                        MessageUtils.ShowMessage("", param + " : " + string.Format(Properties.Resources.Choose_Another_Message, tag));
                        textBox.Text = "";
                        textBox.Focus();
                    }
                    else MessageUtils.ShowMessage("", param + " : " + string.Format(Properties.Resources.Available_Message, tag));
                });

            }, 10);

        }

        private void ButtonCheck_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string tag = button.Tag.ToString();

            string url = Constants.ValidateEmail;
            string url1 = Constants.CheckEmailURL;
            TextBox textBox = TextLoginEmail;
            ProgressBar progressBar = ProgressLoginEmail;

            if (tag == "Email")
            {
                tag = Properties.Resources.Authenticate_Email;
                url = Constants.ValidateEmail;
                url1 = Constants.CheckEmailURL;
                textBox = TextLoginEmail;
                progressBar = ProgressLoginEmail;
            }
            else if (tag == "Mobile")
            {
                tag = Properties.Resources.Authenticate_Mobile;
                url = Constants.ValidateSMS;
                url1 = Constants.CheckMobileURL;
                textBox = TextLoginMobile;
                progressBar = ProgressLoginMobile;
            }
            else if (tag == "Login")
            {
                tag = Properties.Resources.Login_id;
                url1 = Constants.CheckLoginIdURL;
                textBox = TextLoginId;
                progressBar = ProgressLoginId;
            }

            string param = textBox.Text;
            if (param == "") return;
            progressBar.Visibility = Visibility.Visible;

            if (tag == Properties.Resources.Login_id)
            {
                CheckUniqueAuthInfo(textBox, url1, param, tag, progressBar);
            }
            else
            {
                EasyTimer.SetTimeout(() =>
                {
                    ValidModel result = Global.RequestAPI<ValidModel>(url, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("Parameter", param), 
                        Global.BuildDictionary("api-version", 1.0), "");

                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        if (result.Valid)
                        {
                            CheckUniqueAuthInfo(textBox, url1, param, tag, progressBar);
                        }
                        else
                        {
                            MessageUtils.ShowErrorMessage("", param + " : " + string.Format(Properties.Resources.Invalid_Message, tag));
                            progressBar.Visibility = Visibility.Collapsed;
                            textBox.Text = "";
                            textBox.Focus();
                        }
                    });

                }, 10);
            }

        }

        private void TextPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            string password = TextPassword.Password;
            if (password == "") return;

            ProgressComplex.Visibility = Visibility.Visible;
            BorderComplex.Visibility = Visibility.Collapsed;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("password", password);
                ComplexResult = Global.RequestAPI<ComplexResponse>(Constants.ComplexURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    ProgressComplex.Visibility = Visibility.Collapsed;
                    LabelComplex.Content = ComplexResult.Result == "SUCCESS" ? string.Format(Properties.Resources.Password_Strength, ComplexResult.PasswordStrength) : ComplexResult.Reason;
                    BorderComplex.Visibility = Visibility.Visible;

                    Brush[] brushes = new Brush[] { Brushes.Red, Brushes.Red, Brushes.Yellow, Brushes.Green };
                    Brush[] brushes1 = new Brush[] { Brushes.White, Brushes.White, Brushes.Black, Brushes.Black };
                    List<string> type = new List<string> { "Unacceptable", "Weak", "Medium", "Strong" };
                    BorderComplex.Background = brushes[type.IndexOf(ComplexResult.PasswordStrength)];
                    LabelComplex.Foreground = brushes1[type.IndexOf(ComplexResult.PasswordStrength)];
                });

            }, 10);
        }

        private void Suggestion_Password_Click(object sender, RoutedEventArgs e)
        {
            ProgressComplex.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> SuggestionPassword = Global.RequestAPI<Dictionary<string, string>>(Constants.SuggestopnURL, Method.Get, 
                    Global.GetHeader(Global.CustomerToken), null, Global.BuildDictionary("api-version", 1.0), "");
                if (SuggestionPassword == null || SuggestionPassword["Password"] == null) return;

                Application.Current.Dispatcher.Invoke(async delegate
                {
                    ProgressComplex.Visibility = Visibility.Hidden;
                    bool result = await MessageUtils.ConfirmMessageAsync(Properties.Resources.Alert, string.Format("{0}\n{1}", Properties.Resources.Suggestion_Password,
                        SuggestionPassword["Password"]));

                    if (result)
                    {
                        TextPassword.Password = SuggestionPassword["Password"];
                        LabelComplex.Content = string.Format(Properties.Resources.Password_Strength, ComplexResult.PasswordStrength);
                        BorderComplex.Visibility = Visibility.Visible;
                        ComplexResult.PasswordStrength = "Strong";
                        ComplexResult.Reason = "";
                        ComplexResult.Result = "SUCCESS";

                        BorderComplex.Background = Brushes.Green;
                        LabelComplex.Foreground = Brushes.Black;
                    }
                });

            }, 10);
        }

        private void LoadPaymentMethodData()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.1);
                query.Add("Open", true);
                query.Add("DefaultOnly", false);
                List<PaymentMethod> list = Global.RequestAPI<List<PaymentMethod>>(Constants.AvailablePaymentMethodURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    null, query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (list == null) return;

                    List<PaymentMethod> creditCardList = new List<PaymentMethod>();
                    List<PaymentMethod> bankTransferList = new List<PaymentMethod>();

                    foreach (PaymentMethod item in list)
                    {
                        if (item.CategoryCode == "C") creditCardList.Add(item);
                        else if (item.CategoryCode == "D") bankTransferList.Add(item);
                    }

                    ListCreditCard.ItemsSource = creditCardList;
                    ListBankTransfer.ItemsSource = bankTransferList;
                });

            }, 10);
        }

        private void Button_CreditCard_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Bank_Add_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
