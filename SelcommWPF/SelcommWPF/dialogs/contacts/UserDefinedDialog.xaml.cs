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
    /// Interaction logic for UserDefinedDialog.xaml
    /// </summary>
    public partial class UserDefinedDialog : UserControl
    {

        private List<string> UserDefinedTypeLabelList = new List<string>();
        private List<UserDefined> UserDefinedTypeList = new List<UserDefined>();

        public UserDefinedDialog()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                UserDefinedTypeList = Global.contactClient.GetUserDefinedData(Constants.UserDefinedURL);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (UserDefinedTypeList == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    foreach (UserDefined item in UserDefinedTypeList) UserDefinedTypeLabelList.Add(item.Label);
                    ShowUserDefinedData();
                });

            }, 10);
        }

        private void ShowUserDefinedData()
        {
            if (Global.ContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                List<UserDefined> result = Global.contactClient.GetUserDefinedData(Constants.UserDefinedListURL, Global.ContactCode);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<UserDefined> list = list = new List<UserDefined>();
                    ListUserDefined.ItemsSource = new List<UserDefined>();

                    int count = result == null ? 0 : result.Count;
                    for (int i = 0; i < count; i++)
                    {
                        result[i].Created = StringUtils.ConvertDateTime(result[i].Created);
                        result[i].LastUpdated = StringUtils.ConvertDateTime(result[i].LastUpdated);
                        list.Add(result[i]);
                    }

                    ListUserDefined.ItemsSource = list;
                    ShowHideVerticalScroll(Global.HasVerticalScroll);
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private void ShowHideVerticalScroll(bool isChecked)
        {
            ListView[] listViews = new ListView[] { ListUserDefined };
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

            ComboBox combo = new ComboBox();
            combo.Margin = new Thickness(10);
            combo.ItemsSource = UserDefinedTypeLabelList;
            if (item != null) combo.SelectedIndex = UserDefinedTypeLabelList.IndexOf(item.Name);
            combo.FontSize = 16;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(combo, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(combo, Properties.Resources.User_Defined + " *");
            combo.IsEnabled = item == null;
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
            buttonAdd.Click += ButtonSave_Click;
            panel1.Children.Add(buttonAdd);

            Button buttonClose = new Button();
            buttonClose.Content = Properties.Resources.Close;
            buttonClose.Tag = "DetailDialog";
            buttonClose.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            buttonClose.Margin = new Thickness(0, 0, 10, 0);
            buttonClose.Width = 100;
            buttonClose.Click += ButtonClose_Click;
            panel1.Children.Add(buttonClose);

            panel.Children.Add(grid);
            panel.Children.Add(panel1);

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog("DetailDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(panel, "DetailDialog");
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Global.CloseDialog(button.Tag.ToString());
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            BuildUserDefinedView();
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (ListUserDefined == null || ListUserDefined.SelectedItem == null) return;

            UserDefined item = (UserDefined)ListUserDefined.SelectedItem;
            BuildUserDefinedView(item);
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ListUserDefined == null || ListUserDefined.SelectedItem == null) return;

            UserDefined item = (UserDefined)ListUserDefined.SelectedItem;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {

                Global.contactClient.DeleteUserDefinedData(Constants.UserDefinedDetailURL, Global.ContactCode, item.Id);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    ShowUserDefinedData();
                });

            }, 10);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string name = button.Content.ToString();
            StackPanel panel = (StackPanel)button.Parent;
            Grid grid = (Grid)((StackPanel)panel.Parent).Children[1];

            ComboBox combo = (ComboBox)grid.Children[0];
            string id = UserDefinedTypeList[combo.SelectedIndex].Id;

            TextBox text = (TextBox)grid.Children[1];
            string value = text.Text;

            HttpStatusCode statusCode;
            LoadingPanel.Visibility = Visibility.Visible;
            Global.CloseDialog("DetailDialog");

            EasyTimer.SetTimeout(() =>
            {

                if (name == "Add") statusCode = Global.contactClient.CreateUserDefinedData(Constants.UserDefinedDetailURL, Global.ContactCode, id, value);
                else statusCode = Global.contactClient.UpdateUserDefinedData(Constants.UserDefinedDetailURL, Global.ContactCode, id, value);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (statusCode == HttpStatusCode.OK || statusCode == HttpStatusCode.Created)
                    {
                        Global.mainWindow.GetDisplayDetails();
                        ShowUserDefinedData();
                    }
                    else
                    {
                        MessageUtils.ShowErrorMessage("", string.Format(Properties.Resources.User_Defined_Fail, name));
                        LoadingPanel.Visibility = Visibility.Hidden;
                    }
                });

            }, 10);

        }

        private void ListUserDefined_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ButtonUpdate == null || ButtonDelete == null) return;

            bool isEnabled = ListUserDefined.SelectedItem != null;
            ButtonUpdate.IsEnabled = isEnabled;
            ButtonDelete.IsEnabled = isEnabled;
        }

        private void ComboUserDefined_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            StackPanel panel = (StackPanel)combo.Parent;
            TextBox textBox = (TextBox)panel.Children[1];
            textBox.Text = combo.Tag.ToString();
        }
    }
}
