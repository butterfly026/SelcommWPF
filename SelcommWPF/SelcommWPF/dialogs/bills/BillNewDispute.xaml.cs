using Newtonsoft.Json;
using RestSharp;
using SelcommWPF.clients.models.bills;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace SelcommWPF.dialogs.bills
{
    /// <summary>
    /// Interaction logic for BillNewDispute.xaml
    /// </summary>
    public partial class BillNewDispute : UserControl
    {
        public long BillId;
        public string BillNumber;
        private string CheckString = "";
        private BillDisputeModel.Item DisputeData = new BillDisputeModel.Item();

        string[] StatusLabel = new string[] { "New", "Under Assessment", "Pending Approval", "Approved", "Cancelled", "Declined", "On Hold", "Closed Pending Credit", "Closed Pending Raised" };
        string[] StatusValue = new string[] { "New", "UnderAssessment", "PendingApproval", "Approved", "Cancelled", "Declined", "OnHold", "ClosedPendingCredit", "ClosedPendingRaised" };

        public BillNewDispute(long id, string number, long disputeId = 0)
        {
            BillId = id;
            BillNumber = number;
            DisputeData.Id = disputeId;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            
            LabelDialogTitle.Content = string.Format(Properties.Resources.Bill_Dispute_Title, BillNumber);
            ComboStatus.ItemsSource = StatusLabel;
            ComboStatus.SelectedIndex = 0;
            DatePickerDispute.SelectedDate = DateTime.Now;
            TextDisputedAmount.Text = "0";

            if (DisputeData.Id == 0) return;

            LoadingPanel.Visibility = Visibility.Visible;
            GridForUpdates.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                DisputeData = Global.RequestAPI<BillDisputeModel.Item>(Constants.BillDisputesDetailURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("DisputeId", DisputeData.Id), Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {

                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (DisputeData == null || DisputeData.Id == 0)
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources.Bill_Dispute_Faild);
                        return;
                    }

                    DatePickerDispute.SelectedDate = DateTime.Parse(DisputeData.Date);
                    ComboStatus.SelectedIndex = StatusValue.ToList().IndexOf(DisputeData.Status);
                    CheckString = StringUtils.ConvertCurrency(DisputeData.DisputedAmount.Value);
                    TextDisputedAmount.Text = CheckString;
                    TextRaisedBy.Text = DisputeData.RaisedBy;
                    TextContactDetails.Text = DisputeData.ContactDetails;
                    TextApprovedById.Text = DisputeData.ApprovedBy;
                    TextApprovalNotes.Text = DisputeData.ApprovalNotes;
                    TextSettlementAmount.Text = DisputeData.SettlementAmount + "";
                    TextSettlementTax.Text = DisputeData.SettlementTax + "";
                    TextDetails.Text = StringUtils.Remove_Escape_Sequences(DisputeData.Details);

                });

            }, 10);

        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                string date = DatePickerDispute.SelectedDate.Value.ToString("yyyy-MM-dd");
                string status = ComboStatus.SelectedIndex == -1 ? "" : StatusValue[ComboStatus.SelectedIndex];
                double amount = Convert.ToDouble(TextDisputedAmount.Text.Replace("A$", "").Replace(",", ""));
                string details = TextDetails.Text;

                if (date == "" || status == "" || amount == 0 || details == "")
                {
                    MessageUtils.ShowErrorMessage("", Properties.Resources.Check_Email_Param);
                    return;
                }


                DisputeData.ContactDetails = TextContactDetails.Text;
                DisputeData.Date = date;
                DisputeData.Details = details;
                DisputeData.DisputedAmount = amount;
                DisputeData.RaisedBy = TextRaisedBy.Text;
                DisputeData.Status = status;

                if (DisputeData.Id != 0)
                {
                    DisputeData.ApprovedBy = TextApprovedById.Text;
                    DisputeData.ApprovalNotes = TextApprovalNotes.Text;
                    DisputeData.SettlementAmount = TextSettlementAmount.Text == "" ? 0 : Convert.ToDouble(TextSettlementAmount.Text);
                    DisputeData.SettlementTax = TextSettlementTax.Text == "" ? 0 : Convert.ToDouble(TextSettlementTax.Text);
                }

                LoadingPanel.Visibility = Visibility.Visible;
                EasyTimer.SetTimeout(() =>
                {

                    if (DisputeData.Id == 0)
                    {
                        Dictionary<string, object> result = Global.RequestAPI<Dictionary<string, object>>(Constants.BillDisputesListURL, Method.Post, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("BillId", BillId), Global.BuildDictionary("api-version", 1.0), JsonConvert.SerializeObject(DisputeData));

                        Application.Current.Dispatcher.Invoke(delegate
                        {

                            try
                            {
                                if (result != null && result["Id"] != null)
                                {
                                    Global.CloseDialog("DetailDialog");
                                }
                                else
                                {
                                    MessageUtils.ShowErrorMessage("", result["ErrorMessage"].ToString());
                                    LoadingPanel.Visibility = Visibility.Hidden;
                                }

                            }
                            catch
                            {
                                MessageUtils.ShowErrorMessage("", result["ErrorMessage"].ToString());
                                LoadingPanel.Visibility = Visibility.Hidden;
                            }

                        });
                    }
                    else
                    {
                        HttpStatusCode result = Global.RequestAPI(Constants.BillDisputesDetailURL, Method.Patch, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("DisputeId", DisputeData.Id), Global.BuildDictionary("api-version", 1.0), JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(DisputeData)));

                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            if (result == HttpStatusCode.OK)
                            {
                                Global.CloseDialog("DetailDialog");
                            }
                            else
                            {
                                LoadingPanel.Visibility = Visibility.Hidden;
                                MessageUtils.ShowErrorMessage("", Properties.Resources.Bill_Dispute_Update_Fail);
                            }
                        });
                    }
                   

                }, 10);

            }
            catch
            {
                string message = DisputeData.Id == 0 ? Properties.Resources.Add : Properties.Resources.Update;
                MessageUtils.ShowErrorMessage("", message + Properties.Resources.Dispute_Fail);
                LoadingPanel.Visibility = Visibility.Hidden;
            }

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }

        private void TextPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void TextPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string str = textBox.Text;

            if (str == CheckString)
            {
                textBox.SelectionStart = str.Length;
                CheckString = "";
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
                str = str.Replace("A$", "");
                str = str.Replace(",", "");
                double price = Convert.ToDouble(str) * 10;
                if (price > 10000)
                {
                    MessageUtils.ShowErrorMessage("", string.Format(Properties.Resources.Min_Max_String, 0, 10000));
                    return;
                }
                str = StringUtils.ConvertCurrency(price);
            }
            else
            {
                str = StringUtils.ConvertCurrency(Convert.ToDouble(str) / 100);
            }

            CheckString = str;
            textBox.Text = str;
        }

        private void ComboStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonSave.IsEnabled = ComboStatus.SelectedItem != null && TextDetails.Text != "";
        }

        private void TextDetails_TextChanged(object sender, TextChangedEventArgs e)
        {
            ButtonSave.IsEnabled = ComboStatus.SelectedItem != null && TextDetails.Text != "";
        }
    }
}
