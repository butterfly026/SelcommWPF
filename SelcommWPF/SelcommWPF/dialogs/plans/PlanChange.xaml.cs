using RestSharp;
using SelcommWPF.clients.models;
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
using System.Windows.Input;

namespace SelcommWPF.dialogs.plans
{
    /// <summary>
    /// Interaction logic for PlanChange.xaml
    /// </summary>
    public partial class PlanChange : UserControl
    {
        private bool IsService = false;
        private string ServiceReference = "";

        private List<ServicesPlans.Detail> PlanList;
        private ServicesPlans.Detail SelectedPlan;
        private List<ServicesPlans.Option> OptionList;

        private bool IsSelectedPlan = false;
        private IDisposable RequestTimer = null;

        public PlanChange(bool isService = false, string serviceReference = "")
        {
            InitializeComponent();
            IsService = isService;
            ServiceReference = serviceReference;
            InitializeControl();
        }

        private void InitializeControl()
        {
            LabelDialogTitle.Content = string.Format("Plan Change : [{0}]", IsService ? ServiceReference : Global.ContactCode);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("Resource", IsService ? "/Services/PlanChange" : "/Accounts/PlanChange");
                query.Add("api-version", 1.2);
                HttpStatusCode result = Global.RequestAPI(Constants.AuthrisationURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, null);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result == HttpStatusCode.OK)
                    {
                        if (IsService) GetPlanConfigration();
                        else GetPlanScheduleData();
                    }
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
                Dictionary<string, object> path = Global.BuildDictionary("Type", "PlanChanges");
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
                        WrapOtherServices.Visibility = Visibility.Visible;
                        WrapSetPlans.Visibility = Visibility.Visible;
                    }
                    else if (code == HttpStatusCode.OK)
                    {
                        Dictionary<string, object> response = (Dictionary<string, object>)result["data"];
                        foreach (KeyValuePair<string, object> entry in response)
                        {
                            if (entry.Key == "TimeOfDay")
                            {
                                bool visible = Convert.ToBoolean(entry.Value.ToString());
                                TimeOfDay.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                            }
                            else
                            {
                                CheckBox checkBox = Global.FindChild<CheckBox>(MainContent, entry.Key);
                                bool visible = Convert.ToBoolean(entry.Value.ToString());
                                checkBox.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                            }
                        }

                        WrapOtherServices.Visibility = Visibility.Visible;
                        WrapSetPlans.Visibility = Visibility.Visible;
                        GetPlanScheduleData();
                    }
                    else
                    {
                        MessageUtils.ShowErrorMessage("", "Can't load configurations data");
                        Global.CloseDialog();
                    }
                });

            }, 10);
        }

        private void GetPlanScheduleData()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                string url = IsService ? Constants.ServicesPlanScheduledURL : Constants.PlanScheduledURL;
                Dictionary<string, object> path = IsService ? Global.BuildDictionary("ServiceReference", ServiceReference) : Global.BuildDictionary("AccountCode", Global.ContactCode);

                List <Dictionary<string, object>> result = Global.RequestAPI<List<Dictionary<string, object>>>(url, Method.Get, Global.GetHeader(Global.CustomerToken), path, Global.BuildDictionary("api-version", IsService ? 1.2 : 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    List<string> list = new List<string>();
                    foreach (Dictionary<string, object> item in result) list.Add(item["PlanName"].ToString());
                    ListPlanScheduled.ItemsSource = list;
                });

            }, 10);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void TextPlan_KeyUp(object sender, KeyEventArgs e)
        {
            string search = TextPlan.Text;

            if (search == "" || RequestTimer != null || IsSelectedPlan)
            {
                ListPlans.ItemsSource = new List<string>();
                GridPlans.Visibility = Visibility.Collapsed;
                return;
            }

            if (e.Key == Key.Down)
            {
                ListPlans.Focus();
                ListPlans.SelectedIndex = 0;
                return;
            }

            RequestTimer = EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    search = TextPlan.Text;
                    ListPlans.ItemsSource = new List<string>();
                    ProgressPlans.Visibility = Visibility.Visible;
                    GridPlans.Visibility = Visibility.Collapsed;
                });

                string url = IsService ? Constants.ServicesPlanAvailableURL : Constants.PlanAvailableURL;
                Dictionary<string, object> path = IsService ? Global.BuildDictionary("ServiceReference", ServiceReference) : Global.BuildDictionary("AccountCode", Global.ContactCode);
                Dictionary<string, object> query = Global.BuildDictionary("SkipRecords", 0);
                query.Add("TakeRecords", 10);
                query.Add("CountRecords", "Y");
                query.Add("SearchString", search);
                query.Add("CurrentOnly", true);
                query.Add("api-version", IsService ? 1.2 : 1.0);
                ServicesPlans.Item result = Global.RequestAPI<ServicesPlans.Item>(url, Method.Get, Global.GetHeader(Global.CustomerToken), path, query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result != null && result.Count != 0 && !IsSelectedPlan)
                    {
                        List<string> list = new List<string>();
                        PlanList = result.Plans;
                        foreach (ServicesPlans.Detail item in PlanList) list.Add(item.Plan);
                        ListPlans.ItemsSource = list;
                        GridPlans.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ListPlans.ItemsSource = new List<string>();
                        GridPlans.Visibility = Visibility.Hidden;
                    }

                    RequestTimer.Dispose();
                    RequestTimer = null;
                    ProgressPlans.Visibility = Visibility.Collapsed;
                });

            }, 2000);
        }

        private void ListPlans_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectPlans();
        }

        private void ListPlans_KeyUp(object sender, KeyEventArgs e)
        {
            SelectPlans();
        }

        private void SelectPlans()
        {
            if (ListPlans.SelectedIndex == -1) return;

            IsSelectedPlan = true;
            SelectedPlan = PlanList[ListPlans.SelectedIndex];
            OptionList = SelectedPlan.Options;
            TextPlan.Text = ListPlans.SelectedItem.ToString();

            List<string> list = new List<string>();
            foreach (ServicesPlans.Option item in OptionList) list.Add(item.Name);
            ComboOptions.ItemsSource = list;
            ComboOptions.SelectedIndex = 0;
            GridPlans.Visibility = Visibility.Collapsed;
        }

        private void ComboOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isSelected = ComboOptions.SelectedItem != null;
            int count = WrapOtherServices.Children.Count;

            for (int i = 0; i < count; i++) 
            {
                CheckBox checkBox = WrapOtherServices.Children[i] as CheckBox;
                checkBox.IsEnabled = isSelected;
            }
        }
    }
}
