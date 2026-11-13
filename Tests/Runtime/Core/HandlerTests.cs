using System;
using System.Collections;
using NUnit.Framework;
using Unity.AppUI.Core;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.Core
{
    [TestFixture]
    [TestOf(typeof(Handler))]
    [PrebuildSetup(typeof(ThemeResourceSetup))]
    [PostBuildCleanup(typeof(ThemeResourceSetup))]
    class HandlerTests
    {
        UIDocument m_TestUI;

        Looper m_Looper;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            m_TestUI = Utils.ConstructTestUI();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            m_Looper?.Quit();

            if (m_TestUI)
                UnityEngine.Object.Destroy(m_TestUI.gameObject);

            m_TestUI = null;
        }

        [Test, Order(1)]
        public void Handler_Dependencies_CanBeCreated()
        {
            Assert.DoesNotThrow(() => m_Looper = new Looper(m_TestUI.rootVisualElement));
            Assert.IsNotNull(m_Looper);
            m_Looper.Loop();
        }

        [Test, Order(2)]
        public void Handler_Constructor_DoesNotThrow()
        {
            Handler handler = null;
            Assert.DoesNotThrow(() => handler = new Handler(m_Looper, Callback));
            Assert.IsNotNull(handler);
        }

        [Test, Order(2)]
        public void Handler_Constructor_ThrowsWithNullArgs()
        {
            Handler handler = null;
            Assert.Throws<ArgumentNullException>(() => handler = new Handler(m_Looper, null));
            Assert.Throws<ArgumentNullException>(() => handler = new Handler(null, Callback));
            Assert.IsNull(handler);
        }

        [UnityTest, Order(3)]
        public IEnumerator Handler_SendMessage_CanHandleReceivedMessage([Values(1, 2, 1234)] int messageId)
        {
            var handled = false;
            var handler = new Handler(m_Looper, message =>
            {
                if (message.what == messageId)
                {
                    handled = true;
                    return true;
                }
                return false;
            });
            handler.SendMessage(handler.ObtainMessage(messageId, null));

            yield return new WaitUntilOrTimeOut(() => handled);

            Assert.IsTrue(handled);
        }

        [UnityTest, Order(3)]
        public IEnumerator Handler_SendMessageDelayed_CanHandleReceivedMessage([Values(1, 2, 1234)] int messageId, [Values(16, 100)] int delay)
        {
            var handled = false;
            var handler = new Handler(m_Looper, message =>
            {
                if (message.what == messageId)
                {
                    handled = true;
                    return true;
                }
                return false;
            });
            handler.SendMessageDelayed(handler.ObtainMessage(messageId, null), delay);

            yield return new WaitUntilOrTimeOut(() => handled);

            Assert.IsTrue(handled);
        }

        [Test]
        public void Handler_RemoveCallbacksAndMessages_OnlyRemovesOwnMessages([Values(false, true)] bool useToken)
        {
            var looper = new Looper();
            var h1 = new Handler(looper, Callback);
            var h2 = new Handler(looper, Callback);
            var token = useToken ? new object() : null;
            h1.SendMessage(h1.ObtainMessage(1, token));
            h2.SendMessage(h2.ObtainMessage(2, token));

            h1.RemoveCallbacksAndMessages(token);

            Assert.AreEqual(1, looper.queue.Count);
            Assert.IsTrue(looper.queue.TryPeek(out var left));
            Assert.AreEqual(2, left.what);
        }

        [Test]
        public void Handler_RemoveCallbacksAndMessages_WithoutOwnMessages_KeepsOtherHandlersMessages()
        {
            var looper = new Looper();
            var h1 = new Handler(looper, Callback);
            var h2 = new Handler(looper, Callback);
            h2.SendMessage(h2.ObtainMessage(2, null));

            h1.RemoveCallbacksAndMessages(null);

            Assert.AreEqual(1, looper.queue.Count);
        }

        [Test]
        public void Handler_RemoveCallbacksAndMessages_KeepsMessagesPostedConcurrentlyByOtherHandlers()
        {
            const int count = 2000;
            var looper = new Looper();
            var h1 = new Handler(looper, Callback);
            var h2 = new Handler(looper, Callback);
            var messages = new Message[count];
            for (var i = 0; i < count; i++)
                messages[i] = h2.ObtainMessage(2, null);

            var poster = new System.Threading.Thread(() =>
            {
                foreach (var message in messages)
                    h2.SendMessage(message);
            });
            poster.Start();
            while (poster.IsAlive)
            {
                h1.RemoveCallbacksAndMessages(null);
                System.Threading.Thread.Yield();
            }
            poster.Join();

            Assert.AreEqual(count, looper.queue.Count);
        }

        [Test]
        public void Handler_RemoveCallbacksAndMessages_WhileLoopConsumes_NeverRestoresHandledMessages()
        {
            const int count = 2000;
            var looper = new Looper();
            looper.Loop();
            var handled = 0;
            var h1 = new Handler(looper, Callback);
            var h2 = new Handler(looper, _ =>
            {
                handled++;
                return true;
            });
            var stop = false;
            var filter = new System.Threading.Thread(() =>
            {
                while (!System.Threading.Volatile.Read(ref stop))
                {
                    h1.RemoveCallbacksAndMessages(null);
                    System.Threading.Thread.Yield();
                }
            });
            filter.Start();
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var sent = 0;
                while (sent < count && watch.ElapsedMilliseconds < 5000)
                {
                    for (var i = 0; i < 10; i++, sent++)
                        h2.SendMessage(h2.ObtainMessage(2, null));
                    looper.LoopOnce();
                }
                while (looper.queue.Count > 0 && watch.ElapsedMilliseconds < 5000)
                    looper.LoopOnce();
            }
            finally
            {
                System.Threading.Volatile.Write(ref stop, true);
                filter.Join();
            }

            looper.LoopOnce();
            Assert.AreEqual(count, handled);
            Assert.AreEqual(0, looper.queue.Count);
        }

        bool Callback(Message arg)
        {
            return false;
        }
    }
}
