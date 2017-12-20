using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using FactorioBrowser.Prototypes;
using FactorioBrowser.UI.ViewModel;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.EdgeRouting;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using GraphX.PCL.Logic.Models;
using QuickGraph;

namespace FactorioBrowser.UI {

   [ValueConversion(typeof(string), typeof(ImageSource))]
   public sealed class ItemTypeIconSelector : IValueConverter {

      public ImageSource ItemIcon { get; set; }

      public ImageSource FluidIcon { get; set; }

      public ImageSource ToolIcon { get; set; }

      public ImageSource ModuleIcon { get; set; }

      public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
         switch (value?.ToString().ToLower()) {
            case "item":
               return ItemIcon;

            case "fluid":
               return FluidIcon;

            case "tool":
               return ToolIcon;

            case "module":
               return ModuleIcon;

            default:
               throw new NotImplementedException();
         }
      }

      public object ConvertBack(object value, Type targetType, object parameter,
         CultureInfo culture) {
         throw new NotImplementedException();
      }
   }

   public sealed class ItemIconLoader : IMultiValueConverter {

      public ImageSource MissingImagePlaceholder { get; set; }

      public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
         FcItem item = values[0] as FcItem;
         IconCache imageCache = values[1] as IconCache;
         if (item == null || imageCache == null) {
            return null;
         }

         try {
            return imageCache.GetIcon(item);

         } catch (FileNotFoundException) {
            return MissingImagePlaceholder;
         }
      }

      public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
         throw new NotImplementedException();
      }
   }

   /// <summary>
   /// Interaction logic for BrowseWindow.xaml
   /// </summary>
   public partial class BrowseView {

      internal BrowseView() {
         InitializeComponent();

         TechGraph.LogicCore = CreateTechGraphLogic();
         TechGraph.SetVerticesDrag(true);

         RecipeGraph.LogicCore = CreateItemRecipeGraphLogic<RecipeGraphLogic, FcRecipe>();
         RecipeGraph.SetVerticesDrag(true);
      }

      private TechnologyGraphLogic CreateTechGraphLogic() {
         var logic = new TechnologyGraphLogic();
         var layoutAlgo = LayoutAlgorithmTypeEnum.EfficientSugiyama;
         logic.DefaultLayoutAlgorithm = layoutAlgo;

         var layoutParams = (EfficientSugiyamaLayoutParameters) logic.AlgorithmFactory.CreateLayoutParameters(layoutAlgo);
         layoutParams.Direction = LayoutDirection.TopToBottom;
         layoutParams.LayerDistance = 60;
         layoutParams.VertexDistance = 20;
         layoutParams.EdgeRouting = SugiyamaEdgeRoutings.Orthogonal;
         layoutParams.MinimizeEdgeLength = true;
         layoutParams.OptimizeWidth = true;
         logic.DefaultLayoutAlgorithmParams = layoutParams;

         logic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
         logic.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 20;
         logic.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 60;

         logic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
         logic.EdgeCurvingEnabled = false;
         logic.AsyncAlgorithmCompute = true;

         return logic;
      }

      private TLogic CreateItemRecipeGraphLogic<TLogic, TCategory>()
         where TLogic : GXLogicCore<
            StructureGraphVertex<TCategory>,
            StructureGraphEdge<TCategory>,
            BidirectionalGraph<StructureGraphVertex<TCategory>, StructureGraphEdge<TCategory>>>, new()
         where TCategory : FcPrototype {

         var logic = new TLogic();

         var layoutAlgo = LayoutAlgorithmTypeEnum.ISOM;
         logic.DefaultLayoutAlgorithm = layoutAlgo;

         var layoutParams = (ISOMLayoutParameters) logic.AlgorithmFactory.CreateLayoutParameters(layoutAlgo);
         layoutParams.MinRadius = 10;
         layoutParams.InitialRadius = 20;
         logic.DefaultLayoutAlgorithmParams = layoutParams;

         logic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
         logic.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 60;
         logic.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 20;

         EdgeRoutingAlgorithmTypeEnum edgeRouting = EdgeRoutingAlgorithmTypeEnum.PathFinder;
         logic.DefaultEdgeRoutingAlgorithm = edgeRouting;

         var edgeRoutingParams = (PathFinderEdgeRoutingParameters) logic.AlgorithmFactory.CreateEdgeRoutingParameters(edgeRouting);
         edgeRoutingParams.PathFinderAlgorithm = PathFindAlgorithm.EuclideanNoSQR;
         edgeRoutingParams.UseDiagonals = true;
         edgeRoutingParams.UseHeavyDiagonals = true;
         edgeRoutingParams.SearchTriesLimit = 100;

         logic.DefaultEdgeRoutingAlgorithmParams = edgeRoutingParams;

         logic.EdgeCurvingEnabled = false;
         logic.AsyncAlgorithmCompute = true;

         return logic;
      }
   }
}
