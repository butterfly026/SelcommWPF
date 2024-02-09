using Microsoft.Win32;
using SelcommWPF.clients.models.contacts;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for IdentificationDialog.xaml
    /// </summary>
    public partial class IdentificationDialog : UserControl
    {
        private bool IsLoadingUI = false;
        private WrapPanel PanelFileList;
        private WrapPanel WrapUploadedFiles;
        private StackPanel PanelQA;
        private List<Identification> IdentificationType = new List<Identification>();
        private Dictionary<string, object> IdentificationRules = new Dictionary<string, object>();

        public IdentificationDialog()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            string accountType = Global.mainWindow.AccountType == "Individual" ? "PERSONAL" : "CORPORATE";
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                IdentificationType = Global.contactClient.GetIdentificationData(Constants.IdentificationTypeURL);
                IdentificationRules = Global.contactClient.GetIdentificationRules(Constants.IdentificationRuleURL, accountType);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (IdentificationType == null || IdentificationRules == null)
                    {
                        Global.CloseDialog();
                        return;
                    }
                    LabelPoints.Content = string.Format(Properties.Resources.ID_Point_Alert, Convert.ToInt32(IdentificationRules["MinimumPoints"].ToString()), 0);
                });

            }, 10);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void ButtonAddQA_Click(object sender, RoutedEventArgs e)
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;

            TextBox text1 = new TextBox();
            text1.Margin = new Thickness(10);
            text1.FontSize = 16;
            text1.Width = 250;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(text1, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(text1, "Question");

            TextBox text2 = new TextBox();
            text2.Margin = new Thickness(10);
            text2.FontSize = 16;
            text2.Width = 250;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(text2, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(text2, "Answer");

            panel.Children.Add(text1);
            panel.Children.Add(text2);
            PanelQA.Children.Add(panel);
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

            WrapUploadedFiles = new WrapPanel();
            WrapUploadedFiles.Orientation = Orientation.Horizontal;
            panel2.Children.Add(WrapUploadedFiles);

            Grid grid1 = new Grid();
            StackPanel panel11 = new StackPanel();
            panel11.Orientation = Orientation.Vertical;

            PanelQA = new StackPanel();
            PanelQA.Orientation = Orientation.Vertical;
            panel11.Children.Add(PanelQA);

            TextBox textPwd = new TextBox();
            textPwd.Margin = new Thickness(10);
            textPwd.FontSize = 16;
            textPwd.Width = 540;
            textPwd.HorizontalAlignment= HorizontalAlignment.Left;
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(textPwd, true);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(textPwd, Properties.Resources.Enquiry_Password);
            panel11.Children.Add(textPwd);

            Button button1 = new Button();
            button1.Content = "Add Questions & Answer";
            button1.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            button1.Margin = new Thickness(0, 0, 10, 0);
            button1.VerticalAlignment = VerticalAlignment.Top;
            button1.HorizontalAlignment = HorizontalAlignment.Right;
            button1.Click += ButtonAddQA_Click;

            grid1.Children.Add(panel11);
            grid1.Children.Add(button1);

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
            //panel.Children.Add(grid1);
            panel.Children.Add(panel1);
            IsLoadingUI = true;

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog("DetailDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(panel, "DetailDialog");
        }

        private void ButtonAttributesClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }

        private void TextCreditLimit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
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
    }
}
