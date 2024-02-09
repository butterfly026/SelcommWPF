using SelcommWPF.clients.models.services;
using SelcommWPF.global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for AttributeDialog.xaml
    /// </summary>
    public partial class AttributeDialog : UserControl
    {

        private long AttributeId = 0;
        private string ServiceReference;
        private List<Dictionary<string, object>> ListDefinitionIds;

        public AttributeDialog(string reference, long id = 0)
        {
            ServiceReference = reference;
            AttributeId = id;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LabelDialogTitle.Content = AttributeId == 0 ? Properties.Resources.Add_Attributes : Properties.Resources.Edit_Attributes;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> serviceBasicData = Global.servicesClient.GetServiceTypeBasic(Constants.ServicesBasicURL, ServiceReference);
                ListDefinitionIds = Global.servicesClient.GetServicesDefinitionId(Constants.ServicesDefinitionsURL, serviceBasicData["ServiceTypeId"].ToString());
                List<AttributeModel> attributesList = Global.servicesClient.GetServiceAttributesData(Constants.ServicesAttributesURL, ServiceReference);

                AttributeModel result = null;
                if (AttributeId != 0) result = Global.servicesClient.GetServiceAttributeDetail(Constants.ServicesAttributeDetailURL, AttributeId);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (serviceBasicData == null || ListDefinitionIds == null || attributesList == null)
                    {
                        Global.CloseDialog("DetailDialog");
                        return;
                    }

                    List<long> ids = new List<long>();
                    List<string> list = new List<string>();

                    int count = ListDefinitionIds.Count;
                    int count1 = attributesList.Count;

                    for (int i = 0; i <count; i++)
                    {
                        bool isContains = false;
                        long id = Convert.ToInt64(ListDefinitionIds[i]["Id"].ToString());

                        for (int j = 0; j < count1; j++)
                        {
                            if (attributesList[j].DefinitionId == id)
                            {
                                isContains = true;
                                break;
                            }
                        }

                        if (isContains)
                        {
                            ListDefinitionIds.RemoveAt(i);
                            count--;
                            i--;
                        }
                        else
                        {
                            list.Add(ListDefinitionIds[i]["Name"].ToString());
                            ids.Add(id);
                        }
                    }

                    ComboDefinitionId.ItemsSource = list;

                    if (AttributeId == 0)
                    {
                        DatePickerFrom.SelectedDate = DateTime.Now;
                        DatePickerTo.SelectedDate = DateTime.Parse("9999-01-01 00:00:00");
                    }
                    else
                    {
                        ComboDefinitionId.SelectedIndex = ids.IndexOf(result.DefinitionId);
                        DatePickerFrom.SelectedDate = DateTime.Parse(result.From);
                        DatePickerTo.SelectedDate = DateTime.Parse(result.To);
                        EasyTimer.SetTimeout(() =>
                        {
                            Application.Current.Dispatcher.Invoke(delegate
                            {
                                Dictionary<string, object> DefinitionData = ListDefinitionIds[ComboDefinitionId.SelectedIndex];
                                string dataType = DefinitionData["DataType"].ToString();
                                bool required = Convert.ToBoolean(DefinitionData["Required"].ToString());

                                switch (dataType)
                                {
                                    case "Boolean":
                                        ToggleButton toggle = (ToggleButton)GridContent.Children[3];
                                        toggle.IsChecked = (bool)result.Value;
                                        break;
                                    case "DateTime":
                                        DatePicker datePicker = (DatePicker)GridContent.Children[3];
                                        datePicker.SelectedDate = DateTime.Parse(result.Value.ToString());
                                        break;
                                    default:
                                        TextBox textBox = (TextBox)GridContent.Children[3];
                                        textBox.Text = result.Value.ToString();
                                        break;
                                }
                            });
                        }, 100);
                    }
                   
                });

            }, 10);

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }

        private void TextDefinitionId_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void TextDefinitionId_TextChanged(object sender, TextChangedEventArgs e)
        {
            ButtonSave.IsEnabled = ComboDefinitionId.SelectedItem != null && DatePickerFrom.SelectedDate != null && DatePickerTo.SelectedDate != null;
        }

        private void DatePickerFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonSave.IsEnabled = ComboDefinitionId.SelectedItem != null && DatePickerFrom.SelectedDate != null && DatePickerTo.SelectedDate != null;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("DefinitionId", Convert.ToInt64(ListDefinitionIds[ComboDefinitionId.SelectedIndex]["Id"].ToString()));
            body.Add("From", DatePickerFrom.SelectedDate.Value.ToString("yyyy-MM-ddThh:mm:ss"));
            body.Add("To", DatePickerTo.SelectedDate.Value.ToString("yyyy-MM-ddThh:mm:ss"));

            Dictionary<string, object> DefinitionData = ListDefinitionIds[ComboDefinitionId.SelectedIndex];
            string dataType = DefinitionData["DataType"].ToString();
            bool required = Convert.ToBoolean(DefinitionData["Required"].ToString());

            switch (dataType)
            {
                case "Boolean":
                    ToggleButton toggle = (ToggleButton)GridContent.Children[3];
                    body.Add("Value", toggle.IsChecked.Value);
                    break;
                case "DateTime":
                    DatePicker datePicker = (DatePicker)GridContent.Children[3];
                    body.Add("Value", datePicker.SelectedDate.Value.ToString("yyyy-MM-ddThh:mm:ss"));
                    break;
                default:
                    TextBox textBox = (TextBox)GridContent.Children[3];
                    body.Add("Value", textBox.Text);
                    break;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result;
                if (AttributeId == 0) result = Global.servicesClient.CreateServiceAttribute(Constants.ServicesAttributesURL, ServiceReference, body);
                else result = Global.servicesClient.UpdateServiceAttribute(Constants.ServicesAttributeDetailURL, AttributeId, body);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK || result == HttpStatusCode.Created)
                    {
                        Global.CloseDialog("DetailDialog");
                        ServiceAttributes.Instance.InitializeControl();
                    }
                });

            }, 10);

        }

        private void ComboDefinitionId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dictionary<string, object> DefinitionData = ListDefinitionIds[ComboDefinitionId.SelectedIndex];
            string dataType = DefinitionData["DataType"].ToString();
            bool required = Convert.ToBoolean(DefinitionData["Required"].ToString());
            string defaultValue = DefinitionData["DefaultValue"] == null ? "" : DefinitionData["DefaultValue"].ToString();

            if (GridContent.Children.Count == 4) GridContent.Children.RemoveAt(3);
            ButtonSave.IsEnabled = ComboDefinitionId.SelectedItem != null && DatePickerFrom.SelectedDate != null && DatePickerTo.SelectedDate != null;

            switch (dataType)
            {
                case "Boolean":
                    ToggleButton toggle = new ToggleButton();
                    toggle.IsChecked = defaultValue == "" ? false : Convert.ToBoolean(defaultValue);
                    Grid.SetColumn(toggle, 1);
                    GridContent.Children.Add(toggle);
                    break;
                case "DateTime":
                    DatePicker datePicker = new DatePicker();
                    MaterialDesignThemes.Wpf.HintAssist.SetHint(datePicker, required ? Properties.Resources.Value1 : Properties.Resources.Value);
                    MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(datePicker, true);
                    datePicker.SelectedDate = defaultValue == "" ? DateTime.Now : DateTime.Parse(defaultValue);
                    datePicker.Margin = new Thickness(10);
                    datePicker.FontSize = 16;
                    Grid.SetColumn(datePicker, 1);
                    GridContent.Children.Add(datePicker);
                    break;
                default:
                    TextBox textBox = new TextBox();
                    textBox.Text = defaultValue;
                    MaterialDesignThemes.Wpf.HintAssist.SetHint(textBox, required ? Properties.Resources.Value1 : Properties.Resources.Value);
                    MaterialDesignThemes.Wpf.HintAssist.SetIsFloating(textBox, true);
                    textBox.Margin = new Thickness(10);
                    textBox.FontSize = 16;
                    if (dataType == "Integer") textBox.PreviewTextInput += TextInteger_PreviewTextInput;
                    Grid.SetColumn(textBox, 1);
                    GridContent.Children.Add(textBox);
                    break;
            }

        }

        private void TextInteger_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

    }
}
