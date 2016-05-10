using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*public struct UnitInfo
{
    public string unitName;
    public string baseSpriteName;
    public string color0SpriteName;
    public string color1SpriteName;
    public string color2SpriteName;
    public string color3SpriteName;
    public List<MovementTypeParent> movementObjects;
    public List<RelativeDirection> relativeRotationDirections;
    public bool rotationEnabled;
};*/


public class UnitInfo
{
    public string unitName;
    public string baseSpriteName;
    public string color0SpriteName;
    public string color1SpriteName;
    public string color2SpriteName;
    public string color3SpriteName;
    public List<MovementTypeParent> movementObjects = new List<MovementTypeParent>();
    public List<RelativeDirection> relativeRotationDirections = new List<RelativeDirection>();
    public bool rotationEnabled;

    public UnitInfo clone()
    {
        UnitInfo ui = new UnitInfo();
        ui.unitName = unitName;
        ui.baseSpriteName = color0SpriteName;
        ui.color0SpriteName = color1SpriteName;
        ui.color1SpriteName = color2SpriteName;
        ui.color2SpriteName = color3SpriteName;
        for (int i = 0; i < movementObjects.Count; i++)
        {
            ui.movementObjects.Add(movementObjects[i].clone());
        }
        ui.relativeRotationDirections = relativeRotationDirections;
        ui.rotationEnabled = rotationEnabled;

        return ui;
    }
};


public class TempUnitInfos
{
    private static List<UnitInfo> tempUnitInfos = new List<UnitInfo>();

    private static Vector2 vec(int x, int y)
    {
        return new Vector2(x, y);
    }

    private static PathMoveType.PathPos pathPos(Vector2 pos, bool moveable)
    {
        return new PathMoveType.PathPos(pos, moveable);
    }

    public static void constructTempUnitInfos()
    {

        ////////////////
        // Lord
        ////////////////
        UnitInfo LordInfo = new UnitInfo();
        LordInfo.unitName = "Lord";
        /*LordInfo.baseSpriteName = "Lord_Base";
        LordInfo.color0SpriteName = "Lord_Colors";*/
        LordInfo.baseSpriteName = "Cory_Lord";
        LordInfo.color0SpriteName = "Cory_Lord_Colors";
        /*LordInfo.baseSpriteName = "TestUnit_Base";
        LordInfo.color0SpriteName = "TestUnit_Color0";
        LordInfo.color1SpriteName = "TestUnit_Color1";
        LordInfo.color2SpriteName = "TestUnit_Color2";
        LordInfo.color3SpriteName = "TestUnit_Color3";*/

        LordInfo.movementObjects = Util.toList(new MovementTypeParent[]
        {
            new JumpMoveType
            (
                new Vector2[]
                { 
                    vec(0,1), vec(1,-1),  vec(-1,0)
                },
                true //Verify Move
            )
        });
        LordInfo.rotationEnabled = true;
        LordInfo.relativeRotationDirections = Util.toList(new RelativeDirection[]
        {
            RelativeDirection.FORWARD_RIGHT, RelativeDirection.FORWARD_LEFT, RelativeDirection.BACKWARD_LEFT, RelativeDirection.BACKWARD_RIGHT, RelativeDirection.BACKWARD
        });
        tempUnitInfos.Add(LordInfo);



        ////////////////
        // Basic Unit
        ////////////////
        UnitInfo basicUnitInfo = new UnitInfo();
        basicUnitInfo.unitName = "BasicUnit";
        basicUnitInfo.baseSpriteName = "Axel_Swordsman";
        basicUnitInfo.color0SpriteName = "Axel_Swordsman_Colors";
        basicUnitInfo.movementObjects = Util.toList(new MovementTypeParent[] 
        {
            new PathMoveType
            (
                new PathMoveType.PathPos[][]
                {
                    new PathMoveType.PathPos[]
                    {
                        pathPos( vec(0, 1), false),
                        pathPos( vec(0, 2), true)
                    }
                },
                true //Verify Move
            ),
            new JumpMoveType
            (
                new Vector2[]
                {
                    vec(1,0), vec(-1,1)
                },
                true //Verify Move
            )
        });
        basicUnitInfo.rotationEnabled = true;
        basicUnitInfo.relativeRotationDirections = Util.toList(new RelativeDirection[]
        {
            RelativeDirection.FORWARD_RIGHT, RelativeDirection.FORWARD_LEFT, RelativeDirection.BACKWARD_LEFT, RelativeDirection.BACKWARD_RIGHT, RelativeDirection.BACKWARD
        });
        tempUnitInfos.Add(basicUnitInfo);
        ////////////////
        // Basic Unit2
        ////////////////
        UnitInfo basicUnit2Info = new UnitInfo();
        basicUnit2Info.unitName = "BasicUnit2";
        basicUnit2Info.baseSpriteName = "Axel_Spearman";
        basicUnit2Info.color0SpriteName = "Axel_Spearman_Colors";
        basicUnit2Info.movementObjects = Util.toList(new MovementTypeParent[]
        {
            new PathMoveType
            (
                new PathMoveType.PathPos[][]
                {
                    new PathMoveType.PathPos[]
                    {
                        pathPos( vec(-1,1), false),
                        pathPos( vec(-1, 2), true)
                    },                    
                    new PathMoveType.PathPos[]
                    {
                        pathPos( vec(1,0), false),
                        pathPos( vec(1, 1), true)
                    }
                },
                true //Verify Move
            ),
            new JumpMoveType
            (
                new Vector2[]
                {
                    vec(0,1)
                },
                true //Verify Move
            )
        });
        basicUnit2Info.rotationEnabled = true;
        basicUnit2Info.relativeRotationDirections = Util.toList(new RelativeDirection[]
        {
            RelativeDirection.FORWARD_RIGHT, RelativeDirection.FORWARD_LEFT, RelativeDirection.BACKWARD_LEFT, RelativeDirection.BACKWARD_RIGHT, RelativeDirection.BACKWARD
        });
        tempUnitInfos.Add(basicUnit2Info);

        ////////////////
        //Ranged Unit
        ////////////////
        UnitInfo rangedUnitInfo = new UnitInfo();
        rangedUnitInfo.unitName = "RangedUnit";
        rangedUnitInfo.baseSpriteName = "Axel_Archer_Base";
        rangedUnitInfo.color0SpriteName = "Axel_Archer_Colors";
        rangedUnitInfo.movementObjects = Util.toList(new MovementTypeParent[] 
        { 
            new RangedMoveType
            (
                new Vector2[][]
                { 
                    new Vector2[]
                    {
                        vec(0, 2)
                    },
                },
                true //Verify Move
            ), 
            new JumpMoveType
            (
                new Vector2[]
                { 
                    vec(0, 1), vec(-1,0)
                },
                true //Verify Move
            )
        });
        rangedUnitInfo.rotationEnabled = true;
        rangedUnitInfo.relativeRotationDirections = Util.toList(new RelativeDirection[]
        { 
            RelativeDirection.FORWARD_RIGHT, RelativeDirection.FORWARD_LEFT, RelativeDirection.BACKWARD_LEFT, RelativeDirection.BACKWARD_RIGHT, RelativeDirection.BACKWARD
        });
        tempUnitInfos.Add(rangedUnitInfo);


        ////////////////
        //Special Unit
        ////////////////
        UnitInfo specialUnitInfo = new UnitInfo();
        specialUnitInfo.unitName = "SpecialUnit";
        specialUnitInfo.baseSpriteName = "Axel_Theif";
        specialUnitInfo.color0SpriteName = "Axel_Theif_Colors";
        specialUnitInfo.movementObjects = Util.toList(new MovementTypeParent[] 
        {
            new JumpMoveType
            (
                new Vector2[]
                {
                    vec(-2,3), vec(2,1), vec(-1,-1), vec(1,-2)
                },
                true //Verify Move
            )
        });
        specialUnitInfo.rotationEnabled = true;
        specialUnitInfo.relativeRotationDirections = Util.toList(new RelativeDirection[] 
        { 
            RelativeDirection.FORWARD_RIGHT, RelativeDirection.FORWARD_LEFT, RelativeDirection.BACKWARD_LEFT, RelativeDirection.BACKWARD_RIGHT, RelativeDirection.BACKWARD 
        });
        tempUnitInfos.Add(specialUnitInfo);


        ////////////////
        //Knight Unit
        ////////////////
        UnitInfo knightUnitInfo = new UnitInfo();
        knightUnitInfo.unitName = "KnightUnit";
        knightUnitInfo.baseSpriteName = "knightupdate";
        //knightUnitInfo.baseSpriteName = "Knight_Base";
        //knightUnitInfo.color0SpriteName = "Knight_Colors";
        knightUnitInfo.movementObjects = Util.toList(new MovementTypeParent[] 
        { 
            new ChargeMoveType
            (
                new RelativeDirection[] 
                { 
                    RelativeDirection.FORWARD
                    //RotationDirectionObject.UP.getUpDirection()
                   //RotationDirectionObject.DOWN.getUpDirection()//Testing stuff
                }, 
                new int[] 
                { 
                    4
                    //5
                }, 
                new uint[] 
                { 
                    1
                    //1
                },
                true //Verify Move
            ),
            new JumpMoveType
            (
                new Vector2[]
                {
                    vec(1,0), vec(-1,1)
                },
                true //Verify Move
            )
        });
        knightUnitInfo.rotationEnabled = true;
        knightUnitInfo.relativeRotationDirections = Util.toList(new RelativeDirection[]
        { 
            RelativeDirection.FORWARD_LEFT, RelativeDirection.FORWARD_RIGHT 
        });
        tempUnitInfos.Add(knightUnitInfo);


        ////////////////
        //Siege Unit
        ////////////////
        UnitInfo siegeUnitInfo = new UnitInfo();
        siegeUnitInfo.unitName = "SiegeUnit";
        siegeUnitInfo.baseSpriteName = "Cory_Catapult";//Pentagon_Colors
        siegeUnitInfo.color0SpriteName = "Cory_Catapult_Colors";//Pentagon_Colors
        siegeUnitInfo.movementObjects = Util.toList(new MovementTypeParent[] 
        {
            new RangedMoveType
            (
                new Vector2[][]
                {
                    new Vector2[]
                    {
                        vec(0, 2),vec(1,2),vec(-1,2)
                    },
                },
                true //Verify Move
            ),
            
        });
        siegeUnitInfo.rotationEnabled = true;
        siegeUnitInfo.relativeRotationDirections = Util.toList(new RelativeDirection[] 
        { 
            RelativeDirection.FORWARD_RIGHT, RelativeDirection.FORWARD_LEFT
        });
        tempUnitInfos.Add(siegeUnitInfo);



        ////////////////
        //Reposition Unit
        ////////////////
        UnitInfo repositionUnitInfo = new UnitInfo();
        repositionUnitInfo.unitName = "RepositionUnit";
        repositionUnitInfo.baseSpriteName = "Cory_Commander";//Pentagon_Colors
        repositionUnitInfo.color0SpriteName = "Cory_Commander_Colors";//Pentagon_Colors
        repositionUnitInfo.movementObjects = Util.toList(new MovementTypeParent[] 
        { 
            new RepositionMoveType
            (
                new Vector2[]
                {
                    vec(1,0), vec(0,1), vec(1,-1), vec(0,-1), vec(-1,0), vec(-1,1)
                },
                new Vector2[]
                {
                    vec(-1,1), vec(0,1), vec(1,0)
                }/*,// This is something that we may do, it just requires some more effort to implement
                new RelativeDirection[][]
                {
                    new RelativeDirection[]
                    {
                        RelativeDirection.FORWARD, RelativeDirection.FORWARD_RIGHT, RelativeDirection.FORWARD_RIGHT
                    },
                    new RelativeDirection[]
                    {
                        RelativeDirection.FORWARD, RelativeDirection.FORWARD_RIGHT, RelativeDirection.FORWARD_RIGHT
                    },
                    new RelativeDirection[]
                    {
                        RelativeDirection.FORWARD, RelativeDirection.FORWARD_RIGHT, RelativeDirection.FORWARD_RIGHT
                    }
                }*/,
                true //Verify Move
            ),
            new JumpMoveType
            (
                new Vector2[]
                {
                     vec(0,1), vec(1,-1),  vec(-1,0)
                },
                true //Verify Move
            )
        });
        repositionUnitInfo.rotationEnabled = true;
        repositionUnitInfo.relativeRotationDirections = Util.toList(new RelativeDirection[] 
        { 
            RelativeDirection.FORWARD_RIGHT, RelativeDirection.FORWARD_LEFT, RelativeDirection.BACKWARD_LEFT, RelativeDirection.BACKWARD_RIGHT 
        });
        tempUnitInfos.Add(repositionUnitInfo);


    }

    public static List<UnitInfo> getTempUnitInfos()
    {
        return tempUnitInfos;
    }
}

/// Old Normal Unit Cached version

/*new PathMoveType
(
    new Vector2[][] 
    { 
        new Vector2[] 
        { 
            vec(1, 2) 
        }, 
        new Vector2[] 
        { 
            vec(-1, 3) 
        }
    }, 
    new Vector2[][] 
    { 
        new Vector2[] 
        { 
            vec(1, 0), vec(1, 1) 
        }, 
        new Vector2[] 
        { 
            vec(-1, 1), vec(-1, 2) 
        } 
    }
), 
new JumpMoveType
(
    new Vector2[] 
    { 
        vec(0, 1), vec(0, -1) 
    }
) */