using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum UnitType
{
    BasicUnit,
    KingUnit
};

public class UnitScript : MonoBehaviour {

    /// FIND A BETTER WAY
    public static GameControllerScript gameControllerRef = null;

    private HexTile occupyingHexTile = null;
    //private List<Vector2> movePositions;
    private int teamNumber = 0;
    private bool garbage = false;
    private UnitType unitType = UnitType.BasicUnit;

    private SpriteRenderer colorsSpriteRenderer = null;
    private SpriteRenderer baseSpriteRenderer = null;

    private UnitInfo unitInfo;

    private AbsoluteDirection rotationDirection = AbsoluteDirection.UP;

    private GameObject rotationIndicator = null;

    public HexTile getOccupyingHex()
    {
        return occupyingHexTile;
    }

    public Vector2 getCoords()
    {
        return occupyingHexTile.getCoords();
    }

    public void initialize(UnitInfo info, int team = -1)
    {
        SpriteRenderer[] list = GetComponentsInChildren<SpriteRenderer>();
        if (list.Length != 2)
        {
            throw new UnityException("Dude... you need two childern of a unit with spriterenders in them... re-attach them please");
        }
        unitInfo = info;
        baseSpriteRenderer = list[0];
        colorsSpriteRenderer = list[1];

        //teamNumber = team;

        if (info.baseSpriteName != null)
        {
            getBaseSpriteRenderer().sprite = SpriteResourceManager.loadSprite(info.baseSpriteName);
        }
        if (info.colorsSpriteName != null)
        {
            getColorsSpriteRenderer().sprite = SpriteResourceManager.loadSprite(info.colorsSpriteName);
        }

        if (team >= 0)
        {
            setTeam(team);
            //getColorsSpriteRenderer().color = TeamControllerScript.getTeamColor(team);
        } 

        if (unitInfo.rotationEnabled)
        {
            Vector2 pos = gameControllerRef.getTileController().hexCoordToPixelCoord(getCoords());
            rotationIndicator = (GameObject)Instantiate(new GameObject(), new Vector3(pos.x, pos.y, 0), Quaternion.identity);

            rotationIndicator.transform.parent = this.transform;

            SpriteRenderer spr = rotationIndicator.AddComponent<SpriteRenderer>();

            spr.sprite = SpriteResourceManager.loadSprite("RotationIndicator");

            setRotationDirection(AbsoluteDirection.UP);
 
        }
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

        if (unitInfo.rotationEnabled)
        {
            if (rotationIndicator)
            {
                rotationIndicator.transform.position = this.transform.position;

                Vector2 q = gameControllerRef.getTileController().hexCoordToPixelCoord(SpawnTiles.rotationDirectionToObject(rotationDir).getUpDirection() - gameControllerRef.getTileController().hexCoordToPixelCoord(new Vector2()));

                q = q.normalized;
                rotationIndicator.transform.position = rotationIndicator.transform.position + new Vector3(q.x * 0.3f, q.y * 0.3f, 0.0f);

                float radians = Mathf.Atan2(q.x, q.y);

                rotationIndicator.transform.rotation = Quaternion.AngleAxis(radians * 180/Mathf.PI - 90.0f, new Vector3(0.0f,0.0f,-1.0f));
                //print(Quaternion.AngleAxis(1.0f, new Vector3(0.0f, 0.0f, 1.0f)));
            }
        }
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



    public void setTeam(int teamNum)
    {
        teamNumber = teamNum;
        if (teamNum >= 0)
        {
            getColorsSpriteRenderer().color = TeamControllerScript.getTeamColor(teamNum);
        } 
        /*if (teamNum == 1)
        {
            GetComponent<SpriteRenderer>().sprite = tempOtherSprite;
        }*/
    }



    public int getTeam()
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
