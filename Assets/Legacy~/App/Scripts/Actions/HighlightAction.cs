using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightAction
{
    public string highlightName;
    public string actionName;
    public bool isSource;
    public List<string> chainIDs; //list of all subIds
    public Tuple <string, string> colorInfo;   //item1 = hex, item2 = name
    public Tuple <string, string> contents; //item1 = name, item2 = abbreviation
    public Tuple <float, string> volume; //item1 = volume, item2 = units
}
