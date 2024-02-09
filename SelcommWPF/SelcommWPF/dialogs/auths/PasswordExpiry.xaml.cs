using Microsoft.Win32;
using RestSharp;
using SelcommWPF.clients.models.auths;
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

namespace SelcommWPF.dialogs.auths
{
    /// <summary>
    /// Interaction logic for PasswordExpiry.xaml
    /// </summary>
    public partial class PasswordExpiry : UserControl
    {

        private bool IsLoginPage = false;

        public PasswordExpiry(bool isLogin = false)
        {
            IsLoginPage = isLogin;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            bool isExpiry = Convert.ToBoolean(key.GetValue("PasswordExpiry", false));
            key.Close();
            CheckBoxDisplay.IsChecked = isExpiry;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                PasswordInfo result = Global.RequestAPI<PasswordInfo>(Constants.PasswordInformationURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("ContactCode", Global.ContactId), Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }
                    LabelDueDate.Content = StringUtils.ConvertDate(result.ExpiryDate);
                });

            }, 10);

        }

        private void CheckBoxDisplay_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = e.RoutedEvent.Name == "Checked";
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            key.SetValue("PasswordExpiry", isChecked);
            key.Close();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
            if (IsLoginPage) Global.loginWindow.CheckLoginHistory();
        }

        private async void ButtonChange_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ChangePassword(), "MyDialogHost");
        }
    }
}
