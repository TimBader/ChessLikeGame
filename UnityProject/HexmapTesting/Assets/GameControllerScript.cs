using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Helps store information usefully for hex-tile rotations
public class RotationDirectionObject
{
    public static RotationDirectionObject UP = new RotationDirectionObject(new Vector2(0, 1), new Vector2(1, 0));
    public static RotationDirectionObject UP_RIGHT = new RotationDirectionObject(new Vector2(1, 0), new Vector2(1, -1));
    public static RotationDirectionObject DOWN_RIGHT = new RotationDirectionObject(new Vector2(1, -1), new Vector2(0, -1));
    public static RotationDirectionObject DOWN = new RotationDirectionObject(new Vector2(0, -1), new Vector2(-1, 0));
    public static RotationDirectionObject DOWN_LEFT = new RotationDirectionObject(new Vector2(-1, 0), new Vector2(-1, 1));
    public static RotationDirectionObject UP_LEFT = new RotationDirectionObject(new Vector2(-1, 1), new Vector2(0, 1));
    
    private Vector2 upDirection;
    private Vector2 rightDirection;

    private RotationDirectionObject(Vector2 upDir, Vector2 rightDir)
    {
        upDirection = upDir;
        rightDirection = rightDir;
    }

    public Vector2 getUpDirection()
    {
        return upDirection;
    }

    public Vector2 getRightDirection()
    {
        return rightDirection;
    }
};

//Relative
public enum RelativeDirection
{
    FORWARD,
    FORWARD_RIGHT,
    BACKWARD_RIGHT,
    BACKWARD,
    BACKWARD_LEFT,
    FORWARD_LEFT
};

//Absolute
public enum AbsoluteDirection
{
    UP,
    UP_RIGHT,
    DOWN_RIGHT,
    DOWN,
    DOWN_LEFT,
    UP_LEFT
};

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


public enum MovementTypes
{
    Jump
};


public class MovementTypeParent
{
    //This is just to serve as a parent so we can stored all childern of this parent in one place... so far I cant think of anything MovementTypeParent should have
};


public class JumpMoveType : MovementTypeParent
{
    public List<Vector2> jumpPositions = new List<Vector2>();

    public void initialize(List<Vector2> jumpMovementPositions)
    {
        jumpPositions = jumpMovementPositions;
    }
};


public class SlideMoveType : MovementTypeParent
{
    public List<Vector2> directions = new List<Vector2>();
    public int range = -1; //-1 for infinite range

    public void initialize(List<Vector2> slideDirections, int slideRange = -1)
    {
        directions = slideDirections;
        range = slideRange;
    }
}

public struct UnitInfo
{
    public string unitName;
    public string baseSpriteName;
    public string colorsSpriteName;
    public MovementTypes movementType;
    public MovementTypeParent movementObject;
    public List<RelativeDirection> relativeRotationDirections;
    public bool rotationEnabled;
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

    private List<UnitInfo> tempUnitInfos = new List<UnitInfo>();

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

        UnitInfo newUnitInfo = new UnitInfo();

        newUnitInfo.unitName = "King";
        newUnitInfo.baseSpriteName = "King_Base";
        newUnitInfo.colorsSpriteName = "King_Colors";
        List<Vector2> jumpPositions = new List<Vector2>();
        jumpPositions.Add(new Vector2(0, -1));
        jumpPositions.Add(new Vector2(1, -1));
        jumpPositions.Add(new Vector2(1, 0));
        jumpPositions.Add(new Vector2(0, 1));
        jumpPositions.Add(new Vector2(-1, 1));
        jumpPositions.Add(new Vector2(-1, 0));
        List<RelativeDirection> relativeRotations = new List<RelativeDirection>();
        newUnitInfo.relativeRotationDirections = relativeRotations;
        JumpMoveType jumpMove = new JumpMoveType();
        jumpMove.initialize(jumpPositions);
        newUnitInfo.movementObject = jumpMove;
        newUnitInfo.rotationEnabled = false;
        tempUnitInfos.Add(newUnitInfo);

        newUnitInfo.unitName = "BasicUnit";
        newUnitInfo.baseSpriteName = "BasicUnit_Base";
        newUnitInfo.colorsSpriteName = "BasicUnit_Colors";
        jumpPositions = new List<Vector2>();
        //jumpPositions.Add(new Vector2(2, -1));
        jumpPositions.Add(new Vector2(0, 2));
        //jumpPositions.Add(new Vector2(0, -2));
        //jumpPositions.Add(new Vector2(-2, 1));
        relativeRotations = new List<RelativeDirection>();
        relativeRotations.Add(RelativeDirection.FORWARD_RIGHT);
        relativeRotations.Add(RelativeDirection.FORWARD_LEFT);
        newUnitInfo.relativeRotationDirections = relativeRotations;
        jumpMove = new JumpMoveType();
        jumpMove.initialize(jumpPositions);
        newUnitInfo.movementObject = jumpMove;
        newUnitInfo.rotationEnabled = true;
        tempUnitInfos.Add(newUnitInfo);

        //newUnitInfo.unitName = "SpecialUnit";

        newUnitInfo.unitName = "SpecialUnit";
        newUnitInfo.baseSpriteName = "SpecialUnit_Base";
        newUnitInfo.colorsSpriteName = "SpecialUnit_Colors";
        jumpPositions = new List<Vector2>();
        jumpPositions.Add(new Vector2(0, 1));
        jumpPositions.Add(new Vector2(0, 2));/*
        jumpPositions.Add(new Vector2(-1, 0));
        jumpPositions.Add(new Vector2(-2, 0));
        jumpPositions.Add(new Vector2(1, -1));
        jumpPositions.Add(new Vector2(2, -2));
        jumpPositions.Add(new Vector2(-1, 1));
        jumpPositions.Add(new Vector2(-2, 2));*/
        relativeRotations = new List<RelativeDirection>();
        relativeRotations.Add(RelativeDirection.FORWARD_RIGHT);
        relativeRotations.Add(RelativeDirection.FORWARD_LEFT);
        relativeRotations.Add(RelativeDirection.BACKWARD_LEFT);
        relativeRotations.Add(RelativeDirection.BACKWARD_RIGHT);
        relativeRotations.Add(RelativeDirection.BACKWARD);
        newUnitInfo.relativeRotationDirections = relativeRotations;
        jumpMove = new JumpMoveType();
        jumpMove.initialize(jumpPositions);
        newUnitInfo.movementObject = jumpMove;
        newUnitInfo.rotationEnabled = true;
        tempUnitInfos.Add(newUnitInfo);

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
        for (int i = 0; i < tempUnitInfos.Count; i++ )
        {
            addNewUnitInfo(tempUnitInfos[i]);
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

    void switchInteractionState(InteractionStates state)
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

        switch (selectedUnit.getUnitInfo().movementType)
        {
            case MovementTypes.Jump:

                List<Vector2> jumpPositions = ((JumpMoveType)selectedUnit.getUnitInfo().movementObject).jumpPositions;


                List<Vector2> adjustedJumpPositions = new List<Vector2>();
                for (int i = 0; i < jumpPositions.Count; i++ )
                {
                    adjustedJumpPositions.Add(rotate(jumpPositions[i], selectedUnit.getRotation()));
                }


                // !!! Will need to account for ROTATION !!!

                for (int i = 0; i < adjustedJumpPositions.Count; i++)
                {
                    if (adjustedJumpPositions[i].x == 0 && adjustedJumpPositions[i].y == 0)
                    {
                        print("Warning!  Included a jump position that is the same location as the unit relative hex coord of (0,0) ignoring, please remove for optimization");
                        continue;
                    }

                    HexTile tile = tileControllerScript.getTileFromHexCoord(adjustedJumpPositions[i] + selectedUnit.getOccupyingHex().getCoords());

                    if (tile != null)
                    {
                        if (tile.getOccupyingUnit() != null)
                        {
                            if (tile.getOccupyingUnit().getTeam() != currentTeam)
                            {
                                tile.switchState(TileState.ATTACKABLE);
                            }
                        }
                        else
                        {
                            tile.switchState(TileState.SELECTABLE);
                        }
                    }
                }
                break;
        }
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
            AbsoluteDirection rotationDirection = relativeToAbsoluteDirection(selectedUnit.getRotation(), relativeDirections[i]);
            
            //Creating Temp Tiles if any just incase
            Vector2 hexCoord = selectedUnit.getOccupyingHex().getCoords() + rotationDirectionToRelativePos(rotationDirection);
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
                        unitScript.getBaseSpriteRenderer().sprite = spriteResourceManagerScript.loadSprite(newKingInfo.baseSpriteName);
                        unitScript.getColorsSpriteRenderer().sprite = spriteResourceManagerScript.loadSprite(newKingInfo.colorsSpriteName);
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
                            "SpecialUnit"
                        };

                        GameObject spawningUnit = (GameObject)Instantiate(tileControllerScript.spawnUnitType, tileControllerScript.hexCoordToPixelCoord(clickedTile.getCoords()), Quaternion.identity);
                        UnitScript unitScript = (UnitScript)spawningUnit.GetComponent(typeof(UnitScript));
                        clickedTile.setOccupyingUnit(unitScript);
                        unitScript.setOccupyingHex(clickedTile);

                        UnitInfo newUnitInfo = getUnitInfo(tempSpawnList[Random.Range(0, tempSpawnList.Length)]);

                        unitScript.initialize(newUnitInfo);

                        unitScript.getBaseSpriteRenderer().sprite = spriteResourceManagerScript.loadSprite(newUnitInfo.baseSpriteName);
                        unitScript.getColorsSpriteRenderer().sprite = spriteResourceManagerScript.loadSprite(newUnitInfo.colorsSpriteName);
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

            selectedUnit.setRotationDirection(relativePosToAbsoluteDirection(relPosOfClicked));

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
        UnitInfo selectedUnitInfo = selectedUnit.getUnitInfo();
        MovementTypeParent movementObject = selectedUnitInfo.movementObject;
        if (clickedTile.getCurrentTileState() == TileState.SELECTABLE || clickedTile.getCurrentTileState() == TileState.ATTACKABLE)
        {
            switch (selectedUnitInfo.movementType)
            {
                case MovementTypes.Jump:

                    if (clickedTile.getCurrentTileState() == TileState.ATTACKABLE)
                    {
                        if (clickedTile.getOccupyingUnit().getUnitType() == UnitType.KingUnit)
                        {
                            clickedTile.getOccupyingUnit().destroyUnit();
                            clickedTile.setOccupyingUnit(null);
                            print("King Destroyed");
                            Application.Quit();
                        }
                        else
                        {
                            clickedTile.getOccupyingUnit().destroyUnit();
                            clickedTile.setOccupyingUnit(null);
                            print("Unit Destroyed");
                        }
                    }

                    tileControllerScript.transferUnit(selectedUnit.getOccupyingHex(), clickedTile);
                    selectedUnit.transform.position = tileControllerScript.hexCoordToPixelCoord(clickedTile.getCoords());
                    //switchToNextTeam();

                    switchInteractionState(InteractionStates.SelectingUnitToRotate);
                    
                    break;
            }
        }
        else if (clickedTile.getOccupyingUnit() != null)
        {
            if (clickedTile.getOccupyingUnit().getTeam() == currentTeam)
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
        if (clickedTile.getCurrentTileState() == TileState.MOVEABLE)
        {
            selectedUnit.setRotationDirection(relativePosToAbsoluteDirection(clickedTile.getCoords() - selectedUnit.getOccupyingHex().getCoords()));

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

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------//
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------






    AbsoluteDirection relativeToAbsoluteDirection(AbsoluteDirection currentDirection, RelativeDirection relativeDirection)
    {
        int currentDirIdx = (int)currentDirection;
        int relativeDirIdx = (int)relativeDirection;

        currentDirIdx += relativeDirIdx;

        if (currentDirIdx >= 6)
        {
            currentDirIdx -= 6;
        }

        return (AbsoluteDirection)(currentDirIdx);
    }



    /*AbsoluteDirection brelativeDirectionToRotationDirection(AbsoluteDirection currentDirection, RelativeDirection relativeDesinationDirection)
    {
        AbsoluteDirection[] rotationsList = 
        {
            AbsoluteDirection.UP,
            AbsoluteDirection.UP_RIGHT,
            AbsoluteDirection.DOWN_RIGHT,
            AbsoluteDirection.DOWN,
            AbsoluteDirection.DOWN_LEFT,
            AbsoluteDirection.UP_LEFT
        };

        int listPosDisplacement = 0;
        
        switch (relativeDesinationDirection)
        {
            case RelativeDirection.FORWARD_RIGHT:
                listPosDisplacement = 1;
                break;
            
            case RelativeDirection.BACKWARD_RIGHT:
                listPosDisplacement = 2;
                break;

            case RelativeDirection.BACKWARD:
                listPosDisplacement = 3;
                break;

            case RelativeDirection.BACKWARD_LEFT:
                listPosDisplacement = 4;
                break;

            case RelativeDirection.FORWARD_LEFT:
                listPosDisplacement = 5;
                break;
        }

        int initialListPosition = 0;
        //Need to find list position of currentDirection
        for (int i = 0; i < rotationsList.Length; i++)
        {
            if (rotationsList[i] == currentDirection)
            {
                initialListPosition = i;
                break;
            }
        }

        listPosDisplacement += initialListPosition;
        
        if (listPosDisplacement >= 6)
        {
            listPosDisplacement -= 6;
        }

        return rotationsList[listPosDisplacement];

    }*/



    AbsoluteDirection relativePosToAbsoluteDirection(Vector2 relativePosition)
    {
        if (relativePosition.x == 0)
        {
            if (relativePosition.y == 1)
            {
                return AbsoluteDirection.UP;
            }
            if (relativePosition.y == -1)
            {
                return AbsoluteDirection.DOWN;
            }
        }
        else if (relativePosition.x == 1)
        {
            if (relativePosition.y == 0)
            {
                return AbsoluteDirection.UP_RIGHT;
            }
            if (relativePosition.y == -1)
            {
                return AbsoluteDirection.DOWN_RIGHT;
            }
        }
        else if (relativePosition.x == -1)
        {
            if (relativePosition.y == 0)
            {
                return AbsoluteDirection.DOWN_LEFT;
            }
            if (relativePosition.y == 1)
            {
                return AbsoluteDirection.UP_LEFT;
            }
        }
        throw new UnityException("hex coord: (" + relativePosition + ") is not an adjacent/relative coord");
    }


    Vector2 rotationDirectionToRelativePos(AbsoluteDirection rot)
    {
        return rotationDirectionToObject(rot).getUpDirection();
    }


    RotationDirectionObject rotationDirectionToObject(AbsoluteDirection rot)
    {
        switch (rot)
        {
            case AbsoluteDirection.UP:
                return RotationDirectionObject.UP;

            case AbsoluteDirection.UP_RIGHT:
                return RotationDirectionObject.UP_RIGHT;

            case AbsoluteDirection.DOWN_RIGHT:
                return RotationDirectionObject.DOWN_RIGHT;

            case AbsoluteDirection.DOWN:
                return RotationDirectionObject.DOWN;

            case AbsoluteDirection.DOWN_LEFT:
                return RotationDirectionObject.DOWN_LEFT;

            case AbsoluteDirection.UP_LEFT:
                return RotationDirectionObject.UP_LEFT;
        }

        return null;
    }


    //Note pass in the value for units as the original positions of the tile and not a previously rotated version
    Vector2 rotate(Vector2 originalRelativeLocation, AbsoluteDirection rotationTo)
    {
        RotationDirectionObject rotDirObj = rotationDirectionToObject(rotationTo);

        return rotDirObj.getUpDirection() * originalRelativeLocation.y + rotDirObj.getRightDirection() * originalRelativeLocation.x;
    }


};