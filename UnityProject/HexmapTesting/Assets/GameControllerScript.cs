using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum InteractionStates
{
    SelectingAction, 
    SelectingMovement,
    SelectingUnitSpawnPoint,
    SelectingKingLocation
};

public class GameControllerScript : MonoBehaviour
{
    public GameObject teamControllerType;

    private InteractionStates interactionState = InteractionStates.SelectingAction;
    private UnitScript selectedUnit = null;
    private bool interactionEnabled = false;
    private uint numberOfTeams = 2;
    private uint currentTeam = 0;
    private List<TeamControllerScript> teamControllers = new List<TeamControllerScript>();
    private SpawnTiles tileControllerScript = null;



    // Use this for initialization
    void Start()
    {
        GameObject[] gos = FindObjectsOfType<GameObject>();
        for (int i = 0; i < gos.Length; i++)
        {
            SpawnTiles spawnTilesScript = (SpawnTiles)gos[i].GetComponent(typeof(SpawnTiles));
            if (spawnTilesScript != null)
            {
                tileControllerScript = spawnTilesScript;
                break;
            }
        }
        if (tileControllerScript == null)
        {
            throw new MissingComponentException("Tile Controller Script not found on any objects");
        }
        //print("Found TileControllerScript!!!");
        tileControllerScript.initialize();

        enableInteraction();
        switchInteractionState(InteractionStates.SelectingKingLocation);
    }



    public void enableInteraction()
    {
        interactionEnabled = true;
    }



    public void disableInteraction()
    {
        interactionEnabled = false;
    }



    void Update()
    {
        if (interactionEnabled)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Vector3 mpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                HexTile hexTile = tileControllerScript.getTileAtPixelPos(mpos);
                if (hexTile != null)
                {
                    clickedOnTile(hexTile);
                }
            }
            if (Input.GetButtonDown("Fire2"))
            {
                if (interactionState == InteractionStates.SelectingMovement || interactionState == InteractionStates.SelectingUnitSpawnPoint)
                {
                    switchInteractionState(InteractionStates.SelectingAction);
                }
            }
            if (Input.GetButtonDown("Jump"))
            {
                if (interactionState == InteractionStates.SelectingAction)
                {
                    switchInteractionState(InteractionStates.SelectingUnitSpawnPoint);
                }
            }
        }
    }



    void switchToNextTeam()
    {
        currentTeam++;
        if (currentTeam == numberOfTeams)
        {
            currentTeam = 0;
        }

        switchInteractionState(InteractionStates.SelectingAction);
    }



    void createTeamController(uint teamNum, UnitScript kingRef)
    {
        for (int i = 0; i < teamControllers.Count; i++)
        {
            if (teamControllers[i].getTeam() == teamNum)
            {
                throw new UnityException("Team Controller already created with that team");
            }
        }
        GameObject iTeam = (GameObject)Instantiate(teamControllerType, Vector2.zero, Quaternion.identity);
        TeamControllerScript teamScript = (TeamControllerScript)iTeam.GetComponent(typeof(TeamControllerScript));
        teamScript.setKing(kingRef);
        teamScript.setTeam(teamNum);
        teamControllers.Add(teamScript);

    }



    TeamControllerScript getTeamController(uint teamNum)
    {
        for (int i = 0; i < teamControllers.Count; i++)
        {
            if (teamControllers[i].getTeam() == teamNum)
            {
                return teamControllers[i];
            }
        }
        return null;
    }



// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
    // ****** Switch Interaction States ****** //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

    void switchInteractionState(InteractionStates state)
    {
        interactionState = state;
        switch (state)
        {
            case InteractionStates.SelectingAction:
                startSelectingAction();
                break;

            case InteractionStates.SelectingMovement:
                startSelectingMovement();
                break;

            case InteractionStates.SelectingUnitSpawnPoint:
                startSelectingUnitSpawnPoint();
                break;

            case InteractionStates.SelectingKingLocation:
                startSelectingKingLocation();
                break;
        }
    }


    // -- Start Selecting Action
    void startSelectingAction()
    {
        print("Select unit to move or press [ Space ] to spawn a unit");

        selectedUnit = null;

        tileControllerScript.switchAllTileStates();

        UnitScript[] allUnits = FindObjectsOfType<UnitScript>();
        for (int i = 0; i < allUnits.Length; i++)
        {
            if (allUnits[i].getTeam() == currentTeam && !allUnits[i].beingDestroyed())
            {
                //print("Woof");
                allUnits[i].getOccupyingHex().switchState(TileState.SELECTABLE);
            }
        }
    }


    // -- Start Selecting Movement
    void startSelectingMovement()
    {
        print("Select movement");
        if (selectedUnit == null)
        {
            throw new UnassignedReferenceException("Game Interaction State moved into SelectingMovement State without a selected unit");
        }

        tileControllerScript.switchAllTileStates();

        selectedUnit.getOccupyingHex().switchState(TileState.SELECTED);

        List<Vector2> movePositions = selectedUnit.getMovePositions();

        //Find Moveable and attackable tiles
        for (int i = 0; i < movePositions.Count; i++)
        {
            HexTile tile = tileControllerScript.getTileFromHexCoord(movePositions[i] + selectedUnit.getOccupyingHex().getCoords());
            if (tile != null)
            {
                tile.switchState(TileState.MOVEABLE);
                if (tile.getOccupyingUnit() != null)
                {
                    if (tile.getOccupyingUnit().getTeam() != selectedUnit.getTeam())
                    {
                        tile.switchState(TileState.ATTACKABLE);
                    }
                    else
                    {
                        tile.switchState(TileState.NONE);
                    }
                }
            }
        }
    }


    // -- Start Selecting Unit Spawn Point
    void startSelectingUnitSpawnPoint()
    {
        selectedUnit = null;

        tileControllerScript.switchAllTileStates();

        List<HexTile> adjacent = tileControllerScript.getAdjacentTiles(getTeamController(currentTeam).getKing().getOccupyingHex());

        bool freeSpace = false;
        for (int i = 0; i < adjacent.Count; i++)
        {
            if (adjacent[i] != null)
            {
                if (adjacent[i].getOccupyingUnit() == null)
                {
                    adjacent[i].switchState(TileState.SPAWNABLE);
                    if (!freeSpace)
                    {
                        print("Select unit spawn point");
                    }
                    freeSpace = true;
                }
            }
        }
        if (!freeSpace)
        {
            print("Cannot spawn, No spawnable locations");
            switchInteractionState(InteractionStates.SelectingAction);
        }
    }


    // -- Start Selecting King Location
    void startSelectingKingLocation()
    {
        print("Select team " + currentTeam + "'s king location");
        selectedUnit = null;

        List<HexTile> tiles = tileControllerScript.getAllTiles();
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] != null)
            {
                if (tiles[i].teamSpawnLoc == currentTeam)
                {
                    tiles[i].switchState(TileState.SPAWNABLE);
                }
                else
                {
                    tiles[i].switchState(TileState.NONE);
                }
            }
        }
    }



// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
    // ****** Clicked In Interaction States ****** //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

    void clickedOnTile(HexTile clickedTile)
    {
        if (clickedTile != null)
        {
            switch (interactionState)
            {
                case (InteractionStates.SelectingAction):
                    selectingUnit(clickedTile);
                    break;
                case (InteractionStates.SelectingMovement):
                    selectingMovement(clickedTile);
                    break;
                case (InteractionStates.SelectingUnitSpawnPoint):
                    selectingUnitSpawnPoint(clickedTile);
                    break;
                case (InteractionStates.SelectingKingLocation):
                    selectingKingLocation(clickedTile);
                    break;
            }
        }
    }


    // -- Selecting King Location
    void selectingKingLocation(HexTile clickedTile)
    {
        List<HexTile> tiles = tileControllerScript.getAllTiles();
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] != null)
            {
                if (tiles[i] == clickedTile)
                {
                    if (tiles[i].teamSpawnLoc == currentTeam)
                    {
                        List<Vector2> rm2 = new List<Vector2>();
                        rm2.Add(new Vector2(0, -1));
                        rm2.Add(new Vector2(1, -1));
                        rm2.Add(new Vector2(1, 0));
                        rm2.Add(new Vector2(0, 1));
                        rm2.Add(new Vector2(-1, 1));
                        rm2.Add(new Vector2(-1, 0));

                        //Spawing guy on Tile
                        GameObject spawningUnit = (GameObject)Instantiate(tileControllerScript.spawnUnitType, tileControllerScript.hexCoordToPixelCoord(clickedTile.getCoords()), Quaternion.identity);
                        UnitScript unitScript = (UnitScript)spawningUnit.GetComponent(typeof(UnitScript));
                        clickedTile.setOccupyingUnit(unitScript);
                        unitScript.setOccupyingHex(clickedTile);
                        unitScript.setMovePositions(rm2);
                        unitScript.setTeam(currentTeam);
                        unitScript.setUnitType(UnitType.KingUnit);
                        //Set the above Character as the King for teamController
                        createTeamController(currentTeam, unitScript);

                        if (currentTeam + 1 < numberOfTeams)
                        {
                            currentTeam++;
                            switchInteractionState(InteractionStates.SelectingKingLocation);
                        }
                        else
                        {
                            currentTeam = 0;
                            switchInteractionState(InteractionStates.SelectingAction);
                        }
                    }
                }
            }
        }
    }


    // -- Selecting Movement
    void selectingMovement(HexTile clickedTile)
    {
        if (clickedTile == selectedUnit.getOccupyingHex())
        {
            switchInteractionState(InteractionStates.SelectingAction);
        }
        else
        {
            HexTile foundTile = null;
            for (int i = 0; i < selectedUnit.getMovePositions().Count; i++)
            {
                HexTile tile = tileControllerScript.getTileFromHexCoord(selectedUnit.getOccupyingHex().getCoords() + selectedUnit.getMovePositions()[i]);
                if (clickedTile == tile)
                {
                    //Don't allow to move to this tile if it has a unit of the same team on it
                    if (tile.getOccupyingUnit() != null)
                    {
                        if (tile.getOccupyingUnit().getTeam() == selectedUnit.getTeam())
                            continue;
                    }
                    foundTile = tile;
                    break;
                }
            }

            if (foundTile != null)
            {
                selectedUnit.getOccupyingHex().switchState(TileState.NONE);
                tileControllerScript.switchAllTileStates();

                //Test to see if an enemy is on the other team
                if (foundTile.getOccupyingUnit() != null)
                {
                    if (foundTile.getOccupyingUnit().getUnitType() == UnitType.KingUnit)
                    {
                        foundTile.getOccupyingUnit().destroyUnit();
                        foundTile.setOccupyingUnit(null);
                        print("King Destroyed, yeah game wont quit out on itself... idk why");
                        Application.Quit();
                    }
                    else
                    {
                        foundTile.getOccupyingUnit().destroyUnit();
                        foundTile.setOccupyingUnit(null);
                        print("Unit Destroyed");
                    }
                }


                tileControllerScript.transferUnit(selectedUnit.getOccupyingHex(), foundTile);
                selectedUnit.transform.position = tileControllerScript.hexCoordToPixelCoord(clickedTile.getCoords());
                switchToNextTeam();
            }
        }
    }


    // -- Selecting Unit Spawn Point
    void selectingUnitSpawnPoint(HexTile clickedTile)
    {
        if (clickedTile.getOccupyingUnit() == null)
        {
            bool spawned = false;
            List<HexTile> adjacentTiles = tileControllerScript.getAdjacentTiles(getTeamController(currentTeam).getKing().getOccupyingHex());
            for (int i = 0; i < adjacentTiles.Count; i++)
            {
                if (adjacentTiles[i] != null)
                {
                    if (clickedTile == adjacentTiles[i] && adjacentTiles[i].getOccupyingUnit() == null)
                    {
                        List<Vector2> rm = new List<Vector2>();
                        rm.Add(new Vector2(2, -1));
                        rm.Add(new Vector2(0, 2));
                        rm.Add(new Vector2(0, -2));
                        rm.Add(new Vector2(-2, 1));

                        GameObject spawningUnit = (GameObject)Instantiate(tileControllerScript.spawnUnitType, tileControllerScript.hexCoordToPixelCoord(clickedTile.getCoords()), Quaternion.identity);
                        UnitScript unitScript = (UnitScript)spawningUnit.GetComponent(typeof(UnitScript));
                        clickedTile.setOccupyingUnit(unitScript);
                        unitScript.setOccupyingHex(clickedTile);
                        unitScript.setMovePositions(rm);
                        unitScript.setTeam(currentTeam);

                        spawned = true;

                        break;
                    }
                }
            }
            if (spawned)
            {
                switchToNextTeam();
            }
        }
    }


    // -- Selecting Unit
    void selectingUnit(HexTile clickedTile)
    {
        if (clickedTile.getOccupyingUnit() != null)
        {
            //Check to see if clicked unit is on the current team
            if (clickedTile.getOccupyingUnit().getTeam() == currentTeam)
            {
                selectedUnit = clickedTile.getOccupyingUnit();
                switchInteractionState(InteractionStates.SelectingMovement);
            }
        }
    }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

}