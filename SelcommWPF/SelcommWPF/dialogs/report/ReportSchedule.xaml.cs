using SelcommWPF.clients.models;
using SelcommWPF.clients.models.report;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SelcommWPF.dialogs.report
{
    /// <summary>
    /// Interaction logic for ReportSchedule.xaml
    /// </summary>
    public partial class ReportSchedule : UserControl
    {
        private string RequestContent;
        private string DefinitionId;
        private string DefinitionName;
        private ReportDefinition.Item ReportDetailData;
        private int DayOfMonth;
        private bool IsLoadingLazy = false;

        public ReportSchedule(string content, string id, string name)
        {
            RequestContent = content;
            DefinitionId = id;
            DefinitionName = name;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            if (RequestContent == Properties.Resources.Run_Now) ((TabItem)TabContainer.Items[1]).Visibility = Visibility.Collapsed;
            LabelDialogTitle.Content = string.Format("{0} : [{1}]", RequestContent, DefinitionName);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {

                ReportDetailData = Global.reportClient.GetReportDetailData(Constants.ReportDetailURL, DefinitionId);
                List<ReportDefinition.Parameter> parameters = ReportDetailData.Parameters;
                List<ReportDefinition.Email> emails = ReportDetailData.Emails;

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (ReportDetailData == null || parameters == null || emails == null)
                    {
                        Global.CloseDialog("DetailDialog");
                        return;
                    }

                    SetParameterView(parameters);
                    SetEmailsView(emails);
                    ShowHideVerticalScroll(Global.HasVerticalScroll);
                });

            }, 10);

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }

        private void SetParameterView(List<ReportDefinition.Parameter> items)
        {
            List<ReportDefinition.Parameter> parameters = new List<ReportDefinition.Parameter>();
            int count = items.Count;
            for (int i = 0; i < count; i++) if (!items[i].Deleted) parameters.Add(items[i]);

            count = items.Count;
            for (int i = 0; i < count / 2 + 1; i++)
            {
                Grid grid = new Grid();
                ColumnDefinition column1 = new ColumnDefinition();
                column1.Width = new GridLength(1, GridUnitType.Star);
                grid.ColumnDefinitions.Add(column1);
                ColumnDefinition column2 = new ColumnDefinition();
                column2.Width = new GridLength(1, GridUnitType.Star);
                grid.ColumnDefinitions.Add(column2);

                for (int j = 0; j < 2; j++)
                {
                    int index = i * 2 + j;
                    if (index == count) break;

                    StackPanel panel = new StackPanel();
                    panel.Orientation = Orientation.Vertical;
                    panel.Margin = new Thickness(10);
                    panel.Tag = parameters[index].Id;
                    panel.ToolTip = parameters[index].Tooltip;

                    Label label = new Label();
                    label.Content = parameters[index].Name;
                    label.FontSize = 16;
                    label.FontWeight = FontWeight.FromOpenTypeWeight(600);
                    panel.Children.Add(label);

                    switch (parameters[index].DataType)
                    {
                        case "List":
                            TextBox textList = new TextBox();
                            textList.FontSize = 16;
                            textList.Text = parameters[index].Default;
                            textList.IsEnabled = !parameters[index].Locked;
                            textList.IsReadOnly = true;
                            textList.GotFocus += TextBox_GotFocus;
                            textList.LostFocus += TextBox_LostFocus;
                            MaterialDesignThemes.Wpf.TextFieldAssist.SetHasClearButton(textList, true);
                            panel.Children.Add(textList);
                            MaterialDesignThemes.Wpf.Card card1 = new MaterialDesignThemes.Wpf.Card();
                            ListView listView1 = new ListView();
                            listView1.FontSize = 16;
                            listView1.MaxHeight = 100;
                            listView1.SelectionChanged += List_SelectionChanged;
                            SetParameterList(listView1, parameters[index]);
                            card1.Content = listView1;
                            card1.Visibility = Visibility.Collapsed;
                            panel.Children.Add(card1);
                            break;
                        case "Date":
                            DatePicker date = new DatePicker();
                            date.FontSize = 16;
                            date.IsEnabled = !parameters[index].Locked;
                            panel.Children.Add(date);
                            break;
                        case "Boolean":
                            ToggleButton button = new ToggleButton();
                            button.IsChecked = parameters[index].Default == "Y";
                            button.HorizontalAlignment = HorizontalAlignment.Left;
                            button.Margin = new Thickness(10, 0, 0, 0);
                            button.IsEnabled = !parameters[index].Locked;
                            panel.Children.Add(button);
                            break;
                        case "LazyList":
                            TextBox textLazy = new TextBox();
                            textLazy.FontSize = 16;
                            textLazy.Text = parameters[index].Default;
                            textLazy.IsEnabled = !parameters[index].Locked;
                            textLazy.TextChanged += LazyTextBox_TextChanged;
                            textLazy.Tag = parameters[index].URL;
                            MaterialDesignThemes.Wpf.TextFieldAssist.SetHasClearButton(textLazy, true);
                            panel.Children.Add(textLazy);
                            ProgressBar progressBar = new ProgressBar();
                            progressBar.IsIndeterminate = true;
                            progressBar.Visibility = Visibility.Collapsed;
                            panel.Children.Add(progressBar);
                            MaterialDesignThemes.Wpf.Card card = new MaterialDesignThemes.Wpf.Card();
                            ListView listView = new ListView();
                            listView.FontSize = 16;
                            listView.MaxHeight = 100;
                            listView.SelectionChanged += LazyList_SelectionChanged;
                            card.Content = listView;
                            card.Visibility = Visibility.Collapsed;
                            panel.Children.Add(card);
                            break;
                        default:
                            TextBox text = new TextBox();
                            text.FontSize = 16;
                            text.Text = parameters[index].Default;
                            text.IsEnabled = !parameters[index].Locked;
                            panel.Children.Add(text);
                            break;
                    }

                    grid.Children.Add(panel);
                    Grid.SetColumn(panel, j);

                }

                PanelParams.Children.Add(grid);
            }

            StackPanel panel1 = new StackPanel();
            panel1.Orientation = Orientation.Vertical;
            panel1.Margin = new Thickness(10);
            panel1.Tag = 0;

            Label label1 = new Label();
            label1.Content = Properties.Resources.Description;
            label1.FontSize = 16;
            label1.FontWeight = FontWeight.FromOpenTypeWeight(600);
            panel1.Children.Add(label1);

            TextBox text1 = new TextBox();
            text1.FontSize = 16;
            text1.Name = "TextDescription";
            panel1.Children.Add(text1);

            if (count % 2 == 0)
            {
                Grid grid = new Grid();
                ColumnDefinition column1 = new ColumnDefinition();
                column1.Width = new GridLength(1, GridUnitType.Star);
                grid.ColumnDefinitions.Add(column1);
                ColumnDefinition column2 = new ColumnDefinition();
                column2.Width = new GridLength(1, GridUnitType.Star);
                grid.ColumnDefinitions.Add(column2);
                grid.Children.Add(panel1);
                Grid.SetColumn(panel1, 0);
                PanelParams.Children.Add(grid);
            }
            else
            {
                Grid grid = (Grid)PanelParams.Children[PanelParams.Children.Count - 1];
                grid.Children.Add(panel1);
                Grid.SetColumn(panel1, 1);
            }
        }

        private void SetEmailsView(List<ReportDefinition.Email> emails)
        {
            foreach (ReportDefinition.Email item in emails)
            {
                CheckBox checkBox = new CheckBox();
                checkBox.FontSize = 16;
                checkBox.VerticalContentAlignment = VerticalAlignment.Bottom;
                checkBox.Margin = new Thickness(10);
                checkBox.Content = item.Address;
                checkBox.Tag = item.Id;
                PanelEmails.Children.Add(checkBox);
            }
        }

        private void RadioScheduleMode_Checked(object sender, RoutedEventArgs e)
        {
            if (PanelWeekly == null || PanelMonthly == null) return;
            RadioButton radio = (RadioButton)sender;

            if (radio.Content.ToString() == Properties.Resources.Weekly)
            {
                PanelWeekly.Visibility = Visibility.Visible;
                PanelMonthly.Visibility = Visibility.Collapsed;
            }
            else if (radio.Content.ToString() == Properties.Resources.Monthly)
            {
                PanelWeekly.Visibility = Visibility.Collapsed;
                PanelMonthly.Visibility = Visibility.Visible;
            }
        }

        private void ButtonDay_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string content = button.Content.ToString();

            if (content == Properties.Resources.End_Month) DayOfMonth = 31;
            else DayOfMonth = Convert.ToInt32(content.Substring(0, content.Length - 2));

            button.Background = Brushes.LightGray;
            int count = PanelMonthly.Children.Count;
            for (int i = 0; i < count; i++)
            {
                StackPanel panel = (StackPanel)PanelMonthly.Children[i];
                int count1 = panel.Children.Count;
                for (int j = 0; j < count1; j++)
                {
                    Button button1 = (Button)panel.Children[j];
                    button1.Background = button.Content == button1.Content ? Brushes.LightGray : Brushes.White;
                }
            }
        }

        private void ButtonRequest_Click(object sender, RoutedEventArgs e)
        {

            List<Dictionary<string, object>> parameters = new List<Dictionary<string, object>>();
            int count = PanelParams.Children.Count;

            for (int i = 0; i < count; i++)
            {
                Grid grid = (Grid)PanelParams.Children[i];
                int count1 = grid.Children.Count;

                for (int j = 0; j < count1; j++)
                {
                    StackPanel panel = (StackPanel)grid.Children[j];
                    Label label = (Label)panel.Children[0];

                    long id = Convert.ToInt64(panel.Tag);
                    if (id == 0) break;

                    Dictionary<string, object> value = new Dictionary<string, object>();
                    value.Add("ReportParameterId", id);
                    ReportDefinition.Parameter item = new ReportDefinition.Parameter();

                    foreach (ReportDefinition.Parameter row in ReportDetailData.Parameters)
                    {
                        if (row.Id == id)
                        {
                            item = row;
                            break;
                        }
                    }

                    switch (item.DataType)
                    {
                        case "List":
                            ComboBox combo = (ComboBox)panel.Children[1];
                            if (!item.Optional && (combo.SelectedItem == null || combo.SelectedItem.ToString() == ""))
                            {
                                MessageUtils.ShowErrorMessage("", Properties.Resources.Requeir_Field);
                                return;
                            }
                            value.Add("Value", combo.SelectedItem == null ? "" : combo.SelectedItem.ToString());
                            break;
                        case "Date":
                            DatePicker date = (DatePicker)panel.Children[1];
                            if (!item.Optional && date.SelectedDate == null)
                            {
                                MessageUtils.ShowErrorMessage("", Properties.Resources.Requeir_Field);
                                return;
                            }
                            value.Add("Value", date.SelectedDate == null ? "1900-01-01" : date.SelectedDate.Value.ToString("yyyy-MM-dd"));
                            break;
                        case "Boolean":
                            ToggleButton toggle = (ToggleButton)panel.Children[1];
                            value.Add("Value", toggle.IsChecked.Value ? "Y" : "N");
                            break;
                        default:
                            TextBox text = (TextBox)panel.Children[1];
                            if (!item.Optional && text.Text == "")
                            {
                                MessageUtils.ShowErrorMessage("", Properties.Resources.Requeir_Field);
                                return;
                            }
                            value.Add("Value", text.Text);
                            break;
                    }

                    parameters.Add(value);
                }
            }

            List<Dictionary<string, object>> emails = new List<Dictionary<string, object>>();
            count = PanelEmails.Children.Count;

            for (int i = 0; i < count; i++)
            {
                CheckBox checkBox = (CheckBox)PanelEmails.Children[i];
                if (checkBox.IsChecked.Value)
                {
                    Dictionary<string, object> item = new Dictionary<string, object>();
                    item.Add("EmailId", Convert.ToInt64(checkBox.Tag));
                    item.Add("Log", true);
                    item.Add("Output", true);
                    emails.Add(item);
                }
            }

            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("ReportId", DefinitionId);
            body.Add("OutputFileName", "string");
            body.Add("Priority", 0);

            TextBox textDescription = Global.FindChild<TextBox>(PanelParams, "TextDescription");
            body.Add("Comments", textDescription.Text);
            body.Add("Emails", emails);
            body.Add("Parameters", parameters);

            string url = Constants.ReportInstancePostURL;
            if (RequestContent != "Run Now")
            {
                url = Constants.ReportSchedulePostURL;
                body.Add("DayOfMonth", DayOfMonth);
                body.Add("Monday", CheckMonday.IsChecked.Value);
                body.Add("Tuesday", CheckTuesday.IsChecked.Value);
                body.Add("Wednesday", CheckWednesday.IsChecked.Value);
                body.Add("Thursday", CheckThursday.IsChecked.Value);
                body.Add("Saturday", CheckSaturday.IsChecked.Value);
                body.Add("Sunday", CheckSunday.IsChecked.Value);
                body.Add("From", DatePickerStart.SelectedDate == null ? DateTime.Now.ToString("yyyy-MM-dd") : DatePickerStart.SelectedDate.Value.ToString("yyyy-MM-dd"));
                body.Add("To", DateTime.Now.ToString("9999-MM-dd"));
            }

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {

                Dictionary<string, object> result = Global.reportClient.GenerateReport(url, body);
                HttpStatusCode statusCode = (HttpStatusCode)result["statusCode"];

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (statusCode == HttpStatusCode.OK || statusCode == HttpStatusCode.Created) Global.CloseDialog("DetailDialog");
                });

            }, 10);

        }

        private void ShowHideVerticalScroll(bool isChecked)
        {
            ScrollViewer[] scrolls = new ScrollViewer[] { ScrollParams, ScrollSchedule, ScrollEmails };
            int count = scrolls.Length;

            for (int i = 0; i < count; i++) if (scrolls[i] == null) return;
            for (int i = 0; i < count; i++) scrolls[i].VerticalScrollBarVisibility = isChecked ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
        }

        private void SetParameterList(ListView listView, ReportDefinition.Parameter parameter)
        {
            if (parameter.URL == null || parameter.URL == "") return;

            string url = Constants.BaseURL + parameter.URL.Split('#')[0].Substring(1);
            EasyTimer.SetTimeout(() =>
            {
                List<UserModel.BusinessUnitModel> result = Global.reportClient.GetReportParameterList(url);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<string> contents = new List<string>();
                    foreach (UserModel.BusinessUnitModel item in result) contents.Add(string.Format("{0}-{1}", item.Id, item.Name));
                    listView.ItemsSource = contents;
                });

            }, 10);
        }

        private void LazyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoadingLazy)
            {
                IsLoadingLazy = false;
                return;
            }

            TextBox textLazy = (TextBox)sender;
            string search = textLazy.Text;
            StackPanel panel = (StackPanel)textLazy.Parent;
            ProgressBar progressBar = (ProgressBar)panel.Children[2];
            MaterialDesignThemes.Wpf.Card card = (MaterialDesignThemes.Wpf.Card)panel.Children[3];
            ListView listView = (ListView)card.Content;

            if (search == "")
            {
                listView.ItemsSource = new List<string>();
                card.Visibility = Visibility.Collapsed;
                return;
            }

            string url = Constants.BaseURL + textLazy.Tag.ToString().Split('#')[0].Substring(1);
            progressBar.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                List<UserModel.BusinessUnitModel> result = Global.reportClient.GetReportParameterList(url, search);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<string> contents = new List<string>();
                    foreach (UserModel.BusinessUnitModel item in result) contents.Add(string.Format("{0}-{1}", item.Id, item.Name));
                    listView.ItemsSource = contents;
                    progressBar.Visibility = Visibility.Collapsed;
                    card.Visibility = Visibility.Visible;
                });

            }, 10);
        }

        private void LazyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = (ListView)sender;
            if (listView.SelectedItem == null) return;

            MaterialDesignThemes.Wpf.Card card = (MaterialDesignThemes.Wpf.Card)listView.Parent;
            StackPanel panel = (StackPanel)card.Parent;
            TextBox textLazy = (TextBox)panel.Children[1];
            IsLoadingLazy = true;
            textLazy.Text = listView.SelectedItem.ToString().Split('-')[1];
            listView.ItemsSource = new List<string>();
            card.Visibility = Visibility.Collapsed;
            textLazy.SelectionStart = textLazy.Text.Length;
            textLazy.Focus();
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = (ListView)sender;
            if (listView.SelectedItem == null) return;

            MaterialDesignThemes.Wpf.Card card = (MaterialDesignThemes.Wpf.Card)listView.Parent;
            StackPanel panel = (StackPanel)card.Parent;
            TextBox textList = (TextBox)panel.Children[1];
            textList.Text = listView.SelectedItem.ToString().Split('-')[1];
            listView.SelectedIndex = -1;
            card.Visibility = Visibility.Collapsed;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textList = (TextBox)sender;
            StackPanel panel = (StackPanel)textList.Parent;
            MaterialDesignThemes.Wpf.Card card = (MaterialDesignThemes.Wpf.Card)panel.Children[2];
            card.Visibility = Visibility.Visible;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textList = (TextBox)sender;
            StackPanel panel = (StackPanel)textList.Parent;
            MaterialDesignThemes.Wpf.Card card = (MaterialDesignThemes.Wpf.Card)panel.Children[2];
            card.Visibility = Visibility.Collapsed;
        }

    }
}
