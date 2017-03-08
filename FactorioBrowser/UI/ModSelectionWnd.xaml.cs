using System.Windows;
using FactorioBrowser.UI.ViewModel;

namespace FactorioBrowser.UI {

   /// <summary>
   /// Interaction logic for ModSelectionWnd.xaml
   /// </summary>
   public partial class ModSelectionWnd : Window {
      private readonly ModSelectionViewModel _viewModel;

      public ModSelectionWnd(ModSelectionViewModel viewModel) {
         _viewModel = viewModel;
         InitializeComponent();
         DataContext = _viewModel;
      }

      private void RefreshModList(object sender, RoutedEventArgs e) {
         _viewModel.RefreshModList();
      }
   }
}
