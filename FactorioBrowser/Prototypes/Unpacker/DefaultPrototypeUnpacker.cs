using System.Collections.Generic;
using System.Linq;
using QuickGraph;

namespace FactorioBrowser.Prototypes.Unpacker {

   public class DefaultPrototypeUnpacker : IFcPrototypeUnpacker {

      public FcPrototypes Unpack(ILuaTable dataRaw) {
         var unpacked = new UnpackerDispatcher().
            Unpack<IDictionary<string, IDictionary<string, FcPrototype>>>(dataRaw.ToVariant(), "data.raw");

         IList<FcItem> items = new List<FcItem>();
         IList<FcRecipe>recipes = new List<FcRecipe>();
         IList<FcTechnology> technologies = new List<FcTechnology>();
         foreach (var category in unpacked.Values) {
            foreach (var structure in category.Values) {
               if (structure is FcItem) {
                  items.Add((FcItem)structure);

               } else if (structure is FcRecipe) {
                  recipes.Add((FcRecipe)structure);

               } else if (structure is FcTechnology) {
                  technologies.Add((FcTechnology)structure);
               }
            }
         }

         return new FcPrototypes(
            items.OrderBy(i => i.Name),
            recipes.OrderBy(r => r.Name),
            technologies.OrderBy(t => t.Name));
      }
   }
}
