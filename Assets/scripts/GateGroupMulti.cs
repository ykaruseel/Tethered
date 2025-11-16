// Assets/Scripts/CoopGates/GateGroupMulti.cs
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GateGroupMulti : MonoBehaviour
{
    public enum Mode
    {
        TogetherOnly,                 // обе кнопки → открыть и зафиксировать ВСЕ; иначе закрыто
        IndependentThenLatchTogether  // по отдельности открываются, вдвоём — фиксируются навсегда
    }

    [Header("Кнопки")]
    public ButtonPad whitePad; // requiredColor = White
    public ButtonPad blackPad; // requiredColor = Black

    [Header("Ворота по цветам")]
    public GateController[] whiteGates; // управляются белой кнопкой
    public GateController[] blackGates; // управляются чёрной кнопкой

    [Header("Общие ворота (открываются только вместе)")]
    public GateController[] sharedGates;

    [Header("Поведение")]
    public Mode behavior = Mode.IndependentThenLatchTogether;

    [Header("Окно одновременности (сек)")]
    [Min(0f)] public float simultaneousWindow = 0f;

    private float lastWhitePressTime = -999f;
    private float lastBlackPressTime = -999f;

    void OnEnable()
    {
        if (whitePad != null) whitePad.OnPressChanged += OnPadChanged;
        if (blackPad != null) blackPad.OnPressChanged += OnPadChanged;
        Evaluate();
    }

    void OnDisable()
    {
        if (whitePad != null) whitePad.OnPressChanged -= OnPadChanged;
        if (blackPad != null) blackPad.OnPressChanged -= OnPadChanged;
    }

    private void OnPadChanged(ButtonPad pad, bool pressed)
    {
        if (pressed)
        {
            if (pad == whitePad) lastWhitePressTime = Time.time;
            if (pad == blackPad) lastBlackPressTime = Time.time;
        }
        Evaluate();
    }

    private void Evaluate()
    {
        if (whitePad == null || blackPad == null) return;

        bool whitePressed = whitePad.Pressed;
        bool blackPressed = blackPad.Pressed;

        bool bothPressed = whitePressed && blackPressed;
        if (!bothPressed && simultaneousWindow > 0f)
        {
            if (whitePressed && (Time.time - lastBlackPressTime) <= simultaneousWindow) bothPressed = true;
            if (blackPressed && (Time.time - lastWhitePressTime) <= simultaneousWindow) bothPressed = true;
        }

        if (bothPressed)
        {
            Apply(whiteGates, g => g.OpenPermanently());
            Apply(blackGates, g => g.OpenPermanently());
            Apply(sharedGates, g => g.OpenPermanently());
            return;
        }

        switch (behavior)
        {
            case Mode.TogetherOnly:
                Apply(whiteGates, g => g.Close());
                Apply(blackGates, g => g.Close());
                Apply(sharedGates, g => g.Close());
                break;

            case Mode.IndependentThenLatchTogether:
                Apply(whiteGates, g =>
                {
                    if (!g.IsOpenLatched)
                    {
                        if (whitePressed) g.Open();
                        else g.Close();
                    }
                });
                Apply(blackGates, g =>
                {
                    if (!g.IsOpenLatched)
                    {
                        if (blackPressed) g.Open();
                        else g.Close();
                    }
                });
                Apply(sharedGates, g => g.Close());
                break;
        }
    }

    private static void Apply(GateController[] gates, System.Action<GateController> action)
    {
        if (gates == null) return;
        for (int i = 0; i < gates.Length; i++)
        {
            var g = gates[i];
            if (g != null) action(g);
        }
    }
}
