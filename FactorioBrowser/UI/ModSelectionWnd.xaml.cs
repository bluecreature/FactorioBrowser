﻿using System;
using System.Windows;
using FactorioBrowser.UI.ViewModel;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;

namespace FactorioBrowser.UI {

   /// <summary>
   /// Interaction logic for ModSelectionWnd.xaml
   /// </summary>
   public partial class ModSelectionWnd : Window, IDisposable {
      private readonly ModSelectionViewModel _viewModel;

      public ModSelectionWnd(ModSelectionViewModel viewModel) {
         _viewModel = viewModel;
         InitializeComponent();
         DataContext = _viewModel;
         ModGraph.LogicCore = InitModGraphLogic();
      }

      private async void RefreshModList(object sender, RoutedEventArgs e) {
         await _viewModel.RefreshModList();

         ModGraph.ClearLayout();
         ModGraph.GenerateGraph(_viewModel.DependencyGraph);
         ModGraphZoom.ZoomToFill();
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
         logic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
         logic.AsyncAlgorithmCompute = true;

         return logic;
      }

      public void Dispose() {
         ModGraph?.Dispose();
      }
   }
}
