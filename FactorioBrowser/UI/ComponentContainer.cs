using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using Config.Net;
using FactorioBrowser.Mod.Finder;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.Prototypes.Unpacker;
using FactorioBrowser.UI.ViewModel;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Factory;
using Ninject.Modules;

namespace FactorioBrowser.UI {

   public sealed class AppSettings : SettingsContainer {

      public readonly Option<string> GamePath = new Option<string>("game_path", null);

      public readonly Option<string> ModsPath = new Option<string>("mods_path", null);

      public readonly Option<bool> UseSavedSettings = new Option<bool>("use_saved_settings", false);

      protected override void OnConfigure(IConfigConfiguration configuration) {
         var exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
         Debug.Assert(exePath != null);
         var cfgPath = Path.Combine(exePath, "config.ini");

         configuration.UseIniFile(cfgPath);
      }
   }

   public sealed class ComponentContainer {
      private readonly StandardKernel _kernel;

      public ComponentContainer(AppSettings settings) {
         Contract.Assert(settings != null);

         var krnlConfig = new NinjectSettings {
            LoadExtensions = false,
            InjectNonPublic = true,
         };

         _kernel = new StandardKernel(krnlConfig,
            new ModComponents(settings),
            new FuncModule()
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
         Bind<IFcModDataLoader>().ToMethod(CreateModDataLoader);
         Bind<IFcSettingDefsUnpacker>().To<DefaultSettingDefsUnpacker>();
         Bind<IFcPrototypeUnpacker>().To<DefaultPrototypeUnpacker>();
         Bind<IBrowseViewFactory>().To<BrowseViewFactoryImpl>();
         Bind<IBrowseViewModelFactory>().ToFactory();
      }

      private IFcModFinder CreateModFinder(IContext ctx) {
         return new DefaultFcModFinder(
            _settings.GamePath, _settings.ModsPath);
      }

      private IFcModDataLoader CreateModDataLoader(IContext ctx) {
         return new DefaultModDataLoader(_settings.GamePath,
            ctx.Kernel.Get<IFcSettingDefsUnpacker>(), ctx.Kernel.Get<IFcPrototypeUnpacker>());
      }
   }

   public interface IBrowseViewModelFactory {
      BrowseViewModel Create(IEnumerable<FcModFileInfo> modsToLoad);
   }

   internal sealed class BrowseViewFactoryImpl : IBrowseViewFactory {
      private readonly IBrowseViewModelFactory _viewModelFactory;

      public BrowseViewFactoryImpl(IBrowseViewModelFactory viewModelFactory) {
         _viewModelFactory = viewModelFactory;
      }

      public BrowseView Create(IEnumerable<FcModFileInfo> modsToLoad) {
         var viewModel = _viewModelFactory.Create(modsToLoad);
         return new BrowseView(viewModel);
      }
   }
}
