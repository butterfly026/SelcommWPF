using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.costcenter;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace SelcommWPF.dialogs.accounts
{
    /// <summary>
    /// Interaction logic for ChargeHistoryDialog.xaml
    /// </summary>
    public partial class CostCenterListDialog : UserControl
    {

        private string ContactCode = "";

        public CostCenterListDialog(string contactCode = null)
        {
            InitializeComponent();
            InitializeControl();
            ContactCode = contactCode;
        }

        private void InitializeControl()
        {
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("Resource", "/Contacts/CostCenters");
                query.Add("api-version", 1.2);
                HttpStatusCode result = Global.RequestAPI(Constants.AuthrisationURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, null);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result != HttpStatusCode.OK)
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources1.Feature_Error);
                        Global.CloseDialog();
                    }

                    Global.CostCenterListDialog = this;
                    ShowCostCenterList(0, 20);
                });

            }, 10);
        }

        public void ShowCostCenterList(int skip, int take)
        {
            if (skip == 0) ListCostCenter.ItemsSource = new List<CostCenterModel.CostCenterListItem>();
            
            LoadingPanel.Visibility = Visibility.Visible;
            string url = Constants.CostCentersGetURL;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.2);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("CountRecords", "Y");
                List<CostCenterModel> response = Global.RequestAPI<List<CostCenterModel>>(url, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("contactCode", ContactCode), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (response == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    CostCenterModel result = response[0];

                    List<CostCenterModel.CostCenterListItem> list = (List<CostCenterModel.CostCenterListItem>)ListCostCenter.ItemsSource;
                    ListCostCenter.ItemsSource = new List<CostCenterModel.CostCenterListItem>();

                    if (list == null) list = new List<CostCenterModel.CostCenterListItem>();
                    int count = result == null ? 0 : result.Items.Count;
                    ListCostCenter.Tag = result == null ? 0 : result.Count;

                    for (int i = 0; i < count; i++)
                    {
                        CostCenterModel.CostCenterListItem item = new CostCenterModel.CostCenterListItem();
                        item.Id = result.Items[i].Id;
                        item.Created = StringUtils.ConvertDateTime(result.Items[i].Created);
                        item.Status = result.Items[i].Status;
                        item.Name = result.Items[i].Name;
                        item.CreatedBy = result.Items[i].CreatedBy;

                        list.Add(item);
                    }

                    ListCostCenter.ItemsSource = list;
                });

            }, 10);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void ButtonCostCenterDelete_Click(object sender, RoutedEventArgs e)
        {
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("Resource", "/Contacts/CostCenters/Delete");
                query.Add("api-version", 1.2);
                HttpStatusCode result = Global.RequestAPI(Constants.AuthrisationURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, null);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result != HttpStatusCode.OK)
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources1.Feature_Error);
                        Global.CloseDialog();
                    }

                    string url = Constants.CostCenterDeleteURL;
                    string id = ((Button)sender).Tag.ToString();

                    LoadingPanel.Visibility = Visibility.Visible;

                    EasyTimer.SetTimeout(() =>
                    {
                        Dictionary<string, object> path = Global.BuildDictionary("Id", id);
                        HttpStatusCode response = Global.RequestAPI(url, Method.Delete, Global.GetHeader(Global.CustomerToken), path,
                            Global.BuildDictionary("api-version", 1.2), null);

                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            LoadingPanel.Visibility = Visibility.Hidden;
                            if (response == HttpStatusCode.OK)
                            {
                                ShowCostCenterList(0, 20);
                            }
                            else
                            {
                                MessageUtils.ShowErrorMessage("", "CostCenter delete failed.");
                            }
                        });

                    }, 10);
                });

            }, 10);
        }
        private async void ButtonCostCenterEdit_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private async void ButtonCostCenterNew_Click(object sender, RoutedEventArgs e)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(new NewCostCenterDialog(ContactCode), "DetailDialog");
        }

        private void ListCostCenter_Scroll(object sender, ScrollChangedEventArgs e)
        {
            ListView listView = (ListView)e.Source;
            int totalCount = listView.Items.Count;
            int limit = Convert.ToInt32(listView.Tag);
            if (totalCount == 0 || totalCount >= limit) return;
            if (e.VerticalOffset + e.VerticalChange >= totalCount - 9)
            {
                ShowCostCenterList(totalCount, 20);
            }
        }
    }
}
