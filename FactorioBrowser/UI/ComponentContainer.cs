using System;
using System.Collections.Immutable;
using System.Reflection;
using FactorioBrowser.Mod.Finder;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.Prototypes.Unpacker;
using FactorioBrowser.UI.ViewModel;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Factory;
using Ninject.Modules;

namespace FactorioBrowser.UI {

   public sealed class ComponentContainer {
      private readonly StandardKernel _kernel;

      public ComponentContainer() {
         var krnlConfig = new NinjectSettings {
            LoadExtensions = false,
            InjectNonPublic = true,
         };

         _kernel = new StandardKernel(krnlConfig,
            new ModComponents(),
            new FuncModule()
         );
      }

      public T Get<T>() {
         return _kernel.Get<T>();
      }
   }

   internal sealed class ModComponents : NinjectModule {

      public override void Load() {
         Bind<IAppSettings>().ToMethod((ctx) => AppSettingsFactory.Create()).InSingletonScope();
         Bind<IFcModFinder>().To<DefaultFcModFinder>();
         Bind<IFcModSorter>().To<DefaultFcModSorter>();
         Bind<IFcModDataLoader>().ToMethod(CreateModDataLoader);
         Bind<IFcLocalizationLoader>().To<DefaultLocalizationLoader>();
         Bind<IFcSettingDefsUnpacker>().To<DefaultSettingDefsUnpacker>();
         Bind<IFcPrototypeUnpacker>().To<DefaultPrototypeUnpacker>();
         Bind<IBrowserStepsFactory>().ToFactory(() => new BrowserStepInstanceProvider());
      }

      private IFcModDataLoader CreateModDataLoader(IContext ctx) {
         return new DefaultModDataLoader(
            ctx.Kernel.Get<IAppSettings>().GamePath,
            ctx.Kernel.Get<IFcSettingDefsUnpacker>(),
            ctx.Kernel.Get<IFcPrototypeUnpacker>());
      }
   }

   internal sealed class BrowserStepInstanceProvider : StandardInstanceProvider {

      private readonly IImmutableDictionary<string, Type> _factoryMethodToTypeMap;

      public BrowserStepInstanceProvider() {
         var builder = ImmutableDictionary.CreateBuilder<string, Type>();
         builder.Add(nameof(IBrowserStepsFactory.CreateInitialConfigStep), typeof(InitialConfigurationStep));
         builder.Add(nameof(IBrowserStepsFactory.CreateModSelectionStep), typeof(ModSelectionStep));
         builder.Add(nameof(IBrowserStepsFactory.CreateModSettingsStep), typeof(ModSettingsStep));
         builder.Add(nameof(IBrowserStepsFactory.CreateBrowseStep), typeof(BrowseStep));
         _factoryMethodToTypeMap = builder.ToImmutable();
      }

      protected override Type GetType(MethodInfo methodInfo, object[] arguments) {
         Type resolvedType;
         if (methodInfo.DeclaringType == typeof(IBrowserStepsFactory) &&
            _factoryMethodToTypeMap.TryGetValue(methodInfo.Name, out resolvedType)) {

            return resolvedType;
         }

         return base.GetType(methodInfo, arguments);
      }
   }
}
