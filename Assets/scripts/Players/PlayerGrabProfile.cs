using UnityEngine;

public class PlayerGrabProfile : MonoBehaviour
{
    [Header("1. Ajustes de Posicion (Hold)")]
    [Tooltip("Suma esto a la posicion del objeto para ajustar altura/distancia.")]
    public Vector3 HoldPositionOffset = Vector3.zero;

    [Tooltip("Suma esto a la rotacion del objeto.")]
    public Vector3 HoldRotationOffset = Vector3.zero;

    [Header("2. Ajustes de Animacion al Lanzar (IK)")]
    [Tooltip("Que tan ADELANTE estira los brazos este personaje al lanzar? (Ej: 1.2)")]
    public float ThrowArmReach = 1.2f;

    [Tooltip("Que tan ARRIBA levanta las manos este personaje al lanzar? (Ej: 0.1)")]
    public float ThrowArmHeight = 0.1f;

    [Tooltip("Velocidad de la animacion de los brazos para este personaje.")]
    public float ThrowAnimationSpeed = 15f;
}