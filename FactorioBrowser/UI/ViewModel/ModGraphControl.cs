using System;
using GraphX.Controls;
using GraphX.PCL.Common.Models;
using GraphX.PCL.Logic.Models;
using QuickGraph;

namespace FactorioBrowser.UI.ViewModel {

   public sealed class ModGraphVertex : VertexBase, IComparable<ModGraphVertex> {
      public ModListItem Item { get; }

      public ModGraphVertex(ModListItem item) {
         Item = item;
      }

      public override string ToString() {
         return Item.Info.Name;
      }

      public int CompareTo(ModGraphVertex other) {
         if (ReferenceEquals(this, other)) {
            return 0;

         } else if (ReferenceEquals(null, other)) {
            return 1;

         } else {
            return string.Compare(Item.Info.Name, other.Item.Info.Name, StringComparison.Ordinal);
         }
      }
   }

   public sealed class ModGraphEdge : EdgeBase<ModGraphVertex> {

      public ModGraphEdge(ModGraphVertex source, ModGraphVertex target, bool isOptionalDependency)
         : base(source, target, 1) {
         IsOptionalDependency = isOptionalDependency;
      }

      public bool IsOptionalDependency { get; }
   }

   internal sealed class ModGraphLogic :
      GXLogicCore<ModGraphVertex, ModGraphEdge, BidirectionalGraph<ModGraphVertex, ModGraphEdge>> {

   }

   internal sealed class ModGraphControl :
      GraphArea<ModGraphVertex, ModGraphEdge, BidirectionalGraph<ModGraphVertex, ModGraphEdge>> {
   }
}
