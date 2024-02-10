using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCenterPanelViewController : MonoBehaviour
{
    [SerializeField] GameObject timerPrefab;
     
    public void SpawnTimer()
    {
        var timer = Instantiate(timerPrefab);
        timer.transform.position = transform.position;
        timer.transform.rotation = transform.rotation;
        this.gameObject.SetActive(false);
    }
}
