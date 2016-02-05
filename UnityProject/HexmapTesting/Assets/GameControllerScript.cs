using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum InteractionStates
{
    SelectingLordLocation,
    SelectingGuardSpawnPoint,
    SelectingGuardDirection,
    SelectingUnitSpawnPoint,
    SelectingSpawnedUnitDirection,
    SelectingUnitToMove,
    SelectingUnitMovement,
    SelectingUnitToRotate,
    SelectingUnitRotation
};


public enum StateReturns
{
    Next,
    Skipped,
    Canceled,
    Alternate
};


public class GameControllerScript : MonoBehaviour
{
    public GameObject teamControllerType;

    private InteractionStates interactionState = InteractionStates.SelectingLordLocation;
    private UnitScript selectedUnit = null;
    private bool interactionEnabled = false;
    private uint numberOfTeams = 2;
    private int currentTeam = 0;
    private List<TeamControllerScript> teamControllers = new List<TeamControllerScript>();
    private SpawnTiles tileControllerScript = null;

    //private SpriteResourceManager spriteResourceManagerScript = null;

    private Dictionary<string, UnitInfo> unitInfoDictionary = new Dictionary<string, UnitInfo>();

    private UnitScript justSpawnedUnit = null;

    public Camera mainCamera;

    private float minZoomSize = 1.0f;
    private float maxZoomSize = 6.0f;
    private float zoomRate = 2.0f;

    //private List<UnitInfo> tempUnitInfos = new List<UnitInfo>();

    private float rightClickTimeCache;

    private float escapeTimeCache;

    //Counter for alternating movements
    public int altCounter = 0;

    private int tempCounter = 0;

    ///Temp ui stuff
    /// //////////////////////////////////////////////////////////
    public Text tempUIText;
    public Text tempUIText2;
    
    public void setUIText(string text, Color color)
    {
        tempUIText.text = text;
        tempUIText.color = color;
    }

    public void setUIText2(string text, Color color)
    {
        tempUIText2.text = text;
        tempUIText2.color = color;
    }
    /// //////////////////////////////////////////////////////////

    public UnitScript getSelectedUnit()
    {
        return selectedUnit;
    }


    public SpawnTiles getTileController()
    {
        return tileControllerScript;
    }


    public int getCurrentTeam()
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
        if (info.baseSpriteName == "" && info.color0SpriteName == "" && info.color1SpriteName == "" && info.color2SpriteName == "" && info.color3SpriteName == "")
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
        UnitScript.gameControllerRef = this;

        TempUnitInfos.constructTempUnitInfos();


        bool foundTileController = false;
        GameObject[] gos = FindObjectsOfType<GameObject>();
        for (int i = 0; i < gos.Length; i++)
        {
            SpawnTiles spawnTilesScript = (SpawnTiles)gos[i].GetComponent(typeof(SpawnTiles));
            if (spawnTilesScript != null)
            {
                tileControllerScript = spawnTilesScript;
                foundTileController = true;
            }
            if (foundTileController/* && foundSpriteManager*/)
            {
                break;
            }
        }
        if (!foundTileController)
        {
            throw new MissingComponentException("Tile Controller Script not found, please include in scene");
        }

        tileControllerScript.initialize();


        //Adding all the unit data to dictionary
        for (int i = 0; i < TempUnitInfos.getTempUnitInfos().Count; i++ )
        {
            addNewUnitInfo(TempUnitInfos.getTempUnitInfos()[i]);
        }

        setUIText("Temp", Color.black);

        //Testing
        /*spawnUnit("SpecialUnit", tileControllerScript.getTileFromHexCoord(new Vector2(4, 1)), 0, false, AbsoluteDirection.UP);
        spawnUnit("BasicUnit", tileControllerScript.getTileFromHexCoord(new Vector2(4, 6)), 1, false, AbsoluteDirection.UP);
        spawnUnit("BasicUnit", tileControllerScript.getTileFromHexCoord(new Vector2(4, 7)), 1, false, AbsoluteDirection.UP);
        spawnUnit("BasicUnit", tileControllerScript.getTileFromHexCoord(new Vector2(4, 8)), 1, false, AbsoluteDirection.UP);
        spawnUnit("BasicUnit", tileControllerScript.getTileFromHexCoord(new Vector2(4, 9)), 1, false, AbsoluteDirection.UP);
        
        //Testing
        AbsoluteDirection ab = SpawnTiles.getDirectionToTile(tileControllerScript.getTileFromHexCoord(new Vector2(3,3)),tileControllerScript.getTileFromHexCoord(new Vector2(2,-1)));
        print("TESTING: " + ab);*/

        enableInteraction();



        switchInteractionState(InteractionStates.SelectingLordLocation);
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
                    /*if (interactionState == InteractionStates.SelectingUnitSpawnPoint || interactionState == InteractionStates.SelectingSpawnedUnitDirection)
                    {
                        switchInteractionState(InteractionStates.SelectingUnitToMove);
                    }*/

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
            if (Input.GetButtonDown("Cancel"))
            {
                float diff = Time.time - escapeTimeCache;

                if (diff <= 0.20f)
                {
                    print("Quiting Application");
                    Application.Quit();
                }

                escapeTimeCache = Time.time;
            }


            if (Input.GetButtonDown("Jump"))
            {
                if (interactionState == InteractionStates.SelectingUnitSpawnPoint || interactionState == InteractionStates.SelectingSpawnedUnitDirection)
                {
                    switchInteractionState(InteractionStates.SelectingUnitToMove);
                }
                if (interactionState == InteractionStates.SelectingGuardDirection /*For Testing*/|| interactionState == InteractionStates.SelectingGuardSpawnPoint)
                {
                    switchInteractionState(InteractionStates.SelectingGuardSpawnPoint);
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
            if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
            {
                float newZoom = mainCamera.orthographicSize - Input.GetAxis("Mouse ScrollWheel")*zoomRate;
                if (newZoom >= minZoomSize && newZoom <= maxZoomSize )

                mainCamera.orthographicSize = newZoom;
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

        UnitScript[] units = FindObjectsOfType<UnitScript>();
        for (int i = 0; i < units.Length; i++ )
        {
            units[i].justSpawned = false;
        }

        print("Switched Team To: " + currentTeam);

        switchInteractionState(InteractionStates.SelectingUnitSpawnPoint);
    }



    void createTeamController(int teamNum, UnitScript lordRef)
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
        teamScript.setLord(lordRef);
        teamScript.setTeam(teamNum);
        teamControllers.Add(teamScript);

    }



    TeamControllerScript getTeamController(int teamNum)
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



    UnitScript spawnUnit(string unitIDName, HexTile spawningTile, int team, bool markSpawn = false, AbsoluteDirection spawnedUnitDirection = AbsoluteDirection.UP)
    {
        GameObject unitGameObject = (GameObject)Instantiate(tileControllerScript.spawnUnitType);
        UnitScript unitScript = (UnitScript)unitGameObject.GetComponent(typeof(UnitScript));

        UnitInfo spawnedUnitInfo = getUnitInfo(unitIDName);

        unitScript.initialize(spawnedUnitInfo, team);

        spawningTile.setOccupyingUnit(unitScript);
        unitScript.setOccupyingHex(spawningTile);

        unitScript.setRotationDirection(spawnedUnitDirection);

        unitScript.justSpawned = markSpawn;

        //unitScript.setTeam(team);

        return unitScript;
    }



    public void captureUnit(UnitScript capturedUnit)
    {
        if (capturedUnit.getUnitType() == UnitType.LordUnit)
        {
            print("Lord Destroyed, Game Over");
            Application.Quit();
        }
        else
        {
            print("Unit Destroyed");
        }
        capturedUnit.getOccupyingHex().setOccupyingUnit(null);
        capturedUnit.destroyUnit();
    }


    public void gamePlayLoop()
    {

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

            case InteractionStates.SelectingGuardDirection:
                endSelectingGuardDirection();
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
            case InteractionStates.SelectingLordLocation:
                startSelectingLordLocation();
                break;

            case InteractionStates.SelectingGuardSpawnPoint:
                startSelectingGuardSpawnPoint();
                break;

            case InteractionStates.SelectingGuardDirection:
                startSelectingGuardDirection();
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


// -- Start Selecting Lord Location
    void startSelectingLordLocation()
    {
        print("Select Team " + currentTeam + "'s Lord Location");
        selectedUnit = null;

        setUIText("Team: " + currentTeam + ", Select Lord Starting Location", new Color(1.0f, 1.0f, 0.0f));
        setUIText2("", Color.black);

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

// -- Start Selecting Guard Spawn Point
    void startSelectingGuardSpawnPoint()
    {
        //Sadly this is an almost exact copy of the Start Selecting Unit Spawn Point

        if (tempCounter > 1)
        {
            tempCounter = 0;
            currentTeam++;
            if (currentTeam >= numberOfTeams)
            {
                currentTeam = 0;
                tileControllerScript.clearAllTempTiles();
                switchInteractionState(InteractionStates.SelectingUnitSpawnPoint);
                return;
            }
        }
        tempCounter++;


        selectedUnit = null;

        tileControllerScript.switchAllTileStates();

        setUIText("Team: " + currentTeam + ", Select Guard Spawn Point", new Color(1.0f, 1.0f, 0.0f));
        setUIText2("", Color.yellow);

        List<HexTile> adjacent = tileControllerScript.getAdjacentTiles(getTeamController(currentTeam).getLord().getOccupyingHex());

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
            print("Cannot spawn guard, No spawnable locations");
            switchInteractionState(InteractionStates.SelectingUnitToMove);
        }
        print("TEAM: " + currentTeam + " --> Select Guard Spawn Point.");

    }

// -- Start Selecting Guard Unit Direction
    void startSelectingGuardDirection()
    {
        if (selectedUnit == null)
        {
            throw new UnityException("selectedUnit must not be null for startSelectingGuardDirection()");
        }

        setUIText("Team: " + currentTeam + ", Select Guards's Starting Rotation", new Color(0.0f, 1.0f, 0.0f));
        setUIText2("Skip <Space>", Color.yellow);

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
            print("TEAM: " + currentTeam + " --> Select Guard Unit's Rotation.  Press [SpaceBar] to skip");
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
            switchInteractionState(InteractionStates.SelectingGuardSpawnPoint);
        }
    }


// -- Start Selecting Unit Spawn Point
    void startSelectingUnitSpawnPoint()
    {
        selectedUnit = null;

        justSpawnedUnit = null;

        tileControllerScript.switchAllTileStates();

        setUIText("Team: " + currentTeam + ", Select Unit Spawn Point", new Color(1.0f, 1.0f, 0.0f));
        setUIText2("Skip <Space>", Color.yellow);

        List<HexTile> adjacent = tileControllerScript.getAdjacentTiles(getTeamController(currentTeam).getLord().getOccupyingHex());

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

        setUIText("Team: " + currentTeam + ", Select " + selectedUnit.getUnitInfo().unitName + "'s Starting Rotation", new Color(0.0f, 1.0f, 0.0f));
        setUIText2("Skip <Space>", Color.yellow);

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

        setUIText("Team: " + currentTeam + ", Select Unit to Move", new Color(0.0f, 1.0f, 1.0f));
        setUIText2("", Color.yellow);

        bool atLeastOneCanMove = false;
        for (int i = 0; i < allUnits.Length; i++)
        {
            if (allUnits[i].getTeam() == currentTeam && !allUnits[i].justSpawned)
            {
                bool canMove = false;
                for (int j = 0; j < allUnits[i].getUnitInfo().movementObjects.Count; j++)
                {
                    if (allUnits[i].getUnitInfo().movementObjects[j].canMove(allUnits[i], currentTeam))
                    {
                        canMove = true;
                        break;
                    }
                }
                if (canMove)
                {
                    atLeastOneCanMove = true;
                    allUnits[i].getOccupyingHex().switchState(TileState.SELECTABLE);
                }
            }
        }
        if (!atLeastOneCanMove)
        {
            print("Game is over mate, Team: " + currentTeam + " cannot move any of their units");
            Application.Quit();
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

        setUIText("Team: " + currentTeam + ", Select " + selectedUnit.getUnitInfo().unitName + "'s Movement", new Color(0.0f, 1.0f, 1.0f));
        if (selectedUnit.getUnitInfo().movementObjects.Count > 1)
        {
            setUIText2("Select Alternate Moveset(s) <Left Alt>\nDeselect <Double Right Click>", Color.cyan);
        }
        else
        {
            setUIText2("Deselect <Double Right Click>", Color.cyan);
        }


        selectedUnit.getOccupyingHex().switchState(TileState.SELECTED);

        //Temporary Alt counter switching
        for (int i = 0; i < selectedUnit.getUnitInfo().movementObjects.Count; i++)
        {
            int iAlt = altCounter + i;
            if (iAlt >= selectedUnit.getUnitInfo().movementObjects.Count)
            {
                iAlt -= selectedUnit.getUnitInfo().movementObjects.Count;
            }
            if (selectedUnit.getUnitInfo().movementObjects[i].canMove(selectedUnit, currentTeam))
            {
                altCounter = iAlt;
                break;
            }
        }
        selectedUnit.getUnitInfo().movementObjects[altCounter].startSelectingInMode(selectedUnit, currentTeam);
    }


// -- Start Selecting Unit To Rotate
    void startSelectingUnitToRotate()
    {
        selectedUnit = null;
        tileControllerScript.switchAllTileStates();

        UnitScript[] units = FindObjectsOfType<UnitScript>();

        setUIText("Team: " + currentTeam + ", Select Unit to Rotate", new Color(0.0f, 1.0f, 0.0f));
        setUIText2("Skip <Space>", Color.green);

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

        setUIText("Team: " + currentTeam + ", Select " + selectedUnit.getUnitInfo().unitName + "'s Rotation", new Color(0.0f, 1.0f, 0.0f));
        setUIText2("Skip <Space>\nDeselect <Double Right Click>", Color.green);

        selectedUnit.getOccupyingHex().switchState(TileState.SELECTED);

        List<RelativeDirection> relativeDirections = selectedUnit.getUnitInfo().relativeRotationDirections;

        Vector2 originalHexCoordPos = selectedUnit.getOccupyingHex().getCoords() + SpawnTiles.absoluteDirectionToRelativePos(selectedUnit.getRotation());
        tileControllerScript.addNewTempTile(originalHexCoordPos);

        tileControllerScript.getTileFromHexCoord(originalHexCoordPos, true).switchState(TileState.NONSELECTABLE);

        for (int i = 0; i < relativeDirections.Count; i++)
        {
            AbsoluteDirection rotationDirection = SpawnTiles.relativeToAbsoluteDirection(selectedUnit.getRotation(), relativeDirections[i]);
            
            //Creating Temp Tiles if any just incase
            Vector2 hexCoord = selectedUnit.getOccupyingHex().getCoords() + SpawnTiles.absoluteDirectionToRelativePos(rotationDirection);
            tileControllerScript.addNewTempTile(hexCoord);

            HexTile tile = tileControllerScript.getTileFromHexCoord(hexCoord, true);
            /*if (tile != null)
            {*/
                tile.switchState(TileState.MOVEABLE);
            /*}*/
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
                case InteractionStates.SelectingLordLocation:
                    selectingLordLocation(clickedTile);
                    break;

                case InteractionStates.SelectingGuardSpawnPoint:
                    selectingGuardSpawnPoint(clickedTile);
                    break;

                case InteractionStates.SelectingGuardDirection:
                    selectingGuardDirection(clickedTile);
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


// -- Selecting Lord Location
    void selectingLordLocation(HexTile clickedTile)
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

                        UnitScript unitScript = spawnUnit("Lord", clickedTile, currentTeam, false);

                        unitScript.setUnitType(UnitType.LordUnit);
                        //Set the above Character as the Lord for teamController
                        createTeamController(currentTeam, unitScript);
                        getTeamController(currentTeam).defaultDirection = tiles[i].teamSpawnDirection;

                        if (currentTeam + 1 < numberOfTeams)
                        {
                            currentTeam++;
                            switchInteractionState(InteractionStates.SelectingLordLocation);
                        }
                        else
                        {
                            currentTeam = 0;
                            switchInteractionState(InteractionStates.SelectingGuardSpawnPoint);
                        }
                    }
                }
            }
        }
    }


// -- Selecting Guard Spawn Point
    void selectingGuardSpawnPoint(HexTile clickedTile)
    {
        // Very close to Selecting Unit Spawn Point but not

        if (clickedTile.getCurrentTileState() == TileState.SPAWNABLE)
        {
            selectedUnit = spawnUnit("BasicUnit", clickedTile, currentTeam, false, getTeamController(currentTeam).defaultDirection);
            switchInteractionState(InteractionStates.SelectingGuardDirection);
            selectedUnit.justSpawned = false;
        }
    }


    // -- Selecting Guard's Direction
    void selectingGuardDirection(HexTile clickedTile)
    {
        if (clickedTile.getCurrentTileState() == TileState.SELECTED)
        {
            switchInteractionState(InteractionStates.SelectingGuardSpawnPoint);
        }
        else if (clickedTile.getCurrentTileState() == TileState.MOVEABLE)
        {
            Vector2 relPosOfClicked = clickedTile.getCoords() - selectedUnit.getOccupyingHex().getCoords();

            selectedUnit.setRotationDirection(SpawnTiles.relativePosToAbsoluteDirection(relPosOfClicked));

            switchInteractionState(InteractionStates.SelectingGuardSpawnPoint);
        }
    }


// -- Selecting Unit Spawn Point
    void selectingUnitSpawnPoint(HexTile clickedTile)
    {

        if (clickedTile.getCurrentTileState() == TileState.SPAWNABLE)
        {
            string[] tempSpawnList = 
            {
                "BasicUnit",
                "SpecialUnit",
                "KnightUnit",
                "NormalUnit",
                "RangedUnit"
            };

            selectedUnit = spawnUnit(tempSpawnList[Random.Range(0, tempSpawnList.Length)], clickedTile, currentTeam, true, getTeamController(currentTeam).defaultDirection);
            //justSpawnedUnit = selectedUnit;

            switchInteractionState(InteractionStates.SelectingSpawnedUnitDirection);
        }
    }


// -- Selecting Spawned Unit's Direction
    void selectingSpawnedUnitDirection(HexTile clickedTile)
    {
        if (clickedTile.getCurrentTileState() == TileState.SELECTED)
        {
            switchInteractionState(InteractionStates.SelectingUnitToMove);
        }

        else if (clickedTile.getCurrentTileState() == TileState.MOVEABLE)
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
            selectedUnit.getUnitInfo().movementObjects[altCounter].clickedInMode(clickedTile, selectedUnit, currentTeam);

        }
        else if (clickedTile.getOccupyingUnit())
        {
            if (selectedUnit == clickedTile.getOccupyingUnit())
            {
                switchInteractionState(InteractionStates.SelectingUnitToMove);
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
        if (clickedTile.getCurrentTileState() == TileState.MOVEABLE)
        {
            selectedUnit.setRotationDirection(SpawnTiles.relativePosToAbsoluteDirection(clickedTile.getCoords() - selectedUnit.getOccupyingHex().getCoords()));

            switchToNextTeam();
        }
    }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
    // ****** End In Interaction State ****** //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------


// -- End Selecting Guard Direction
    void endSelectingGuardDirection()
    {
        tileControllerScript.clearAllTempTiles();
    }


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