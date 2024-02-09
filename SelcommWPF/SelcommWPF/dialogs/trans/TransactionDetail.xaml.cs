using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.messages;
using SelcommWPF.global;
using SelcommWPF.utils;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;

namespace SelcommWPF.dialogs
{
    /// <summary>
    /// Interaction logic for TransactionDetailxaml.xaml
    /// </summary>
    public partial class TransactionDetail : UserControl
    {

        private long TransactionId = 0;
        private Transactions.Item TransactionData;

        public TransactionDetail(long id)
        {
            TransactionId = id;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {

            LabelDialogTitle.Content = string.Format(Properties.Resources.Transaction_Title, TransactionId);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                TransactionData = Global.RequestAPI<Transactions.Item>(Constants.TransactionDetilURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("TransactionId", TransactionId), Global.BuildDictionary("api-version", 1.0), "");
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (TransactionData == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    LabelType.Content = TransactionData.Type;
                    LabelNumber.Content = string.Format(Properties.Resources.Number + " : {0}", TransactionData.Number);
                    LabelAmount.Content = string.Format(Properties.Resources.Amount + " : {0}", StringUtils.ConvertCurrency(TransactionData.Amount));
                    LabelTaxAmount.Content = string.Format(Properties.Resources.Tax_Amount + " : {0}", StringUtils.ConvertCurrency(TransactionData.TaxAmount));
                    LabelOpenAmount.Content = string.Format(Properties.Resources.Open_Amount + " : {0}", StringUtils.ConvertCurrency(TransactionData.OpenAmount));
                    LabelDate.Content = string.Format(Properties.Resources.Date + " : {0}", StringUtils.ConvertDateTime(TransactionData.Date));
                    LabelDueDate.Content = string.Format(Properties.Resources.Due_Date + " : {0}", StringUtils.ConvertDateTime(TransactionData.DueDate));
                    LabelCategory.Content = string.Format(Properties.Resources.Category + " : {0}", TransactionData.Category);
                    Labelstatus.Content = string.Format(Properties.Resources.Status + " : {0}", TransactionData.Status);
                    LabelSource.Content = string.Format(Properties.Resources.Source + " : {0}", TransactionData.Source);
                    LabelReason.Content = string.Format(Properties.Resources.Reason + " : {0}", TransactionData.Reason);
                    LabelReference.Content = string.Format(Properties.Resources.Service_Reference + " : {0}", TransactionData.ServiceReference);
                    LabelReference.Content = string.Format(Properties.Resources.Bill_Number + " : {0}", TransactionData.BillNumber);

                    LabelCreated.Content = StringUtils.ConvertDateTime(TransactionData.Created);
                    LabelCreatedBy.Content = TransactionData.CreatedBy;
                    LabelUpdated.Content = StringUtils.ConvertDateTime(TransactionData.LastUpdated);
                    LabelUpdatedBy.Content = TransactionData.UpdatedBy;

                    PanelDetail.Visibility = Visibility.Visible;
                    ListAllocations.ItemsSource = TransactionData.Allocations;
                    ListDisturibution.ItemsSource = TransactionData.Distributions;
                    ListEvent.ItemsSource = TransactionData.Events;
                    ListPayRequest.ItemsSource = TransactionData.PayRequests;
                    ListExternal.ItemsSource = TransactionData.ExternalTransactions;

                });

            }, 10);

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }
    }
}
