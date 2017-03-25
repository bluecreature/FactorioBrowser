using System;
using System.Windows;
using System.Windows.Controls;
using FactorioBrowser.UI.ViewModel;

namespace FactorioBrowser.UI {
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window {
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
         var container = new ComponentContainer(settings);
         var view = container.Get<ModSelectionWnd>();
         Grid.SetColumn(view, 0);
         Grid.SetRow(view, 0);
         view.VerticalAlignment = VerticalAlignment.Stretch;
         view.HorizontalAlignment = HorizontalAlignment.Stretch;
         Layout.Children.Add(view);
      }
   }
}
