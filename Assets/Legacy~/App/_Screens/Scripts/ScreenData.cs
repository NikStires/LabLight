using UnityEngine;

public enum ScreenType
{
    Calibration = 1,
    WorldLocking = 2,
    Menu = 3,
    Procedure = 4,
    Tracking = 5,
    Settings = 6,
    Intro = 7,
    WellPlateSettings = 8
}

public enum ParentType
{
    Root = 0,
    Stage = 1,
    Charuco = 2
}

[CreateAssetMenu(fileName = "New Screen Data", menuName = "LabLightAR/ScreenData")]
public class ScreenData : ScriptableObject
{
    public ScreenType Type;
    public ParentType Parent;
    public bool ShowGrid;
}

