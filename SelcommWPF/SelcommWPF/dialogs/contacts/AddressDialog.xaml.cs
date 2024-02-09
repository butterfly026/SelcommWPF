using RestSharp;
using SelcommWPF.clients.models.contacts;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// Interaction logic for AddressDialog.xaml
    /// </summary>
    public partial class AddressDialog : UserControl
    {
        private AddressModel AddressData = new AddressModel();
        private List<AddressModel.AddressUsage> AddressUsageList;

        private List<string> CountriesStringList = new List<string>();
        private List<string> CountriesCodeList = new List<string>();
        private List<AddressModel.Country> CountriesList = new List<AddressModel.Country>();

        private List<string> AddressTypeList = new List<string>();
        private List<string> AddressTypeCodeList = new List<string>();

        private List<string> AddressMandatoryRules = new List<string>();
        private List<string> AddressMandatoryRulesCode = new List<string>();

        private List<string> AddressTypeCheck = new List<string>();
        private string[] FormatTypeList = new string[] { Properties.Resources.Search, Properties.Resources.Manual, Properties.Resources.Extended };

        private IDisposable RequestTimer = null;
        private bool SelectedAddressStatus = false;

        public AddressDialog()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.1);
                query.Add("IncludeTypes", true);
                query.Add("IncludeMandatoryRules", true);
                AddressData = Global.RequestAPI<AddressModel>(Constants.ContactAddressURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), query, "");
                CountriesList = Global.RequestAPI<List<AddressModel.Country>>(Constants.AddressCountriesURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, Global.BuildDictionary("api-version", 1.1), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (AddressData == null || CountriesList == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    if (AddressData.ContactAddressMandatoryRules != null)
                    {
                        foreach (AddressModel.AddressMandatoryRule item in AddressData.ContactAddressMandatoryRules)
                        {
                            AddressMandatoryRules.Add(item.Type);
                            AddressMandatoryRulesCode.Add(item.TypeCode);
                        }
                    }

                    if (AddressData.ContactAddressTypes != null)
                    {
                        foreach (AddressModel.AddressType item in AddressData.ContactAddressTypes)
                        {
                            AddressTypeList.Add(item.Name);
                            AddressTypeCodeList.Add(item.Code);
                        }
                    }

                    foreach (AddressModel.Country item in CountriesList)
                    {
                        CountriesStringList.Add(item.Name);
                        CountriesCodeList.Add(item.Code);
                    }


                    AddressUsageList = new List<AddressModel.AddressUsage>();
                    foreach (AddressModel.AddressUsage item in AddressData.ContactAddressUsage)
                    {
                        List<AddressModel.AddressType> list = item.AddressTypes;
                        int count = list.Count;
                        for (int i = 0; i < count; i++)
                        {
                            AddressModel.AddressUsage address = item;
                            address.AddressTypes = new List<AddressModel.AddressType>();
                            address.AddressTypes.Add(list[i]);
                            AddAddressInfo(AddressTypeCheck.Count, address);
                            AddressTypeCheck.Add(list[i].Name);
                        }
                    }

                });

            }, 10);
        }

        private async void ButtonHistory_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("SearchAddress")) Global.CloseDialog("SearchAddress");
            await MaterialDesignThemes.Wpf.DialogHost.Show(new AddressHistory(), "SearchAddress");
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            int count = PanelAddress.Children.Count;
            List<AddressModel.AddressUsage> list = new List<AddressModel.AddressUsage>();
           
            for (int i = 0; i < count; i++)
            {
                AddressModel.AddressUsage address = new AddressModel.AddressUsage();
                StackPanel panel = (StackPanel)PanelAddress.Children[i];

                Grid grid = (Grid)panel.Children[0];
                ComboBox comboAddressType = (ComboBox)grid.Children[0];

                AddressModel.AddressType type = new AddressModel.AddressType();
                type.Code = AddressTypeCodeList[comboAddressType.SelectedIndex];
                address.AddressTypes = new List<AddressModel.AddressType>() { type };

                grid = (Grid)panel.Children[1];
                panel = (StackPanel)grid.Children[1];

                address.AddressLine1 = ((TextBox)((Grid)panel.Children[0]).Children[0]).Text;
                address.AddressLine2 = ((TextBox)((Grid)panel.Children[0]).Children[1]).Text;
                address.PostCode = ((TextBox)((Grid)panel.Children[1]).Children[0]).Text;

                ComboBox comboSuburb = (ComboBox)((Grid)panel.Children[1]).Children[1];
                address.Suburb = comboSuburb.SelectedItem == null ? comboSuburb.Tag.ToString() : comboSuburb.SelectedItem.ToString();

                address.City = ((TextBox)((Grid)panel.Children[2]).Children[0]).Text;
                address.State = ((ComboBox)((Grid)panel.Children[2]).Children[1]).SelectedItem.ToString();
                address.CountryCode = CountriesCodeList[((ComboBox)((Grid)panel.Children[3]).Children[0]).SelectedIndex];

                list.Add(address);
            }


            int length = AddressMandatoryRules.Count;
            for (int i = 0; i < length; i++)
            {
                bool isContains = false;
                for (int j = 0; j < count; j++)
                {
                    if (AddressMandatoryRulesCode[i] == list[j].AddressTypes[0].Code)
                    {
                        isContains = true;
                        break;
                    }
                }

                if (!isContains)
                {
                    MessageUtils.ShowErrorMessage("", Properties.Resources.Must_have + AddressMandatoryRules[i]);
                    return;
                }

            }

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> body = Global.BuildDictionary("deleteOtherAddresses", false);
                body.Add("contactAddressUsage", list);
                HttpStatusCode result = Global.RequestAPI(Constants.AddressUpdateURL, Method.Post, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.1), body);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result == HttpStatusCode.OK)
                    {
                        MessageUtils.ShowMessage("", Properties.Resources.Address_Update);
                        Global.mainWindow.GetDisplayDetails();
                        Global.CloseDialog();
                    }
                });

            }, 10);

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
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

            ComboBox combo = new ComboBox();
            combo.Margin = new Thickness(10);
            combo.FontSize = 16;
            combo.SelectedIndex = address == null ? -1 : AddressTypeList.IndexOf(address.AddressTypes[0].Name);

            int count = AddressTypeList.Count;
            for (int i = 0; i< count; i++)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = AddressTypeList[i];
                item.IsEnabled = AddressTypeCheck.IndexOf(AddressTypeList[i]) == -1;
                combo.Items.Add(item);
            }

            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(combo, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(combo, Properties.Resources.Address_Type + " *");
            combo.IsEnabled = address == null;
            grid.Children.Add(combo);
            Grid.SetColumn(combo, 0);

            ComboBox combo1 = new ComboBox();
            combo1.ItemsSource = FormatTypeList;
            combo1.Margin = new Thickness(10);
            combo1.FontSize = 16;
            combo1.SelectedIndex = 0;
            combo1.SelectionChanged += ComboBox_SelectionChanged;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(combo1, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(combo1, Properties.Resources.Address_Format + " *");
            grid.Children.Add(combo1);
            Grid.SetColumn(combo1, 1);

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
            text.LostFocus += TextBox_LostFocus;
            text.KeyUp += TextBoxSearch_KeyDown;

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
            button.Click += ButtonDelete_Click;
            grid1.Children.Add(button);
            Grid.SetColumn(button, 1);

            panel.Children.Add(grid);
            panel.Children.Add(grid1);
            PanelAddress.Children.Add(panel);

            textPostalCode.Text = address == null ? "" : address.PostCode;
            comboCountry.SelectedIndex = address == null ? -1 : CountriesStringList.IndexOf(address.Country);
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            PanelAddress.Children.Remove(((StackPanel)((Grid)button.Parent).Parent));
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
                List<Dictionary<string, object>> result = Global.RequestAPI<List<Dictionary<string, object>>>(Constants.PostCodeURL, Method.Get, Global.GetHeader(Global.CustomerToken), path, Global.BuildDictionary("api-version", 1.1), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    List<string> suburbList = new List<string>();
                    foreach (Dictionary<string, object> item in result) suburbList.Add(item["LocalityName"].ToString());
                    comboSuburb.ItemsSource = suburbList;
                    comboSuburb.SelectedIndex = suburbList.IndexOf(comboSuburb.Tag.ToString());
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
                List<AddressModel.Country> result = Global.RequestAPI<List<AddressModel.Country>>(Constants.AddressStateURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.1), "", false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null) return;

                    List<string> stateList = new List<string>();
                    foreach (AddressModel.Country item in result) stateList.Add(item.Code);
                    ComboState.ItemsSource = stateList;
                    ComboState.SelectedIndex = stateList.IndexOf(ComboState.Tag.ToString());
                });

            }, 10);

        }

        private void TextBoxSearch_KeyDown(object sender, KeyEventArgs e)
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
                List<Dictionary<string, string>> result = Global.RequestAPI<List<Dictionary<string, string>>>(Constants.AutoCompleteURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    progress.Visibility = Visibility.Collapsed;
                    List<string> addresses = new List<string>();
                    if (result != null) foreach (Dictionary<string, string> item in result) addresses.Add(item["Address"]);
                    listView.ItemsSource = new List<string>();
                    listView.ItemsSource = addresses;
                    card.Visibility = addresses.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                    ScrollView.ScrollToBottom();
                    RequestTimer.Dispose();
                    RequestTimer = null;
                });

            }, 2000);
        }

        private void ButtonDialogClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MaterialDesignThemes.Wpf.DialogHost.Close("SearchAddress");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SelectedAddressStatus = false;
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
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("Term", address);
                Dictionary<string, string> result = Global.RequestAPI<Dictionary<string, string>>(Constants.ParseAddressURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, "");

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

    }
}
