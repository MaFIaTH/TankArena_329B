using UnityEngine;


[CreateAssetMenu(fileName = "TeamColorLookup", menuName = "ScriptableObjects/TeamColorLookup")]
public class TeamColorLookup : ScriptableObject
{
    [SerializeField] private Color[] teamColors;
    
    public Color? GetTeamColor(int index)
    {
        if (index < 0) return null;
        if (index >= teamColors.Length) return Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        return teamColors[index];
    }
}
