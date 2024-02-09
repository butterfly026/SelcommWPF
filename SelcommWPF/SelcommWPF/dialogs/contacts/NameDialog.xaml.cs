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
    /// Interaction logic for NameDialog.xaml
    /// </summary>
    public partial class NameDialog : UserControl
    {
        private NameModel NameData = new NameModel();
        private List<string> AliasTypes = new List<string>();
        private List<string> AliasCodeTypes = new List<string>();
        private List<string> TitlesList = new List<string>();
        private string AccountType = "";

        public NameDialog(string type)
        {
            AccountType = type;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            ComboAliasType.Visibility = AccountType == "Individual" ? Visibility.Visible : Visibility.Collapsed;
            TextFirstName.Visibility = AccountType == "Individual" ? Visibility.Visible : Visibility.Collapsed;
            TextLastName.Width = AccountType == "Individual" ? 260 : 800;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> header = Global.GetHeader(Global.CustomerToken);
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.1);
                List<Dictionary<string, string>> aliasTypes = Global.RequestAPI<List<Dictionary<string, string>>>(Constants.AliasTypeURL, Method.Get, header, null, query, "");
                List<Dictionary<string, string>> titles = Global.RequestAPI<List<Dictionary<string, string>>>(Constants.ContactTitlesURL, Method.Get, header, null, query, "");
                query.Add("IncludeTypes", true);
                query.Add("IncludeTitles", true);
                NameData = Global.RequestAPI<NameModel>(Constants.ContactNameURL, Method.Get, header, Global.BuildDictionary("ContactCode", Global.ContactCode), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (NameData == null || aliasTypes == null || titles == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    TextFirstName.Text = NameData.FirstName;
                    TextLastName.Text = NameData.Name;

                    foreach (Dictionary<string, string> item in aliasTypes)
                    {
                        AliasTypes.Add(item["Name"]);
                        AliasCodeTypes.Add(item["Code"]);
                    }

                    foreach (Dictionary<string, string> item in titles) TitlesList.Add(item["Name"]);
                    foreach (NameModel.ContactAliase item in NameData.ContactAliases) AddAilasView(item);

                    ComboAliasType.ItemsSource = TitlesList;
                    ComboAliasType.SelectedIndex = TitlesList.IndexOf(NameData.Title);
                });

            }, 10);
        }

        private async void ButtonHistory_Click(object sender, RoutedEventArgs e)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(new PhoneHistory(), "DetailDialog");
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            int count = PanelNames.Children.Count;
            List<Dictionary<string, string>> items = new List<Dictionary<string, string>>();

            for (int i = 0; i < count; i++)
            {
                Grid grid = (Grid)PanelNames.Children[i];
                ComboBox combo = (ComboBox)grid.Children[0];
                TextBox text = (TextBox)grid.Children[1];

                Dictionary<string, string> alisa = new Dictionary<string, string>();
                alisa.Add("Alias", text.Text);
                alisa.Add("TypeCode", AliasCodeTypes[combo.SelectedIndex]);

                items.Add(alisa);
            }

            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("Name", TextLastName.Text);
            body.Add("ContactAliases", items);
            if (TextFirstName.Name != "") body.Add("FirstName", TextFirstName.Text);
            if (ComboAliasType.SelectedItem != null) body.Add("Title", ComboAliasType.SelectedItem.ToString());
            if (NameData.Initials != "") body.Add("Initials", NameData.Initials);
            if (NameData.ContactKey != "") body.Add("ContactKey", NameData.ContactKey);

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.ContactNameURL, Method.Put, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.1), body);

                Application.Current.Dispatcher.Invoke(delegate
                {

                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK)
                    {
                        MessageUtils.ShowMessage("", Properties.Resources.Contact_Name_Update);
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
            AddAilasView();
        }

        private void AddAilasView(NameModel.ContactAliase aliase = null)
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
            combo.ItemsSource = AliasTypes;
            combo.Margin = new Thickness(10);
            combo.FontSize = 16;
            combo.SelectedIndex = aliase == null ? -1 : AliasTypes.IndexOf(aliase.Type);
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(combo, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(combo, Properties.Resources.Alias_Type + " *");
            grid.Children.Add(combo);
            Grid.SetColumn(combo, 0);

            TextBox text = new TextBox();
            text.Margin = new Thickness(10);
            text.FontSize = 16;
            text.Text = aliase == null ? "" : aliase.Alias;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(text, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(text, Properties.Resources.Alias + " *");
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

            PanelNames.Children.Add(grid);
        }


        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            PanelNames.Children.Remove((Grid)button.Parent);
        }

        private async void ButtonAliasedHistory_Click(object sender, RoutedEventArgs e)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(new AliasesHistory(true), "DetailDialog");
        }

        private async void ButtonNamesHistory_Click(object sender, RoutedEventArgs e)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(new AliasesHistory(false), "DetailDialog");
        }

        private void TextFirstName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AccountType == "Individual") ButtonSave.IsEnabled = TextFirstName.Text != "" && TextLastName.Text != "";
        }

        private void TextLastName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AccountType == "Individual") ButtonSave.IsEnabled = TextFirstName.Text != "" && TextLastName.Text != "";
            else ButtonSave.IsEnabled = TextLastName.Text != "";
        }

        private void ComboAliasType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}
