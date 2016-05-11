using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//******************************************************************************************************************************************************************************************************************
//  ------  Ranged  ------  //
//******************************************************************************************************************************************************************************************************************

public class RangedMoveType : MovementTypeParent
{
    public List<List<Vector2>> relativeLocations = new List<List<Vector2>>();
    public List<List<Vector2>> adjustedLocations = new List<List<Vector2>>();

    public RangedMoveType(Vector2[][] relativeLocs, bool verify = false)
    {
        for (int i = 0; i < relativeLocs.Length; i++)
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
                            for (int k = 0; k < adjustedLocations[i].Count; k++)
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
        Vector2 tileLoc = clickedTile.getCoords();

        for (int i = 0; i < adjustedLocations.Count; i++)
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

    public override bool canMove(UnitScript selectedUnit, int currentTeam)
    {
        for (int i = 0; i < relativeLocations.Count; i++)
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
                relPos.Add(TileController.rotate(relativeLocations[i][j], direction));
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
        //gameControllerRef.printString("Meowsquers");
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
                    InteractionIcon.createInteractionIcon("RangedIcon", tile.getCoords(), new Color(0.0f, 1.0f, 0.0f, 1.0f), 0);
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

                        if (foundFriend && j == adjustedLocations[i].Count - 1)
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
                        InteractionIcon.createInteractionIcon("AttackIcon", tile.getCoords(), new Color(1.0f, 0.0f, 0.0f, 1.0f), 1);
                    }
                    else
                    {
                        //createMovementIcon("RangedIcon", tile.getCoords(), new Color(0.0f, 1.0f, 0.0f, 1.0f), 0);
                        InteractionIcon.createInteractionIcon("CrossIcon", tile.getCoords(), new Color(1.0f, 0.0f, 0.0f, 1.0f), 1);
                    }
                }
            }
        }
    }
}