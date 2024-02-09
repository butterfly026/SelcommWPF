using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using RestSharp;
using SelcommWPF.clients.models;
using System;
using System.Net;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;
using SelcommWPF.dialogs;
using System.Threading.Tasks;
using SelcommWPF.global;
using Constants = SelcommWPF.global.Constants;

namespace SelcommWPF.utils
{
    class MessageUtils
    {

        public static void ShowMessage(string title, string message, int offsetX = 10, int offsetY = 10)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                Window parent = Application.Current.MainWindow;
                if (parent == null) return;

                Notifier notifier = new Notifier(cfg =>
                {
                    cfg.PositionProvider = new WindowPositionProvider(
                        parentWindow: parent,
                        corner: Corner.BottomRight,
                        offsetX: offsetX,
                        offsetY: offsetY);

                    cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                        notificationLifetime: TimeSpan.FromSeconds(3),
                        maximumNotificationCount: MaximumNotificationCount.FromCount(2));

                    cfg.Dispatcher = Application.Current.Dispatcher;
                });

                notifier.ShowInformation(message);

            });
        }

        public static void ShowErrorMessage(string title, string message, int offsetX = 10, int offsetY = 10)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                Window parent = Application.Current.MainWindow;
                if (parent == null) return;

                Notifier notifier = new Notifier(cfg =>
                {
                    cfg.PositionProvider = new WindowPositionProvider(
                        parentWindow: parent,
                        corner: Corner.BottomRight,
                        offsetX: offsetX,
                        offsetY: offsetY);
                    cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                        notificationLifetime: TimeSpan.FromSeconds(3),
                        maximumNotificationCount: MaximumNotificationCount.FromCount(2)
                    );
                    cfg.Dispatcher = Application.Current.Dispatcher;
                });

                notifier.ShowError(message);
            });
        }

        public static async Task<bool> ConfirmMessageAsync(string title, string message, int type = 0)
        {
            //show the dialog
            var result = await DialogHost.Show(new ConfirmDialog(message, type), "ConfirmDialog");
            return (bool)result;
        }

        public static void ParseResponse(RestResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created) 
            {
                if (response.StatusCode.ToString() == "0")
                {
                    ShowErrorMessage(Properties.Resources.Error, Properties.Resources.No_Internet);
                } 
                else
                {
                    try
                    {
                        ErrorResponse content = JsonConvert.DeserializeObject<ErrorResponse>(response.Content);
                        if (content == null || content.Errors == null)
                        {
                            ErrorResponse.ErrorBody errorBody = JsonConvert.DeserializeObject<ErrorResponse.ErrorBody>(response.Content);
                            ShowErrorMessage(Properties.Resources.Error,
                                errorBody == null || errorBody.ErrorMessage == null ? response.StatusCode.ToString() : errorBody.ErrorMessage);
                        }
                        else
                        {
                            ShowErrorMessage(Properties.Resources.Error, content.Errors.ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Unknown_Error);
                    }
                }
            }

            if (Constants.DEBUG_MODE) Global.WriteLogFile(response);
        }

    }
}
