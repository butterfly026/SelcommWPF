using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.payment;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SelcommWPF.dialogs.trans
{
    /// <summary>
    /// Interaction logic for ReceipDialog.xaml
    /// </summary>
    public partial class ReceiptDialog : UserControl
    {

        private string CheckString = "";
        private List<AllocationModel> AllocationList;
        private string SelectedPaymentMethodId;
        private double SurchargeAmount = 0;

        public ReceiptDialog()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {

            CheckAutoAllocation.IsChecked = true;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("Open", true);
                query.Add("DefaultOnly", false);
                Dictionary<string, string> receiptNumber = Global.RequestAPI<Dictionary<string, string>>(Constants.TransactionNumberURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("Type", "R"), query, "");
                query.Clear();
                query.Add("api-version", "1.1");
                query.Add("Open", true);
                query.Add("DefaultOnly", false);
                List<PaymentMethod> paymentMethodList = Global.RequestAPI<List<PaymentMethod>>(Constants.PayMethodListURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (receiptNumber == null || paymentMethodList == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    TextNumber.Text = receiptNumber["Number"];
                    TextAmount.Text = "0";
                    DatePickerReceipt.SelectedDate = DateTime.Now;
                    TextCash.Text = "0";
                    TextCheque.Text = "0";
                    BitmapImage logo = new BitmapImage(new Uri("/SelcommWPF;component/resources/ic_credit_card.png", UriKind.Relative));
                    paymentMethodList = paymentMethodList ?? new List<PaymentMethod>();

                    foreach (PaymentMethod item in paymentMethodList)
                    {
                        StackPanel panel = new StackPanel();
                        panel.Orientation = Orientation.Vertical;
                        panel.Margin = new Thickness(20);

                        Grid grid = new Grid();
                        ColumnDefinition col1 = new ColumnDefinition();
                        col1.Width = new GridLength(1, GridUnitType.Star);
                        grid.ColumnDefinitions.Add(col1);
                        ColumnDefinition col2 = new ColumnDefinition();
                        col1.Width = GridLength.Auto;
                        grid.ColumnDefinitions.Add(col2);

                        Image image = new Image();
                        image.Width = 200;
                        image.Height = 30;
                        image.Source = logo;
                        grid.Children.Add(image);
                        Grid.SetColumn(image, 0);

                        MaterialDesignThemes.Wpf.PackIcon icon = new MaterialDesignThemes.Wpf.PackIcon();
                        icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckCircle;
                        icon.Foreground = Brushes.Green;
                        icon.VerticalAlignment = VerticalAlignment.Center;
                        icon.Visibility = Visibility.Hidden;

                        grid.Children.Add(icon);
                        Grid.SetColumn(icon, 1);
                        panel.Children.Add(grid);

                        Label label1 = new Label();
                        item.CreditCard = string.Format("{0} ending in {1}", item.Description, item.AccountNumber.Substring(item.AccountNumber.Length - 4));
                        label1.Content = string.Format("{0} : {1}", Properties.Resources.Credit_Card, item.CreditCard);
                        label1.FontSize = 16;
                        panel.Children.Add(label1);

                        Label label2 = new Label();
                        label2.Content = string.Format("{0} : {1}", Properties.Resources.Name_on_Card, item.AccountName);
                        label2.FontSize = 16;
                        panel.Children.Add(label2);

                        Label label3 = new Label();
                        label3.Content = string.Format("{0} : {1}", Properties.Resources.Expires_On, item.ExpiryDate);
                        label3.FontSize = 16;
                        panel.Children.Add(label3);


                        Button button = new Button();
                        button.Background = Brushes.Transparent;
                        button.BorderBrush = Brushes.White;
                        button.Width = 320;
                        button.Height = 180;
                        button.Margin = new Thickness(40, 20, 40, 20);
                        button.Tag = item.Id;
                        button.Click += ButtonChooseCard_Click;


                        button.Content = panel;
                        PanelCreditCard.Children.Add(button);
                    }

                    GetAllocationList();

                });

            }, 10);

        }

        private void GetAllocationList(bool isOpenOnly = true)
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.1);
                query.Add("api-version", "1.0");
                query.Add("SkipRecords", 0);
                query.Add("TakeRecords", 100);
                query.Add("OpenOnly", isOpenOnly);
                query.Add("OpenFirst", true);
                List<AllocationModel> result = Global.RequestAPI<List<AllocationModel>>(Constants.AllocationListURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("AccountCode", Global.ContactCode), query, "");
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null) return;

                    int count = result.Count;
                    for (int i = 0; i < count; i++)
                    {
                        result[i].AllocateAmountText = StringUtils.ConvertCurrency(result[i].AllocateAmount);
                        result[i].AmountText = StringUtils.ConvertCurrency(result[i].Amount);
                        result[i].OpenAmountText = StringUtils.ConvertCurrency(result[i].OpenAmount);
                    }

                    AllocationList = result;
                    ListAllocations.ItemsSource = result;
                    AllocatedAmount();
                    ShowHideVerticalScroll(Global.HasVerticalScroll);
                });

            }, 10);
        }

        private void TextAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void TextAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            TextBox textBox = (TextBox)sender;
            string str = textBox.Text;

            if (str == CheckString)
            {
                textBox.SelectionStart = str.Length;
                CheckString = "";
                CheckOverTotalAmount((TextBox)sender);
                ButtonSave.IsEnabled = TextAmount.Text != "";
                return;
            }

            if (str.Length > 19)
            {
                textBox.Text = str.Substring(0, 19);
                textBox.SelectionStart = str.Length;
                CheckString = textBox.Text;
                return;
            }

            if (str.Contains("A$"))
            {
                TextChange change = e.Changes.First();
                str = str.Replace("A$", "").Replace(",", "");
                if (change.AddedLength == 0 && change.RemovedLength > 0) 
                {

                    double price = Convert.ToDouble(str) / 10;
                    str = StringUtils.ConvertCurrency(price);
                }
                else
                {

                    double price = Convert.ToDouble(str) * 10;
                    str = StringUtils.ConvertCurrency(price);
                }
            }
            else
            {
                str = StringUtils.ConvertCurrency(Convert.ToDouble(str) / 100);
            }

            CheckString = str;
            textBox.Text = str;
        }

        private void CheckOverTotalAmount(TextBox textBox)
        {
            try
            {
                if (textBox == TextAmount)
                {
                    TextCash.Text = "0";
                    TextCheque.Text = "0";
                    AllocatedAmount();
                    return;
                } 

                if (textBox != TextCash && textBox != TextCheque)
                {
                    long id = Convert.ToInt64(textBox.Tag.ToString());
                    List<AllocationModel> list = (List<AllocationModel>)ListAllocations.ItemsSource;
                    int count = list.Count;

                    double oldAmount = 0;
                    double sumAmount = 0;
                    double currentAmount = 0;
                    double beforeAmount = 0;
                    int index = 0;

                    for (int i = 0; i < count; i++)
                    {
                        if (list[i].Id == id)
                        {
                            index = i;
                            oldAmount = list[i].AllocateAmount;
                            list[i].AllocateAmountText = textBox.Text;
                            list[i].AllocateAmount = Convert.ToDouble(textBox.Text.Replace("A$", "").Replace(",", ""));
                            currentAmount = list[i].AllocateAmount;
                        }
                        if (index == 0) beforeAmount += list[i].AllocateAmount;
                        sumAmount += list[i].AllocateAmount;
                    }

                    double amount = Convert.ToDouble(TextAmount.Text.Replace("A$", "").Replace(",", ""));
                    if (amount <= beforeAmount)
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources.Not_Allocate_Amount);
                        list[index].AllocateAmount = 0;
                        list[index].AllocateAmountText = StringUtils.ConvertCurrency(list[index].AllocateAmount);
                    }
                    else if (amount < sumAmount)
                    {
                        double limitAmount = Math.Round(amount - (sumAmount - currentAmount), 2);
                        MessageUtils.ShowErrorMessage("", string.Format(Properties.Resources.Shoud_Be_Allocate, limitAmount));
                        list[index].AllocateAmount = oldAmount;
                        list[index].AllocateAmountText = StringUtils.ConvertCurrency(list[index].AllocateAmount);
                    }

                    ListAllocations.ItemsSource = new List<AllocationModel>();
                    ListAllocations.ItemsSource = list;
                    GetRemainingAmount();
                    return;
                }

                double totalAmount = Convert.ToDouble(TextAmount.Text.Replace("A$", "").Replace(",", ""));
                double cashAmount = Convert.ToDouble(TextCash.Text.Replace("A$", "").Replace(",", ""));
                double chequeAmount = Convert.ToDouble(TextCheque.Text.Replace("A$", "").Replace(",", ""));

                bool isVisible = chequeAmount > 0;
                TextChequeNumber.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                TextBSB.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;

                if (totalAmount < cashAmount + chequeAmount)
                {
                    MessageUtils.ShowErrorMessage("", Properties.Resources.Amount_Invalid);
                    if (textBox == TextCash)
                    {
                        cashAmount = totalAmount - chequeAmount;
                        TextCash.Text = (cashAmount / 0.01 + "").Replace(".", "");
                    }
                    else if (textBox == TextCheque)
                    {
                        chequeAmount = totalAmount - cashAmount;
                        TextCheque.Text = (chequeAmount / 0.01 + "").Replace(".", "");
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void ButtonChooseCard_Click(object sender, RoutedEventArgs e)
        {
            string id = ((Button)sender).Tag.ToString();
            double amount = Convert.ToDouble(TextAmount.Text.Replace("A$", ""));
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> path = Global.BuildDictionary("PaymentMethodId", id);
                path.Add("Amount", amount);
                path.Add("Source", "ServiceDesk");
                Dictionary<string, double> result = Global.RequestAPI<Dictionary<string, double>>(Constants.SurchargeURL, Method.Get, Global.GetHeader(Global.CustomerToken), path, Global.BuildDictionary("api-version", 1.1), "");
                Application.Current.Dispatcher.Invoke(delegate
                {
                    int count = PanelCreditCard.Children.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Button button = (Button)PanelCreditCard.Children[i];
                        StackPanel panel = (StackPanel)button.Content;
                        Grid grid = (Grid)panel.Children[0];
                        MaterialDesignThemes.Wpf.PackIcon icon = (MaterialDesignThemes.Wpf.PackIcon)grid.Children[1];
                        icon.Visibility = button.Tag.ToString() == id ? Visibility.Visible : Visibility.Hidden;
                    }

                    SurchargeAmount = result["SurchargeAmount"];
                    SelectedPaymentMethodId = id;
                    TextSurcharge.Text = SurchargeAmount / 0.01 + "";
                    LabelTotalAmount.Content = string.Format("{0} : {1}", Properties.Resources.Allocated_Amount, StringUtils.ConvertCurrency(amount + SurchargeAmount));
                    TextSurcharge.Visibility = Visibility.Visible;
                    LabelTotalAmount.Visibility = Visibility.Visible;
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);

        }

        private void ShowHideVerticalScroll(bool isChecked)
        {
            ListView[] listViews = new ListView[] { ListAllocations };
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

            ScrollViewer[] scrolls = new ScrollViewer[] { ScrollReceipt };
            count = scrolls.Length;

            for (int i = 0; i < count; i++) if (scrolls[i] == null) return;
            for (int i = 0; i < count; i++) scrolls[i].VerticalScrollBarVisibility = isChecked ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void CheckAutoAllocation_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = e.RoutedEvent.Name == "Checked";
            PanelAllocation.Visibility = isChecked ? Visibility.Collapsed : Visibility.Visible;
            ShowHideVerticalScroll(Global.HasVerticalScroll);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                TabControl tab = (TabControl)e.Source;
                if (tab.SelectedIndex == 0) return;
                AllocatedAmount();
            }
        }

        private void AllocatedAmount()
        {

            LabelTAmountAllocate.Content = TextAmount.Text;
            double totalAmount = Convert.ToDouble(TextAmount.Text.Replace("A$", "").Replace(",", ""));
            if (totalAmount == 0) return;

            List<AllocationModel> list = (List<AllocationModel>)ListAllocations.ItemsSource;
            int count = list.Count;

            for (int i = 0; i < count; i++)
            {
                if (totalAmount > list[i].OpenAmount)
                {
                    list[i].AllocateAmount = list[i].OpenAmount;
                    list[i].AllocateAmountText = list[i].OpenAmountText;
                    totalAmount -= list[i].OpenAmount;
                }
                else
                {
                    list[i].AllocateAmount = totalAmount;
                    list[i].AllocateAmountText = StringUtils.ConvertCurrency(totalAmount);
                    totalAmount -= list[i].OpenAmount;
                    break;
                }
            }

            ListAllocations.ItemsSource = new List<AllocationModel>();
            ListAllocations.ItemsSource = list;
            LabelRAmountAllocate.Content = StringUtils.ConvertCurrency(totalAmount < 0 ? 0 : totalAmount);
        }

        private void GetRemainingAmount()
        {
            double totalAmount = Convert.ToDouble(TextAmount.Text.Replace("A$", "").Replace(",", ""));
            if (totalAmount == 0) return;
            List<AllocationModel> list = (List<AllocationModel>)ListAllocations.ItemsSource;
            foreach (AllocationModel item in list) totalAmount -= item.AllocateAmount;
            LabelRAmountAllocate.Content = StringUtils.ConvertCurrency(totalAmount < 0 ? 0 : totalAmount);
        }

        private void CheckShowZero_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = e.RoutedEvent.Name == "Checked";
            GetAllocationList(!isChecked);
        }

        private void CheckBestMatch_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = e.RoutedEvent.Name == "Checked";
            LabelRAmountAllocate.Content = "A$0.00";
            double totalAmount = Convert.ToDouble(TextAmount.Text.Replace("A$", "").Replace(",", ""));
            if (totalAmount == 0) return;

            if (isChecked)
            {
                List<AllocationModel> list = new List<AllocationModel>();
                int count = AllocationList.Count;

                for (int i = 0; i < count; i++)
                {
                    if (AllocationList[i].OpenAmount == totalAmount)
                    {
                        list.Add(AllocationList[i]);
                        break;
                    }
                }

                if (list.Count == 1)
                {
                    list[0].AllocateAmount = totalAmount;
                    list[0].AllocateAmountText = StringUtils.ConvertCurrency(totalAmount);
                }

                ListAllocations.ItemsSource = new List<AllocationModel>();
                ListAllocations.ItemsSource = list;
            }
            else
            {
                ListAllocations.ItemsSource = new List<AllocationModel>();
                ListAllocations.ItemsSource = AllocationList;
                AllocatedAmount();
            }
        }

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {

            bool confirm = await MessageUtils.ConfirmMessageAsync("", Properties.Resources.Confirm_Collect_Money);

            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("Amount", Convert.ToDouble(TextAmount.Text.Replace("A$", "").Replace(",", "")));
            body.Add("AutoAllocate", CheckAutoAllocation.IsChecked.Value);
            body.Add("Collect", confirm);
            body.Add("ChequeNumber", TextChequeNumber.Text);
            body.Add("CreateDocument", CheckDocument.IsChecked.Value);
            body.Add("Email", CheckEmail.IsChecked.Value);
            body.Add("Date", DatePickerReceipt.SelectedDate.Value.ToString("yyyy-MM-dd"));
            body.Add("Number", TextNumber.Text);
            body.Add("OtherReference", TextReference.Text);
            body.Add("PaymentMethodId", Convert.ToInt64(SelectedPaymentMethodId));
            body.Add("SurchargeAmount", SurchargeAmount);
            body.Add("SourceCode", Global.LoggedUser.BusinessUnits[0].Id);
            body.Add("StatusCode", "CR");

            List<object> allocations = new List<object>();
            List<AllocationModel> list = (List<AllocationModel>)ListAllocations.ItemsSource;
            
            foreach(AllocationModel item in list) allocations.Add(new { item.Id, Amount = item.AllocateAmount });
            body.Add("Allocations", allocations);

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.ReceiptCreateURL, Method.Post, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("AccountCode", Global.ContactCode),
                    Global.BuildDictionary("api-version", 1.0), body);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result == HttpStatusCode.OK || result == HttpStatusCode.Created)
                    {
                        MessageUtils.ShowMessage("", Properties.Resources.Receipt_Create_Success);
                        Global.CloseDialog();
                    }
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);

        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            List<AllocationModel> list = (List<AllocationModel>)ListAllocations.ItemsSource;
            int count = list.Count;

            for (int i = 0; i < count; i++)
            {
                list[i].AllocateAmount = 0;
                list[i].AllocateAmountText = StringUtils.ConvertCurrency(list[i].AllocateAmount);
            }

            ListAllocations.ItemsSource = new List<AllocationModel>();
            ListAllocations.ItemsSource = list;
        }

        private async void ButtonAddCreditCard_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) MaterialDesignThemes.Wpf.DialogHost.Close("DetailDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(new CreditCard(Global.ContactCode, "DetailDialog"), "DetailDialog");
        }

        private async void ButtonAddBank_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) MaterialDesignThemes.Wpf.DialogHost.Close("DetailDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(new BankAccount(Global.ContactCode, "DetailDialog"), "DetailDialog");
        }
    }
}
