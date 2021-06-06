using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Berezhnoy.Dialogue.Editor;

namespace Berezhnoy.Dialogue.Serializable
{


    [Serializable]
    public class DialogueNodeData
    {

        #region Fields

        public string GUID;
        public string DialogueText;

        public List<DialogueNodePort> OutputPorts;

        public Rect Position;

        #endregion

        #region Constructors

        public DialogueNodeData(string guid, string dialogueText, Rect position)
        {

            GUID = guid;
            DialogueText = dialogueText;
            Position = position;

            OutputPorts = new List<DialogueNodePort>();

        }

        public DialogueNodeData(DialogueNode NodeInstance)
        {

            GUID = NodeInstance.GUID;
            DialogueText = NodeInstance.DialogueText;
            Position = NodeInstance.GetPosition();

            OutputPorts = new List<DialogueNodePort>();

            var OutputPortCount = NodeInstance.outputContainer.Query("connector").ToList().Count;

            for (int i = 0; i < OutputPortCount; i++)
            {

                var nodePort = (Port)NodeInstance.outputContainer[i];

                OutputPorts.Add(new DialogueNodePort()
                {

                    PortName = nodePort.portName,
                    PortIndex = i

                });

            };

        }

        #endregion

    }

}
