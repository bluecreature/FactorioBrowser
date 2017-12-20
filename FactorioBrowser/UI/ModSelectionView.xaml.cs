using System.Windows;
using FactorioBrowser.UI.ViewModel;
using GraphX.Controls.Models;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using QuickGraph;

namespace FactorioBrowser.UI {

   /// <summary>
   /// Interaction logic for ModSelectionView.xaml
   /// </summary>
   public partial class ModSelectionView {

      public static DependencyProperty ModDependencyGraphProperty = DependencyProperty.Register(
         "ModDependencyGraph",
         typeof(BidirectionalGraph<ModGraphVertex, ModGraphEdge>),
         typeof(ModSelectionView),
         new FrameworkPropertyMetadata() {
            PropertyChangedCallback = OnDependencyGraphChanged,
            BindsTwoWayByDefault = false
         }
      );

      public BidirectionalGraph<ModGraphVertex, ModGraphEdge> ModDependencyGraph {
         get {
            return (BidirectionalGraph<ModGraphVertex, ModGraphEdge>) GetValue(ModDependencyGraphProperty);
         }

         set {
            SetValue(ModDependencyGraphProperty, value);
         }
      }

      private static void OnDependencyGraphChanged(DependencyObject d,
         DependencyPropertyChangedEventArgs e) {

         var view = d as ModSelectionView;
         var graph = e.NewValue as BidirectionalGraph<ModGraphVertex, ModGraphEdge>;
         if (view != null && graph != null) {
            view.ModGraph.GenerateGraph(graph);
            view.ModGraphZoom.ZoomToFill();
         }
      }

      public ModSelectionView() {
         InitializeComponent();
         ModGraph.LogicCore = InitModGraphLogic();
         ModGraph.SetVerticesDrag(true);
         SetBinding(ModDependencyGraphProperty, nameof(ModSelectionViewModel.DependencyGraph));
      }

      private ModGraphLogic InitModGraphLogic() {
         var logic = new ModGraphLogic();
         var layoutAlgo = LayoutAlgorithmTypeEnum.BoundedFR;
         var algoParams = (BoundedFRLayoutParameters) logic.AlgorithmFactory.CreateLayoutParameters(layoutAlgo);
         algoParams.CoolingFunction = FRCoolingFunction.Linear;
         logic.DefaultLayoutAlgorithm = layoutAlgo;
         logic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
         logic.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 60;
         logic.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 20;

         logic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
         logic.AsyncAlgorithmCompute = true;

         return logic;
      }

      private void ModGraph_OnVertexSelected(object sender, VertexSelectedEventArgs args) {
         ModGraphVertex vertex = (ModGraphVertex) args.VertexControl.Vertex;
         vertex.Item.Selected = true;
         ModListView.ScrollIntoView(vertex.Item);
      }

      private void ModSelectionView_OnUnloaded(object sender, RoutedEventArgs e) {
         ModGraph?.Dispose();
      }
   }
}
