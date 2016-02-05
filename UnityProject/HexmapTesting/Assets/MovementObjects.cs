﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MovementTypes
{
    Path,
    Jump,
    Slide,
    Charge,
    Ranged
}

//!!!!! Always make sure each does not have a relative position/direction of (0,0)
public abstract class MovementTypeParent
{
    //This is just to serve as a parent so we can stored all childern of this parent in one place... so far I cant think of anything MovementTypeParent should have
    protected static GameControllerScript gameControllerRef = null;

    protected MovementTypes movementType = MovementTypes.Path;

    public MovementTypeParent() { }

    public static void setGameControllerRef(GameControllerScript gameControllerScriptRef)
    {
        gameControllerRef = gameControllerScriptRef;
    }

    public abstract void startSelectingInMode(UnitScript selectedUnit, int currentTeam);

    public abstract void clickedInMode(HexTile clickedTile, UnitScript selecetdUnit, int currentTeam);

    public abstract bool canMove(UnitScript selectedUnit, int currentTeam);

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
};




//******************************************************************************************************************************************************************************************************************
        //  ------  Normal  ------  //
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

    public PathMoveType(PathPos[][] paths)
    {
        pathList = new List<List<PathPos>>();

        movementType = MovementTypes.Path;
        
        for (int i = 0; i < paths.Length; i++)
        {
            List<List<PathPos>> newPart = new List<List<PathPos>>();
            pathList.Add(Util.toList(paths[i]));
        }


        bool foundOne = false;
        for (int i = 0; i < pathList.Count; i++)
        {
            for (int j = 0; j < pathList[i].Count; j++)
            {
                if (paths[i][j].moveable)
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
    }


    public override void startSelectingInMode(UnitScript selectedUnit, int currentTeam)
    {
        //Adjusting Positions
        List<List<PathPos>> adjustedPathList = new List<List<PathPos>>();
        for (int i = 0; i < pathList.Count; i++ )
        {
            List<PathPos> pathPart = new List<PathPos>();
            for (int j = 0; j < pathList[i].Count; j++)
            {
                pathPart.Add(new PathPos(SpawnTiles.rotate(pathList[i][j].pos, selectedUnit.getRotation()), pathList[i][j].moveable));
            }
            adjustedPathList.Add(pathPart);
        }

        //MarkingEm
        for (int i = 0; i < adjustedPathList.Count; i++ )
        {
            bool blocked = false;
            for (int j = 0; j < adjustedPathList[i].Count; j++)
            {
                if (blocked)
                {
                    HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + adjustedPathList[i][j].pos);
                    if (tile)
                    {
                        if (tile.getCurrentTileState() != TileState.SELECTABLE)
                        {
                            tile.switchState(TileState.NONSELECTABLE);
                        }
                    }
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
                                    tile.switchState(TileState.ATTACKABLE);
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
                        if (tile.getCurrentTileState() != TileState.SELECTABLE)
                        {
                            tile.switchState(TileState.NONSELECTABLE);
                        }
                    }
                    else
                    {
                        blocked = true;
                    }
                }
            }
        }

    }

    public override void clickedInMode(HexTile clickedTile, UnitScript selectedUnit, int currentTeam)
    {
        if (clickedTile.getCurrentTileState() == TileState.ATTACKABLE)
        {
            gameControllerRef.captureUnit(clickedTile.getOccupyingUnit());
        }

        gameControllerRef.getTileController().transferUnit(selectedUnit.getOccupyingHex(), clickedTile);
        //selectedUnit.transform.position = gameControllerRef.getTileController().hexCoordToPixelCoord(clickedTile.getCoords());
        //switchToNextTeam();
        //gameControllerRef.altCounter = 0;

        gameControllerRef.switchInteractionState(InteractionStates.SelectingUnitToRotate);
    }

    public override bool canMove(UnitScript selectedUnit, int currentTeam)
    {
        //Adjusting Positions
        List<List<PathPos>> adjustedPathList = new List<List<PathPos>>();
        for (int i = 0; i < pathList.Count; i++)
        {
            List<PathPos> pathPart = new List<PathPos>();
            for (int j = 0; j < pathList[i].Count; j++)
            {
                pathPart.Add(new PathPos(SpawnTiles.rotate(pathList[i][j].pos, selectedUnit.getRotation()), pathList[i][j].moveable));
            }
            adjustedPathList.Add(pathPart);
        }

        gameControllerRef.printString("" + adjustedPathList.Count);

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
}



//******************************************************************************************************************************************************************************************************************
        //  ------  Jump  ------  //
//******************************************************************************************************************************************************************************************************************

public class JumpMoveType : MovementTypeParent
{
    public List<Vector2> jumpPositions = new List<Vector2>();

    public JumpMoveType(Vector2[] jumpMovementPositions)
    {
        jumpPositions = Util.toList(jumpMovementPositions);
        movementType = MovementTypes.Jump;

        if (!testForInvalidPositions(jumpPositions))
        {
            throw new UnityException("Jump Locations have an invalid positions (0,0)");
        }
    }


    public override void startSelectingInMode(UnitScript selectedUnit, int currentTeam)
    {
        List<Vector2> adjustedJumpPositions = new List<Vector2>();
        for (int i = 0; i < jumpPositions.Count; i++)
        {
            adjustedJumpPositions.Add(SpawnTiles.rotate(jumpPositions[i], selectedUnit.getRotation()));
        }

        for (int i = 0; i < adjustedJumpPositions.Count; i++)
        {
            HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(adjustedJumpPositions[i] + selectedUnit.getOccupyingHex().getCoords());

            if (tile != null)
            {
                if (tile.getOccupyingUnit() != null)
                {
                    if (tile.getOccupyingUnit().getTeam() != currentTeam)
                    {
                        tile.switchState(TileState.ATTACKABLE);
                    }
                    else
                    {
                        tile.switchState(TileState.NONSELECTABLE);
                    }
                }
                else
                {
                    tile.switchState(TileState.SELECTABLE);
                }
            }
        }
    }


    public override void clickedInMode(HexTile clickedTile, UnitScript selectedUnit, int currentTeam)
    {
        if (clickedTile.getCurrentTileState() == TileState.ATTACKABLE)
        {
            gameControllerRef.captureUnit(clickedTile.getOccupyingUnit());
        }

        gameControllerRef.getTileController().transferUnit(selectedUnit.getOccupyingHex(), clickedTile);
        //selectedUnit.transform.position = gameControllerRef.getTileController().hexCoordToPixelCoord(clickedTile.getCoords());

        gameControllerRef.switchInteractionState(InteractionStates.SelectingUnitToRotate);
    }


    public override bool canMove(UnitScript selectedUnit, int currentTeam)
    {
        for (int i = 0; i < jumpPositions.Count; i++ )
        {
            HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + SpawnTiles.rotate(jumpPositions[i], selectedUnit.getRotation()));
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
};



//******************************************************************************************************************************************************************************************************************
        //  ------  Slide  ------  //
//******************************************************************************************************************************************************************************************************************

public class SlideMoveType : MovementTypeParent
{
    public List<RelativeDirection> directions = new List<RelativeDirection>();
    public List<int> ranges = new List<int>(); //-1 for infinite range

    public SlideMoveType(RelativeDirection[] slideDirections, int[] slideRanges)
    {
        //initialize(Util.toList(slideDirections), Util.toList(slideRanges));

        directions = Util.toList(slideDirections);
        ranges = Util.toList(slideRanges);
        movementType = MovementTypes.Slide;

        if (directions.Count != ranges.Count)
        {
            throw new UnityException("The amount of Slide Directions do not match the amount of slide ranges");
        }

        List<RelativeDirection> tempList = new List<RelativeDirection>();
        for (int i = 0; i < ranges.Count; i++ )
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

    public override void startSelectingInMode(UnitScript selectedUnit, int currentTeam)
    {
        List<Vector2> adjustedDirections = new List<Vector2>();
        for (int i = 0; i < directions.Count; i++)
        {
            adjustedDirections.Add( SpawnTiles.absoluteDirectionToObject( SpawnTiles.relativeToAbsoluteDirection(selectedUnit.getRotation(), directions[i]) ).getUpDirection() );
        }

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
                            prevTile.switchState(TileState.MOVEABLE);
                        }
                    }
                    else
                    {
                        if (currentTile.getOccupyingUnit().getTeam() != currentTeam)
                        {
                            currentTile.switchState(TileState.ATTACKABLE);
                            if (prevTile)
                            {
                                prevTile.switchState(TileState.MOVEABLE);
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
    }

    public override void clickedInMode(HexTile clickedTile, UnitScript selectedUnit, int currentTeam)
    {
        if (clickedTile.getCurrentTileState() == TileState.ATTACKABLE)
        {
            gameControllerRef.captureUnit(clickedTile.getOccupyingUnit());
        }

        gameControllerRef.getTileController().transferUnit(selectedUnit.getOccupyingHex(), clickedTile);
        //selectedUnit.transform.position = gameControllerRef.getTileController().hexCoordToPixelCoord(clickedTile.getCoords());
        //switchToNextTeam();

        gameControllerRef.switchInteractionState(InteractionStates.SelectingUnitToRotate);
    }

    public override bool canMove(UnitScript selectedUnit, int currentTeam)
    {
        for (int i = 0; i < directions.Count; i++ )
        {
            HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + SpawnTiles.absoluteDirectionToObject(SpawnTiles.relativeToAbsoluteDirection(selectedUnit.getRotation(), directions[i])).getUpDirection());
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
}



//******************************************************************************************************************************************************************************************************************
    //  ------  Charge  ------  //
//******************************************************************************************************************************************************************************************************************

public class ChargeMoveType : MovementTypeParent
{
    public List<RelativeDirection> directions = new List<RelativeDirection>();
    public List<int> ranges = new List<int>(); //-1 for infinite range
    public List<uint> blockingExtent = new List<uint>();

    public ChargeMoveType(RelativeDirection[] slideDirections, int[] slideRanges, uint[] blockExtent)
    {
        //None should be in the same direction, check for that
        //initialize(Util.toList(slideDirections), Util.toList(slideRanges), Util.toList(blockExtent));

        directions = Util.toList(slideDirections);
        ranges = Util.toList(slideRanges);
        blockingExtent = Util.toList(blockExtent);

        int listSize = directions.Count;
        if (listSize != ranges.Count)
        {
            throw new UnityException("The amount of Slide Ranges do not match the amount of Slide Directions");
        }
        if (listSize != blockingExtent.Count)
        {
            throw new UnityException("The amount of Block Extents do not match the amount of Slide Directions");
        }
        /*if (!SpawnTiles.checkValidAdjacency(slideDirections))
        {
            throw new UnityException("Slide Directions has a non adjacent position");
        }*/
        List<RelativeDirection> tempList = new List<RelativeDirection>();

        for (int i = 0; i < listSize; i++)
        {
            if (slideRanges[i] < -1 || slideRanges[i] == 0)
            {
                throw new UnityException("Charge Ranges has an invalid range (< -1) || (= 0)");
            }
            if (blockExtent[i] <= 0)
            {
                throw new UnityException("Block Extent has an invalid range (<= 0)");
            }
            if (blockExtent[i] >= slideRanges[i] && slideRanges[i] != -1)
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

        movementType = MovementTypes.Charge;
    }

    public override void startSelectingInMode(UnitScript selectedUnit, int currentTeam)
    {
        List<Vector2> calculatedDirs = new List<Vector2>();

        for (int i = 0; i < directions.Count; i++)
        {
            calculatedDirs.Add( SpawnTiles.absoluteDirectionToObject( SpawnTiles.relativeToAbsoluteDirection(selectedUnit.getRotation(), directions[i]) ).getUpDirection() );
            //adjustedDirs.Add(SpawnTiles.rotate(directions[i], selectedUnit.getRotation()));
        }

        for (int i = 0; i < calculatedDirs.Count; i++)
        {
            HexTile currentTile = selectedUnit.getOccupyingHex();
            HexTile prevTile = null;
            int j = 0;
            while (j <= ranges[i] || ranges[i] == -1/*For infinite range (Mabye it works)*/)
            {
                currentTile = gameControllerRef.getTileController().getTileFromHexCoord(currentTile.getCoords() + calculatedDirs[i]);
                if (j >= blockingExtent[i])
                {
                    if (currentTile)
                    {
                        currentTile.switchState(TileState.MOVEABLE);
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
                        currentTile.switchState(TileState.NONSELECTABLE);
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
                    prevTile.switchState(TileState.ATTACKABLE);
                }
            }
        }
    }

    public override void clickedInMode(HexTile clickedTile, UnitScript selectedUnit, int currentTeam)
    {
        if (clickedTile.getCurrentTileState() == TileState.SELECTABLE || clickedTile.getCurrentTileState() == TileState.ATTACKABLE)
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
                if (posDir == SpawnTiles.absoluteDirectionToObject( SpawnTiles.relativeToAbsoluteDirection(selectedUnit.getRotation(), directions[i])).getUpDirection() )
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
                        gameControllerRef.captureUnit(tile.getOccupyingUnit());
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
            Vector2 currentDirection = SpawnTiles.absoluteDirectionToObject(SpawnTiles.relativeToAbsoluteDirection(selectedUnit.getRotation(), directions[i])).getUpDirection();
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
}



//******************************************************************************************************************************************************************************************************************
//  ------  Ranged  ------  //
//******************************************************************************************************************************************************************************************************************

public class RangedMoveType : MovementTypeParent
{
    public List<Vector2> relativeLocations = new List<Vector2>();

    public RangedMoveType(Vector2[] relativeLocs)
    {

        relativeLocations = Util.toList(relativeLocs);
        movementType = MovementTypes.Ranged;

        if (!testForInvalidPositions(relativeLocations))
        {
            throw new UnityException("Relative Locs has an invalid location (0,0)");
        }
    }

    public override void startSelectingInMode(UnitScript selectedUnit, int currentTeam)
    {
        List<Vector2> adjustedRelPos = new List<Vector2>();

        for (int i = 0; i < relativeLocations.Count; i++)
        {
            adjustedRelPos.Add(SpawnTiles.rotate(relativeLocations[i], selectedUnit.getRotation()));
        }

        for (int i = 0; i < relativeLocations.Count; i++)
        {
            HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + adjustedRelPos[i]);
            if (tile)
            {
                if (tile.getOccupyingUnit())
                {
                    if (tile.getOccupyingUnit().getTeam() != currentTeam)
                    {
                        tile.switchState(TileState.ATTACKABLE);
                    }
                    else
                    {
                        tile.switchState(TileState.NONSELECTABLE);
                        //tile.switchState(TileState.MOVEABLE);
                    }
                }
                else
                {
                    //tile.switchState(TileState.MOVEABLE);
                    tile.switchState(TileState.NONSELECTABLE);
                }
            }
        }
    }

    public override void clickedInMode(HexTile clickedTile, UnitScript selecetdUnit, int currentTeam)
    {
        if (clickedTile.getCurrentTileState() == TileState.ATTACKABLE)
        {
            gameControllerRef.captureUnit(clickedTile.getOccupyingUnit());

            gameControllerRef.switchInteractionState(InteractionStates.SelectingUnitToRotate);
        }
    }

    public override bool canMove(UnitScript selectedUnit, int currentTeam)
    {
        for (int i = 0; i < relativeLocations.Count; i++ )
        {
            HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + SpawnTiles.rotate(relativeLocations[i], selectedUnit.getRotation()));
            if (tile)
            {
                return true;
            }
        }
        return false;
    }
}

/////// Old Cached NormalMove

/* public List<List<Vector2>> moveLocations = new List<List<Vector2>>();
 public List<List<Vector2>> blockingLocations = new List<List<Vector2>>();

 public PathMoveType(Vector2[][] moveLocs, Vector2[][] blockingLocs)
 {
     List<List<Vector2>> newMoveLocs = new List<List<Vector2>>();
     for (int i = 0; i < moveLocs.Length; i++ )
     {
         newMoveLocs.Add(Util.toList(moveLocs[i]));
     }
     List<List<Vector2>> newBlockingLocs = new List<List<Vector2>>();
     for (int i = 0; i < blockingLocs.Length; i++)
     {
         newBlockingLocs.Add(Util.toList(blockingLocs[i]));
     }
        
     initialize(newMoveLocs, newBlockingLocs);
 }

 public void initialize(List<List<Vector2>> moveLocs, List<List<Vector2>> blockingLocs)
 {
     if (moveLocs.Count != blockingLocs.Count)
     {
         throw new UnityException("Move Locations and Blocking Locations do not have the same size, we need to parallelize these lists for simplicity");
     }
     if (!testForInvalidPositions(moveLocs))
     {
         throw new UnityException("Move Locations have an invalid positions (0,0)");
     }
     if (!testForInvalidPositions(blockingLocs))
     {
         throw new UnityException("Blocking Locations have an invalid positions (0,0)");
     }        

     moveLocations = moveLocs;
     blockingLocations = blockingLocs;

     movementType = MovementTypes.Normal;
 }


 public override void startSelectingInMode(UnitScript selectedUnit, int currentTeam)
 {
     List<List<Vector2>> adjustedMoveLocs = new List<List<Vector2>>();
     List<List<Vector2>> adjustedBlockingLocs = new List<List<Vector2>>();

     for (int i = 0; i < moveLocations.Count; i++)
     {
         adjustedMoveLocs.Add(new List<Vector2>());
         adjustedBlockingLocs.Add(new List<Vector2>());
         for (int j = 0; j < moveLocations[i].Count; j++)
         {
             //print("m: " + moveLocs[i][j]);
             adjustedMoveLocs[i].Add(SpawnTiles.rotate(moveLocations[i][j], selectedUnit.getRotation()));
         }
         for (int j = 0; j < blockingLocations[i].Count; j++)
         {
             //print("b: " + blockingLocs[i][j]);
             adjustedBlockingLocs[i].Add(SpawnTiles.rotate(blockingLocations[i][j], selectedUnit.getRotation()));
         }
     }

     for (int i = 0; i < adjustedMoveLocs.Count; i++)
     {
         bool blocked = false;
         for (int j = 0; j < adjustedBlockingLocs[i].Count; j++)
         {
             HexTile blockingTile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + adjustedBlockingLocs[i][j]);
             if (blockingTile)
             {
                 if (blockingTile.getOccupyingUnit())
                 {
                     blocked = true;
                     break;
                 }
             }
             else
             {
                 blocked = true;
                 break;
             }
         }

         if (!blocked)
         {
             for (int j = 0; j < adjustedMoveLocs[i].Count; j++)
             {
                 HexTile moveTile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + adjustedMoveLocs[i][j]);
                 if (moveTile)
                 {
                     if (moveTile.getOccupyingUnit() == null)
                     {
                         moveTile.switchState(TileState.SELECTABLE);
                     }
                     else if (moveTile.getOccupyingUnit().getTeam() != currentTeam)
                     {
                         moveTile.switchState(TileState.ATTACKABLE);
                     }
                 }
             }
         }
     }
 }


 public override void clickedInMode(HexTile clickedTile, UnitScript selectedUnit, int currentTeam)
 {
     if (clickedTile.getCurrentTileState() == TileState.ATTACKABLE)
     {
         gameControllerRef.captureUnit(clickedTile.getOccupyingUnit());
     }

     gameControllerRef.getTileController().transferUnit(selectedUnit.getOccupyingHex(), clickedTile);
     //selectedUnit.transform.position = gameControllerRef.getTileController().hexCoordToPixelCoord(clickedTile.getCoords());
     //switchToNextTeam();
     gameControllerRef.altCounter = 0;

     gameControllerRef.switchInteractionState(InteractionStates.SelectingUnitToRotate);
 }

 public override bool canMove(UnitScript selectedUnit, int currentTeam)
 {
     for (int i = 0; i < moveLocations.Count; i++)
     {
         //Check normal location (optimizaiton)
         bool good = false;
         HexTile tile = null;
         for (int j = 0; j < moveLocations[i].Count; j++)
         {
             tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + SpawnTiles.rotate(moveLocations[i][j], selectedUnit.getRotation()));
             if (tile)
             {
                 if (tile.getOccupyingUnit())
                 {
                     if (tile.getOccupyingUnit().getTeam() != currentTeam)
                     {
                         good = true;
                         break;
                     }
                 }
                 else
                 {
                     good = true;
                     break;
                 }
             }
         }
         if (good)
         {
             for (int j = 0; j < blockingLocations[i].Count; j++)
             {
                 tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + SpawnTiles.rotate(blockingLocations[i][j], selectedUnit.getRotation()));
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
         }
         if (good)
         {
             return true;
         }
     }
     return false;
 }*/