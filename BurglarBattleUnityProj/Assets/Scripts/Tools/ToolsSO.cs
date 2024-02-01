// Joshua Weston

using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Tool", menuName = "Tool")]
public class ToolsSO : ScriptableObject
{
    public string toolName;
    public string toolDescription;
    public int toolUses;
    public int toolRange;
    public int toolIndex;
    public GameObject toolPrefab;
    public Sprite toolSprite;
}
