using SelcommWPF.clients.models;
using SelcommWPF.clients.models.payment;
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

namespace SelcommWPF.dialogs.plans
{
    /// <summary>
    /// Interaction logic for PlanDetail.xaml
    /// </summary>
    public partial class PlanDetail : UserControl
    {
        private long ServiceReference, DefinitionId;
        private PlanModel.Detail PlanDetailData;

        public PlanDetail(long reference, long id)
        {
            ServiceReference = reference;
            DefinitionId = id;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LabelDialogTitle.Content = string.Format("{0} : {1}", Properties.Resources.Plan_Detail, DefinitionId);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                string url = ServiceReference == 0 ? Constants.PlanDefinitionURL : Constants.ServicesPlanDetailURL;
                PlanDetailData = Global.planClient.GetPlanDetailData(url, ServiceReference, DefinitionId);
                TransactionRate result = null;
                if (ServiceReference != 0) result = Global.planClient.GetPlanTransactionRates(Constants.ServicesRatesURL, ServiceReference, 0, 20, "Y");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (PlanDetailData == null)
                    {
                        LoadingPanel.Visibility = Visibility.Hidden;
                        Global.CloseDialog("DetailDialog");
                        return;
                    }

                    LabelId.Content = PlanDetailData.Id;
                    LabelName.Content = PlanDetailData.Name;
                    LabelDiaplayName.Content = PlanDetailData.DisplayName;
                    LabelGroupId.Content = PlanDetailData.GroupId;
                    LabelGroup.Content = PlanDetailData.Group;
                    LabelTypeId.Content = PlanDetailData.TypeId;
                    LabelType.Content = PlanDetailData.Type;
                    LabelTypeDefault.Content = PlanDetailData.TypeDefault;
                    LabelContractId.Content = PlanDetailData.ContractId;
                    LabelContract.Content = PlanDetailData.Contract;
                    LabelTransactionPlanId.Content = PlanDetailData.TransactionPlanId;
                    LabelTransactionPlan.Content = PlanDetailData.TransactionPlan;
                    LabelFrom.Content = StringUtils.ConvertDateTime(PlanDetailData.From);
                    LabelTo.Content = StringUtils.ConvertDateTime(PlanDetailData.To);
                    LabelBillingInterval.Content = PlanDetailData.BillingInterval;
                    LabelAvailability.Content = PlanDetailData.Availability;
                    LabelCreated.Content = StringUtils.ConvertDateTime(PlanDetailData.Created);
                    LabelCreatedBy.Content = PlanDetailData.CreatedBy;
                    LabelUpdated.Content = StringUtils.ConvertDateTime(PlanDetailData.LastUpdated);
                    LabelUpdatedBy.Content = PlanDetailData.UpdatedBy;

                    string serviceTypes = "";
                    foreach (UserModel.BusinessUnitModel item in PlanDetailData.ServiceTypes) serviceTypes += ", " + item.Name;
                    LabelServiceTypes.Content = serviceTypes == "" ? "" : serviceTypes.Substring(2);

                    List<string> list = new List<string>();
                    foreach (PlanModel.PlanOption item in PlanDetailData.Options)
                    {
                        list.Add(item.Name);
                        LoadOptionsData(item);
                    }

                    ComboCharges.ItemsSource = list;
                    ComboCharges.SelectedIndex = 0;
                    ComboDiscounts.ItemsSource = list;
                    ComboDiscounts.SelectedIndex = 0;
                    ComboAtturibes.ItemsSource = list;
                    ComboAtturibes.SelectedIndex = 0;
                    if (result != null) ListUsageRates.ItemsSource = result.Items;

                    ShowHideVerticalScroll(Global.HasVerticalScroll);
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);

        }

        private void LoadOptionsData(PlanModel.PlanOption item)
        {
            Grid grid1 = new Grid();
            grid1.Width = 420;
            ColumnDefinition column1 = new ColumnDefinition();
            column1.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column2 = new ColumnDefinition();
            column2.Width = new GridLength(1, GridUnitType.Star);
            grid1.ColumnDefinitions.Add(column1);
            grid1.ColumnDefinitions.Add(column2);

            Label label1 = new Label();
            label1.Content = Properties.Resources.Id;
            label1.FontSize = 16;
            Grid.SetColumn(label1, 0);
            grid1.Children.Add(label1);

            Label label2 = new Label();
            label2.Content = item.Id;
            label2.FontSize = 16;
            Grid.SetColumn(label2, 1);
            grid1.Children.Add(label2);

            Grid grid2 = new Grid();
            grid2.Width = 420;
            ColumnDefinition column3 = new ColumnDefinition();
            column3.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column4 = new ColumnDefinition();
            column4.Width = new GridLength(1, GridUnitType.Star);
            grid2.ColumnDefinitions.Add(column3);
            grid2.ColumnDefinitions.Add(column4);

            Label label3 = new Label();
            label3.Content = Properties.Resources.Name;
            label3.FontSize = 16;
            Grid.SetColumn(label3, 0);
            grid2.Children.Add(label3);

            Label label4 = new Label();
            label4.Content = item.Name;
            label4.FontSize = 16;
            Grid.SetColumn(label4, 1);
            grid2.Children.Add(label4);

            Grid grid3 = new Grid();
            grid3.Width = 420;
            ColumnDefinition column5 = new ColumnDefinition();
            column5.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column6 = new ColumnDefinition();
            column6.Width = new GridLength(1, GridUnitType.Star);
            grid3.ColumnDefinitions.Add(column5);
            grid3.ColumnDefinitions.Add(column6);

            Label label5 = new Label();
            label5.Content = Properties.Resources.Order;
            label5.FontSize = 16;
            Grid.SetColumn(label5, 0);
            grid3.Children.Add(label5);

            Label label6 = new Label();
            label6.Content = item.Id;
            label6.FontSize = 16;
            Grid.SetColumn(label6, 1);
            grid3.Children.Add(label6);

            Grid grid4 = new Grid();
            grid4.Width = 420;
            ColumnDefinition column7 = new ColumnDefinition();
            column7.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column8 = new ColumnDefinition();
            column8.Width = new GridLength(1, GridUnitType.Star);
            grid4.ColumnDefinitions.Add(column7);
            grid4.ColumnDefinitions.Add(column8);

            Label label7 = new Label();
            label7.Content = Properties.Resources.Contract_Action;
            label7.FontSize = 16;
            Grid.SetColumn(label7, 0);
            grid4.Children.Add(label7);

            Label label8 = new Label();
            label8.Content = item.ContractAction;
            label8.FontSize = 16;
            Grid.SetColumn(label8, 1);
            grid4.Children.Add(label8);

            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;
            panel.Children.Add(grid1);
            panel.Children.Add(grid2);
            panel.Children.Add(grid3);
            panel.Children.Add(grid4);

            MaterialDesignThemes.Wpf.Card card = new MaterialDesignThemes.Wpf.Card();
            card.Margin = new Thickness(10);
            card.Content = panel;
            PanelOptions.Children.Add(card);
        }

        private void ShowHideVerticalScroll(bool isChecked)
        {
            ListView[] listViews = new ListView[] { ListCharges, ListDiscounts, ListAtturibe, ListUsageRates};
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

            ScrollViewer[] scrolls = new ScrollViewer[] { ScrollViewGeneral, ScrollViewOptions};
            count = scrolls.Length;

            for (int i = 0; i < count; i++) if (scrolls[i] == null) return;
            for (int i = 0; i < count; i++) scrolls[i].VerticalScrollBarVisibility = isChecked ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
        }

        private void TextSearch_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CheckCharges_Checked(object sender, RoutedEventArgs e)
        {
            LoadChargeList();
        }

        private void ComboCharges_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadChargeList();
        }

        private void LoadChargeList()
        {
            if (ComboCharges.SelectedIndex == -1) return;

            List<AccountCharge.History> list = PlanDetailData.Options[ComboCharges.SelectedIndex].Charges;
            int count = list.Count;
            bool isCurrent = CheckCharges.IsChecked.Value;

            for (int i = 0; i < count; i++)
            {
                if (!list[i].To.Contains("9999-01-01") && isCurrent)
                {
                    list.RemoveAt(i);
                    count--;
                    i--;
                }
            }

            ListCharges.ItemsSource = new List<AccountCharge.History>();
            ListCharges.ItemsSource = list;
        }

        private void ComboDiscounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadDiscountList();
        }

        private void CheckDiscounts_Checked(object sender, RoutedEventArgs e)
        {
            LoadDiscountList();
        }

        private void LoadDiscountList()
        {
            if (ComboDiscounts.SelectedIndex == -1) return;

            List<PlanModel.PlanDiscount> list = PlanDetailData.Options[ComboDiscounts.SelectedIndex].Discounts;
            int count = list.Count;
            bool isCurrent = CheckDiscounts.IsChecked.Value;

            for (int i = 0; i < count; i++)
            {
                if (!list[i].To.Contains("9999-01-01") && isCurrent)
                {
                    list.RemoveAt(i);
                    count--;
                    i--;
                }
            }

            ListDiscounts.ItemsSource = new List<PlanModel.PlanDiscount>();
            ListDiscounts.ItemsSource = list;
        }

        private void ComboAtturibes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAtturibesList();
        }

        private void CheckAtturibes_Checked(object sender, RoutedEventArgs e)
        {
            LoadAtturibesList();
        }

        private void LoadAtturibesList()
        {
            if (ComboAtturibes.SelectedIndex == -1) return;

            List<PlanModel.AttrCharge> list = PlanDetailData.Options[ComboAtturibes.SelectedIndex].AttributeCharges;
            int count = list.Count;
            bool isCurrent = CheckAtturibes.IsChecked.Value;

            /*for (int i = 0; i < count; i++)
            {
                if (list[i].To != "on going" && isCurrent)
                {
                    list.RemoveAt(i);
                    count--;
                    i--;
                }
            }*/

            ListAtturibe.ItemsSource = new List<PlanModel.AttrCharge>();
            ListAtturibe.ItemsSource = PlanDetailData.Options[ComboAtturibes.SelectedIndex].AttributeCharges;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }
    }
}
