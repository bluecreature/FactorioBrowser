using System.Windows;
using FactorioBrowser.UI.ViewModel;

namespace FactorioBrowser.UI {

   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow {

      private readonly ComponentContainer _components;

      public MainWindow() {
         InitializeComponent();
         _components = new ComponentContainer();
         DataContext = _components.Get<MainWindowViewModel>();
      }

      private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
         await ((MainWindowViewModel) DataContext).ConfigureAndStart();
      }
   }
}
