using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FactorioBrowser.Mod.Loader;

namespace FactorioBrowser.UI.ViewModel {

   public sealed class GameDirectories {

      public string GamePath { get; }

      public string ModsPath { get; }

      public GameDirectories(string gamePath, string modsPath) {
         GamePath = gamePath;
         ModsPath = modsPath;
      }
   }

   public sealed class MainWindowViewModel : BindableBase {
      private readonly IBrowserStepsFactory _stepsFactory;
      private readonly IFcLocalizationLoader _localizationLoader;

      private INestedScreen _currentScreen;
      private bool _busy = false;

      public MainWindowViewModel(IBrowserStepsFactory stepsFactory, IFcLocalizationLoader localizationLoader) {
         _stepsFactory = stepsFactory;
         _localizationLoader = localizationLoader;
      }

      public INestedScreen CurrentScreen {
         get {
            return _currentScreen;
         }

         private set {
            UpdateProperty(ref _currentScreen, value);
         }
      }

      public bool IsBusy {
         get {
            return _busy || (_currentScreen?.IsBusy ?? false);
         }

         set {
            UpdateProperty(ref _busy, value);
         }
      }

      public async Task ConfigureAndStart() {
         var gameDirectories = await Run(_stepsFactory.CreateInitialConfigStep());
         var mods = await Run(_stepsFactory.CreateModSelectionStep(gameDirectories));
         var coreMod = FcModFileInfo.FromMetaInfo(mods.CoreMod);
         var userMods = mods.SelectableMods.Select(FcModFileInfo.FromMetaInfo);
         var localizationDirectory = await LoadLocalizationDirectory(new [] { coreMod }.Concat(userMods));
         var settings = await Run(_stepsFactory.CreateModSettingsStep(localizationDirectory, mods));
         await Run(_stepsFactory.CreateBrowseStep(localizationDirectory, mods, settings));
      }

      private async Task<ILocalizationDirectory> LoadLocalizationDirectory(IEnumerable<FcModFileInfo> mods) {
         IsBusy = true;
         try {
            return await Task.Factory.StartNew(
               () => _localizationLoader.LoadLocalizationTables(mods));
         } finally {
            IsBusy = false;
         }
      }

      private Task<TResult> Run<TResult>(IBrowserStep<TResult> step) {
         if (CurrentScreen != null) {
            CurrentScreen.PropertyChanged -= OnCurrentScreenPropertyChanged;
         }

         CurrentScreen = step;
         CurrentScreen.PropertyChanged += OnCurrentScreenPropertyChanged;

         return step.Run();
      }

      private void OnCurrentScreenPropertyChanged(object sender, PropertyChangedEventArgs e) {
         if (e.PropertyName == nameof(INestedScreen.IsBusy)) {
            FirePropertyChanged(nameof(IsBusy));
         }
      }
   }

   internal sealed class ActionCommand : ICommand {
      private readonly Action _action;

      public ActionCommand(Action action) {
         _action = action;
      }

      public bool CanExecute(object parameter) {
         return true;
      }

      public void Execute(object parameter) {
         _action?.Invoke();
      }

      public event EventHandler CanExecuteChanged;
   }
}
