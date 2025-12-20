using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerConversationTrigger : MonoBehaviour
{
    [System.Serializable]
    public struct ConversationLine
    {
        public enum Speaker { Player1, Player2, NPC }
        public Speaker speaker;
        [TextArea(3, 10)]
        public string message;
        public float duration;
        public float delayAfter;
    }

    [Header("Conversation Settings")]
    [Tooltip("Asigna aqui el NPC que participara en la conversacion (si lo hay).")]
    [SerializeField] private GameObject npcObject;
    [SerializeField] private List<ConversationLine> conversationLines = new List<ConversationLine>();
    [SerializeField] private bool playOnce = true;
    [SerializeField] private float initialDelay = 0.5f;

    private bool hasPlayed = false;
    private Coroutine conversationCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (hasPlayed && playOnce) return;

        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            
            
            if (conversationCoroutine == null)
            {
                conversationCoroutine = StartCoroutine(PlayConversation());
            }
        }
    }

    private IEnumerator PlayConversation()
    {
        hasPlayed = true;
        yield return new WaitForSeconds(initialDelay);

        GameObject p1 = DialogueManager.FindPlayerByTag("Player1");
        GameObject p2 = DialogueManager.FindPlayerByTag("Player2");

        foreach (var line in conversationLines)
        {
            GameObject speakerObj = null;

            switch (line.speaker)
            {
                case ConversationLine.Speaker.Player1:
                    speakerObj = p1;
                    break;
                case ConversationLine.Speaker.Player2:
                    speakerObj = p2;
                    break;
                case ConversationLine.Speaker.NPC:
                    speakerObj = npcObject;
                    break;
            }

            if (speakerObj != null)
            {
                DialogueManager.ShowPlayerMessage(speakerObj, line.message, line.duration);
            }

            
            yield return new WaitForSeconds(line.duration + line.delayAfter);
        }

        conversationCoroutine = null;
    }

    
    public void ResetConversation()
    {
        hasPlayed = false;
        if (conversationCoroutine != null)
        {
            StopCoroutine(conversationCoroutine);
            conversationCoroutine = null;
        }
    }
}
