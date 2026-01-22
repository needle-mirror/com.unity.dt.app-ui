using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.Core
{
    [TestFixture]
    [TestOf(typeof(Platform))]
    public class PlatformTests
    {
        static byte[] GetDataForType(PasteboardType type)
        {
            switch (type)
            {
                case PasteboardType.Text:
                    return System.Text.Encoding.UTF8.GetBytes("Hello, World!");
                case PasteboardType.PNG:
                    var tex = new Texture2D(1, 1);
                    tex.SetPixel(0, 0, Color.red);
                    tex.Apply();
                    return tex.EncodeToPNG();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        static void CheckPasteboardData(PasteboardType type, byte[] data)
        {
            switch (type)
            {
                case PasteboardType.Text:
                    Assert.AreEqual("Hello, World!", data);
                    break;
                case PasteboardType.PNG:
                    var tex = new Texture2D(1, 1);
                    tex.LoadImage(data);
                    Assert.AreEqual(Color.red, tex.GetPixel(0, 0));
                    Assert.AreEqual(1, tex.width);
                    Assert.AreEqual(1, tex.height);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        [Test]
        [TestCase(PasteboardType.Text)]
        [TestCase(PasteboardType.PNG)]
        public void CanGetAndSetPasteboardData(PasteboardType type)
        {
            // create data and set it to the pasteboard
            var data = GetDataForType(type);
            Assert.DoesNotThrow(() => Platform.SetPasteboardData(type, data));

            // check if the pasteboard has data
            var hasData = Platform.HasPasteboardData(type);
            Assert.IsTrue(hasData);

            // get the data from the pasteboard and check if the data is the same as the original data
            var pasteboardData = Platform.GetPasteboardData(type);
            CheckPasteboardData(type, pasteboardData);
        }

        [UnityTest]
        public IEnumerator CanGetAndSetPasteboardDataAsync()
        {
            // create text data
            var data = GetDataForType(PasteboardType.Text);

            // set it to the pasteboard asynchronously
            var setTask = Platform.SetPasteboardDataAsync(PasteboardType.Text, data);
            yield return new WaitUntil(() => setTask.IsCompleted);

            // get the data asynchronously
            var getTask = Platform.GetPasteboardDataAsync(PasteboardType.Text);
            yield return new WaitUntil(() => getTask.IsCompleted);

            // check if the data is correct
            var pasteboardData = getTask.Result;
            CheckPasteboardData(PasteboardType.Text, pasteboardData);
        }

        [UnityTest]
        public IEnumerator HasPasteboardDataAsyncWorks()
        {
            // set data first
            var data = GetDataForType(PasteboardType.Text);
            var setTask = Platform.SetPasteboardDataAsync(PasteboardType.Text, data);
            yield return new WaitUntil(() => setTask.IsCompleted);

            // check if data exists asynchronously
            var hasDataTask = Platform.HasPasteboardDataAsync(PasteboardType.Text);
            yield return new WaitUntil(() => hasDataTask.IsCompleted);

            Assert.IsTrue(hasDataTask.Result);
        }

        [UnityTest]
        public IEnumerator AsyncMethodsReturnTasks()
        {
            var data = GetDataForType(PasteboardType.Text);

            // Verify that async methods return Task objects
            var hasDataTask = Platform.HasPasteboardDataAsync(PasteboardType.Text);
            Assert.IsNotNull(hasDataTask);

            var getTask = Platform.GetPasteboardDataAsync(PasteboardType.Text);
            Assert.IsNotNull(getTask);

            var setTask = Platform.SetPasteboardDataAsync(PasteboardType.Text, data);
            Assert.IsNotNull(setTask);

            // Wait for completion
            yield return new WaitUntil(() => setTask.IsCompleted && getTask.IsCompleted && hasDataTask.IsCompleted);
        }
    }
}
