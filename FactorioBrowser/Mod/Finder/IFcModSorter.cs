using System.Collections.Generic;
using System.Collections.Immutable;

namespace FactorioBrowser.Mod.Finder {

   public interface IDependencyProblem { }

   public sealed class MissingDependency : IDependencyProblem {
      public string Dependency { get; }

      public MissingDependency(string dependency) {
         Dependency = dependency;
      }
   }

   public sealed class DependencyVersionMismatch : IDependencyProblem {
      public string Dependency { get; }

      public FcVersionRequirement ExpectedVersion { get; }

      public FcVersion ActualVersion { get; }

      public DependencyVersionMismatch(string dependency, FcVersionRequirement expected,
         FcVersion actual) {

         Dependency = dependency;
         ExpectedVersion = expected;
         ActualVersion = actual;
      }
   }

   public sealed class CyclicDependency : IDependencyProblem {
      public IImmutableList<string> Cycle { get; }

      public CyclicDependency(IImmutableList<string> cycle) {
         Cycle = cycle;
      }
   }

   public sealed class SortStatus {
      public FcModMetaInfo ModInfo { get; }

      public bool Successful { get; }

      public IImmutableList<IDependencyProblem> Problems { get; }

      public SortStatus(FcModMetaInfo modInfo, IList<IDependencyProblem> problems) {
         ModInfo = modInfo;
         Successful = problems.Count == 0;
         Problems = ImmutableList.CreateRange(problems);
      }

      public static SortStatus Create(FcModMetaInfo modInfo, params IDependencyProblem[] problems) {
         return new SortStatus(modInfo, ImmutableList.CreateRange(problems));
      }
   }


   public interface IFcModSorter {

      IImmutableList<SortStatus> Sort(IEnumerable<FcModMetaInfo> modList);
   }
}
