using System.Collections.Generic;
using FactorioBrowser.Prototypes.Unpacker;

namespace FactorioBrowser.Prototypes {

   [ModelMirror]
   [TypeDiscriminatorField("type", "recipe")]
   public sealed class FcRecipe : FcDataStructure {

      [DataFieldMirror("enabled")]
      public bool Enabled { get; private set; }

      [DataFieldMirror("ingredients")]
      [CustomUnpacker(nameof(UnpackIngredients))]
      public IList<FcRecipeIngredient> Ingredients { get; private set; }

      [DataFieldMirror]
      [CustomUnpacker(nameof(UnpackResults))]
      public IList<FcRecipeProduct> Results { get; private set; }

      [DataFieldMirror("icon")]
      public string Icon { get; private set; }

      private static IList<FcRecipeProduct> UnpackResults(IVariantUnpacker unpacker,
         ILuaVariant recipe, string path) {

         RequireTable(recipe, path);

         ILuaTable recipeTable = recipe.AsTable;
         ILuaVariant directResult;
         if ((directResult = recipeTable.Get("result")) != null
            && directResult.ValueType == LuaValueType.String) {

            int? count = unpacker.Unpack<int?>(
               recipeTable.Get("result_count"), path + ".result_count");

            return new List<FcRecipeProduct> {
               new FcRecipeProduct(directResult.AsString, count ?? 1)
            };

         } else if (recipeTable.Get("results") != null) {
            return unpacker.Unpack<IList<FcRecipeProduct>>(recipe, path + ".results");

         } else {
            return new FcRecipeProduct[0];
         }
      }

      private static IList<FcRecipeIngredient> UnpackIngredients(IVariantUnpacker unpacker,
         ILuaVariant ingredients, string path) {

         RequireTable(ingredients, path);
         var ingredientsTable = ingredients.AsTable;

         int index = 1;
         ILuaVariant item;
         IList<FcRecipeIngredient> result = new List<FcRecipeIngredient>();
         while ((item = ingredientsTable.Get(index)) != null) {
            var itemPath = $"{path}[{index}]";
            index++;

            RequireTable(item, itemPath);
            if (item.AsTable.Get("type") != null) {
               var u = unpacker.Unpack<FcRecipeIngredientDictionary>(item, itemPath);
               result.Add(new FcRecipeIngredient(u.Item, u.ItemType, u.Count));
            } else {
               var u = unpacker.Unpack<FcRecipeIngredientList>(item, itemPath);
               result.Add(new FcRecipeIngredient(u.Item, u.Count));
            }
         }

         return result;
      }

      private static void RequireTable(ILuaVariant value, string path) {
         if (value?.ValueType != LuaValueType.Table) {
            throw new PrototypeUnpackException(path,
               "Table expected, but found: " + value?.ValueType);
         }
      }

      // ReSharper disable once ClassNeverInstantiated.Local
      [ModelMirror]
      private sealed class FcRecipeIngredientDictionary {
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

   public sealed class FcRecipeIngredient {

      public string Item { get; }

      public string ItemType { get; }

      public int Count { get; }

      public FcRecipeIngredient(string item, string itemType, int count) {
         Item = item;
         ItemType = itemType;
         Count = count;
      }

      public FcRecipeIngredient(string item, int count) : this(item, "item", count) {
      }
   }

   [ModelMirror]
   public sealed class FcRecipeProduct {

      [DataFieldMirror("name")]
      public string Item { get; private set; }

      [DataFieldMirror("type")]
      public string ItemType { get; private set; }

      [DataFieldMirror("amount")]
      public double Amount { get; private set; }

      [DataFieldMirror("amount_min")]
      public int AmountMin { get; private set; }

      [DataFieldMirror("amount_max")]
      public int AmountMax { get; private set; }

      [DataFieldMirror("probability")]
      public double Probability { get; private set; }

      public FcRecipeProduct() {
      }

      public FcRecipeProduct(string item, double amount) {
         Item = item;
         ItemType = "item";
         Amount = amount;
         AmountMin = 0;
         AmountMax = 0;
         Probability = 1;
      }
   }
}
