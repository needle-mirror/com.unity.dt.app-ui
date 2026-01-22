using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Test data source for GridView binding tests.
    /// </summary>
    public class GridViewDataSource : ScriptableObject
    {
        public int columnCountValue = 1;
        public int selectedIndexValue = -1;
        public int selectionCountValue = 0;
        public float itemWidthValue = 0f;
    }
}
