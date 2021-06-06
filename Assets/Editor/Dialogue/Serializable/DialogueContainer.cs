using System;
using System.Collections.Generic;
using UnityEngine;

namespace Berezhnoy.Dialogue.Serializable
{

    [Serializable]
    public class DialogueContainer : ScriptableObject
    {

        public string EntryNodeGUID;

        public List<DialogueNodeLinkData> NodesLinks = new List<DialogueNodeLinkData>();

        public List<DialogueNodeData> NodesData = new List<DialogueNodeData>();

        public List<ExposedProperty> ExposedProperies = new List<ExposedProperty>();

    }

}
