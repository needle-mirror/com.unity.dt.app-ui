using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.Core
{
    [TestFixture]
    [TestOf(typeof(ApplicationContext))]
    class ApplicationContextTests
    {
        [Test]
        public void ApplicationContext_Constructor_FromApplication_ShouldSucceed()
        {
            var app = new Panel();
            var ctx = new ApplicationContext(app);

            Assert.AreEqual(app.context, ctx);
        }

        [Test]
        [TestCase("de", "light", "medium")]
        [TestCase("fr", "light", "large")]
        [TestCase("en", "dark", "large")]
        [TestCase("de", "dark", "large")]
        public void ApplicationContext_Constructor_FromOverride_ShouldSucceed(string lang, string theme, string scale)
        {
            var app = new Panel();
            var ctx = new ApplicationContext(app);

            var provider = new ContextProvider { lang = lang, theme = theme, scale = scale };
            app.Add(provider);

            var subCtx = new ApplicationContext(ctx, provider);

            Assert.AreEqual(app, subCtx.panel);
            Assert.AreEqual(scale, subCtx.scale);
            Assert.AreEqual(lang, subCtx.lang);
            Assert.AreEqual(theme, subCtx.theme);
        }
    }
}
