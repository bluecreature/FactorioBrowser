using System.Collections.Generic;
using FactorioBrowser.Prototypes;
using FactorioBrowser.Prototypes.Unpacker;
using NUnit.Framework;

namespace FactorioBrowserTests.Prototypes.Unpacker {

   [ModelMirror]
   internal sealed class StructA {

      [DataFieldMirror("f1")]
      public int FieldOne { get; private set; }
   }

   [ModelMirror]
   internal sealed class StructB {

      [DataFieldMirror("f2")]
      public StructA Nested { get; private set; }
   }

   [TestFixture]
   public class StructureUnpackerTest {

      [Test]
      public void TestUnpackConcreteClass() {
         var unpacked = Unpack<StructA>(new Dictionary<object, object>() {
            ["f1"] = 100,
         });

         Assert.AreEqual(100, unpacked.FieldOne);
      }

      [Test]
      public void TestUnpackNestedClass() {
         var unpacked = Unpack<StructB>(new Dictionary<object, object>() {
            ["f2"] = new Dictionary<object, object>() {
               ["f1"] = 100,
            },
         });

         Assert.AreEqual(100, unpacked.Nested.FieldOne);
      }

      private T Unpack<T>(IDictionary<object, object> table) where T : class {
         return new StructureUnpacker<T>(new UnpackerDispatcher())
            .Unpack(new DictionaryLuaTable(table), "root");
      }
   }
}
