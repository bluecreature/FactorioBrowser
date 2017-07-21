using System;
using FactorioBrowser.Prototypes.Unpacker;
using NUnit.Framework;

namespace FactorioBrowserTests.Prototypes.Unpacker {

   [TestFixture]
   public sealed class UnpackerDispatcherTest {

      [TestCase(100)]
      [TestCase(100L)]
      [TestCase("test")]
      [TestCase(false)]
      public void TestDispatchAtomicValue(object value) {
         var unpacked = Unpack(value.GetType(), value);

         Assert.NotNull(unpacked);
         Assert.IsAssignableFrom(value.GetType(), unpacked);
         Assert.AreEqual(value, unpacked);
      }

      [Test]
      public void TestDispatchNullableValue() {
         var unpacked = Unpack<long?>(100L);
         Assert.IsInstanceOf<long?>(unpacked);
         Assert.AreEqual(((long?) unpacked).Value, 100L);
      }

      private object Unpack<T>(object data) {
         return Unpack(typeof(T), data);
      }

      private object Unpack(Type targetType, object data) {
         // TODO : ideally we should only test dispatching logic, not the unpacker that is being dispatched to.
         return new UnpackerDispatcher().Unpack(targetType, new ObjectLuaVariant(data), "root");
      }
   }
}
