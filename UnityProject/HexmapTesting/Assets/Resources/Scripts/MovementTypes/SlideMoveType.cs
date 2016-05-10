using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//******************************************************************************************************************************************************************************************************************
//  ------  Slide  ------  //
//******************************************************************************************************************************************************************************************************************

public class SlideMoveType : MovementTypeParent
{
    public List<RelativeDirection> directions = new List<RelativeDirection>();
    public List<Vector2> adjustedDirections = new List<Vector2>();
    public List<int> ranges = new List<int>(); //-1 for infinite range

    public SlideMoveType(RelativeDirection[] slideDirections, int[] slideRanges, bool verify = false)
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
        for (int i = 0; i < directions.Count; i++)
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
            adjustedDirections.Add(TileController.absoluteDirectionToObject(TileController.relativeToAbsoluteDirection(direction, directions[i])).getUpDirection());
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
            InteractionIcon.createInteractionIcon("PathIcon", selectedUnit.getCoords(), Color.cyan, 3);
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
                            InteractionIcon.createInteractionIcon("AttackIcon", currentTile.getCoords(), Color.red, 4);
                        }
                        else
                        {
                            InteractionIcon.createInteractionIcon("CrossIcon", currentTile.getCoords(), Color.red, 4);
                        }
                        break;
                    }
                    lastTile = currentTile;

                }
                else
                {
                    InteractionIcon.createInteractionIcon("CrossIcon", currentTileLoc, Color.red, 4);
                    break;
                }
                j++;
            }

            if (lastTile)
            {
                InteractionIcon m = InteractionIcon.createInteractionIcon("PathConnectionIcon", selectedUnit.getCoords(), Color.cyan, 1);
                Vector2 lastTileCoords = TileController.hexCoordToPixelCoord(lastTile.getCoords());
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

                InteractionIcon mHead = InteractionIcon.createInteractionIcon("ArrowHeadIcon", lastTileCoords, Color.cyan, 2, true);
                mHead.transform.Rotate(Vector3.forward, angle);

                InteractionIcon.createInteractionIcon("PathIcon", lastTileCoords, Color.cyan, 1, true);
            }
        }
    }
}