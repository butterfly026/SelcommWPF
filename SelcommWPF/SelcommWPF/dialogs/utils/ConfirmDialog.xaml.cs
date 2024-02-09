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

namespace SelcommWPF.dialogs
{
    /// <summary>
    /// Interaction logic for ConfirmDialog.xaml
    /// </summary>
    public partial class ConfirmDialog : UserControl
    {

        public ConfirmDialog(string message, int type = 0)
        {
            InitializeComponent();
            DialogMessage.Text = message;
            if (type == 1) ButtonCancel.Visibility = Visibility.Collapsed;
        }

    }
}
