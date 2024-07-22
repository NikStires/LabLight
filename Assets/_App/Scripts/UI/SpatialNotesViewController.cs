using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SpatialNotesViewController : LLBasePanel
{
    public void CloseSpatialNotesMenu()
    {
        SceneLoader.Instance.UnloadScene("SpatialNotesEditor");
    }
}
