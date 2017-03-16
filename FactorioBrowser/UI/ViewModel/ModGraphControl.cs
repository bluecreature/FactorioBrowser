using System;
using GraphX.Controls;
using GraphX.PCL.Common.Models;
using GraphX.PCL.Logic.Models;
using QuickGraph;

namespace FactorioBrowser.UI.ViewModel {

   public sealed class ModGraphVertex : VertexBase, IComparable<ModGraphVertex> {
      public string ModName { get; }

      public ModGraphVertex(string modName) {
         ModName = modName;
      }

      public override string ToString() {
         return ModName;
      }

      public int CompareTo(ModGraphVertex other) {
         if (ReferenceEquals(this, other)) {
            return 0;

         } else if (ReferenceEquals(null, other)) {
            return 1;

         } else {
            return string.Compare(ModName, other.ModName, StringComparison.Ordinal);
         }
      }
   }

   public sealed class ModGraphEdge : EdgeBase<ModGraphVertex> {

      public ModGraphEdge(ModGraphVertex source, ModGraphVertex target, double weight = 1)
         : base(source, target, weight) {
      }
   }

   public sealed class ModGraphLogic :
      GXLogicCore<ModGraphVertex, ModGraphEdge, BidirectionalGraph<ModGraphVertex, ModGraphEdge>> {

   }

   public sealed class ModGraphControl :
      GraphArea<ModGraphVertex, ModGraphEdge, BidirectionalGraph<ModGraphVertex, ModGraphEdge>> {
   }
}
