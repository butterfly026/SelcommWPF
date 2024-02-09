using RestSharp;
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
    /// Interaction logic for ReturnReason.xaml
    /// </summary>
    public partial class ReturnReason : UserControl
    {
        private ComboBox ComboBillReturn;
        private List<BillReturns> BillReturnList = new List<BillReturns>();
        private List<BillReturns> ReturnReasonList = new List<BillReturns>();

        public ReturnReason()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                BillReturnList = Global.RequestAPI<List<BillReturns>>(Constants.BillReturnReasonListURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.0), "");
                ReturnReasonList = Global.RequestAPI<List<BillReturns>>(Constants.BillReturnReasonURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, 
                    Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (BillReturnList == null || ReturnReasonList == null)
                    {
                        Global.CloseDialog("DetailDialog");
                        return;
                    }

                    ListBillReturns.ItemsSource = BillReturnList;
                    ListBillReturns.SelectedIndex = 0;
                });

            }, 10);
        }

        private async void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();
            foreach (BillReturns item in ReturnReasonList) list.Add(item.Reason);

            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;
            panel.Margin = new Thickness(20);

            Label label = new Label();
            label.Content = Properties.Resources.New_Bill_Return;
            label.FontSize = 20;
            label.FontWeight = FontWeight.FromOpenTypeWeight(700);
            label.Margin = new Thickness(10);
            label.HorizontalAlignment = HorizontalAlignment.Center;

            ComboBillReturn = new ComboBox();
            MaterialDesignThemes.Wpf.HintAssist.SetHint(ComboBillReturn, Properties.Resources.Return_Bill1);
            MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(ComboBillReturn, true);
            ComboBillReturn.FontSize = 16;
            ComboBillReturn.HorizontalAlignment = HorizontalAlignment.Center;
            ComboBillReturn.Margin = new Thickness(10);
            ComboBillReturn.ItemsSource = list;
            ComboBillReturn.Width = 400;

            StackPanel panel1 = new StackPanel();
            panel1.Orientation = Orientation.Horizontal;
            panel1.Margin = new Thickness(0, 10, 0, 0);
            panel1.HorizontalAlignment = HorizontalAlignment.Center;

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
            panel.Children.Add(ComboBillReturn);
            panel.Children.Add(panel1);

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("SubDialog")) Global.CloseDialog("SubDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(panel, "SubDialog");

        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            object id = BillReturnList[ListBillReturns.SelectedIndex].Id;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> path = Global.BuildDictionary("ContactCode", Global.ContactCode);
                path.Add("Id", id);
                Global.RequestAPI(Constants.BillReturnReasonDeleteURL, Method.Delete, Global.GetHeader(Global.CustomerToken), path, Global.BuildDictionary("api-version", 1.0), null);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    InitializeControl();
                });

            }, 10);

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.BillOptionDialog.LabelReturnBill.Content = BillReturnList[ListBillReturns.SelectedIndex].Reason;
            Global.CloseDialog("DetailDialog");
        }

        private void ButtonDetailClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("SubDialog");
        }

        private void ButtonDetailCreate_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBillReturn.SelectedIndex == -1)
            {
                ComboBillReturn.Focus();
                return;
            }

            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("ReasonId", ReturnReasonList[ComboBillReturn.SelectedIndex].Id.ToString());
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Global.RequestAPI(Constants.BillReturnReasonListURL, Method.Post, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode),
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
