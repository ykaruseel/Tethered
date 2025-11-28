using System.Collections;
using UnityEngine;

public enum AnchorSide
{
    Left,
    Right,
    Top,
    Bottom
}

public class ObstacleTriggerController : MonoBehaviour
{
    [Header("Pad Trigger")]
    public UniversalPad pad;
    public bool fireOnPress = true;     // true = срабатывает при нажатии, false = при отпускании
    public bool onceOnly = true;        // срабатывает один раз

    [Header("Gates To Open")]
    public GateController[] gates;

    [Header("Ceiling Shrink")]
    public Transform ceiling;
    public float ceilingTargetScaleX = 0.3f;
    public float ceilingShrinkDuration = 0.5f;
    public AnchorSide ceilingAnchorSide = AnchorSide.Left;

    [Header("Second Platform (autoStart must be OFF)")]
    public UpDownPlatform2D secondPlatform;

    [Header("Objects to Disable on Trigger")]
    public GameObject[] objectsToDisable;

    private bool triggered;

    private void OnEnable()
    {
        if (pad != null)
            pad.OnPressChanged += OnPadPressChanged;
    }

    private void OnDisable()
    {
        if (pad != null)
            pad.OnPressChanged -= OnPadPressChanged;
    }

    private void OnPadPressChanged(UniversalPad _, bool pressed)
    {
        if (triggered && onceOnly)
            return;

        bool shouldFire = fireOnPress ? pressed : !pressed;
        if (!shouldFire)
            return;

        triggered = true;

        // 1. Открываем ворота навсегда
        if (gates != null)
        {
            foreach (var g in gates)
            {
                if (g != null)
                    g.OpenPermanently();
            }
        }

        // 2. Сжимаем потолок
        if (ceiling != null)
        {
            StartCoroutine(AnimateCeilingScaleXAnchored(
                ceiling,
                ceilingTargetScaleX,
                ceilingShrinkDuration,
                ceilingAnchorSide
            ));
        }

        // 3. Запускаем вторую платформу
        if (secondPlatform != null)
            secondPlatform.StartMoving();

        // 4. Выключаем указанные объекты
        if (objectsToDisable != null)
        {
            foreach (var obj in objectsToDisable)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
    }

    private IEnumerator AnimateCeilingScaleXAnchored(
        Transform target,
        float targetScaleX,
        float duration,
        AnchorSide anchorSide)
    {
        var rend = target.GetComponentInChildren<Renderer>();

        Vector3 startScale = target.localScale;
        float startX = startScale.x;
        float endX = targetScaleX;

        float t = 0f;

        if (rend == null)
        {
            // fallback — без якоря
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                target.localScale = new Vector3(Mathf.Lerp(startX, endX, k), startScale.y, startScale.z);
                yield return null;
            }

            target.localScale = new Vector3(endX, startScale.y, startScale.z);
            yield break;
        }

        // Исходный bounds
        Bounds b0 = rend.bounds;
        float anchorPos = anchorSide switch
        {
            AnchorSide.Left => b0.min.x,
            AnchorSide.Right => b0.max.x,
            AnchorSide.Bottom => b0.min.y,
            AnchorSide.Top => b0.max.y,
            _ => b0.min.x
        };

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);

            float newX = Mathf.Lerp(startX, endX, k);
            target.localScale = new Vector3(newX, startScale.y, startScale.z);

            // Пересчитать позицию так, чтобы якорная сторона оставалась на месте
            Bounds b = rend.bounds;
            Vector3 pos = target.position;

            switch (anchorSide)
            {
                case AnchorSide.Left:
                    pos.x = anchorPos + b.extents.x;
                    break;

                case AnchorSide.Right:
                    pos.x = anchorPos - b.extents.x;
                    break;

                case AnchorSide.Bottom:
                    pos.y = anchorPos + b.extents.y;
                    break;

                case AnchorSide.Top:
                    pos.y = anchorPos - b.extents.y;
                    break;
            }

            target.position = pos;

            yield return null;
        }

        // Финальное значение
        target.localScale = new Vector3(endX, startScale.y, startScale.z);

        // Последнее выравнивание
        Bounds bf = rend.bounds;
        Vector3 posF = target.position;

        switch (anchorSide)
        {
            case AnchorSide.Left:
                posF.x = anchorPos + bf.extents.x;
                break;

            case AnchorSide.Right:
                posF.x = anchorPos - bf.extents.x;
                break;

            case AnchorSide.Bottom:
                posF.y = anchorPos + bf.extents.y;
                break;

            case AnchorSide.Top:
                posF.y = anchorPos - bf.extents.y;
                break;
        }

        target.position = posF;
    }
}
