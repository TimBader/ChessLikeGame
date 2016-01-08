using UnityEngine;
using System.Collections;

public enum TileState
{
    NONE,
    SELECTED,
    MOVEABLE,
    SELECTABLE,
    ATTACKABLE,
    SPAWNABLE
};

public class HexTile : MonoBehaviour {

    public Sprite baseSprite;
    public Sprite selectedSprite;
    public Sprite moveableSprite;
    public Sprite selectableSprite;
    public Sprite attackableSprite;
    public Sprite spawnableSprite;
    //Sets this tile to be a location in which a king of the specific team could be spawned on, -1 for no king spawning
    public int teamSpawnLoc = -1;

    private Vector2 hexCoord = new Vector2();
    private bool posHasBeenSet = false;
    private UnitScript occupyingUnit = null;
    private SpriteRenderer spriteRendererRef = null;
    private TileState currentState = TileState.NONE;



    public void initialize()
    {
        spriteRendererRef = GetComponent<SpriteRenderer>();
    }



    //Find Something much better to use that
    public void switchState(TileState state = TileState.NONE)
    {
        currentState = state;
        switch (state)
        {
            case TileState.NONE:
                spriteRendererRef.sprite = baseSprite;
                break;

            case TileState.SELECTED:
                spriteRendererRef.sprite = selectedSprite;
                break;

            case TileState.MOVEABLE:
                spriteRendererRef.sprite = moveableSprite;
                break;

            case TileState.SELECTABLE:
                spriteRendererRef.sprite = selectableSprite;
                break;

            case TileState.ATTACKABLE:
                spriteRendererRef.sprite = attackableSprite;
                break;

            case TileState.SPAWNABLE:
                spriteRendererRef.sprite = spawnableSprite;
                break;
        }
    }



    public TileState getCurrentTileState()
    {
        return currentState;
    }



    public void setCoords(Vector2 newHexCoord)
    {
        hexCoord = newHexCoord;
        posHasBeenSet = true;
    }



    public bool checkHasBeenSet()
    {
        return posHasBeenSet;
    }



    public Vector2 getCoords()
    {
        return hexCoord;
    }



    public UnitScript getOccupyingUnit()
    {
        return occupyingUnit;
    }



    public void setOccupyingUnit(UnitScript unit)
    {
        occupyingUnit = unit;
    }
}
