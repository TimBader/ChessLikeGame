using UnityEngine;
using System.Collections;
using System.Collections.Generic;



//******************************************************************************************************************************************************************************************************************
//  ------  Charge  ------  //
//******************************************************************************************************************************************************************************************************************

public class ChargeMoveType : MovementTypeParent
{
    public List<RelativeDirection> directions = new List<RelativeDirection>();
    public List<Vector2> adjustedDirections = new List<Vector2>();
    public List<int> ranges = new List<int>(); //-1 for infinite range
    public List<uint> blockingExtent = new List<uint>();

    public ChargeMoveType(RelativeDirection[] slideDirections, int[] slideRanges, uint[] blockExtent, bool verify = false)
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

    public ChargeMoveType(List<RelativeDirection> slideDirections, List<int> slideRanges, List<uint> blockExtent, bool verify = false)
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
        AbsoluteDirection direction = TileController.getDirectionToTile(selectedUnit.getOccupyingHex(), clickedTile);

        if (direction == AbsoluteDirection.NONE)
        {
            return;
        }

        Vector2 posDir = TileController.absoluteDirectionToRelativePos(direction);

        int pos = -1;
        for (int i = 0; i < directions.Count; i++)
        {
            if (posDir == adjustedDirections[i])
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
        while (j < ranges[pos] || ranges[pos] == -1)
        {
            HexTile tile = gameControllerRef.getTileController().getTileFromHexCoord(selectedUnit.getCoords() + posDir * (j + 1));

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

    public override bool canMove(UnitScript selectedUnit, int currentTeam)
    {
        for (int i = 0; i < directions.Count; i++)
        {
            bool good = true;
            //Plus one since we need to see if the one after the blocking exent is avialiable
            Vector2 currentDirection = adjustedDirections[i];
            HexTile tile = null;
            //Starts at one since you dont want to check against self :P
            for (int j = 1; j <= blockingExtent[i]; j++)
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
            adjustedDirections.Add(TileController.absoluteDirectionToObject(TileController.relativeToAbsoluteDirection(direction, directions[i])).getUpDirection());
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
                                InteractionIcon.createInteractionIcon("CrossIcon", currentTileCoords, Color.red, 4);
                                break;
                            }
                            else
                            {
                                InteractionIcon.createInteractionIcon("AttackIcon", currentTileCoords, Color.red, 4);
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
                        InteractionIcon.createInteractionIcon("CrossIcon", currentTileCoords, Color.red, 4);
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
                            InteractionIcon.createInteractionIcon("CrossIcon", currentTileCoords, Color.red, 4);
                            break;
                        }
                        InteractionIcon.createInteractionIcon("CrossIcon", currentTileCoords, new Color(0.2f, 0.2f, 0.2f, 1.0f), 0);

                    }
                    else
                    {
                        InteractionIcon.createInteractionIcon("CrossIcon", currentTileCoords, Color.red, 4);
                        //drawingColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
                        //prevTile = currentTile;
                        break;
                    }
                }

                j++;
            }

            if (prevTile)
            {
                InteractionIcon m = InteractionIcon.createInteractionIcon("PathConnectionIcon", selectedUnit.getCoords(), drawingColor, 1);
                Vector2 lastTileCoords = TileController.hexCoordToPixelCoord(prevTile.getCoords());
                Vector2 startTileCoords = TileController.hexCoordToPixelCoord(selectedUnit.getCoords());

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

                InteractionIcon mHead = InteractionIcon.createInteractionIcon("ArrowHeadIcon", lastTileCoords, drawingColor, 2, true);
                mHead.transform.Rotate(Vector3.forward, angle);

                InteractionIcon.createInteractionIcon("PathIcon", lastTileCoords, drawingColor, 3, true);
            }

            InteractionIcon.createInteractionIcon("PathIcon", selectedUnit.getCoords(), drawingColor, 3);

        }
    }
}
