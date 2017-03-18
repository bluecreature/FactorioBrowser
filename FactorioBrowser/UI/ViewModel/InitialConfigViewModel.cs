namespace FactorioBrowser.UI.ViewModel {

   public sealed class InitialConfigViewModel : BindableBase {

      private readonly AppSettings _settings;
      private string _gamePath;
      private string _userDataPath;
      private bool? _useByDefault;

      public InitialConfigViewModel(AppSettings settings) {
         _settings = settings;
         _gamePath = _settings.GamePath;
         _userDataPath = _settings.ModsPath;
         _useByDefault = _settings.UseSavedSettings;
      }

      public string GamePath {
         get {
            return _gamePath;
         }

         set {
            _settings.GamePath.Write(value);
            UpdateProperty(ref _gamePath, value);
         }
      }

      public string UserDataPath {
         get {
            return _userDataPath;
         }

         set {
            _settings.ModsPath.Write(value);
            UpdateProperty(ref _userDataPath, value);
         }
      }

      public bool? UseByDefault {
         get {
            return _useByDefault;
         }

         set {
            _settings.UseSavedSettings.Write(value ?? false);
            UpdateProperty(ref _useByDefault, value);
         }
      }
   }
}
