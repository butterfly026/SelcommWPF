using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using RestSharp;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace SelcommWPF.dialogs.contacts
{
    /// <summary>
    /// Interaction logic for EventDialog.xaml
    /// </summary>
    public partial class NewDocumentDialog : UserControl
    {
        private string FileContent;
        private string CurrentContactCode;
        private bool IsService;
        private long ServiceReference;

        public NewDocumentDialog(bool isService, string ContactCode, long reference)
        {
            IsService = isService;
            if (!isService) CurrentContactCode = ContactCode;
            else ServiceReference = reference;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            DateAuthored.SelectedDate = DateTime.Now;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("Resource", IsService ? "/Services/Documents/New" : "/Contacts/Documents/New");
                query.Add("api-version", 1.2);
                HttpStatusCode result = Global.RequestAPI(Constants.AuthrisationURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, null);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result != HttpStatusCode.OK) {
                        MessageUtils.ShowErrorMessage("", Properties.Resources1.Feature_Error);
                        Global.CloseDialog();
                    }
                });

            }, 10);
        }

        private void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            string filename = TextFileName.Text, namebody = "", extension = "";
            int i;
            for (i = filename.Length - 1; i >= 0 && filename[i] != '.'; i--) ;
            if(i >= 0) { namebody = filename.Substring(0, i); extension = filename.Substring(i + 1); }
            else { namebody = filename; }

            DateTime date = DateAuthored.SelectedDate.Value;
            string formattedDate = date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("Name", namebody);
            body.Add("Category", "User");
            body.Add("FileType", extension);
            body.Add("Note", TextNote.Text);
            body.Add("Author", TextAuthor.Text);
            body.Add("DateAuthored", formattedDate);
            body.Add("UserEditable", CheckUserEditable.IsChecked);
            body.Add("ContactEditable", CheckContactEditable.IsChecked);
            body.Add("ContactVisible", CheckContactVisible.IsChecked);
            body.Add("Content", FileContent);
            LoadingPanel.Visibility = Visibility;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(IsService ? Constants.ServiceDocumentCreateURL : Constants.DocumentCreateURL, Method.Post, Global.GetHeader(Global.CustomerToken),
                    IsService ? Global.BuildDictionary("ServiceReference", ServiceReference) : Global.BuildDictionary("ContactCode", CurrentContactCode), Global.BuildDictionary("api-version", 2.0), body, false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;

                    if (result == HttpStatusCode.Created)
                    {
                        Global.mainWindow.ShowDocumentList(IsService);
                        MessageUtils.ShowMessage("", "Document Created Successfully");
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

        private void ButtonSelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (!dlg.ShowDialog().Value) return;

            TextFileName.Text = dlg.SafeFileName;
            byte[] bytes = File.ReadAllBytes(dlg.FileName);
            FileContent = Convert.ToBase64String(bytes);
            CheckCreatable();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void CheckCreatable()
        {
            bool creatable = true;
            if (TextFileName.Text.IsNullOrEmpty() || TextAuthor.Text.IsNullOrEmpty()) creatable = false;
            if (DateAuthored.SelectedDate == null) creatable = false;

            ButtonCreate.IsEnabled = creatable;
        }

        private void TextInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckCreatable();
        }

        private void DateAuthored_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckCreatable();
        }
    }
}
