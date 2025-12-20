using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class NPCBehaviorManager : MonoBehaviour
{
    [Header("Referencias de Sistemas")]
    [Tooltip("El manager de dialogos para leer las Flags (si se elige llevarlo).")]
    [SerializeField] private NPCDialogueDataManager dialogueDataManager;

    [Tooltip("Arrastra aqui tu DialogueCameraController de la escena para la cinematica de la puerta.")]
    [SerializeField] private DialogueCameraController dialogueCamera;

    [Tooltip("Arrastra aqui el componente NPCPopupBillboard del NPC (para que hable al correr).")]
    [SerializeField] private NPCPopupBillboard npcPopup;

    [Header("Configuracion de Velocidades")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 6f;
    public float crouchSpeed = 2f;
    public float stoppingDistance = 2.0f;

    [Header("Audio")]
    [SerializeField] private AudioClip[] footstepSounds;
    private AudioSource audioSource;

    [Header("Flags de Dialogo")]
    [Tooltip("Flag que activa el seguimiento al Player 1.")]
    public string flagFollowP1 = "NPC_Follows_P1";
    [Tooltip("Flag que activa el seguimiento al Player 2.")]
    public string flagFollowP2 = "NPC_Follows_P2";

    
    private NavMeshAgent agent;
    private Animator animator;
    private MonoBehaviour currentLeaderScript;
    private Transform currentLeaderTransform;

    
    private bool isRunningToExit = false;
    private bool isFollowing = false;

    
    public bool IsFollowing => isFollowing;
    
    public Transform CurrentLeaderTransform => currentLeaderTransform;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; 
            audioSource.playOnAwake = false;
        }
        
        agent.stoppingDistance = stoppingDistance;

        
        if (dialogueDataManager == null)
            dialogueDataManager = FindObjectOfType<NPCDialogueDataManager>();

        if (dialogueCamera == null)
            dialogueCamera = FindObjectOfType<DialogueCameraController>();

        if (npcPopup == null)
            npcPopup = GetComponentInChildren<NPCPopupBillboard>();
    }

    private void Update()
    {
        
        if (isRunningToExit)
        {
            UpdateAnimation(runSpeed, false, true);

            if (!agent.pathPending && agent.remainingDistance <= 0.5f)
            {
                DesaparecerEnPuerta();
            }
            return;
        }

        
        if (!isFollowing && dialogueDataManager != null)
        {
            CheckDialogueFlags();
        }

        
        if (isFollowing && currentLeaderTransform != null)
        {
            SeguirLider();
        }
        else
        {
            UpdateAnimation(0, false, false);
        }
    }

    

    public void RunToDoorAndVanish(Transform doorExitPoint)
    {


        agent.ResetPath();
        agent.speed = runSpeed;
        agent.SetDestination(doorExitPoint.position);
        agent.isStopped = false;

        isRunningToExit = true;
        isFollowing = false;

        if (npcPopup != null)
            npcPopup.ShowMessage("Well done! See you upstairs!", 4f);

        if (dialogueCamera != null)
            dialogueCamera.StartDialogueCamera(this.transform, null);
    }

    private void DesaparecerEnPuerta()
    {
        if (dialogueCamera != null)
            dialogueCamera.EndDialogueCamera();

        gameObject.SetActive(false);
    }

    

    private void CheckDialogueFlags()
    {
        if (dialogueDataManager.HasFlag("Player1", flagFollowP1))
            AssignLeader(GameObject.FindGameObjectWithTag("Player1"));
        else if (dialogueDataManager.HasFlag("Player2", flagFollowP2))
            AssignLeader(GameObject.FindGameObjectWithTag("Player2"));
    }

    private void AssignLeader(GameObject playerObj)
    {
        if (playerObj == null) return;

        currentLeaderTransform = playerObj.transform;

        if (playerObj.GetComponent<MovJugador1>())
            currentLeaderScript = playerObj.GetComponent<MovJugador1>();
        else if (playerObj.GetComponent<MovJugador2>())
            currentLeaderScript = playerObj.GetComponent<MovJugador2>();

        isFollowing = true;
    }

    private void SeguirLider()
    {
        if (currentLeaderTransform == null) return;

        agent.SetDestination(currentLeaderTransform.position);

        bool leaderCrouching = false;
        bool leaderRunning = false;

        if (currentLeaderScript is MovJugador1 p1)
        {
            leaderCrouching = p1.IsCrouchingState;
            leaderRunning = p1.IsRunningState;
        }
        else if (currentLeaderScript is MovJugador2 p2)
        {
            leaderCrouching = p2.IsCrouchingState;
            leaderRunning = p2.IsRunningState;
        }

        if (leaderCrouching) agent.speed = crouchSpeed;
        else if (leaderRunning) agent.speed = runSpeed;
        else agent.speed = walkSpeed;

        UpdateAnimation(agent.velocity.magnitude, leaderCrouching, leaderRunning);
    }

    private void UpdateAnimation(float speed, bool isCrouching, bool isRunning)
    {
        
        
        
        float targetAnimSpeed = speed > 0.1f ? (isRunning ? 2f : 1f) : 0f;

        
        animator.SetFloat("Speed", targetAnimSpeed, 0.2f, Time.deltaTime);

        animator.SetBool("IsCrouching", isCrouching);
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsFollowing", isFollowing);
    }
    
    
    public void PlayFootstepSound()
    {
        if (footstepSounds != null && footstepSounds.Length > 0 && audioSource != null)
        {
            
            if (agent.velocity.magnitude > 0.1f)
            {
                AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
                audioSource.PlayOneShot(clip, 0.3f);
            }
        }
    }
}
