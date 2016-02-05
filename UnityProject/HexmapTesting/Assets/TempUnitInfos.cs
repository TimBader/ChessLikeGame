using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct UnitInfo
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
        // King
        ////////////////
        UnitInfo LordInfo = new UnitInfo();
        LordInfo.unitName = "Lord";
        /*LordInfo.baseSpriteName = "Lord_Base";
        LordInfo.color0SpriteName = "Lord_Colors";*/
        LordInfo.baseSpriteName = "TestUnit_Base";
        LordInfo.color0SpriteName = "TestUnit_Color0";
        LordInfo.color1SpriteName = "TestUnit_Color1";
        LordInfo.color2SpriteName = "TestUnit_Color2";
        LordInfo.color3SpriteName = "TestUnit_Color3";

        LordInfo.movementObjects = Util.toList(new MovementTypeParent[]
        {
            new JumpMoveType
            (
                new Vector2[]
                { 
                    vec(0,-1), vec(1,-1), vec(1,0), vec(0,1), vec(-1,1), vec(-1,0)
                }
            )
        });
        LordInfo.rotationEnabled = false;
        LordInfo.relativeRotationDirections = new List<RelativeDirection>();
        tempUnitInfos.Add(LordInfo);



        ////////////////
        // Basic Unit
        ////////////////
        UnitInfo basicUnitInfo = new UnitInfo();
        basicUnitInfo.unitName = "BasicUnit";
        //basicUnitInfo.baseSpriteName = "BasicUnit_Base";
        basicUnitInfo.color0SpriteName = "BasicUnit_Base";
        basicUnitInfo.movementObjects = Util.toList(new MovementTypeParent[] 
        { 
            new JumpMoveType
            (
                new Vector2[]
                { 
                    vec(0,2), vec(1,0), vec(-1,1) 
                }
            ) 
        });
        basicUnitInfo.rotationEnabled = true;
        basicUnitInfo.relativeRotationDirections = Util.toList(new RelativeDirection[] 
        {
            RelativeDirection.FORWARD_RIGHT, RelativeDirection.FORWARD_LEFT 
        });
        tempUnitInfos.Add(basicUnitInfo);


        ////////////////
        //Ranged Unit
        ////////////////
        UnitInfo rangedUnitInfo = new UnitInfo();
        rangedUnitInfo.unitName = "RangedUnit";
        //rangedUnitInfo.baseSpriteName = "SpecialUnit_Base";
        rangedUnitInfo.color0SpriteName = "Triangle_Colors";
        rangedUnitInfo.movementObjects = Util.toList(new MovementTypeParent[] 
        { 
            new RangedMoveType
            (
                new Vector2[] 
                { 
                    vec(0, 3) 
                }
            ), 
            new JumpMoveType
            (
                new Vector2[]
                { 
                    vec(0, 1), vec(0,-1)
                }
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
        //specialUnitInfo.baseSpriteName = "SpecialUnit_Base";
        specialUnitInfo.color0SpriteName = "SpecialUnit_Base";
        specialUnitInfo.movementObjects = Util.toList(new MovementTypeParent[] 
        { 
            new SlideMoveType
            ( 
                //new Vector2[] 
                new RelativeDirection[]
                { 
                    RelativeDirection.FORWARD
                    //RotationDirectionObject.UP.getUpDirection() 
                }, 
                new int[] 
                { 
                    -1//3 -1 means infinite range
                }
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
        knightUnitInfo.color0SpriteName = "Star_Colors";
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
                    -1
                    //5
                }, 
                new uint[] 
                { 
                    1
                    //1
                }
            ),
            new JumpMoveType
            (
                new Vector2[]
                {
                    vec(1,0), vec(-1,1)
                }
            )
        });
        knightUnitInfo.rotationEnabled = true;
        knightUnitInfo.relativeRotationDirections = Util.toList(new RelativeDirection[]
        { 
            RelativeDirection.FORWARD_LEFT, RelativeDirection.FORWARD_RIGHT 
        } );
        tempUnitInfos.Add(knightUnitInfo);


        ////////////////
        //Normal Unit
        ////////////////
        UnitInfo normalUnitInfo = new UnitInfo();
        normalUnitInfo.unitName = "NormalUnit";
        normalUnitInfo.color0SpriteName = "Pentagon_Colors";//Pentagon_Colors
        normalUnitInfo.movementObjects = Util.toList(new MovementTypeParent[] 
        { 
            new PathMoveType
            (
                new PathMoveType.PathPos[][]
                {
                    /*new PathMoveType.PathPos[]
                    {
                        pathPos( vec(0, 1), false),
                        pathPos( vec(0, 2), true),
                        pathPos( vec(0, 3), false),
                        pathPos( vec(1, 3), true)
                    },
                    new PathMoveType.PathPos[]
                    {
                        pathPos( vec(0, 1), false),
                        pathPos( vec(0, 2), true),
                        pathPos( vec(0, 3), false),
                        pathPos( vec(-1, 4), true)
                    }*/
                    new PathMoveType.PathPos[]
                    {
                        pathPos( vec(1, 0), false),
                        pathPos( vec(1, 1), false),
                        pathPos( vec(1, 2), false),
                        pathPos( vec(0, 3), true)
                    },
                    new PathMoveType.PathPos[]
                    {
                        pathPos( vec(-1, 1), false),
                        pathPos( vec(-1, 2), false),
                        pathPos( vec(-1, 3), false),
                        pathPos( vec(0, 3), true)
                    }
                }
            ),
            new JumpMoveType
            (
                new Vector2[]
                {
                    vec(1,0), vec(-1,1)
                }
            )
        });
        normalUnitInfo.rotationEnabled = true;
        normalUnitInfo.relativeRotationDirections = Util.toList(new RelativeDirection[] 
        { 
            RelativeDirection.FORWARD_RIGHT, RelativeDirection.FORWARD_LEFT, RelativeDirection.BACKWARD_LEFT, RelativeDirection.BACKWARD_RIGHT 
        });
        tempUnitInfos.Add(normalUnitInfo);

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