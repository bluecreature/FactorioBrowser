using System.Collections.Generic;
using System.Linq;
using FactorioBrowser.Prototypes.Unpacker;

namespace FactorioBrowser.Prototypes {

   [ModelMirror]
   [TypeDiscriminatorField("type", "recipe")]
   public sealed class FcRecipe : FcEntity {

      [DataFieldMirror("enabled")]
      public bool Enabled { get; private set; }

      [DataFieldMirror("ingredients")]
      [CustomUnpacker(nameof(UnpackIngredients))]
      public IList<FcRecipeInputOutput> Ingredients { get; private set; }

      [DataFieldMirror]
      [CustomUnpacker(nameof(UnpackResults))]
      public IList<FcRecipeInputOutput> Results { get; private set; }

      [DataFieldMirror("icon")]
      public string Icon { get; private set; }

      private static IList<FcRecipeInputOutput> UnpackResults(IVariantUnpacker unpacker,
         ILuaVariant recipe, string path) {

         RequireTable(recipe, path);

         ILuaTable recipeTable = recipe.AsTable;
         ILuaVariant directResult;
         if ((directResult = recipeTable.Get("result")) != null
            && directResult.ValueType == LuaValueType.String) {

            int? count = unpacker.Unpack<int?>(
               recipeTable.Get("result_count"), path + ".result_count");

            return new List<FcRecipeInputOutput> {
               new FcRecipeInputOutput(directResult.AsString, count ?? 1)
            };

         } else if (recipeTable.Get("results") != null) {
            return unpacker.Unpack<IList<FcRecipeIoDictionary>>(recipe, path + ".results")
               .Select(u => new FcRecipeInputOutput(u.Item, u.ItemType, u.Count))
               .ToList();

         } else {
            return new FcRecipeInputOutput[0];
         }
      }

      private static IList<FcRecipeInputOutput> UnpackIngredients(IVariantUnpacker unpacker,
         ILuaVariant ingredients, string path) {

         RequireTable(ingredients, path);
         RequireTable(ingredients.AsTable.Get(1), $"{path}[1]");

         if (ingredients.AsTable.Get(1).AsTable.Get("type") != null) {
            return unpacker.Unpack<IList<FcRecipeIoDictionary>>(ingredients, path)
               .Select(u => new FcRecipeInputOutput(u.Item, u.ItemType, u.Count))
               .ToList();

         } else {
            return unpacker.Unpack<IList<FcRecipeIngredientList>>(ingredients, path)
               .Select(u => new FcRecipeInputOutput(u.Item, u.Count))
               .ToList();
         }
      }

      private static void RequireTable(ILuaVariant value, string path) {
         if (value?.ValueType != LuaValueType.Table) {
            throw new PrototypeUnpackException(path,
               "Table expected, but found: " + value?.ValueType);
         }
      }

      // ReSharper disable once ClassNeverInstantiated.Local
      [ModelMirror]
      private sealed class FcRecipeIoDictionary {
         [DataFieldMirror("name")]
         public string Item { get; private set; }

         [DataFieldMirror("type")]
         public string ItemType { get; private set; }

         [DataFieldMirror("amount")]
         public int Count { get; private set; }
      }

      // ReSharper disable once ClassNeverInstantiated.Local
      [ModelMirror]
      private sealed class FcRecipeIngredientList {
         [DataFieldMirror(1)]
         public string Item { get; private set; }

         [DataFieldMirror(2)]
         public int Count { get; private set; }
      }
   }

   public sealed class FcRecipeInputOutput {

      public string Item { get; private set; }

      public string ItemType { get; private set; } = "item";

      public int Count { get; private set; }

      public FcRecipeInputOutput() {
      }

      public FcRecipeInputOutput(string item, string itemType, int count) {
         Item = item;
         ItemType = itemType;
         Count = count;
      }

      public FcRecipeInputOutput(string item, int count) : this(item, "item", count) {
      }
   }
}
