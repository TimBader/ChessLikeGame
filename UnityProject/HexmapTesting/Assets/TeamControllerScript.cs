using UnityEngine;
using System.Collections;

public class TeamControllerScript : MonoBehaviour {

    private int team = 0;
    private UnitScript kingRef = null;

    private static Color[] teamColorList = 
    {
        new Color(0.0f, 0.0f, 1.0f),
        new Color(1.0f, 0.0f, 0.0f)
    };

    public void setTeam (int newTeam = 0)
    {
        team = newTeam;
    }



    public int getTeam()
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



    public static Color getTeamColor(int teamNum = 0)
    {
        if (teamNum >= teamColorList.Length)
        {
            throw new UnityException("Team " + teamNum + " does not have a team color set");
        }
        return teamColorList[teamNum];
    }
}
