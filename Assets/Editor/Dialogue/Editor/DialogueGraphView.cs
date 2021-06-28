using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using Berezhnoy.Dialogue.Serializable;

namespace Berezhnoy.Dialogue.Editor
{
    public class DialogueGraphView : GraphView
    {

        #region Constants

        static readonly public Vector2 NodeDefaultSize = new Vector2(100f, 150f);

        static readonly public Rect NodeDefaultPosition = new Rect(Vector2.zero, NodeDefaultSize);

        static readonly public Rect BlackboardDefaultPosition = new Rect(10, 30, 200, 300);

        #endregion

        #region Fields

        private NodeSearchWindow nodeSearchWindow;

        public List<ExposedProperty> exposedProperties = new List<ExposedProperty>();

        public Blackboard targetBlackboard;

        #endregion

        #region Constructors

        public DialogueGraphView(DialogueGraph TargetDialogueGraph)
        {

            // Stylesheets
            styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));

            // Grid
            var grid = new GridBackground();

            Insert(0, grid);

            grid.StretchToParentSize();

            // Zoom
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // Cursor manipulators
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // Entry node
            CreateNode(GenerateEntryPointNode());

            // Search window
            AddSearchWindow(TargetDialogueGraph);

        }

        #endregion

        #region Methods

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {

            var compatiblePorts = new List<Port>();

            ports.ForEach((port) =>
            {

                if (startPort != port
                    && startPort.node != port.node)
                {

                    compatiblePorts.Add(port);

                };

            });

            return compatiblePorts;

        }

        private Port GeneratePort(DialogueNode dialogueNode, Direction portDirection, Port.Capacity portCapacity = Port.Capacity.Single)
        {

            return dialogueNode.InstantiatePort(Orientation.Horizontal, portDirection, portCapacity, typeof(float));

        }

        public DialogueNode GenerateEntryPointNode()
        {

            // Create a new node
            var dialogueNode = new DialogueNode
            {

                title = "ENTRY",

                GUID = Guid.NewGuid().ToString(),

                DialogueText = "ENTRY POINT",

                EntryPoint = true

            };

            // USS
            dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

            // Ports
            var OutputPort = GeneratePort(dialogueNode, Direction.Output);

            OutputPort.portName = "Next";

            dialogueNode.outputContainer.Add(OutputPort);

            dialogueNode.capabilities -= Capabilities.Movable;
            dialogueNode.capabilities -= Capabilities.Deletable;

            dialogueNode.RefreshExpandedState();
            dialogueNode.RefreshPorts();

            // Position
            dialogueNode.SetPosition(new Rect(new Vector2(100, 200), NodeDefaultSize));

            return dialogueNode;

        }

        public DialogueNode CreateDialogueNode(string nodeName)
        {

            // Create a new node
            var dialogueNode = new DialogueNode
            {

                title = nodeName,

                GUID = Guid.NewGuid().ToString(),

                DialogueText = nodeName

            };

            // USS
            dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

            // Port
            var InputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);

            InputPort.portName = "Input";

            dialogueNode.inputContainer.Add(InputPort);

            // Button
            var Button_AddChoice = new Button(() => { AddChoicePort(dialogueNode); });

            Button_AddChoice.text = "Add Choice";

            dialogueNode.titleContainer.Add(Button_AddChoice);

            //
            var textField = new TextField(string.Empty);

            textField.RegisterValueChangedCallback(evt =>
            {

                dialogueNode.DialogueText = evt.newValue;
                dialogueNode.title = evt.newValue;

            });

            textField.SetValueWithoutNotify(dialogueNode.title);

            dialogueNode.mainContainer.Add(textField);

            // Refresh
            dialogueNode.RefreshExpandedState();
            dialogueNode.RefreshPorts();
            dialogueNode.SetPosition(NodeDefaultPosition);

            return dialogueNode;

        }

        public DialogueNode CreateDialogueNode(string nodeName, string guid, Rect position)
        {

            var dialogueNode = CreateDialogueNode(nodeName); // basing on method

            if (!string.IsNullOrEmpty(guid))
            {

                dialogueNode.GUID = guid;

            };

            dialogueNode.SetPosition(position);

            return dialogueNode;

        }

        public void CreateNode(DialogueNode node)
        {

            AddElement(node);

        }

        public void AddChoicePort(DialogueNode dialogueNode, string overriddenPortName = "")
        {

            // Port itself
            var OutputPort = GeneratePort(dialogueNode, Direction.Output);

            var OutputPortLabel = OutputPort.contentContainer.Q<Label>("type");

            OutputPort.contentContainer.Remove(OutputPortLabel);

            var OutputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;

            OutputPort.portName = string.IsNullOrEmpty(overriddenPortName) ? $"Choice {OutputPortCount + 1}" : overriddenPortName;

            OutputPort.contentContainer.Add(new Label("   "));

            // Textfield related to the port
            var portTextField = new TextField()
            {

                name = overriddenPortName,
                value = OutputPort.portName

            };

            portTextField.RegisterValueChangedCallback(evt => OutputPort.portName = evt.newValue);

            OutputPort.contentContainer.Add(portTextField);

            // Remove port button
            var DeleteButton = new Button(() => RemovePort(dialogueNode, OutputPort))
            {

                text = "X"

            };

            OutputPort.contentContainer.Add(DeleteButton);

            // Adding port to dialogue node
            dialogueNode.outputContainer.Add(OutputPort);

            dialogueNode.RefreshExpandedState();
            dialogueNode.RefreshPorts();

        }

        public void RemovePort(DialogueNode dialogueNode, Port OutputPort)
        {

            // First - delete edge
            var OutputPortEdges = edges.ToList().Where(x => x.output.node == OutputPort.node && x.output.portName == OutputPort.portName);

            if (OutputPortEdges.Any())
            {

                var OutputPortEdge = OutputPortEdges.First();

                OutputPortEdge.input.Disconnect(OutputPortEdge);
                OutputPortEdge.output.Disconnect(OutputPortEdge);

                RemoveElement(OutputPortEdge);

            };

            // Delete output port
            dialogueNode.outputContainer.Remove(OutputPort);

            dialogueNode.RefreshPorts();
            dialogueNode.RefreshExpandedState();

        }

        private void AddSearchWindow(DialogueGraph TargetDialogueGraph)
        {

            nodeSearchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();

            nodeSearchWindow.Init(TargetDialogueGraph, this);

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), nodeSearchWindow);

        }

        public void AddPropertyToBlackboard(ExposedProperty exposedProperty)
        {

            // Unique property name creation
            var localPropertyName   = ExposedProperty.GetUniquePropertyName(exposedProperties, exposedProperty);
            var localPropertyValue  = exposedProperty.PropertyValue;

            // Create exposed property
            var property = new ExposedProperty();

            property.PropertyName = localPropertyName;
            property.PropertyValue = localPropertyValue;

            exposedProperties.Add(property);

            // Create container with a blackboard field
            var container = new VisualElement();
            var blackboardField = new BlackboardField(){

                text = property.PropertyName,
                typeText = "string property"

            };

            container.Add(blackboardField);

            // Creating a textfield
            var propertyValueTextField = new TextField("Value:")
            {

                value = localPropertyValue

            };

            propertyValueTextField.RegisterValueChangedCallback(str =>
            {

                var changingPropertyIndex = exposedProperties.FindIndex(x => x.PropertyName == property.PropertyName);

                exposedProperties[changingPropertyIndex].PropertyValue = str.newValue;

            });

            var blackboardValueRow = new BlackboardRow(blackboardField, propertyValueTextField);

            container.Add(blackboardValueRow);

            targetBlackboard.Add(container);

        }

        #endregion

    }

}
