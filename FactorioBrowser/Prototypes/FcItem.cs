namespace FactorioBrowser.Prototypes {

   [ModelMirror]
   [TypeDiscriminatorField("type", "item")]
   public sealed class FcItem : FcDataStructure {

      [DataFieldMirror("icon")]
      public string Icon { get; private set; }
   }
}
