// Assets/Scripts/CoopGates/ObstacleTriggerController.cs
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ObstacleTriggerController : MonoBehaviour
{
    public enum AnchorSide { Left, Right }

    [Header("Кнопка")]
    public UniversalPad pad;

    [Header("Ворота")]
    public GateController[] gates;

    [Header("Потолочная стенка")]
    public Transform ceiling;
    [Min(0.01f)] public float ceilingTargetScaleX = 0.4f;
    [Min(0.01f)] public float ceilingShrinkDuration = 0.4f;
    public AnchorSide ceilingAnchorSide = AnchorSide.Left; // какой край фиксировать

    [Header("Вторая платформа (управляемая)")]
    public UpDownPlatform2D secondPlatform;   // autoStart = false

    [Header("Опции")]
    public bool fireOnPress = true;
    public bool ensureSecondPlatformStoppedOnEnable = true;

    private bool _fired;

    void OnEnable()
    {
        if (pad != null) pad.OnPressChanged += OnPadChanged;
        if (ensureSecondPlatformStoppedOnEnable && secondPlatform != null)
            secondPlatform.StopMoving();
    }

    void OnDisable()
    {
        if (pad != null) pad.OnPressChanged -= OnPadChanged;
    }

    void OnPadChanged(UniversalPad _, bool pressed)
    {
        if (_fired) return;
        if ((fireOnPress && pressed) || (!fireOnPress && !pressed))
            TriggerSequence();
    }

    void TriggerSequence()
    {
        _fired = true;

        // 1) Ворота
        if (gates != null)
            for (int i = 0; i < gates.Length; i++)
                if (gates[i] != null) gates[i].OpenPermanently();

        // 2) Потолок: сжать по X с якорем
        if (ceiling != null)
            StartCoroutine(AnimateCeilingScaleXAnchored(ceiling, ceilingTargetScaleX, ceilingShrinkDuration, ceilingAnchorSide));

        // 3) Платформа
        if (secondPlatform != null)
            secondPlatform.StartMoving();
    }

    static IEnumerator AnimateCeilingScaleXAnchored(Transform t, float targetScaleX, float duration, AnchorSide anchor)
    {
        // why: работаем в мировых координатах, чтобы якорь оставался на месте
        var rend = t.GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            // fallback: просто меняем scale без компенсации
            yield return AnimateScaleXOnly(t, targetScaleX, duration);
            yield break;
        }

        var startScale = t.localScale;
        float startScaleX = startScale.x;
        float endScaleX = targetScaleX;

        var startBounds = rend.bounds;
        float startWidth = startBounds.size.x;
        float anchorX = (anchor == AnchorSide.Left) ? startBounds.min.x : startBounds.max.x;

        float time = 0f;
        if (duration <= 0f)
        {
            // мгновенно
            t.localScale = new Vector3(endScaleX, startScale.y, startScale.z);
            // компенсация позиции
            yield return null;
            rend = t.GetComponentInChildren<Renderer>(); // обновить bounds
            var curBounds = rend.bounds;
            float newWidth = curBounds.size.x; // после скейла
            float newCenterX = (anchor == AnchorSide.Left)
                ? anchorX + newWidth * 0.5f
                : anchorX - newWidth * 0.5f;
            var p = t.position; p.x = newCenterX; t.position = p;
            yield break;
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            float k = Mathf.Clamp01(time / duration);
            float sx = Mathf.Lerp(startScaleX, endScaleX, k);

            // применяем скейл
            t.localScale = new Vector3(sx, startScale.y, startScale.z);

            // после скейла обновляем bounds и компенсируем позицию по X
            var curBounds = rend.bounds;
            float newWidth = curBounds.size.x;
            float newCenterX = (anchor == AnchorSide.Left)
                ? anchorX + newWidth * 0.5f
                : anchorX - newWidth * 0.5f;

            var pos = t.position; pos.x = newCenterX; t.position = pos;

            yield return null;
        }

        // финальная коррекция
        var finalBounds = rend.bounds;
        float finalWidth = finalBounds.size.x;
        float finalCenterX = (anchor == AnchorSide.Left)
            ? anchorX + finalWidth * 0.5f
            : anchorX - finalWidth * 0.5f;

        t.localScale = new Vector3(endScaleX, startScale.y, startScale.z);
        var fp = t.position; fp.x = finalCenterX; t.position = fp;
    }

    static IEnumerator AnimateScaleXOnly(Transform t, float targetScaleX, float duration)
    {
        var start = t.localScale;
        var end = new Vector3(targetScaleX, start.y, start.z);

        if (duration <= 0f) { t.localScale = end; yield break; }

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float k = Mathf.Clamp01(time / duration);
            t.localScale = Vector3.Lerp(start, end, k);
            yield return null;
        }
        t.localScale = end;
    }
}
