using RestSharp;
using SelcommWPF.clients.models.contacts;
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
    /// Interaction logic for PhoneDialog.xaml
    /// </summary>
    public partial class PhoneDialog : UserControl
    {
        private PhoneModel PhoneData = new PhoneModel();
        private List<string> PhoneTypeList = new List<string>();

        private List<string> PhoneMandatoryRule = new List<string>();
        private List<string> PhoneMandatoryRuleCode = new List<string>();

        public PhoneDialog()
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
                PhoneData = Global.RequestAPI<PhoneModel>(Constants.ContactPhoneURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (PhoneData == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    foreach (PhoneModel.PhoneType item in PhoneData.ContactPhoneTypes) PhoneTypeList.Add(item.Name);
                    foreach (PhoneModel.PhoneUsage item in PhoneData.ContactPhoneUsages) AddPhoneNumber(item);
                    foreach (PhoneModel.PhoneMandatoryRule item in PhoneData.ContactPhoneMandatoryRules)
                    {
                        PhoneMandatoryRule.Add(item.Type);
                        PhoneMandatoryRuleCode.Add(item.TypeCode);
                    }

                });

            }, 10);
        }

        private async void ButtonHistory_Click(object sender, RoutedEventArgs e)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(new PhoneHistory(), "DetailDialog");
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            
            int count = PanelPhone.Children.Count;
            List<PhoneModel.PhoneUsage> items = new List<PhoneModel.PhoneUsage>();

            for (int i = 0; i < count; i++)
            {
                List<PhoneModel.PhoneType> typeList = new List<PhoneModel.PhoneType>();
                PhoneModel.PhoneType type = new PhoneModel.PhoneType();

                Grid grid = (Grid)PanelPhone.Children[i];
                ComboBox combo = (ComboBox)grid.Children[0];
                TextBox text = (TextBox)grid.Children[1];
                type.Code = PhoneData.ContactPhoneTypes[PhoneTypeList.IndexOf(combo.SelectedItem.ToString())].Code;
                typeList.Add(type);

                PhoneModel.PhoneUsage phone = new PhoneModel.PhoneUsage();
                phone.PhoneNumber = text.Text;
                phone.PhoneTypes = typeList;
                items.Add(phone);
            }

            int length = PhoneMandatoryRule.Count;
            for (int i = 0; i < length; i++)
            {
                bool isContains = false;
                for (int j = 0; j < count; j++)
                {
                    if (PhoneMandatoryRuleCode[i] == items[j].PhoneTypes[0].Code)
                    {
                        isContains = true;
                        break;
                    }
                }

                if (!isContains)
                {
                    MessageUtils.ShowErrorMessage("", Properties.Resources.Must_have + PhoneMandatoryRule[i]);
                    return;
                }

            }

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.ContactPhoneURL, Method.Post, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.1), Global.BuildDictionary("ContactPhoneUsage", items));

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    //HttpStatusCode statusCode = (HttpStatusCode)result["statusCode"];
                    if (result == HttpStatusCode.OK)
                    {
                        MessageUtils.ShowMessage("", Properties.Resources.Contact_Phone_Update);
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
            AddPhoneNumber();
        }

        private void AddPhoneNumber(PhoneModel.PhoneUsage phone = null)
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
            combo.ItemsSource = PhoneTypeList;
            combo.Margin = new Thickness(10);
            combo.FontSize = 16;
            combo.SelectedIndex = phone == null ? -1 : PhoneTypeList.IndexOf(phone.PhoneTypes[0].Name);
            combo.SelectionChanged += ComboBox_SelectionChanged;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(combo, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(combo, Properties.Resources.Phone_Type + " *");
            grid.Children.Add(combo);
            Grid.SetColumn(combo, 0);

            TextBox text = new TextBox();
            text.Margin = new Thickness(10);
            text.FontSize = 16;
            text.Text = phone == null ? "" : phone.PhoneNumber;
            text.TextChanged += TextBox_TextChanged;
            text.PreviewTextInput += TextBox_PreviewTextInput;
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
            button.Click += ButtonDelete_Click;
            grid.Children.Add(button);
            Grid.SetColumn(button, 2);

            PanelPhone.Children.Add(grid);
        }


        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            PanelPhone.Children.Remove((Grid)button.Parent);
            ButtonSave.IsEnabled = true;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null) ButtonSave.IsEnabled = true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender != null)
            {
                TextBox textBox = (TextBox)sender;
                string phone = textBox.Text;
                ButtonSave.IsEnabled = true;
                //if (phone.Length < 4) MessageUtils.ShowErrorMessage("", "Invalide Phone Number");

            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }
    }
}
