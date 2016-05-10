using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    public RepositionMoveType(Vector2[] selectableLocs, Vector2[] repositionLocs, bool verify = false)//, RelativeDirection[][] rotateDirs)
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

    public RepositionMoveType(List<Vector2> selectableLocs, List<Vector2> repositionLocs, bool verify = false)//, RelativeDirection[][] rotateDirs)
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
                InteractionIcon.clearAllInteractionIcons();
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
        for (int i = 0; i < selectableLocations.Count; i++)
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
            adjustedSelectableLocations.Add(TileController.rotate(selectableLocations[i], direction));
        }

        adjustedRepositionLocations = new List<Vector2>();
        for (int i = 0; i < repositionLocations.Count; i++)
        {
            adjustedRepositionLocations.Add(TileController.rotate(repositionLocations[i], direction));
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
                    InteractionIcon.createInteractionIcon("SelectableIcon", tile.getCoords(), new Color(1.0f, 0.8f, 0.8f, 1.0f), 0);
                    if (tile.getOccupyingUnit())
                    {
                        if (tile.getOccupyingUnitTeam() != selectedUnit.getTeam())
                        {
                            InteractionIcon.createInteractionIcon("CrossIcon", tile.getCoords(), new Color(1.0f, 0.0f, 0.0f, 1.0f), 1);
                        }
                    }
                    else
                    {
                        InteractionIcon.createInteractionIcon("CrossIcon", tile.getCoords(), new Color(1.0f, 0.0f, 0.0f, 1.0f), 1);
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
                    InteractionIcon.createInteractionIcon("PlaceIcon", tile.getCoords(), new Color(1.0f, 0.8f, 0.8f, 1.0f), 0);
                    if (tile.getOccupyingUnit() && tile.getOccupyingUnit() != repositioningUnit)
                    {
                        InteractionIcon.createInteractionIcon("CrossIcon", tile.getCoords(), new Color(1.0f, 0.0f, 0.0f, 1.0f), 1);
                    }
                }
            }
        }
    }
}

