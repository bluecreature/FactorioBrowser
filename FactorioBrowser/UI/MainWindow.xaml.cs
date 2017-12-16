using System.Collections.Immutable;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using FactorioBrowser.Mod.Loader;

namespace FactorioBrowser.UI {

   public abstract class BrowserState {
      public delegate void SwitchEventHandler(BrowserState nextState);

      public event SwitchEventHandler SwitchState;

      public abstract FrameworkElement View { get; }

      protected void InvokeSwitchState(BrowserState nextState) {
         SwitchState?.Invoke(nextState);
      }
   }

   public sealed class InitialConfigState : BrowserState {
      private readonly IViewsFactory _viewsFactory;
      private readonly InitialConfigView _view;

      public InitialConfigState(IViewsFactory viewsFactory) {
         _viewsFactory = viewsFactory;
         _view = viewsFactory.CreateInitialConfigView();
         _view.ConfigurationConfirmed += InitialConfigConfirmed;
      }

      private void InitialConfigConfirmed() {
         _view.ConfigurationConfirmed -= InitialConfigConfirmed;
         InvokeSwitchState(new ModSelectionState(_viewsFactory));
      }

      public override FrameworkElement View => _view;
   }

   public sealed class ModSelectionState : BrowserState {
      private readonly ModSelectionView _view;
      private readonly IViewsFactory _viewsFactory;

      public ModSelectionState(IViewsFactory viewsFactory) {
         _viewsFactory = viewsFactory;
         _view = _viewsFactory.CreateModSelectionView();
         _view.SelectionConfirmed += ModSelectionConfirmed;
      }

      public override FrameworkElement View => _view;

      private void ModSelectionConfirmed(IImmutableList<FcModFileInfo> selectedMods) {
         _view.SelectionConfirmed -= ModSelectionConfirmed;
         InvokeSwitchState(new ModSettingsState(_viewsFactory, selectedMods));
      }
   }

   public sealed class ModSettingsState : BrowserState {
      private readonly IImmutableList<FcModFileInfo> _selectedMods;
      private readonly SettingsView _view;
      private readonly IViewsFactory _viewsFactory;

      public ModSettingsState(IViewsFactory viewsFactory,
         IImmutableList<FcModFileInfo> selectedMods) {

         _viewsFactory = viewsFactory;
         _selectedMods = selectedMods;
         _view = viewsFactory.CreateSettingsView(_selectedMods);
         _view.SelectionConfirmed += ModSettingsConfirmed;
      }

      private void ModSettingsConfirmed(IImmutableDictionary<string, object> settings) {

         _view.SelectionConfirmed -= ModSettingsConfirmed;
         InvokeSwitchState(new BrowseState(_viewsFactory, _selectedMods, settings));
      }

      public override FrameworkElement View => _view;
   }

   public sealed class BrowseState : BrowserState {

      public BrowseState(IViewsFactory viewsFactory, IImmutableList<FcModFileInfo> selectedMods,
         IImmutableDictionary<string, object> settings) {

         View = viewsFactory.CreateBrowseView(selectedMods, settings);
      }

      public override FrameworkElement View { get; }
   }

   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow {

      private readonly ComponentContainer _components;

      private BrowserState _currentState;

      public MainWindow() {
         InitializeComponent();
         _components = new ComponentContainer();
      }

      private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
         ShowInitialConfigView();
      }

      private void ShowInitialConfigView() {
         _currentState = _components.Get<InitialConfigState>();
         _currentState.SwitchState += OnSwitchState;
         ShowView(_currentState.View);
      }

      private void OnSwitchState(BrowserState nextState) {
         Layout.Children.Remove(_currentState.View);
         _currentState.SwitchState -= OnSwitchState;
         nextState.SwitchState += OnSwitchState;
         _currentState = nextState;
         ShowView(_currentState.View);
      }

      private void ShowView(FrameworkElement ui) {
         Debug.Assert(ui != null);

         ui.VerticalAlignment = VerticalAlignment.Stretch;
         ui.HorizontalAlignment = HorizontalAlignment.Stretch;
         Grid.SetColumn(ui, 0);
         Grid.SetRow(ui, 0);
         Layout.Children.Add(ui);
      }
   }
}
