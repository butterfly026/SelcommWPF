using Newtonsoft.Json;
using SelcommWPF.clients.models;
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
    /// Interaction logic for ServicePlanDialog.xaml
    /// </summary>
    public partial class ServicePlanDialog : UserControl
    {
        private Dictionary<string, object> ServiceData;
        private List<Dictionary<string, object>> PlanList;
        private string SelectedPlan = "";

        public ServicePlanDialog(string item)
        {
            ServiceData = JsonConvert.DeserializeObject<Dictionary<string, object>>(item);
            PlanList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(ServiceData["Plans"].ToString());
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            List<ServicesPlans.Detail> details = new List<ServicesPlans.Detail>();
            foreach (Dictionary<string, object> item in PlanList)
            {
                ServicesPlans.Detail detail = new ServicesPlans.Detail();
                detail.Plan = item["Plan"].ToString();
                detail.DisplayName = item["TechnicalDetails"] == null ? "" : item["TechnicalDetails"].ToString();
                detail.From = detail.DisplayName == "" ? "Collapsed" : "Visible";
                detail.Type = "$" + item["InstallationFee"].ToString();
                detail.GroupId = "$" + item["OngoingFee"].ToString();
                detail.Group = "Requires Technical Approval : " + item["RequiresTechnicalApproval"].ToString();
                detail.TypeId = item["CommercialDetails"].ToString();
                details.Add(detail);    
            }

            ListPlan.ItemsSource = details;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("PlanDialog");
        }

        private void ButtonSelect_Click(object sender, RoutedEventArgs e)
        {
            WrapPanel panel = Global.FindChild<WrapPanel>(ListPlan, "WrapContent");
            int count = panel.Children.Count;

            Button button = sender as Button;
            StackPanel panel1 = button.Parent as StackPanel;
            Grid grid = panel1.Children[0] as Grid;
            MaterialDesignThemes.Wpf.PackIcon icon1 = grid.Children[1] as MaterialDesignThemes.Wpf.PackIcon;
            SelectedPlan = button.Tag as string;

            for (int i = 0; i < count; i++)
            {
                MaterialDesignThemes.Wpf.PackIcon icon = Global.FindChild<MaterialDesignThemes.Wpf.PackIcon>(panel.Children[i], "SelectedIcon");
                icon.Visibility = icon == icon1 ? Visibility.Visible : Visibility.Collapsed;
            }

        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("PlanDialog");
        }
    }
}
