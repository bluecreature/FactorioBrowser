namespace FactorioBrowser.Prototypes {

   [ModelMirror]
   [TypeDiscriminatorField("type", "item", "fluid", "tool", "module")]
   public sealed class FcItem : FcPrototype {

      [DataFieldMirror("icon")]
      public string Icon { get; private set; }

      [DataFieldMirror("stack_size")]
      public long StackSize { get; private set; }
   }
}
