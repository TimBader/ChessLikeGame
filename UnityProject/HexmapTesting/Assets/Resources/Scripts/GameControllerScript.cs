using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    private GameObject teamControllerType;

    private InteractionStates interactionState = InteractionStates.SelectingLordLocation;
    private UnitScript selectedUnit = null;
    private bool gameInteractionEnabled = false;
    private bool uiInteractionEnabled = false;
    private uint numberOfTeams = 2;
    private int currentTeam = 0;
    private List<TeamControllerScript> teamControllers = new List<TeamControllerScript>();
    private TileController tileControllerScript = null;

    private List<InteractionIcon> currentModesMovementIcons = new List<InteractionIcon>();

    public GameObject tempMenu = null;
    //private SpriteResourceManager spriteResourceManagerScript = null;

    private Dictionary<string, UnitInfo> unitInfoDictionary = new Dictionary<string, UnitInfo>();

    private UnitScript justSpawnedUnit = null;

    public Camera mainCamera;

    private float minZoomSize = 0.5f;
    private float maxZoomSize = 6.0f;
    private float zoomRate = 2.0f;

    //private List<UnitInfo> tempUnitInfos = new List<UnitInfo>();

    private float rightClickDownTime = 0.0f;
    private float leftClickDownTime = 0.0f;
    private float rightDoubleClickTime = 0.0f;
    private float leftDoubleClickTime = 0.0f;

    private float escapeTimeCache;

    //Counter for alternating movements
    public int altCounter = 0;

    private int tempCounter = 0;

    ///Temp ui stuff
    /// //////////////////////////////////////////////////////////
    public Text tempUIText;
    public Text tempUIText2;

    private bool menuUp = false;

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


    public TileController getTileController()
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

    // Copied from MovementObjects xP
    /*public MovementIcon createMovementIcon(string spriteName, Vector2 coords, Color blendColor, int depth = 0, bool pixelCoords = false)
    {
        GameObject unitGameObject = (GameObject)Instantiate(movementIconPrefab);
        MovementIcon m = (MovementIcon)unitGameObject.GetComponent(typeof(MovementIcon));
        m.initialize(spriteName, coords, blendColor, depth, pixelCoords);
        currentModesMovementIcons.Add(m);
        return m;
    }

    public void clearAllMovementIcons()
    {
        while (currentModesMovementIcons.Count != 0)
        {
            //Delete MovementIcons
            DestroyObject(currentModesMovementIcons[0].gameObject);
            currentModesMovementIcons.RemoveAt(0);
        }
    }*/
    //

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
        teamControllerType = PrefabResourceManager.loadPrefab("TeamController");
        MovementTypeParent.setGameControllerRef(this);
        //UnitScript.gameControllerRef = this;

        TempUnitInfos.constructTempUnitInfos();


        bool foundTileController = false;
        GameObject[] gos = FindObjectsOfType<GameObject>();
        for (int i = 0; i < gos.Length; i++)
        {
            TileController spawnTilesScript = (TileController)gos[i].GetComponent(typeof(TileController));
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
        for (int i = 0; i < TempUnitInfos.getTempUnitInfos().Count; i++)
        {
            addNewUnitInfo(TempUnitInfos.getTempUnitInfos()[i]);
        }

        setUIText("Temp", Color.black);

        //Testing
        spawnUnit("BasicUnit", tileControllerScript.getTileFromHexCoord(new Vector2(0, 4)), 0, false, AbsoluteDirection.UP);
        spawnUnit("BasicUnit", tileControllerScript.getTileFromHexCoord(new Vector2(0, 5)), 1, false, AbsoluteDirection.UP);
        spawnUnit("SpecialUnit", tileControllerScript.getTileFromHexCoord(new Vector2(1, 4)), 0, false, AbsoluteDirection.UP);
        spawnUnit("SpecialUnit", tileControllerScript.getTileFromHexCoord(new Vector2(1, 5)), 1, false, AbsoluteDirection.UP);
        spawnUnit("Lord", tileControllerScript.getTileFromHexCoord(new Vector2(2, 4)), 0, false, AbsoluteDirection.UP);
        spawnUnit("Lord", tileControllerScript.getTileFromHexCoord(new Vector2(2, 5)), 1, false, AbsoluteDirection.UP);
        spawnUnit("KnightUnit", tileControllerScript.getTileFromHexCoord(new Vector2(3, 4)), 0, false, AbsoluteDirection.UP);
        spawnUnit("KnightUnit", tileControllerScript.getTileFromHexCoord(new Vector2(3, 5)), 1, false, AbsoluteDirection.UP);
        spawnUnit("RepositionUnit", tileControllerScript.getTileFromHexCoord(new Vector2(4, 4)), 0, false, AbsoluteDirection.UP);
        spawnUnit("RepositionUnit", tileControllerScript.getTileFromHexCoord(new Vector2(4, 5)), 1, false, AbsoluteDirection.UP);
        spawnUnit("SiegeUnit", tileControllerScript.getTileFromHexCoord(new Vector2(5, 4)), 0, false, AbsoluteDirection.UP);
        spawnUnit("SiegeUnit", tileControllerScript.getTileFromHexCoord(new Vector2(5, 5)), 1, false, AbsoluteDirection.UP);
        spawnUnit("RangedUnit", tileControllerScript.getTileFromHexCoord(new Vector2(6, 4)), 0, false, AbsoluteDirection.UP);
        spawnUnit("RangedUnit", tileControllerScript.getTileFromHexCoord(new Vector2(6, 5)), 1, false, AbsoluteDirection.UP);
        

        //spawnUnit("RepositionUnit", tileControllerScript.getTileFromHexCoord(new Vector2(4, 3)), 0, false, AbsoluteDirection.UP_RIGHT);
        //spawnUnit("RepositionUnit", tileControllerScript.getTileFromHexCoord(new Vector2(3, 5)), 0, false, AbsoluteDirection.UP);
        //spawnUnit("RepositionUnit", tileControllerScript.getTileFromHexCoord(new Vector2(4, 5)), 1, false, AbsoluteDirection.UP);



        uiInteractionEnabled = true;
        gameInteractionEnabled = true;

        switchInteractionState(InteractionStates.SelectingLordLocation);
    }

    public void hitExitButton()
    {
        Application.Quit();
    }


    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuUp)
            {
                tempMenu.SetActive(false);
                menuUp = false;
            }
            else
            {
                tempMenu.SetActive(true);
                menuUp = true;
            }
            /*float diff = Time.time - escapeTimeCache;

            if (diff <= 0.20f)
            {
                print("Quiting Application");
                Application.Quit();
            }

            escapeTimeCache = Time.time;*/
        }


        if (gameInteractionEnabled)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                /*if ((EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.layer != 5) || EventSystem.current.currentSelectedGameObject == null)
                {
                    //print("TITTY TWISTERS!!!");
                    Vector3 mpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    HexTile hexTile = tileControllerScript.getTileAtPixelPos(mpos, true);
                    if (hexTile != null)
                    {
                        clickedOnTile(hexTile);
                    }
                }*/
                leftClickDownTime = Time.time;
            }
            else if (Input.GetButtonUp("Fire1"))
            {
                float diff = Time.time - leftClickDownTime;
                if (diff <= 0.2)
                {
                    if ((EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.layer != 5) || EventSystem.current.currentSelectedGameObject == null)
                    {
                        //print("TITTY TWISTERS!!!");
                        Vector3 mpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        HexTile hexTile = tileControllerScript.getTileAtPixelPos(mpos, true);
                        if (hexTile != null)
                        {
                            clickedOnTile(hexTile);
                        }
                    }
                }
                float doubleClickDiff = Time.time - leftDoubleClickTime;
                if (doubleClickDiff <= 0.2)
                {

                }
                leftDoubleClickTime = Time.time;
            }
            else if (Input.GetButtonDown("Fire2"))
            {
                //float diff = Time.time - rightClickTimeCache;

                //if (diff <= 0.20)
                //{
                    /*if (interactionState == InteractionStates.SelectingUnitSpawnPoint || interactionState == InteractionStates.SelectingSpawnedUnitDirection)
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
                    }*/
                //}

                rightClickDownTime = Time.time;
            }
            else if (Input.GetButtonUp("Fire2"))
            {
                float diff = Time.time - rightClickDownTime;
                if (diff <= 0.2)
                {
                    if (interactionState == InteractionStates.SelectingUnitMovement)
                    {
                        switchInteractionState(InteractionStates.SelectingUnitToMove);
                    }

                    if (interactionState == InteractionStates.SelectingUnitRotation)
                    {
                        switchInteractionState(InteractionStates.SelectingUnitToRotate);
                    }
                }
                float doubleClickDiff = Time.time - rightDoubleClickTime;
                if (doubleClickDiff <= 0.2)
                {
                    if (interactionState == InteractionStates.SelectingUnitSpawnPoint || interactionState == InteractionStates.SelectingSpawnedUnitDirection || interactionState == InteractionStates.SelectingGuardDirection)
                    {
                        switchInteractionState(InteractionStates.SelectingUnitToMove);
                    }

                    if (interactionState == InteractionStates.SelectingUnitToRotate)
                    {
                        switchToNextTeam();
                    }

                }
                rightDoubleClickTime = Time.time;
            }
            else if (Input.GetButtonDown("Jump"))
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
                float newZoom = mainCamera.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * zoomRate;
                if (newZoom >= minZoomSize && newZoom <= maxZoomSize)

                    mainCamera.orthographicSize = newZoom;
            }

            if (Input.GetButtonDown("q"))
            {
                Vector3 mpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                print("HexCoord at Mouse Position: " + TileController.pixelCoordToHex(new Vector2(mpos.x, mpos.y)));

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
        for (int i = 0; i < units.Length; i++)
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

            case InteractionStates.SelectingLordLocation:
                endSelectingLordLocation();
                break;

            case InteractionStates.SelectingGuardSpawnPoint:
                endSelectingGuardSpawnPoint();
                break;

            case InteractionStates.SelectingUnitSpawnPoint:
                endSelectingUnitSpawnPoint();
                break;

            case InteractionStates.SelectingUnitToMove:
                endSelectingUnitToMove();
                break;

            case InteractionStates.SelectingUnitToRotate:
                endSelectingUnitToRotate();
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
                    tiles[i].switchState(TileState.SELECTABLE);
                    Color drawColor = Color.yellow;
                    AbsoluteDirection rotationDirection = tiles[i].spawnDirection;


                    InteractionIcon.createInteractionIcon("PlaceIcon", tiles[i].getCoords(), drawColor);

                    Vector2 hexCoord = tiles[i].getCoords() + TileController.absoluteDirectionToRelativePos(rotationDirection);

                    Vector2 pixCoords = TileController.hexCoordToPixelCoord(hexCoord);

                    InteractionIcon pathStartIcon = InteractionIcon.createInteractionIcon("PathStartIcon", pixCoords, drawColor, 0, true);
                    InteractionIcon arrowHead = InteractionIcon.createInteractionIcon("ArrowHeadIcon", pixCoords, drawColor, 1, true);

                    Vector2 Q = pixCoords - tiles[i].getPixelCoords();
                    float angle = Mathf.Atan2(Q.y, Q.x) * 180 / Mathf.PI;
                    pathStartIcon.transform.Rotate(Vector3.forward, angle);
                    Vector2 newRelPosition = Q.normalized * 46 / 100;

                    pathStartIcon.transform.position = pathStartIcon.transform.position - new Vector3(newRelPosition.x, newRelPosition.y, 1.0f);
                    arrowHead.transform.Rotate(Vector3.forward, angle);
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

        setUIText("Team: " + currentTeam + ", Select Guard Spawn Point", Color.yellow);
        setUIText2("", Color.yellow);

        List<HexTile> adjacent = tileControllerScript.getAdjacentTiles(getTeamController(currentTeam).getLord().getOccupyingHex());

        bool freeSpace = false;
        for (int i = 0; i < adjacent.Count; i++)
        {
            if (adjacent[i] != null)
            {
                InteractionIcon.createInteractionIcon("PlaceIcon", adjacent[i].getCoords(), Color.yellow);
                if (adjacent[i].getOccupyingUnit() == null)
                {
                    adjacent[i].switchState(TileState.SELECTABLE);
                    freeSpace = true;
                }
                else
                {
                    InteractionIcon.createInteractionIcon("CrossIcon", adjacent[i].getCoords(), Color.red, 1);
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

        Vector2 unitCoords = TileController.hexCoordToPixelCoord(selectedUnit.getCoords());

        if (selectedUnit.getUnitInfo().rotationEnabled)
        {
            print("TEAM: " + currentTeam + " --> Select Guard Unit's Rotation.  Press [SpaceBar] to skip");
            for (int i = 0; i < adjacentTiles.Count; i++)
            {
                if (adjacentTiles[i] != null)
                {
                    adjacentTiles[i].switchState(TileState.SELECTABLE);

                    //Creating Temp Tiles if any just incase
                    Vector2 pixCoords = TileController.hexCoordToPixelCoord(adjacentTiles[i].getCoords());

                    InteractionIcon pathStartIcon = InteractionIcon.createInteractionIcon("PathStartIcon", pixCoords, Color.green, 0, true);
                    InteractionIcon arrowHead = InteractionIcon.createInteractionIcon("ArrowHeadIcon", pixCoords, Color.green, 1, true);

                    Vector2 Q = pixCoords - unitCoords;
                    float angle = Mathf.Atan2(Q.y, Q.x) * 180 / Mathf.PI;
                    pathStartIcon.transform.Rotate(Vector3.forward, angle);
                    Vector2 newRelPosition = Q.normalized * 46 / 100;

                    pathStartIcon.transform.position = pathStartIcon.transform.position - new Vector3(newRelPosition.x, newRelPosition.y, 1.0f);
                    arrowHead.transform.Rotate(Vector3.forward, angle);
                    //.transform.localScale = new Vector2(dist / 24, 1.0f);
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
                InteractionIcon.createInteractionIcon("PlaceIcon", adjacent[i].getCoords(), Color.yellow, 0);
                if (adjacent[i].getOccupyingUnit() == null)
                {
                    adjacent[i].switchState(TileState.SELECTABLE);
                    freeSpace = true;
                }
                else
                {
                    InteractionIcon.createInteractionIcon("CrossIcon", adjacent[i].getCoords(), Color.red, 1);
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

        Vector2 unitCoords = TileController.hexCoordToPixelCoord(selectedUnit.getCoords());

        if (selectedUnit.getUnitInfo().rotationEnabled)
        {
            print("TEAM: " + currentTeam + " --> Select Spawned Unit's Rotation.  Press [SpaceBar] to skip");
            for (int i = 0; i < adjacentTiles.Count; i++)
            {
                if (adjacentTiles[i] != null)
                {
                    adjacentTiles[i].switchState(TileState.SELECTABLE);

                    //Creating Temp Tiles if any just incase
                    Vector2 pixCoords = TileController.hexCoordToPixelCoord(adjacentTiles[i].getCoords());

                    InteractionIcon pathStartIcon = InteractionIcon.createInteractionIcon("PathStartIcon", pixCoords, Color.green, 0, true);
                    InteractionIcon arrowHead = InteractionIcon.createInteractionIcon("ArrowHeadIcon", pixCoords, Color.green, 1, true);

                    Vector2 Q = pixCoords - unitCoords;
                    float angle = Mathf.Atan2(Q.y, Q.x) * 180 / Mathf.PI;
                    pathStartIcon.transform.Rotate(Vector3.forward, angle);
                    Vector2 newRelPosition = Q.normalized * 46 / 100;

                    pathStartIcon.transform.position = pathStartIcon.transform.position - new Vector3(newRelPosition.x, newRelPosition.y, 1.0f);
                    arrowHead.transform.Rotate(Vector3.forward, angle);
                    //.transform.localScale = new Vector2(dist / 24, 1.0f);
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
                    InteractionIcon.createInteractionIcon("SelectableIcon", allUnits[i].getCoords(), Color.cyan);
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
            //if (selectedUnit.getUnitInfo().movementObjects[i].canMove(selectedUnit, currentTeam))
            //{
            altCounter = iAlt;
            break;
            //}
        }

        if (selectedUnit.getUnitInfo().movementObjects.Count > 1)
        {
            InteractionIcon.createInteractionIcon("AlternateIcon", selectedUnit.getPixelCoords(), Color.yellow, 5, true);
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
            if (units[i].getTeam() == currentTeam && !units[i].garbage)
            {
                //print("Kitty cat was found!!!");
                if (units[i].getUnitInfo().rotationEnabled || units[i].getUnitInfo().relativeRotationDirections.Count != 0)
                {
                    foundOne = true;
                    units[i].getOccupyingHex().switchState(TileState.SELECTABLE);
                    InteractionIcon.createInteractionIcon("SelectableIcon", units[i].getCoords(), Color.green);
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

        List<RelativeDirection> tempRelDirList = selectedUnit.getUnitInfo().relativeRotationDirections;
        List<RelativeDirection> relativeDirections = new List<RelativeDirection>();
        for (int i = 0; i < tempRelDirList.Count; i++)
        {
            relativeDirections.Add(tempRelDirList[i]);
        }


        print("Count Mate: " + relativeDirections.Count); //Saftychecking
        relativeDirections.Add(RelativeDirection.FORWARD);
        //The original direction draw it
        //Vector2 originalHexCoordPos = selectedUnit.getOccupyingHex().getCoords() + SpawnTiles.absoluteDirectionToRelativePos(selectedUnit.getRotation());

        //tileControllerScript.addNewTempTile(originalHexCoordPos);

        //tileControllerScript.getTileFromHexCoord(originalHexCoordPos, true).switchState(TileState.NONSELECTABLE);

        Vector2 unitCoords = TileController.hexCoordToPixelCoord(selectedUnit.getCoords());

        for (int i = 0; i < relativeDirections.Count; i++)
        {
            Color drawColor = Color.green;
            //Since you cant rotate forward, draw it black to show original direction
            if (relativeDirections[i] == RelativeDirection.FORWARD)
            {
                drawColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
            }

            AbsoluteDirection rotationDirection = TileController.relativeToAbsoluteDirection(selectedUnit.getRotation(), relativeDirections[i]);

            //Spawning direction arrow
            Vector2 hexCoord = selectedUnit.getOccupyingHex().getCoords() + TileController.absoluteDirectionToRelativePos(rotationDirection);

            Vector2 pixCoords = TileController.hexCoordToPixelCoord(hexCoord);

            InteractionIcon pathStartIcon = InteractionIcon.createInteractionIcon("PathStartIcon", pixCoords, drawColor, 0, true);
            InteractionIcon arrowHead = InteractionIcon.createInteractionIcon("ArrowHeadIcon", pixCoords, drawColor, 1, true);

            Vector2 Q = pixCoords - unitCoords;
            float angle = Mathf.Atan2(Q.y, Q.x) * 180 / Mathf.PI;
            pathStartIcon.transform.Rotate(Vector3.forward, angle);
            Vector2 newRelPosition = Q.normalized * 46 / 100;

            pathStartIcon.transform.position = pathStartIcon.transform.position - new Vector3(newRelPosition.x, newRelPosition.y, 1.0f);
            arrowHead.transform.Rotate(Vector3.forward, angle);
            //.transform.localScale = new Vector2(dist / 24, 1.0f);

            //Creating Temp Tiles if any just incase
            tileControllerScript.addNewTempTile(hexCoord);

            HexTile tile = tileControllerScript.getTileFromHexCoord(hexCoord, true);
            if (relativeDirections[i] != RelativeDirection.FORWARD)
            {
                tile.switchState(TileState.SELECTABLE);
            }
        }

        //tileControllerScript.getTileFromHexCoord(originalHexCoordPos, true).switchState(TileState.NONSELECTABLE);/

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
                        unitScript.setRotationDirection(tiles[i].spawnDirection);

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

        if (clickedTile.getCurrentTileState() == TileState.SELECTABLE)
        {
            selectedUnit = spawnUnit("BasicUnit", clickedTile, currentTeam, false, getTeamController(currentTeam).getLord().getRotation());
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
        else if (clickedTile.getCurrentTileState() == TileState.SELECTABLE)
        {
            Vector2 relPosOfClicked = clickedTile.getCoords() - selectedUnit.getOccupyingHex().getCoords();

            selectedUnit.setRotationDirection(TileController.relativePosToAbsoluteDirection(relPosOfClicked));

            switchInteractionState(InteractionStates.SelectingGuardSpawnPoint);
        }
    }


    // -- Selecting Unit Spawn Point
    void selectingUnitSpawnPoint(HexTile clickedTile)
    {

        if (clickedTile.getCurrentTileState() == TileState.SELECTABLE)
        {
            string[] tempSpawnList = 
            {
                "BasicUnit",
                "SpecialUnit",
                "KnightUnit",
                "SiegeUnit",
                "RangedUnit",
                "RepositionUnit",
                "BasicUnit2"
            };

            selectedUnit = spawnUnit(tempSpawnList[Random.Range(0, tempSpawnList.Length)], clickedTile, currentTeam, true, getTeamController(currentTeam).getLord().getRotation());
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

        else if (clickedTile.getCurrentTileState() == TileState.SELECTABLE)
        {
            Vector2 relPosOfClicked = clickedTile.getCoords() - selectedUnit.getOccupyingHex().getCoords();

            selectedUnit.setRotationDirection(TileController.relativePosToAbsoluteDirection(relPosOfClicked));

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
        if (clickedTile.getCurrentTileState() == TileState.SELECTED)
        {
            if (selectedUnit == clickedTile.getOccupyingUnit())
            {
                //switchInteractionState(InteractionStates.SelectingUnitToMove);
                altCounter++;
                if (altCounter >= selectedUnit.getUnitInfo().movementObjects.Count)
                {
                    altCounter = 0;
                    switchInteractionState(InteractionStates.SelectingUnitToMove);
                    return;
                }

                switchInteractionState(InteractionStates.SelectingUnitMovement);
                return;
            }
        }
        else if (clickedTile.getCurrentTileState() == TileState.SELECTABLE)
        {
            selectedUnit.getUnitInfo().movementObjects[altCounter].clickedInMode(clickedTile, selectedUnit, currentTeam);
        }
        else if (clickedTile.getCurrentTileState() == TileState.NONE)
        {
            if (clickedTile.getOccupyingUnit() && clickedTile.getOccupyingUnitTeam() == currentTeam)
            {
                if (clickedTile.getOccupyingUnit().justSpawned)
                {
                    selectedUnit = null;
                    switchInteractionState(InteractionStates.SelectingUnitToMove);
                }
                else
                {
                    selectedUnit = clickedTile.getOccupyingUnit();
                    switchInteractionState(InteractionStates.SelectingUnitMovement);
                }
            }
        }
    }


    // -- Selecting Unit To Rotate
    void selectingUnitToRotate(HexTile clickedTile)
    {
        if (clickedTile.getCurrentTileState() == TileState.SELECTABLE)
        {
            selectedUnit = clickedTile.getOccupyingUnit();

            switchInteractionState(InteractionStates.SelectingUnitRotation);
        }
    }


    // -- Selecting Unit Rotation
    void selectingUnitRotation(HexTile clickedTile)
    {
        if (clickedTile.getCurrentTileState() == TileState.SELECTED)
        {
            switchInteractionState(InteractionStates.SelectingUnitToRotate);
        }
        else if (clickedTile.getCurrentTileState() == TileState.SELECTABLE)
        {
            selectedUnit.setRotationDirection(TileController.relativePosToAbsoluteDirection(clickedTile.getCoords() - selectedUnit.getOccupyingHex().getCoords()));

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
        InteractionIcon.clearAllInteractionIcons();
    }


    // -- End Selecting Spawned Unit Direction
    void endSelectingSpawnedUnitDirection()
    {
        tileControllerScript.clearAllTempTiles();
        InteractionIcon.clearAllInteractionIcons();
    }


    // -- End Selecting Unit Rotation
    void endSelectingUnitRotation()
    {
        tileControllerScript.clearAllTempTiles();
        InteractionIcon.clearAllInteractionIcons();
    }


    // -- End Selecting Unit Movement
    void endSelectingUnitMovement(InteractionStates nextInteractionState)
    {
        if (nextInteractionState != InteractionStates.SelectingUnitMovement)
        {
            altCounter = 0;
        }
        InteractionIcon.clearAllInteractionIcons();
    }

    // -- End Selecting Lord Location
    void endSelectingLordLocation()
    {
        InteractionIcon.clearAllInteractionIcons();
    }

    // -- End Selecting Guard Spawn Point
    void endSelectingGuardSpawnPoint()
    {
        InteractionIcon.clearAllInteractionIcons();
    }

    // -- End Selecting Unit Spawn Point
    void endSelectingUnitSpawnPoint()
    {
        InteractionIcon.clearAllInteractionIcons();
    }

    // -- End Selecting Unit To Move
    void endSelectingUnitToMove()
    {
        InteractionIcon.clearAllInteractionIcons();
    }

    // -- End Selecting Unit To Rotate
    void endSelectingUnitToRotate()
    {
        InteractionIcon.clearAllInteractionIcons();
    }

    // ----------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------//
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------------

};