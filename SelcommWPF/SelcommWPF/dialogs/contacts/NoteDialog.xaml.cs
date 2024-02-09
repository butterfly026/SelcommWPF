using RestSharp;
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
using System.Windows.Shapes;

namespace SelcommWPF.dialogs
{
    /// <summary>
    /// Interaction logic for NoteDialog.xaml
    /// </summary>
    public partial class NoteDialog : UserControl
    {
        private int Type;
        private string ContactCode;
        private string DialogTitle;
        private long NoteId;
        private string NoteContent;
        private string UserName;
        private string DateTime;

        public NoteDialog(int type, string contact, string title, long id, string content, string user = "", string date = "")
        {
            Type = type;
            ContactCode = contact;
            DialogTitle = title;
            NoteId = id;
            NoteContent = content;
            UserName = user;
            DateTime = date;

            InitializeComponent();
            InitializeDialog();
        }

        private void InitializeDialog()
        {
            LabelTitle.Content = DialogTitle;
            TextNote.Text = NoteContent;

            if (DialogTitle.Contains(Properties.Resources.Event)) MaterialDesignThemes.Wpf.HintAssist.SetHint(TextNote, Properties.Resources.Event);
            else if (DialogTitle.Contains(Properties.Resources.Note)) MaterialDesignThemes.Wpf.HintAssist.SetHint(TextNote, Properties.Resources.Note);

            if (NoteId == 0)
            {
                LabelUser.Visibility = Visibility.Collapsed;
                LabelDateTime.Visibility = Visibility.Collapsed;
            }
            else
            {
                LabelUser.Content = UserName;
                LabelDateTime.Content = DateTime;
            }
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            string note = TextNote.Text;
            if (note == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Note);
                return;
            }

            var window = this;
            LoadingPanel.Visibility = Visibility.Visible;

            if (NoteId == 0)
            {
                EasyTimer.SetTimeout(() =>
                {
                    Global.RequestAPI(new List<string>() { Constants.NotesListURL, Constants.ServicesNoteURL }[Type], Method.Post, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("code", Global.ContactCode), Global.BuildDictionary("api-version", new List<string>() { "1.1", "1.2" }[Type]), Global.BuildDictionary("Note", note));

                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        Global.mainWindow.GetDisplayDetails();
                        Global.CloseDialog();
                    });

                }, 10);

            }
            else
            {
                EasyTimer.SetTimeout(() =>
                {
                    Global.RequestAPI(new List<string>() { Constants.NoteEditURL, Constants.ServicesNoteEditURL }[Type], Method.Put, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("noteId", NoteId), Global.BuildDictionary("api-version", new List<string>() { "1.1", "1.2" }[Type]), Global.BuildDictionary("Note", note));

                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        Global.mainWindow.GetDisplayDetails();
                        Global.CloseDialog();
                    });

                }, 10);
            }

        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void TextNote_TextChanged(object sender, TextChangedEventArgs e)
        {
            ButtonSave.IsEnabled = TextNote.Text != "";
        }
    }
}
