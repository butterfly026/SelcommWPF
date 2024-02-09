using SelcommWPF.clients.models.messages;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace SelcommWPF.dialogs.order
{
    /// <summary>
    /// Interaction logic for OrderDialog.xaml
    /// </summary>
    public partial class OrderDialog : UserControl
    {

        private IDisposable RequestTimer = null;
        private bool SelectedAddressStatus = false;
        private string CurrentProduct = "";
        private string CurrentPlan = "";
        private string CurrentHardware = "";
        private string CheckString = "";

        public OrderDialog()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            DatePickerInstallation.SelectedDate = DateTime.Now;
            ShowHideVerticalScroll(Global.HasVerticalScroll);
        }

        private void ShowHideVerticalScroll(bool isChecked)
        {
            ScrollViewer[] scrolls = new ScrollViewer[] { ScrollViewPlan };
            int count = scrolls.Length;

            for (int i = 0; i < count; i++) if (scrolls[i] == null) return;
            for (int i = 0; i < count; i++) scrolls[i].VerticalScrollBarVisibility = isChecked ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void TextBoxAddress_KeyDown(object sender, KeyEventArgs e)
        {
            string search = TextAddress.Text;
            if (search == "" || RequestTimer != null || SelectedAddressStatus)
            {
                ListAddress.ItemsSource = new List<string>();
                CardAddress.Visibility = Visibility.Collapsed;
                return;
            }

            if (e.Key == Key.Down)
            {
                ListAddress.Focus();
                ListAddress.SelectedIndex = 0;
                return;
            }

            RequestTimer = EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    search = TextAddress.Text;
                    ProgressAddress.Visibility = Visibility.Visible;
                    CardAddress.Visibility = Visibility.Collapsed;
                });

                if (Global.contactClient == null) Global.contactClient = new clients.ContactClient();
                List<Dictionary<string, string>> result = Global.contactClient.AutoCompleteAustralia(Constants.AutoCompleteURL, search);
                List<string> addresses = new List<string>();
                if (result != null) foreach (Dictionary<string, string> item in result) addresses.Add(item["Address"]);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    ProgressAddress.Visibility = Visibility.Collapsed;
                    ListAddress.ItemsSource = new List<string>();
                    ListAddress.ItemsSource = addresses;
                    CardAddress.Visibility = addresses.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                    RequestTimer.Dispose();
                    RequestTimer = null;
                });

            }, 2000);
        }

        private void TextBoxAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            SelectedAddressStatus = false;
        }

        private void ListViewItem_MouseUpClick(object sender, MouseButtonEventArgs e)
        {
            if (ListAddress.SelectedItem == null) return;
            TextAddress.Text = ListAddress.SelectedItem.ToString();
            ListAddress.ItemsSource = new List<string>();
            CardAddress.Visibility = Visibility.Collapsed;
            ShowProductList();
        }

        private void ListViewItem_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ListAddress.SelectedItem == null) return;
                TextAddress.Text = ListAddress.SelectedItem.ToString();
                ListAddress.ItemsSource = new List<string>();
                CardAddress.Visibility = Visibility.Collapsed;
                ShowProductList();
            }
        }

        private void ShowProductList()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    TabAccountComponents.SelectedIndex = 1;
                    ProductPanel.Visibility = Visibility.Visible;
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 1000);
        }

        private void CardProduct_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int count = ProductPanel.Children.Count;
            MaterialDesignThemes.Wpf.Card card = (MaterialDesignThemes.Wpf.Card)sender;

            for (int i = 0; i < count; i++)
            {
                MaterialDesignThemes.Wpf.Card card1 = (MaterialDesignThemes.Wpf.Card)ProductPanel.Children[i];
                MaterialDesignThemes.Wpf.PackIcon icon = (MaterialDesignThemes.Wpf.PackIcon)((Grid)card1.Content).Children[0];
                icon.Visibility = card == card1 ? Visibility.Visible : Visibility.Collapsed;
            }

            List<string> list = new List<string>() { "Home FTTB/FTTP", "Fixed Wirless", "VOIP" };
            List<WrapPanel> panels = new List<WrapPanel>() { PlanPanelFTTP, PlanPanelWireless, PlanPanelVOIP };

            string tag = ((MaterialDesignThemes.Wpf.Card)sender).Tag.ToString();
            CurrentProduct = tag;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    TabAccountComponents.SelectedIndex = 2;
                    for (int i = 0; i < 3; i++) panels[i].Visibility = i == list.IndexOf(tag) ? Visibility.Visible : Visibility.Collapsed;
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 500);
        }

        private void CardPlan_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MaterialDesignThemes.Wpf.Card card = (MaterialDesignThemes.Wpf.Card)sender;
            Label label = ((Label)((Grid)((StackPanel)card.Content).Children[0]).Children[0]);
            CurrentPlan = label.Content.ToString();

            WrapPanel panel = (WrapPanel)card.Parent;
            int count = panel.Children.Count;
            for (int i = 0; i < count; i++)
            {
                MaterialDesignThemes.Wpf.Card card1 = (MaterialDesignThemes.Wpf.Card)panel.Children[i];
                MaterialDesignThemes.Wpf.PackIcon icon = ((MaterialDesignThemes.Wpf.PackIcon)((Grid)((StackPanel)card1.Content).Children[0]).Children[1]);
                icon.Visibility = card == card1 ? Visibility.Visible : Visibility.Collapsed;
            }

            
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (CurrentProduct == "Fixed Wirless" || CurrentProduct == "VOIP")
                    {
                        MessageUtils.ShowMessage("", "There is no hardware.");
                        CurrentHardware = "No Hardware";
                        TabAccountComponents.SelectedIndex = 5;
                        PanelSchedule.Visibility = Visibility.Visible;
                        TextDeliveryAddress.Text = TextAddress.Text;
                        LoadingPanel.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        TabAccountComponents.SelectedIndex = 3;
                        PanelHardware.Visibility = Visibility.Visible;
                    }
                });

            }, 500);
        }

        private void CardHardware_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MaterialDesignThemes.Wpf.Card card = (MaterialDesignThemes.Wpf.Card)sender;
            Label label = ((Label)((Grid)((StackPanel)card.Content).Children[0]).Children[0]);
            CurrentHardware = label.Content.ToString();

            WrapPanel panel = (WrapPanel)card.Parent;
            int count = panel.Children.Count;

            for (int i = 0; i < count; i++)
            {
                MaterialDesignThemes.Wpf.Card card1 = (MaterialDesignThemes.Wpf.Card)panel.Children[i];
                MaterialDesignThemes.Wpf.PackIcon icon = ((MaterialDesignThemes.Wpf.PackIcon)((Grid)((StackPanel)card1.Content).Children[0]).Children[1]);
                icon.Visibility = card == card1 ? Visibility.Visible : Visibility.Collapsed;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    TabAccountComponents.SelectedIndex = 5;
                    PanelSchedule.Visibility = Visibility.Visible;
                    TextDeliveryAddress.Text = TextAddress.Text;
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 500);
        }

        private void TextBoxDeliveryAddress_KeyDown(object sender, KeyEventArgs e)
        {
            string search = TextDeliveryAddress.Text;
            if (search == "" || RequestTimer != null || SelectedAddressStatus)
            {
                ListDeliveryAddress.ItemsSource = new List<string>();
                CardDeliveryAddress.Visibility = Visibility.Collapsed;
                return;
            }

            if (e.Key == Key.Down)
            {
                ListDeliveryAddress.Focus();
                ListDeliveryAddress.SelectedIndex = 0;
                return;
            }

            RequestTimer = EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    search = TextDeliveryAddress.Text;
                    ProgressDeliveryAddress.Visibility = Visibility.Visible;
                    CardDeliveryAddress.Visibility = Visibility.Collapsed;
                });

                if (Global.contactClient == null) Global.contactClient = new clients.ContactClient();
                List<Dictionary<string, string>> result = Global.contactClient.AutoCompleteAustralia(Constants.AutoCompleteURL, search);
                List<string> addresses = new List<string>();
                if (result != null) foreach (Dictionary<string, string> item in result) addresses.Add(item["Address"]);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    ProgressDeliveryAddress.Visibility = Visibility.Collapsed;
                    ListDeliveryAddress.ItemsSource = new List<string>();
                    ListDeliveryAddress.ItemsSource = addresses;
                    CardDeliveryAddress.Visibility = addresses.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                    RequestTimer.Dispose();
                    RequestTimer = null;
                });

            }, 2000);
        }

        private void TextBoxDeliveryAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            SelectedAddressStatus = false;
        }

        private void ListViewItemDelivery_MouseUpClick(object sender, MouseButtonEventArgs e)
        {
            if (ListDeliveryAddress.SelectedItem == null) return;
            TextDeliveryAddress.Text = ListDeliveryAddress.SelectedItem.ToString();
            ListDeliveryAddress.ItemsSource = new List<string>();
            CardDeliveryAddress.Visibility = Visibility.Collapsed;
        }

        private void ListViewItemDelivery_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ListDeliveryAddress.SelectedItem == null) return;
                TextAddress.Text = ListAddress.SelectedItem.ToString();
                ListAddress.ItemsSource = new List<string>();
                CardDeliveryAddress.Visibility = Visibility.Collapsed;
            }
        }

        private void CheckValidPhone(string param)
        {
            EasyTimer.SetTimeout(() =>
            {
                ValidModel result = Global.messageClient.ValidatePhoneOrEmail(Constants.ValidatePhone, param);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result.Valid) CheckUniqueAuthInfo(param);
                    else
                    {
                        LoadingPanel.Visibility = Visibility.Hidden;
                        MessageUtils.ShowErrorMessage("", string.Format(Properties.Resources.Invalid_Message, Properties.Resources.Contact_Phone));
                    }
                });

            }, 10);
        }

        private void CheckUniqueAuthInfo(string param)
        {
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, bool> result = Global.userClient.GetAuthenticateUnique(Constants.CheckMobileURL, param);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result == null || result["Used"])
                    {
                        LoadingPanel.Visibility = Visibility.Hidden;
                        MessageUtils.ShowMessage("", string.Format(Properties.Resources.Choose_Another_Message, Properties.Resources.Contact_Phone));
                    }
                    else AddToCart();
                });

            }, 10);
        }

        private void ButtonAddCart_Click(object sender, RoutedEventArgs e)
        {
            LoadingPanel.Visibility = Visibility.Visible;

            /*string param = TextContactPhone.Text;
            if (param != "") CheckValidPhone(param);
            else AddToCart();*/
            AddToCart();
        }

        private void AddToCart()
        {
            OrderModel order = new OrderModel();
            order.Address = TextDeliveryAddress.Text;
            order.Plan = CurrentPlan;
            order.Product = CurrentProduct;
            order.Hardware = CurrentHardware;
            order.InstallationDate = DatePickerInstallation.SelectedDate.Value.ToString("MM/d/yyyy");
            order.ContactPhone = TextContactPhone.Text;
            order.Note = TextNote.Text;

            EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<OrderModel> list = (List<OrderModel>)ListOrders.ItemsSource;
                    if (list == null || list.Count == 0)
                    {
                        list = new List<OrderModel>();
                        long number = new Random().Next(100000, 999999);
                        LabelNumber.Content = string.Format("Order Number : {0}", number);
                    }

                    order.No = list.Count + 1;
                    list.Add(order);

                    ListOrders.ItemsSource = new List<OrderModel>();
                    ListOrders.ItemsSource = list;
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 500);
        }

        private void ButtonRemoveCart_Click(object sender, RoutedEventArgs e)
        {
            int index = Convert.ToInt32(((Button)sender).Tag);
            List<OrderModel> list = (List<OrderModel>)ListOrders.ItemsSource;

            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                if (index == list[i].No)
                {
                    list.RemoveAt(i);
                    break;
                }
            }

            ListOrders.ItemsSource = new List<OrderModel>();
            ListOrders.ItemsSource = list;
        }

        public class OrderModel
        {
            public int No { get; set; }

            public string Address { get; set; }

            public string Plan { get; set; }

            public string Product { get; set; }

            public string Hardware { get; set; }

            public string InstallationDate { get; set; }

            public string ContactPhone { get; set; }

            public string Note { get; set; }
        }

        private void MenuItemSQ_Checked(object sender, RoutedEventArgs e)
        {
            if (TabAccountComponents == null || PanelHardwareMenu == null) return;
            TabAccountComponents.Visibility = Visibility.Visible;
            PanelHardwareMenu.Visibility = Visibility.Collapsed;
        }

        private void MenuItemHardWare_Checked(object sender, RoutedEventArgs e)
        {
            if (TabAccountComponents == null || PanelHardwareMenu == null) return;
            TabAccountComponents.Visibility = Visibility.Collapsed;
            PanelHardwareMenu.Visibility = Visibility.Visible;
        }

        private void TextBoxHardware_KeyDown(object sender, KeyEventArgs e)
        {
            string search = TextHardwareName.Text;
            if (search == "" || RequestTimer != null || SelectedAddressStatus)
            {
                ListHardware.ItemsSource = new List<string>();
                CardHardware.Visibility = Visibility.Collapsed;
                return;
            }

            if (e.Key == Key.Down)
            {
                ListHardware.Focus();
                ListHardware.SelectedIndex = 0;
                return;
            }

            RequestTimer = EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    search = TextHardwareName.Text;
                    ProgressHardware.Visibility = Visibility.Visible;
                    CardHardware.Visibility = Visibility.Collapsed;
                });

                EasyTimer.SetTimeout(() =>
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        ProgressHardware.Visibility = Visibility.Collapsed;
                        ListHardware.ItemsSource = new List<string>();
                        ListHardware.ItemsSource = new string[] { "Hardware1", "Hardware2", "Hardware3", "Hardware4", "Hardware5"};
                        CardHardware.Visibility = Visibility.Visible;
                        RequestTimer.Dispose();
                        RequestTimer = null;
                    });

                }, 2000);

            }, 1000);
        }

        private void TextBoxHardware_LostFocus(object sender, RoutedEventArgs e)
        {
            SelectedAddressStatus = false;
        }

        private void ListViewItemHardware_MouseUpClick(object sender, MouseButtonEventArgs e)
        {
            if (ListHardware.SelectedItem == null) return;
            TextHardwareName.Text = ListHardware.SelectedItem.ToString();
            ListHardware.ItemsSource = new List<string>();
            CardHardware.Visibility = Visibility.Collapsed;
            TextHardWareCount.Visibility = Visibility.Visible;
            TextHardWarePrice.Visibility = Visibility.Visible;
            TextHardWarePrice.Text = "0";
            ButtonAddCart.Visibility = Visibility.Visible;
        }

        private void ButtonRemoveCartHardware_Click(object sender, RoutedEventArgs e)
        {
            int index = Convert.ToInt32(((Button)sender).Tag);
            List<OrderModel> list = (List<OrderModel>)ListOrdersHardware.ItemsSource;

            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                if (index == list[i].No)
                {
                    list.RemoveAt(i);
                    break;
                }
            }

            ListOrdersHardware.ItemsSource = new List<OrderModel>();
            ListOrdersHardware.ItemsSource = list;
        }

        private void ButtonAddCartHardware_Click(object sender, RoutedEventArgs e)
        {
            OrderModel order = new OrderModel();
            order.Plan = TextHardwareName.Text;
            order.Product = TextHardWareCount.Text;
            order.Hardware = TextHardWarePrice.Text;

            if (order.Plan == "")
            {
                TextHardwareName.Focus();
                return;
            }

            if (order.Product == "")
            {
                TextHardWareCount.Focus();
                return;
            }

            if (order.Hardware == "")
            {
                TextHardWarePrice.Focus();
                return;
            }

            order.InstallationDate = StringUtils.ConvertCurrency(Convert.ToInt32(order.Product) * Convert.ToDouble(order.Hardware.Replace("A$", "").Replace(",", "")));
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    List<OrderModel> list = (List<OrderModel>)ListOrdersHardware.ItemsSource;
                    if (list == null || list.Count == 0)
                    {
                        list = new List<OrderModel>();
                        long number = new Random().Next(100000, 999999);
                        LabelNumber.Content = string.Format("Order Number : {0}", number);
                    }

                    order.No = list.Count + 1;
                    list.Add(order);

                    ListOrdersHardware.ItemsSource = new List<OrderModel>();
                    ListOrdersHardware.ItemsSource = list;
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 500);
        }


        private void TextAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void TextAmount_TextChanged(object sender, TextChangedEventArgs e)
        {

            TextBox textBox = (TextBox)sender;
            string str = textBox.Text;

            if (str == CheckString)
            {
                textBox.SelectionStart = str.Length;
                CheckString = "";
                return;
            }

            if (str.Length > 19)
            {
                textBox.Text = str.Substring(0, 19);
                textBox.SelectionStart = str.Length;
                CheckString = textBox.Text;
                return;
            }

            if (str.Contains("A$"))
            {
                TextChange change = e.Changes.First();
                str = str.Replace("A$", "").Replace(",", "");
                if (change.AddedLength == 0 && change.RemovedLength > 0)
                {

                    double price = Convert.ToDouble(str) / 10;
                    str = StringUtils.ConvertCurrency(price);
                }
                else
                {

                    double price = Convert.ToDouble(str) * 10;
                    str = StringUtils.ConvertCurrency(price);
                }
            }
            else
            {
                str = StringUtils.ConvertCurrency(Convert.ToDouble(str) / 100);
            }

            CheckString = str;
            textBox.Text = str;
        }



    }
}
