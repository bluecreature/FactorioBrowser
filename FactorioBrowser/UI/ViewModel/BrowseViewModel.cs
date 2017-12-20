using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FactorioBrowser.Mod.Finder;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.Prototypes;
using FactorioBrowser.Prototypes.Unpacker;
using TechnologyGraphVertex = FactorioBrowser.UI.ViewModel.StructureGraphVertex<FactorioBrowser.Prototypes.FcTechnology>;
using TechnologyGraphEdge = FactorioBrowser.UI.ViewModel.StructureGraphEdge<FactorioBrowser.Prototypes.FcTechnology>;
using TechnologyGraph = QuickGraph.BidirectionalGraph<
   FactorioBrowser.UI.ViewModel.StructureGraphVertex<FactorioBrowser.Prototypes.FcTechnology>,
   FactorioBrowser.UI.ViewModel.StructureGraphEdge<FactorioBrowser.Prototypes.FcTechnology>>;

namespace FactorioBrowser.UI.ViewModel {

   public sealed class FcPrototypeKey {

      private readonly int _hashCode;

      public FcPrototypeKey(string type, string name) {
         TypeName = type;
         Name = name;
         _hashCode = ComputeHashCode();
      }

      public string TypeName { get; }

      public string Name { get; }

      public override int GetHashCode() {
         return _hashCode;
      }

      public override bool Equals(object obj) {
         FcPrototypeKey other = obj as FcPrototypeKey;
         if (other == null) {
            return false;

         } else if (ReferenceEquals(this, obj)) {
            return true;

         } else {
            return TypeName.Equals(other.TypeName) && Name.Equals(other.Name);
         }
      }

      private int ComputeHashCode() {
         unchecked {
            int hash = 29;
            hash = hash * 37 + TypeName.GetHashCode();
            hash = hash * 37 + Name.GetHashCode();
            return hash;
         }
      }
   }

   // TODO : move to the backend
   public sealed class IconCache : IDisposable {
      private readonly IModFileResolver _assetsResolver;
      private readonly ConcurrentDictionary<object, ImageSource> _cache;

      public IconCache(IEnumerable<FcModFileInfo> modList) {
         _assetsResolver = ModFileResolverFactory.CreateRoutingResolver(modList);
         _cache = new ConcurrentDictionary<object, ImageSource>();
      }

      public ImageSource GetIcon(FcItem item) {
         var key = new FcPrototypeKey(item.Type, item.Name);
         return _cache.GetOrAdd(key, (k) => ResolveItemIcon(item));
      }

      public void Dispose() {
         _assetsResolver.Dispose();
      }

      private ImageSource ResolveItemIcon(FcItem item) {
         if (item.Icon != null) {
            return GetOrLoadImage(item.Icon);

         } else if (IsSimpleSingleLayerIcon(item.IconLayers)) {
            return GetOrLoadImage(item.IconLayers[0].Icon);

         } else if (item.IconLayers.Count > 0) {
            return ComposeImage(item.IconLayers);

         } else {
            return null;
         }
      }

      private ImageSource GetOrLoadImage(string path) {
         return _cache.GetOrAdd(path, (k) => LoadImage(path));
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

      private ImageSource ComposeImage(IList<FcIconLayer> layers) {
         var drawingVisual = new DrawingVisual();
         using (var drawingContext = drawingVisual.RenderOpen()) {
            foreach (var layer in layers) {
               DrawLayer(drawingContext, layer);
            }
         }

         var composite = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
         composite.Render(drawingVisual);
         return composite;
      }

      private void DrawLayer(DrawingContext drawingContext, FcIconLayer layer) {
         ImageSource baseImage = GetOrLoadImage(layer.Icon);
         Action<DrawingContext> drawAction = (ctx) => DrawSimpleImage(ctx, baseImage);

         // The tinting must be the second inner-most action so that the shift/scale can affect the tinted image
         if (layer.Tint != null) {
            var innerAction = drawAction;
            drawAction = (ctx) => ApplyTint(ctx, innerAction, layer.Tint, baseImage);
         }

         if (layer.Scale.HasValue) {
            var innerAction = drawAction;
            drawAction = (ctx) => ApplyScale(ctx, innerAction, layer.Scale.Value);
         }

         if (layer.Shift != null) {
            var innerAction = drawAction;
            drawAction = (ctx) => ApplyShift(ctx, innerAction, layer.Shift);
         }

         drawAction.Invoke(drawingContext);
      }

      private Color ParseFcColor(FcColor fcColor) {
         return Color.FromArgb(
            ScaleColorComponent(fcColor.Alpha.GetValueOrDefault(1)),
            ScaleColorComponent(fcColor.Red.GetValueOrDefault(0)),
            ScaleColorComponent(fcColor.Green.GetValueOrDefault(0)),
            ScaleColorComponent(fcColor.Blue.GetValueOrDefault(0)));
      }

      private byte ScaleColorComponent(double component) {
         return (byte) Math.Ceiling(255 * component);
      }

      private void ApplyScale(DrawingContext drawingContext, Action<DrawingContext> innerAction,
         double scale) {
         drawingContext.PushTransform(new ScaleTransform(scale, scale));
         innerAction.Invoke(drawingContext);
         drawingContext.Pop();
      }

      private void ApplyShift(DrawingContext drawingContext, Action<DrawingContext> innerAction,
         FcPosition shift) {

         drawingContext.PushTransform(new TranslateTransform(shift.X, shift.Y));
         innerAction.Invoke(drawingContext);
         drawingContext.Pop();
      }

      private void ApplyTint(DrawingContext drawingContext, Action<DrawingContext> innerAction,
         FcColor tint, ImageSource tintMask) {

         innerAction.Invoke(drawingContext);

         Brush opacityMask = new ImageBrush(tintMask);
         Brush tintBrush = new SolidColorBrush(ParseFcColor(tint));

         drawingContext.PushOpacityMask(opacityMask);
         drawingContext.PushOpacity(0.5);
         drawingContext.DrawRectangle(tintBrush, null, new Rect(0, 0, tintMask.Width, tintMask.Height));
         drawingContext.Pop();
         drawingContext.Pop();
      }

      private void DrawSimpleImage(DrawingContext drawingContext, ImageSource image) {
         drawingContext.DrawImage(image, new Rect(0, 0, image.Width, image.Height));
      }

      private bool IsSimpleSingleLayerIcon(IList<FcIconLayer> layers) {
         return layers.Count == 1 && IsSimpleLayer(layers[0]);
      }

      private bool IsSimpleLayer(FcIconLayer layer) {
         return !layer.Scale.HasValue && layer.Shift == null && layer.Tint == null;
      }
   }

   public sealed class BrowseStep : BindableBase, IBrowserStep<object> {
      private readonly IFcModDataLoader _dataLoader;
      private readonly ILocalizationDirectory _localizationDirectory;
      private readonly IImmutableDictionary<string, object> _settings;
      private readonly FcModList _modsToLoad;

      private bool _isBusy;
      private object _viewModel;

      public BrowseStep(IFcModDataLoader dataLoader, ILocalizationDirectory localizationDirectory,
         FcModList modsToLoad, IImmutableDictionary<string, object> settings) {
         _dataLoader = dataLoader;
         _localizationDirectory = localizationDirectory;
         _settings = settings;
         _modsToLoad = modsToLoad;
      }

      public bool IsBusy {
         get {
            return _isBusy;
         }

         private set {
            UpdateProperty(ref _isBusy, value);
         }
      }

      public object ViewModel {
         get {
            return _viewModel;
         }

         private set {
            UpdateProperty(ref _viewModel, value);
         }
      }

      public async Task<object> Run() {
         IsBusy = true;
         try {
            IImmutableList<FcModFileInfo> modFiles = _modsToLoad.SelectableMods
               .Select(FcModFileInfo.FromMetaInfo)
               .ToImmutableList();
            FcPrototypes prototypes = await Task.Factory.StartNew(() =>
               _dataLoader.LoadPrototypes(modFiles, _settings));
            ViewModel = new BrowseViewModel(modFiles, prototypes);
            return null;

         } finally {
            IsBusy = false;
         }
      }
   }

   public sealed class BrowseViewModel : BindableBase, IDisposable {
      private readonly IconCache _imageCache;

      public BrowseViewModel(IImmutableList<FcModFileInfo> modsToLoad, FcPrototypes prototypes) {

         Debug.Assert(modsToLoad != null);
         _imageCache = new IconCache(modsToLoad);

         Items = new ObservableCollection<FcItem>();
         Items.AddRange(prototypes.Items);

         Recipes = new ObservableCollection<FcRecipe>();
         Recipes.AddRange(prototypes.Recipes);

         Technologies = new ObservableCollection<FcTechnology>();
         Technologies.AddRange(prototypes.Technologies);
         TechnologyGraph = BuildTechnologyGraph(prototypes.Technologies);
      }

      public IconCache ImageCache => _imageCache;

      public ObservableCollection<FcItem> Items { get; }

      public ObservableCollection<FcRecipe> Recipes { get; }

      public ObservableCollection<FcTechnology> Technologies { get; }

      public TechnologyGraph TechnologyGraph { get; private set; }

      public void Dispose() {
         _imageCache.Dispose();
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
   }
}
