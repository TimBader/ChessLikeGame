using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class TeamColorPallet : MonoBehaviour
{
    public static void initalizeColorPallets()
    {
        teamPallets = Util.toList( new TeamColorPallet[]
        {
            // RGB Colors, can take float or hexidecimal values
            //Team 0 Colors
            new TeamColorPallet(hexColor(0xff, 0x00, 0), hexColor(0x05, 0x2e, 0x95), hexColor(0x93,0x98,0xc8), hexColor(0x2e, 0x95, 0x05)),
            //Team 1 Colors
            new TeamColorPallet(hexColor(0, 0xa6, 0xff), hexColor(0xff, 0x09, 0x09), hexColor(0x3b, 0x3b, 0x3b), hexColor(0x71, 0x05, 0x95)),
        });
    }

    public static Color hexColor(float rHexValue, float gHexValue, float bHexValue)
    {
        return new Color(rHexValue / 255.0f, gHexValue / 255.0f, bHexValue / 255.0f);
    }

    public const int maxColorsPerPallet = 4;

    private static List<TeamColorPallet> teamPallets = null;

    private List<Color> colors = null;

    public TeamColorPallet(Color color0, Color color1, Color color2, Color color3)
    {
        colors = new List<Color> { color0, color1, color2, color3};
        //colors.Add(color0); colors.Add(color1); colors.Add(color2); colors.Add(color3);
    }

    public static TeamColorPallet getTeamColorPallet(int teamNum)
    {
        if (teamPallets == null)
        {
            initalizeColorPallets();
        }

        if (teamNum >= teamPallets.Count || teamNum < 0)
        {
            throw new UnityException("Team " + teamNum + " does not exist");
        }

        return teamPallets[teamNum];
    }

    public static Color getColorFromTeamPallet(int teamNum, int colorNum)
    {
        if (teamPallets == null)
        {
            initalizeColorPallets();
        }

        if (teamNum >= teamPallets.Count || teamNum < 0)
        {
            throw new UnityException("Team " + teamNum + " does not exist");
        }
        if (colorNum >= maxColorsPerPallet || colorNum < 0)
        {
            throw new UnityException("Color " + colorNum + " does not exist, max amount of colors per pallet: " + maxColorsPerPallet);
        }
        return teamPallets[teamNum].getColor(colorNum);
    }

    public Color getColor(int colorNum)
    {
        if (colorNum >= maxColorsPerPallet || colorNum < 0)
        {
            throw new UnityException("Color " + colorNum + " does not exist, max amount of colors per pallet: " + maxColorsPerPallet);
        }
        return colors[colorNum];
    }

}


public class TeamControllerScript : MonoBehaviour {

    private int team = 0;
    private UnitScript lordRef = null;
    //public AbsoluteDirection defaultDirection = AbsoluteDirection.UP;

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



    public void setLord(UnitScript unitRef)
    {
        lordRef = unitRef;
    }



    public UnitScript getLord()
    {
        return lordRef;
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
