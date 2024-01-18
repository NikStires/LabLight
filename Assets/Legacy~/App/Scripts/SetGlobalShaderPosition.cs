using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SetGlobalShaderPosition : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalVector("_CenterPosition", transform.position);
    }
}
