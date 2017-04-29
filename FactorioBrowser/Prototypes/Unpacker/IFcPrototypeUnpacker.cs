using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace FactorioBrowser.Prototypes.Unpacker {

   public class PrototypeUnpackException : Exception {
      public string Path { get; }

      public PrototypeUnpackException(string path) : base() {
         Path = path;
      }

      public PrototypeUnpackException(string path, string message) : base(message) {
         Path = path;
      }

      public PrototypeUnpackException(string path, string message, Exception innerException) : base(message, innerException) {
         Path = path;
      }
   }

   public sealed class FcPrototypes {

      public IImmutableList<FcItem> Items { get; }

      public IImmutableList<FcRecipe> Recipes { get; }

      public IImmutableList<FcTechnology> Technologies { get; }

      public FcPrototypes(IEnumerable<FcItem> items,
         IEnumerable<FcRecipe> recipes, IEnumerable<FcTechnology> technologies) {

         Items = items.ToImmutableList();
         Recipes = recipes.ToImmutableList();
         Technologies = technologies.ToImmutableList();
      }
   }

   public interface IFcPrototypeUnpacker {

      FcPrototypes Unpack(ILuaTable dataRaw);
   }
}
