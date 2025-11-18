// Assets/Scripts/CoopGates/GateGroup.cs
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GateGroup : MonoBehaviour
{
    [Header("Кнопки")]
    public ButtonPad whitePad;
    public ButtonPad blackPad;

    [Header("Ворота")]
    public GateController gate;

    void OnEnable()
    {
        if (whitePad != null) whitePad.OnPressChanged += HandlePadChanged;
        if (blackPad != null) blackPad.OnPressChanged += HandlePadChanged;
        Evaluate();
    }

    void OnDisable()
    {
        if (whitePad != null) whitePad.OnPressChanged -= HandlePadChanged;
        if (blackPad != null) blackPad.OnPressChanged -= HandlePadChanged;
    }

    void HandlePadChanged(ButtonPad _, bool __) => Evaluate();

    void Evaluate()
    {
        if (whitePad == null || blackPad == null || gate == null) return;

        if (whitePad.Pressed && blackPad.Pressed)
            gate.OpenPermanently();
        else
            gate.Close();
    }
}
