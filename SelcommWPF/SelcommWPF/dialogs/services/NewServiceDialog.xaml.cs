using Newtonsoft.Json;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.contacts;
using SelcommWPF.clients.models.report;
using SelcommWPF.clients.models.services;
using SelcommWPF.dialogs.contacts;
using SelcommWPF.dialogs.plans;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SelcommWPF.dialogs.accounts
{
    /// <summary>
    /// Interaction logic for NewServiceDialog.xaml
    /// </summary>
    public partial class NewServiceDialog : UserControl
    {
        public static NewServiceDialog Instance;
        private bool IsAuthrisation = false;
        public int DefinitionId = 0;

        private bool IsLoadedDetail = false;
        private List<ServicesType.Item> ServiceTypesList = new List<ServicesType.Item>();
        private List<Dictionary<string, object>> ServiceStatusList = new List<Dictionary<string, object>>();

        private string CheckString = "";
        private bool IsLoadedAttributes = false;
        private List<Dictionary<string, object>> ServiceAttrDefinitions = new List<Dictionary<string, object>>();

        private bool IsLoadedPlan = false;
        private bool SelectedPlanData = false;
        private ServicesPlans.Item ServicePlansData = new ServicesPlans.Item();

        private bool IsLoadedAddress = false;
        private string[] FormatTypeList = new string[] { Properties.Resources.Search, Properties.Resources.Manual, Properties.Resources.Extended };
        private List<AddressModel.AddressType> AddressType = new List<AddressModel.AddressType>();
        private List<string> AddressTypeList = new List<string>();
        private List<string> AddressTypeCodeList = new List<string>();
        private List<string> CountriesStringList = new List<string>();
        private List<string> CountriesCodeList = new List<string>();
        private List<AddressModel.Country> CountriesList = new List<AddressModel.Country>();

        private IDisposable RequestTimer = null;
        private bool SelectedAddressStatus = false;

        private bool IsLoadedContact = false;
        private List<string> TitlesList = new List<string>();
        private List<RelatedContactModel.Relationship> RelationShipList = new List<RelatedContactModel.Relationship>();
        private List<string> RelationTypeList = new List<string>();

        public NewServiceDialog()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            Instance = this;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.servicesClient.GetAuthrisations(Constants.AuthrisationURL, "/ServiceDesk/Services/New");
                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result == HttpStatusCode.OK)
                    {
                        IsAuthrisation = true;
                        LoadServiceDetail();
                    }
                    else
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources1.Feature_Error);
                        Global.CloseDialog();
                    }
                });

            }, 10);
        }

        private void ShowHideVerticalScroll(bool isChecked)
        {
            ListView[] listViews = new ListView[] { ListAttributes };
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

            ScrollViewer[] scrolls = new ScrollViewer[] { ScrollViewAddress, ScrollViewContact };
            count = scrolls.Length;

            for (int i = 0; i < count; i++) if (scrolls[i] == null) return;
            for (int i = 0; i < count; i++) scrolls[i].VerticalScrollBarVisibility = isChecked ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            int index = Convert.ToInt32(((Button)sender).Tag);
            TabServices.SelectedIndex = index + 1;
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
            int index = Convert.ToInt32(((Button)sender).Tag);
            TabServices.SelectedIndex = index - 1;
        }

        private void TabServices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                TabItem item = (TabItem)TabServices.Items[TabServices.SelectedIndex];
                string header = item.Header.ToString();

                switch (header)
                {
                    case "Details":
                        if (!IsLoadedDetail) LoadServiceDetail();
                        break;
                    case "Attributes":
                        if (!IsLoadedAttributes) LoadAttributesData();
                        break;
                    case "Plans":
                        if (!IsLoadedPlan) LoadServicePlansData();
                        break;
                    case "Addresses":
                        if (!IsLoadedAddress) LoadAddressesData();
                        break;
                    case "Site":
                        break;
                    case "Cost Centres":
                        break;
                    case "Service Groups":
                        break;
                    case "Network Elements":
                        break;
                    case "Related Contacts":
                        if (!IsLoadedContact) LoadRelatedContactData();
                        break;
                    case "Documents":
                        break;
                    case "Notifications":
                        break;
                }
            }
        }

        private void LoadServiceDetail()
        {
            if (!IsAuthrisation) return;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                ServiceTypesList = Global.servicesClient.GetServiceTypes(Constants.ServiceTypesURL);
                ServiceStatusList = Global.servicesClient.GetServiceAttrDefinitions(Constants.ServiceStatusURL);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (ServiceTypesList == null || ServiceStatusList == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    List<string> list = new List<string>();
                    foreach (ServicesType.Item item in ServiceTypesList) list.Add(item.Name);

                    List<string> list1 = new List<string>();
                    foreach (Dictionary<string, object> item in ServiceStatusList) list1.Add(item["Status"].ToString());

                    ComboServiceType.ItemsSource= list;
                    ComboStatus.ItemsSource= list1;
                    IsLoadedDetail = true;
                    DatePickerConnected.SelectedDate = DateTime.Now;
                });

            }, 10);
        }

        private void ComboServiceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Visibility visibility = ComboServiceType.SelectedIndex == -1 ? Visibility.Hidden : Visibility.Visible;
            ButtonSQ.Visibility = visibility;
            TextServiceNumber.Visibility = visibility;
            ComboStatus.Visibility = visibility;
            TextServiceNarrative.Visibility = visibility;
            DatePickerConnected.Visibility = visibility;
            TextEnquiryPassword.Visibility = visibility;
            ComboParents.Visibility = visibility;
            ButtonPreferred.Visibility = visibility;
            IsLoadedAttributes = false;

            string serviceTypeId = ServiceTypesList[ComboServiceType.SelectedIndex].Id;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> result = Global.servicesClient.GetServiceConfigData(Constants.ServiceConfigURL, serviceTypeId);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;

                    if (result == null || result.Count == 0) GetDefaultConfiguration();
                    else
                    {
                        ButtonNext.Visibility = Visibility.Visible;
                        int count = TabServices.Items.Count;

                        for (int i = 0; i < count; i++)
                        {
                            TabItem item = TabServices.Items[i] as TabItem;
                            string key = item.Header.ToString().Replace(" ", "");
                            if (result.ContainsKey(key))
                            {
                                var value = result[key];
                                if (value == null) continue;
                                Dictionary<string, object> tabInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(value.ToString());
                                bool enabled = Convert.ToBoolean(tabInfo["Enabled"].ToString());
                                item.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
                            }

                        }
                    }

                });

            }, 10);

        }

        private void GetDefaultConfiguration()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> result = Global.servicesClient.GetServiceConfigData(Constants.ServiceConfigDefaultURL, "");
                Application.Current.Dispatcher.Invoke(delegate
                {
                    int count = TabServices.Items.Count;
                    LoadingPanel.Visibility = Visibility.Hidden;

                    if (result == null || result.Count == 0)
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources1.Config_Error);
                        for (int i = 0; i < count; i++)
                        {
                            TabItem item = TabServices.Items[i] as TabItem;
                            if (i != 0) item.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        ButtonNext.Visibility = Visibility.Visible;

                        for (int i = 0; i < count; i++)
                        {
                            TabItem item = TabServices.Items[i] as TabItem;
                            string key = item.Header.ToString().Replace(" ", "");
                            if (result.ContainsKey(key))
                            {
                                var value = result[key];
                                if (value == null) continue;
                                Dictionary<string, object> tabInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(value.ToString());
                                bool enabled = Convert.ToBoolean(tabInfo["Enabled"].ToString());
                                item.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
                            }

                        }
                    }

                });

            }, 10);
        }

        private async void ButtonSQ_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ServiceSQDialog(), "DetailDialog");
        }

        private async void ButtonPreferred_Click(object sender, RoutedEventArgs e)
        {
            if (ComboServiceType.SelectedIndex == -1) return;

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new PreferredDialog(ServiceTypesList[ComboServiceType.SelectedIndex].Id, TextServiceNumber.Text), "DetailDialog");
        }

        private void LoadAttributesData()
        {
            if (ComboServiceType.SelectedIndex == -1) return;
            LoadingPanel.Visibility = Visibility.Visible;
            string serviceTypeId = ServiceTypesList[ComboServiceType.SelectedIndex].Id;

            EasyTimer.SetTimeout(() =>
            {
                List<ReportDefinition.Parameter> list = new List<ReportDefinition.Parameter>();
                ServiceAttrDefinitions = Global.servicesClient.GetServiceAttrDefinitions(Constants.ServiceAttrDefinitionsURL, serviceTypeId);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (ServiceAttrDefinitions == null) return;

                    foreach (Dictionary<string, object> item in ServiceAttrDefinitions)
                    {
                        bool required = Convert.ToBoolean(item["Required"].ToString());
                        string name = item["Name"].ToString();
                        if (required) name += " *";
                        string dataType = item["DataType"].ToString();

                        list.Add(new ReportDefinition.Parameter()
                        {
                            Name = name,
                            DataType = dataType,
                            Default = item["DefaultValue"] == null ? "" : item["DefaultValue"].ToString(),
                            Optional = !required,
                            VisibleString = dataType == "String" ? "Visible" : "Collapsed",
                            VisibleBoolean = dataType == "Boolean" ? "Visible" : "Collapsed",
                            VisibleDate = dataType == "Date" ? "Visible" : "Collapsed",
                            VisibleInteger = dataType == "Integer" ? "Visible" : "Collapsed",
                            VisibleDecimal = dataType == "Decimal" ? "Visible" : "Collapsed",
                            VisibleCurrency = dataType == "Currency" ? "Visible" : "Collapsed"
                        });
                    }

                    ListAttributes.ItemsSource = list;
                    IsLoadedAttributes = true;
                    ShowHideVerticalScroll(Global.HasVerticalScroll);
                });

            }, 10);
        }

        private void TextAttributes_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            bool optional = Convert.ToBoolean(textBox.Tag.ToString());
            if (!optional && textBox.Text == "") MessageUtils.ShowErrorMessage("", "This field is require.");
        }

        private void TextInteger_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void TextDecimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void TextPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Name != "TextCurrency") return;
            string str = textBox.Text;

            try
            {
                string value = str.Replace("A$", "").Replace(",", "");
                Convert.ToDouble(value);    
            } 
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

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
                TextChange change = e.Changes.First();
                str = str.Replace("A$", "").Replace(",", "");
                if (change.AddedLength == 0 && change.RemovedLength > 0)
                {

                    double price = Convert.ToDouble(str) / 10;
                    str = StringUtils.ConvertCurrency(price);
                }
                else
                {

                    double price = Convert.ToDouble(str) * 10;
                    str = StringUtils.ConvertCurrency(price);
                }
            }
            else
            {
                str = StringUtils.ConvertCurrency(Convert.ToDouble(str) / 100);
            }

            CheckString = str;
            textBox.Text = str;
        }


        private void TextPlans_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ComboServiceType.SelectedIndex == -1 || SelectedPlanData) return;
            LoadingPanel.Visibility = Visibility.Visible;

            string serviceTypeId = ServiceTypesList[ComboServiceType.SelectedIndex].Id;
            string search = TextPlans.Text;

            EasyTimer.SetTimeout(() =>
            {
                List<string> list = new List<string>();
                ServicePlansData = Global.servicesClient.GetServicePlansData(Constants.ServicePlansURL, Global.ContactCode, serviceTypeId, 0, 20, search);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (ServicePlansData == null || ServicePlansData.Plans == null)
                    {
                        ListPlans.ItemsSource = new List<string>();
                        CardPlans.Visibility = Visibility.Collapsed;
                        return;
                    }

                    foreach (ServicesPlans.Detail item in ServicePlansData.Plans) list.Add(item.Plan);
                    ListPlans.ItemsSource = list;
                    CardPlans.Visibility = Visibility.Visible;
                });

            }, 10);
        }

        private void TextPlans_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                ListPlans.Focus();
                ListPlans.SelectedIndex = 0;
            }
        }

        private void ListPlans_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ListPlans.SelectedIndex == -1) return;

            SelectedPlanData = true;
            TextPlans.Text = ServicePlansData.Plans[ListPlans.SelectedIndex].Plan;
            List<ServicesPlans.Option> options = ServicePlansData.Plans[ListPlans.SelectedIndex].Options;
            options.Sort((x, y) => x.Order.CompareTo(y.Order));

            int index = 0;
            int count = options.Count;
            List<string> list = new List<string>();

            for (int i = 0; i < count; i++)
            {
                list.Add(options[i].Name);
                if (options[i].Default) index = i;
            }

            ComboOptions.ItemsSource = list;
            ComboOptions.SelectedIndex = index;
            ListPlans.ItemsSource = new List<string>();
            CardPlans.Visibility = Visibility.Collapsed;
            ButtonDetail.IsEnabled = true;
        }

        private async void ButtonPlanDetail_Click(object sender, RoutedEventArgs e)
        {
            if (DefinitionId == 0) return;

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new PlanDetail(0, DefinitionId), "DetailDialog");
        }

        private void ListPlans_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ListPlans.SelectedIndex == -1) return;

                SelectedPlanData = true;
                TextPlans.Text = ServicePlansData.Plans[ListPlans.SelectedIndex].Plan;
                List<ServicesPlans.Option> options = ServicePlansData.Plans[ListPlans.SelectedIndex].Options;
                options.Sort((x, y) => x.Order.CompareTo(y.Order));

                int index = 0;
                int count = options.Count;
                List<string> list = new List<string>();

                for (int i = 0; i < count; i++)
                {
                    list.Add(options[i].Name);
                    if (options[i].Default) index = i;
                }

                ComboOptions.ItemsSource = list;
                ComboOptions.SelectedIndex = index;
                ListPlans.ItemsSource = new List<string>();
                CardPlans.Visibility = Visibility.Collapsed;
                ButtonDetail.IsEnabled = true;
            }
        }

        private void LoadServicePlansData()
        {
            
        }

        private void LoadAddressesData()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                AddressType = Global.contactClient.GetContactAddressType(Constants.AddressTypeURL);
                CountriesList = Global.contactClient.GetContactAddressCountries(Constants.AddressCountriesURL);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    AddressTypeList = new List<string>();
                    AddressTypeCodeList = new List<string>();
                    CountriesCodeList = new List<string>();
                    CountriesStringList = new List<string>();

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

                    IsLoadedAddress = true;
                    ShowHideVerticalScroll(Global.HasVerticalScroll);
                    LoadingPanel.Visibility = Visibility.Hidden;
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
            combo.SelectionChanged += ComboBoxAddressType_SelectionChanged;
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

                List<Dictionary<string, string>> result = Global.contactClient.AutoCompleteAustralia(Constants.AutoCompleteURL, search);
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

            try
            {
                MaterialDesignThemes.Wpf.DialogHost.Close("SearchAddress");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> result = Global.contactClient.ParseAddressAustralia(Constants.ParseAddressURL, address);
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

                List<AddressModel.Country> result = Global.contactClient.GetAddressStates(Constants.AddressStateURL, countryCode);
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

                List<Dictionary<string, object>> result = Global.contactClient.GetAddressPostCode(Constants.PostCodeURL, "AU", postalCode);
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


        private void ComboBoxAddressType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void LoadRelatedContactData()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                List<Dictionary<string, string>> titles = Global.contactClient.GetAliasTypesAndTitles(Constants.ContactTitlesURL);
                RelationShipList = Global.contactClient.GetRelationShipList(Constants.RelationShipURL);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    TitlesList = new List<string>();
                    RelationTypeList = new List<string>();

                    foreach (Dictionary<string, string> item in titles) TitlesList.Add(item["Name"]);
                    foreach (RelatedContactModel.Relationship item in RelationShipList) RelationTypeList.Add(item.Name);

                    ShowHideVerticalScroll(Global.HasVerticalScroll);
                    IsLoadedContact = true;
                    LoadingPanel.Visibility = Visibility.Hidden;
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

    }
}
