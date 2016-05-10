using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Helps store information usefully for hex-tile rotations
public class RotationDirectionObject
{
    public static RotationDirectionObject UP = new RotationDirectionObject(new Vector2(0, 1), new Vector2(1, 0));
    public static RotationDirectionObject UP_RIGHT = new RotationDirectionObject(new Vector2(1, 0), new Vector2(1, -1));
    public static RotationDirectionObject DOWN_RIGHT = new RotationDirectionObject(new Vector2(1, -1), new Vector2(0, -1));
    public static RotationDirectionObject DOWN = new RotationDirectionObject(new Vector2(0, -1), new Vector2(-1, 0));
    public static RotationDirectionObject DOWN_LEFT = new RotationDirectionObject(new Vector2(-1, 0), new Vector2(-1, 1));
    public static RotationDirectionObject UP_LEFT = new RotationDirectionObject(new Vector2(-1, 1), new Vector2(0, 1));

    private Vector2 upDirection;
    private Vector2 rightDirection;

    private RotationDirectionObject(Vector2 upDir, Vector2 rightDir)
    {
        upDirection = upDir;
        rightDirection = rightDir;
    }

    public Vector2 getUpDirection()
    {
        return upDirection;
    }

    public Vector2 getRightDirection()
    {
        return rightDirection;
    }
};

//Relative
public enum RelativeDirection
{
    FORWARD,
    FORWARD_RIGHT,
    BACKWARD_RIGHT,
    BACKWARD,
    BACKWARD_LEFT,
    FORWARD_LEFT,
    NONE
};

//Absolute
public enum AbsoluteDirection
{
    UP,
    UP_RIGHT,
    DOWN_RIGHT,
    DOWN,
    DOWN_LEFT,
    UP_LEFT,
    NONE
};

public class TileController : MonoBehaviour {


	// Use this for initialization
    public GameObject hexTileType;
    public GameObject spawnUnitType;
    public Sprite tempSprite;
    public Sprite tempUntochedSprite;

    //Remember units are in unity units. 100px = 1 unity unit
    public const float TILE_INNER_WIDTH = 0.79f;
    public const float TILE_OUTER_WIDTH = 0.35f;
    public const float TILE_HEIGHT = 0.74f;
    public const float TILE_WIDTH = TILE_INNER_WIDTH + 2*TILE_OUTER_WIDTH;
    public const float TILE_WIDTH_PART = TILE_INNER_WIDTH + TILE_OUTER_WIDTH;

    private int tileMapWidth = 0;
    private int tileMapHeight = 0;
    private Vector2 minCoords = new Vector2(0x7fff, 0x7fff);
    private Vector2 maxCoords = new Vector2(-0x7fff, -0x7fff);
    private List<HexTile> hexTileList;
    private static Vector2 OriginTileWorldOffset = new Vector2(-1.0f,-1.0f);

    private List<HexTile> tempTiles = new List<HexTile>();

    //Temp map positions that will generate the map off of
    private Vector2[] tempMap =
    {
        new Vector2(0,0),
        new Vector2(2,-1),
        new Vector2(4,-2),
        new Vector2(6,-3),

        new Vector2(1,0),
        new Vector2(3,-1),
        new Vector2(5,-2),
        new Vector2(7,-3),

        new Vector2(0,1),
        new Vector2(2,0),
        new Vector2(4,-1),
        new Vector2(6,-2),

        new Vector2(1,1),
        new Vector2(3,0),
        new Vector2(5,-1),
        new Vector2(7,-2),

        new Vector2(0,2),
        new Vector2(2,1),
        new Vector2(4,0),
        new Vector2(6,-1),

        new Vector2(1,2),
        new Vector2(3,1),
        new Vector2(5,0),
        new Vector2(7,-1),

        new Vector2(0,3),
        new Vector2(2,2),
        new Vector2(4,1),
        new Vector2(6,0),

        new Vector2(1,3),
        new Vector2(3,2),
        new Vector2(5,1),
        new Vector2(7,0),

        new Vector2(0,4),
        new Vector2(2,3),
        new Vector2(4,2),
        new Vector2(6,1),

        new Vector2(1,4),
        new Vector2(3,3),
        new Vector2(5,2),
        new Vector2(7,1),

        new Vector2(0,5),
        new Vector2(2,4),
        new Vector2(4,3),
        new Vector2(6,2),

        new Vector2(1,5),
        new Vector2(3,4),
        new Vector2(5,3),
        new Vector2(7,2),

        new Vector2(0,6),
        new Vector2(2,5),
        new Vector2(4,4),
        new Vector2(6,3),

        new Vector2(1,6),
        new Vector2(3,5),
        new Vector2(5,4),
        new Vector2(7,3),

        new Vector2(0,7),
        new Vector2(2,6),
        new Vector2(4,5),
        new Vector2(6,4),

        new Vector2(1,7),
        new Vector2(3,6),
        new Vector2(5,5),
        new Vector2(7,4),

        new Vector2(0,8),
        new Vector2(2,7),
        new Vector2(4,6),
        new Vector2(6,5),

        new Vector2(1,8),
        new Vector2(3,7),
        new Vector2(5,6),
        new Vector2(7,5),


        new Vector2(0,9),
        new Vector2(2,8),
        new Vector2(4,7),
        new Vector2(6,6),

        new Vector2(1,9),
        new Vector2(3,8),
        new Vector2(5,7),
        new Vector2(7,6),


        new Vector2(0,10),
        new Vector2(2,9),
        new Vector2(4,8),
        new Vector2(6,7),

        new Vector2(1,10),
        new Vector2(3,9),
        new Vector2(5,8),
        new Vector2(7,7),


        new Vector2(0,11),
        new Vector2(2,10),
        new Vector2(4,9),
        new Vector2(6,8),

        new Vector2(1,11),
        new Vector2(3,10),
        new Vector2(5,9),
        new Vector2(7,8),

    };
    
    //Spawn points for teams
    private Vector2[] tempTeam0SpawnPoints = 
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(2, -1),
        new Vector2(3, -1),
        new Vector2(4, -2),
        new Vector2(5, -2),
        new Vector2(6, -3),
        new Vector2(7, -3)
    };

    private Vector2[] tempTeam1SpawnPoints =
    {
        new Vector2(0, 11),
        new Vector2(1, 11),
        new Vector2(2, 10),
        new Vector2(3, 10),
        new Vector2(4, 9),
        new Vector2(5, 9),
        new Vector2(6, 8),
        new Vector2(7, 8)
    };

    public AbsoluteDirection[] tempTeamDirections =
    {
        AbsoluteDirection.UP,
        AbsoluteDirection.DOWN
    };


    public void initialize()
    {
        hexTileList = new List<HexTile>();
        buildMap(tempMap);
    }
	
    //Wont create one if a tile already exists at location
    public void addNewTempTile(Vector2 newTempTilePosition)
    {
        if (getTileFromHexCoord(newTempTilePosition) == null)
        {
            GameObject go = (GameObject)Instantiate(hexTileType, hexCoordToPixelCoord(newTempTilePosition), Quaternion.identity);
            HexTile hexTileScript = (HexTile)go.GetComponent(typeof(HexTile));
            hexTileScript.initialize();
            hexTileScript.getSpriteRenderer().sprite = hexTileScript.nonSelectableSprite;
            hexTileScript.setCoords(newTempTilePosition);
            tempTiles.Add(hexTileScript);
        }
    }


    public void clearAllTempTiles()
    {
        for (int i = 0; i < tempTiles.Count; i++ )
        {
            Destroy(tempTiles[i].gameObject);
        }

        tempTiles.Clear();
    }


    HexTile getTempTile(Vector2 hexCoord)
    {
        for (int i = 0; i < tempTiles.Count; i++)
        {
            //print(tempTiles[i].getCoords());
            if (tempTiles[i].getCoords() == hexCoord)
            {
                return tempTiles[i];
            }
        }
        return null;
    }

    void buildMap(Vector2[] hexCoordMap)
    {
        for (int i = 0; i < hexCoordMap.Length; i++)
        {
            Vector2 hexLoc = hexCoordMap[i];

            if (hexLoc.x < minCoords.x) { minCoords.x = hexLoc.x; }
            if (hexLoc.y < minCoords.y) { minCoords.y = hexLoc.y; }
            if (hexLoc.x > maxCoords.x) { maxCoords.x = hexLoc.x; }
            if (hexLoc.y > maxCoords.y) { maxCoords.y = hexLoc.y; }
        }

        tileMapWidth = Mathf.FloorToInt(Mathf.Abs(maxCoords.x - minCoords.x)) + 1;
        tileMapHeight = Mathf.FloorToInt(Mathf.Abs(maxCoords.y - minCoords.y)) + 1;

        print("MapWidth: " + tileMapWidth);
        print("MapHeight: " + tileMapHeight);

        for (int i = 0; i < tileMapWidth * tileMapHeight; i++)
        {
            hexTileList.Add(null);
        }

        for (int i = 0; i < hexCoordMap.Length; i++)
        {
            Vector2 hexLoc = hexCoordMap[i];

            int col = Mathf.FloorToInt(Mathf.Abs(hexCoordMap[i].x - minCoords.x));
            int row = Mathf.FloorToInt(Mathf.Abs(hexCoordMap[i].y - minCoords.y)) * tileMapWidth;

            GameObject go = (GameObject)Instantiate(hexTileType, hexCoordToPixelCoord(hexLoc), Quaternion.identity);
            HexTile hexTileScript = (HexTile)go.GetComponent(typeof(HexTile));
            hexTileScript.initialize();
            hexTileScript.setCoords(hexCoordMap[i]);
            hexTileList[row + col] = hexTileScript;
        }

        //Telling tiles that they could be spawned on
        for (int i = 0; i < tempTeam0SpawnPoints.Length; i++)
        {
            if (getTileFromHexCoord(tempTeam0SpawnPoints[i]) != null)
            {
                getTileFromHexCoord(tempTeam0SpawnPoints[i]).teamSpawnLoc = 0;
                getTileFromHexCoord(tempTeam0SpawnPoints[i]).spawnDirection = tempTeamDirections[0];
            }
        }

        for (int i = 0; i < tempTeam1SpawnPoints.Length; i++)
        {
            if (getTileFromHexCoord(tempTeam1SpawnPoints[i]) != null)
            {
                getTileFromHexCoord(tempTeam1SpawnPoints[i]).teamSpawnLoc = 1;
                getTileFromHexCoord(tempTeam1SpawnPoints[i]).spawnDirection = tempTeamDirections[1];
            }
        }
    }



    public List<HexTile> getAllTiles()
    {
        return hexTileList;
    }



    public HexTile getTileFromHexCoord(Vector2 hexCoord, bool getTempTiles = false)
    {
        HexTile outHexTile = null;
        int hCx = (int)hexCoord.x;int hCy = (int)hexCoord.y;
        if (hCx >= minCoords.x && hCy >= minCoords.y && hCx <= maxCoords.x && hCy <= maxCoords.y)
        {
            outHexTile = hexTileList[getIndexFromHexCoord(hexCoord)];
        }
        if (getTempTiles && outHexTile == null)
        {
            //print("MEOW KITTY CATS!!!");
            outHexTile = getTempTile(hexCoord);
            //print(outHexTile.getCoords());
        }
        return outHexTile;
    }



    int getIndexFromHexCoord(Vector2 hexCoord)
    {
        return Mathf.FloorToInt(Mathf.Abs(hexCoord.y - minCoords.y)) * tileMapWidth + Mathf.FloorToInt(Mathf.Abs(hexCoord.x - minCoords.x));
    }



    /** Note will not reposition the transfering unit if any*/
    public bool transferUnit(HexTile fromTile, HexTile toTile)
    {
        if (fromTile != null && toTile != null)
        {
            if (fromTile.getOccupyingUnit() != null)
            {
                UnitScript occupyingUnit = fromTile.getOccupyingUnit();
                UnitScript oScript = (UnitScript)occupyingUnit.GetComponent(typeof(UnitScript));
                oScript.setOccupyingHex(toTile);
                toTile.setOccupyingUnit(occupyingUnit);
                fromTile.setOccupyingUnit(null);
                return true;
            }
        }
        return false;
    }



    public List<HexTile> getAdjacentTiles(HexTile centerTile, bool getTempTiles = false)
    {
        List<HexTile> adjacents = new List<HexTile>();
        Vector2 centerCoord = centerTile.getCoords();

        adjacents.Add(getTileFromHexCoord(centerCoord + new Vector2(0, 1), getTempTiles));
        adjacents.Add(getTileFromHexCoord(centerCoord + new Vector2(1, 0), getTempTiles));
        adjacents.Add(getTileFromHexCoord(centerCoord + new Vector2(1, -1), getTempTiles));
        adjacents.Add(getTileFromHexCoord(centerCoord + new Vector2(0, -1), getTempTiles));
        adjacents.Add(getTileFromHexCoord(centerCoord + new Vector2(-1, 0), getTempTiles));
        adjacents.Add(getTileFromHexCoord(centerCoord + new Vector2(-1, 1), getTempTiles));

        return adjacents;
    }

    

    public HexTile getTileAtPixelPos(Vector2 pixelPos, bool getTempTiles = false)
    {
        return ( getTileFromHexCoord( pixelCoordToHex(new Vector2(pixelPos.x - OriginTileWorldOffset.x, pixelPos.y - OriginTileWorldOffset.y)), getTempTiles));
    }



    public static Vector2 hexCoordToPixelCoord(Vector2 hexCoord, bool ignoreOffsets = false)
    {
        if (ignoreOffsets)
            return new Vector2(
                hexCoord.x * TILE_WIDTH_PART,
                hexCoord.x / 2 * TILE_HEIGHT + hexCoord.y * TILE_HEIGHT
                );
        return new Vector2(
            hexCoord.x * TILE_WIDTH_PART + OriginTileWorldOffset.x,
            hexCoord.x / 2 * TILE_HEIGHT + hexCoord.y * TILE_HEIGHT + OriginTileWorldOffset.y
            );
    }



    public static Vector2 getOriginTileOffset()
    {
        return OriginTileWorldOffset;
    }



    public static Vector2 pixelCoordToHex(Vector2 pixelCoord)
    {
        float xHexFloaty = (pixelCoord.x) / TILE_WIDTH_PART;
        float yHexFloaty = -(pixelCoord.x) / (2 * TILE_WIDTH_PART) + (pixelCoord.y) / TILE_HEIGHT;

        float x = xHexFloaty;
        float z = yHexFloaty;
        float y = -x - z;

        float rx = Mathf.Round(x);
        float ry = Mathf.Round(y);
        float rz = Mathf.Round(z);

        float dx = Mathf.Abs(rx - x);
        float dy = Mathf.Abs(ry - y);
        float dz = Mathf.Abs(rz - z);

        if (dx > dy && dx > dz)
        {
            rx = -ry - rz;
        }
        else if (dy > dz)
        {
            ry = -rx - rz;
        }
        else
        {
            rz = -rx - ry;
        }

        return new Vector2(Mathf.FloorToInt(rx), Mathf.FloorToInt(rz));
    }



    public void switchTilesStates(List<Vector2> tiles, TileState state = TileState.NONE)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            HexTile tile = getTileFromHexCoord(tiles[i]);
            if (tile != null)
            {
                tile.switchState(state);
            }
        }
    }



    public void switchAllTileStates(TileState state = TileState.NONE)
    {
        for (int i = 0; i < hexTileList.Count; i++)
        {
            if (hexTileList[i] != null)
            {
                hexTileList[i].switchState(state);
            }
        }
    }


    //Checks to see if given Vector is considered an adjacent position relative to (0,0)
    public static bool checkValidAdjacency(Vector2 checkingPosition)
    {
        if (checkingPosition.x == 0)
        {
            if (checkingPosition.y == 1 || checkingPosition.y == -1)
                return true;
            return false;
        }
        else if (checkingPosition.x == 1)
        {
            if (checkingPosition.y == 0 || checkingPosition.y == -1)
                return true;
            return false;
        }
        if (checkingPosition.x == -1)
        {
            if (checkingPosition.y == 0 || checkingPosition.y == 1)
                return true;
            return false;
        }
        return false;
    }

    public static bool checkValidAdjacency(List<Vector2> checkingPositions)
    {
        for (int i = 0; i < checkingPositions.Count; i++)
        {
            if (!checkValidAdjacency(checkingPositions[i]))
                return false;
        }
        return true;
    }

    public static AbsoluteDirection getDirectionToTile(HexTile fromTile, HexTile toTile)
    {
        return getDirectionToTile(fromTile.getCoords(), toTile.getCoords());
    }

    public static AbsoluteDirection getDirectionToTile(Vector2 fromPos, Vector2 toPos)
    {
        //Vector2 fromCoords = new Vector2(0,0);
        Vector2 relCoords = toPos - fromPos;

        if (relCoords[0] == 0 && relCoords[1] == 0)
        {
            return AbsoluteDirection.NONE;
        }

        int relSigns0 = (int)(relCoords[0]/Mathf.Abs(relCoords[0]));
        int relSigns1 = (int)(relCoords[1]/Mathf.Abs(relCoords[1]));

        if (relCoords[0] == 0)
            if (relSigns1 == 1)
                return AbsoluteDirection.UP;
            else
                return AbsoluteDirection.DOWN;
        
        if (relCoords[1] == 0)
            if (relSigns0 == 1)
                return AbsoluteDirection.UP_RIGHT;
            else
                return AbsoluteDirection.DOWN_LEFT;

        if (Mathf.Abs(relCoords[0]) == Mathf.Abs(relCoords[1]))
            if (relSigns0 == 1)
                return AbsoluteDirection.DOWN_RIGHT;
            else
                return AbsoluteDirection.UP_LEFT;

        return AbsoluteDirection.NONE;

    }

    public static AbsoluteDirection relativeToAbsoluteDirection(AbsoluteDirection currentDirection, RelativeDirection relativeDirection)
    {
        int currentDirIdx = (int)currentDirection;
        int relativeDirIdx = (int)relativeDirection;

        currentDirIdx += relativeDirIdx;

        if (currentDirIdx >= 6)
        {
            currentDirIdx -= 6;
        }

        return (AbsoluteDirection)(currentDirIdx);
    }

    public static AbsoluteDirection relativePosToAbsoluteDirection(Vector2 relativePosition)
    {
        if (relativePosition.x == 0)
        {
            if (relativePosition.y == 1)
            {
                return AbsoluteDirection.UP;
            }
            if (relativePosition.y == -1)
            {
                return AbsoluteDirection.DOWN;
            }
        }
        else if (relativePosition.x == 1)
        {
            if (relativePosition.y == 0)
            {
                return AbsoluteDirection.UP_RIGHT;
            }
            if (relativePosition.y == -1)
            {
                return AbsoluteDirection.DOWN_RIGHT;
            }
        }
        else if (relativePosition.x == -1)
        {
            if (relativePosition.y == 0)
            {
                return AbsoluteDirection.DOWN_LEFT;
            }
            if (relativePosition.y == 1)
            {
                return AbsoluteDirection.UP_LEFT;
            }
        }
        throw new UnityException("hex coord: (" + relativePosition + ") is not an adjacent/relative coord");
    }

    public static Vector2 absoluteDirectionToRelativePos(AbsoluteDirection rot)
    {
        return absoluteDirectionToObject(rot).getUpDirection();
    }

    public static RotationDirectionObject absoluteDirectionToObject(AbsoluteDirection rot)
    {
        switch (rot)
        {
            case AbsoluteDirection.UP:
                return RotationDirectionObject.UP;

            case AbsoluteDirection.UP_RIGHT:
                return RotationDirectionObject.UP_RIGHT;

            case AbsoluteDirection.DOWN_RIGHT:
                return RotationDirectionObject.DOWN_RIGHT;

            case AbsoluteDirection.DOWN:
                return RotationDirectionObject.DOWN;

            case AbsoluteDirection.DOWN_LEFT:
                return RotationDirectionObject.DOWN_LEFT;

            case AbsoluteDirection.UP_LEFT:
                return RotationDirectionObject.UP_LEFT;
        }

        return null;
    }

    public static Vector2 rotate(Vector2 originalRelativeLocation, AbsoluteDirection rotationTo)
    {
        RotationDirectionObject rotDirObj = absoluteDirectionToObject(rotationTo);

        return rotDirObj.getUpDirection() * originalRelativeLocation.y + rotDirObj.getRightDirection() * originalRelativeLocation.x;
    }
}
