using UnityEngine;

public class EndOfDemoTrigger : MonoBehaviour
{
    [Header("Player Detection")]
    [Tooltip("Tags to recognize as players")]
    [SerializeField] private string[] playerTags = new string[] { "Player1", "Player2", "Player" };

    [Header("Trigger Behavior")]
    [Tooltip("Hide this GameObject when triggered")]
    [SerializeField] private bool hideOnTrigger = true;

    [Tooltip("Disable the trigger collider after activation")]
    [SerializeField] private bool disableColliderOnTrigger = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
        {
            return;
        }

        if (IsPlayer(other.gameObject))
        {
            Debug.Log($"[EndOfDemoTrigger] Player detected: {other.gameObject.name}. Activating end screen...");
            hasTriggered = true;
            ActivateEndScreen();
        }
    }

    private bool IsPlayer(GameObject obj)
    {
        foreach (string tag in playerTags)
        {
            if (obj.CompareTag(tag))
            {
                Debug.Log($"[EndOfDemoTrigger] Matched player by tag: {tag}");
                return true;
            }
        }

        if (obj.GetComponent<MovJugador1>() != null || obj.GetComponent<MovJugador2>() != null)
        {
            Debug.Log("[EndOfDemoTrigger] Matched player by MovJugador component");
            return true;
        }

        if (obj.name.Contains("Player1") || obj.name.Contains("Player2"))
        {
            Debug.Log("[EndOfDemoTrigger] Matched player by name");
            return true;
        }

        return false;
    }

    private void ActivateEndScreen()
    {
        Debug.Log("[EndOfDemoTrigger] Activating end of demo sequence...");

        EndOfDemoController.Instance.ShowEndScreen();

        if (hideOnTrigger)
        {
            Debug.Log("[EndOfDemoTrigger] Hiding trigger GameObject...");
            gameObject.SetActive(false);
        }

        if (disableColliderOnTrigger)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Debug.Log("[EndOfDemoTrigger] Disabling trigger collider...");
                col.enabled = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = hasTriggered ? Color.red : Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
        }
    }
}
