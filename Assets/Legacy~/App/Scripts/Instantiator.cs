using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instantiator : MonoBehaviour
{
    public GameObject Prefab;

    public Vector3 Step;

    public int CountX;
    public int CountY;
    public int CountZ;

    private void Start()
    {
        for (int i=0; i < transform.childCount; i++)
        {
            GameObject.Destroy(transform.GetChild(i));
        }

        Vector3 offset = 0.5f * new Vector3((CountX-1) * Step.x, (CountY-1) * Step.y, (CountZ-1) * Step.z);
        

        for (int i=0; i < CountX; i++)
        {
            for (int j = 0; j < CountY; j++)
            {
                for (int k = 0; k < CountZ; k++)
                {
                    Instantiate(Prefab, new Vector3(i * Step.x, j * Step.y, k * Step.z) - offset, Quaternion.identity, transform);
                }
            }
        }
    }
}
