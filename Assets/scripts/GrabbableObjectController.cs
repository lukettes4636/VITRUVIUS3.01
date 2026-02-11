using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(AudioSource))]
    public class GrabbableObjectController : MonoBehaviour
    {
        [Header("Input Settings")]
        public string ActionName = "Interact";
        public string ThrowActionName = "Throw";

        [Header("Interaction Settings")]
        public float GrabRadius = 2.0f;
        [Tooltip("Tiempo en segundos antes de poder agarrar el objeto después de soltarlo")]
        public float PickupCooldown = 0.3f;
        [Tooltip("Layer mask para detectar obstáculos entre el jugador y el objeto")]
        public LayerMask ObstacleLayer = -1;

        [Header("Posicion Base del Objeto")]
        public Vector3 HoldOffset = new Vector3(0.5f, 0.8f, 0.7f);
        public Vector3 HoldRotation = Vector3.zero;

        [Header("Posicion Agachado")]
        public Vector3 CrouchHoldOffset = new Vector3(0.5f, 0.45f, 0.7f);
        public Vector3 CrouchHoldRotation = new Vector3(15f, 0f, 0f);

        [Header("Suavizado")]
        public float positionSmoothSpeed = 10f;
        public LayerMask PlayerLayer = -1;

        [Header("Throw Settings")]
        public float ThrowForce = 12f;
        public float ThrowUpwardForce = 3f;
        public float ThrowAnimationDelay = 0.15f;
        public float VisualThrowDistance = 1.2f;

        [Header("Audio Settings")]
        public AudioClip CollisionSound;
        [Range(0f, 1f)] public float CollisionVolume = 1f;
        public float NoiseRange = 15f;
        public float MinCollisionForce = 2f;

        [Header("Outline Settings")]
        [SerializeField] private string outlineColorProperty = "_Outline_Color";
        [SerializeField] private string outlineScaleProperty = "_Outline_Scale";
        [SerializeField] private float activeOutlineScale = 0.0125f;
        [SerializeField] private Color cooperativeOutlineColor = Color.yellow;

        [Header("Debug")]
        public bool VerboseLogging = false;

        protected Rigidbody rb;
        protected Collider col;
        protected AudioSource audioSource;
        protected Transform originalParent;
        protected Transform currentHolder;
        protected bool isHeld = false;
        protected bool isThrowingInProgress = false;

        protected Collider currentHolderCollider;
        protected Animator holderAnimator;
        protected PlayerGrabProfile currentHolderProfile;

        // Sistema de cooldown
        private float lastDropTime = -999f;
        private Transform lastHolderTransform;

        private Renderer meshRenderer;
        private MaterialPropertyBlock propertyBlock;
        private int outlineColorID;
        private int outlineScaleID;
        private HashSet<GameObject> hoveringPlayers = new HashSet<GameObject>();

        public bool IsHeld => isHeld;
        public Transform CurrentHolder => currentHolder;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            audioSource = GetComponent<AudioSource>();
            originalParent = transform.parent;

            SetupAudioSource();
            SetupOutline();
        }

        private void SetupOutline()
        {
            meshRenderer = GetComponentInChildren<Renderer>();
            if (meshRenderer != null)
            {
                propertyBlock = new MaterialPropertyBlock();
                outlineColorID = Shader.PropertyToID(outlineColorProperty);
                outlineScaleID = Shader.PropertyToID(outlineScaleProperty);
                SetOutlineState(Color.black, 0f);
            }
        }

        protected void SetupAudioSource()
        {
            if (audioSource != null)
            {
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.2f;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.maxDistance = NoiseRange;
            }
        }

        protected virtual void Update()
        {
            if (isHeld) HandleHeldState();
            else HandleIdleState();
        }

        protected virtual void HandleIdleState()
        {
            // Verificar si el cooldown ha pasado
            float timeSinceLastDrop = Time.time - lastDropTime;
            bool cooldownActive = timeSinceLastDrop < PickupCooldown;

            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, GrabRadius, PlayerLayer);

            UpdateOutlineDetection(nearbyColliders);

            foreach (var nearbyCol in nearbyColliders)
            {
                PlayerInput playerInput = nearbyCol.GetComponentInParent<PlayerInput>();
                if (playerInput == null || playerInput.actions == null) continue;
                if (IsPlayerHoldingSomething(playerInput.transform)) continue;

                // NUEVO: Verificar si este jugador fue quien soltó el objeto recientemente
                if (cooldownActive && lastHolderTransform == playerInput.transform)
                {
                    if (VerboseLogging)
                        Debug.Log($"[GrabbableObject] {playerInput.name} intentó agarrar {gameObject.name} durante cooldown. Bloqueado.");
                    continue;
                }

                // NUEVO: Verificar que no hay paredes/obstáculos entre el jugador y el objeto
                if (!HasClearLineOfSight(playerInput.transform.position))
                {
                    if (VerboseLogging)
                        Debug.Log($"[GrabbableObject] {playerInput.name} no tiene línea de visión clara a {gameObject.name}");
                    continue;
                }

                InputAction interactAction = playerInput.actions.FindAction(ActionName);
                if (interactAction != null && interactAction.WasPerformedThisFrame())
                {
                    Pickup(playerInput.transform);
                    break;
                }
            }
        }

        /// <summary>
        /// Verifica que no hay obstáculos entre el jugador y el objeto
        /// </summary>
        private bool HasClearLineOfSight(Vector3 playerPosition)
        {
            Vector3 directionToObject = transform.position - playerPosition;
            float distanceToObject = directionToObject.magnitude;

            // Raycast desde el jugador hacia el objeto
            if (Physics.Raycast(playerPosition, directionToObject.normalized, out RaycastHit hit, distanceToObject, ObstacleLayer))
            {
                // Si el raycast golpeó algo antes de llegar al objeto, hay un obstáculo
                if (hit.collider.gameObject != gameObject)
                {
                    return false;
                }
            }

            // También hacer un SphereCast para objetos más grandes
            if (Physics.SphereCast(playerPosition, 0.1f, directionToObject.normalized, out RaycastHit sphereHit, distanceToObject, ObstacleLayer))
            {
                if (sphereHit.collider.gameObject != gameObject)
                {
                    return false;
                }
            }

            return true;
        }

        private void UpdateOutlineDetection(Collider[] nearbyColliders)
        {
            hoveringPlayers.Clear();

            foreach (var hit in nearbyColliders)
            {
                PlayerIdentifier pi = hit.GetComponentInParent<PlayerIdentifier>();
                if (pi != null)
                {
                    hoveringPlayers.Add(pi.gameObject);
                }
            }

            if (hoveringPlayers.Count == 0)
            {
                SetOutlineState(Color.black, 0f);
            }
            else if (hoveringPlayers.Count >= 2)
            {
                SetOutlineState(cooperativeOutlineColor, activeOutlineScale);
            }
            else
            {
                GameObject player = hoveringPlayers.First();
                PlayerIdentifier pi = player.GetComponent<PlayerIdentifier>();
                if (pi != null)
                {
                    SetOutlineState(pi.PlayerOutlineColor, activeOutlineScale);
                }
            }
        }

        private void SetOutlineState(Color color, float scale)
        {
            if (meshRenderer == null || propertyBlock == null) return;

            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(outlineColorID, color);
            propertyBlock.SetFloat(outlineScaleID, scale);

            meshRenderer.SetPropertyBlock(propertyBlock);
        }

        protected virtual void HandleHeldState()
        {
            if (currentHolder == null) { Drop(); return; }

            UpdateHeldPositionAndRotation();

            if (isThrowingInProgress) return;

            PlayerInput playerInput = currentHolder.GetComponent<PlayerInput>();
            if (playerInput == null) playerInput = currentHolder.GetComponentInParent<PlayerInput>();
            if (playerInput == null || playerInput.actions == null) return;

            InputAction interactAction = playerInput.actions.FindAction(ActionName);
            if (interactAction != null && interactAction.WasPerformedThisFrame())
            {
                Drop();
                return;
            }

            InputAction throwAction = playerInput.actions.FindAction(ThrowActionName);
            if (throwAction != null && throwAction.WasPerformedThisFrame())
            {
                StartCoroutine(ThrowSequence());
                return;
            }
        }

        protected void UpdateHeldPositionAndRotation()
        {
            Vector3 finalTargetOffset = HoldOffset;
            Vector3 finalTargetRotation = HoldRotation;

            if (holderAnimator != null && holderAnimator.GetBool("IsCrouching"))
            {
                finalTargetOffset = CrouchHoldOffset;
                finalTargetRotation = CrouchHoldRotation;
            }

            if (currentHolderProfile != null)
            {
                finalTargetOffset += currentHolderProfile.HoldPositionOffset;
                finalTargetRotation += currentHolderProfile.HoldRotationOffset;
            }

            transform.localPosition = Vector3.Lerp(transform.localPosition, finalTargetOffset, Time.deltaTime * positionSmoothSpeed);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(finalTargetRotation), Time.deltaTime * positionSmoothSpeed);
        }

        protected bool IsPlayerHoldingSomething(Transform playerTransform)
        {
            var heldObjects = playerTransform.GetComponentsInChildren<GrabbableObjectController>();
            return heldObjects.Any(obj => obj.isHeld && obj != this);
        }

        public virtual void Pickup(Transform holder)
        {
            isHeld = true;
            isThrowingInProgress = false;
            currentHolder = holder;

            // Resetear el cooldown cuando se agarra
            lastDropTime = -999f;
            lastHolderTransform = null;

            SetOutlineState(Color.black, 0f);
            hoveringPlayers.Clear();

            holderAnimator = holder.GetComponent<Animator>();
            if (holderAnimator == null) holderAnimator = holder.GetComponentInParent<Animator>();

            currentHolderProfile = holder.GetComponent<PlayerGrabProfile>();
            if (currentHolderProfile == null) currentHolderProfile = holder.GetComponentInParent<PlayerGrabProfile>();

            currentHolderCollider = holder.GetComponent<Collider>();
            if (currentHolderCollider == null) currentHolderCollider = holder.GetComponentInParent<Collider>();

            if (currentHolderCollider != null && col != null) Physics.IgnoreCollision(col, currentHolderCollider, true);

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            transform.SetParent(holder);

            Vector3 initialOffset = HoldOffset;
            if (currentHolderProfile != null) initialOffset += currentHolderProfile.HoldPositionOffset;

            transform.localPosition = initialOffset;
            transform.localEulerAngles = HoldRotation;

            var ikSystem = holder.GetComponent<ObjectCarryingSystem>();
            if (ikSystem == null) ikSystem = holder.GetComponentInParent<ObjectCarryingSystem>();

            if (ikSystem != null)
            {
                ikSystem.ApplyProfile(currentHolderProfile);
                ikSystem.StartCarrying(this.gameObject);
            }

            if (VerboseLogging)
                Debug.Log($"[GrabbableObject] {gameObject.name} fue agarrado por {holder.name}");
        }

        public virtual void Drop()
        {
            if (currentHolder != null)
            {
                var ikSystem = currentHolder.GetComponent<ObjectCarryingSystem>();
                if (ikSystem == null) ikSystem = currentHolder.GetComponentInParent<ObjectCarryingSystem>();

                if (ikSystem != null) ikSystem.StopCarrying();

                // NUEVO: Guardar referencia del jugador que soltó el objeto y el tiempo
                lastHolderTransform = currentHolder;
                lastDropTime = Time.time;

                if (VerboseLogging)
                    Debug.Log($"[GrabbableObject] {gameObject.name} fue soltado por {currentHolder.name}. Cooldown activo por {PickupCooldown}s");
            }

            if (currentHolderCollider != null && col != null) StartCoroutine(ResetCollisionDelay(col, currentHolderCollider, 0.5f));

            isHeld = false;
            isThrowingInProgress = false;
            currentHolder = null;
            holderAnimator = null;
            currentHolderProfile = null;

            transform.SetParent(originalParent);
            rb.isKinematic = false;
        }

        protected IEnumerator ThrowSequence()
        {
            if (currentHolder == null) yield break;
            isThrowingInProgress = true;

            // NUEVO: Guardar referencia antes de lanzar
            Transform thrower = currentHolder;

            var ikSystem = currentHolder.GetComponent<ObjectCarryingSystem>();
            if (ikSystem == null) ikSystem = currentHolder.GetComponentInParent<ObjectCarryingSystem>();

            if (ikSystem != null) ikSystem.PlayThrowAnimation();

            float elapsedTime = 0f;
            Vector3 startPos = transform.localPosition;
            Vector3 targetLocalPos = startPos + (Vector3.forward * VisualThrowDistance) + (Vector3.up * 0.2f);

            while (elapsedTime < ThrowAnimationDelay)
            {
                if (currentHolder == null) yield break;
                transform.localPosition = Vector3.Lerp(startPos, targetLocalPos, elapsedTime / ThrowAnimationDelay);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            ExecutePhysicsThrow(thrower);
        }

        protected void ExecutePhysicsThrow(Transform thrower = null)
        {
            if (currentHolder == null && thrower == null) { Drop(); return; }

            Transform actualThrower = thrower != null ? thrower : currentHolder;
            Vector3 throwDirection = actualThrower.forward;
            Collider tempHolderCollider = currentHolderCollider;

            // NUEVO: Guardar referencia del jugador que lanzó
            lastHolderTransform = actualThrower;
            lastDropTime = Time.time;

            isHeld = false;
            isThrowingInProgress = false;
            currentHolder = null;
            holderAnimator = null;
            currentHolderProfile = null;
            currentHolderCollider = null;

            transform.SetParent(originalParent);
            rb.isKinematic = false;

            Vector3 forceVector = (throwDirection * ThrowForce) + (Vector3.up * ThrowUpwardForce);
            rb.AddForce(forceVector, ForceMode.Impulse);
            rb.AddTorque(UnityEngine.Random.insideUnitSphere * 10f, ForceMode.Impulse);

            if (tempHolderCollider != null && col != null) StartCoroutine(ResetCollisionDelay(col, tempHolderCollider, 0.5f));

            if (VerboseLogging)
                Debug.Log($"[GrabbableObject] {gameObject.name} fue lanzado por {actualThrower.name}. Cooldown activo por {PickupCooldown}s");
        }

        private IEnumerator ResetCollisionDelay(Collider objectCol, Collider playerCol, float delay)
        {
            if (objectCol == null || playerCol == null) yield break;
            yield return new WaitForSeconds(delay);
            if (objectCol != null && playerCol != null) Physics.IgnoreCollision(objectCol, playerCol, false);
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (isHeld) return;
            if (collision.relativeVelocity.magnitude >= MinCollisionForce)
            {
                PlayCollisionSound(collision.relativeVelocity.magnitude);

                ObjectNoiseEmitter noiseEmitter = GetComponent<ObjectNoiseEmitter>();
                if (noiseEmitter != null)
                {
                    noiseEmitter.TriggerCollisionNoise(collision.relativeVelocity.magnitude);
                }
            }
        }

        protected void PlayCollisionSound(float impactMagnitude)
        {
            if (audioSource != null && CollisionSound != null)
            {
                float dynamicVolume = Mathf.Clamp(impactMagnitude / 10f, 0.2f, 1f) * CollisionVolume;
                audioSource.PlayOneShot(CollisionSound, dynamicVolume);
            }
        }
    }
}