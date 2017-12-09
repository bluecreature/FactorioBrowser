namespace FactorioBrowser.Prototypes {

   [ModelMirror]
   public abstract class FcPrototype {

      [DataFieldMirror("type")]
      public string Type { get; private set; }

      [DataFieldMirror("name")]
      public string Name { get; private set; }

      [DataFieldMirror("__mod__")]
      public string SourceMod { get; private set; } = "base";
   }

   [ModelMirror]
   public sealed class FcColor {

      [DataFieldMirror("r", required: false)]
      public double? Red { get; private set; }

      [DataFieldMirror("g", required: false)]
      public double? Green { get; private set; }

      [DataFieldMirror("b", required: false)]
      public double? Blue { get; private set; }

      [DataFieldMirror("a", required: false)]
      public double? Alpha { get; private set; }
   }

   [ModelMirror]
   public sealed class FcPosition {
      // TODO : support unpacking the dictionary representation

      [DataFieldMirror(1, required: true)]
      public double X { get; private set; }

      [DataFieldMirror(2, required: true)]
      public double Y { get; private set; }
   }

   [ModelMirror]
   public sealed class FcIconLayer {

      [DataFieldMirror("icon", required: true)]
      public string Icon { get; private set; }

      [DataFieldMirror("tint", required: false)]
      public FcColor Tint { get; private set; }

      [DataFieldMirror("scale", required: false)]
      public double? Scale { get; private set; }

      [DataFieldMirror("shift", required: false)]
      public FcPosition Shift { get; private set; }
   }
}
