// Assets/Scripts/CoopGates/PlayerIdentity.cs
using UnityEngine;

public enum PlayerColor { White, Black }

[DisallowMultipleComponent]
public sealed class PlayerIdentity : MonoBehaviour
{
    public PlayerColor color = PlayerColor.White;
}
