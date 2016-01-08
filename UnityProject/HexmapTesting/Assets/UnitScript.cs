using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum UnitType
{
    BasicUnit,
    KingUnit
};

public class UnitScript : MonoBehaviour {

    private HexTile occupyingHexTile = null;
    //private List<Vector2> movePositions;
    private uint teamNumber = 0;
    private bool garbage = false;
    private UnitType unitType = UnitType.BasicUnit;

    private SpriteRenderer colorsSpriteRenderer = null;
    private SpriteRenderer baseSpriteRenderer = null;

    private UnitInfo unitInfo;

    private AbsoluteDirection rotationDirection = AbsoluteDirection.UP;

    public HexTile getOccupyingHex()
    {
        return occupyingHexTile;
    }

    public void initialize(UnitInfo info)
    {
        SpriteRenderer[] list = GetComponentsInChildren<SpriteRenderer>();
        if (list.Length != 2)
        {
            throw new UnityException("Dude... you need two childern of a unit with spriterenders in them... re-attach them please");
        }
        unitInfo = info;
        baseSpriteRenderer = list[0];
        colorsSpriteRenderer = list[1];
    }



    public UnitInfo getUnitInfo()
    {
        return unitInfo;
    }



    public void setOccupyingHex(HexTile newOccupyingHexTile)
    {
        occupyingHexTile = newOccupyingHexTile;
    }


    public void setRotationDirection(AbsoluteDirection rotationDir)
    {
        rotationDirection = rotationDir;
    }

    public AbsoluteDirection getRotation()
    {
        return rotationDirection;
    }

    /*public void setMovePositions(List<Vector2> newMovePositions)
    {
        movePositions = newMovePositions;
    }



    public List<Vector2> getMovePositions()
    {
        return movePositions;
    }*/



    public SpriteRenderer getBaseSpriteRenderer()
    {
        return baseSpriteRenderer;
    }



    public SpriteRenderer getColorsSpriteRenderer()
    {
        return colorsSpriteRenderer;
    }



    public void setTeam(uint teamNum)
    {
        teamNumber = teamNum;
        /*if (teamNum == 1)
        {
            GetComponent<SpriteRenderer>().sprite = tempOtherSprite;
        }*/
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
        DestroyObject(baseSpriteRenderer);
        DestroyObject(colorsSpriteRenderer);
        DestroyObject(this.gameObject);
        
    }



    public void setUnitType(UnitType newUnitType)
    {
        unitType = newUnitType;
    }



    public UnitType getUnitType()
    {
        return unitType;
    }
}
