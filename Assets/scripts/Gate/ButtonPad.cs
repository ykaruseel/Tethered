using System;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class ButtonPad : MonoBehaviour
{
    [Header("Требуемый цвет")]
    public PlayerColor requiredColor = PlayerColor.White;

    [Header("Анимация кнопки")]
    [Tooltip("Что опускать при нажатии. Если не задано — двигается сам объект кнопки.")]
    public Transform topVisual;
    [Tooltip("Насколько опускать (в локальных единицах). Отрицательное — вниз.")]
    public float pressedOffsetY = -0.08f;
    [Tooltip("Скорость сглаживания движения.")]
    public float pressLerpSpeed = 12f;

    [Header("FMOD")]
    [SerializeField] private EventReference pressSfx;
    [SerializeField] private EventReference releaseSfx;

    public event Action<ButtonPad, bool> OnPressChanged;
    public bool Pressed { get; private set; }

    private readonly HashSet<GameObject> _pressingPlayers = new();

    private Transform _top;
    private Vector3 _topStartLocalPos;
    private Collider2D _col;

    void Awake()
    {
        _col = GetComponent<Collider2D>();
        _col.isTrigger = true;

        _top = topVisual != null ? topVisual : transform;
        _topStartLocalPos = _top.localPosition;
    }

    void OnValidate()
    {
        if (_col == null) _col = GetComponent<Collider2D>();
        if (_col != null) _col.isTrigger = true;
    }

    void Update()
    {
        if (_top == null) return;

        Vector3 target = _topStartLocalPos +
                         Vector3.up * (Pressed ? pressedOffsetY : 0f);

        _top.localPosition = Vector3.Lerp(
            _top.localPosition,
            target,
            Time.deltaTime * pressLerpSpeed
        );
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var playerRoot = GetValidPlayerRoot(other);
        if (playerRoot == null) return;

        if (_pressingPlayers.Add(playerRoot))
            SetPressed(_pressingPlayers.Count > 0);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var playerRoot = GetValidPlayerRoot(other);
        if (playerRoot == null) return;

        if (_pressingPlayers.Remove(playerRoot))
            SetPressed(_pressingPlayers.Count > 0);
    }

    private GameObject GetValidPlayerRoot(Collider2D col)
    {
        var id = col.GetComponentInParent<PlayerIdentity>();
        if (id == null || id.color != requiredColor) return null;
        return id.gameObject;
    }

    private void SetPressed(bool value)
    {
        if (Pressed == value) return;

        bool wasPressed = Pressed;
        Pressed = value;

        // переход false -> true: нажатие
        if (!wasPressed && Pressed && !pressSfx.IsNull)
            RuntimeManager.PlayOneShot(pressSfx, transform.position);

        // переход true -> false: отпускание
        if (wasPressed && !Pressed && !releaseSfx.IsNull)
            RuntimeManager.PlayOneShot(releaseSfx, transform.position);

        OnPressChanged?.Invoke(this, Pressed);
    }
}