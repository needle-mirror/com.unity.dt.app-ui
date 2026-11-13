#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="FloatingWindow"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="FloatingWindowTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(FloatingWindow))]
    class FloatingWindowSnapshotTests : UISnapshotTests<FloatingWindow>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Open", (ctx) => Build(true));
                yield return new Story("AutoSize", (ctx) =>
                {
                    var w = Build(true);
                    w.autoSize = true;
                    return w;
                });
            }
        }


        static FloatingWindow Build(bool open)
        {
            var window = new FloatingWindow
            {
                title = "Inspector",
                storage = new MemoryStorage(),
                storageKey = "test-window",
                defaultWidth = 360f,
                defaultHeight = 240f,
            };
            window.Add(new Text { text = "Content" });
            window.open = open;
            return window;
        }


        /// <summary>
        /// An in-memory <see cref="IWindowLayoutStorage"/>, so the tests never touch PlayerPrefs.
        /// </summary>
        class MemoryStorage : IWindowLayoutStorage
        {
            readonly Dictionary<string, string> m_Values = new Dictionary<string, string>();

            public int writes { get; private set; }

            public string GetItem(string key) => m_Values.TryGetValue(key, out var v) ? v : null;

            public void SetItem(string key, string value)
            {
                m_Values[key] = value;
                writes++;
            }

            public void RemoveItem(string key) => m_Values.Remove(key);
        }
    }
}
#endif
