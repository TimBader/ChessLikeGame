using UnityEngine;
using System.Collections;

public class TeamControllerScript : MonoBehaviour {

    private uint team = 0;
    private UnitScript kingRef = null;



    public void setTeam (uint newTeam = 0)
    {
        team = newTeam;
    }



    public uint getTeam()
    {
        return team;
    }



    public void setKing(UnitScript unitRef)
    {
        kingRef = unitRef;
    }



    public UnitScript getKing()
    {
        return kingRef;
    }
}
