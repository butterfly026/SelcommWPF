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

namespace SelcommWPF.dialogs.report
{
    /// <summary>
    /// Interaction logic for ScheduleEnd.xaml
    /// </summary>
    public partial class ScheduleEnd : UserControl
    {
        private long RequestId;
        private string RequestName;

        public ScheduleEnd(long id, string name)
        {
            RequestId = id;
            RequestName = name;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LabelDialogTitle.Content = string.Format(Properties.Resources.End_Schedule_Title, RequestName);
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
            string dateTime = string.Format("{0}T{1}.000Z", CalendarEnd.SelectedDate.Value.ToString("yyyy-MM-dd"), ClockEnd.Time.ToString("HH:mm:ss"));
            Global.ReportDetailDialog.UpdateScheduleEnd(RequestId, dateTime);
            Global.CloseDialog("ArgumentDialog");
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("ArgumentDialog");
        }

    }
}
