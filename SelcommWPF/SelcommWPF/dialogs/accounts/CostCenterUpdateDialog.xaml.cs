using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using Newtonsoft.Json;
using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.costcenter;
using SelcommWPF.clients.models.services;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.PeerToPeer.Collaboration;
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

namespace SelcommWPF.dialogs.accounts
{
    /// <summary>
    /// Interaction logic for EventDialog.xaml
    /// </summary>
    public partial class CostCenterUpdateDialog : UserControl
    {
        private string CCId;
        private CostCenterModel.CostCenter CostCenter;

        public CostCenterUpdateDialog(string ccId)
        {
            CCId = ccId;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }

        private void CheckCreatable()
        {
            ButtonUpdate.IsEnabled = !TextName.Text.IsNullOrEmpty();
        }

        private void TextName_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckCreatable();
        }
    }
}
