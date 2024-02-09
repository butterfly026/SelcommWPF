using RestSharp;
using SelcommWPF.global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace SelcommWPF.dialogs.identification
{
    /// <summary>
    /// Interaction logic for QADialog.xaml
    /// </summary>
    public partial class QADialog : UserControl
    {
        private List<Dictionary<string, object>> QuestionList;

        public QADialog()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                QuestionList = Global.RequestAPI<List<Dictionary<string, object>>>(Constants.QuestionAnswerURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.1), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<string> list = new List<string>();
                    LoadingPanel.Visibility = Visibility.Hidden;

                    if (QuestionList == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    int index = 1;
                    foreach (Dictionary<string, object> item in QuestionList) list.Add(item["Question"].ToString());
                    foreach (Dictionary<string, object> item in QuestionList)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            ColumnDefinition column1 = new ColumnDefinition();
                            column1.Width = GridLength.Auto;
                            ColumnDefinition column2 = new ColumnDefinition();
                            column2.Width = GridLength.Auto;
                            ColumnDefinition column3 = new ColumnDefinition();
                            column3.Width = new GridLength(1, GridUnitType.Star);

                            Grid grid = new Grid();
                            grid.ColumnDefinitions.Add(column1);
                            grid.ColumnDefinitions.Add(column2);
                            grid.ColumnDefinitions.Add(column3);

                            MaterialDesignThemes.Wpf.PackIcon icon = new MaterialDesignThemes.Wpf.PackIcon();
                            icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Asterisk;
                            icon.Foreground = Brushes.Red;
                            icon.Width = 15;
                            icon.Height = 15;
                            icon.VerticalAlignment = VerticalAlignment.Center;
                            Grid.SetColumn(icon, 0);
                            grid.Children.Add(icon);

                            Label label = new Label();
                            label.Content = i == 0 ? "Question " + index : "Answer " + index;
                            label.FontSize = 16;
                            label.Margin = new Thickness(5, 0, 20, 0);
                            label.Width = 100;
                            Grid.SetColumn(label, 1);
                            grid.Children.Add(label);

                            if (i == 0)
                            {
                                ComboBox combo = new ComboBox();
                                combo.ItemsSource = list;
                                combo.SelectedIndex = 0;
                                combo.FontSize = 16;
                                MaterialDesignThemes.Wpf.HintAssist.SetHint(combo, "Select Question");
                                Grid.SetColumn(combo, 2);
                                grid.Children.Add(combo);
                            }
                            else
                            {
                                TextBox textBox = new TextBox();
                                textBox.FontSize = 16;
                                MaterialDesignThemes.Wpf.HintAssist.SetHint(textBox, "Enter Answer");
                                Grid.SetColumn(textBox, 2);
                                grid.Children.Add(textBox);
                            }

                            PanelQuestions.Children.Add(grid);
                        }
                        index++;
                    }
                });

            }, 10);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }
    }
}
