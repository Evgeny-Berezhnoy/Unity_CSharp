using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Berezhnoy.Dialogue.Editor
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {

        #region Fields

        private DialogueGraphView targetGraphView;

        private DialogueGraph targetGraph;

        private Texture2D indentationIcon;

        #endregion

        #region Methods

        public void Init(DialogueGraph Graph, DialogueGraphView GraphView)
        {

            targetGraphView = GraphView;

            targetGraph = Graph;

            // Indentation hack for search window as a transparent icon 
            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0)); // Don't forget to set the alpha to 0 as well
            indentationIcon.Apply();

        }

        #endregion

        #region ISearchWindowProvider Methods

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {

            var tree = new List<SearchTreeEntry>()
        {

            new SearchTreeGroupEntry(new GUIContent("Create Elements"), 0),
            new SearchTreeGroupEntry(new GUIContent("Dialogue"), 1),
            new SearchTreeEntry(new GUIContent("Dialogue Node", indentationIcon))
            {

                userData = new DialogueNode(),
                level = 2

            }

        };

            return tree;

        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {

            // Get mouse position
            var worldMousePosition = targetGraph.rootVisualElement.ChangeCoordinatesTo(targetGraph.rootVisualElement.parent,
                                                                                        context.screenMousePosition - targetGraph.position.position);

            var localMousePosition = targetGraphView.contentViewContainer.WorldToLocal(worldMousePosition);

            // Check user data and execute actions depending on the type
            switch (SearchTreeEntry.userData)
            {

                case DialogueNode dialogueNode:

                    dialogueNode = targetGraphView.CreateDialogueNode("Dialogue node", "", new Rect(localMousePosition, DialogueGraphView.NodeDefaultSize));

                    targetGraphView.CreateNode(dialogueNode);

                    return true;

                default:
                    return false;

            };

        }

        #endregion

    }

}
