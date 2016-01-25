using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpriteResourceManager : MonoBehaviour {
    private static Dictionary<string, Sprite> alreadyLoadedSprites = new Dictionary<string, Sprite>();

    private const string spriteResourcesPath = "";

    public static Sprite loadSprite(string spriteName)
    {
        Sprite sprite;
        if  (alreadyLoadedSprites.TryGetValue(spriteName, out sprite))
        {
            return sprite;
        }
        sprite = Resources.Load<Sprite>(spriteName);
        if (sprite == null)
        {
            throw new UnityException("Cannot find '" + spriteResourcesPath + spriteName + "' sprite");
        }
        alreadyLoadedSprites.Add(spriteName, sprite);
        return sprite;
    }
}
