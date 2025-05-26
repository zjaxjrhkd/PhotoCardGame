using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteList", menuName = "GameData/SpriteList")]
public class SpriteListSO : ScriptableObject
{
    public List<Sprite> sprites;
}
