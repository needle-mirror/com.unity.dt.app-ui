using System.Collections.Generic;
using Unity.AppUI.Navigation;
using UnityEngine;

namespace Unity.AppUI.VisualDoc
{
    static class VisualDocNavGraph
    {
        static NavGraphViewAsset s_Instance;

        public static NavGraphViewAsset instance => s_Instance ??= CreateGraphAsset();

        static NavGraphViewAsset CreateGraphAsset()
        {
            var graphAsset = ScriptableObject.CreateInstance<NavGraphViewAsset>();

            var rootGraph = ScriptableObject.CreateInstance<NavGraph>();

            var rootItems = DocTree.BuildTree();
            var items = TreeViewParser.GetAllItems(rootItems);
            foreach (var item in items)
            {
                if (item.IsBrowsable)
                    CreateDestinationAndAction(item, rootGraph, graphAsset);
            }

            graphAsset.AddNode(rootGraph);

            return graphAsset;
        }

        static void CreateDestinationAndAction(TreeViewItemModel item, NavGraph rootGraph, NavGraphViewAsset graphAsset)
        {
            var destination = ScriptableObject.CreateInstance<NavDestination>();
            destination.name = item.Name;
            destination.label = item.DisplayName;
            destination.destinationTemplate = new VisualDocPageDestinationTemplate();
            destination.parent = rootGraph;
            destination.arguments = new List<Argument>();
            if (item.IsBrowsable)
            {
                destination.arguments.Add(new Argument("pageKind", item.PageKind.ToString()));
                destination.arguments.Add(new Argument("pageRef", item.ContentRef));
                destination.arguments.Add(new VisualDocNavArgument("treeViewItem", item));
            }
            rootGraph.startDestination ??= destination;

            graphAsset.AddNode(destination);

            var action = ScriptableObject.CreateInstance<NavAction>();
            action.name = VisualDocActions.GoTo(item.Name);
            action.destination = destination;
            action.options = new NavOptions
            {
                launchSingleTop = true,
                popUpToStrategy = PopupToStrategy.CurrentStartDestination,
                restoreState = false,
                popUpToSaveState = false,
                popUpToInclusive = true
            };
            rootGraph.actions.Add(action);

            graphAsset.AddNode(action);
        }
    }
}
