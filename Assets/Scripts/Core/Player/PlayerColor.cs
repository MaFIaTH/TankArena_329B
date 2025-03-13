using UnityEngine;

public class PlayerColor : MonoBehaviour
{
    [SerializeField] private TankPlayer player;
    [SerializeField] private SpriteRenderer[] playerSprites;
    [SerializeField] private Color[] tankColor;
    [SerializeField] private int colorIndex;
    
    private void Start()
    {
        HandlePlayerColorChanged(0, player.PlayerColorIndex.Value);
        player.PlayerColorIndex.OnValueChanged += HandlePlayerColorChanged;
    }
    
    private void HandlePlayerColorChanged(int oldIndex,int newIndex)
    {
        colorIndex = newIndex;
        foreach(SpriteRenderer sprite in playerSprites)
        {
            sprite.color = tankColor[colorIndex];
        }
    }

    private void OnDestroy()
    {
        player.PlayerColorIndex.OnValueChanged -= HandlePlayerColorChanged;
    }
}
