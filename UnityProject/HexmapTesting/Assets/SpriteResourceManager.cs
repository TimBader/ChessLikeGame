using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpriteResourceManager : MonoBehaviour {
    private Dictionary<string, Sprite> alreadyLoadedSprites = new Dictionary<string, Sprite>();

    private const string spriteResourcesPath = "";

    public Sprite loadSprite(string spriteName)
    {
        Sprite sprite;
        if  (alreadyLoadedSprites.TryGetValue(spriteName, out sprite))
        {
            //print("Already had loaded " + spriteName);
            return sprite;
        }
        //print("Loading " + spriteName);
        sprite = Resources.Load<Sprite>(spriteName);
        if (sprite == null)
        {
            throw new UnityException("Cannot find '" + spriteResourcesPath + spriteName + "' sprite");
        }
        alreadyLoadedSprites.Add(spriteName, sprite);
        return sprite;
    }
}
