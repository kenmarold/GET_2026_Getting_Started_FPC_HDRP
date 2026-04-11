using UnityEngine;
using TMPro;

public class NPCDialogueBubble : MonoBehaviour
{
    [SerializeField] private GameObject bubbleRoot; // the canvas root
    [SerializeField] private TMP_Text bubbleText;

    public void Show(string text)
    {
        bubbleRoot.SetActive(true);
        bubbleText.text = text;
    }

    public void Hide()
    {
        bubbleRoot.SetActive(false);
    }
}