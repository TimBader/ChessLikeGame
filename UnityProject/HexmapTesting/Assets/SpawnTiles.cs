using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnTiles : MonoBehaviour {


	// Use this for initialization
    public GameObject hexTile;
    public GameObject spawnUnitType;
    public Sprite tempSprite;
    public Sprite tempUntochedSprite;

    //Remember units are in unity units. 100px = 1 unity unit
    public const float TILE_INNER_WIDTH = 0.80f;
    public const float TILE_OUTER_WIDTH = 0.35f;
    public const float TILE_HEIGHT = 0.70f;
    public const float TILE_WIDTH = TILE_INNER_WIDTH + 2*TILE_OUTER_WIDTH;
    public const float TILE_WIDTH_PART = TILE_INNER_WIDTH + TILE_OUTER_WIDTH;

    private int tileMapWidth = 0;
    private int tileMapHeight = 0;
    private Vector2 minCoords = new Vector2(0x7fff, 0x7fff);
    private Vector2 maxCoords = new Vector2(-0x7fff, -0x7fff);
    private List<HexTile> hexTileList;
    private Vector2 OriginTileWorldOffset = new Vector2(-1.0f,-1.0f);

    //Temp map positions that will generate the map off of
    private Vector2[] tempMap = 
    {
      new Vector2(-3,0),
      new Vector2(-3,1),
      new Vector2(-3,2),
      new Vector2(-3,3),

      new Vector2(-2,0),
      new Vector2(-2,1),
      new Vector2(-2,2),

      new Vector2(-1,-1),
      new Vector2(-1,0),
      new Vector2(-1,1),
      //new Vector2(-1,2),

      new Vector2(0,-1),
      new Vector2(0,0),
      //new Vector2(0,1),

      new Vector2(1,-2),
      new Vector2(1,-1),
      new Vector2(1,0),
      //new Vector2(1,1),

      new Vector2(2,-2),
      new Vector2(2,-1),
      new Vector2(2,0),

      new Vector2(3,-3),
      new Vector2(3,-2),
      new Vector2(3,-1),
      new Vector2(3,0),


      new Vector2(-3, 4),
      new Vector2(-3, 5),
      new Vector2(-3, 6),
      new Vector2(-3, 7),

      new Vector2(-2, 3),
      new Vector2(-2, 4),
      new Vector2(-2, 5),
      new Vector2(-2, 6),

      //new Vector2(-1, 3),
      new Vector2(-1, 4),
      new Vector2(-1, 5),
      new Vector2(-1, 6),

      //new Vector2(0, 2),
      //new Vector2(0, 3),
      new Vector2(0, 4),
      new Vector2(0, 5),

      //new Vector2(1, 2),
      new Vector2(1, 3),
      new Vector2(1, 4),
      new Vector2(1, 5),

      new Vector2(2, 1),
      new Vector2(2, 2),
      new Vector2(2, 3),
      new Vector2(2, 4),

      new Vector2(3, 1),
      new Vector2(3, 2),
      new Vector2(3, 3),
      new Vector2(3, 4)
    };

    //Spawn points for teams
    private Vector2[] tempTeam0SpawnPoints = 
    {
        new Vector2(-3, 0),
        new Vector2(-2, 0),
        new Vector2(-1, -1),
        new Vector2(0, -1),
        new Vector2(1, -2),
        new Vector2(2, -2),
        new Vector2(3, -3)
    };

    private Vector2[] tempTeam1SpawnPoints =
    {
        new Vector2(-3, 7),
        new Vector2(-2, 6),
        new Vector2(-1, 6),
        new Vector2(-0, 5),
        new Vector2(1, 5),
        new Vector2(2, 4),
        new Vector2(3, 4)
    };



    public void initialize()
    {
        hexTileList = new List<HexTile>();
        buildMap(tempMap);
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

            GameObject go = (GameObject)Instantiate(hexTile, hexCoordToPixelCoord(hexLoc), Quaternion.identity);
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
            }
        }

        for (int i = 0; i < tempTeam1SpawnPoints.Length; i++)
        {
            if (getTileFromHexCoord(tempTeam1SpawnPoints[i]) != null)
            {
                getTileFromHexCoord(tempTeam1SpawnPoints[i]).teamSpawnLoc = 1;
            }
        }
    }



    public List<HexTile> getAllTiles()
    {
        return hexTileList;
    }



    public HexTile getTileFromHexCoord(Vector2 hexCoord)
    {
        int hCx = (int)hexCoord.x;int hCy = (int)hexCoord.y;
        if (hCx >= minCoords.x && hCy >= minCoords.y && hCx <= maxCoords.x && hCy <= maxCoords.y)
        {
            return hexTileList[getIndexFromHexCoord(hexCoord)];
        }
        return null;
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



    public List<HexTile> getAdjacentTiles(HexTile centerTile)
    {
        List<HexTile> adjacents = new List<HexTile>();
        Vector2 centerCoord = centerTile.getCoords();
        for (int y = -1; y < 2; y++ )
        {
            for (int x = -1; x < 2; x++)
            {
                if ((x == 0 && y == 0) || (x == -1 && y == -1) || (x == 1 && y == 1))
                    continue;
                //GameObject aGo = getTileFromCoords(new Vector2(centerCoord.x+x, centerCoord.y+y));
                HexTile aTile = getTileFromHexCoord(new Vector2(centerCoord.x + x, centerCoord.y + y));
                if (aTile != null)
                    adjacents.Add(aTile);
            }
        }
        return adjacents;
    }

    

    public HexTile getTileAtPixelPos(Vector2 pixelPos)
    {
        return ( getTileFromHexCoord( pixelCoordToHex(new Vector2(pixelPos.x - OriginTileWorldOffset.x, pixelPos.y - OriginTileWorldOffset.y))));
    }



    public Vector2 hexCoordToPixelCoord(Vector2 hexCoord)
    {
        return new Vector2(
            hexCoord.x * TILE_WIDTH_PART + OriginTileWorldOffset.x,
            hexCoord.x / 2 * TILE_HEIGHT + hexCoord.y * TILE_HEIGHT + OriginTileWorldOffset.y
            );
    }



    public Vector2 getOriginTileOffset()
    {
        return OriginTileWorldOffset;
    }



    public Vector2 pixelCoordToHex(Vector2 pixelCoord)
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

}
