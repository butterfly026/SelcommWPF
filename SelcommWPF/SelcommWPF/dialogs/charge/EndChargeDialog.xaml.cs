using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SelcommWPF.dialogs
{
    /// <summary>
    /// Interaction logic for EndChargeDialog.xaml
    /// </summary>
    public partial class EndChargeDialog : UserControl
    {
        private long ProfileId;
        private bool IsServices;
        private string ServiceReference;

        public EndChargeDialog(long id, string code, bool isServices = true, string reference = "")
        {
            ProfileId = id;
            IsServices = isServices;
            ServiceReference = reference;
            InitializeComponent();
            LabelDialogTitle.Content = isServices ? string.Format(Properties.Resources.End_Charge_Service, code) : string.Format(Properties.Resources.End_Charge_Contact, code);
        }

        private void InitializeControl()
        {
            CalendarEnd.SelectedDate = DateTime.Now;
            ClockEnd.Time = DateTime.Now;
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            if (CalendarEnd == null || CalendarEnd.SelectedDate == null)
            {
                MessageUtils.ShowErrorMessage("", Properties.Resources.Choose_Date);
                return;
            }
            string dateTime = string.Format("{0}T{1}", CalendarEnd.SelectedDate.Value.ToString("yyyy-MM-dd"), ClockEnd.Time.ToString("HH:mm"));
            Global.ChargeDialog.UpdateChargeEnd(ProfileId, dateTime);
            Global.CloseDialog("DetailDialog");
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }
       
    }
}
