using SelcommWPF.clients.models;
using SelcommWPF.clients.models.contacts;
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
    /// Interaction logic for RelatedContactDialog.xaml
    /// </summary>
    public partial class RelatedContactDialog : UserControl
    {
        private bool IsFromMenu = false;
        private string ServiceProvidedNextId = "";
        private ComplexResponse ComplexResult = new ComplexResponse();

        private RelatedContactModel RelatedContactData = new RelatedContactModel();
        private List<Dictionary<string, string>> TitleList = new List<Dictionary<string, string>>();
        private List<RelatedContactModel.Relationship> RelationShipList = new List<RelatedContactModel.Relationship>();

        public RelatedContactDialog(string id = "", bool fromMenu = false)
        {
            RelatedContactData.Id = id;
            IsFromMenu = fromMenu;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LabelDialogTitle.Content = RelatedContactData.Id == "" ? Properties.Resources.Add_Related_Contact : string.Format(Properties.Resources.Update_Related_Contact, RelatedContactData.Id);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                RelationShipList = Global.contactClient.GetRelationShipList(Constants.RelationShipURL);
                TitleList = Global.contactClient.GetAliasTypesAndTitles(Constants.ContactTitlesURL);
                Dictionary<string, string> result = Global.userClient.GetServiceProvidedNextId(Constants.NextLoginIdURL);
                ServiceProvidedNextId = result["UserId"];

                if (RelatedContactData.Id != "")
                {
                    RelatedContactData = Global.contactClient.GetRelatedContactDetail(Constants.RelatedContactDetailURL, Global.ContactCode, RelatedContactData.Id);
                    RelatedContactData.Mobile = RelatedContactData.MobilePhone;
                }

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (RelationShipList == null || TitleList == null || result == null || RelatedContactData == null)
                    {
                        Global.CloseDialog(IsFromMenu ? "MyDialogHost" : "DetailDialog");
                        return;
                    }

                    List<string> genders = new List<string>() { Properties.Resources.Male, Properties.Resources.Female, Properties.Resources.Undefined };
                    List<string> titles = new List<string>();
                    foreach (Dictionary<string, string> item in TitleList) titles.Add(item["Name"]);

                    ComboTitle.ItemsSource = titles;
                    ComboGender.ItemsSource = genders;
                    ComboGender.SelectedIndex = 0;
                    TextLoginId.Text = ServiceProvidedNextId;

                    foreach (RelatedContactModel.Relationship item in RelationShipList)
                    {
                        CheckBox checkBox = new CheckBox();
                        checkBox.Content = item.Name;
                        checkBox.Tag = item.Id;
                        checkBox.FontSize = 16;
                        checkBox.VerticalAlignment = VerticalAlignment.Bottom;
                        checkBox.HorizontalAlignment = HorizontalAlignment.Left;
                        PanelRelationShip.Children.Add(checkBox);
                    }

                    if (RelatedContactData.Id != "")
                    {
                        TextMobile.Text = RelatedContactData.Mobile;
                        TextEmail.Text = RelatedContactData.Email;
                        TextFamilyName.Text = RelatedContactData.FamilyName;
                        TextFirstName.Text = RelatedContactData.FirstName;

                        ComboTitle.SelectedIndex = titles.IndexOf(RelatedContactData.Title);
                        if (RelatedContactData.DateOfBirth != null && RelatedContactData.DateOfBirth != "") DatePickerBirth.SelectedDate = DateTime.Parse(RelatedContactData.DateOfBirth);
                        ComboGender.SelectedIndex = genders.IndexOf(RelatedContactData.Gender);

                        int count = PanelRelationShip.Children.Count;
                        for (int i = 0; i < count; i++)
                        {
                            CheckBox checkBox = (CheckBox)PanelRelationShip.Children[i];
                            foreach(RelatedContactModel.Relationship item in RelatedContactData.RelationshipTypes)
                            {
                                if (checkBox.Tag.ToString() == item.Id)
                                {
                                    checkBox.IsChecked = true;
                                    break;
                                }
                            }
                        }

                        TextLoginMobile.Text = RelatedContactData.Mobile;
                        TextLoginEmail.Text = RelatedContactData.Email;

                    }
                });
            }, 10);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog(IsFromMenu ? "MyDialogHost" : "DetailDialog");
        }

        private void CheckAuthenticate_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = e.RoutedEvent.Name == "Checked";
            GridAuthenticate.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("Id", ServiceProvidedNextId);
            body.Add("Mobile", TextMobile.Text);
            body.Add("Email", TextEmail.Text);
            body.Add("FamilyName", TextFamilyName.Text);
            body.Add("FirstName", TextFirstName.Text);
            body.Add("Title", ComboTitle.SelectedItem?.ToString());
            body.Add("DateOfBirth", DatePickerBirth.SelectedDate?.ToString("yyyy-MM-dd"));
            body.Add("Gender", ComboGender.SelectedItem?.ToString());
            body.Add("TimeZoneId", 0);
            body.Add("AuthenticationLoginId",  CheckAuthenticate.IsChecked.Value ? TextLoginId.Text : "");
            body.Add("AuthenticationMobile", CheckAuthenticate.IsChecked.Value ? TextLoginMobile.Text : "");
            body.Add("AuthenticationEmail", CheckAuthenticate.IsChecked.Value ? TextLoginEmail.Text : "");
            body.Add("ChangePasswordOnFirstLogin", CheckAuthenticate.IsChecked.Value ? ToggleChangePassword.IsChecked.Value : false);
            body.Add("Password", CheckAuthenticate.IsChecked.Value ? TextPassword.Password : "");

            int count = PanelRelationShip.Children.Count;
            List<Dictionary<string, string>> relationships = new List<Dictionary<string, string>>();

            for (int i = 0; i < count; i++)
            {
                Dictionary<string, string> item = new Dictionary<string, string>();
                CheckBox checkBox = (CheckBox)PanelRelationShip.Children[i];
                if (checkBox.IsChecked.Value)
                {
                    item.Add("Id", checkBox.Tag.ToString());
                    relationships.Add(item);
                }
            }
            
            body.Add("RelationshipTypes", relationships);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = 
                    RelatedContactData.Id == "" ? Global.contactClient.CreateRelatedContactData(Constants.RelatedContactListURL, Global.ContactCode, body)
                    : Global.contactClient.UpdateRelatedContactData(Constants.RelatedContactDetailURL, Global.ContactCode, RelatedContactData.Id, body);

                Application.Current.Dispatcher.Invoke(async delegate
                {
                    if (result == HttpStatusCode.OK || result == HttpStatusCode.Created)
                    {
                        MessageUtils.ShowMessage("", string.Format(Properties.Resources.Related_Contact_Success, 
                            RelatedContactData.Id == "" ? Properties.Resources.Add : Properties.Resources.Update));
                        Global.CloseDialog(IsFromMenu ? "MyDialogHost" : "DetailDialog");
                        if (!IsFromMenu)
                        {
                            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MyDialogHost")) Global.CloseDialog();
                            await MaterialDesignThemes.Wpf.DialogHost.Show(new RelatedContactsList(), "MyDialogHost");
                        }
                    }
                    else
                    {
                        //MessageUtils.ShowErrorMessage("", string.Format("Related Contact {0} failed", RelatedContactData.Id == "" ? "add" : "update"));
                        LoadingPanel.Visibility = Visibility.Hidden;
                    }
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
                CheckUniqueAuthInfo(url1, param, tag, progressBar);
            }
            else
            {
                EasyTimer.SetTimeout(() =>
                {

                    ValidModel result = Global.messageClient.ValidatePhoneOrEmail(url, param);
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        if (result.Valid)
                        {
                            CheckUniqueAuthInfo(url1, param, tag, progressBar);
                        }
                        else
                        {
                            MessageUtils.ShowErrorMessage("", string.Format(Properties.Resources.Invalid_Message, tag));
                            progressBar.Visibility = Visibility.Collapsed;
                        }
                    });

                }, 10);
            }

        }

        private void CheckUniqueAuthInfo(string url, string param, string tag, ProgressBar progressBar)
        {

            EasyTimer.SetTimeout(() =>
            {

                Dictionary<string, bool> result = Global.userClient.GetAuthenticateUnique(url, param);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    progressBar.Visibility = Visibility.Collapsed;
                    if (result == null || result["Used"]) MessageUtils.ShowMessage("", string.Format(Properties.Resources.Choose_Another_Message, tag));
                    else MessageUtils.ShowMessage("", string.Format(Properties.Resources.Available_Message, tag));
                });

            }, 10);

        }

        private void TextPhoneOrEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            StackPanel panel = (StackPanel)textBox.Parent;
            ProgressBar progressBar = (ProgressBar)panel.Children[1];

            string url = Constants.ValidatePhone;
            string param = textBox.Text;
            if (param == "") return;
            string tag = textBox.Tag.ToString();
            progressBar.Visibility = Visibility.Visible;

            if (tag == Properties.Resources.Contact_Phone) url = Constants.ValidatePhone;
            else if (tag == Properties.Resources.Email_Address || tag == Properties.Resources.Authenticate_Email) url = Constants.ValidateEmail;
            else if (tag == Properties.Resources.Authenticate_Mobile) url = Constants.ValidateSMS;

            EasyTimer.SetTimeout(() =>
            {

                ValidModel result = Global.messageClient.ValidatePhoneOrEmail(url, param);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result.Valid) MessageUtils.ShowMessage("", string.Format(Properties.Resources.Available_Message, tag));
                    else MessageUtils.ShowErrorMessage("", string.Format(Properties.Resources.Invalid_Message, tag));
                    progressBar.Visibility = Visibility.Collapsed;
                });

            }, 10);

        }

        private void TextMobile_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CheckAuthenticate.IsChecked.Value) return;
            TextLoginMobile.Text = TextMobile.Text;
        }

        private void TextEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CheckAuthenticate.IsChecked.Value) return;
            TextLoginEmail.Text = TextEmail.Text;
        }

        private void Suggestion_Password_Click(object sender, RoutedEventArgs e)
        {
            ProgressComplex.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> SuggestionPassword = Global.userClient.GetSuggestion(Constants.SuggestopnURL);
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

        private void TextPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            string password = TextPassword.Password;
            if (password == "") return;

            ProgressComplex.Visibility = Visibility.Visible;
            BorderComplex.Visibility = Visibility.Collapsed;

            EasyTimer.SetTimeout(() =>
            {

                ComplexResult = Global.userClient.CheckComplexity(Constants.ComplexURL, password);
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

        private void TextFamilyName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ButtonSave.IsEnabled = TextFamilyName.Text != "";
        }
    }
}
