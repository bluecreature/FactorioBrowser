using System;
using System.Collections.Immutable;

namespace FactorioBrowser.Mod.Finder {

   public class FcModInfoException : Exception {
      public FcModInfoException() {
      }

      public FcModInfoException(string message)
         : base(message) {
      }

      public FcModInfoException(string message, Exception inner)
         : base(message, inner) {
      }
   }

   public enum FcModDeploymentType {
      Directory,
      ZipFile,
   }

   public enum FcVersionRange {
      After,
      AtLeast,
      Exactly,
   }

   public sealed class FcVersionRequirement {
      public FcVersion Version { get; }

      public FcVersionRange Range { get; }
      public FcVersionRequirement(FcVersion version, FcVersionRange range) {
         Version = version;
         Range = range;
      }
   }

   public sealed class FcModDependency {
      public string ModName { get; }

      public FcVersionRequirement RequiredVersion { get; }

      public bool Optional { get; }

      public FcModDependency(string modName, FcVersionRequirement requiredVersion, bool optional) {

         this.ModName = modName;
         this.RequiredVersion = requiredVersion;
         Optional = optional;
      }
   }

   public sealed class FcModMetaInfo {
      public string Path { get; }

      public FcModDeploymentType DeploymentType { get; }

      public string Name { get; }

      public FcVersion Version { get; }

      public IImmutableList<FcModDependency> Dependencies { get; }

      public FcModMetaInfo(string path, FcModDeploymentType deploymentType, string name,
         FcVersion version, FcModDependency[] dependencies) {

         Path = path;
         DeploymentType = deploymentType;
         Name = name;
         Version = version;

         if (dependencies != null) {
            var builder = ImmutableArray.CreateBuilder<FcModDependency>(dependencies.Length);
            builder.AddRange(dependencies);
            Dependencies = builder.ToImmutable();

         } else {
            Dependencies = ImmutableArray.Create<FcModDependency>();
         }
      }

      public string VersionedName() {
         return $"{Name}_{Version.ToDotNotation()}";
      }
   }

   public interface IFcModFinder {
      IImmutableList<FcModMetaInfo> FindAll();
   }
}
