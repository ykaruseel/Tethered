// Assets/Scripts/CoopGates/ButtonPad.cs
using System;
using System.Collections.Generic;
using UnityEngine;

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

    public event Action<ButtonPad, bool> OnPressChanged;
    public bool Pressed { get; private set; }

    private readonly HashSet<GameObject> _pressingPlayers = new();
    private Transform _top;           // что двигаем
    private Vector3 _topStartLocalPos;
    private Collider2D _col;

    void Awake()
    {
        _col = GetComponent<Collider2D>();
        _col.isTrigger = true; // why: игрок встаёт на триггер-плиту

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
        var target = _topStartLocalPos + Vector3.up * (Pressed ? pressedOffsetY : 0f);
        _top.localPosition = Vector3.Lerp(_top.localPosition, target, Time.deltaTime * pressLerpSpeed);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var root = GetValidPlayerRoot(other);
        if (root == null) return;
        if (_pressingPlayers.Add(root))
            SetPressed(_pressingPlayers.Count > 0);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var root = GetValidPlayerRoot(other);
        if (root == null) return;
        if (_pressingPlayers.Remove(root))
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
        Pressed = value;
        OnPressChanged?.Invoke(this, Pressed);
    }
}
