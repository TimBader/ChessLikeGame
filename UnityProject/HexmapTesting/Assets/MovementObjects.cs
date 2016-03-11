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
    //This is just to serve as a parent so we can stored all childern of this parent in one place... so far I cant think of anything MovementTypeParent should have
    protected static GameControllerScript gameControllerRef = null;

    protected MovementTypes movementType = MovementTypes.Path;

    public MovementTypeParent() { }

    public List<MovementIcon> movementIcons = new List<MovementIcon>();

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

    public void clearMovementIcons()
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
    }
};




//******************************************************************************************************************************************************************************************************************
        //  ------  Path  ------  //
//******************************************************************************************************************************************************************************************************************

public class PathMoveType : MovementTypeParent
{
    //Helper class to combine a hex position and a boolean
    public class PathPos
    {
        public Vector2 pos;
        //Tells whether or not this position is able to be moved to
        public bool moveable;

        public PathPos(Vector2 pathPos, bool canMoveTo)
        {
            pos = pathPos;
            moveable = canMoveTo;
        }
    };

    public List<List<PathPos>> pathList = new List<List<PathPos>>();
    public List<List<PathPos>> adjustedPathList = new List<List<PathPos>>();

    public PathMoveType(PathPos[][] paths, bool verify=false)
    {
        movementType = MovementTypes.Path;

        pathList = new List<List<PathPos>>();
        
        for (int i = 0; i < paths.Length; i++)
        {
            List<List<PathPos>> newPart = new List<List<PathPos>>();
            pathList.Add(Util.toList(paths[i]));
        }

        if (verify)
        {
            verifyMove();
        }

        unitRotated(AbsoluteDirection.UP);
    }


    public PathMoveType(List<List<PathPos>> paths, bool verify=false)
    {
        movementType = MovementTypes.Path;

        pathList = paths;

        unitRotated(AbsoluteDirection.UP);
    }

    public override void startSelectingInMode(UnitScript selectedUnit, int currentTeam)
    {
        //MarkingEm
        for (int i = 0; i < adjustedPathList.Count; i++ )
        {
            bool blocked = false;
            for (int j = 0; j < adjustedPathList[i].Count; j++)
            {
                if (blocked)
                {
                    HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + adjustedPathList[i][j].pos);
                    continue;
                }
                else
                {
                    HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + adjustedPathList[i][j].pos);
                    if (tile)
                    {
                        if (tile.getOccupyingUnit())
                        {
                            blocked = true;
                            if (adjustedPathList[i][j].moveable)
                            {
                                if (tile.getOccupyingUnitTeam() != selectedUnit.getTeam())
                                {
                                    tile.switchState(TileState.SELECTABLE);
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            if (adjustedPathList[i][j].moveable)
                            {
                                tile.switchState(TileState.SELECTABLE);
                                continue;
                            }
                        }
                    }
                    else
                    {
                        blocked = true;
                    }
                }
            }
        }
        drawMovementIcons(selectedUnit);
    }

    public override void clickedInMode(HexTile clickedTile, UnitScript selectedUnit, int currentTeam)
    {
        if (clickedTile.getCurrentTileState() == TileState.SELECTABLE)
        {
            if (clickedTile.getOccupyingUnit())
            {
                gameControllerRef.captureUnit(clickedTile.getOccupyingUnit());
            }

            gameControllerRef.getTileController().transferUnit(selectedUnit.getOccupyingHex(), clickedTile);
            gameControllerRef.switchInteractionState(InteractionStates.SelectingUnitToRotate);
        }
    }

    public override bool canMove(UnitScript selectedUnit, int currentTeam)
    {
        //Checking Em
        for (int i = 0; i < adjustedPathList.Count; i++)
        {
            for (int j = 0; j < adjustedPathList[i].Count; j++)
            {
                HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + adjustedPathList[i][j].pos);
                if (tile)
                {
                    if (tile.getOccupyingUnit())
                    {
                        if (adjustedPathList[i][j].moveable)
                        {
                            if (tile.getOccupyingUnitTeam() != selectedUnit.getTeam())
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (adjustedPathList[i][j].moveable)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }

        //gameControllerRef.printString("Found no blocking nor place to move to in PathMoveType");
        return false;
    }

    public override void unitRotated(AbsoluteDirection direction)
    {
        //Adjusting Positions
        adjustedPathList = new List<List<PathPos>>();
        for (int i = 0; i < pathList.Count; i++)
        {
            List<PathPos> pathPart = new List<PathPos>();
            for (int j = 0; j < pathList[i].Count; j++)
            {
                pathPart.Add(new PathPos(SpawnTiles.rotate(pathList[i][j].pos, direction), pathList[i][j].moveable));
            }
            adjustedPathList.Add(pathPart);
        }
    }

    public override void verifyMove()
    {
        bool foundOne = false;
        for (int i = 0; i < pathList.Count; i++)
        {
            for (int j = 0; j < pathList[i].Count; j++)
            {
                if (pathList[i][j].moveable)
                {
                    foundOne = true;
                    break;
                }
            }
            if (foundOne)
                break;
        }
        if (!foundOne)
        {
            throw new UnityException("Issue: Was unable to find a position in a PathMoveType is set to moveable");
        }

        //throw new System.NotImplementedException();
    }

    public override MovementTypeParent clone()
    {
        return new PathMoveType(pathList);
    }

    public override void drawMovementIcons(UnitScript selectedUnit)
    {
        for (int i = 0; i < adjustedPathList.Count; i++)
        {
            bool blocked = false;
            HexTile lastTile = selectedUnit.getOccupyingHex();
            createMovementIcon("PathIcon", selectedUnit.getCoords(), Color.blue, 2);
            for (int j = 0; j < adjustedPathList[i].Count; j++)
            {
                Vector2 tileCoords = selectedUnit.getOccupyingHex().getCoords() + adjustedPathList[i][j].pos;
                HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(tileCoords);
                if (tile)
                {
                    Color color = new Color(0.0f, 0.0f, 1.0f, 1.0f);
                    int depth = 2;//to allow blocked symbols to appear under non-blocked ones
                    if (blocked)
                    {
                        color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
                        depth = 1;
                    }
                    if (tile.getOccupyingUnit())
                    {
                        if (!blocked)
                        {
                            blocked = true;
                            if (tile.getOccupyingUnitTeam() == selectedUnit.getTeam() || !adjustedPathList[i][j].moveable)
                            {
                                createMovementIcon("CrossIcon", tileCoords, new Color(1.0f, 0.0f, 0.0f, 1.0f), 3);
                            }
                            else
                            {
                                createMovementIcon("AttackIcon", tileCoords, new Color(1.0f, 0.0f, 0.0f, 1.0f), 3);
                            }
                        }
                    }

                    Vector2 lastCoords = SpawnTiles.hexCoordToPixelCoord(lastTile.getCoords());
                    Vector2 currentCoords = SpawnTiles.hexCoordToPixelCoord(tileCoords);
                    MovementIcon m = createMovementIcon("PathConnectionIcon", lastCoords, color, 0, true);

                    Vector2 Q = currentCoords - lastCoords;
                    float dist = Q.magnitude * 100;

                    if (adjustedPathList[i][j].moveable)
                    {
                        createMovementIcon("PathSelectableIcon", tileCoords, color, depth);
                        dist -= 15;
                        if (dist < 0)
                            dist = 0;
                    }
                    else
                    {
                        createMovementIcon("PathIcon", tileCoords, color, depth);
                    }

                    float angle = Mathf.Atan2(Q.y, Q.x) * 180 / Mathf.PI;
                    gameControllerRef.printString(dist.ToString());
                    m.transform.Rotate(Vector3.forward, angle);
                    m.transform.localScale = new Vector2(dist / 24, 1.0f);

                    lastTile = tile;
                }
                else
                {
                    createMovementIcon("CrossIcon", tileCoords, new Color(1.0f, 0.0f, 0.0f, 1.0f), 3);
                    break;
                }
            }
        }
    }
}



//******************************************************************************************************************************************************************************************************************
        //  ------  Jump  ------  //
//******************************************************************************************************************************************************************************************************************

public class JumpMoveType : MovementTypeParent
{
    public List<Vector2> jumpPositions = new List<Vector2>();
    public List<Vector2> adjustedJumpPositions = new List<Vector2>();

    public JumpMoveType(Vector2[] jumpMovementPositions, bool verify=false)
    {
        jumpPositions = Util.toList(jumpMovementPositions);
        movementType = MovementTypes.Jump;

        if (verify)
        {
            verifyMove();
        }

        unitRotated(AbsoluteDirection.UP);
    }

    public JumpMoveType(List<Vector2> jumpMovementPositions, bool verify = false)
    {
        jumpPositions = jumpMovementPositions;
        movementType = MovementTypes.Jump;

        if (verify)
        {
            verifyMove();
        }

        unitRotated(AbsoluteDirection.UP);
    }


    public override void startSelectingInMode(UnitScript selectedUnit, int currentTeam)
    {
        for (int i = 0; i < adjustedJumpPositions.Count; i++)
        {
            HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(adjustedJumpPositions[i] + selectedUnit.getOccupyingHex().getCoords());

            if (tile != null)
            {
                if (tile.getOccupyingUnit() != null)
                {
                    if (tile.getOccupyingUnit().getTeam() != currentTeam)
                    {
                        tile.switchState(TileState.SELECTABLE);
                    }
                }
                else
                {
                    tile.switchState(TileState.SELECTABLE);
                }
            }
        }

        drawMovementIcons(selectedUnit);
    }


    public override void clickedInMode(HexTile clickedTile, UnitScript selectedUnit, int currentTeam)
    {
        if (clickedTile.getCurrentTileState() == TileState.SELECTABLE)
        {
            if (clickedTile.getOccupyingUnit())
            {
                gameControllerRef.captureUnit(clickedTile.getOccupyingUnit());
            }

            gameControllerRef.getTileController().transferUnit(selectedUnit.getOccupyingHex(), clickedTile);
            //selectedUnit.transform.position = gameControllerRef.getTileController().hexCoordToPixelCoord(clickedTile.getCoords());

            gameControllerRef.switchInteractionState(InteractionStates.SelectingUnitToRotate);
        }
    }


    public override bool canMove(UnitScript selectedUnit, int currentTeam)
    {
        for (int i = 0; i < jumpPositions.Count; i++ )
        {
            HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + adjustedJumpPositions[i]);
            if (tile)
            {
                if (tile.getOccupyingUnit())
                {
                    if (tile.getOccupyingUnit().getTeam() != currentTeam)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;                        
                }
           }
        }
        return false;
    }

    public override void unitRotated(AbsoluteDirection direction)
    {
        adjustedJumpPositions = new List<Vector2>();
        for (int i = 0; i < jumpPositions.Count; i++)
        {
            adjustedJumpPositions.Add(SpawnTiles.rotate(jumpPositions[i], direction));
        }
    }

    public override void verifyMove()
    {
        if (!testForInvalidPositions(jumpPositions))
        {
            throw new UnityException("Jump Locations have an invalid positions (0,0)");
        }
    }

    public override MovementTypeParent clone()
    {
        return new JumpMoveType(jumpPositions);
    }

    public override void drawMovementIcons(UnitScript selectedUnit)
    {
        for (int i = 0; i < jumpPositions.Count; i++)
        {
            HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + adjustedJumpPositions[i]);
            if (tile)
            {
                createMovementIcon("JumpIcon", tile.getCoords(), new Color(1.0f,0.0f,1.0f,1.0f), 0);
                if (tile.getOccupyingUnit())
                {
                    if (tile.getOccupyingUnit().getTeam() != selectedUnit.getTeam())
                    {
                        createMovementIcon("AttackIcon", tile.getCoords(), new Color(1.0f, 0.0f, 0.0f, 1.0f), 1);
                    }
                    else
                    {
                        createMovementIcon("CrossIcon", tile.getCoords(), new Color(1.0f, 0.0f, 0.0f, 1.0f), 1);
                    }
                }
            }
        }
        //MovementIcon m = new MovementIcon();
        //m.initialize("PlaceIcon", );
    }
};



//******************************************************************************************************************************************************************************************************************
        //  ------  Slide  ------  //
//******************************************************************************************************************************************************************************************************************

public class SlideMoveType : MovementTypeParent
{
    public List<RelativeDirection> directions = new List<RelativeDirection>();
    public List<Vector2> adjustedDirections = new List<Vector2>();
    public List<int> ranges = new List<int>(); //-1 for infinite range

    public SlideMoveType(RelativeDirection[] slideDirections, int[] slideRanges, bool verify=false)
    {
        directions = Util.toList(slideDirections);
        ranges = Util.toList(slideRanges);
        movementType = MovementTypes.Slide;

        if (verify)
        {
            verifyMove();
        }

        unitRotated(AbsoluteDirection.UP);
    }

    public SlideMoveType(List<RelativeDirection> slideDirections, List<int> slideRanges, bool verify = false)
    {
        directions = slideDirections;
        ranges = slideRanges;
        movementType = MovementTypes.Slide;

        if (verify)
        {
            verifyMove();
        }

        unitRotated(AbsoluteDirection.UP);
    }

    public override void startSelectingInMode(UnitScript selectedUnit, int currentTeam)
    {
        for (int i = 0; i < adjustedDirections.Count; i++)
        {
            int j = 0;
            HexTile prevTile = null;
            Vector2 currentTileLoc = selectedUnit.getOccupyingHex().getCoords();
            while (j < ranges[i] || ranges[i] == -1)
            {
                currentTileLoc += adjustedDirections[i];

                HexTile currentTile = gameControllerRef.getTileController().getTileFromHexCoord(currentTileLoc);

                //prevTile = currentTile;
                if (currentTile)
                {
                    if (currentTile.getOccupyingUnit() == null)
                    {
                        currentTile.switchState(TileState.SELECTABLE);

                        if (prevTile)
                        {
                            prevTile.switchState(TileState.NONE);
                        }
                    }
                    else
                    {
                        if (currentTile.getOccupyingUnit().getTeam() != currentTeam)
                        {
                            currentTile.switchState(TileState.SELECTABLE);
                            if (prevTile)
                            {
                                prevTile.switchState(TileState.NONE);
                            }
                        }
                        break;
                    }

                    prevTile = currentTile;
                }
                else
                {
                    break;
                }
                j++;
            }
        }

        drawMovementIcons(selectedUnit);
    }

    public override void clickedInMode(HexTile clickedTile, UnitScript selectedUnit, int currentTeam)
    {
        if (clickedTile.getCurrentTileState() == TileState.SELECTABLE)
        {
            if (clickedTile.getOccupyingUnit())
            {
                gameControllerRef.captureUnit(clickedTile.getOccupyingUnit());
            }

            gameControllerRef.getTileController().transferUnit(selectedUnit.getOccupyingHex(), clickedTile);
            //selectedUnit.transform.position = gameControllerRef.getTileController().hexCoordToPixelCoord(clickedTile.getCoords());
            //switchToNextTeam();

            gameControllerRef.switchInteractionState(InteractionStates.SelectingUnitToRotate);
        }
    }

    public override bool canMove(UnitScript selectedUnit, int currentTeam)
    {
        for (int i = 0; i < directions.Count; i++ )
        {
            HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + adjustedDirections[i]);
            if (tile)
            {
                if (tile.getOccupyingUnit())
                {
                    if (tile.getOccupyingUnit().getTeam() != currentTeam)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
        }

        return false;
    }

    public override void unitRotated(AbsoluteDirection direction)
    {
        adjustedDirections = new List<Vector2>();
        for (int i = 0; i < directions.Count; i++)
        {
            adjustedDirections.Add(SpawnTiles.absoluteDirectionToObject(SpawnTiles.relativeToAbsoluteDirection(direction, directions[i])).getUpDirection());
        }
    }

    public override void verifyMove()
    {
        if (directions.Count != ranges.Count)
        {
            throw new UnityException("The amount of Slide Directions do not match the amount of slide ranges");
        }

        List<RelativeDirection> tempList = new List<RelativeDirection>();
        for (int i = 0; i < ranges.Count; i++)
        {
            if (ranges[i] < -1 || ranges[i] == 0)
            {
                throw new UnityException("Slide Ranges has an invalid range (< -1) || (= 0)");
            }

            if (tempList.Contains(directions[i]))
            {
                throw new UnityException("Slide Directions must not be repeated");
            }
        }
    }

    public override MovementTypeParent clone()
    {
        return new SlideMoveType(directions, ranges);
    }

    public override void drawMovementIcons(UnitScript selectedUnit)
    {
        for (int i = 0; i < adjustedDirections.Count; i++)
        {
            int j = 0;
            Vector2 currentTileLoc = selectedUnit.getOccupyingHex().getCoords();
            HexTile lastTile = null;
            createMovementIcon("PathIcon", selectedUnit.getCoords(), Color.cyan, 3);
            while (j < ranges[i] || ranges[i] == -1)
            {
                currentTileLoc += adjustedDirections[i];

                HexTile currentTile = gameControllerRef.getTileController().getTileFromHexCoord(currentTileLoc);

                //prevTile = currentTile;
                if (currentTile)
                {
                    if (currentTile.getOccupyingUnit())
                    {
                        if (currentTile.getOccupyingUnit().getTeam() != selectedUnit.getTeam())
                        {
                            lastTile = currentTile;
                            createMovementIcon("AttackIcon", currentTile.getCoords(), Color.red, 4);
                        }
                        else
                        {
                            createMovementIcon("CrossIcon", currentTile.getCoords(), Color.red, 4);
                        }
                        break;
                    }
                    lastTile = currentTile;

                }
                else
                {
                    createMovementIcon("CrossIcon", currentTileLoc, Color.red, 4);
                    break;
                }
                j++;
            }

            if (lastTile)
            {
                MovementIcon m = createMovementIcon("PathConnectionIcon", selectedUnit.getCoords(), Color.cyan, 1);
                Vector2 lastTileCoords = SpawnTiles.hexCoordToPixelCoord(lastTile.getCoords());
                Vector2 startTileCoords = SpawnTiles.hexCoordToPixelCoord(selectedUnit.getCoords());

                Vector2 Q = lastTileCoords - startTileCoords;
                float dist = Q.magnitude*100 - 15;
                if (dist < 0)
                {
                    dist = 0;
                }
                float angle = Mathf.Atan2(Q.y, Q.x) * 180 / Mathf.PI;
                //gameControllerRef.printString(dist.ToString());
                m.transform.Rotate(Vector3.forward, angle);
                m.transform.localScale = new Vector2(dist/24, 1.0f);

                MovementIcon mHead = createMovementIcon("ArrowHeadIcon", lastTileCoords, Color.cyan, 2, true);
                mHead.transform.Rotate(Vector3.forward, angle);

                createMovementIcon("PathIcon", lastTileCoords, Color.cyan, 1, true);
            }
        }
    }
}



//******************************************************************************************************************************************************************************************************************
    //  ------  Charge  ------  //
//******************************************************************************************************************************************************************************************************************

public class ChargeMoveType : MovementTypeParent
{
    public List<RelativeDirection> directions = new List<RelativeDirection>();
    public List<Vector2> adjustedDirections = new List<Vector2>();
    public List<int> ranges = new List<int>(); //-1 for infinite range
    public List<uint> blockingExtent = new List<uint>();

    public ChargeMoveType(RelativeDirection[] slideDirections, int[] slideRanges, uint[] blockExtent, bool verify=false)
    {
        directions = Util.toList(slideDirections);
        ranges = Util.toList(slideRanges);
        blockingExtent = Util.toList(blockExtent);

        if (verify)
        {
            verifyMove();
        }

        movementType = MovementTypes.Charge;
        unitRotated(AbsoluteDirection.UP);
    }

    public ChargeMoveType(List<RelativeDirection> slideDirections, List<int> slideRanges, List<uint> blockExtent, bool verify=false)
    {
        directions = slideDirections;
        ranges = slideRanges;
        blockingExtent = blockExtent;

        if (verify)
        {
            verifyMove();
        }

        movementType = MovementTypes.Charge;
        unitRotated(AbsoluteDirection.UP);
    }

    public override void startSelectingInMode(UnitScript selectedUnit, int currentTeam)
    {
        for (int i = 0; i < adjustedDirections.Count; i++)
        {
            HexTile currentTile = selectedUnit.getOccupyingHex();
            HexTile prevTile = null;
            int j = 0;
            while (j < ranges[i] || ranges[i] == -1/*For infinite range (Mabye it works)*/)
            {
                currentTile = gameControllerRef.getTileController().getTileFromHexCoord(currentTile.getCoords() + adjustedDirections[i]);
                if (j >= blockingExtent[i])
                {
                    if (currentTile)
                    {
                        if (currentTile.getOccupyingUnit())
                        {
                            if (currentTile.getOccupyingUnit().getTeam() == currentTeam)
                            {
                                currentTile.switchState();
                                break;
                            }
                        }
                        prevTile = currentTile;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    if (currentTile)
                    {
                        if (currentTile.getOccupyingUnit())
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                j++;
            }

            if (prevTile)
            {
                if (!prevTile.getOccupyingUnit())
                {
                    prevTile.switchState(TileState.SELECTABLE);
                }
                else if (prevTile.getOccupyingUnit().getTeam() != currentTeam)
                {
                    prevTile.switchState(TileState.SELECTABLE);
                }
            }
        }

        drawMovementIcons(selectedUnit);
    }

    public override void clickedInMode(HexTile clickedTile, UnitScript selectedUnit, int currentTeam)
    {
        if (clickedTile.getCurrentTileState() == TileState.SELECTABLE)
        {
            AbsoluteDirection direction = SpawnTiles.getDirectionToTile(selectedUnit.getOccupyingHex(), clickedTile);

            if (direction == AbsoluteDirection.NONE)
            {
                return;
            }

            Vector2 posDir = SpawnTiles.absoluteDirectionToRelativePos(direction);

            int pos = -1;
            for (int i = 0; i < directions.Count; i++ )
            {
                if (posDir == adjustedDirections[i] )
                {
                    pos = i;
                    break;
                }
            }
            if (pos == -1)
            {
                throw new UnityException("Issue: Charged move click on tile error, selected tile that has been marked but is NOT in any direction");
            }

            int j = 0;
            while ( j < ranges[pos] || ranges[pos] == -1)
            {
                HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getCoords() + posDir * (j+1));

                if (tile)
                {
                    if (tile.getOccupyingUnit())
                    {
                        if (tile.getOccupyingUnitTeam() == selectedUnit.getTeam())
                        {
                            break;
                        }
                        else
                        {
                            gameControllerRef.captureUnit(tile.getOccupyingUnit());
                        }
                    }
                }
                else
                {
                    break;
                }
                j++;
            }

            gameControllerRef.getTileController().transferUnit(selectedUnit.getOccupyingHex(), clickedTile);
            gameControllerRef.switchInteractionState(InteractionStates.SelectingUnitToRotate);        
        }
    }

    public override bool canMove(UnitScript selectedUnit, int currentTeam)
    {
        for (int i = 0; i < directions.Count; i++ )
        {
            bool good = true;
            //Plus one since we need to see if the one after the blocking exent is avialiable
            Vector2 currentDirection = adjustedDirections[i];
            HexTile tile = null;
            //Starts at one since you dont want to check against self :P
            for (int j = 1; j <= blockingExtent[i]; j++ )
            {
                tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + currentDirection * j);
                if (tile)
                {
                    if (tile.getOccupyingUnit())
                    {
                        good = false;
                        break;
                    }
                }
                else
                {
                    good = false;
                    break;
                }
            }
            if (good)
            {
                tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + currentDirection * (blockingExtent[i] + 1));
                if (tile)
                {
                    if (tile.getOccupyingUnit())
                    {
                        if (tile.getOccupyingUnit().getTeam() == currentTeam)
                        {
                            good = false;
                        }
                    }
                }
            }
            if (good)
            {
                return true;
            }
        }

        return false;
    }

    public override void unitRotated(AbsoluteDirection direction)
    {
        adjustedDirections = new List<Vector2>();

        for (int i = 0; i < directions.Count; i++)
        {
            adjustedDirections.Add(SpawnTiles.absoluteDirectionToObject(SpawnTiles.relativeToAbsoluteDirection(direction, directions[i])).getUpDirection());
            //adjustedDirs.Add(SpawnTiles.rotate(directions[i], selectedUnit.getRotation()));
        }
    }

    public override void verifyMove()
    {
        int listSize = directions.Count;
        if (listSize != ranges.Count)
        {
            throw new UnityException("The amount of Slide Ranges do not match the amount of Slide Directions");
        }
        if (listSize != blockingExtent.Count)
        {
            throw new UnityException("The amount of Block Extents do not match the amount of Slide Directions");
        }

        List<RelativeDirection> tempList = new List<RelativeDirection>();

        for (int i = 0; i < listSize; i++)
        {
            if (ranges[i] < -1 || ranges[i] == 0)
            {
                throw new UnityException("Charge Ranges has an invalid range (< -1) || (= 0)");
            }
            if (blockingExtent[i] <= 0)
            {
                throw new UnityException("Block Extent has an invalid range (<= 0)");
            }
            if (blockingExtent[i] >= ranges[i] && ranges[i] != -1)
            {
                throw new UnityException("Block Extent has an invalid range (>= slideRanges at index)");
            }
            if (tempList.Contains(directions[i]))
            {
                throw new UnityException("Directions must never be repeated");
            }
            else
            {
                tempList.Add(directions[i]);
            }
        }
    }

    public override MovementTypeParent clone()
    {
        return new ChargeMoveType(directions, ranges, blockingExtent);
    }

    public override void drawMovementIcons(UnitScript selectedUnit)
    {
        for (int i = 0; i < adjustedDirections.Count; i++)
        {
            HexTile currentTile = selectedUnit.getOccupyingHex();
            HexTile prevTile = null;
            //Color drawingColor = Color.yellow;
            Color drawingColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
            int j = 0;
            while (j < ranges[i] || ranges[i] == -1)
            {
                Vector2 currentTileCoords = currentTile.getCoords() + adjustedDirections[i];
                currentTile = gameControllerRef.getTileController().getTileFromHexCoord(currentTile.getCoords() + adjustedDirections[i]);
                if (j >= blockingExtent[i])
                {
                    if (currentTile)
                    {
                        if (currentTile.getOccupyingUnit())
                        {
                            if (currentTile.getOccupyingUnit().getTeam() == selectedUnit.getTeam())
                            {
                                createMovementIcon("CrossIcon", currentTileCoords, Color.red, 4);
                                break;
                            }
                            else
                            {
                                createMovementIcon("AttackIcon", currentTileCoords, Color.red, 4);
                                drawingColor = Color.yellow;
                            }
                        }
                        else
                        {
                            drawingColor = Color.yellow;
                        }
                        prevTile = currentTile;
                    }
                    else
                    {
                        createMovementIcon("CrossIcon", currentTileCoords, Color.red, 4);
                        break;
                    }
                }
                else
                {
                    if (currentTile)
                    {
                        prevTile = currentTile;//Have arrow go to the blocked tile
                        if (currentTile.getOccupyingUnit())
                        {
                            createMovementIcon("CrossIcon", currentTileCoords, Color.red, 4);
                            break;
                        }
                        createMovementIcon("CrossIcon", currentTileCoords, new Color(0.2f, 0.2f, 0.2f, 1.0f), 0);
                        
                    }
                    else
                    {
                        createMovementIcon("CrossIcon", currentTileCoords, Color.red, 4);
                        //drawingColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
                        //prevTile = currentTile;
                        break;
                    }
                }

                j++;
            }

            if (prevTile)
            {
                MovementIcon m = createMovementIcon("PathConnectionIcon", selectedUnit.getCoords(), drawingColor, 1);
                Vector2 lastTileCoords = SpawnTiles.hexCoordToPixelCoord(prevTile.getCoords());
                Vector2 startTileCoords = SpawnTiles.hexCoordToPixelCoord(selectedUnit.getCoords());

                Vector2 Q = lastTileCoords - startTileCoords;
                float dist = Q.magnitude * 100 - 15;
                if (dist < 0)
                {
                    dist = 0;
                }
                float angle = Mathf.Atan2(Q.y, Q.x) * 180 / Mathf.PI;
                //gameControllerRef.printString(dist.ToString());
                m.transform.Rotate(Vector3.forward, angle);
                m.transform.localScale = new Vector2(dist / 24, 1.0f);

                MovementIcon mHead = createMovementIcon("ArrowHeadIcon", lastTileCoords, drawingColor, 2, true);
                mHead.transform.Rotate(Vector3.forward, angle);

                createMovementIcon("PathIcon", lastTileCoords, drawingColor, 3, true);
            }

            createMovementIcon("PathIcon", selectedUnit.getCoords(), drawingColor, 3);

        }
    }
}



//******************************************************************************************************************************************************************************************************************
    //  ------  Ranged  ------  //
//******************************************************************************************************************************************************************************************************************

public class RangedMoveType : MovementTypeParent
{
    public List<List<Vector2>> relativeLocations = new List<List<Vector2>>();
    public List<List<Vector2>> adjustedLocations = new List<List<Vector2>>();

    public RangedMoveType(Vector2[][] relativeLocs, bool verify=false)
    {
        for (int i = 0; i < relativeLocs.Length; i++ )
        {
            relativeLocations.Add(Util.toList(relativeLocs[i]));
        }

        movementType = MovementTypes.Ranged;

        if (verify)
        {
            verifyMove();
        }

        unitRotated(AbsoluteDirection.UP);
    }

    public RangedMoveType(List<List<Vector2>> relativeLocs, bool verify = false)
    {
        for (int i = 0; i < relativeLocs.Count; i++)
        {
            relativeLocations.Add(relativeLocs[i]);
        }

        movementType = MovementTypes.Ranged;

        if (verify)
        {
            verifyMove();
        }

        unitRotated(AbsoluteDirection.UP);
    }

    public override void startSelectingInMode(UnitScript selectedUnit, int currentTeam)
    {
        for (int i = 0; i < adjustedLocations.Count; i++)
        {
            for (int j = 0; j < adjustedLocations[i].Count; j++)
            {
                HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + adjustedLocations[i][j]);
                if (tile)
                {
                    if (tile.getOccupyingUnit())
                    {
                        if (tile.getOccupyingUnit().getTeam() != currentTeam)
                        {
                            for (int k = 0; k < adjustedLocations[i].Count; k++ )
                            {
                                HexTile tile2 = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + adjustedLocations[i][k]);
                                tile2.switchState(TileState.SELECTABLE);
                            }
                            break;
                        }
                    }
                }
            }
        }
        drawMovementIcons(selectedUnit);
    }

    public override void clickedInMode(HexTile clickedTile, UnitScript selectedUnit, int currentTeam)
    {

        if (clickedTile.getCurrentTileState() == TileState.SELECTABLE)
        {
            Vector2 tileLoc = clickedTile.getCoords();

            for (int i = 0; i < adjustedLocations.Count; i++ )
            {
                if (adjustedLocations[i].Contains(tileLoc - selectedUnit.getCoords()))
                {
                    for (int j = 0; j < adjustedLocations[i].Count; j++)
                    {
                        HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getCoords() + adjustedLocations[i][j]);
                        if (tile)
                        {
                            if (tile.getOccupyingUnit())
                            {
                                //removing ones on same team
                                //if (tile.getOccupyingUnitTeam() != selectedUnit.getTeam())
                                //{
                                    gameControllerRef.captureUnit(tile.getOccupyingUnit());
                                //}
                            }
                        }
                    }
                }
            }

            gameControllerRef.switchInteractionState(InteractionStates.SelectingUnitToRotate);
        }
    }

    public override bool canMove(UnitScript selectedUnit, int currentTeam)
    {
        for (int i = 0; i < relativeLocations.Count; i++ )
        {
            for (int j = 0; j < relativeLocations[i].Count; j++)
            {
                HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + adjustedLocations[i][j]);
                if (tile)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public override void unitRotated(AbsoluteDirection direction)
    {
        adjustedLocations = new List<List<Vector2>>();

        for (int i = 0; i < relativeLocations.Count; i++)
        {
            List<Vector2> relPos = new List<Vector2>();
            for (int j = 0; j < relativeLocations[i].Count; j++)
            {
                relPos.Add(SpawnTiles.rotate(relativeLocations[i][j], direction));
            }
            adjustedLocations.Add(relPos);
        }
    }

    public override void verifyMove()
    {
        HashSet<Vector2> tempSet = new HashSet<Vector2>();
        Vector2 zeroVec = new Vector2(0, 0);
        for (int i = 0; i < relativeLocations.Count; i++)
        {
            for (int j = 0; j < relativeLocations[i].Count; j++)
            {
                if (relativeLocations[i][j] == zeroVec)
                {
                    throw new UnityException("A relative locations is an invalid location (0,0)");
                }
                if (!tempSet.Contains(relativeLocations[i][j]))
                {
                    tempSet.Add(relativeLocations[i][j]);
                }
                else
                {
                    throw new UnityException("A relative location is repeated in this RangedMove");
                }
            }
        }
    }

    public override MovementTypeParent clone()
    {
        return new RangedMoveType(relativeLocations);
    }

    public override void drawMovementIcons(UnitScript selectedUnit)
    {
        gameControllerRef.printString("Meowsquers");
        for (int i = 0; i < adjustedLocations.Count; i++)
        {
            bool foundEnemy = false;
            bool foundFriend = false;
            bool onlyFriends = false;
            for (int j = 0; j < adjustedLocations[i].Count; j++)
            {
                HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + adjustedLocations[i][j]);
                if (tile)
                {
                    createMovementIcon("RangedIcon", tile.getCoords(), new Color(0.0f, 1.0f, 0.0f, 1.0f), 0);
                    if (!foundEnemy && !onlyFriends)
                    {
                        if (tile.getOccupyingUnit())
                        {
                            if (tile.getOccupyingUnit().getTeam() != selectedUnit.getTeam())
                            {
                                foundEnemy = true;
                                foundFriend = false;
                                j = -1;
                                //clearMovementIcons();
                                continue;
                            }
                            else
                            {
                                foundFriend = true;
                            }
                        }

                        if (foundFriend && j == adjustedLocations[i].Count-1)
                        {
                            onlyFriends = true;
                            j = -1;
                            //clearMovementIcons();
                            continue;
                        }
                    }
                    else if (foundEnemy)
                    {
                        tile.switchState(TileState.SELECTABLE);
                        //createMovementIcon("RangedIcon", tile.getCoords(), new Color(0.0f, 1.0f, 0.0f, 1.0f), 0);
                        createMovementIcon("AttackIcon", tile.getCoords(), new Color(1.0f, 0.0f, 0.0f, 1.0f), 1);
                    }
                    else
                    {
                        //createMovementIcon("RangedIcon", tile.getCoords(), new Color(0.0f, 1.0f, 0.0f, 1.0f), 0);
                        createMovementIcon("CrossIcon", tile.getCoords(), new Color(1.0f, 0.0f, 0.0f, 1.0f), 1);
                    }
                }
            }
        }
    }
}


//******************************************************************************************************************************************************************************************************************
    //  ------  Reposition  ------  //
//******************************************************************************************************************************************************************************************************************

public class RepositionMoveType : MovementTypeParent
{
    public List<Vector2> selectableLocations = new List<Vector2>();
    public List<Vector2> adjustedSelectableLocations = new List<Vector2>();
    public List<Vector2> repositionLocations = new List<Vector2>();
    public List<Vector2> adjustedRepositionLocations = new List<Vector2>();  
    //public List<List<RelativeDirection>> rotateDirections = new List<List<RelativeDirection>>();

    public int mode = 0;
    public UnitScript repositioningUnit = null;

    public RepositionMoveType(Vector2[] selectableLocs, Vector2[] repositionLocs, bool verify=false)//, RelativeDirection[][] rotateDirs)
    {
        selectableLocations = Util.toList(selectableLocs);
        repositionLocations = Util.toList(repositionLocs);

        /*for (int i = 0; i < rotateDirs.Length; i++ )
        {
            rotateDirections.Add(Util.toList(rotateDirs[i]));
        }*/

        if (verify)
        {
            verifyMove();
        }

        movementType = MovementTypes.Reposition;

        unitRotated(AbsoluteDirection.UP);
        //Make Sure Rotations are not the same.
    }

    public RepositionMoveType(List<Vector2> selectableLocs, List<Vector2> repositionLocs, bool verify=false)//, RelativeDirection[][] rotateDirs)
    {
        selectableLocations = selectableLocs;
        repositionLocations = repositionLocs;

        /*for (int i = 0; i < rotateDirs.Length; i++ )
        {
            rotateDirections.Add(Util.toList(rotateDirs[i]));
        }*/

        if (verify)
        {
            verifyMove();
        }

        movementType = MovementTypes.Reposition;

        unitRotated(AbsoluteDirection.UP);
        //Make Sure Rotations are not the same.
    }

    public override void startSelectingInMode(UnitScript selectedUnit, int currentTeam)
    {
        mode = 0;
        for (int i = 0; i < selectableLocations.Count; i++)
        {
            HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getCoords() + adjustedSelectableLocations[i]);
            if (tile)
            {
                if (tile.getOccupyingUnit())
                {
                    if (tile.getOccupyingUnitTeam() == selectedUnit.getTeam())
                    {
                        tile.switchState(TileState.SELECTABLE);
                    }
                }
            }
        }
        drawMovementIcons(selectedUnit);
    }

    /*private void drawMoveableTiles(UnitScript selectedUnit, int currentTeam)
    {
        gameControllerRef.getTileController().switchAllTileStates();
        for (int i = 0; i < repositionLocations.Count; i++)
        {
            HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getCoords() + adjustedRepositionLocations[i]);
            if (tile)
            {
                tile.switchState(TileState.NONSELECTABLE);
                if (!tile.getOccupyingUnit())
                {
                    tile.switchState(TileState.SELECTABLE);
                }
                else if (tile.getOccupyingUnitTeam() == selectedUnit.getTeam())
                {
                    tile.switchState(TileState.SELECTABLE);
                }
            }
        }
    }*/

    public override void clickedInMode(HexTile clickedTile, UnitScript selectedUnit, int currentTeam)
    {
        if (mode == 0)
        {
            if (clickedTile.getCurrentTileState() == TileState.SELECTABLE)
            {
                mode = 1;
                //drawMoveableTiles(selectedUnit, currentTeam);

                gameControllerRef.getTileController().switchAllTileStates();

                selectedUnit.getOccupyingHex().switchState(TileState.SELECTED);
                for (int i = 0; i < repositionLocations.Count; i++)
                {
                    HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getCoords() + adjustedRepositionLocations[i]);
                    if (tile)
                    {
                        if (!tile.getOccupyingUnit() || tile == clickedTile)
                        {
                            tile.switchState(TileState.SELECTABLE);
                        }
                    }

                }

                repositioningUnit = clickedTile.getOccupyingUnit();
                clearMovementIcons();
                drawMovementIcons(selectedUnit);
                return;
            }
        }

        if (mode == 1)
        {
            if (clickedTile.getCurrentTileState() == TileState.SELECTABLE)
            {
                if (!clickedTile.getOccupyingUnit())
                {
                    gameControllerRef.getTileController().transferUnit(repositioningUnit.getOccupyingHex(), clickedTile);
                }
                repositioningUnit.setRotationDirection(selectedUnit.getRotation());
                mode = 0;
                repositioningUnit = null;
                gameControllerRef.switchInteractionState(InteractionStates.SelectingUnitToRotate);
            }
        }
    }

    public override bool canMove(UnitScript selectedUnit, int currentTeam)
    {
        bool good = false;
        for (int i = 0; i < selectableLocations.Count; i++ )
        {
            HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getCoords() + adjustedSelectableLocations[i]);
            if (tile)
            {
                if (tile.getOccupyingUnit())
                {
                    if (tile.getOccupyingUnitTeam() == selectedUnit.getTeam())
                    {
                        good = true;
                        break;
                    }
                }
            }
        }
        if (!good)
        {
            return false;
        }

        for (int i = 0; i < repositionLocations.Count; i++)
        {
            HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getCoords() + adjustedRepositionLocations[i]);
            if (tile)
            {
                if (!tile.getOccupyingUnit())
                {
                    return true;
                }
            }
        }

        return false;
    }

    public override void unitRotated(AbsoluteDirection direction)
    {
        adjustedSelectableLocations = new List<Vector2>();
        for (int i = 0; i < selectableLocations.Count; i++)
        {
            adjustedSelectableLocations.Add(SpawnTiles.rotate(selectableLocations[i], direction));
        }

        adjustedRepositionLocations = new List<Vector2>();
        for (int i = 0; i < repositionLocations.Count; i++)
        {
            adjustedRepositionLocations.Add(SpawnTiles.rotate(repositionLocations[i], direction));
        }
    }

    public override void verifyMove()
    {
        if (!testForInvalidPositions(selectableLocations))
        {
            throw new UnityException("Selectable Locs has an invalid location (0,0)");
        }

        if (!testForInvalidPositions(repositionLocations))
        {
            throw new UnityException("Selectable Locs has an invalid location (0,0)");
        }
    }

    public override MovementTypeParent clone()
    {
        return new RepositionMoveType(selectableLocations, repositionLocations);
    }

    public override void drawMovementIcons(UnitScript selectedUnit)
    {
        if (mode == 0)
        {
            for (int i = 0; i < adjustedSelectableLocations.Count; i++)
            {
                HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getCoords() + adjustedSelectableLocations[i]);
                if (tile)
                {
                    createMovementIcon("SelectableIcon", tile.getCoords(), new Color(1.0f, 0.8f, 0.8f, 1.0f), 0);
                    if (tile.getOccupyingUnit())
                    {
                        if (tile.getOccupyingUnitTeam() != selectedUnit.getTeam())
                        {
                            createMovementIcon("CrossIcon", tile.getCoords(), new Color(1.0f, 0.0f, 0.0f, 1.0f), 1);
                        }
                    }
                    else
                    {
                        createMovementIcon("CrossIcon", tile.getCoords(), new Color(1.0f, 0.0f, 0.0f, 1.0f), 1);
                    }
                }
            }
        }
        if (mode == 1)
        {
            //gameControllerRef.printString("radaradaradarada");
            for (int i = 0; i < adjustedRepositionLocations.Count; i++)
            {
                HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getCoords() + adjustedRepositionLocations[i]);
                if (tile)
                {
                    createMovementIcon("PlaceIcon", tile.getCoords(), new Color(1.0f, 0.8f, 0.8f, 1.0f), 0);
                    if (tile.getOccupyingUnit() && tile.getOccupyingUnit() != repositioningUnit)
                    {
                        createMovementIcon("CrossIcon", tile.getCoords(), new Color(1.0f, 0.0f, 0.0f, 1.0f), 1);
                    }
                }
            }
        }
    }
}

