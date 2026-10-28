using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.Navigation;
using UnityEngine;

namespace Unity.AppUI.Tests.Navigation
{
    [TestFixture]
    [TestOf(typeof(NavGraphViewAsset))]
    class NavGraphViewAssetTests
    {
        readonly List<Object> m_Objects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in m_Objects)
                Object.DestroyImmediate(obj);
            m_Objects.Clear();
        }

        T Create<T>(string name) where T : ScriptableObject
        {
            var obj = ScriptableObject.CreateInstance<T>();
            obj.name = name;
            m_Objects.Add(obj);
            return obj;
        }

        NavGraphViewAsset CreateAsset(out NavGraph root)
        {
            var asset = Create<NavGraphViewAsset>("graph_asset");
            root = Create<NavGraph>("nav_graph");
            asset.AddNode(root);
            return asset;
        }

        NavDestination AddDestination(NavGraphViewAsset asset, NavGraph root, string name)
        {
            var destination = Create<NavDestination>(name);
            destination.parent = root;
            asset.AddNode(destination);
            return destination;
        }

        NavAction AddAction(NavGraphViewAsset asset, NavGraph root, string name)
        {
            var action = Create<NavAction>(name);
            root.actions.Add(action);
            asset.AddNode(action);
            return action;
        }

        [Test]
        public void FindDestinationByRoute_ReturnsNullWhenRouteNamesAnAction()
        {
            var asset = CreateAsset(out var root);
            root.startDestination = AddDestination(asset, root, "home");
            var settings = AddDestination(asset, root, "settings");
            AddAction(asset, root, "toSettings").destination = settings;

            Assert.IsNull(asset.FindDestinationByRoute("toSettings"));
        }

        [Test]
        public void FindDestinationByRoute_SkipsActionWithSameNameAsDestination()
        {
            var asset = CreateAsset(out var root);
            root.startDestination = AddDestination(asset, root, "home");
            var action = AddAction(asset, root, "settings");
            var settings = AddDestination(asset, root, "settings");
            action.destination = settings;

            Assert.AreSame(settings, asset.FindDestinationByRoute("settings"));
        }
    }
}
