using SelcommWPF.clients.models.report;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace SelcommWPF.dialogs.report
{
    /// <summary>
    /// Interaction logic for ReportDialog.xaml
    /// </summary>
    public partial class ReportDialog : UserControl
    {
        public ReportDialog()
        {
            InitializeComponent();
            ShowReportList(0, 20);
        }
        
        private void ShowReportList(int skip, int take, bool isScroll = false)
        {
            if (skip == 0) ListReports.ItemsSource = new List<ReportDefinition.Item>();
            if (Global.ContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            string search = TextReportSearch.Text;

            EasyTimer.SetTimeout(() =>
            {

                ReportDefinition result = Global.reportClient.GetReportList(Constants.ReportListURL, skip, take, "Y", search);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    List<ReportDefinition.Item> list = isScroll ? (List<ReportDefinition.Item>)ListReports.ItemsSource : new List<ReportDefinition.Item>();
                    ListReports.ItemsSource = new List<ReportDefinition.Item>();

                    if (list == null) list = new List<ReportDefinition.Item>();
                    int count = result == null ? 0 : result.ReportDefinitions.Count;
                    ListReports.Tag = result.Count;

                    for (int i = 0; i < count; i++)
                    {
                        result.ReportDefinitions[i].Created = StringUtils.ConvertDateTime(result.ReportDefinitions[i].Created);
                        result.ReportDefinitions[i].LastUpdated = StringUtils.ConvertDateTime(result.ReportDefinitions[i].LastUpdated);
                        list.Add(result.ReportDefinitions[i]);
                    }

                    ListReports.ItemsSource = list;
                    ShowHideVerticalScroll(Global.HasVerticalScroll);
                });

            }, 10);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void TextReportSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowReportList(0, 20);
        }

        private void ListReports_Scroll(object sender, ScrollChangedEventArgs e)
        {
            ListView listView = (ListView)e.Source;
            int totalCount = listView.Items.Count;
            int limit = Convert.ToInt32(listView.Tag);
            if (totalCount == 0 || totalCount >= limit) return;
            if (e.VerticalOffset + e.VerticalChange >= totalCount - 6)
            {
                ShowReportList(totalCount, 20, true);
            }
        }

        private async void ListReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListReports == null || ListReports.SelectedItem == null) return;

            ReportDefinition.Item item = (ReportDefinition.Item)ListReports.SelectedItem;
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ReportDetail(item.Id, item.Name), "DetailDialog");
            ListReports.SelectedIndex = -1;
        }

        private async void ButtonSchedule_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string id = button.Tag.ToString();
            string content = button.Content.ToString();
            if (content != Properties.Resources.Run_Now) content += " " + Properties.Resources.Reports;
            string name = "";

            List<ReportDefinition.Item> items = (List<ReportDefinition.Item>)ListReports.ItemsSource;
            foreach (ReportDefinition.Item row in items)
            {
                if (row.Id == id)
                {
                    name = row.Name;
                    break;
                }
            }

            ReportDefinition.Item item = (ReportDefinition.Item)ListReports.SelectedItem;
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ReportSchedule(content, id, name), "DetailDialog");
        }

        public void ShowHideVerticalScroll(bool isChecked)
        {
            ListView[] listViews = new ListView[] { ListReports};

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

    }
}
