using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using FactorioBrowser.UI.ViewModel;

namespace FactorioBrowser.UI {
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window {

      private ModSelectionView _modSelectionView = null;

      public MainWindow() {
         InitializeComponent();
      }

      private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
         AppSettings settings = new AppSettings();
         if (settings.UseSavedSettings) { // TODO : validate existing values
            ShowModSelectionView(settings);
         } else {
            AskForInitialConfiguration(settings);
         }
      }

      private void AskForInitialConfiguration(AppSettings settings) {
         Window configWnd = new InitialConfigWnd(new InitialConfigViewModel(settings));
         if (configWnd.ShowDialog() == true) {
            ShowModSelectionView(settings);
         } else {
            Close();
         }
      }

      private void ShowModSelectionView(AppSettings settings) {
         Debug.Assert(_modSelectionView == null);

         var container = new ComponentContainer(settings);
         _modSelectionView = container.Get<ModSelectionView>();
         _modSelectionView.VerticalAlignment = VerticalAlignment.Stretch;
         _modSelectionView.HorizontalAlignment = HorizontalAlignment.Stretch;
         Grid.SetColumn(_modSelectionView, 0);
         Grid.SetRow(_modSelectionView, 0);

         Layout.Children.Add(_modSelectionView);

#pragma warning disable 4014
         _modSelectionView.Refresh();
#pragma warning restore 4014
      }
   }
}
