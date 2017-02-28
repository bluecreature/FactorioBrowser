using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FactorioBrowser.Mod.Finder {
   internal sealed class CycleFinderAlgorithm {
      private readonly IDictionary<string, ICollection<string>> _adjList;

      public CycleFinderAlgorithm(IDictionary<string, ICollection<string>> adjList) {
         _adjList = adjList;
      }

      public IImmutableList<IImmutableList<string>> FindCycles() {

         ISet<string> completeNodes = new HashSet<string>();
         IList<IImmutableList<string>> cycles = new List<IImmutableList<string>>();

         foreach (var node in _adjList.Keys) {
            WalkDepthFirst(node, completeNodes, new LinkedList<string>(), cycles);
         }

         return cycles.ToImmutableArray();
      }

      private void WalkDepthFirst(string node, ISet<string> completeNodes,
         LinkedList<string> walkPath, IList<IImmutableList<string>> cycles) {

         if (completeNodes.Contains(node)) {
            return;
         }

         LinkedListNode<string> onCurrentPath;
         if ((onCurrentPath = walkPath.Find(node)) != null) {
            var cycle = new List<string>();
            while (onCurrentPath != null) {
               cycle.Add(onCurrentPath.Value);
               onCurrentPath = onCurrentPath.Next;
            }

            cycle.Add(node);
            cycles.Add(cycle.ToImmutableArray());
            return;
         }

         walkPath.AddLast(node);
         ICollection<string> neighbours = null;
         _adjList.TryGetValue(node, out neighbours);
         foreach (var neighbour in neighbours ?? Enumerable.Empty<string>()) {
            WalkDepthFirst(neighbour, completeNodes, walkPath, cycles);
         }

         walkPath.RemoveLast();
         completeNodes.Add(node);
      }
   }
}
