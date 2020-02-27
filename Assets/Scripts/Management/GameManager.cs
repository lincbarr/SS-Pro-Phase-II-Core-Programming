using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool _isGameOver = false;

    public void RestartGame()
    {
        if (_isGameOver == true)
        {
            SceneManager.LoadScene(1); // Current Game Scene
        }
    }

    public void GameOver()
    {
        _isGameOver = true;
    }

    public bool IsGameOver()
    {
        return _isGameOver;
    }
}
