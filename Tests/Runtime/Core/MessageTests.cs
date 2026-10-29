using NUnit.Framework;
using Unity.AppUI.Core;

namespace Unity.AppUI.Tests.Core
{
    [TestFixture]
    [TestOf(typeof(Message))]
    class MessageTests
    {
        [Test]
        [TestCase(1, 0, null)]
        [TestCase(2, 1, "")]
        [TestCase(3, 2, "a text")]
        [TestCase(3, 2, 42)]
        public void Message_Obtain_ShouldSetPropertiesProperly(int id, int arg1, object obj)
        {
            var msg = Message.Obtain(null, id, arg1, obj);
            Assert.AreEqual(id, msg.what);
            Assert.AreEqual(arg1, msg.arg1);
            Assert.AreEqual(obj, msg.obj);
            Assert.IsNull(msg.target);
            Assert.DoesNotThrow(msg.Recycle);
        }

        [Test]
        public void Message_Obtain_ShouldReuseRecycledMessage_AfterManyRecycles()
        {
            // More cycles than the pool's maximum size, so the pool counter would saturate
            // if Obtain did not give the slot back.
            for (var i = 0; i < 100; i++)
            {
                Message.Obtain(null, i, null).Recycle();
            }

            var first = Message.Obtain(null, 1, null);
            first.Recycle();
            var second = Message.Obtain(null, 2, null);
            Assert.AreSame(first, second);
            second.Recycle();
        }
    }
}
