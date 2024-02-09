using Newtonsoft.Json;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.messages;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// Interaction logic for TaskDialog.xaml
    /// </summary>
    public partial class TaskDialog : UserControl
    {
        private TaskModel.Item SelectedTask;
        private List<Dictionary<string, object>> TaskTypeList;
        private List<Dictionary<string, object>> RequestorList;
        private List<Dictionary<string, object>> StatusList;
        private List<Dictionary<string, object>> PriorityList;
        private List<Dictionary<string, object>> EmailAddressList;
        private List<Dictionary<string, object>> ResolutionList;

        private string TaskNumber;
        private bool IsService = false;
        private string Code = Global.ContactCode;
        private List<TaskModel.Item> comments, timeLogs, resourceList, dependences;

        public TaskDialog(string data, bool isService, string code)
        {
            SelectedTask = data == "" ? null : JsonConvert.DeserializeObject<TaskModel.Item>(data);
            IsService = isService;
            Code = code;
            InitializeComponent();
            IntializeControl();
        }

        private void IntializeControl()
        {
            LabelDialogTitle.Content = Properties.Resources.Task;
            LoadingPanel.Visibility = Visibility.Visible;
            DatePickerCreated.SelectedDate = DateTime.Now;

            EasyTimer.SetTimeout(() =>
            {
                TaskTypeList = Global.contactClient.GetTaskParameters(Constants.TaskTypeListURL);
                RequestorList = Global.contactClient.GetTaskParameters(Constants.RequestorListURL, Global.ContactCode);
                StatusList = Global.contactClient.GetTaskParameters(Constants.TaskStatusListURL);
                PriorityList = Global.contactClient.GetTaskParameters(Constants.PriorityListURL);
                EmailAddressList = Global.contactClient.GetTaskParameters(Constants.EmailListURL, Global.ContactCode);
                ResolutionList = Global.contactClient.GetTaskParameters(Constants.ResolutionURL);

                if (SelectedTask != null)
                {
                    comments = Global.contactClient.GetTaskSubListData(Constants.CommentsURL, SelectedTask.Id);
                    timeLogs = Global.contactClient.GetTaskSubListData(Constants.TimeLogsURL, SelectedTask.Id);
                    resourceList = Global.contactClient.GetTaskSubListData(Constants.ResourcesURL, SelectedTask.Id);
                    dependences = Global.contactClient.GetTaskSubListData(Constants.DependenciesURL, SelectedTask.Id);
                    TaskNumber = SelectedTask.Number;
                }
                else
                {
                    Dictionary<string, string> result = Global.contactClient.GetTaskNextNumber(Constants.TaskNumberURL);
                    TaskNumber = "TASK" + new Random().Next(1000, 9999); //result["Number"];
                }

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (TaskTypeList == null || RequestorList == null || StatusList == null || PriorityList == null || EmailAddressList == null ||
                        ResolutionList == null || comments == null || timeLogs == null || resourceList == null || dependences == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    List<string> list1 = new List<string>();
                    foreach (Dictionary<string, object> item in TaskTypeList) list1.Add(item["Name"].ToString());
                    ComboTaskType.ItemsSource = list1;

                    List<string> list2 = new List<string>();
                    foreach (Dictionary<string, object> item in RequestorList) list2.Add(item["Name"].ToString());
                    ComboRequester.ItemsSource = list2;

                    List<string> list3 = new List<string>();
                    foreach (Dictionary<string, object> item in StatusList) list3.Add(item["Name"].ToString());
                    ComboTaskStatus.ItemsSource = list3;

                    List<string> list4 = new List<string>();
                    foreach (Dictionary<string, object> item in PriorityList) list4.Add(item["Name"].ToString());
                    ComboPriority.ItemsSource = list4;

                    List<string> list5 = new List<string>();
                    foreach (Dictionary<string, object> item in EmailAddressList) list5.Add(item["Email"].ToString());
                    ComboEmailAddress.ItemsSource = list5;

                    List<string> list6 = new List<string>();
                    foreach (Dictionary<string, object> item in ResolutionList) list6.Add(item["Name"].ToString());
                    ComboResolution.ItemsSource = list5;

                    if (SelectedTask != null)
                    {
                        ComboTaskType.SelectedIndex = list1.IndexOf(SelectedTask.Type);
                        ComboRequester.SelectedIndex = list2.IndexOf(SelectedTask.RequestedBy);
                        ComboTaskStatus.SelectedIndex = list3.IndexOf(SelectedTask.Status);
                        ComboPriority.SelectedIndex = list4.IndexOf(SelectedTask.Priority);
                        ComboEmailAddress.SelectedIndex = list5.IndexOf(SelectedTask.Emails[0].Address);
                        ComboResolution.SelectedIndex = list6.IndexOf(SelectedTask.Resolution);
                        TextCustomerRef.Text = SelectedTask.Reference;
                        if (!string.IsNullOrEmpty(SelectedTask.Created)) DatePickerCreated.SelectedDate = DateTime.Parse(SelectedTask.Created);
                        TextShortDescription.Text = SelectedTask.ShortDescription;

                        DatePickerRequired.IsEnabled = !string.IsNullOrEmpty(SelectedTask.RequiredDate);
                        if (!string.IsNullOrEmpty(SelectedTask.RequiredDate)) DatePickerRequired.SelectedDate = DateTime.Parse(SelectedTask.RequiredDate);
                        DatePickerFollowup.IsEnabled = !string.IsNullOrEmpty(SelectedTask.NextFollowupDate);
                        if (!string.IsNullOrEmpty(SelectedTask.NextFollowupDate)) DatePickerRequired.SelectedDate = DateTime.Parse(SelectedTask.NextFollowupDate);
                        DatePickerCompleted.IsEnabled = !string.IsNullOrEmpty(SelectedTask.CompletedDate);
                        if (!string.IsNullOrEmpty(SelectedTask.CompletedDate)) DatePickerCompleted.SelectedDate = DateTime.Parse(SelectedTask.CompletedDate);
                        DatePickerEST.IsEnabled = !string.IsNullOrEmpty(SelectedTask.EstimatedDate);
                        if (!string.IsNullOrEmpty(SelectedTask.EstimatedDate)) DatePickerEST.SelectedDate = DateTime.Parse(SelectedTask.EstimatedDate);
                        DatePickerSLA.IsEnabled = !string.IsNullOrEmpty(SelectedTask.SLAData);
                        if (!string.IsNullOrEmpty(SelectedTask.SLAData)) DatePickerSLA.SelectedDate = DateTime.Parse(SelectedTask.SLAData);

                        TextFixedHours.Text = SelectedTask.QuotedHours + "";
                        TextFixedAmount.Text = SelectedTask.QuotedPrice + "";
                        RadioFixedAmount.IsChecked = SelectedTask.QuoteFixedPrice;
                        TextInvoiceNumber.Text = SelectedTask.InvoiceNumber;
                        CheckFAQ.IsChecked = SelectedTask.FAQ;
                        TextFAQ.Text = SelectedTask.FAQTag;
                        TextTaskDetails.Text = SelectedTask.Description;
                        TextResolutionShortDesc.Text = SelectedTask.ShortResolution;
                        TextResolutionDetail.Text = SelectedTask.ResolutionDetail;

                        ListComments.ItemsSource = comments;
                        ListTimeLog.ItemsSource = timeLogs;
                        ListResources.ItemsSource = resourceList;
                        ListDependences.ItemsSource = dependences;
                        ShowMessageList(0, 10);
                    }

                    Visibility visible = SelectedTask == null ? Visibility.Collapsed : Visibility.Visible;
                    TabComments.Visibility = visible;
                    TabMessages.Visibility = visible;
                    TabTimeLogs.Visibility = visible;
                    TabResources.Visibility = visible;
                    TabDependencies.Visibility = visible;

                    LabelDialogTitle.Content = string.Format(Properties.Resources.Task + " [" + TaskNumber + "]");
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);

        }

        public void ShowMessageList(int skip, int take)
        {
            if (skip == 0) ListMessages.ItemsSource = new List<MessageModel.Item>();
            if (Global.ContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                MessageModel result = Global.contactClient.GetMessageList(Constants.TaskMessagesURL, SelectedTask.Id, skip, take);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<MessageModel.Item> list = (List<MessageModel.Item>)ListMessages.ItemsSource;
                    ListMessages.ItemsSource = new List<MessageModel.Item>();

                    if (list == null) list = new List<MessageModel.Item>();
                    int count = result == null ? 0 : result.Messages.Count;
                    ListMessages.Tag = result.Count;

                    for (int i = 0; i < count; i++)
                    {
                        result.Messages[i].VisiblityNew = result.Messages[i].Status == "NEW" ? "Visible" : "Collapsed";
                        result.Messages[i].VisiblityOther = result.Messages[i].Status != "NEW" ? "Visible" : "Collapsed";
                        result.Messages[i].AddressText = result.Messages[i].Addresses == null || result.Messages[i].Addresses.Count == 0 ? "" : result.Messages[i].Addresses[0].Address;
                        list.Add(result.Messages[i]);
                    }

                    ListMessages.ItemsSource = list;
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private void ListMessages_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ListView listView = (ListView)e.Source;
            int totalCount = listView.Items.Count;
            int limit = Convert.ToInt32(listView.Tag);
            if (totalCount == 0 || totalCount >= limit) return;
            if (e.VerticalOffset + e.VerticalChange >= totalCount - 10)
            {
                ShowMessageList(totalCount, 10);
            }
        }

        private void CheckFAQ_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = e.RoutedEvent.Name == "Checked";
            TextFAQ.IsEnabled = isChecked;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (TextShortDescription.Text == "")
            {
                MessageUtils.ShowErrorMessage("", Properties.Resources.Short_Desc_Require);
                TextShortDescription.Focus();
                return;
            }

            Dictionary<string, object> email = null;
            Dictionary<string, object> item = ComboEmailAddress.SelectedIndex == -1 ? null : EmailAddressList[ComboEmailAddress.SelectedIndex];
            if (item != null)
            {
                email = new Dictionary<string, object>();
                email.Add("Id", null);// Convert.ToInt32(item["Id"].ToString()));
                email.Add("Address", item["Email"].ToString());
            }
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>() { email };

            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("Id", SelectedTask == null ? 0 : SelectedTask.Id);
            body.Add("GroupId", 0);
            //body.Add("TypeId", ComboTaskType.SelectedIndex == -1 ? 0 : Convert.ToInt64(TaskTypeList[ComboTaskType.SelectedIndex]["Id"].ToString()));
            body.Add("TypeId", 6);
            body.Add("Number", TaskNumber);
            body.Add("Reference", TextCustomerRef.Text);
            body.Add("RequestedBy", ComboRequester.SelectedItem == null ? "" : ComboRequester.SelectedItem.ToString());
            body.Add("Emails", list);
            //body.Add("StatusId", ComboTaskStatus.SelectedIndex == -1 ? 0 : Convert.ToInt64(StatusList[ComboTaskStatus.SelectedIndex]["Id"].ToString()));
            body.Add("StatusId", 1);
            body.Add("ResolutionId", ComboResolution.SelectedIndex == -1 ? null : ResolutionList[ComboResolution.SelectedIndex]["Id"].ToString());
            //body.Add("PriorityId", ComboPriority.SelectedIndex == -1 ? 0 : Convert.ToInt64(PriorityList[ComboPriority.SelectedIndex]["Id"].ToString()));
            body.Add("PriorityId", 3);
            body.Add("ShortDescription", TextShortDescription.Text);
            body.Add("Description", TextTaskDetails.Text);
            body.Add("ShortResolution", TextResolutionShortDesc.Text);
            body.Add("ResolutionDetail", TextResolutionDetail.Text);
            body.Add("RequiredDate", DatePickerRequired.SelectedDate == null || !DatePickerRequired.IsEnabled ? null : DatePickerRequired.SelectedDate.Value.ToString("yyyy-MM-dd"));
            body.Add("CompletedDate", DatePickerCompleted.SelectedDate == null || !DatePickerCompleted.IsEnabled ? null : DatePickerCompleted.SelectedDate.Value.ToString("yyyy-MM-dd"));
            body.Add("EstimatedDate", DatePickerEST.SelectedDate == null || !DatePickerEST.IsEnabled ? null : DatePickerCompleted.SelectedDate.Value.ToString("yyyy-MM-dd"));
            body.Add("SLAData", DatePickerSLA.SelectedDate == null || !DatePickerSLA.IsEnabled ? null : DatePickerSLA.SelectedDate.Value.ToString("yyyy-MM-dd"));
            body.Add("NextFollowupDate", DatePickerFollowup.SelectedDate == null || !DatePickerFollowup.IsEnabled ? null : DatePickerFollowup.SelectedDate.Value.ToString("yyyy-MM-dd"));
            body.Add("VisibleToCustomer", CheckDisplayUser.IsChecked.Value);
            body.Add("Ticket", true);
            body.Add("FAQ", TextFAQ.IsEnabled);
            body.Add("FAQTag", TextFAQ.Text);
            body.Add("QuotedHours", TextFixedHours.Text == "" ? 0 : Convert.ToInt32(TextFixedHours.Text));
            body.Add("QuotedPrice", TextFixedAmount.Text == "" ? 0 : Convert.ToDouble(TextFixedAmount.Text));
            body.Add("QuoteFixedPrice", RadioFixedAmount.IsChecked.Value);
            body.Add("InvoiceNumber", TextInvoiceNumber.Text);
            body.Add("PercentComplete", 0);
            
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> result = null;
                if (IsService)
                {
                    if (SelectedTask == null) result = Global.contactClient.CreateNewTask(Constants.ServiceTaskListURL, Code, body, true);
                    else Global.contactClient.UpdateTaskData(Constants.TaskUpdateURL, SelectedTask.Id, body);
                }
                else
                {
                    if (SelectedTask == null) result = Global.contactClient.CreateNewTask(Constants.TaskListURL, Code, body);
                    else Global.contactClient.UpdateTaskData(Constants.TaskUpdateURL, SelectedTask.Id, body);
                }

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null) return;

                    HttpStatusCode code = (HttpStatusCode)result["StatusCode"];
                    if (code == HttpStatusCode.OK || code == HttpStatusCode.Created)
                    {
                        Dictionary<string, long> response = (Dictionary<string, long>)result["Response"];
                        SelectedTask = new TaskModel.Item();
                        SelectedTask.Id = response["Id"];
                        Visibility visible = SelectedTask == null ? Visibility.Collapsed : Visibility.Visible;
                        TabComments.Visibility = visible;
                        TabMessages.Visibility = visible;
                        TabTimeLogs.Visibility = visible;
                        TabResources.Visibility = visible;
                        TabDependencies.Visibility = visible;
                    }
                });

            }, 10);

        }

        private void ComboTaskType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonSave.IsEnabled = CheckEnableSaveButton();
        }

        private void ComboEmailAddress_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonSave.IsEnabled = CheckEnableSaveButton();
        }

        private void TextShortDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            ButtonSave.IsEnabled = CheckEnableSaveButton();
        }

        private bool CheckEnableSaveButton()
        {
            if (ComboTaskType == null || ComboEmailAddress == null || TextShortDescription == null) return false;
            return ComboTaskType.SelectedIndex != -1 && ComboEmailAddress.SelectedIndex != -1 && TextShortDescription.Text != "";
        }

        private async void ButtonSMS_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new SMSDialog(), "DetailDialog");
        }

        private async void ButtonEmail_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new EmailDialog(), "DetailDialog");
        }

        private async void ButtonAddComment_Click(object sender, RoutedEventArgs e)
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;
            panel.Margin = new Thickness(10);
            panel.Width = 500;

            Label label = new Label();
            label.Content = Properties.Resources.Comment;
            label.FontSize = 16;
            label.Margin = new Thickness(10);
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.FontWeight = FontWeights.Bold;
            panel.Children.Add(label);

            TextBox textBox = new TextBox();
            textBox.Name = "TextComment";
            textBox.Height = 100;
            textBox.Padding = new Thickness(5);
            textBox.Style = (Style)Application.Current.Resources["MaterialDesignOutlinedTextBox"];
            textBox.AcceptsReturn = true;
            textBox.TextWrapping = TextWrapping.Wrap;
            textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            textBox.FontSize = 16;
            MaterialDesignThemes.Wpf.HintAssist.SetHint(textBox, Properties.Resources.Comment);
            textBox.Margin = new Thickness(10);
            panel.Children.Add(textBox);

            CheckBox checkBox = new CheckBox();
            checkBox.Content = Properties.Resources.Internal;
            checkBox.VerticalContentAlignment = VerticalAlignment.Bottom;
            checkBox.FontSize = 16;
            checkBox.Margin = new Thickness(10, 0, 0, 0);
            panel.Children.Add(checkBox);

            Button button = new Button();
            button.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            button.Content = Properties.Resources.Save;
            button.Margin = new Thickness(10);
            button.Tag = 0;
            button.Click += ButtonSaveComment_Click;

            Button button1 = new Button();
            button1.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            button1.Content = Properties.Resources.Close;
            button1.Margin = new Thickness(10);
            button1.Click += ButtonCloseComment_Click;

            StackPanel panel2 = new StackPanel();
            panel2.Orientation = Orientation.Horizontal;
            panel2.HorizontalAlignment = HorizontalAlignment.Right;
            panel2.Children.Add(button);
            panel2.Children.Add(button1);
            panel.Children.Add(panel2);

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog("DetailDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(panel, "DetailDialog");
        }

        private async void ButtonEditComment_Click(object sender, RoutedEventArgs e)
        {
            if (ListComments.SelectedIndex == -1) return;
            TaskModel.Item item = (TaskModel.Item)ListComments.SelectedItem;

            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;
            panel.Margin = new Thickness(10);
            panel.Width = 500;

            Label label = new Label();
            label.Content = Properties.Resources.Comment + "[" + item.Id + "]";
            label.FontSize = 16;
            label.Margin = new Thickness(10);
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.FontWeight = FontWeights.Bold;
            panel.Children.Add(label);

            TextBox textBox = new TextBox();
            textBox.Name = "TextComment";
            textBox.Height = 100;
            textBox.Padding = new Thickness(5);
            textBox.Style = (Style)Application.Current.Resources["MaterialDesignOutlinedTextBox"];
            textBox.AcceptsReturn = true;
            textBox.TextWrapping = TextWrapping.Wrap;
            textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            textBox.FontSize = 16;
            textBox.Text = item.Comment;
            MaterialDesignThemes.Wpf.HintAssist.SetHint(textBox, Properties.Resources.Comment);
            textBox.Margin = new Thickness(10);
            panel.Children.Add(textBox);

            CheckBox checkBox = new CheckBox();
            checkBox.Content = Properties.Resources.Internal;
            checkBox.VerticalContentAlignment = VerticalAlignment.Bottom;
            checkBox.FontSize = 16;
            checkBox.Margin = new Thickness(10, 0, 0, 0);
            checkBox.IsChecked = item.Internal;
            panel.Children.Add(checkBox);
            
            Button button = new Button();
            button.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            button.Content = Properties.Resources.Save;
            button.Margin = new Thickness(10);
            button.Tag = item.Id;
            button.Click += ButtonSaveComment_Click;

            Button button1 = new Button();
            button1.Style = (Style)Application.Current.Resources["MaterialDesignPaperLightButton"];
            button1.Content = Properties.Resources.Close;
            button1.Margin = new Thickness(10);
            button1.Click += ButtonCloseComment_Click;

            StackPanel panel2 = new StackPanel();
            panel2.Orientation = Orientation.Horizontal;
            panel2.HorizontalAlignment = HorizontalAlignment.Right;
            panel2.Children.Add(button);
            panel2.Children.Add(button1);
            panel.Children.Add(panel2);

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog("DetailDialog");
            await MaterialDesignThemes.Wpf.DialogHost.Show(panel, "DetailDialog");
        }

        private void ButtonDeleteComment_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonCloseComment_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }

        private void ButtonSaveComment_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            long id = Convert.ToInt64(button.Tag.ToString());

            StackPanel panel = (StackPanel)button.Parent;
            panel = (StackPanel)panel.Parent;
            TextBox textBox = (TextBox)panel.Children[1];
            CheckBox checkBox = (CheckBox)panel.Children[2];

            string comment = textBox.Text;
            bool isInternal = checkBox.IsChecked.Value;
            if (comment == "") return;

            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("Internal", isInternal);
            body.Add("Comment", comment);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                if (id == 0) Global.contactClient.CreateTaskParameters(Constants.CommentsURL, SelectedTask.Id, body);
                else Global.contactClient.UpdateTaskData(Constants.CommentsDetailURL, id, body);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    Global.CloseDialog("DetailDialog");
                    ReloadCommentList();
                });

            }, 10);

        }

        private void ReloadCommentList()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                comments = Global.contactClient.GetTaskSubListData(Constants.CommentsURL, SelectedTask.Id);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    ListComments.ItemsSource = comments;
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }


    }
}
