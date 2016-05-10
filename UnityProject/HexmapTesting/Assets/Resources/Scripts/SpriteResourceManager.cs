using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class SpriteResourceManager
{
    private const string SPRITES_RESOURCE_PATH = "Sprites/";

    private static Dictionary<string, Sprite> alreadyLoadedSprites = new Dictionary<string, Sprite>();

    public static Sprite loadSprite(string spriteName)
    {
        Sprite sprite;
        if  (alreadyLoadedSprites.TryGetValue(spriteName, out sprite))
        {
            return sprite;
        }
        sprite = Resources.Load<Sprite>(SPRITES_RESOURCE_PATH + spriteName);
        if (sprite == null)
        {
            throw new UnityException("Cannot find '" + SPRITES_RESOURCE_PATH + spriteName + "' sprite");
        }
        alreadyLoadedSprites.Add(spriteName, sprite);
        return sprite;
    }
}
