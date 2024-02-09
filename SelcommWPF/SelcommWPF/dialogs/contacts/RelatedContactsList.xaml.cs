using SelcommWPF.clients.models.contacts;
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

namespace SelcommWPF.dialogs.contacts
{
    /// <summary>
    /// Interaction logic for RelatedContacts.xaml
    /// </summary>
    public partial class RelatedContactsList : UserControl
    {
        
        public RelatedContactsList()
        {
            InitializeComponent();
            ShowRelatedContactList();
        }

        public void ShowRelatedContactList()
        {
            if (Global.ContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            Global.RelatedContactDialg = this;
            LoadingPanel.Visibility = Visibility.Visible;
            ListRelatedContact.ItemsSource = new List<RelatedContactModel>();

            EasyTimer.SetTimeout(() =>
            {
                List<RelatedContactModel> result = Global.contactClient.GetRelatedContactList(Constants.RelatedContactListURL, Global.ContactCode);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    List<RelatedContactModel> list = new List<RelatedContactModel>();

                    int count = result == null ? 0 : result.Count;
                    for (int i = 0; i < count; i++)
                    {
                        string relationship = "";
                        foreach (RelatedContactModel.Relationship item in result[i].Relationships) relationship += ", " + item.Name;
                        result[i].RelationshipText = relationship == "" ? "" : relationship.Substring(2);
                        list.Add(result[i]);
                    }

                    ListRelatedContact.ItemsSource = list;
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private async void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) MaterialDesignThemes.Wpf.DialogHost.Close("DetailDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(new RelatedContactDialog(), "DetailDialog");
        }

        private async void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (ListRelatedContact == null || ListRelatedContact.SelectedItem == null) return;

            RelatedContactModel item = (RelatedContactModel)ListRelatedContact.SelectedItem;
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) MaterialDesignThemes.Wpf.DialogHost.Close("DetailDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(new RelatedContactDialog(item.RelatedContactCode), "DetailDialog");
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ListRelatedContact == null || ListRelatedContact.SelectedItem == null) return;

            RelatedContactModel item = (RelatedContactModel)ListRelatedContact.SelectedItem;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Global.contactClient.DeleteUserDefinedData(Constants.RelatedContactDetailURL, Global.ContactCode, item.RelatedContactCode);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    ShowRelatedContactList();
                });
            }, 10);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void ListRelatedContact_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListRelatedContact == null || ListRelatedContact.SelectedItem == null) return;
            bool isEnabled = ListRelatedContact.SelectedItem != null;
            ButtonUpdate.IsEnabled = isEnabled;
            ButtonDelete.IsEnabled = isEnabled;
        }
    }
}
