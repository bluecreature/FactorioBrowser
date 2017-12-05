using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.Prototypes;
using FactorioBrowser.Prototypes.Unpacker;

using TechnologyGraphVertex = FactorioBrowser.UI.ViewModel.StructureGraphVertex<FactorioBrowser.Prototypes.FcTechnology>;
using TechnologyGraphEdge = FactorioBrowser.UI.ViewModel.StructureGraphEdge<FactorioBrowser.Prototypes.FcTechnology>;
using TechnologyGraph = QuickGraph.BidirectionalGraph<
   FactorioBrowser.UI.ViewModel.StructureGraphVertex<FactorioBrowser.Prototypes.FcTechnology>,
   FactorioBrowser.UI.ViewModel.StructureGraphEdge<FactorioBrowser.Prototypes.FcTechnology>>;

namespace FactorioBrowser.UI.ViewModel {

   public sealed class ImageAssetsCache : IDisposable {
      private readonly IModFileResolver _assetsResolver;
      private readonly ConcurrentDictionary<string, ImageSource> _cache;

      public ImageAssetsCache(IEnumerable<FcModFileInfo> modList) {
         _assetsResolver = ModFileResolverFactory.CreateRoutingResolver(modList);
         _cache = new ConcurrentDictionary<string, ImageSource>();
      }

      public ImageSource GetImage(string path) {
         return _cache.GetOrAdd(path, LoadImage);
      }

      public void Dispose() {
         _assetsResolver.Dispose();
      }

      private ImageSource LoadImage(string path) {
         Stream stream = null;
         try {
            stream = _assetsResolver.Open(path);
            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.CacheOption = BitmapCacheOption.OnLoad;
            imageSource.StreamSource = stream;
            imageSource.EndInit();
            return imageSource;
         } finally {
            stream?.Close();
         }
      }
   }

   public sealed class BrowseViewModel : BindableBase, IDisposable {

      private readonly IFcModDataLoader _modLoader;
      private readonly IImmutableList<FcModFileInfo> _modsToLoad;
      private readonly IImmutableDictionary<string, object> _modSettings;
      private readonly ImageAssetsCache _imageCache;

      private bool _isBusy;

      public BrowseViewModel(IFcModDataLoader modLoader, IImmutableList<FcModFileInfo> modsToLoad,
         IImmutableDictionary<string, object> modSettings) {

         Debug.Assert(modsToLoad != null);
         _modLoader = modLoader;
         _modsToLoad = modsToLoad;
         _modSettings = modSettings;
         _imageCache = new ImageAssetsCache(modsToLoad);
         _isBusy = false;
         Items = new ObservableCollection<FcItem>();
         Recipes = new ObservableCollection<FcRecipe>();
         Technologies = new ObservableCollection<FcTechnology>();
      }

      public bool IsBusy {
         get {
            return _isBusy;
         }

         private set {
            UpdateProperty(ref _isBusy, value);
         }
      }

      public ImageAssetsCache ImageCache => _imageCache;

      public ObservableCollection<FcItem> Items { get; }

      public ObservableCollection<FcRecipe> Recipes { get; }

      public ObservableCollection<FcTechnology> Technologies { get; }

      public TechnologyGraph TechnologyGraph { get; private set; }

      public void Dispose() {
         _imageCache.Dispose();
      }

      public async Task LoadData() {
         IsBusy = true;
         try {
            var unpackedProtos = await Task.Factory.StartNew(LoadAndUnpackData);
            Items.Clear();
            Items.AddRange(unpackedProtos.Items);

            Recipes.Clear();
            Recipes.AddRange(unpackedProtos.Recipes);

            Technologies.Clear();
            Technologies.AddRange(unpackedProtos.Technologies);
            TechnologyGraph = BuildTechnologyGraph(unpackedProtos.Technologies);

         } finally {
            IsBusy = false;
         }
      }


      private TechnologyGraph BuildTechnologyGraph(IImmutableList<FcTechnology> technologies) {
         TechnologyGraph graph = new TechnologyGraph(allowParallelEdges: false,
            vertexCapacity: technologies.Count);

         IDictionary<string, TechnologyGraphVertex> vertexByName =
            new Dictionary<string, TechnologyGraphVertex>(technologies.Count);
         foreach (var tech in technologies) {
            var vertex = new TechnologyGraphVertex(tech);
            graph.AddVertex(vertex);
            vertexByName.Add(tech.Name, vertex);
         }

         foreach (var tech in technologies) {
            if (tech.Prerequisites == null) { // TODO : guarantee non-null for collection fields
               continue;
            }

            foreach (var prerequisite in tech.Prerequisites) {
               var techVertex = vertexByName[tech.Name];
               TechnologyGraphVertex prereqVertex;
               if (vertexByName.TryGetValue(prerequisite, out prereqVertex)) {
                  var edge = new TechnologyGraphEdge(prereqVertex, techVertex);
                  graph.AddEdge(edge);
               }
            }
         }

         return graph;
      }

      private FcPrototypes LoadAndUnpackData() {
         return _modLoader.LoadPrototypes(_modsToLoad, _modSettings);
      }
   }
}
