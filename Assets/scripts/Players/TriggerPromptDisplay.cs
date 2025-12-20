using UnityEngine;

public class SimpleTriggerPrompt : MonoBehaviour
{
    [Header("Canvas Reference")]
    [Tooltip("Tu canvas de World Space que quieres mostrar/ocultar")]
    [SerializeField] private GameObject promptCanvas;

    [Header("Trigger Settings")]
    [Tooltip("Solo se activa una vez")]
    [SerializeField] private bool triggerOnce = false;

    [Tooltip("Tags de jugadores que pueden activar este trigger")]
    [SerializeField] private string[] validPlayerTags = { "Player1", "Player2" };

    [Tooltip("Hacer billboard (que el canvas siempre mire a la cmara)")]
    [SerializeField] private bool useBillboard = true;

    private bool hasTriggered = false;

    void Start()
    {
        
        if (promptCanvas != null)
        {
            promptCanvas.SetActive(false);
        }

        
        Collider col = GetComponent<Collider>();
        if (col == null)
        {

            col = gameObject.AddComponent<BoxCollider>();
        }

        if (!col.isTrigger)
        {

            col.isTrigger = true;
        }
    }

    void Update()
    {
        
        if (useBillboard && promptCanvas != null && promptCanvas.activeSelf)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 lookPos = promptCanvas.transform.position + cam.transform.rotation * Vector3.forward;
                promptCanvas.transform.LookAt(lookPos, cam.transform.rotation * Vector3.up);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (triggerOnce && hasTriggered)
            return;

        
        if (!IsValidPlayer(other.gameObject))
            return;

        
        if (promptCanvas != null)
        {
            promptCanvas.SetActive(true);
        }

        
        hasTriggered = true;
    }

    private void OnTriggerExit(Collider other)
    {
        
        if (!IsValidPlayer(other.gameObject))
            return;

        
        if (promptCanvas != null)
        {
            promptCanvas.SetActive(false);
        }

        
        if (triggerOnce)
        {
            Destroy(gameObject);
        }
    }

    private bool IsValidPlayer(GameObject obj)
    {
        
        if (validPlayerTags == null || validPlayerTags.Length == 0)
            return true;

        
        foreach (string tag in validPlayerTags)
        {
            if (obj.CompareTag(tag))
                return true;
        }

        return false;
    }

    void OnDrawGizmos()
    {
        
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphere = col as SphereCollider;
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }
    }
}
