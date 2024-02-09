using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.services;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.PeerToPeer.Collaboration;
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

namespace SelcommWPF.dialogs.services
{
    /// <summary>
    /// Interaction logic for EventDialog.xaml
    /// </summary>
    public partial class EventDialog : UserControl
    {
        private string ServiceReference;
        private string ServiceId;

        private bool IsSelectedDefinition = false;
        private IDisposable RequestTimer = null;

        private List<DefinitionModel> EventDefinitions;
        private List<EventReason> EventReasons;
        private DefinitionModel SelectedDefinition;

        public EventDialog(string serviceReference, string serviceId)
        {
            ServiceReference = serviceReference;
            ServiceId = serviceId;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            //ComboStatus.ItemsSource = new string[] { "Schedulable", "Not Schedulable" };
            LabelDialogTitle.Content = "New Event  " + ServiceId;
            DatePickerEvent.SelectedDate = DateTime.Today;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("Resource", "/Services/Events/New");
                query.Add("api-version", 1.2);
                HttpStatusCode result = Global.RequestAPI(Constants.AuthrisationURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, null);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK) GetEventDefinitions();
                    else
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources1.Feature_Error);
                        Global.CloseDialog();
                    }
                });

            }, 10);
        }

        private void GetEventDefinitions()
        {

        }

        private void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("DefinitionId", SelectedDefinition.Id);
            body.Add("Note", TextTerminateNote.Text);
            body.Add("ReasonId", EventReasons.Find(evt => evt.Reason.Reason == ComboReason.Text).Reason.Id);
            body.Add("Due", DatePickerEvent.SelectedDate);
            LoadingPanel.Visibility = Visibility;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.EventCreateURL, Method.Post, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ServiceReference", ServiceReference), Global.BuildDictionary("api-version", 1.0), body, false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;

                    if (result == HttpStatusCode.Created)
                    {
                        MessageUtils.ShowMessage("", Properties.Resources1.Event_Created_Successfully);
                        Global.CloseDialog();
                    }
                    else
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources1.Create_Error);
                        Global.CloseDialog();
                    }
                });
            }, 10);
            
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void TextDefinition_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = TextDefinition.Text;
            if (search == "" || RequestTimer != null)
            {
                ListDefinitions.ItemsSource = new List<string>();
                GridDefinitions.Visibility = Visibility.Collapsed;
                ComboReason.Visibility = Visibility.Collapsed;
                DatePickerEvent.Visibility = Visibility.Collapsed;
                ComboScheduleTo.Visibility = Visibility.Collapsed;
                ComboTeamScheduleTo.Visibility = Visibility.Collapsed;
                TextTerminateNote.Visibility = Visibility.Collapsed;
                CheckCreatable();
            }
        }

        private void TextDefinition_KeyUp(object sender, KeyEventArgs e)
        {
            string search = TextDefinition.Text;

            if (e.Key == Key.Down)
            {
                ListDefinitions.Focus();
                ListDefinitions.SelectedIndex = 0;
                return;
            }

            if (RequestTimer != null) { RequestTimer.Dispose(); RequestTimer = null; }

            RequestTimer = EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    search = TextDefinition.Text;
                    ListDefinitions.ItemsSource = new List<string>();
                    ProgressDetail.Visibility = Visibility.Visible;
                    GridDefinitions.Visibility = Visibility.Collapsed;
                });

                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("SearchString", search);
                query.Add("SkipRecords", 0);
                query.Add("TakeRecords", 20);
                query.Add("CountRecords", "Y");

                Dictionary<string, object> result = Global.RequestAPI<Dictionary<string, object>>(Constants.EventDefinitionURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary ("ServiceReference", ServiceReference), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result != null && result.Count != 0)
                    {
                        EventDefinitions = JsonConvert.DeserializeObject<List<DefinitionModel>>(result["Items"].ToString());
                        List<string> items = new List<string>();
                        foreach (DefinitionModel item in EventDefinitions) items.Add(item.Name);
                        ListDefinitions.ItemsSource = items;
                        GridDefinitions.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ListDefinitions.ItemsSource = new List<string>();
                        GridDefinitions.Visibility = Visibility.Hidden;
                    }

                    RequestTimer.Dispose();
                    RequestTimer = null;
                    ProgressDetail.Visibility = Visibility.Collapsed;
                });

            }, 2000);
        }

        private void CheckCreatable()
        {
            Boolean enable = true;
            if (TextDefinition.Text.IsNullOrEmpty()) enable = false;;
            if (ComboTeamScheduleTo.SelectedIndex == -1) enable = false;
            ButtonCreate.IsEnabled = enable;
        }

        private void TextDefinition_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back) IsSelectedDefinition = false;
        }

        private void TextDefinition_GotFocus(object sender, RoutedEventArgs e)
        {
            GridDefinitions.Visibility = ListDefinitions != null && ListDefinitions.Items.Count > 0 ? Visibility.Visible : Visibility.Hidden;
        }

        private void ListDefinitions_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SelectDefinition();
        }

        private void ListDefinitions_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectDefinition();
        }

        private void SelectDefinition()
        {
            if (ListDefinitions.SelectedIndex == -1) return;
            SelectedDefinition = EventDefinitions[ListDefinitions.SelectedIndex];
            IsSelectedDefinition = true;

            CheckCreatable();

            LoadingPanel.Visibility = Visibility;
            EasyTimer.SetTimeout(() =>
            {
                SelectedDefinition = Global.RequestAPI<DefinitionModel>(Constants.EventDefDetailURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("Id", SelectedDefinition.Id), Global.BuildDictionary("api-version", 1.0), "");
                EventReasons = Global.RequestAPI<List<EventReason>>(Constants.EventReasonURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("Id", SelectedDefinition.Id), Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (SelectedDefinition == null) return;
                    ListDefinitions.ItemsSource = new List<string>();
                    GridDefinitions.Visibility = Visibility.Hidden;
                    TextDefinition.Text = SelectedDefinition.Name;
                
                    List<string> TeamsSource = new List<string>();
                    foreach (var source in SelectedDefinition.Teams) TeamsSource.Add(source.Name);
                    ComboTeamScheduleTo.ItemsSource = TeamsSource;

                    List<string> ReasonsSource = new List<string>();
                    foreach (var source in EventReasons) ReasonsSource.Add(source.Reason.Reason);
                    ComboReason.ItemsSource = ReasonsSource;

                    if (SelectedDefinition.Schedulable != "Yes") {
                        ComboScheduleTo.Visibility = Visibility.Collapsed;
                        ComboTeamScheduleTo.Visibility = Visibility.Collapsed;
                    } 
                    else {
                        ComboScheduleTo.Visibility = Visibility.Visible;
                        ComboTeamScheduleTo.Visibility = Visibility.Visible;
                    }
                    ComboReason.Visibility = Visibility.Visible;
                    DatePickerEvent.Visibility = Visibility.Visible;
                    //ComboStatus.Visibility = Visibility.Visible;
                    TextTerminateNote.Visibility = Visibility.Visible;

                    if (!SelectedDefinition.ReSchedulable)
                    {
                        DatePickerEvent.SelectedDate = DateTime.Now;
                        DatePickerEvent.IsEnabled = false;
                    }

                    if (TeamsSource.Count == 0)
                        ComboTeamScheduleTo.Visibility = Visibility.Collapsed;

                });

            }, 10);
        }

        private void ComboTeamScheduleTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckCreatable();
        }
    }
}
