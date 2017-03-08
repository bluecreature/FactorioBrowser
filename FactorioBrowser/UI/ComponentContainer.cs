using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using Config.Net;
using FactorioBrowser.Mod.Finder;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;

namespace FactorioBrowser.UI {

   public sealed class AppSettings : SettingsContainer {

      public readonly Option<string> GamePath = new Option<string>("game_path", null);

      public readonly Option<string> UserDataPath = new Option<string>("user_data_path", null);

      public readonly Option<bool> UseSavedSettings = new Option<bool>("use_saved_settings", false);

      protected override void OnConfigure(IConfigConfiguration configuration) {
         var exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
         Debug.Assert(exePath != null);
         var cfgPath = Path.Combine(exePath, "config.ini");

         configuration.UseIniFile(cfgPath);
      }
   }

   public sealed class ComponentContainer {

      private readonly AppSettings _settings;
      private readonly StandardKernel _kernel;

      public ComponentContainer(AppSettings settings) {
         Contract.Assert(settings != null);
         _settings = settings;

         var krnlConfig = new NinjectSettings {
            LoadExtensions = false
         };

         _kernel = new StandardKernel(krnlConfig,
            new ModComponents(settings)
         );
      }

      public T Get<T>() {
         return _kernel.Get<T>();
      }
   }

   internal sealed class ModComponents : NinjectModule {

      private readonly AppSettings _settings;

      public ModComponents(AppSettings settings) {
         _settings = settings;
      }

      public override void Load() {
         Bind<IFcModFinder>().ToMethod(CreateModFinder);
         Bind<IFcModSorter>().To<DefaultFcModSorter>();
      }

      private IFcModFinder CreateModFinder(IContext ctx) {
         return new DefaultFcModFinder(
            _settings.GamePath, _settings.UserDataPath);
      }
   }
}
