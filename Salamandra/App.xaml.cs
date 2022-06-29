using Salamandra.Engine.Domain.Settings;
using Salamandra.Engine.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace Salamandra
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private LogManager InitializeAppLogManager()
        {
            LogManager logManager = new LogManager(String.Empty, "Salamandra", false);
            logManager.InitializeLog();

            return logManager;
        }

        private ApplicationSettings LoadSettingsFile(SettingsManager<ApplicationSettings> settingsManager, LogManager? logManager)
        {
            ApplicationSettings? settings = null;

            try
            {
                settings = settingsManager.LoadSettings();

                if (settings == null)
                    throw new Exception();
            }
            catch (Exception ex)
            {
                settings = new ApplicationSettings();

                logManager?.Fatal(String.Format("Error loading settings file. Resetting to default settings. ({0})", ex.Message),
                    "Settings");
            }

            return settings;
        }

        private static void SetCultureFromSystem(string cultureName)
        {
            var vCulture = new CultureInfo(cultureName);

            Thread.CurrentThread.CurrentCulture = vCulture;
            Thread.CurrentThread.CurrentUICulture = vCulture;
            CultureInfo.DefaultThreadCurrentCulture = vCulture;
            CultureInfo.DefaultThreadCurrentUICulture = vCulture;

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var appLogManager = InitializeAppLogManager();
            var settingsManager = new SettingsManager<ApplicationSettings>("application_settings.json");
            var settings = LoadSettingsFile(settingsManager, appLogManager);

            SetCultureFromSystem(settings.GeneralSettings.ViewLanguageToCultureName(settings.GeneralSettings.ViewLanguage));

            MainWindow mainWindow = new MainWindow(appLogManager, settingsManager, settings);
            mainWindow.Show();
        }
    }
}
