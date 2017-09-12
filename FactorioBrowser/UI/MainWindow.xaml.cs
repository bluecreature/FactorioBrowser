using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.UI.ViewModel;

namespace FactorioBrowser.UI {
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow {

      private readonly AppSettings _settings;
      private readonly ComponentContainer _components;

      private ModSelectionView _modSelectionView;
      private SettingsView _settingsView;
      private BrowseView _browseView;

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
            ShowSettingsView(selectedMods);
         }
      }

      private void ShowSettingsView(IEnumerable<FcModFileInfo> selectedMods) {
         Debug.Assert(_settingsView == null);
         _settingsView = _components.Get<ISettingsViewFactory>().Create(selectedMods);
         SwitchTo(_settingsView);
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
