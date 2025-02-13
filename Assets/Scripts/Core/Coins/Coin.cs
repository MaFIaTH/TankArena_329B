using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public abstract class Coin : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    protected int coinValue;

    protected bool alreadyCollected;

    public abstract int Collect();

    public void SetValue(int value)
    {
        coinValue = value;
    }

    public void Show(bool show)
    {
        spriteRenderer.enabled = show;
    }
    
}
