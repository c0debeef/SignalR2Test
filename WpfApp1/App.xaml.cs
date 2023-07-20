using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly string Username = Guid.NewGuid().ToString("B");
        protected /*async*/ override void OnStartup(StartupEventArgs args)
        {
            try
            {
                SignalRManager.Start(Username);
                //await SignalRManager.Start(Username);
            }
            catch (Exception exception)
            {
                ;
            }

            base.OnStartup(args);
        }
        protected async override void OnExit(ExitEventArgs args)
        {
            try
            {
                MessageBox.Show("Before crash", "Step #1", MessageBoxButton.OK, MessageBoxImage.Question);

                await SignalRManager.Stop();

                MessageBox.Show("After crash", "Step #2", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (Exception exception)
            {
                ;
            }

            base.OnExit(args);
        }
    }
}
