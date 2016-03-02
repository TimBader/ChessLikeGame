using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum UnitType
{
    BasicUnit,
    LordUnit
};

public class UnitScript : MonoBehaviour {

    private Vector2 spriteOffset = new Vector2(0.0f,0.0f);

    /// FIND A BETTER WAY
    public static GameControllerScript gameControllerRef = null;

    private HexTile occupyingHexTile = null;
    //private List<Vector2> movePositions;
    private int teamNumber = -1;
    private bool garbage = false;
    private UnitType unitType = UnitType.BasicUnit;

    private SpriteRenderer baseSpriteRenderer = null;
    private SpriteRenderer color0SpriteRenderer = null;
    private SpriteRenderer color1SpriteRenderer = null;
    private SpriteRenderer color2SpriteRenderer = null;
    private SpriteRenderer color3SpriteRenderer = null;

    private UnitInfo unitInfo;

    private AbsoluteDirection rotationDirection = AbsoluteDirection.UP;

    private GameObject rotationIndicator = null;

    public bool justSpawned = false;

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
        if (list.Length != 5)
        {
            throw new UnityException("Dude... you need two childern of a unit with spriterenders in them... re-attach them please");
        }
        unitInfo = info.clone();
        baseSpriteRenderer = list[0];
        color0SpriteRenderer = list[1];
        color1SpriteRenderer = list[2];
        color2SpriteRenderer = list[3];
        color3SpriteRenderer = list[4];

        //teamNumber = team;

        if (info.baseSpriteName != null)
        {
            getBaseSpriteRenderer().sprite = SpriteResourceManager.loadSprite(info.baseSpriteName);
        }
        if (info.color0SpriteName != null)
        {
            getColor0SpriteRenderer().sprite = SpriteResourceManager.loadSprite(info.color0SpriteName);
        }
        if (info.color1SpriteName != null)
        {
            getColor1SpriteRenderer().sprite = SpriteResourceManager.loadSprite(info.color1SpriteName);
        }
        if (info.color2SpriteName != null)
        {
            getColor2SpriteRenderer().sprite = SpriteResourceManager.loadSprite(info.color2SpriteName);
        }
        if (info.color3SpriteName != null)
        {
            getColor3SpriteRenderer().sprite = SpriteResourceManager.loadSprite(info.color3SpriteName);
        }

        /*for (int i = 0; i < unitInfo.movementObjects.Count; i++)
        {
            unitInfo.movementObjects[i] = unitInfo.movementObjects[i].clone();//Create unit's own movementtype
        }*/

        if (team >= 0)
        {
            setTeam(team);
            //getColorsSpriteRenderer().color = TeamControllerScript.getTeamColor(team);
        } 


        if (unitInfo.rotationEnabled)
        {
            // Rotation Indicator
            Vector2 pos = gameControllerRef.getTileController().hexCoordToPixelCoord(this.transform.position);
            rotationIndicator = (GameObject)Instantiate(new GameObject(), new Vector3(pos.x, pos.y, 0), Quaternion.identity);

            rotationIndicator.transform.parent = this.transform;

            SpriteRenderer spr = rotationIndicator.AddComponent<SpriteRenderer>();

            spr.sprite = SpriteResourceManager.loadSprite("RotationIndicator");
            spr.sortingLayerName = "SpriteLayer";
            spr.sortingOrder = 1000; // We want this to be above everythign else

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
        this.transform.position = gameControllerRef.getTileController().hexCoordToPixelCoord(newOccupyingHexTile.getCoords()) + spriteOffset;

        if (this.baseSpriteRenderer)
            this.baseSpriteRenderer.sortingOrder = (int)(this.transform.position.y * -100);
        if (this.color0SpriteRenderer)
            this.color0SpriteRenderer.sortingOrder = (int)(this.transform.position.y * -100);
        if (this.color1SpriteRenderer)
            this.color1SpriteRenderer.sortingOrder = (int)(this.transform.position.y * -100);
        if (this.color2SpriteRenderer)
            this.color2SpriteRenderer.sortingOrder = (int)(this.transform.position.y * -100);
        if (this.color3SpriteRenderer)
            this.color3SpriteRenderer.sortingOrder = (int)(this.transform.position.y * -100);
    }


    public void setRotationDirection(AbsoluteDirection rotationDir)
    {
        //print(unitInfo.baseSpriteName)
        if (unitInfo.rotationEnabled)
        {
            rotationDirection = rotationDir;
            if (rotationIndicator)
            {
                // Rotation Indicator
                rotationIndicator.transform.position = this.transform.position;

                Vector2 q = gameControllerRef.getTileController().hexCoordToPixelCoord(SpawnTiles.absoluteDirectionToObject(rotationDir).getUpDirection(), true).normalized;
                rotationIndicator.transform.position = rotationIndicator.transform.position + new Vector3(q.x * 0.3f, q.y * 0.3f, 0.0f);

                rotationIndicator.transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(q.x, q.y) * 180.0f/Mathf.PI - 90.0f, new Vector3(0.0f,0.0f,-1.0f));
            }

            for (int i = 0; i < unitInfo.movementObjects.Count; i++)
            {
                unitInfo.movementObjects[i].unitRotated(rotationDir);
            }
        }
        /*else
        {
            throw new UnityException(unitInfo.unitName + " cannot rotate but rotate function was called on it");
        }*/
    }

    public AbsoluteDirection getRotation()
    {
        return rotationDirection;
    }



    public SpriteRenderer getBaseSpriteRenderer()
    {
        return baseSpriteRenderer;
    }
    public SpriteRenderer getColor0SpriteRenderer()
    {
        return color0SpriteRenderer;
    }
    public SpriteRenderer getColor1SpriteRenderer()
    {
        return color1SpriteRenderer;
    }
    public SpriteRenderer getColor2SpriteRenderer()
    {
        return color2SpriteRenderer;
    }
    public SpriteRenderer getColor3SpriteRenderer()
    {
        return color3SpriteRenderer;
    }



    public void setTeam(int teamNum)
    {
        teamNumber = teamNum;
        if (teamNum >= 0)
        {
            TeamColorPallet tCP = TeamColorPallet.getTeamColorPallet(teamNum);
            getColor0SpriteRenderer().color = tCP.getColor(0);
            getColor1SpriteRenderer().color = tCP.getColor(1);
            getColor2SpriteRenderer().color = tCP.getColor(2);
            getColor3SpriteRenderer().color = tCP.getColor(3);
            //getColor0SpriteRenderer().color = TeamControllerScript.getTeamColor(teamNum);
        } 
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
        DestroyObject(color0SpriteRenderer);
        DestroyObject(color1SpriteRenderer);
        DestroyObject(color2SpriteRenderer);
        DestroyObject(color3SpriteRenderer);
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
