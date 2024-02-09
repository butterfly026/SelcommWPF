using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.contacts;
using SelcommWPF.clients.models.messages;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
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

namespace SelcommWPF.dialogs.users
{
    /// <summary>
    /// Interaction logic for ServiceProviderUser.xaml
    /// </summary>
    public partial class ServiceProviderUser : UserControl
    {
        private string SelectedUserId;

        private Dictionary<string, object> ConfigurationData;
        private List<Dictionary<string, string>> TitleList;
        private List<UserModel.BusinessUnitModel> BusinessUnitsList;
        private List<UserModel.BusinessUnitModel> UserTeamsList;
        private List<UserModel.BusinessUnitModel> UserRolesList;
        private List<RelatedContactModel.TimeZone> DefaultTimeZoneList;
        private List<RelatedContactModel.TimeZone> CurrentTimeZoneList;
        private RelatedContactModel.TimeZone SelectedTimeZone;

        private IDisposable RequestTimer = null;
        private ComplexResponse ComplexResult = new ComplexResponse();

        public ServiceProviderUser(string userId = "")
        {
            SelectedUserId = userId;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            LabelDialogTitle.Content = SelectedUserId == "" ? "Create Service Provider User" : string.Format("Servic Provider User : [{0}]", SelectedUserId);
            if (SelectedUserId != "")
            {
                TextUserId.Visibility = Visibility.Hidden;
                ButtonSuspend.Visibility = Visibility.Visible;
                ButtonReset.Visibility = Visibility.Visible;
                ButtonDelete.Visibility = Visibility.Visible;
                GridPassword.Visibility = Visibility.Collapsed;
                PanelPassword.Visibility = Visibility.Collapsed;
            }

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> header = Global.GetHeader(Global.CustomerToken);
                Dictionary<string, object> query1 = Global.BuildDictionary("api-version", 1.1);
                Dictionary<string, object> query0 = Global.BuildDictionary("api-version", 1.0);

                if (Global.userClient == null) Global.userClient = new clients.UserClient();
                if (Global.reportClient == null) Global.reportClient = new ReportClient();
                if (Global.contactClient == null) Global.contactClient = new clients.ContactClient();

                Dictionary<string, string> result = new Dictionary<string, string>();
                UserModel selectedUser = new UserModel();

                if (SelectedUserId == "") result = Global.RequestAPI<Dictionary<string, string>>(Constants.NextLoginIdURL, Method.Post, header, null, query0, "");
                else selectedUser = Global.RequestAPI<UserModel>(Constants.FullUserURL, Method.Get, header, Global.BuildDictionary("userId", SelectedUserId), query0, "");

                TitleList = Global.RequestAPI<List<Dictionary<string, string>>>(Constants.ContactTitlesURL, Method.Get, header, null, query1, "");
                ConfigurationData = Global.RequestAPI<Dictionary<string,object>>(Constants.ServiceProviderUserConfigURL, Method.Get, header, null, query0, "");
                BusinessUnitsList = Global.RequestAPI<List<UserModel.BusinessUnitModel>>(Constants.BusinessUnitsURL, Method.Get, header, null, query0, "");
                UserTeamsList = Global.RequestAPI<List<UserModel.BusinessUnitModel>>(Constants.UserTeamsURL, Method.Get, header, null, query0, "");
                UserRolesList = Global.RequestAPI<List<UserModel.BusinessUnitModel>>(Constants.ServiceProviderUserRolesURL, Method.Get, header, null, query0, "");
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.1);
                query.Add("SearchString", "");
                query.Add("SkipRecords", 0);
                query.Add("TakeRecords", 10);
                DefaultTimeZoneList = Global.RequestAPI<List<RelatedContactModel.TimeZone>>(Constants.TimeZoneDefaultURL, Method.Get, header, null, query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (TitleList == null || ConfigurationData == null || BusinessUnitsList == null || UserTeamsList == null || UserRolesList == null || DefaultTimeZoneList == null)
                    {
                        Global.CloseDialog("DetailDialog");
                        return;
                    }

                    ComboGender.ItemsSource = new List<string>() { "Male", "Female", "Unknown" };
                    if (DefaultTimeZoneList != null && DefaultTimeZoneList.Count > 0)
                    {
                        TextTimeZone.Text = string.Format("{0} [{1}]", DefaultTimeZoneList[0].Name, DefaultTimeZoneList[0].Country);
                        SelectedTimeZone = DefaultTimeZoneList[0];
                    }

                    List<string> list1 = new List<string>();
                    foreach (Dictionary<string, string> item in TitleList) list1.Add(item["Name"]);
                    ComboTitle.ItemsSource = list1;

                    if (SelectedUserId == "")
                    {
                        if (result == null || result.Count == 0) return;
                        TextUserId.Text = result["UserId"];
                        TextLoginId.Text = Global.UserID;
                    }
                    else
                    {
                        TextName.Text = selectedUser.FamilyName;
                        TextFirstName.Text = selectedUser.FirstName;

                        ComboTitle.SelectedIndex = list1.IndexOf(selectedUser.Title);
                        DatePickerBirth.SelectedDate = DateTime.Parse(selectedUser.DateOfBirth);
                        ComboGender.SelectedIndex = ((List<string>)ComboGender.ItemsSource).IndexOf(selectedUser.Gender);
                        TextEmployeeReference.Text = selectedUser.EmployeeReference;

                        if (BusinessUnitsList != null)
                        {
                            int count = BusinessUnitsList.Count;
                            for (int i = 0; i < count; i++)
                            {
                                foreach (UserModel.BusinessUnitModel item in selectedUser.BusinessUnits)
                                {
                                    if (BusinessUnitsList[i].Id == item.Id)
                                    {
                                        BusinessUnitsList[i].IsChecked = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (UserRolesList != null)
                        {
                            int count = UserRolesList.Count;
                            for (int i = 0; i < count; i++)
                            {
                                foreach (UserModel.BusinessUnitModel item in selectedUser.Roles)
                                {
                                    if (UserRolesList[i].Name == item.Name)
                                    {
                                        UserRolesList[i].IsChecked = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (UserTeamsList != null)
                        {
                            int count = UserTeamsList.Count;
                            for (int i = 0; i < count; i++)
                            {
                                foreach (UserModel.BusinessUnitModel item in selectedUser.Teams)
                                {
                                    if (UserTeamsList[i].Id == item.Id)
                                    {
                                        UserTeamsList[i].IsChecked = true;
                                        break;
                                    }
                                }
                            }

                        }

                        TextLoginEmail.Text = selectedUser.AuthenticationEmail;
                        TextLoginMobile.Text = selectedUser.AuthenticationMobile;
                        TextLoginId.Text = selectedUser.AuthenticationLoginId;
                    }

                    ComboBusinessUnit.ItemsSource = BusinessUnitsList;
                    ComboRole.ItemsSource = UserRolesList;
                    ComboTeam.ItemsSource = UserTeamsList;

                    if (SelectedUserId != "")
                    {
                        SetMultipleComboBox(ComboBusinessUnit);
                        SetMultipleComboBox(ComboRole);
                        SetMultipleComboBox(ComboTeam);
                    }
                });

            }, 10);


        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, object> body = new Dictionary<string, object>();
            if (SelectedUserId == "") body.Add("Id", TextUserId.Text);
            body.Add("EmployeeReference", TextEmployeeReference.Text);
            body.Add("TimeZoneId", SelectedTimeZone.Id);
            body.Add("Gender", ComboGender.SelectedItem == null ? null : ComboGender.SelectedItem.ToString());
            body.Add("DateOfBirth", DatePickerBirth.SelectedDate == null ? null : DatePickerBirth.SelectedDate.Value.ToString("yyyy-MM-ddThh:mm:ss"));
            body.Add("FirstName", TextFirstName.Text);
            body.Add("FamilyName", TextName.Text);
            body.Add("Password", TextPassword.Password);
            body.Add("ChangePasswordOnFirstLogin", ToggleChangePassword.IsChecked.Value);
            body.Add("AuthenticationEmail", TextLoginEmail.Text == "" ? TextDefaultEmail.Text : TextLoginEmail.Text);
            body.Add("AuthenticationMobile", TextLoginMobile.Text);
            body.Add("AuthenticationLoginId", TextLoginId.Text);
            body.Add("Title", ComboTitle.SelectedItem == null ? null : ComboTitle.SelectedItem.ToString());

            List<Dictionary<string, object>> listBU = new List<Dictionary<string, object>>();
            int count = ComboBusinessUnit.Items.Count;
            string name = ComboDefaultBusinessUnit.SelectedItem.ToString();

            for (int i = 0; i < count; i++)
            {
                UserModel.BusinessUnitModel item = ComboBusinessUnit.Items[i] as UserModel.BusinessUnitModel;
                Dictionary<string, object> value = new Dictionary<string, object>();
                if (item.IsChecked)
                {
                    value.Add("Id", item.Id);
                    if (item.Name == name) value.Add("Default", true);
                    listBU.Add(value);
                }
            }

            List<Dictionary<string, object>> listRole = new List<Dictionary<string, object>>();
            count = ComboRole.Items.Count;
            name = ComboDefaultRole.SelectedItem.ToString();

            for (int i = 0; i < count; i++)
            {
                UserModel.BusinessUnitModel item = ComboRole.Items[i] as UserModel.BusinessUnitModel;
                Dictionary<string, object> value = new Dictionary<string, object>();
                if (item.IsChecked)
                {
                    value.Add("Name", item.Name);
                    if (item.Name == name) value.Add("Default", true);
                    listRole.Add(value);
                }
            }

            List<Dictionary<string, object>> listTeam = new List<Dictionary<string, object>>();
            count = ComboTeam.Items.Count;

            for (int i = 0; i < count; i++)
            {
                UserModel.BusinessUnitModel item = ComboTeam.Items[i] as UserModel.BusinessUnitModel;
                Dictionary<string, object> value = new Dictionary<string, object>();
                if (item.IsChecked)
                {
                    value.Add("Id", item.Id);
                    listTeam.Add(value);
                }
            }

            body.Add("BusinessUnits", listBU);
            body.Add("Roles", listRole);
            body.Add("Teams", listTeam);

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = SelectedUserId == "" ? Global.RequestAPI(Constants.ServiceProviderUsersURL, Method.Post, Global.GetHeader(Global.CustomerToken), null, Global.BuildDictionary("api-version", 1.0), body)
                    : Global.RequestAPI(Constants.FullUserURL, Method.Patch, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("userId", SelectedUserId), Global.BuildDictionary("api-version", 1.0), body);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result != HttpStatusCode.OK && result != HttpStatusCode.Created) MessageUtils.ShowErrorMessage("", "Create Service Provider User failed.");
                    else
                    {
                        ServiceProvidersDialog.Instance.ShowServiceProviderUsers(0, 20);
                        Global.CloseDialog("DetailDialog");
                    }
                });
            }, 10);

        }

        private void TextPhoneOrEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            StackPanel panel = (StackPanel)textBox.Parent;
            ProgressBar progressBar = (ProgressBar)panel.Children[1];

            string url = Constants.ValidatePhone;
            string url1 = Constants.CheckEmailURL;
            string param = textBox.Text;
            if (param == "") return;
            string tag = textBox.Tag.ToString();
            progressBar.Visibility = Visibility.Visible;

            if (tag == "Login Id")
            {
                url = Constants.CheckLoginIdURL;
                CheckUniqueAuthInfo(textBox, url, param, tag, progressBar);
                return;
            }
            else if (tag == Properties.Resources.Authenticate_Email)
            {
                url = Constants.ValidateEmail;
                url1 = Constants.CheckEmailURL;
            }
            else if (tag == Properties.Resources.Authenticate_Mobile)
            {
                url = Constants.ValidateSMS;
                url1 = Constants.CheckMobileURL;
            }

            EasyTimer.SetTimeout(() =>
            {
                ValidModel result = Global.RequestAPI<ValidModel>(url, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("Param", param), Global.BuildDictionary("api-version", 1.0), "");
                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result.Valid)
                    {
                        CheckUniqueAuthInfo(textBox, url1, param, tag, progressBar);
                    }
                    else
                    {
                        MessageUtils.ShowErrorMessage("", param + " : " + string.Format(Properties.Resources.Invalid_Message, tag));
                        progressBar.Visibility = Visibility.Collapsed;
                        textBox.Text = "";
                        textBox.Focus();
                    }
                });

            }, 10);
        }

        private void CheckUniqueAuthInfo(TextBox textBox, string url, string param, string tag, ProgressBar progressBar)
        {
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, bool> result = Global.RequestAPI<Dictionary<string, bool>>(url, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("Parameter", param), Global.BuildDictionary("api-version", 1.0), "");
                Application.Current.Dispatcher.Invoke(delegate
                {
                    progressBar.Visibility = Visibility.Collapsed;
                    if (result == null || result["Used"])
                    {
                        MessageUtils.ShowMessage("", param + " : " + string.Format(Properties.Resources.Choose_Another_Message, tag));
                        textBox.Text = "";
                        textBox.Focus();
                    }
                    else MessageUtils.ShowMessage("", param + " : " + string.Format(Properties.Resources.Available_Message, tag));
                });

            }, 10);

        }

        private void ButtonCheck_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string tag = button.Tag.ToString();

            string url = Constants.ValidateEmail;
            string url1 = Constants.CheckEmailURL;
            TextBox textBox = TextLoginEmail;
            ProgressBar progressBar = ProgressLoginEmail;

            if (tag == "Email")
            {
                tag = Properties.Resources.Authenticate_Email;
                url = Constants.ValidateEmail;
                url1 = Constants.CheckEmailURL;
                textBox = TextLoginEmail;
                progressBar = ProgressLoginEmail;
            }
            else if (tag == "Mobile")
            {
                tag = Properties.Resources.Authenticate_Mobile;
                url = Constants.ValidateSMS;
                url1 = Constants.CheckMobileURL;
                textBox = TextLoginMobile;
                progressBar = ProgressLoginMobile;
            }
            else if (tag == "Login")
            {
                tag = Properties.Resources.Login_id;
                url1 = Constants.CheckLoginIdURL;
                textBox = TextLoginId;
                progressBar = ProgressLoginId;
            }

            string param = textBox.Text;
            if (param == "") return;
            progressBar.Visibility = Visibility.Visible;

            if (tag == Properties.Resources.Login_id)
            {
                CheckUniqueAuthInfo(textBox, url1, param, tag, progressBar);
            }
            else
            {
                EasyTimer.SetTimeout(() =>
                {

                    ValidModel result = Global.RequestAPI<ValidModel>(url, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("Param", param), Global.BuildDictionary("api-version", 1.0), "");

                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        if (result.Valid)
                        {
                            CheckUniqueAuthInfo(textBox, url1, param, tag, progressBar);
                        }
                        else
                        {
                            MessageUtils.ShowErrorMessage("", param + " : " + string.Format(Properties.Resources.Invalid_Message, tag));
                            progressBar.Visibility = Visibility.Collapsed;
                            textBox.Text = "";
                            textBox.Focus();
                        }
                    });

                }, 10);
            }
        }

        private void TextPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            string password = TextPassword.Password;
            if (password == "") return;

            ProgressComplex.Visibility = Visibility.Visible;
            BorderComplex.Visibility = Visibility.Collapsed;

            EasyTimer.SetTimeout(() =>
            {
                ComplexResult = Global.RequestAPI<ComplexResponse>(Constants.ComplexURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("password", password), Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    ProgressComplex.Visibility = Visibility.Collapsed;
                    LabelComplex.Content = ComplexResult.Result == "SUCCESS" ? string.Format(Properties.Resources.Password_Strength, ComplexResult.PasswordStrength) : ComplexResult.Reason;
                    BorderComplex.Visibility = Visibility.Visible;

                    Brush[] brushes = new Brush[] { Brushes.Red, Brushes.Red, Brushes.Yellow, Brushes.Green };
                    Brush[] brushes1 = new Brush[] { Brushes.White, Brushes.White, Brushes.Black, Brushes.Black };
                    List<string> type = new List<string> { "Unacceptable", "Weak", "Medium", "Strong" };
                    BorderComplex.Background = brushes[type.IndexOf(ComplexResult.PasswordStrength)];
                    LabelComplex.Foreground = brushes1[type.IndexOf(ComplexResult.PasswordStrength)];
                });

            }, 10);
        }

        private void Suggestion_Password_Click(object sender, RoutedEventArgs e)
        {
            ProgressComplex.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> SuggestionPassword = Global.RequestAPI<Dictionary<string, string>>(Constants.SuggestopnURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, Global.BuildDictionary("api-version", 1.0), "");

                if (SuggestionPassword == null || SuggestionPassword["Password"] == null) return;

                Application.Current.Dispatcher.Invoke(async delegate
                {
                    ProgressComplex.Visibility = Visibility.Hidden;
                    bool result = await MessageUtils.ConfirmMessageAsync(Properties.Resources.Alert, string.Format("{0}\n{1}", Properties.Resources.Suggestion_Password,
                        SuggestionPassword["Password"]));

                    if (result)
                    {
                        TextPassword.Password = SuggestionPassword["Password"];
                        LabelComplex.Content = string.Format(Properties.Resources.Password_Strength, ComplexResult.PasswordStrength);
                        BorderComplex.Visibility = Visibility.Visible;
                        ComplexResult.PasswordStrength = "Strong";
                        ComplexResult.Reason = "";
                        ComplexResult.Result = "SUCCESS";

                        BorderComplex.Background = Brushes.Green;
                        LabelComplex.Foreground = Brushes.Black;
                    }
                });
            }, 10);
        }

        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string name = ((Label)sender).Tag.ToString();
            ComboBox combo = Global.FindChild<ComboBox>(this, name);
            combo.IsDropDownOpen= true; 
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = e.RoutedEvent.Name == "Checked";
            CheckBox checkBox = (CheckBox)sender;
            string name = checkBox.Content.ToString();
            string tag = checkBox.Tag.ToString();

            ComboBox combo = Global.FindChild<ComboBox>(this, tag);
            SetMultipleComboBox(combo);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            if (combo.SelectedIndex == -1) return;
            SetMultipleComboBox(combo);
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            SetMultipleComboBox(combo);
        }

        private void SetMultipleComboBox(ComboBox combo)
        {
            Label label = ((Grid)combo.Parent).Children[1] as Label;
            combo.SelectedIndex = 0;

            string content = "";
            List<string> list = new List<string>();
            int count = combo.Items.Count;

            for (int i = 0; i < count; i++)
            {
                UserModel.BusinessUnitModel item = combo.Items[i] as UserModel.BusinessUnitModel;
                if (item.IsChecked)
                {
                    content += ", " + item.Name;
                    list.Add(item.Name);
                }
            }

            content = content == "" ? "" : content.Substring(2);
            label.Content = content;
            label.Background = content == "" ? Brushes.Transparent : Brushes.White;
            if (content == "") combo.SelectedIndex = -1;

            if (combo.Name == "ComboBusinessUnit")
            {
                ComboDefaultBusinessUnit.ItemsSource = list;
                ComboDefaultBusinessUnit.SelectedIndex = list.Count == 1 ? 0 : -1;
            }
            else if (combo.Name == "ComboRole")
            {
                ComboDefaultRole.ItemsSource = list;
                ComboDefaultRole.SelectedIndex = list.Count == 1 ? 0 : -1;
            }
        }

        private void TextTimeZone_KeyUp(object sender, KeyEventArgs e)
        {
            string search = TextTimeZone.Text;
            if (search == "" || RequestTimer != null)
            {
                ListTimeZone.ItemsSource = new List<string>();
                CardTimeZone.Visibility = Visibility.Collapsed;
                return;
            }

            if (e.Key == Key.Down)
            {
                ListTimeZone.Focus();
                ListTimeZone.SelectedIndex = 0;
                return;
            }

            RequestTimer = EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    search = TextTimeZone.Text;
                    ProgressTimeZone.Visibility = Visibility.Visible;
                    CardTimeZone.Visibility = Visibility.Collapsed;
                });

                List<string> list = new List<string>();
                if (search != "")
                {
                    Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                    query.Add("SearchString", search);
                    query.Add("SkipRecords", 0);
                    query.Add("TakeRecords", 10);
                    CurrentTimeZoneList = Global.RequestAPI<List<RelatedContactModel.TimeZone>>(Constants.TimeZoneListURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, "");
                    foreach (RelatedContactModel.TimeZone item in CurrentTimeZoneList) list.Add(string.Format("{0} [{1}]", item.Name, item.Country));
                }

                Application.Current.Dispatcher.Invoke(delegate
                {
                    ProgressTimeZone.Visibility = Visibility.Collapsed;
                    ListTimeZone.ItemsSource = new List<string>();
                    ListTimeZone.ItemsSource = list;
                    CardTimeZone.Visibility = CurrentTimeZoneList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                    RequestTimer.Dispose();
                    RequestTimer = null;
                });

            }, 2000);
        }

        private void ListTimeZone_KeyUp(object sender, KeyEventArgs e)
        {
            if (ListTimeZone.SelectedItem == null || e.Key != Key.Enter) return;
            SelectTimeZone();
        }

        private void ListTimeZone_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ListTimeZone.SelectedItem == null) return;
            SelectTimeZone();
        }

        private void SelectTimeZone()
        {
            RelatedContactModel.TimeZone item = CurrentTimeZoneList[ListTimeZone.SelectedIndex];
            SelectedTimeZone = item;
            TextTimeZone.Text = item.Name;
            ListTimeZone.ItemsSource = new List<string>();
            CardTimeZone.Visibility = Visibility.Collapsed;
            TextTimeZone.Focus();
            TextTimeZone.SelectionStart = TextTimeZone.Text.Length;
        }

        private void TextName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ButtonSave.IsEnabled = TextName.Text != "" && TextFirstName.Text != "" && ComboDefaultBusinessUnit.SelectedIndex != -1 && ComboDefaultRole.SelectedIndex != -1
                && TextTimeZone.Text != "" && TextLoginId.Text != "";
            ButtonCheckLogin.IsEnabled = TextLoginId.Text != "";
            ButtonCheckEmail.IsEnabled = TextLoginEmail.Text != "";
            ButtonCheckMobile.IsEnabled = TextLoginMobile.Text != "";
        }

        private void ComboRequire_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonSave.IsEnabled = TextName.Text != "" && TextFirstName.Text != "" && ComboDefaultBusinessUnit.SelectedIndex != -1 && ComboDefaultRole.SelectedIndex != -1
                && TextTimeZone.Text != "" && TextLoginId.Text != "";
        }

        private void ButtonSuspend_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("to", "9999-12-31T12:59:59");
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.SelcommUserLockURL, Method.Put, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("Identifier", Global.UserID), Global.BuildDictionary("api-version", 3.1), body);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK || result == HttpStatusCode.NoContent) 
                    {
                        MessageUtils.ShowMessage("", "User suspended successfully");
                        Global.CloseDialog("DetailDialog");
                    }
                    else MessageUtils.ShowErrorMessage("", "User suspend failed");
                });

            }, 10);
        }

        private async void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("PasswordDialog")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ResetPassword(SelectedUserId), "PasswordDialog");
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.DeleteUserURL, Method.Delete, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", SelectedUserId), Global.BuildDictionary("api-version", 1.0), null);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK)
                    {
                        MessageUtils.ShowMessage("", "User " + SelectedUserId + " has been deleted");
                        ServiceProvidersDialog.Instance.ShowServiceProviderUsers(0, 20);
                        Global.CloseDialog("DetailDialog");
                    }
                });

            }, 10);
        }
    }
}
