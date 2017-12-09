using System.Collections.Generic;

namespace FactorioBrowser.Prototypes {

   [ModelMirror]
   [TypeDiscriminatorField("type", "item", "fluid", "tool", "module")]
   public sealed class FcItem : FcPrototype {

      [DataFieldMirror("icon", required: false)]
      public string Icon { get; private set; }

      [DataFieldMirror("icons", required: false)]
      public IList<FcIconLayer> IconLayers { get; private set; }

      [DataFieldMirror("stack_size")]
      public long StackSize { get; private set; }
   }
}
