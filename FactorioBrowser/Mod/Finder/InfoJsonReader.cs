using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FactorioBrowser.Mod.Finder {

   public class InfoJson {
      public string Name { get; }

      public FcVersion Version { get; }

      public FcModDependency[] Dependencies { get; }

      public InfoJson(string name, FcVersion version, FcModDependency[] dependencies) {
         Name = name;
         Version = version;
         Dependencies = dependencies;
      }

      public override string ToString() {
         return $"info.json {{ name: {Name}, version: {Version.ToDotNotation()}, " +
                $"dependencies: [{String.Join("", Dependencies.Select(d => d.ToString()))}] }}";
      }
   }

   public class InfoJsonReader {
      private const string KeyName = "name";
      private const string KeyVersion = "version";
      private const string KeyDependencies = "dependencies";
      private static readonly Regex DependencySpecPattern = new Regex(
         "^(\\?)?\\s*([\\w\\s\\-]+)(?:(>|>=|=>|=)\\s*([\\d\\.]+))?", RegexOptions.Compiled);

      private readonly StreamReader _reader;
      private readonly string _loadErrorMsg;

      public InfoJsonReader(string source, StreamReader reader) {
         _reader = reader;
         _loadErrorMsg = $"Error loading info.json for `{source}': ";
      }

      public InfoJson Read() {
         JsonSerializer serializer = new JsonSerializer();
         JsonReader jsonReader = new JsonTextReader(_reader);
         Dictionary<string, object> rawData = serializer.
            Deserialize<Dictionary<string, object>>(jsonReader);

         string name = RequireKeyOfType<string>(rawData, KeyName, "string");
         string version = RequireKeyOfType<string>(rawData, KeyVersion, "string");
         FcModDependency[] deps = null;
         object rawDeps;
         if (rawData.TryGetValue(KeyDependencies, out rawDeps)) {
            deps = ParseDependencies(rawDeps);
         }

         var parsedVersion = FcVersion.FromDotNotation(version);

         return new InfoJson(name, parsedVersion, deps);
      }

      private object RequireKey(Dictionary<string, object> info, string key) {
         if (!info.ContainsKey(key)) {
            throw new FcModInfoException(_loadErrorMsg + $"required key {key} is missing.");
         }

         return info[key];
      }

      private T RequireType<T>(string key, object value, string typeFriendlyName) where T: class {
         if (!(value is T)) {
            throw new FcModInfoException(
               _loadErrorMsg + $"expected {key} of type `{typeFriendlyName}'.");
         }

         return (T) value;
      }

      private T RequireKeyOfType<T>(Dictionary<string, object> info, string key,
         string typeFriendlyName) where T : class {

         object value = RequireKey(info, key);
         return RequireType<T>(key, value, typeFriendlyName);
      }

      private FcModDependency[] ParseDependencies(object rawDepsDefinition) {
         return RequireType<JArray>(KeyDependencies, rawDepsDefinition, "array")
            .Select(v => ParseDependencySpec((string) v))
            .ToArray(); // XXX : handle malformed specs
      }

      private FcModDependency ParseDependencySpec(string depSpec) {
         var match = DependencySpecPattern.Match(depSpec);
         if (!match.Success) {
            throw new FcModInfoException(
               _loadErrorMsg + $"invalid dependency specification: `{depSpec}'");
         }

         bool optional = match.Groups[1].Value.Trim().Equals("?");
         string modName = match.Groups[2].Value.Trim();

         FcVersionRequirement requiredVersion;
         if (match.Groups[3].Value.Length > 0) {
            FcVersionRange range = ParseReqOperator(match.Groups[3].Value);
            FcVersion version = FcVersion.FromDotNotation(match.Groups[4].Value);
            requiredVersion = new FcVersionRequirement(version, range);
         } else {
            requiredVersion = null;
         }
         // TODO : support optional version requirement

         return new FcModDependency(modName, requiredVersion, optional);
      }

      private FcVersionRange ParseReqOperator(string op) {
         switch (op) {
            case ">":
               return  FcVersionRange.After;
            case "=>":
            case ">=":
               return FcVersionRange.AtLeast;
            default:
               Debug.Assert(op.Equals("="), $"op is `{op}', should be one of >, >=, <, <=, =.");
               return FcVersionRange.Exactly;
         }
      }
   }
}
