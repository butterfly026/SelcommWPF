using Newtonsoft.Json;
using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
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

namespace SelcommWPF.dialogs.services
{
    /// <summary>
    /// Interaction logic for TerminationDialog.xaml
    /// </summary>
    public partial class TerminationDialog : UserControl
    {
        private string ServiceReference = "";
        private bool HasShowDetail = false;

        private Dictionary<string, object> TerminationData;
        private List<UserModel.BusinessUnitModel> ReasonList;

        public TerminationDialog(string serviceReference = "")
        {
            ServiceReference = serviceReference;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            DatePickerTermination.SelectedDate = DateTime.Now;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("Resource", "/ServiceDesk/Services/Terminations");
                query.Add("api-version", 1.2);
                HttpStatusCode result = Global.RequestAPI(Constants.AuthrisationURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, null);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result == HttpStatusCode.OK) GetPlanConfigration();
                    else
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources1.Feature_Error);
                        Global.CloseDialog();
                    }
                });

            }, 10);
        }

        private void GetPlanConfigration()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> header = Global.GetHeader(Global.CustomerToken);
                Dictionary<string, object> path = Global.BuildDictionary("Type", "Terminations");
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.2);

                Dictionary<string, object> services = Global.RequestAPI<Dictionary<string, object>>(Constants.ServicesBasicURL, Method.Get, header, Global.BuildDictionary("ServiceReference", ServiceReference), query, "");

                path.Add("ServiceTypeId", services["ServiceTypeId"].ToString());
                Dictionary<string, object> result = Global.RequestAPIWithStatusCode(Constants.ConfigurationsURL, Method.Get, header, path, query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    HttpStatusCode code = (HttpStatusCode)result["code"];

                    if (code == HttpStatusCode.NoContent)
                    {
                        PanelCheckBoxs.Visibility = Visibility.Visible;
                        WrapOtherServices.Visibility = Visibility.Visible;
                        GetTerminationData();
                    }
                    else if (code == HttpStatusCode.OK)
                    {
                        Dictionary<string, object> response = (Dictionary<string, object>)result["data"];
                        foreach (KeyValuePair<string, object> entry in response)
                        {
                            CheckBox checkBox = Global.FindChild<CheckBox>(MainContent, entry.Key);
                            bool visible = Convert.ToBoolean(entry.Value.ToString());
                            checkBox.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                        }

                        PanelCheckBoxs.Visibility = Visibility.Visible;
                        WrapOtherServices.Visibility = Visibility.Visible;
                        GetTerminationData();
                    }
                    else
                    {
                        MessageUtils.ShowErrorMessage("", "Can't load configurations data");
                        Global.CloseDialog();
                    }

                });


            }, 10);
        }

        private void GetTerminationData()
        {
            string terminateDate = DatePickerTermination.SelectedDate.Value.ToString("yyyy-MM-dd");
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> header = Global.GetHeader(Global.CustomerToken);
                Dictionary<string, object> path = Global.BuildDictionary("ServiceReference", ServiceReference);
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.2);

                TerminationData = Global.RequestAPIWithStatusCode(Constants.TerminationsURL, Method.Get, header, path, query, "");
                ReasonList = Global.RequestAPI<List<UserModel.BusinessUnitModel>>(Constants.TerminateReasonURL, Method.Get, header, null, query, "");

                path.Add("Type", "Information");
                query.Add("TerminationDateTime", terminateDate);
                Dictionary<string, object> TerminateInfoData = Global.RequestAPI<Dictionary<string, object>>(Constants.TerminateInfoURL, Method.Get, header, path, query, "");

                path["Type"] = "Penalty";
                Dictionary<string, object> TerminatePenaltyData = Global.RequestAPI<Dictionary<string, object>>(Constants.TerminateInfoURL, Method.Get, header, path, query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    Dictionary<string, object> configuration = JsonConvert.DeserializeObject<Dictionary<string, object>>(TerminateInfoData["Configuration"].ToString());
                    foreach (KeyValuePair<string, object> entry in configuration)
                    {
                        CheckBox checkBox = Global.FindChild<CheckBox>(MainContent, entry.Key);
                        checkBox.IsChecked = Convert.ToBoolean(entry.Value.ToString());
                    }

                    List<string> list = new List<string>();
                    foreach (UserModel.BusinessUnitModel item in ReasonList) list.Add(item.Name);
                    ComboReason.ItemsSource = list;

                    LabelContractCode.Content = TerminateInfoData["ContractId"] == null ? TerminatePenaltyData["ContractInstanceId"].ToString() : TerminateInfoData["ContractId"].ToString();
                    LabelContractDesc.Content = TerminateInfoData["Contract"].ToString();
                    LabelContractTerm.Content = TerminateInfoData["ContractTerm"].ToString() + " " + TerminateInfoData["ContractTermUnits"].ToString();
                    LabelNoticePeriod.Content = string.Format("{0} {1}", TerminateInfoData["NoticePeriodTerm"] == null ? "0" : TerminateInfoData["NoticePeriodTerm"].ToString(), TerminateInfoData["NoticeTermUnits"]);
                    LabelCoolOffPeriod.Content = TerminateInfoData["CoolOffDays"].ToString() + " Days";
                    LabelDisconnectFee.Content = TerminatePenaltyData["PenaltyFee"].ToString();
                    LabelCalcMethod.Content = TerminatePenaltyData["ContractPenaltyCalculationMethod"].ToString();
                    LabelChargeCode.Content = TerminatePenaltyData["ContractPenaltyCalculationMethodCode"].ToString();
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void ButtonDetail_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            HasShowDetail = !HasShowDetail;
            button.Content = HasShowDetail ? "Hide" : "Detail";
            PanelDetail.Visibility = HasShowDetail ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ComboReason_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonSave.IsEnabled = ComboReason.SelectedIndex != -1;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
