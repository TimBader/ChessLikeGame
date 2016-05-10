using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InteractionIcon : MonoBehaviour
{
    private static List<InteractionIcon> activeIcons = new List<InteractionIcon>();

    public static GameObject interactionIconPrefab = PrefabResourceManager.loadPrefab("InteractionIconPrefab");

    SpriteRenderer spriteRenderer = null;
    // Use this for initialization
    /*void Start ()
    {
	    if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
	}*/

    public static InteractionIcon createInteractionIcon(string spriteName, Vector2 coords, Color blendColor, int depth = 0, bool pixelCoords = false)
    {
        GameObject go = (GameObject)Instantiate(interactionIconPrefab);
        InteractionIcon m = (InteractionIcon)go.GetComponent(typeof(InteractionIcon));
        m.initialize(spriteName, coords, blendColor, depth, pixelCoords);
        activeIcons.Add(m);
        return m;
    }

    public static void clearAllInteractionIcons()
    {
        while (activeIcons.Count != 0)
        {
            DestroyObject(activeIcons[0].gameObject);
            activeIcons.RemoveAt(0);
        }
    }

    public void initialize(string spriteName, Vector2 coords, Color blendColor, int depth = 0, bool pixelCoords = false)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = SpriteResourceManager.loadSprite("InteractionIcons/" + spriteName);
        spriteRenderer.sortingOrder = depth;
        spriteRenderer.color = blendColor;

        if (pixelCoords)
        {
            transform.position = coords;
        }
        else
        {
            transform.position = TileController.hexCoordToPixelCoord(coords);
        }
    }

    public SpriteRenderer getSpriteRenderer()
    {
        return spriteRenderer;
    }

	// Update is called once per frame
	/*void Update () {
	
	}*/
}
