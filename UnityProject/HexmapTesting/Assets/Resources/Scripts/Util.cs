using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Util
{
    public static List<T> toList<T>(T[] oList)
    {
        List<T> list = new List<T>();
        for (int i = 0; i < oList.Length; i++)
        {
            list.Add(oList[i]);
        }
        return list;
    }

    public static Color alphaColor(Color oColor, float newAlpha)
    {
        return new Color(oColor.r, oColor.g, oColor.b, newAlpha);
    }
}
