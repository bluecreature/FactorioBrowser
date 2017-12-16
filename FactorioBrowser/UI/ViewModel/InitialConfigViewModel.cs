namespace FactorioBrowser.UI.ViewModel {

   public sealed class InitialConfigViewModel : BindableBase {

      private readonly IAppSettings _settings;
      private string _gamePath;
      private string _modsPath;
      private bool? _useByDefault;

      public InitialConfigViewModel(IAppSettings settings) {
         _settings = settings;
         _gamePath = _settings.GamePath;
         _modsPath = _settings.ModsPath;
         _useByDefault = _settings.UseSavedSettings;
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
   }
}
