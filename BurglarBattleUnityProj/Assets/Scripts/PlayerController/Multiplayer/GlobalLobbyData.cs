using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalLobbyData
{
    public static List<int> s_deviceIDPair = new List<int>();

    public static int GetID(int index)
    {
        if(index < s_deviceIDPair.Count && index >= 0)
        {
            return s_deviceIDPair[index];
        }
        return -1;
    }
    public static void AddID(int id)
    {
        if(s_deviceIDPair.Count < 4)
        {
            s_deviceIDPair.Add(id);
        }
        else
        {
            CleanIDs();
            s_deviceIDPair.Add(id);
        }
    }
    public static void AddStartID(int id)
    {
        if (s_deviceIDPair.Count < 4)
        {
            s_deviceIDPair.Insert(0, id);
        }
        else
        {
            CleanIDs();
            s_deviceIDPair.Insert(0, id);
        }
    }
    public static void CleanIDs()
    {
        if(s_deviceIDPair.Count > 0)
        {
            s_deviceIDPair.Clear();
        }
    }
    public static bool isListEmpty()
    {
        return s_deviceIDPair.Count < 1;
    }
}
