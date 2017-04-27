using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.UI.ViewModel;

namespace FactorioBrowser.UI {
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window {

      private readonly AppSettings _settings;
      private readonly ComponentContainer _components;

      private ModSelectionView _modSelectionView = null;
      private BrowseView _browseView = null;

      public MainWindow() {
         InitializeComponent();
         _settings = new AppSettings();
         _components = new ComponentContainer(_settings);
      }

      private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
         if (_settings.UseSavedSettings) { // TODO : validate existing values
            ShowModSelectionView();
         } else {
            AskForInitialConfiguration();
         }
      }

      private void LoadModListConfirmed(IEnumerable<FcModFileInfo> selectedMods) {
         ModSelectionView view;
         if ((view = Interlocked.Exchange(ref _modSelectionView, null)) != null) {
            view.SelectionConfirmed -= LoadModListConfirmed;
            Layout.Children.Remove(view);
            ShowBrowseView(selectedMods);
         }
      }

      private void ShowBrowseView(IEnumerable<FcModFileInfo> selectedMods) {
         Debug.Assert(_browseView == null);
         _browseView = _components.Get<IBrowseViewFactory>().Create(selectedMods);
         SwitchTo(_browseView);
      }

      private void AskForInitialConfiguration() {
         Window configWnd = new InitialConfigWnd(new InitialConfigViewModel(_settings));
         if (configWnd.ShowDialog() == true) {
            ShowModSelectionView();
         } else {
            Close();
         }
      }

      private void ShowModSelectionView() {
         Debug.Assert(_modSelectionView == null);

         _modSelectionView = _components.Get<ModSelectionView>();
         _modSelectionView.SelectionConfirmed += LoadModListConfirmed;
         SwitchTo(_modSelectionView);
#pragma warning disable 4014
         _modSelectionView.Refresh();
#pragma warning restore 4014
      }

      private void SwitchTo(FrameworkElement ui) {
         Debug.Assert(ui != null);

         ui.VerticalAlignment = VerticalAlignment.Stretch;
         ui.HorizontalAlignment = HorizontalAlignment.Stretch;
         Grid.SetColumn(ui, 0);
         Grid.SetRow(ui, 0);
         Layout.Children.Add(ui);
      }
   }
}
