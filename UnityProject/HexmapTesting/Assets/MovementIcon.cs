using UnityEngine;
using System.Collections;

public class MovementIcon : MonoBehaviour
{
    SpriteRenderer spriteRenderer = null;
	// Use this for initialization
	void Start ()
    {
	    if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
	}
	
    public void initialize(string spriteName, Vector2 coords, Color blendColor, int depth = 0, bool pixelCoords = false)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = SpriteResourceManager.loadSprite(spriteName);
        spriteRenderer.sortingOrder = depth;
        spriteRenderer.color = blendColor;

        if (pixelCoords)
        {
            transform.position = coords;
        }
        else
        {
            transform.position = SpawnTiles.hexCoordToPixelCoord(coords);
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
