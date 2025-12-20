using UnityEngine;

public class PrefabAnimationController : MonoBehaviour
{
    [Header("Player 1 Settings")]
    [Tooltip("Arrastra el prefab de la linterna de Player 1 aqui.")]
    public GameObject player1FlashlightPrefab;
    [Tooltip("Arrastra el componente Animator de Player 1 aqui.")]
    public Animator player1Animator;

    [Header("Player 2 Settings")]
    [Tooltip("Arrastra el prefab de la linterna de Player 2 aqui.")]
    public GameObject player2FlashlightPrefab;
    [Tooltip("Arrastra el componente Animator de Player 2 aqui.")]
    public Animator player2Animator;

    [Header("Animation Settings")]
    [Tooltip("The names of the crouch animation states (for example, 'Crouched Walking', 'Crouched Idle').")]
    public string[] crouchAnimationStateNames = { "Crouch" };

    [Header("Hand Bone Settings")]
    [Tooltip("The name of the left hand bone for Player 1 (e.g. 'mixamorig:LeftHandIndex4').")]
    public string player1HandBoneName = "mixamorig:LeftHandIndex4";
    [Tooltip("El nombre del hueso de la mano izquierda para Player 2 (ej. 'mixamorig2:LeftHandIndex4').")]
    public string player2HandBoneName = "mixamorig2:LeftHandIndex4";

    private TransformData player1OriginalTransform;
    private TransformData player2OriginalTransform;

    private bool player1IsCrouching = false;
    private bool player2IsCrouching = false;

    private struct TransformData
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Transform parent;
    }

    void Start()
    {
        
        if (player1FlashlightPrefab == null)
        {

        }
        if (player1Animator == null)
        {

        }
        if (player2FlashlightPrefab == null)
        {

        }
        if (player2Animator == null)
        {

        }
    }

    void LateUpdate()
    {
        HandlePlayerPrefab(player1FlashlightPrefab, player1Animator, ref player1IsCrouching, ref player1OriginalTransform);
        HandlePlayerPrefab(player2FlashlightPrefab, player2Animator, ref player2IsCrouching, ref player2OriginalTransform);
    }

    private void HandlePlayerPrefab(GameObject prefab, Animator animator, ref bool isCrouching, ref TransformData originalTransform)
    {
        if (prefab == null || animator == null)
        {
            return;
        }

        bool currentlyCrouching = IsAnimatorInCrouchState(animator);

        if (currentlyCrouching && !isCrouching)
        {
            
            isCrouching = true;
            SaveOriginalTransform(prefab, ref originalTransform);

            
            string handBoneName = (prefab == player1FlashlightPrefab) ? player1HandBoneName : player2HandBoneName;
            Transform handBone = animator.GetBoneTransform(HumanBodyBones.LeftHand); 
            if (handBone == null)
            {
                
                handBone = FindDeepChild(animator.transform, handBoneName);
            }

            if (handBone != null)
            {
                prefab.transform.SetParent(handBone);
                prefab.transform.localPosition = Vector3.zero;
                prefab.transform.localRotation = Quaternion.identity;
            }
            else
            {

            }
        }
        else if (!currentlyCrouching && isCrouching)
        {
            
            isCrouching = false;
            
            prefab.transform.SetParent(originalTransform.parent);
            prefab.transform.localPosition = originalTransform.localPosition;
            prefab.transform.localRotation = originalTransform.localRotation;
        }

        if (isCrouching)
        {
            
            
            
            
            if (prefab.transform.parent == null || prefab.transform.parent != animator.GetBoneTransform(HumanBodyBones.LeftHand))
            {
                prefab.transform.localPosition = originalTransform.localPosition;
                prefab.transform.localRotation = originalTransform.localRotation;
            }
        }
    }

    private void SaveOriginalTransform(GameObject prefab, ref TransformData transformData)
    {
        transformData.localPosition = prefab.transform.localPosition;
        transformData.localRotation = prefab.transform.localRotation;
        transformData.parent = prefab.transform.parent;
    }

    private bool IsAnimatorInCrouchState(Animator animator)
    {
        if (animator == null || crouchAnimationStateNames == null || crouchAnimationStateNames.Length == 0)
        {
            return false;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        foreach (string stateName in crouchAnimationStateNames)
        {
            if (stateInfo.IsName(stateName))
            {
                return true;
            }
        }
        return false;
    }

    
    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }
            Transform found = FindDeepChild(child, childName);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
}
