using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Util : MonoBehaviour 
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
}
