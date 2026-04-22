using System.Collections;
using UnityEngine;

/// <summary>
/// Animaciones del vehiculo 100% por codigo. Sin Animator.
/// Adjuntar al mismo GameObject que PlayerController.
///
/// ANIMACIONES:
///   - Pitch (X): tilt hacia atras al acelerar, hacia adelante al frenar
///   - Roll  (Z): lean lateral al cambiar de carril
///   - Hit Shake: oscilacion de roll al recibir un golpe
///
/// PARA ACTIVAR EL HIT SHAKE: llamar TriggerHitReaction() desde PlayerController.
/// </summary>
public class VehicleAnimator : MonoBehaviour
{
    // ─── Tilt Aceleración / Frenada ───────────────────────────────────────────
    [Header("Pitch — Aceleración / Frenada")]
    [Tooltip("Grados de inclinación hacia atrás al acelerar (valor positivo = nariz arriba)")]
    [SerializeField] private float pitchOnAccel = 6f;
    [Tooltip("Grados de inclinación hacia adelante al frenar (valor negativo = nariz abajo)")]
    [SerializeField] private float pitchOnBrake = -4f;
    [Tooltip("Velocidad con que se aplica el tilt")]
    [SerializeField] private float pitchSmoothIn = 6f;
    [Tooltip("Velocidad con que vuelve a 0 cuando no hay input")]
    [SerializeField] private float pitchSmoothOut = 9f;

    // ─── Lean Cambio de Carril ────────────────────────────────────────────────
    [Header("Roll — Cambio de Carril")]
    [Tooltip("Máximo ángulo de lean lateral en grados")]
    [SerializeField] private float maxRollAngle = 12f;
    [Tooltip("Cuánto influye la velocidad horizontal en el roll (multiplicador)")]
    [SerializeField] private float rollSensitivity = 0.9f;
    [SerializeField] private float rollSmoothIn = 11f;
    [SerializeField] private float rollSmoothOut = 7f;
    [Tooltip("Velocidad mínima de movimiento lateral para activar el lean")]
    [SerializeField] private float rollXThreshold = 0.08f;

    // ─── Hit Shake ────────────────────────────────────────────────────────────
    [Header("Hit Shake")]
    [Tooltip("Ángulo máximo de oscilación al recibir un golpe")]
    [SerializeField] private float hitShakeAngle = 18f;
    [Tooltip("Duración total del shake en segundos")]
    [SerializeField] private float hitShakeDuration = 0.45f;
    [Tooltip("Cantidad de oscilaciones completas")]
    [SerializeField] private int hitShakeCycles = 4;

    // ─── Privados ─────────────────────────────────────────────────────────────
    private float currentPitch = 0f;
    private float currentRoll = 0f;
    private float prevX;
    private bool isShaking = false;
    private Coroutine shakeCoroutine;

    // ─────────────────────────────────────────────────────────────────────────
    private void Start()
    {
        prevX = transform.position.x;
    }

    /// <summary>
    /// LateUpdate para que lea la posicion final del frame (despues de que PlayerController movio al player).
    /// </summary>
    private void LateUpdate()
    {
        if (isShaking) return; // el shake toma control total de la rotacion

        UpdatePitch();
        UpdateRoll();

        transform.localRotation = Quaternion.Euler(currentPitch, 0f, currentRoll);
    }

    // ─── Pitch ────────────────────────────────────────────────────────────────
    private void UpdatePitch()
    {
        bool accelInput = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
        bool brakeInput = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);

        float targetPitch = 0f;
        if (accelInput) targetPitch = pitchOnAccel;
        else if (brakeInput) targetPitch = pitchOnBrake;

        float smooth = (accelInput || brakeInput) ? pitchSmoothIn : pitchSmoothOut;
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, smooth * Time.deltaTime);
    }

    // ─── Roll ─────────────────────────────────────────────────────────────────
    private void UpdateRoll()
    {
        float xDelta = transform.position.x - prevX;
        float xVelocity = xDelta / Mathf.Max(Time.deltaTime, 0.0001f);

        float targetRoll = 0f;
        bool moving = Mathf.Abs(xVelocity) > rollXThreshold;

        if (moving)
        {
            // Mover a la derecha = lean derecha = roll negativo en Unity (Z local)
            targetRoll = -xVelocity * rollSensitivity;
            targetRoll = Mathf.Clamp(targetRoll, -maxRollAngle, maxRollAngle);
        }

        float smooth = moving ? rollSmoothIn : rollSmoothOut;
        currentRoll = Mathf.Lerp(currentRoll, targetRoll, smooth * Time.deltaTime);

        prevX = transform.position.x;
    }

    // ─── Hit Shake ────────────────────────────────────────────────────────────

    /// <summary>
    /// Llamar desde PlayerController.HandleObstacleCollision() al perder una vida.
    /// </summary>
    public void TriggerHitReaction()
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(HitShakeCoroutine());
    }

    private IEnumerator HitShakeCoroutine()
    {
        isShaking = true;

        int steps = hitShakeCycles * 2;           // medio ciclo por paso
        float stepTime = hitShakeDuration / steps;
        float dir = 1f;

        for (int i = 0; i < steps; i++)
        {
            // Amplitud decrece progresivamente (damped shake)
            float t = 1f - (float)i / steps;
            float angle = hitShakeAngle * dir * t;

            transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            dir *= -1f;

            yield return new WaitForSeconds(stepTime);
        }

        // Restaurar rotacion y resetear pitch/roll para que LateUpdate empiece desde 0
        transform.localRotation = Quaternion.identity;
        currentPitch = 0f;
        currentRoll = 0f;
        isShaking = false;
    }
}