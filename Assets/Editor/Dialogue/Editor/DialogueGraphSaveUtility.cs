using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using Berezhnoy.Dialogue.Serializable;

namespace Berezhnoy.Dialogue.Editor
{
    public class DialogueGraphSaveLoadUtility
    {

        #region Constants

        static readonly private string DialogueFolder = "Dialogues";
        static readonly private string ResourcesDialogueFolder = $"Resources/{DialogueFolder}";
        static readonly private string AssetsResourcesDialogueFolder = $"Assets/{ResourcesDialogueFolder}";
        static readonly private string ResourcesDialogueFolder_Full = $"{Application.dataPath}/{ResourcesDialogueFolder}";

        #endregion

        #region Fields 

        private DialogueGraphView targetGraphView;

        private DialogueContainer dialogueContainerCache;

        private List<Edge> Edges => targetGraphView.edges.ToList();
        private List<DialogueNode> Nodes => targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

        #endregion

        #region Static methods

        public static DialogueGraphSaveLoadUtility GetInstance(DialogueGraphView TargetGraphView)
        {

            return new DialogueGraphSaveLoadUtility
            {

                targetGraphView = TargetGraphView

            };

        }

        #endregion

        #region Methods

        public void Save(string FileName)
        {

            var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();

            // Saving edges
            dialogueContainer.EntryNodeGUID = Nodes.Where(x => x.EntryPoint).ToList().ElementAt(0).GUID;

            var connectedPorts = Edges.Where((x) => (x.input.node != null)).ToList();

            connectedPorts.ForEach(EdgeInstance =>
            {

                var OutputNode = EdgeInstance.output.node as DialogueNode;
                var InputNode = EdgeInstance.input.node as DialogueNode;

                dialogueContainer.NodesLinks.Add(new DialogueNodeLinkData()
                {

                    BaseNodeGUID = OutputNode.GUID,
                    BaseNodePortIndex = OutputNode.outputContainer.IndexOf(EdgeInstance.output),
                    TargetNodeGUID = InputNode.GUID

                });

            });

            // Saving ports
            var connectedNodes = Nodes.Where((x) => (!x.EntryPoint)).ToList();

            connectedNodes.ForEach((NodeInstance) =>
            {

                dialogueContainer.NodesData.Add(new DialogueNodeData(NodeInstance));

            });

            // Saving exposed properies
            dialogueContainer.ExposedProperies.AddRange(targetGraphView.exposedProperties);


            // Checking if Resources Dialogue folder exists
            if (!Directory.Exists(ResourcesDialogueFolder_Full))
            {

                Directory.CreateDirectory(ResourcesDialogueFolder_Full);

            };

            // Immediate data save
            AssetDatabase.CreateAsset(dialogueContainer, $"{AssetsResourcesDialogueFolder}/{FileName}.asset");
            AssetDatabase.SaveAssets();

        }

        public void Load(string FileName)
        {

            dialogueContainerCache = Resources.Load<DialogueContainer>($"{DialogueFolder}/{FileName}");

            if (!dialogueContainerCache)
            {

                EditorUtility.DisplayDialog("File not found", "Target file graph file does not exist!", "OK");

                return;

            };

            ClearGraph();

            CreateNodes();

            ConnectNodes();

            CreateExposedProperies();

        }

        private void ClearGraph()
        {

            Nodes.Find(x => x.EntryPoint).GUID = dialogueContainerCache.EntryNodeGUID;

            foreach (DialogueNode dialogueNode in Nodes)
            {

                if (!dialogueNode.EntryPoint)
                {

                    Edges.Where(x => x.input.node == dialogueNode).ToList().ForEach(edge => targetGraphView.RemoveElement(edge));

                    targetGraphView.RemoveElement(dialogueNode);

                };

            };

        }

        private void CreateNodes()
        {

            foreach (var nodeData in dialogueContainerCache.NodesData)
            {

                // Adding node to graph
                var dialogueNode = targetGraphView.CreateDialogueNode(nodeData.DialogueText, nodeData.GUID, nodeData.Position);

                targetGraphView.AddElement(dialogueNode);

                // Adding output ports
                foreach (DialogueNodePort nodePort in nodeData.OutputPorts)
                {

                    targetGraphView.AddChoicePort(dialogueNode, nodePort.PortName);

                };

            };

        }

        private void ConnectNodes()
        {

            Nodes.ForEach(node_1 =>
            {

                Nodes.ForEach(node_2 =>
                {

                    var NodesConnections = dialogueContainerCache.NodesLinks.Where(x => x.BaseNodeGUID == node_1.GUID
                                                                                    && x.TargetNodeGUID == node_2.GUID).ToList();

                    if (NodesConnections.Any())
                    {

                        var node_2_Port = (Port)node_2.inputContainer[0];

                        NodesConnections.ForEach(nodeConnection =>
                        {

                            var node_1_Port = (Port)node_1.outputContainer[nodeConnection.BaseNodePortIndex];

                            var PortsEdge = new Edge
                            {

                                output = node_1_Port,
                                input = node_2_Port

                            };

                            PortsEdge.input.Connect(PortsEdge);
                            PortsEdge.output.Connect(PortsEdge);

                            targetGraphView.Add(PortsEdge);

                        });

                    }

                });

            });

        }

        private void CreateExposedProperies()
        {

            // Clear existing properties
            targetGraphView.exposedProperties.Clear();
            targetGraphView.targetBlackboard.Clear();

            //targetGraphView.targetBlackboard.

            // Adding properties from cache
            dialogueContainerCache.ExposedProperies.ForEach(exposedProperty =>
            {

                targetGraphView.AddPropertyToBlackboard(exposedProperty);

            });

        }

        #endregion
    
    }

}
