using MaterialDesignColors;
using Newtonsoft.Json;
using SelcommWPF.clients.models;
using SelcommWPF.dialogs.contacts;
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

namespace SelcommWPF.dialogs.accounts
{
    /// <summary>
    /// Interaction logic for ServiceSQDialog.xaml
    /// </summary>
    public partial class ServiceSQDialog : UserControl
    {
        private List<Dictionary<string, object>> SQList = new List<Dictionary<string, object>>();
        private Dictionary<string, object> SelectedSQData = null;
        private ServicesPlans.Item PlanDetailData;

        private IDisposable RequestTimer = null;
        private bool SelectedAddressStatus = false;

        public ServiceSQDialog()
        {
            InitializeComponent();
        }

        private void ShowHideVerticalScroll(bool isChecked)
        {
            ListView[] listViews = new ListView[] { ListSQAddress, ListProduct };
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

            ScrollViewer[] scrolls = new ScrollViewer[] { ScrollViewProduct };
            count = scrolls.Length;

            for (int i = 0; i < count; i++) if (scrolls[i] == null) return;
            for (int i = 0; i < count; i++) scrolls[i].VerticalScrollBarVisibility = isChecked ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            string tag = ((Button)sender).Tag as string;
            Global.CloseDialog(tag);
        }

        private void TextSQAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextSQAddress.Text == "")
            {
                ListSQAddress.ItemsSource = new List<string>();
                CardSQAddress.Visibility = Visibility.Collapsed;
                ListProduct.ItemsSource = new List<ServicesPlans.Detail>();
            }
        }

        private void TextSQAddress_KeyUp(object sender, KeyEventArgs e)
        {

            string search = TextSQAddress.Text;

            if (search == "" || RequestTimer != null || SelectedAddressStatus)
            {
                ListSQAddress.ItemsSource = new List<string>();
                CardSQAddress.Visibility = Visibility.Collapsed;
                return;
            }

            if (e.Key == Key.Down)
            {
                ListSQAddress.Focus();
                ListSQAddress.SelectedIndex = 0;
            }

            RequestTimer = EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    search = TextSQAddress.Text;
                    LoadingPanel.Visibility = Visibility.Visible;
                    CardSQAddress.Visibility = Visibility.Collapsed;
                    ListProduct.ItemsSource = new List<ServicesPlans.Detail>();
                });

                SQList = Global.servicesClient.GetServiceQualifications(Constants.ServiceQualificationURL, 0, 20, search);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    List<string> list = new List<string>();
                    foreach (Dictionary<string, object> item in SQList) list.Add(item["Address"].ToString());
                    ListSQAddress.ItemsSource = list;
                    CardSQAddress.Visibility = list.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                    ShowHideVerticalScroll(Global.HasVerticalScroll);
                    RequestTimer.Dispose();
                    RequestTimer = null;
                });

            }, 2000);

        }

        private void ListSQAddress_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ListSQAddress.SelectedItem == null) return;
            TextSQAddress.Text = ListSQAddress.SelectedItem.ToString();
            SelectedSQData = SQList[ListSQAddress.SelectedIndex];
            ListSQAddress.ItemsSource = new List<string>();   
            CardSQAddress.Visibility = Visibility.Collapsed;

            List<Dictionary<string, object>> list = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(SelectedSQData["Services"].ToString());
            List<ServicesPlans.Detail> details = new List<ServicesPlans.Detail>();

            foreach (Dictionary<string, object> item in list)
            {
                ServicesPlans.Detail detail = new ServicesPlans.Detail();
                List<Dictionary<string, object>> plans = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(item["Plans"].ToString());
                if (plans != null && plans.Count > 0) detail.PlanId = Convert.ToInt32(plans[0]["Id"].ToString());
                detail.TypeId = item["TypeId"].ToString();
                detail.Type = item["Type"].ToString();
                detail.Plan = item["Technology"].ToString();
                detail.TypeDefault = Convert.ToBoolean(item["RequiresTechnicalApproval"].ToString());
                detail.Group = item["Network"].ToString();
                detail.GroupId = item["MaximumBandwidth"].ToString();
                detail.DisplayName = JsonConvert.SerializeObject(item);
                details.Add(detail);
            }

            ListProduct.ItemsSource = new List<ServicesPlans.Detail>();
            ListProduct.ItemsSource = details;
            ShowHideVerticalScroll(Global.HasVerticalScroll);
            ListProduct.Visibility = Visibility.Visible;
        }

        private void ListSQAddress_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ListSQAddress.SelectedItem == null) return;
                TextSQAddress.Text = ListSQAddress.SelectedItem.ToString();
                SelectedSQData = SQList[ListSQAddress.SelectedIndex];
                ListSQAddress.ItemsSource = new List<string>();
                CardSQAddress.Visibility = Visibility.Collapsed;

                List<Dictionary<string, object>> list = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(SelectedSQData["Services"].ToString());
                List<ServicesPlans.Detail> details = new List<ServicesPlans.Detail>();

                foreach (Dictionary<string, object> item in list)
                {
                    ServicesPlans.Detail detail = new ServicesPlans.Detail();
                    detail.TypeId = item["TypeId"].ToString();
                    detail.Type = item["Type"].ToString();
                    detail.Plan = item["Technology"].ToString();
                    detail.TypeDefault = Convert.ToBoolean(item["RequiresTechnicalApproval"].ToString());
                    detail.Group = item["Network"].ToString();
                    detail.GroupId = item["MaximumBandwidth"].ToString();
                    detail.DisplayName = JsonConvert.SerializeObject(item);
                    details.Add(detail);
                }

                ListProduct.ItemsSource = new List<ServicesPlans.Detail>();
                ListProduct.ItemsSource = details;
                ShowHideVerticalScroll(Global.HasVerticalScroll);
                ListProduct.Visibility = Visibility.Visible;
            }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (ListProduct.SelectedItem == null) return;
            ServicesPlans.Detail item = (ServicesPlans.Detail)ListProduct.SelectedItem;
            NewServiceDialog.Instance.ComboServiceType.SelectedIndex = ((List<string>)NewServiceDialog.Instance.ComboServiceType.ItemsSource).IndexOf(item.Type);
            NewServiceDialog.Instance.DefinitionId = item.PlanId;
            Global.CloseDialog("DetailDialog");
        }

        private void ListProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WrapPanel panel = Global.FindChild<WrapPanel>(ListProduct, "WrapContent");
            int count = panel.Children.Count;
            int selectedIndex = ListProduct.SelectedIndex;

            for (int i = 0; i < count; i++)
            {
                MaterialDesignThemes.Wpf.PackIcon icon = Global.FindChild<MaterialDesignThemes.Wpf.PackIcon>(panel.Children[i], "SelectedIcon");
                icon.Visibility = i == selectedIndex ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private async void ButtonPlan_Click(object sender, RoutedEventArgs e)
        {
            string plans = ((Button)sender).Tag as string;

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("PlanDialog")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ServicePlanDialog(plans), "PlanDialog");

            /*string serviceTypeId = ((Button)sender).Tag as string;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                List<string> list = new List<string>();
                PlanDetailData = Global.servicesClient.GetServicePlansData(Constants.ServicePlansURL, Global.ContactCode, serviceTypeId, 0, 20, "");

                Application.Current.Dispatcher.Invoke(async delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (PlanDetailData == null || PlanDetailData.Plans == null) return;

                    StackPanel panel = new StackPanel();
                    foreach (ServicesPlans.Detail item in PlanDetailData.Plans) list.Add(item.Plan);

                    Label label = new Label();
                    label.Content = Properties.Resources.Plans;
                    label.FontSize = 20;
                    label.HorizontalAlignment = HorizontalAlignment.Center;
                    label.FontWeight = FontWeights.Bold;
                    label.Margin = new Thickness(10);

                    ListView listView = new ListView();
                    listView.ItemsSource = list;
                    listView.Margin = new Thickness(10);
                    listView.FontSize = 16;
                    listView.Width = 500;
                    listView.SelectionChanged += ListPlan_SelectionChanged;

                    Button button = new Button();
                    button.Margin = new Thickness(10);
                    button.VerticalAlignment = VerticalAlignment.Center;
                    button.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
                    button.Content = Properties.Resources.Close;
                    button.Tag = "PlanDialog";
                    button.Click += ButtonClose_Click;
                    button.Width = 100;

                    panel.Children.Add(label);
                    panel.Children.Add(listView);
                    panel.Children.Add(button); 

                    if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("PlanDialog")) Global.CloseDialog();
                    await MaterialDesignThemes.Wpf.DialogHost.Show(panel, "PlanDialog");

                });

            }, 10);*/

        }

        private void ListPlan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listPlan = sender as ListView; 
            NewServiceDialog.Instance.TextPlans.Text = listPlan.SelectedItem as string;
            List<ServicesPlans.Option> options = PlanDetailData.Plans[listPlan.SelectedIndex].Options;
            options.Sort((x, y) => x.Order.CompareTo(y.Order));

            int index = 0;
            int count = options.Count;
            List<string> list = new List<string>();

            for (int i = 0; i < count; i++)
            {
                list.Add(options[i].Name);
                if (options[i].Default) index = i;
            }

            NewServiceDialog.Instance.ComboOptions.ItemsSource = list;
            NewServiceDialog.Instance.ComboOptions.SelectedIndex = index;
            NewServiceDialog.Instance.ButtonDetail.IsEnabled = true;
            Global.CloseDialog("PlanDialog");
        }

    }
}
