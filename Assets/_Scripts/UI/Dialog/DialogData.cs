using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DialogData", menuName = "Dialog/Dialog Data")]
public class DialogData : ScriptableObject
{
    public string playerName = "Lian";
    public string npcName = "NPC";
    public Sprite playerSprite;
    public Sprite npcSprite;

    [Space]
    public List<DialogLine> lines = new();
}
