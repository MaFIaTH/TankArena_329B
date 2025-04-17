using UnityEngine;

public class PlayerColor : MonoBehaviour
{
    [SerializeField] private TeamColorLookup teamColorLookup;
    [SerializeField] private TankPlayer player;
    [SerializeField] private SpriteRenderer[] playerSprites;
    [SerializeField] private int colorIndex;
    
    private void Start()
    {
        HandlePlayerColorChanged(0, player.PlayerColorIndex.Value);
        player.PlayerColorIndex.OnValueChanged += HandlePlayerColorChanged;
        player.TeamIndex.OnValueChanged += HandleTeamChanged;
    }
    
    private void HandlePlayerColorChanged(int oldIndex,int newIndex)
    {
        colorIndex = newIndex;
        foreach(SpriteRenderer sprite in playerSprites)
        {
            var color = teamColorLookup.GetTeamColor(colorIndex);
            if (color == null) return;
            sprite.color = (Color) color;
        }
    }
    
    public void HandleTeamChanged(int oldIndex, int newIndex)
    {
        var color = teamColorLookup.GetTeamColor(newIndex);
        if (color == null) return;
        foreach(SpriteRenderer sprite in playerSprites)
        {
            sprite.color = (Color) color;
        }
    }

    private void OnDestroy()
    {
        player.PlayerColorIndex.OnValueChanged -= HandlePlayerColorChanged;
    }
}
