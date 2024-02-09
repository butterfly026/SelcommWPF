using Microsoft.Win32;
using SelcommWPF.clients.models.report;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SelcommWPF.dialogs.report
{
    /// <summary>
    /// Interaction logic for ReportDetail.xaml
    /// </summary>
    public partial class ReportDetail : UserControl
    {
        private string DefinitionId;

        public ReportDetail(string id, string name)
        {
            DefinitionId = id;
            InitializeComponent();
            LabelDialogTitle.Content = string.Format(Properties.Resources.Report, name);
            ShowReportDetail(ListReportInstances, Constants.ReportInstanceURL, 0, 20);
        }

        private void ShowReportDetail(ListView listView, string url, int skip, int take)
        {
            if (skip == 0) listView.ItemsSource = new List<ReportDetailModel.Item>();
            if (Global.ContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {

                ReportDetailModel result = Global.reportClient.GetReportDetailList(url, DefinitionId, skip, take, "Y");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    List<ReportDetailModel.Item> list = (List<ReportDetailModel.Item>)listView.ItemsSource;
                    listView.ItemsSource = new List<ReportDetailModel.Item>();

                    if (list == null) list = new List<ReportDetailModel.Item>();
                    int count = result == null ? 0 : result.Reports.Count;
                    listView.Tag = result.Count;

                    for (int i = 0; i < count; i++)
                    {
                        string emailsText = "";
                        foreach (ReportDetailModel.Email item in result.Reports[i].Emails) emailsText += ", " + item.Address;
                        result.Reports[i].EmailsText = emailsText == "" ? "" : emailsText.Substring(2);
                        result.Reports[i].StatusDateTime = StringUtils.ConvertDateTime(result.Reports[i].StatusDateTime);
                        result.Reports[i].From = StringUtils.ConvertDateTime(result.Reports[i].From);
                        result.Reports[i].To = StringUtils.ConvertDateTime(result.Reports[i].To);
                        list.Add(result.Reports[i]);
                    }
                    
                    listView.ItemsSource = list;
                    if (skip != 0) MessageUtils.ShowMessage("", string.Format("records {0} to {1} loaded more...", skip + 1, skip + take));
                    if (skip == 0)
                    {
                        ShowHideVerticalScroll(Global.HasVerticalScroll);
                        if (url == Constants.ReportInstanceURL) ShowReportDetail(ListReportSchedule, Constants.ReportScheduleURL, 0, 20);
                    }
                });

            }, 10);
        }


        private void ListReportDetail_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ListView listView = (ListView)e.Source;
            int totalCount = listView.Items.Count;
            int limit = Convert.ToInt32(listView.Tag);
            if (totalCount == 0 || totalCount >= limit) return;
            if (e.VerticalOffset + e.VerticalChange >= totalCount - 9)
            {
                string url = listView.Name == "ListReportInstances" ? Constants.ReportInstanceURL : Constants.ReportScheduleURL;
                ShowReportDetail(listView, url, totalCount, 20);
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Global.CloseDialog(button.Tag.ToString());
        }

        private async void ButtonArgument_Click(object sender, RoutedEventArgs e)
        {
            string tag = ((Button)sender).Tag.ToString();
            ListView listView = ListReportInstances;

            if (tag == "1")
            {
                if (ListReportInstances == null || ListReportInstances.SelectedItem == null) return;
                listView = ListReportInstances;
            }
            else if (tag == "2")
            {
                if (ListReportSchedule == null || ListReportSchedule.SelectedItem == null) return;
                listView = ListReportSchedule;
            }

            ReportDetailModel.Item item = (ReportDetailModel.Item)listView.SelectedItem;

            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;
            panel.Margin = new Thickness(20);
            panel.Width = 600;

            Label labelTitle = new Label();
            labelTitle.FontSize = 20;
            labelTitle.FontWeight = FontWeight.FromOpenTypeWeight(700);
            labelTitle.HorizontalAlignment = HorizontalAlignment.Center;
            labelTitle.Content = string.Format(Properties.Resources.Parameters, item.Id, item.Name);
            panel.Children.Add(labelTitle);

            foreach(ReportDetailModel.Parameter param in item.Parameters)
            {
                Grid grid = new Grid();
                grid.Margin = new Thickness(0, 10, 0, 0);

                ColumnDefinition column = new ColumnDefinition();
                column.Width = new GridLength(1, GridUnitType.Star);
                ColumnDefinition column1 = new ColumnDefinition();
                column1.Width = new GridLength(1, GridUnitType.Star);

                grid.ColumnDefinitions.Add(column);
                grid.ColumnDefinitions.Add(column1);

                StackPanel panel1 = new StackPanel();
                panel1.Orientation = Orientation.Horizontal;

                Label label1 = new Label();
                label1.FontSize = 16;
                label1.FontWeight = FontWeight.FromOpenTypeWeight(600);
                label1.Content = Properties.Resources.Name + " : ";

                Label label2 = new Label();
                label2.FontSize = 16;
                label2.Content = param.Name;

                panel1.Children.Add(label1);
                panel1.Children.Add(label2);

                grid.Children.Add(panel1);
                Grid.SetColumn(panel1, 0);

                StackPanel panel2 = new StackPanel();
                panel2.Orientation = Orientation.Horizontal;

                Label label3 = new Label();
                label3.FontSize = 16;
                label3.FontWeight = FontWeight.FromOpenTypeWeight(600);
                label3.Content = Properties.Resources.Value + " : ";

                Label label4 = new Label();
                label4.FontSize = 16;
                label4.Content = param.Value;

                panel2.Children.Add(label3);
                panel2.Children.Add(label4);

                grid.Children.Add(panel2);
                Grid.SetColumn(panel2, 1);

                panel.Children.Add(grid);
            }

            Button button = new Button();
            button.Margin = new Thickness(0, 10, 0, 0);
            button.VerticalAlignment = VerticalAlignment.Center;
            button.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            button.Content = Properties.Resources.Close;
            button.Tag = "ArgumentDialog";
            button.Width = 100;
            button.Click += ButtonClose_Click;
            panel.Children.Add(button);

            Global.CloseDialog("ArgumentDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(panel, "ArgumentDialog");

        }

        private void ButtonDownload_Click(object sender, RoutedEventArgs e)
        {
            LoadingPanel.Visibility = Visibility.Visible;
            ReportDetailModel.Item item = (ReportDetailModel.Item)ListReportInstances.SelectedItem;
            if (item == null || item.Id == 0) return;

            EasyTimer.SetTimeout(() =>
            {

                Dictionary<string, object> result = Global.reportClient.GetDownloadReport(Constants.ReportDownloadURL, item.Id);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    try
                    {
                        LoadingPanel.Visibility = Visibility.Hidden;
                        string fileType = result["FileType"].ToString();

                        if (result["Content"] == null || result["Content"].ToString() == "")
                        {
                            MessageUtils.ShowErrorMessage("", "Report is not available for download.");
                            return;
                        }

                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.Filter = string.Format("{0} File|*.{1}", fileType, fileType);
                        saveFileDialog.Title = string.Format("Save As {0} File", fileType);
                        saveFileDialog.FileName = string.Format("Report # {0}.{1}", result["Id"], fileType);

                        if (saveFileDialog.ShowDialog().Value)
                        {
                            File.WriteAllBytes(saveFileDialog.FileName, Convert.FromBase64String(result["Content"].ToString()));
                            MessageUtils.ShowMessage("", string.Format(Properties.Resources.Download_Success, fileType));
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });

            }, 10);
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            string tag = ((Button)sender).Tag.ToString();
            ListView listView = ListReportInstances;
            string url = Constants.ReportInstanceDeleteURL;
            string url1 = Constants.ReportInstanceURL;

            if (tag == "1")
            {
                if (ListReportInstances == null || ListReportInstances.SelectedItem == null) return;
                listView = ListReportInstances;
                url = Constants.ReportInstanceDeleteURL;
                url1 = Constants.ReportInstanceURL;
            }
            else if (tag == "2")
            {
                if (ListReportSchedule == null || ListReportSchedule.SelectedItem == null) return;
                listView = ListReportSchedule;
                url = Constants.ReportScheduleDeleteURL;
                url1 = Constants.ReportScheduleURL;
            }

            ReportDetailModel.Item item = (ReportDetailModel.Item)listView.SelectedItem;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {

                Global.reportClient.DeleteReportDetail(url, item.Id);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    ShowReportDetail(listView, url1, 0, listView.Items.Count);
                });

            }, 10);

        }

        private async void ButtonEnd_Click(object sender, RoutedEventArgs e)
        {
            if (ListReportSchedule == null || ListReportSchedule.SelectedItem == null) return;
            ReportDetailModel.Item item = (ReportDetailModel.Item)ListReportSchedule.SelectedItem;

            Global.ReportDetailDialog = this;
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ScheduleEnd(item.Id, item.Name), "ArgumentDialog");
        }

        public void UpdateScheduleEnd(long id, string dateTime)
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.reportClient.UpdateEndSchedule(Constants.ReportSchedultEndURL, id, dateTime);
                Application.Current.Dispatcher.Invoke(delegate
                {

                    if (result == HttpStatusCode.OK)
                    {
                        List<ReportDetailModel.Item> list = (List<ReportDetailModel.Item>)ListReportSchedule.ItemsSource;
                        int count = list.Count;

                        for (int i = 0; i < count; i++)
                        {
                            if (id == list[i].Id)
                            {
                                list[i].To = StringUtils.ConvertDateTime(dateTime);
                                break;
                            }
                        }

                        ListReportSchedule.ItemsSource = new List<ReportDetailModel.Item>();
                        ListReportSchedule.ItemsSource = list;
                        LoadingPanel.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources.End_Schedule_Fail);
                        LoadingPanel.Visibility = Visibility.Hidden;
                    }

                   
                });
            }, 10);

        }

        public void ShowHideVerticalScroll(bool isChecked)
        {
            ListView[] listViews = new ListView[] { ListReportInstances, ListReportSchedule };

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

        private void ListReportInstances_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListReportInstances == null || ListReportInstances.SelectedItem == null) return;
            bool isEnabled = ListReportInstances.SelectedItem != null;
            ButtonInstanceView.IsEnabled = isEnabled;
            ButtonInstanceDownload.IsEnabled = isEnabled;
            ButtonInstanceDelete.IsEnabled = isEnabled;
        }

        private void ListReportSchedule_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListReportSchedule == null || ListReportSchedule.SelectedItem == null) return;
            bool isEnabled = ListReportSchedule.SelectedItem != null;
            ButtonScheduleEnd.IsEnabled = isEnabled;
            ButtonScheduleView.IsEnabled = isEnabled;
            ButtonScheduleDelete.IsEnabled = isEnabled;
        }
    }
}
