using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestKeyboardInput : MonoBehaviour
{
    // Start is called before the first frame update

    // private InputAction nextProcedureAction;
    // private InputAction previousProcedureAction;

    // void Start()
    // {
    //     nextProcedureAction = InputSystem.actions.FindAction("Progress Forward");
    //     previousProcedureAction = InputSystem.actions.FindAction("Progress Backward");
    // }

    // // Update is called once per frame
    // void Update()
    // {
    //     if (nextProcedureAction.IsPressed())
    //     {
    //         Debug.Log("Next action pressed");
    //     }

    //     if (previousProcedureAction.IsPressed())
    //     {
    //         Debug.Log("Previous action pressed");
    //     }
    // }
    
    void OnEnable()
    {
        InputSystem.actions.FindAction("Progress Forward").performed += TestNext;
        InputSystem.actions.FindAction("Progress Backward").performed += TestPrevious;
    }

    void OnDisable()
    {
        InputSystem.actions.FindAction("Progress Forward").performed -= TestNext;
        InputSystem.actions.FindAction("Progress Backward").performed -= TestPrevious;
    }

    public void TestNext(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            Debug.Log("Next action performed");
        }
    }

    public void TestPrevious(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            Debug.Log("Previous action performed");
        }
    }
}
