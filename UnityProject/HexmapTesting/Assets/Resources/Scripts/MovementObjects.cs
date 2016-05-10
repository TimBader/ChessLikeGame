using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MovementTypes
{
    Path,
    Jump,
    Slide,
    Charge,
    Ranged,
    Reposition
}

//!!!!! Always make sure each does not have a relative position/direction of (0,0)
public abstract class MovementTypeParent : Object
{
    protected static GameControllerScript gameControllerRef = null;

    protected MovementTypes movementType = MovementTypes.Path;

    public MovementTypeParent() { }

    //public List<MovementIcon> movementIcons = new List<MovementIcon>();

    public static void setGameControllerRef(GameControllerScript gameControllerScriptRef)
    {
        gameControllerRef = gameControllerScriptRef;
    }

    public abstract void startSelectingInMode(UnitScript selectedUnit, int currentTeam);

    public abstract void clickedInMode(HexTile clickedTile, UnitScript selecetdUnit, int currentTeam);

    public abstract bool canMove(UnitScript selectedUnit, int currentTeam);

    public abstract void unitRotated(AbsoluteDirection direction);

    public abstract MovementTypeParent clone();

    public abstract void verifyMove();

    public static bool testForInvalidPositions(List<Vector2> positionsList)
    {
        Vector2 invalidPosition = new Vector2(0,0);
        for (int i = 0; i < positionsList.Count; i++ )
        {
                if (positionsList[i] == invalidPosition)
                {
                    return false;
                }
        }
        return true;
    }

    public static bool testForInvalidPositions(List<List<Vector2>> positionsLists)
    {
        for (int i = 0; i < positionsLists.Count; i++)
        {
            if (!testForInvalidPositions(positionsLists[i]))
            {
                return false;
            }
        }
        return true;
    }

    public MovementTypes getMovementType()
    {
        return movementType;
    }

    public abstract void drawMovementIcons(UnitScript selectedUnit);

   /* public void clearMovementIcons()
    {
        while (movementIcons.Count != 0)
        {
            //Delete MovementIcons
            DestroyObject(movementIcons[0].gameObject);
            movementIcons.RemoveAt(0);
        }
    }

    public MovementIcon createMovementIcon(string spriteName, Vector2 coords, Color blendColor, int depth = 0, bool pixelCoords = false)
    {
        GameObject unitGameObject = (GameObject)Instantiate(gameControllerRef.movementIconPrefab);
        MovementIcon m = (MovementIcon)unitGameObject.GetComponent(typeof(MovementIcon));
        m.initialize(spriteName, coords, blendColor, depth, pixelCoords);
        movementIcons.Add(m);
        return m;
    }*/
};




