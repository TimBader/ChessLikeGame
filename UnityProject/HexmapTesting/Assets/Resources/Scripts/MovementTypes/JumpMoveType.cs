using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//******************************************************************************************************************************************************************************************************************
//  ------  Jump  ------  //
//******************************************************************************************************************************************************************************************************************

public class JumpMoveType : MovementTypeParent
{
    public List<Vector2> jumpPositions = new List<Vector2>();
    public List<Vector2> adjustedJumpPositions = new List<Vector2>();

    public JumpMoveType(Vector2[] jumpMovementPositions, bool verify = false)
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
        for (int i = 0; i < jumpPositions.Count; i++)
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
            adjustedJumpPositions.Add(TileController.rotate(jumpPositions[i], direction));
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
                InteractionIcon.createInteractionIcon("JumpIcon", tile.getCoords(), new Color(1.0f, 0.0f, 1.0f, 1.0f), 0);
                if (tile.getOccupyingUnit())
                {
                    if (tile.getOccupyingUnit().getTeam() != selectedUnit.getTeam())
                    {
                        InteractionIcon.createInteractionIcon("AttackIcon", tile.getCoords(), new Color(1.0f, 0.0f, 0.0f, 1.0f), 1);
                    }
                    else
                    {
                        InteractionIcon.createInteractionIcon("CrossIcon", tile.getCoords(), new Color(1.0f, 0.0f, 0.0f, 1.0f), 1);
                    }
                }
            }
        }
        //MovementIcon m = new MovementIcon();
        //m.initialize("PlaceIcon", );
    }
};