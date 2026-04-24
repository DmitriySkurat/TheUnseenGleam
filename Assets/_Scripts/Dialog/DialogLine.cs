using UnityEngine;

public enum DialogSpeaker { Player, NPC }

[System.Serializable]
public class DialogLine
{
    public DialogSpeaker speaker;
    [TextArea(2, 5)] public string text;
}
