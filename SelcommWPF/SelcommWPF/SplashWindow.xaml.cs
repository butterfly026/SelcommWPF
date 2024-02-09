using Microsoft.Win32;
using RestSharp;
using SelcommWPF.clients;
using SelcommWPF.clients.models;
using SelcommWPF.global;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SelcommWPF
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {

        public SplashWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            Global.ScreenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            Global.ScreenHeigth = System.Windows.SystemParameters.PrimaryScreenHeight;
            
            this.Width = Global.ScreenWidth;
            this.Height = Global.ScreenHeigth - 30;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> path = new Dictionary<string, object>();
                path.Add("SiteId", Constants.SiteId);
                Dictionary<string, object> query = new Dictionary<string, object>();
                query.Add("api-version", 3.1);
                Global.AccessToken = Global.RequestAPI<TokenResponse>(Constants.AccessTokenURL, Method.Post, null, path, query, "");

                if (Global.AccessToken != null )
                {
                    if (Global.AccessToken.Type != null && Global.AccessToken.Credentials != null)
                    {
                        Dictionary<string, string> header = new Dictionary<string, string>();
                        header.Add("Authorization", Global.AccessToken.Type + " " + Global.AccessToken.Credentials);
                        query["api-version"] = 1.0;
                        Global.Sites = Global.RequestAPI<List<SiteResponse>>(Constants.SitesURL, Method.Get, header, null, query, "");
                        Global.CurrentSites = Global.RequestAPI<SiteResponse>(Constants.DefaultSiteURL, Method.Get, header, null, query, "");
                        DoNextPage();
                        return;
                    }
                }

                EasyTimer.SetTimeout(() => CloseApplication(), 15000);

            }, 1000);

            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SelcommWPF");
            Global.LanguageCode = key.GetValue("lang") == null ? "en-US" : key.GetValue("lang").ToString();
            key.Close();
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Global.LanguageCode);

        }

        private void DoNextPage()
        {
            var window = this;
            Application.Current.Dispatcher.Invoke(delegate
            {
                new LoginWindow().Show();
                window.Close();
            });
        }

        private void CloseApplication()
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                Application.Current.Shutdown();
            });
        }


    }
}
