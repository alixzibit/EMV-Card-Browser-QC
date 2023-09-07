using System;
using System.Linq;
using System.Windows;

namespace EMV_Card_Browser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            // Check if readCardOnStart argument is present
            if (e.Args.Contains("readCardOnStart"))
            {
                mainWindow.ReadCard();
            }
        }
    }
}
