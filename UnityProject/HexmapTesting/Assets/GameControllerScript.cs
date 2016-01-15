using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum InteractionStates
{
    SelectingKingLocation,
    SelectingUnitSpawnPoint,
    SelectingSpawnedUnitDirection,
    SelectingUnitToMove,
    SelectingUnitMovement,
    SelectingUnitToRotate,
    SelectingUnitRotation
};



public class GameControllerScript : MonoBehaviour
{
    public GameObject teamControllerType;

    private InteractionStates interactionState = InteractionStates.SelectingKingLocation;
    private UnitScript selectedUnit = null;
    private bool interactionEnabled = false;
    private uint numberOfTeams = 2;
    private uint currentTeam = 0;
    private List<TeamControllerScript> teamControllers = new List<TeamControllerScript>();
    private SpawnTiles tileControllerScript = null;

    private SpriteResourceManager spriteResourceManagerScript = null;

    private Dictionary<string, UnitInfo> unitInfoDictionary = new Dictionary<string, UnitInfo>();

    //private List<UnitInfo> tempUnitInfos = new List<UnitInfo>();

    private float rightClickTimeCache;

    //Counter for alternating movements
    public int altCounter = 0;

    private Color[] teamColorList = 
    {
        new Color(0.0f, 0.0f, 1.0f),
        new Color(1.0f, 0.0f, 0.0f)
    };


    public Color getTeamColor(uint teamNum = 0)
    {
        if (teamNum >= teamColorList.Length)
        {
            throw new UnityException("Team " + teamNum + " does not have a team color set");
        }
        return teamColorList[teamNum];
    }


    public UnitScript getSelectedUnit()
    {
        return selectedUnit;
    }


    public SpawnTiles getTileController()
    {
        return tileControllerScript;
    }


    public uint getCurrentTeam()
    {
        return currentTeam;
    }


    //Helper for non-MonoDevelopment scripts print stuff / such as MovementObjects :D
    public void printString(string str)
    {
        print(str);
    }


    public void addNewUnitInfo(UnitInfo info)
    {
        if (info.unitName == "")
        {
            throw new UnityException("A unit's name is not defined");
        }
        if (info.baseSpriteName == "" && info.colorsSpriteName == "")
        {
            throw new UnityException(info.unitName + " does not have any images named");
        }
        if (unitInfoDictionary.ContainsKey(info.unitName))
        {
            throw new UnityException(info.unitName + " already created");
        }
        unitInfoDictionary.Add(info.unitName, info);
    }



    public UnitInfo getUnitInfo(string unitName)
    {
        UnitInfo info;
        if (!unitInfoDictionary.TryGetValue(unitName, out info))
        {
            throw new UnityException(unitName + " does not exist");
        }

        return info;
    }


    // Use this for initialization
    void Start()
    {

        MovementTypeParent.setGameControllerRef(this);


        TempUnitInfos.constructTempUnitInfos();


        bool foundTileController = false;
        bool foundSpriteManager = false;
        GameObject[] gos = FindObjectsOfType<GameObject>();
        for (int i = 0; i < gos.Length; i++)
        {
            SpawnTiles spawnTilesScript = (SpawnTiles)gos[i].GetComponent(typeof(SpawnTiles));
            if (spawnTilesScript != null)
            {
                tileControllerScript = spawnTilesScript;
                foundTileController = true;
            }
            else
            {
                SpriteResourceManager spm = (SpriteResourceManager)gos[i].GetComponent(typeof(SpriteResourceManager));
                if (spm != null)
                {
                    spriteResourceManagerScript = spm;
                    foundSpriteManager = true;
                }
            }
            if (foundTileController && foundSpriteManager)
            {
                break;
            }
        }
        if (!foundTileController)
        {
            throw new MissingComponentException("Tile Controller Script not found, please include in scene");
        }
        if (!foundSpriteManager)
        {
            throw new MissingComponentException("Sprite Resource Manager Script not found, please include in scene");
        }
        tileControllerScript.initialize();


        //Adding all the unit data to dictionary
        for (int i = 0; i < TempUnitInfos.getTempUnitInfos().Count; i++ )
        {
            addNewUnitInfo(TempUnitInfos.getTempUnitInfos()[i]);
        }


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
                HexTile hexTile = tileControllerScript.getTileAtPixelPos(mpos, true);
                if (hexTile != null)
                {
                    clickedOnTile(hexTile);
                }
            }
            if (Input.GetButtonDown("Fire2"))
            {
                float diff = Time.time - rightClickTimeCache;

                if (diff <= 0.20)
                {
                    if (interactionState == InteractionStates.SelectingUnitSpawnPoint || interactionState == InteractionStates.SelectingSpawnedUnitDirection)
                    {
                        switchInteractionState(InteractionStates.SelectingUnitToMove);
                    }

                    if (interactionState == InteractionStates.SelectingUnitMovement)
                    {
                        switchInteractionState(InteractionStates.SelectingUnitToMove);
                    }

                    if (interactionState == InteractionStates.SelectingUnitRotation)
                    {
                        switchInteractionState(InteractionStates.SelectingUnitToRotate);
                    }
                }

                rightClickTimeCache = Time.time;
            }
            if (Input.GetButtonDown("Jump"))
            {
                if (interactionState == InteractionStates.SelectingUnitSpawnPoint || interactionState == InteractionStates.SelectingSpawnedUnitDirection)
                {
                    switchInteractionState(InteractionStates.SelectingUnitToMove);
                }
                if (interactionState == InteractionStates.SelectingUnitRotation || interactionState == InteractionStates.SelectingUnitToRotate)
                {
                    switchToNextTeam();
                }
            }
            if (Input.GetButtonDown("Alt"))
            {
                if (interactionState == InteractionStates.SelectingUnitMovement)
                {
                    altCounter++;
                    if (altCounter >= selectedUnit.getUnitInfo().movementObjects.Count)
                    {
                        altCounter = 0;
                    }

                    switchInteractionState(InteractionStates.SelectingUnitMovement);
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

        print("Switched Team To: " + currentTeam);

        switchInteractionState(InteractionStates.SelectingUnitSpawnPoint);
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

    public void switchInteractionState(InteractionStates state)
    {
        //Ending Previous State
        switch (interactionState)
        {
            case InteractionStates.SelectingSpawnedUnitDirection:
                endSelectingSpawnedUnitDirection();
                break;

            case InteractionStates.SelectingUnitRotation:
                endSelectingUnitRotation();
                break;

            case InteractionStates.SelectingUnitMovement:
                endSelectingUnitMovement(state);
                break;

        }

        //Starting Next State
        interactionState = state;
        switch (state)
        {
            case InteractionStates.SelectingKingLocation:
                startSelectingKingLocation();
                break;

            case InteractionStates.SelectingUnitSpawnPoint:
                startSelectingUnitSpawnPoint();
                break;

            case InteractionStates.SelectingSpawnedUnitDirection:
                startSelectingSpawnedUnitDirection();
                break;

            case InteractionStates.SelectingUnitToMove:
                startSelectingUnitToMove();
                break;

            case InteractionStates.SelectingUnitMovement:
                startSelectingUnitMovement();
                break;

            case InteractionStates.SelectingUnitToRotate:
                startSelectingUnitToRotate();
                break;

            case InteractionStates.SelectingUnitRotation:
                startSelectingUnitRotation();
                break;

        }
    }


// -- Start Selecting King Location
    void startSelectingKingLocation()
    {
        print("Select Team " + currentTeam + "'s King Location");
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
                    freeSpace = true;
                }
            }
        }
        if (!freeSpace)
        {
            print("Cannot spawn, No spawnable locations");
            switchInteractionState(InteractionStates.SelectingUnitToMove);
        }
        print("TEAM: " + currentTeam + " --> Select Unit Spawn Point.  Press [SpaceBar] to skip");
    }


// -- Start Selecting Spawned Unit Direction
    void startSelectingSpawnedUnitDirection()
    {
        if (selectedUnit == null)
        {
            throw new UnityException("selectedUnit must not be null for startSelectingSpawnedUnitDirection()");
        }

        tileControllerScript.switchAllTileStates();

        selectedUnit.getOccupyingHex().switchState(TileState.SELECTED);

        tileControllerScript.addNewTempTile(selectedUnit.getOccupyingHex().getCoords() + new Vector2(0, 1));
        tileControllerScript.addNewTempTile(selectedUnit.getOccupyingHex().getCoords() + new Vector2(1, 0));
        tileControllerScript.addNewTempTile(selectedUnit.getOccupyingHex().getCoords() + new Vector2(1, -1));
        tileControllerScript.addNewTempTile(selectedUnit.getOccupyingHex().getCoords() + new Vector2(0, -1));
        tileControllerScript.addNewTempTile(selectedUnit.getOccupyingHex().getCoords() + new Vector2(-1, 0));
        tileControllerScript.addNewTempTile(selectedUnit.getOccupyingHex().getCoords() + new Vector2(-1, 1));

        List<HexTile> adjacentTiles = tileControllerScript.getAdjacentTiles(selectedUnit.getOccupyingHex(), true);

        if (selectedUnit.getUnitInfo().rotationEnabled)
        {
            print("TEAM: " + currentTeam + " --> Select Spawned Unit's Rotation.  Press [SpaceBar] to skip");
            for (int i = 0; i < adjacentTiles.Count; i++)
            {
                if (adjacentTiles[i] != null)
                {
                    adjacentTiles[i].switchState(TileState.MOVEABLE);
                }
                else
                {
                    throw new UnityException("Odd there isnt a hex tile where there should... double check that");
                }
            }
        }
        else
        {
            switchInteractionState(InteractionStates.SelectingUnitToMove);
        }
    }


// -- Start Selecting Unit To Move
    void startSelectingUnitToMove()
    {
        print("TEAM: " + currentTeam + " --> Select Unit To Move");

        selectedUnit = null;

        tileControllerScript.switchAllTileStates(TileState.NONE);

        UnitScript[] allUnits = FindObjectsOfType<UnitScript>();

        for (int i = 0; i < allUnits.Length; i++)
        {
            if (allUnits[i].getTeam() == currentTeam)
            {
                allUnits[i].getOccupyingHex().switchState(TileState.SELECTABLE);
            }

            //Make Sure to do checks for no movement possibilities!!!
        }
    }


// -- Start Selecting Unit Movement
    void startSelectingUnitMovement()
    {
        //SelectedUnit is needed
        if (selectedUnit == null)
        {
            throw new UnityException("SelectedUnit is required for interaction state: startSelectingUnitMovement()");
        }

        print("TEAM: " + currentTeam + " --> Select What Tile To Move The Selected Unit To");

        tileControllerScript.switchAllTileStates();

        selectedUnit.getOccupyingHex().switchState(TileState.SELECTED);

        //selectedUnit.getUnitInfo().movementObject.startSelectingInMode(selectedUnit, currentTeam);
        selectedUnit.getUnitInfo().movementObjects[altCounter].startSelectingInMode(selectedUnit, currentTeam);
    }


// -- Start Selecting Unit To Rotate
    void startSelectingUnitToRotate()
    {
        selectedUnit = null;
        tileControllerScript.switchAllTileStates();

        UnitScript[] units = FindObjectsOfType<UnitScript>();

        bool foundOne = false;
        for (int i = 0; i < units.Length; i++)
        {
            if (units[i].getTeam() == currentTeam)
            {
                //print("Kitty cat was found!!!");
                if (units[i].getUnitInfo().rotationEnabled || units[i].getUnitInfo().relativeRotationDirections.Count != 0)
                {
                    foundOne = true;
                    units[i].getOccupyingHex().switchState(TileState.MOVEABLE);
                }
            }
        }
        if (!foundOne)
        {
            switchToNextTeam();
        }
        print("TEAM: " + currentTeam + " --> Select Unit to Rotate.  Press [SpaceBar] to skip");
    }


// -- Start Selecting Unit Rotation
    void startSelectingUnitRotation()
    {
        if (selectedUnit == null)
        {
            throw new UnityException("selectedUnit must not be null for startSelectingUnitRotation()");
        }

        print("TEAM: " + currentTeam + " --> Select Unit's Rotation Direciton.  Press [SpaceBar] to skip");

        tileControllerScript.switchAllTileStates();

        selectedUnit.getOccupyingHex().switchState(TileState.SELECTED);

        List<RelativeDirection> relativeDirections = selectedUnit.getUnitInfo().relativeRotationDirections;

        for (int i = 0; i < relativeDirections.Count; i++)
        {
            AbsoluteDirection rotationDirection = SpawnTiles.relativeToAbsoluteDirection(selectedUnit.getRotation(), relativeDirections[i]);
            
            //Creating Temp Tiles if any just incase
            Vector2 hexCoord = selectedUnit.getOccupyingHex().getCoords() + SpawnTiles.rotationDirectionToRelativePos(rotationDirection);
            tileControllerScript.addNewTempTile(hexCoord);

            HexTile tile = tileControllerScript.getTileFromHexCoord(hexCoord, true);
            if (tile != null)
            {
                tile.switchState(TileState.MOVEABLE);
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
                case InteractionStates.SelectingKingLocation:
                    selectingKingLocation(clickedTile);
                    break;

                case InteractionStates.SelectingUnitSpawnPoint:
                    selectingUnitSpawnPoint(clickedTile);
                    break;

                case InteractionStates.SelectingSpawnedUnitDirection:
                    selectingSpawnedUnitDirection(clickedTile);
                    break;

                case InteractionStates.SelectingUnitToMove:
                    selectingUnitToMove(clickedTile);
                    break;

                case InteractionStates.SelectingUnitMovement:
                    selectingUnitMovement(clickedTile);
                    break;

                case InteractionStates.SelectingUnitToRotate:
                    selectingUnitToRotate(clickedTile);
                    break;

                case InteractionStates.SelectingUnitRotation:
                    selectingUnitRotation(clickedTile);
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

                        //Spawing guy on Tile
                        GameObject spawningUnit = (GameObject)Instantiate(tileControllerScript.spawnUnitType, tileControllerScript.hexCoordToPixelCoord(clickedTile.getCoords()), Quaternion.identity);
                        UnitScript unitScript = (UnitScript)spawningUnit.GetComponent(typeof(UnitScript));
                        clickedTile.setOccupyingUnit(unitScript);
                        unitScript.setOccupyingHex(clickedTile);

                        UnitInfo newKingInfo = getUnitInfo("King");
                        unitScript.initialize(newKingInfo);

                        //unitScript.setMovePositions(newKingInfo.movements);
                        if (newKingInfo.baseSpriteName != null)
                        {
                            unitScript.getBaseSpriteRenderer().sprite = spriteResourceManagerScript.loadSprite(newKingInfo.baseSpriteName);
                        }
                        if (newKingInfo.colorsSpriteName != null)
                        {
                            unitScript.getColorsSpriteRenderer().sprite = spriteResourceManagerScript.loadSprite(newKingInfo.colorsSpriteName);
                        } 
                        unitScript.getColorsSpriteRenderer().color = getTeamColor(currentTeam);

                        //unitScript.setMovePositions(rm2);
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
                            switchInteractionState(InteractionStates.SelectingUnitSpawnPoint);
                        }
                    }
                }
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
                        //For temp spawn list
                        string[] tempSpawnList = 
                        {
                            "BasicUnit",
                            "SpecialUnit",
                            "KnightUnit",
                            "NormalUnit",
                            "RangedUnit"
                        };

                        GameObject spawningUnit = (GameObject)Instantiate(tileControllerScript.spawnUnitType, tileControllerScript.hexCoordToPixelCoord(clickedTile.getCoords()), Quaternion.identity);
                        UnitScript unitScript = (UnitScript)spawningUnit.GetComponent(typeof(UnitScript));
                        clickedTile.setOccupyingUnit(unitScript);
                        unitScript.setOccupyingHex(clickedTile);

                        UnitInfo newUnitInfo = getUnitInfo(tempSpawnList[Random.Range(0, tempSpawnList.Length)]);

                        unitScript.initialize(newUnitInfo);

                        if (newUnitInfo.baseSpriteName != null)
                        {
                            unitScript.getBaseSpriteRenderer().sprite = spriteResourceManagerScript.loadSprite(newUnitInfo.baseSpriteName);
                        }
                        if (newUnitInfo.colorsSpriteName != null)
                        {
                            unitScript.getColorsSpriteRenderer().sprite = spriteResourceManagerScript.loadSprite(newUnitInfo.colorsSpriteName);
                        }
                        unitScript.getColorsSpriteRenderer().color = getTeamColor(currentTeam);
                        //unitScript.setMovePositions(newUnitInfo.movements);
                        unitScript.setTeam(currentTeam);

                        selectedUnit = unitScript;

                        spawned = true;
                        break;
                    }
                }
            }
            if (spawned)
            {
                //switchInteractionState(InteractionStates.SelectingSpawnedUnitDirection);
                switchInteractionState(InteractionStates.SelectingSpawnedUnitDirection);
            }
        }
    }


// -- Selecting Spawned Unit's Direction
    void selectingSpawnedUnitDirection(HexTile clickedTile)
    {
        if (clickedTile.getCurrentTileState() == TileState.MOVEABLE)
        {
            Vector2 relPosOfClicked = clickedTile.getCoords() - selectedUnit.getOccupyingHex().getCoords();

            selectedUnit.setRotationDirection(SpawnTiles.relativePosToAbsoluteDirection(relPosOfClicked));

            switchInteractionState(InteractionStates.SelectingUnitToMove);
        }
    }


// -- Selecting Unit To Move
    void selectingUnitToMove(HexTile clickedTile)
    {
        if (clickedTile.getCurrentTileState() == TileState.SELECTABLE)
        {
            selectedUnit = clickedTile.getOccupyingUnit();
            switchInteractionState(InteractionStates.SelectingUnitMovement);
        }
    }


// -- Selecting Unit Movement
    void selectingUnitMovement(HexTile clickedTile)
    {
        if (clickedTile.getCurrentTileState() == TileState.SELECTABLE || clickedTile.getCurrentTileState() == TileState.ATTACKABLE)
        {
            //selectedUnit.getUnitInfo().movementObject.clickedInMode(clickedTile, selectedUnit, currentTeam);
            selectedUnit.getUnitInfo().movementObjects[altCounter].clickedInMode(clickedTile, selectedUnit, currentTeam);

        }
        else if (clickedTile.getOccupyingUnit())
        {
            if (selectedUnit == clickedTile.getOccupyingUnit())
            {
                switchInteractionState(InteractionStates.SelectingUnitToMove);
            }
            else if (clickedTile.getOccupyingUnit().getTeam() == currentTeam)
            {
                selectedUnit = clickedTile.getOccupyingUnit();
                switchInteractionState(InteractionStates.SelectingUnitMovement);
            }
        }
    }


// -- Selecting Unit To Rotate
    void selectingUnitToRotate(HexTile clickedTile)
    {
        if (clickedTile.getCurrentTileState() == TileState.MOVEABLE)
        {
            selectedUnit = clickedTile.getOccupyingUnit();

            switchInteractionState(InteractionStates.SelectingUnitRotation);
        }
    }


// -- Selecting Unit Rotation
    void selectingUnitRotation(HexTile clickedTile)
    {
        if (clickedTile.getOccupyingUnit())
        {
            if (selectedUnit == clickedTile.getOccupyingUnit())
            {
                switchInteractionState(InteractionStates.SelectingUnitToRotate);
            }
        }
        else if (clickedTile.getCurrentTileState() == TileState.MOVEABLE)
        {
            selectedUnit.setRotationDirection(SpawnTiles.relativePosToAbsoluteDirection(clickedTile.getCoords() - selectedUnit.getOccupyingHex().getCoords()));

            switchToNextTeam();
        }
    }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
    // ****** End In Interaction State ****** //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

// -- End Selecting Spawned Unit Direction
    void endSelectingSpawnedUnitDirection()
    {
        tileControllerScript.clearAllTempTiles();
    }


// -- End Selecting Unit Rotation
    void endSelectingUnitRotation()
    {
        tileControllerScript.clearAllTempTiles();
    }

// -- End Selecting Unit Movement
    void endSelectingUnitMovement(InteractionStates nextInteractionState)
    {
        if (nextInteractionState != InteractionStates.SelectingUnitMovement)
        {
            altCounter = 0;
        }
    }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------//
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

};