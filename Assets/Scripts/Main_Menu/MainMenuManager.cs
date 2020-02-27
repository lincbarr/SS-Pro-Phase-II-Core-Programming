using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuManager : MonoBehaviour
{
    public void OnMenuQuit(InputAction.CallbackContext context)
    {
        Application.Quit();
    }
}
