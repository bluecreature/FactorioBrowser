using System.Windows;

namespace FactorioBrowser.UI {

   public sealed partial class App : Application {

      public App() {
         QuickConverter.EquationTokenizer.AddNamespace(typeof(object));
         QuickConverter.EquationTokenizer.AddNamespace(typeof(System.Windows.Visibility));
      }
   }
}
