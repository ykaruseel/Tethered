// Assets/Scripts/Gate/UniversalPad.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class UniversalPad : MonoBehaviour
{
    [Header("Кто может нажимать")]
    public LayerMask activatorLayers = ~0;

    [Header("Анимация кнопки")]
    [Tooltip("Если пусто — двигается сам объект кнопки.")]
    public Transform topVisual;
    [Tooltip("Насколько опускать вниз (локально).")]
    public float pressedOffsetY = -0.08f;
    [Tooltip("Скорость сглаживания движения.")]
    public float pressLerpSpeed = 12f;

    public event Action<UniversalPad, bool> OnPressChanged;
    public bool Pressed { get; private set; }

    private readonly HashSet<GameObject> _pressing = new();
    private Transform _top;
    private Vector3 _topStartLocal;
    private Collider2D _col;

    void Awake()
    {
        _col = GetComponent<Collider2D>();
        _col.isTrigger = true;

        _top = topVisual != null ? topVisual : transform;
        _topStartLocal = _top.localPosition;
    }

    void OnValidate()
    {
        if (_col == null) _col = GetComponent<Collider2D>();
        if (_col != null) _col.isTrigger = true;
    }

    void Update()
    {
        if (_top == null) return;
        var target = _topStartLocal + Vector3.up * (Pressed ? pressedOffsetY : 0f);
        _top.localPosition = Vector3.Lerp(_top.localPosition, target, Time.deltaTime * pressLerpSpeed);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsActivator(other.gameObject)) return;
        var root = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
        if (_pressing.Add(root)) SetPressed(_pressing.Count > 0);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsActivator(other.gameObject)) return;
        var root = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
        if (_pressing.Remove(root)) SetPressed(_pressing.Count > 0);
    }

    bool IsActivator(GameObject go) => ((1 << go.layer) & activatorLayers) != 0;

    void SetPressed(bool v)
    {
        if (Pressed == v) return;
        Pressed = v;
        OnPressChanged?.Invoke(this, Pressed);
    }
}
