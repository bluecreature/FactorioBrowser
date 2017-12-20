using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FactorioBrowser.UI.ViewModel {

   public sealed class InitialConfigurationStep : BindableBase, IBrowserStep<GameDirectories> {
      private readonly IAppSettings _settings;
      private object _viewModel;

      public object ViewModel {
         get {
            return _viewModel;
         }

         private set {
            UpdateProperty(ref _viewModel, value);
         }
      }

      public bool IsBusy => false;

      public InitialConfigurationStep(IAppSettings settings) {
         _settings = settings;
         _viewModel = null;
      }

      public Task<GameDirectories> Run() {
         var taskSource = new TaskCompletionSource<GameDirectories>();
         if (_settings.UseSavedSettings) {
            var dirsFromSettings = new GameDirectories(_settings.GamePath, _settings.ModsPath);
            taskSource.SetResult(dirsFromSettings);

         } else {
            Action<GameDirectories> submitAction = (dirs) => {
               taskSource.SetResult(dirs);
            };

            var viewModel = new InitialConfigViewModel(_settings, submitAction);
            ViewModel = viewModel;
         }

         return taskSource.Task;
      }
   }

   public sealed class InitialConfigViewModel : BindableBase {

      private readonly IAppSettings _settings;
      private string _gamePath;
      private string _modsPath;
      private bool? _useByDefault;

      public InitialConfigViewModel(IAppSettings settings,
         Action<GameDirectories> submitAction) {

         _settings = settings;
         _gamePath = _settings.GamePath;
         _modsPath = _settings.ModsPath;
         _useByDefault = _settings.UseSavedSettings;
         SubmitCommand = new ActionCommand(() => {
            submitAction.Invoke(new GameDirectories(_gamePath, _modsPath));
         });
      }

      public string GamePath {
         get {
            return _gamePath;
         }

         set {
            _settings.GamePath = value;
            UpdateProperty(ref _gamePath, value);
         }
      }

      public string ModsPath {
         get {
            return _modsPath;
         }

         set {
            _settings.ModsPath = value;
            UpdateProperty(ref _modsPath, value);
         }
      }

      public bool? UseByDefault {
         get {
            return _useByDefault;
         }

         set {
            _settings.UseSavedSettings = value ?? false;
            UpdateProperty(ref _useByDefault, value);
         }
      }

      public ICommand SubmitCommand { get; }
   }
}
