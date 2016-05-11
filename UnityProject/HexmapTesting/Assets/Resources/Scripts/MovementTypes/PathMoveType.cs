using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    public PathMoveType(PathPos[][] paths, bool verify = false)
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


    public PathMoveType(List<List<PathPos>> paths, bool verify = false)
    {
        movementType = MovementTypes.Path;

        pathList = paths;

        unitRotated(AbsoluteDirection.UP);
    }

    public override void startSelectingInMode(UnitScript selectedUnit, int currentTeam)
    {
        //MarkingEm
        for (int i = 0; i < adjustedPathList.Count; i++)
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
        if (clickedTile.getOccupyingUnit())
        {
            gameControllerRef.captureUnit(clickedTile.getOccupyingUnit());
        }

        gameControllerRef.getTileController().transferUnit(selectedUnit.getOccupyingHex(), clickedTile);
        gameControllerRef.switchInteractionState(InteractionStates.SelectingUnitToRotate);
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
                pathPart.Add(new PathPos(TileController.rotate(pathList[i][j].pos, direction), pathList[i][j].moveable));
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
            InteractionIcon.createInteractionIcon("PathIcon", selectedUnit.getCoords(), new Color(0.0f,0.0f,1.0f,1.0f), 2);
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
                                InteractionIcon.createInteractionIcon("CrossIcon", tileCoords, new Color(1.0f, 0.0f, 0.0f, 0.4f), 3);
                            }
                            else
                            {
                                InteractionIcon.createInteractionIcon("AttackIcon", tileCoords, new Color(1.0f, 0.0f, 0.0f, 1.0f), 3);
                            }
                        }
                    }

                    Vector2 lastCoords = TileController.hexCoordToPixelCoord(lastTile.getCoords());
                    Vector2 currentCoords = TileController.hexCoordToPixelCoord(tileCoords);
                    InteractionIcon m = InteractionIcon.createInteractionIcon("PathConnectionIcon", lastCoords, color, 0, true);

                    Vector2 Q = currentCoords - lastCoords;
                    float dist = Q.magnitude * 100;

                    if (adjustedPathList[i][j].moveable)
                    {
                        InteractionIcon.createInteractionIcon("PathSelectableIcon", tileCoords, color, depth);
                        dist -= 15;
                        if (dist < 0)
                            dist = 0;
                    }
                    else
                    {
                        InteractionIcon.createInteractionIcon("PathIcon", tileCoords, color, depth);
                    }

                    float angle = Mathf.Atan2(Q.y, Q.x) * 180 / Mathf.PI;
                    gameControllerRef.printString(dist.ToString());
                    m.transform.Rotate(Vector3.forward, angle);
                    m.transform.localScale = new Vector2(dist / 24, 1.0f);

                    lastTile = tile;
                }
                else
                {
                    InteractionIcon.createInteractionIcon("CrossIcon", tileCoords, new Color(1.0f, 0.0f, 0.0f, 0.4f), 3);
                    break;
                }
            }
        }
    }
}
