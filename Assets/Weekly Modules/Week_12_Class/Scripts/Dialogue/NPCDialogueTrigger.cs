using System.Collections;
using UnityEngine;

public class NPCDialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private bool onlyOnce = false;

    [Header("Greeting → Auto-start conversation")]
    [SerializeField] private float autoStartMin = 0.8f;
    [SerializeField] private float autoStartMax = 1.2f;

    private bool hasTriggered;
    private bool playerInside;
    private bool conversationStarted;

    private NPCDialogueBubble bubble;
    private NPCPulseGlow pulseGlow;

    private int lastGreetingIndex = -1;
    private Coroutine autoStartRoutine;

    private void Awake()
    {
        bubble = GetComponentInChildren<NPCDialogueBubble>(true);
        pulseGlow = GetComponentInChildren<NPCPulseGlow>(true);
    }

    private void Update()
    {
        if (!playerInside) return;
        if (conversationStarted) return;
        if (DialogueManager.Instance == null) return;

        // Press to start conversation early (during greeting wait)
        if (DialogueManager.Instance.AdvancePressedThisFrame())
        {
            TryStartConversation();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (onlyOnce && hasTriggered) return;

        if (bubble == null)
        {
            Debug.LogWarning($"No NPCDialogueBubble found on {name}.");
            return;
        }

        hasTriggered = true;
        playerInside = true;
        conversationStarted = false;

        // Start pulsing glow when player enters range
        pulseGlow?.StartPulse();

        // Show random greeting immediately
        ShowRandomGreeting();

        // Start auto-start timer for conversation
        if (autoStartRoutine != null) StopCoroutine(autoStartRoutine);
        autoStartRoutine = StartCoroutine(AutoStartAfterDelay());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        conversationStarted = false;

        // Stop pulsing glow when player leaves range
        pulseGlow?.StopPulse();

        if (autoStartRoutine != null)
        {
            StopCoroutine(autoStartRoutine);
            autoStartRoutine = null;
        }

        // End dialogue if this NPC was speaking
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.ForceEndDialogue(this);

        bubble?.Hide();
    }

    private IEnumerator AutoStartAfterDelay()
    {
        float delay = Random.Range(autoStartMin, autoStartMax);
        yield return new WaitForSeconds(delay);

        TryStartConversation();
    }

    private void TryStartConversation()
    {
        if (!playerInside) return;
        if (conversationStarted) return;
        if (DialogueManager.Instance == null) return;

        // Don't interrupt another NPC's dialogue
        if (DialogueManager.Instance.IsDialogueActive()) return;

        // IMPORTANT: this must match your DialogueData fields
        if (dialogue == null || dialogue.conversation == null || dialogue.conversation.Length == 0)
            return;

        conversationStarted = true;

        if (autoStartRoutine != null)
        {
            StopCoroutine(autoStartRoutine);
            autoStartRoutine = null;
        }

        // ✅ FIX: pass string[] lines (not DialogueData)
        DialogueManager.Instance.StartDialogue(dialogue.conversation, bubble, this);
    }

    private void ShowRandomGreeting()
    {
        if (dialogue == null || dialogue.greetings == null || dialogue.greetings.Length == 0)
            return;

        int index;

        if (dialogue.greetings.Length == 1)
        {
            index = 0;
        }
        else
        {
            do
            {
                index = Random.Range(0, dialogue.greetings.Length);
            } while (index == lastGreetingIndex);
        }

        lastGreetingIndex = index;
        bubble.Show(dialogue.greetings[index]);
    }
}