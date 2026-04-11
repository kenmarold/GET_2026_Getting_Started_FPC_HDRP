using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Input")]
    [SerializeField] private Key advanceKey = Key.E;

    [Header("Auto Advance")]
    [SerializeField] private bool autoAdvance = true;
    [SerializeField] private float autoAdvanceSeconds = 2f;

    private string[] currentLines;
    private int index;
    private bool isActive;

    private NPCDialogueBubble currentBubble;
    private NPCDialogueTrigger currentOwner;

    private Coroutine autoRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (!isActive) return;
        if (Keyboard.current == null) return;

        // Option B: manual advance is allowed even with auto-advance on
        if (AdvancePressedThisFrame())
        {
            NextLine();
        }
    }

    public bool AdvancePressedThisFrame()
    {
        if (Keyboard.current == null) return false;

        return Keyboard.current[advanceKey].wasPressedThisFrame ||
               Keyboard.current.spaceKey.wasPressedThisFrame ||
               Keyboard.current.enterKey.wasPressedThisFrame;
    }

    public void StartDialogue(string[] lines, NPCDialogueBubble bubble, NPCDialogueTrigger owner)
    {
        if (lines == null || lines.Length == 0) return;
        if (bubble == null) return;

        currentLines = lines;
        currentBubble = bubble;
        currentOwner = owner;

        index = 0;
        isActive = true;

        currentBubble.Show(currentLines[index]);

        if (autoRoutine != null) StopCoroutine(autoRoutine);
        if (autoAdvance) autoRoutine = StartCoroutine(AutoAdvanceRoutine());
    }

    private IEnumerator AutoAdvanceRoutine()
    {
        while (isActive)
        {
            yield return new WaitForSeconds(autoAdvanceSeconds);
            if (!isActive) yield break;
            NextLine();
        }
    }

    private void NextLine()
    {
        index++;

        if (index >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        currentBubble.Show(currentLines[index]);
    }

    private void EndDialogue()
    {
        isActive = false;

        if (autoRoutine != null)
        {
            StopCoroutine(autoRoutine);
            autoRoutine = null;
        }

        currentBubble?.Hide();

        currentLines = null;
        currentBubble = null;
        currentOwner = null;
    }

    public void ForceEndDialogue(NPCDialogueTrigger owner)
    {
        if (!isActive) return;
        if (currentOwner != owner) return;
        EndDialogue();
    }

    public bool IsDialogueActive() => isActive;
}