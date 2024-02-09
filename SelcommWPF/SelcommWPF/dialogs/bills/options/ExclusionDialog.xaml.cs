using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.bills;
using SelcommWPF.global;
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

namespace SelcommWPF.dialogs.bills.options
{
    /// <summary>
    /// Interaction logic for BillExclusions.xaml
    /// </summary>
    public partial class ExclusionDialog : UserControl
    {
        private ComboBox ComboStartPeriod;
        private ComboBox ComboEndPeriod;
        private List<BillExclusions.History> ExclusionList = new List<BillExclusions.History>();
        private List<BillTerms.History> BillPeriodList = new List<BillTerms.History>();

        public ExclusionDialog()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                BillExclusions result = Global.RequestAPI<BillExclusions>(Constants.BillRunExclusionsURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.0), "");
                BillPeriodList = Global.RequestAPI<List<BillTerms.History>>(Constants.BillPeriodsURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    null, Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog("DetailDialog");
                        return;
                    }

                    ExclusionList = result.BillRunExclusionHistory;
                    ListExclusions.ItemsSource = ExclusionList;
                    ListExclusions.SelectedIndex = 0;
                });

            }, 10);
        }

        private async void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            List<long> list = new List<long>();
            foreach (BillTerms.History item in BillPeriodList) list.Add(Convert.ToInt64(item.Id));
            list.Sort((a, b) => b.CompareTo(a));

            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;
            panel.Margin = new Thickness(20);

            Label label = new Label();
            label.Content = Properties.Resources.New_Bill_Return;
            label.FontSize = 20;
            label.FontWeight = FontWeight.FromOpenTypeWeight(700);
            label.Margin = new Thickness(10);
            label.HorizontalAlignment = HorizontalAlignment.Center;

            StackPanel panel2 = new StackPanel();
            panel2.Orientation = Orientation.Horizontal;
            panel2.Margin = new Thickness(0, 10, 0, 0);

            ComboStartPeriod = new ComboBox();
            MaterialDesignThemes.Wpf.HintAssist.SetHint(ComboStartPeriod, Properties.Resources.Start_Period);
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(ComboStartPeriod, true);
            ComboStartPeriod.FontSize = 16;
            ComboStartPeriod.HorizontalAlignment = HorizontalAlignment.Center;
            ComboStartPeriod.Margin = new Thickness(10);
            ComboStartPeriod.ItemsSource = list;
            ComboStartPeriod.SelectionChanged += ComboStartPeriod_SelectionChanged;
            ComboStartPeriod.Width = 400;
            panel2.Children.Add(ComboStartPeriod);

            ComboEndPeriod = new ComboBox();
            MaterialDesignThemes.Wpf.HintAssist.SetHint(ComboEndPeriod, Properties.Resources.End_Period);
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(ComboEndPeriod, true);
            ComboEndPeriod.FontSize = 16;
            ComboEndPeriod.HorizontalAlignment = HorizontalAlignment.Center;
            ComboEndPeriod.Margin = new Thickness(10);
            ComboEndPeriod.Width = 400;
            panel2.Children.Add(ComboEndPeriod);

            StackPanel panel1 = new StackPanel();
            panel1.Orientation = Orientation.Horizontal;
            panel1.Margin = new Thickness(0, 10, 0, 0);

            Button buttonCreate = new Button();
            buttonCreate.Content = Properties.Resources.Create;
            buttonCreate.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            buttonCreate.Margin = new Thickness(10);
            buttonCreate.Click += ButtonDetailCreate_Click;
            panel1.Children.Add(buttonCreate);

            Button buttonClose = new Button();
            buttonClose.Content = Properties.Resources.Close;
            buttonClose.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            buttonClose.Margin = new Thickness(10);
            buttonClose.Click += ButtonDetailClose_Click;
            panel1.Children.Add(buttonClose);

            panel.Children.Add(label);
            panel.Children.Add(panel2);
            panel.Children.Add(panel1);

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("SubDialog")) Global.CloseDialog("SubDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(panel, "SubDialog");
        }

        private async void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            List<long> list = new List<long>();
            foreach (BillTerms.History item in BillPeriodList) list.Add(Convert.ToInt64(item.Id));
            list.Sort((a, b) => b.CompareTo(a));
            
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;
            panel.Margin = new Thickness(20);

            Label label = new Label();
            label.Content = Properties.Resources.New_Bill_Return;
            label.FontSize = 20;
            label.FontWeight = FontWeight.FromOpenTypeWeight(700);
            label.Margin = new Thickness(10);
            label.HorizontalAlignment = HorizontalAlignment.Center;

            StackPanel panel2 = new StackPanel();
            panel2.Orientation = Orientation.Horizontal;
            panel2.Margin = new Thickness(0, 10, 0, 0);

            ComboStartPeriod = new ComboBox();
            MaterialDesignThemes.Wpf.HintAssist.SetHint(ComboStartPeriod, Properties.Resources.Start_Period);
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(ComboStartPeriod, true);
            ComboStartPeriod.FontSize = 16;
            ComboStartPeriod.HorizontalAlignment = HorizontalAlignment.Center;
            ComboStartPeriod.Margin = new Thickness(10);
            ComboStartPeriod.ItemsSource = list;
            ComboStartPeriod.SelectionChanged += ComboStartPeriod_SelectionChanged;
            ComboStartPeriod.Width = 400;
            panel2.Children.Add(ComboStartPeriod);

            ComboEndPeriod = new ComboBox();
            MaterialDesignThemes.Wpf.HintAssist.SetHint(ComboEndPeriod, Properties.Resources.End_Period);
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(ComboEndPeriod, true);
            ComboEndPeriod.FontSize = 16;
            ComboEndPeriod.HorizontalAlignment = HorizontalAlignment.Center;
            ComboEndPeriod.Margin = new Thickness(10);
            ComboEndPeriod.Width = 400;

            BillExclusions.History selectedItem = (BillExclusions.History)ListExclusions.SelectedItem;
            ComboEndPeriod.Tag = selectedItem.EndPeriod;
            ComboStartPeriod.SelectedIndex = list.IndexOf(selectedItem.StartPeriod);
            panel2.Children.Add(ComboEndPeriod);

            StackPanel panel1 = new StackPanel();
            panel1.Orientation = Orientation.Horizontal;
            panel1.HorizontalAlignment = HorizontalAlignment.Center;
            panel1.Margin = new Thickness(0, 10, 0, 0);

            Button buttonUpdate = new Button();
            buttonUpdate.Content = Properties.Resources.Update;
            buttonUpdate.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            buttonUpdate.Margin = new Thickness(10);
            buttonUpdate.Click += ButtonDetailUpdate_Click;
            panel1.Children.Add(buttonUpdate);

            Button buttonClose = new Button();
            buttonClose.Content = Properties.Resources.Close;
            buttonClose.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            buttonClose.Margin = new Thickness(10);
            buttonClose.Click += ButtonDetailClose_Click;
            panel1.Children.Add(buttonClose);

            panel.Children.Add(label);
            panel.Children.Add(panel2);
            panel.Children.Add(panel1);

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("SubDialog")) Global.CloseDialog("SubDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(panel, "SubDialog");
        }

        private void ComboStartPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboEndPeriod == null) return;
            long selectedItem = (long)ComboStartPeriod.SelectedItem;
            List<long> list = (List<long>)ComboStartPeriod.ItemsSource;
            List<long> list1 = new List<long>();
            
            foreach(long item in list)
            {
                if (item >= selectedItem) list1.Add(item);
                else break;
            }

            ComboEndPeriod.ItemsSource = list1;
            ComboEndPeriod.SelectedIndex = ComboEndPeriod.Tag == null ? -1 : list1.IndexOf(Convert.ToInt64(ComboEndPeriod.Tag));
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            object id = ExclusionList[ListExclusions.SelectedIndex].Id;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> path = Global.BuildDictionary("ContactCode", Global.ContactCode);
                path.Add("Id", id);
                Global.RequestAPI(Constants.BillExclusionsDetailURL, Method.Delete, Global.GetHeader(Global.CustomerToken), path, Global.BuildDictionary("api-version", 1.0), null);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    InitializeControl();
                });

            }, 10);

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }

        private void ButtonDetailClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("SubDialog");
        }

        private void ButtonDetailCreate_Click(object sender, RoutedEventArgs e)
        {
            if (ComboStartPeriod.SelectedIndex == -1)
            {
                ComboStartPeriod.Focus();
                return;
            }

            if (ComboEndPeriod.SelectedIndex == -1)
            {
                ComboEndPeriod.Focus();
                return;
            }

            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("StartPeriod", Convert.ToInt64(ComboStartPeriod.SelectedItem));
            body.Add("EndPeriod", Convert.ToInt64(ComboEndPeriod.SelectedItem));
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Global.RequestAPI(Constants.BillRunExclusionsURL, Method.Post, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode),
                    Global.BuildDictionary("api-version", 1.0), body, false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    Global.CloseDialog("SubDialog");
                    InitializeControl();
                });

            }, 10);

        }

        private void ButtonDetailUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (ComboStartPeriod.SelectedIndex == -1)
            {
                ComboStartPeriod.Focus();
                return;
            }

            if (ComboEndPeriod.SelectedIndex == -1)
            {
                ComboEndPeriod.Focus();
                return;
            }

            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("StartPeriod", Convert.ToInt64(ComboStartPeriod.SelectedItem));
            body.Add("EndPeriod", Convert.ToInt64(ComboEndPeriod.SelectedItem));
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Global.RequestAPI(Constants.BillRunExclusionsURL, Method.Put, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode),
                    Global.BuildDictionary("api-version", 1.0), body, false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    Global.CloseDialog("SubDialog");
                    InitializeControl();
                });

            }, 10);

        }

    }
}
