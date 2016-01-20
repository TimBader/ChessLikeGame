using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct UnitInfo
{
    //Name of the unit
    public string unitName;
    //Un-Colored Sprite Name
    public string baseSpriteName;
    //Team-Colored Sprite Name
    public string colorsSpriteName;
    //Enum Type of the Unit's Movement
    //public MovementTypes movementTypes
    //The object required that holds all the movement logic and requirements for movement
    //public MovementTypeParent movementObject;
    public List<MovementTypeParent> movementObjects;
    //A list of directions a unit can rotate
    public List<RelativeDirection> relativeRotationDirections;
    //Allow rotations
    public bool rotationEnabled;
};


public class TempUnitInfos
{
    private static List<UnitInfo> tempUnitInfos = new List<UnitInfo>();

    private static List<T> toList<T>(T[] oList)
    {
        List<T> list = new List<T>();
        for (int i = 0; i < oList.Length; i++)
        {
            list.Add(oList[i]);
        }
        return list;
    }

    private static Vector2 vec(int x, int y)
    {
        return new Vector2(x, y);
    }

    public static void constructTempUnitInfos()
    {
        Vector2[] list;

        // King
        UnitInfo kingInfo = new UnitInfo();
        kingInfo.unitName = "King";
        kingInfo.baseSpriteName = "King_Base";
        kingInfo.colorsSpriteName = "King_Colors";
        /*List<Vector2> kingJumpPositions = new List<Vector2>();
        kingJumpPositions.Add(new Vector2(0, -1));
        kingJumpPositions.Add(new Vector2(1, -1));
        kingJumpPositions.Add(new Vector2(1, 0));
        kingJumpPositions.Add(new Vector2(0, 1));
        kingJumpPositions.Add(new Vector2(-1, 1));
        kingJumpPositions.Add(new Vector2(-1, 0));*/
        //List<MovementTypeParent> kingMovementObjects = new List<MovementTypeParent>();
        //JumpMoveType kingJumpMove = new JumpMoveType(toList(new Vector2[]{ vec(0,-1), vec(1,-1), vec(1,0), vec(0,1), vec(-1,1), vec(-1,0)} ));
        //kingJumpMove.initialize(toList(new Vector2[]{ vec(0,-1), vec(1,-1), vec(1,0), vec(0,1), vec(-1,1), vec(-1,0)} ));
        //kingMovementObjects.Add(kingJumpMove);
        kingInfo.movementObjects = toList(new MovementTypeParent[]
        {
            new JumpMoveType
            (
                toList(new Vector2[]
                { 
                    vec(0,-1), vec(1,-1), vec(1,0), vec(0,1), vec(-1,1), vec(-1,0)
                } )
            )
        });//kingMovementObjects;
        //kingInfo.movementObject = kingJumpMove;
        kingInfo.rotationEnabled = false;
        kingInfo.relativeRotationDirections = new List<RelativeDirection>();
        //kingInfo.movementType = MovementTypes.Jump;
        tempUnitInfos.Add(kingInfo);
        

        //test([new Vector2(0,0)]);


        // Basic Unit
        UnitInfo basicUnitInfo = new UnitInfo();
        basicUnitInfo.unitName = "BasicUnit";
        //basicUnitInfo.baseSpriteName = "BasicUnit_Base";
        basicUnitInfo.colorsSpriteName = "BasicUnit_Base";
        /*List<Vector2> basicUnitJumpPositions = new List<Vector2>();
        basicUnitJumpPositions = new List<Vector2>();
        basicUnitJumpPositions.Add(new Vector2(0, 2));
        basicUnitJumpPositions.Add(new Vector2(1, 0));
        basicUnitJumpPositions.Add(new Vector2(-1, 1));*/
        //List<MovementTypeParent> basicUnitMovementObjects = new List<MovementTypeParent>();
        //JumpMoveType basicUnitJumpMove = new JumpMoveType(toList(new Vector2[]{ vec(0,2), vec(1,0), vec(-1,1) } ));
        //basicUnitJumpMove.initialize(toList(new Vector2[]{ vec(0,2), vec(1,0), vec(-1,1)} ));
        //basicUnitMovementObjects.Add(basicUnitJumpMove);
        /*basicUnitInfo.relativeRotationDirections = new List<RelativeDirection>();
        basicUnitInfo.relativeRotationDirections.Add(RelativeDirection.FORWARD_RIGHT);
        basicUnitInfo.relativeRotationDirections.Add(RelativeDirection.FORWARD_LEFT);*/
        //basicUnitInfo.movementObjects = basicUnitMovementObjects;
        basicUnitInfo.movementObjects = toList(new MovementTypeParent[] 
        { 
            new JumpMoveType
            (
                toList(new Vector2[]
                { 
                    vec(0,2), vec(1,0), vec(-1,1) 
                } )
            ) 
        });
        //basicUnitInfo.movementObject = basicUnitJumpMove;
        basicUnitInfo.rotationEnabled = true;
        basicUnitInfo.relativeRotationDirections = toList(new RelativeDirection[] 
        {
            RelativeDirection.FORWARD_RIGHT, RelativeDirection.FORWARD_LEFT 
        });
        //basicUnitInfo.movementType = MovementTypes.Jump;
        tempUnitInfos.Add(basicUnitInfo);



        //Ranged Unit
        UnitInfo rangedUnitInfo = new UnitInfo();
        rangedUnitInfo.unitName = "RangedUnit";
        //rangedUnitInfo.baseSpriteName = "SpecialUnit_Base";
        rangedUnitInfo.colorsSpriteName = "Triangle_Colors";
        /*List<Vector2> rangePos = new List<Vector2>();
        rangePos.Add(new Vector2(0, 3));
        rangePos.Add(new Vector2(1, 2));
        rangePos.Add(new Vector2(-1, 3));*/
        /*List<MovementTypeParent> rangedUnitMovementObjects = new List<MovementTypeParent>();
        RangedMoveType rangedUnitMoveType = new RangedMoveType( toList( new Vector2[]{ vec(0,3), vec(1,2), vec(-1,3) } ) );
        rangedUnitMoveType.initialize(toList(new Vector2[]{ vec(0,3), vec(1,2), vec(-1,3) }));
        //rangedUnitMovementObjects.Add(rangedUnitMoveType);
        JumpMoveType rangedUnitJumpMoveType = new JumpMoveType(toList(new Vector2[] { vec(0, 1), vec(1, 0), vec(-1, 1) }));*/
        /*List<Vector2> jumpPos = new List<Vector2>();
        jumpPos.Add(new Vector2(0,1));
        jumpPos.Add(new Vector2(1, 0));
        jumpPos.Add(new Vector2(-1, 1));*/
        //rangedUnitJumpMoveType.initialize(toList(new Vector2[]{ vec(0,1), vec(1,0), vec(-1,1) }));
        //rangedUnitMovementObjects.Add(rangedUnitJumpMoveType);
        rangedUnitInfo.movementObjects = toList(new MovementTypeParent[] 
        { 
            new RangedMoveType
            (
                toList(new Vector2[] 
                { 
                    vec(0, 3) 
                })), 
                new JumpMoveType(toList(new Vector2[]
                { 
                    vec(0, 1), vec(0,-1)
                })
            )
        });
        //rangedUnitInfo.movementType = MovementTypes.Ranged;
        //rangedUnitInfo.movementObject = rangedUnitMoveType;
        /*rangedUnitInfo.relativeRotationDirections = new List<RelativeDirection>();
        rangedUnitInfo.relativeRotationDirections.Add(RelativeDirection.FORWARD_RIGHT);
        rangedUnitInfo.relativeRotationDirections.Add(RelativeDirection.FORWARD_LEFT);
        rangedUnitInfo.relativeRotationDirections.Add(RelativeDirection.BACKWARD_LEFT);
        rangedUnitInfo.relativeRotationDirections.Add(RelativeDirection.BACKWARD_RIGHT);
        rangedUnitInfo.relativeRotationDirections.Add(RelativeDirection.BACKWARD);*/
        rangedUnitInfo.rotationEnabled = true;
        rangedUnitInfo.relativeRotationDirections = toList(new RelativeDirection[]
        { 
            RelativeDirection.FORWARD_RIGHT, RelativeDirection.FORWARD_LEFT, RelativeDirection.BACKWARD_LEFT, RelativeDirection.BACKWARD_RIGHT, RelativeDirection.BACKWARD
        });
        tempUnitInfos.Add(rangedUnitInfo);



        //Special Unit
        UnitInfo specialUnitInfo = new UnitInfo();
        specialUnitInfo.unitName = "SpecialUnit";
        //specialUnitInfo.baseSpriteName = "SpecialUnit_Base";
        specialUnitInfo.colorsSpriteName = "SpecialUnit_Base";
        /*List<Vector2> specialUnitSlideDirections = new List<Vector2>();
        specialUnitSlideDirections.Add(RotationDirectionObject.UP.getUpDirection());*/
        /*List<int> specialUnitSlideRanges = new List<int>();
        specialUnitSlideRanges.Add(3);*/
        //List<MovementTypeParent> specialUnitMovementObjects = new List<MovementTypeParent>();
        //SlideMoveType slideMove = new SlideMoveType( toList( new Vector2[]{ RotationDirectionObject.UP.getUpDirection() } ) , toList( new int[]{ 3 } ) );
        //slideMove.initialize(toList(new Vector2[]{ RotationDirectionObject.UP.getUpDirection() }), toList(new int[]{ 3 } ));
        //specialUnitMovementObjects.Add(slideMove);
        //specialUnitInfo.movementObjects = specialUnitMovementObjects;
        specialUnitInfo.movementObjects = toList(new MovementTypeParent[] 
        { 
            new SlideMoveType
            ( 
                toList( new Vector2[] 
                { 
                    RotationDirectionObject.UP.getUpDirection() 
                }), 
                toList(new int[] 
                { 
                    3 
                })
            ) 
        });
        //specialUnitInfo.movementObject = slideMove;
        //specialUnitInfo.movementType = MovementTypes.Slide;
        /*specialUnitInfo.relativeRotationDirections = new List<RelativeDirection>();
        specialUnitInfo.relativeRotationDirections.Add(RelativeDirection.FORWARD_RIGHT);
        specialUnitInfo.relativeRotationDirections.Add(RelativeDirection.FORWARD_LEFT);
        specialUnitInfo.relativeRotationDirections.Add(RelativeDirection.BACKWARD_LEFT);
        specialUnitInfo.relativeRotationDirections.Add(RelativeDirection.BACKWARD_RIGHT);
        specialUnitInfo.relativeRotationDirections.Add(RelativeDirection.BACKWARD);*/
        specialUnitInfo.rotationEnabled = true;
        specialUnitInfo.relativeRotationDirections = toList(new RelativeDirection[] 
        { 
            RelativeDirection.FORWARD_RIGHT, RelativeDirection.FORWARD_LEFT, RelativeDirection.BACKWARD_LEFT, RelativeDirection.BACKWARD_RIGHT, RelativeDirection.BACKWARD 
        });
        tempUnitInfos.Add(specialUnitInfo);



        //Knight Unit
        UnitInfo knightUnitInfo = new UnitInfo();
        knightUnitInfo.unitName = "KnightUnit";
        knightUnitInfo.colorsSpriteName = "Star_Colors";
        /*List<Vector2> knightUnitChargeDirections = new List<Vector2>();
        knightUnitChargeDirections.Add(RotationDirectionObject.UP.getUpDirection());
        List<MovementTypeParent> knightUnitMovementObjects = new List<MovementTypeParent>();
        List<uint> knightUnitBlockingExtents = new List<uint>();
        knightUnitBlockingExtents.Add(1);
        List<uint> knightUnitRanges = new List<uint>();
        knightUnitRanges.Add(5);*/
        //ChargeMoveType knightUnitChargeMoveType = new ChargeMoveType(toList(new Vector2[] { RotationDirectionObject.UP.getUpDirection() }), toList(new uint[] { 5 }), toList(new uint[] { 1 }));
        //knightUnitChargeMoveType.initialize(knightUnitChargeDirections, knightUnitRanges, knightUnitBlockingExtents);
        //knightUnitMovementObjects.Add(knightUnitChargeMoveType);
        //knightUnitInfo.movementType = MovementTypes.Charge;
        //knightUnitInfo.movementObject = knightUnitChargeMoveType;
        knightUnitInfo.movementObjects = toList(new MovementTypeParent[] 
        { 
            new ChargeMoveType
            (
                toList(new Vector2[] 
                { 
                    RotationDirectionObject.UP.getUpDirection() 
                }), 
                toList(new uint[] 
                { 
                    5 
                }), 
                toList(new uint[] 
                { 
                    1 
                })
            ),
            new JumpMoveType
            (
                toList(new Vector2[]
                {
                    vec(1,0), vec(-1,1)
                })
            )
        });
        /*knightUnitInfo.relativeRotationDirections = new List<RelativeDirection>();
        knightUnitInfo.relativeRotationDirections.Add(RelativeDirection.FORWARD_RIGHT);
        knightUnitInfo.relativeRotationDirections.Add(RelativeDirection.FORWARD_LEFT);*/
        knightUnitInfo.rotationEnabled = true;        
        knightUnitInfo.relativeRotationDirections = toList( new RelativeDirection[]
        { 
            RelativeDirection.FORWARD_LEFT, RelativeDirection.FORWARD_RIGHT 
        } );
        tempUnitInfos.Add(knightUnitInfo);



        //Normal Unit
        UnitInfo normalUnitInfo = new UnitInfo();
        normalUnitInfo.unitName = "NormalUnit";
        normalUnitInfo.colorsSpriteName = "Pentagon_Colors";
        /*List<List<Vector2>> normalUnitMovePositions = new List<List<Vector2>>();
        List<Vector2> movePos = new List<Vector2>();
        movePos.Add(new Vector2(1, 2));
        normalUnitMovePositions.Add(movePos);
        movePos = new List<Vector2>();
        movePos.Add(new Vector2(-1, 3));
        normalUnitMovePositions.Add(movePos);
        List<Vector2> jumpPositions = new List<Vector2>();
        jumpPositions.Add(new Vector2(0, 1));
        jumpPositions.Add(new Vector2(0, -1));
        List<List<Vector2>> normalUnitBlockingPositions = new List<List<Vector2>>();
        List<Vector2> blockingPos = new List<Vector2>();
        blockingPos.Add(new Vector2(1, 0));
        blockingPos.Add(new Vector2(1, 1));
        normalUnitBlockingPositions.Add(blockingPos);
        blockingPos = new List<Vector2>();
        blockingPos.Add(new Vector2(-1, 1));
        blockingPos.Add(new Vector2(-1, 2));
        normalUnitBlockingPositions.Add(blockingPos);*/
        //NormalMoveType normalUnitNormalMove = new NormalMoveType( new List<List<Vector2>>{ toList(new Vector2[] { vec(1, 2) }), toList(new Vector2[] { vec(-1, 3) }) }, new List<List<Vector2>>{ toList( new Vector2[]{ vec(1,0), vec(1,1) } ), toList( new Vector2[]{ vec(-1,1), vec(-1,2) }) });
        //normalUnitNormalMove.initialize(normalUnitMovePositions, normalUnitBlockingPositions);
        //JumpMoveType normalUnitJumpMove = new JumpMoveType( toList( new Vector2[]{ vec(0,1), vec(1,1) } ) );
        //normalUnitJumpMove.initialize(jumpPositions);
        //List<MovementTypeParent> normalUnitMovementObjects = new List<MovementTypeParent>();
        //normalUnitMovementObjects.Add(normalUnitNormalMove);
        //normalUnitMovementObjects.Add(normalUnitJumpMove);

        normalUnitInfo.movementObjects = toList(new MovementTypeParent[] 
        { 
            new NormalMoveType
            (
                new List<List<Vector2>> 
                { 
                    toList(new Vector2[] 
                    { 
                        vec(1, 2) 
                    }), 
                    toList(new Vector2[] 
                    { 
                        vec(-1, 3) 
                    }) 
                }, 
                new List<List<Vector2>> 
                { 
                    toList(new Vector2[] 
                    { 
                        vec(1, 0), vec(1, 1) 
                    }), 
                    toList(new Vector2[] 
                    { 
                        vec(-1, 1), vec(-1, 2) 
                    }) 
                }
            ), 
            new JumpMoveType
            (
                toList(new Vector2[] 
                { 
                    vec(0, 1), vec(0, -1) 
                })
            ) 
        });

        //normalUnitInfo.movementObjects = normalUnitMovementObjects;
        //normalUnitInfo.movementType = MovementTypes.Normal;
        //normalUnitInfo.movementObject = normalUnitNormalMove;
        /*normalUnitInfo.relativeRotationDirections = new List<RelativeDirection>();
        normalUnitInfo.relativeRotationDirections.Add(RelativeDirection.FORWARD_RIGHT);
        normalUnitInfo.relativeRotationDirections.Add(RelativeDirection.FORWARD_LEFT);
        normalUnitInfo.relativeRotationDirections.Add(RelativeDirection.BACKWARD_LEFT);
        normalUnitInfo.relativeRotationDirections.Add(RelativeDirection.BACKWARD_RIGHT);*/
        normalUnitInfo.rotationEnabled = true;        
        normalUnitInfo.relativeRotationDirections = toList(new RelativeDirection[] 
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
