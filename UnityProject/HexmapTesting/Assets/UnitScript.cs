using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum UnitType
{
    BasicUnit,
    KingUnit
};

public class UnitScript : MonoBehaviour {

    public Sprite tempOtherSprite;
    public Sprite King0Sprite;
    public Sprite King1Sprite;

    private HexTile occupyingHexTile = null;
    private List<Vector2> movePositions;
    private uint teamNumber = 0;
    private bool garbage = false;
    private UnitType unitType = UnitType.BasicUnit;



    public HexTile getOccupyingHex()
    {
        return occupyingHexTile;
    }



    public void setOccupyingHex(HexTile newOccupyingHexTile)
    {
        occupyingHexTile = newOccupyingHexTile;
    }



    public void setMovePositions(List<Vector2> newMovePositions)
    {
        movePositions = newMovePositions;
    }



    public List<Vector2> getMovePositions()
    {
        return movePositions;
    }



    public void setTeam(uint teamNum)
    {
        teamNumber = teamNum;
        if (teamNum == 1)
        {
            GetComponent<SpriteRenderer>().sprite = tempOtherSprite;
        }
    }



    public uint getTeam()
    {
        return teamNumber;
    }



    public bool beingDestroyed()
    {
        return garbage;
    }



    public void destroyUnit()
    {
        garbage = true;
        DestroyObject(this.gameObject);
    }



    public void setUnitType(UnitType newUnitType)
    {
        if (newUnitType == UnitType.KingUnit)
        {
            if (teamNumber == 0)
            {
                GetComponent<SpriteRenderer>().sprite = King0Sprite;
            }
            else
            {
                GetComponent<SpriteRenderer>().sprite = King1Sprite;
            }
        }
        unitType = newUnitType;
    }

    public UnitType getUnitType()
    {
        return unitType;
    }
}
