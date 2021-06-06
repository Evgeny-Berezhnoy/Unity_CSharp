using System;

namespace Berezhnoy.Dialogue.Serializable
{

    [Serializable]
    public class DialogueNodeLinkData
    {

        public string BaseNodeGUID;
        public int BaseNodePortIndex;
        public string TargetNodeGUID;

    }

}
