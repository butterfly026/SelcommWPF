using RestSharp;
using SelcommWPF.clients.models.bills;
using SelcommWPF.dialogs.bills.options;
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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SelcommWPF.dialogs.bills
{
    /// <summary>
    /// Interaction logic for BillingOption.xaml
    /// </summary>
    public partial class BillOptions : UserControl
    {
        private List<BillingOption> BillingOptionList = new List<BillingOption>();
        private List<BillFormat> BillFormatList = new List<BillFormat>();
        private List<Dictionary<string, string>> InvoiceIntervalList = new List<Dictionary<string, string>>();
        private List<BillFormat> AvailableCurrencyList = new List<BillFormat>();
        private List<BillTax> TaxList = new List<BillTax>();
        private List<TaxExemption> TaxExemptionList = new List<TaxExemption>();
        private List<BillTax> TaxRateList = new List<BillTax>();
        private List<BillFormat> TaxTransactionTypeList = new List<BillFormat>();
        private List<BillTerms.History> TermsList = new List<BillTerms.History>();
        private bool IsLoading = false;
        private List<string> IntervalList = new List<string>() { "Monthly", "Quarterly", "Half Yourly", "Yearly" };

        public BillOptions()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            Global.BillOptionDialog = this;
            ShowHideVerticalScroll(Global.HasVerticalScroll);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                BillingOptionList = Global.RequestAPI<List<BillingOption>>(Constants.BillOptionsURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.0), "");
                BillFormatList = Global.RequestAPI<List<BillFormat>>(Constants.BillFormatsURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, Global.BuildDictionary("api-version", 1.0), "");
                InvoiceIntervalList = Global.RequestAPI<List<Dictionary<string, string>>>(Constants.InvoiceIntervalsURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, Global.BuildDictionary("api-version", 1.0), "");
                AvailableCurrencyList = Global.RequestAPI<List<BillFormat>>(Constants.AvailableCurrenciesURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.0), "");
                TaxList = Global.RequestAPI<List<BillTax>>(Constants.BillTaxesURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, Global.BuildDictionary("api-version", 1.0), "");
                TaxExemptionList = Global.RequestAPI<List<TaxExemption>>(Constants.TaxExemptionsURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.0), "");
                TaxRateList = Global.RequestAPI<List<BillTax>>(Constants.TaxRatesURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, Global.BuildDictionary("api-version", 1.0), "");
                TaxTransactionTypeList = Global.RequestAPI<List<BillFormat>>(Constants.TaxTransactionTypesURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, Global.BuildDictionary("api-version", 1.0), "");
                TermsList = Global.RequestAPI<List<BillTerms.History>>(Constants.BillTermsURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (BillingOptionList == null || BillFormatList == null || InvoiceIntervalList == null || AvailableCurrencyList == null ||
                        TaxList == null || TaxExemptionList == null || TaxRateList == null || TaxTransactionTypeList == null || TermsList == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    // Bill Format ComboBox
                    List<string> list1 = new List<string>();
                    foreach (BillFormat item in BillFormatList) list1.Add(item.Name);
                    ComboBillFormat.ItemsSource = list1;

                    // Invoice Interval
                    List<string> list2 = new List<string>();
                    foreach (Dictionary<string, string> item in InvoiceIntervalList) list2.Add(item["Interval"]);
                    ComboInvoiceInterval.ItemsSource = list2;

                    // Available Currencies
                    List<string> list3 = new List<string>();
                    foreach (BillFormat item in AvailableCurrencyList) list3.Add(item.Name);
                    ComboCurrency.ItemsSource = list3;

                    List<string> list4 = new List<string>();
                    foreach (BillTax item in TaxList) list4.Add(item.Name);
                    ComboTaxation.ItemsSource = list4;

                    List<string> list5 = new List<string>();
                    foreach (TaxExemption item in TaxExemptionList) list5.Add(item.Tax + " - " + item.TransactionType);
                    ComboExemptions.ItemsSource = list5;

                    foreach(BillingOption item in BillingOptionList)
                    {
                        switch (item.Id)
                        {
                            case "BLFM":
                                ComboBillFormat.IsEnabled = item.Editable;
                                ButtonBillFormat.IsEnabled = item.Editable;
                                ComboBillFormat.SelectedIndex = ((List<string>)ComboBillFormat.ItemsSource).IndexOf(item.Value);
                                if (ButtonBillFormat.IsEnabled) ButtonBillFormat.IsEnabled = ComboBillFormat.SelectedIndex == -1;
                                ComboBillFormat.Tag = item.Value;
                                break;
                            case "P":
                                TogglePaperBill.IsEnabled = item.Editable;
                                TogglePaperBill.IsChecked = item.Value == "TRUE";
                                TogglePaperBill.Tag = item.Value;
                                break;
                            case "E":
                                ToggleEmailBill.IsEnabled = item.Editable;
                                ToggleEmailBill.IsChecked = item.Value == "TRUE";
                                ToggleEmailBill.Tag = item.Value;
                                break;
                            case "X":
                                ToggleExcelBill.IsEnabled = item.Editable;
                                ToggleExcelBill.IsChecked = item.Value == "TRUE";
                                ToggleExcelBill.Tag = item.Value;
                                break;
                            case "BLRT":
                                LabelReturnBill.Content = item.Value == "No return" ? "" : item.Value;
                                break;
                            case "BLPR":
                                ToggleProofing.IsEnabled = item.Editable;
                                ToggleProofing.IsChecked = item.Value == "Yes";
                                break;
                            case "BLCC":
                                LabelBillCycle.Content = item.Value;
                                break;
                            case "BLII":
                                ComboInvoiceInterval.IsEnabled = item.Editable;
                                ButtonInvoiceInterval.IsEnabled = item.Editable;
                                ComboInvoiceInterval.SelectedIndex = IntervalList.IndexOf(item.Value);
                                if (ButtonInvoiceInterval.IsEnabled) ButtonInvoiceInterval.IsEnabled = ComboInvoiceInterval.SelectedIndex == -1;
                                ComboInvoiceInterval.Tag = item.Value;
                                break;
                            case "BLEX":
                                LabelExclusion.Content = item.Value;
                                break;
                            case "TERM":
                                LabelTerm.Content = item.Value;
                                break;
                            case "CUCU":
                                ComboCurrency.IsEnabled = item.Editable;
                                ButtonCurrency.IsEnabled = item.Editable;
                                ComboCurrency.SelectedIndex = ((List<string>)ComboCurrency.ItemsSource).IndexOf(item.Value);
                                if (ButtonCurrency.IsEnabled) ButtonCurrency.IsEnabled = ComboCurrency.SelectedIndex == -1;
                                ComboCurrency.Tag = item.Value;
                                break;
                            case "TAEX":
                                ComboExemptions.IsEnabled = item.Editable;
                                ButtonExemptions.IsEnabled = item.Editable;
                                ComboExemptions.SelectedIndex = ((List<string>)ComboExemptions.ItemsSource).IndexOf(item.Value);
                                if (ButtonExemptions.IsEnabled) ButtonExemptions.IsEnabled = ComboExemptions.SelectedIndex == -1;
                                ComboExemptions.Tag = item.Value;
                                break;
                            case "TATA":
                                ComboTaxation.IsEnabled = item.Editable;
                                ButtonTaxation.IsEnabled = item.Editable;
                                ComboTaxation.SelectedIndex = ((List<string>)ComboTaxation.ItemsSource).IndexOf(item.Value);
                                if (ButtonTaxation.IsEnabled) ButtonTaxation.IsEnabled = ComboExemptions.SelectedIndex == -1;
                                ComboTaxation.Tag = item.Value;
                                break;
                        }
                    }

                    IsLoading = true;

                });

            }, 10);
        }

        private void ShowHideVerticalScroll(bool isChecked)
        {
            ScrollViewer[] scrolls = new ScrollViewer[] { ScrollBillOption };
            int count = scrolls.Length;

            for (int i = 0; i < count; i++) if (scrolls[i] == null) return;
            for (int i = 0; i < count; i++) scrolls[i].VerticalScrollBarVisibility = isChecked ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void ComboBillFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            if (combo.Tag == null || ButtonBillFormat == null) return;

            string tag = combo.Tag.ToString();
            ButtonBillFormat.IsEnabled = tag != combo.SelectedItem.ToString();
        }

        private void TogglePaperBill_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = e.RoutedEvent.Name == "Checked";
            ToggleButton toggle = (ToggleButton)sender;
            if (toggle.Tag == null || ButtonPaperEmail == null) return;

            bool tag = toggle.Tag.ToString() == "TRUE";
            ButtonPaperEmail.IsEnabled = isChecked != tag;
        }

        private void ToggleExcelBill_Unchecked(object sender, RoutedEventArgs e)
        {
            bool isChecked = e.RoutedEvent.Name == "Checked";
            ToggleButton toggle = (ToggleButton)sender;
            if (toggle.Tag == null || ButtonExcelBill == null) return;

            bool tag = toggle.Tag.ToString() == "TRUE";
            ButtonExcelBill.IsEnabled = isChecked != tag;
        }

        private void ComboInvoiceInterval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            if (combo.Tag == null || ButtonInvoiceInterval == null) return;

            string tag = combo.Tag.ToString();
            ButtonInvoiceInterval.IsEnabled = tag != combo.SelectedItem.ToString();
        }

        private void ComboCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            if (combo.Tag == null || ButtonInvoiceInterval == null) return;

            string tag = combo.Tag.ToString();
            ButtonCurrency.IsEnabled = tag != combo.SelectedItem.ToString();
        }

        private void ComboTaxation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            if (combo.Tag == null || ButtonInvoiceInterval == null) return;

            string tag = combo.Tag.ToString();
            ButtonTaxation.IsEnabled = tag != combo.SelectedItem.ToString();
        }

        private void ComboExemptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            if (combo.Tag == null || ButtonInvoiceInterval == null) return;

            string tag = combo.Tag.ToString();
            ButtonExemptions.IsEnabled = tag != combo.SelectedItem.ToString();
        }

        private void ButtonBillFormat_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("Id", BillFormatList[ComboBillFormat.SelectedIndex].Id);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.BillFormatsAccountURL, Method.Put, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.0), body);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK)
                    {
                        MessageUtils.ShowMessage("", Properties.Resources.Bill_Format_Success);
                        ComboBillFormat.Tag = ComboBillFormat.SelectedItem.ToString();
                        ButtonBillFormat.IsEnabled = false;
                    }
                    else MessageUtils.ShowErrorMessage("", Properties.Resources.Bill_Format_Failed);
                });

            }, 10);

        }

        private void ButtonPaperEmail_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("Mail", TogglePaperBill.IsChecked.Value);
            body.Add("Email", ToggleEmailBill.IsChecked.Value);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.BillDeliveryOptionURL, Method.Put, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.0), body);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK)
                    {
                        MessageUtils.ShowMessage("", Properties.Resources.Bill_Format_Success);
                        TogglePaperBill.Tag = TogglePaperBill.IsChecked.Value.ToString().ToUpper();
                        ToggleEmailBill.Tag = ToggleEmailBill.IsChecked.Value.ToString().ToUpper();
                        ButtonPaperEmail.IsEnabled = false;
                    }
                    else MessageUtils.ShowErrorMessage("", Properties.Resources.Bill_Format_Failed);
                });

            }, 10);
        }

        private void ButtonExcelBill_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("Excel", ToggleExcelBill.IsChecked.Value);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.BillMediaOptionURL, Method.Put, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.0), body);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK)
                    {
                        MessageUtils.ShowMessage("", Properties.Resources.Bill_Format_Success);
                        ToggleExcelBill.Tag = ToggleExcelBill.IsChecked.Value.ToString().ToUpper();
                        ButtonExcelBill.IsEnabled = false;
                    }
                    else MessageUtils.ShowErrorMessage("", Properties.Resources.Bill_Format_Failed);
                });

            }, 10);
        }
    
        private async void ButtonReturnBill_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ReturnReason(), "DetailDialog");
        }

        private async void ToggleProofing_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoading) return;
            bool isChecked = e.RoutedEvent.Name == "Checked";

            if (isChecked)
            {
                if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog();
                await MaterialDesignThemes.Wpf.DialogHost.Show(new BillProofing(), "DetailDialog");
            }
            else
            {
                LoadingPanel.Visibility = Visibility.Visible;
                EasyTimer.SetTimeout(() =>
                {
                    Global.RequestAPI(Constants.BillProofingURL, Method.Delete, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.0), null);

                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        LoadingPanel.Visibility = Visibility.Hidden;
                    });

                }, 10);
            }
        }

        private async void ButtonBillCycle_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new BillCycleDialog(), "DetailDialog");
        }

        private void ButtonInvoiceInterval_Click(object sender, RoutedEventArgs e)
        {
            if (ComboInvoiceInterval.SelectedItem == null) return;
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("Interval", ComboInvoiceInterval.SelectedItem.ToString());
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.BillIntervalURL, Method.Put, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.0), body);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK)
                    {
                        MessageUtils.ShowMessage("", Properties.Resources.Interval_Invoice_Success);
                        ComboInvoiceInterval.Tag = ComboInvoiceInterval.SelectedItem.ToString();
                        ButtonInvoiceInterval.IsEnabled = false;
                    }
                    else MessageUtils.ShowErrorMessage("", Properties.Resources.Interval_Invoice_Failed);
                });

            }, 10);
        }

        private async void ButtonExclusion_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ExclusionDialog(), "DetailDialog");
        }

        private async void ButtonTerms_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new BillTermsDialog(), "DetailDialog");
        }

        private void ButtonExemptions_Click(object sender, RoutedEventArgs e)
        {
            if (ComboExemptions.SelectedIndex == -1) return;
            TaxExemption item = TaxExemptionList[ComboExemptions.SelectedIndex];

            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("From", item.From);
            body.Add("To", item.To);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> path = Global.BuildDictionary("ContactCode", Global.ContactCode);
                path.Add("Id", item.Id);
                HttpStatusCode result = Global.RequestAPI(Constants.ExemptionsUpdateURL, Method.Put, Global.GetHeader(Global.CustomerToken), path, Global.BuildDictionary("api-version", 1.0), body);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK)
                    {
                        MessageUtils.ShowMessage("", Properties.Resources1.Exemptions_Success);
                        ComboExemptions.Tag = ComboExemptions.SelectedItem.ToString();
                        ButtonExemptions.IsEnabled = false;
                    }
                    else MessageUtils.ShowErrorMessage("", Properties.Resources1.Exemptions_Failed);
                });

            }, 10);
        }
    }
}
