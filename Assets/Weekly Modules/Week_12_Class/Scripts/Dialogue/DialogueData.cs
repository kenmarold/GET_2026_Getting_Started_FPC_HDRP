using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Shown when you ENTER the trigger (random pick)")]
    [TextArea(1, 3)]
    public string[] greetings;

    [Header("Shown when you advance dialogue (full conversation)")]
    [TextArea(2, 4)]
    public string[] conversation;
}