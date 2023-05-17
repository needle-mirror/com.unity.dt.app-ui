using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(FloatField))]
    class FloatFieldTests : NumericalFieldTests<FloatField, float>
    {
        protected override string mainUssClassName => FloatField.ussClassName;
    }
}
