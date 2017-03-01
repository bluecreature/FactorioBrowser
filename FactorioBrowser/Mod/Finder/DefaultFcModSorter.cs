using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace FactorioBrowser.Mod.Finder {

   internal sealed class DefaultFcModSorter : IFcModSorter {

      public IImmutableList<SortStatus> Sort(IEnumerable<FcModMetaInfo> modList) {
         var infoByName = MapByName(modList);
         var statusByName = new ModDependencyValidator(infoByName).Validate();
         var modByDependants = MapByDependency(statusByName);
         var topologicalOrder = new TopologicalSortAlgorithm(modByDependants).Sort();

         var statusList = new List<SortStatus>(statusByName.Count);
         foreach (var mod in topologicalOrder) {
            var status = statusByName[mod];
            statusList.Add(status);
            statusByName.Remove(mod);
         }


         foreach (var reject in statusByName) {
            Debug.Assert(!reject.Value.Successful,
               "Mod topological sort missed successfully resolved mod " + reject.Key);

            statusList.Add(reject.Value);
         }

         return statusList.ToImmutableArray();
      }

      private IDictionary<string, ICollection<string>> MapByDependency(
         IDictionary<string, SortStatus> statusByName) {

         var depGraph = new Dictionary<string, ICollection<string>>();
         foreach (var modStatus in statusByName) {
            if (modStatus.Value.Successful) {
               if (!depGraph.ContainsKey(modStatus.Key)) {
                  depGraph.Add(modStatus.Key, new HashSet<string>());
               }

               foreach (var dep in modStatus.Value.ModInfo.Dependencies) {
                  if (IsResolvedSuccessfully(statusByName, dep.ModName)) {
                     AddValue(depGraph, dep.ModName, modStatus.Key);
                  }
               }
            }
         }

         return depGraph;
      }

      private bool IsResolvedSuccessfully(IDictionary<string, SortStatus> statusByName,
         string modName) {

         SortStatus status;
         return statusByName.TryGetValue(modName, out status) && status.Successful;
      }

      private void AddValue<TKey, TValue>(IDictionary<TKey, ICollection<TValue>> multiDict,
         TKey key, TValue value) {

         if (!multiDict.ContainsKey(key)) {
            multiDict[key] = new HashSet<TValue>();
         }

         multiDict[key].Add(value);
      }

      private IDictionary<string, FcModMetaInfo> MapByName(IEnumerable<FcModMetaInfo> modList) {
         IDictionary<string, FcModMetaInfo> byName = new Dictionary<string, FcModMetaInfo>();
         foreach (var modInfo in modList) {
            byName[modInfo.Name] = modInfo; // TODO : check duplicates
         }

         return byName;
      }
   }
}
