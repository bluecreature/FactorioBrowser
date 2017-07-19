using System.Collections.Generic;

namespace FactorioBrowser.Prototypes {
   [ModelMirror]
   [TypeDiscriminatorField("type", "technology")]
   public sealed class FcTechnology : FcPrototype {

      [DataFieldMirror("icon")]
      public string Icon { get; private set; }

      [DataFieldMirror("effects")]
      public IList<FcTechnologyEffect> Effects { get; private set; }

      [DataFieldMirror("prerequisites")]
      public IList<string> Prerequisites { get; private set; }

      [DataFieldMirror("unit")]
      public FcTechResearchCost Cost { get; private set; }
   }


   [ModelMirror]
   public sealed class FcTechnologyEffect {
      [DataFieldMirror("type")]
      public string Type { get; private set; }
   }

   [ModelMirror]
   public sealed class FcTechResearchIngredient {
      [DataFieldMirror(1)]
      public string Item { get; private set; }

      [DataFieldMirror(2)]
      public int Count { get; private set; }
   }

   [ModelMirror]
   public sealed class FcTechResearchCost {
      [DataFieldMirror("count")]
      public int Count { get; private set; }

      [DataFieldMirror("time")]
      public int Time { get; private set; }

      [DataFieldMirror("ingredients")]
      public IList<FcTechResearchIngredient> Ingredients { get; private set; }
   }
}
