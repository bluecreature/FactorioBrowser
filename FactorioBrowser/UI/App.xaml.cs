using System.Windows;

namespace FactorioBrowser.UI {

   public partial class App : Application {

      private void FactorioBrowserAppStartup(object sender, StartupEventArgs args) {
         Window mainWnd = new InitialConfigWnd();
         mainWnd.Show();
      }
   }
}
