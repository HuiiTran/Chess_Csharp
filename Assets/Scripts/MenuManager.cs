using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class MenuManager : MonoBehaviour
{
    public TMP_Dropdown ddTime;
    public TMP_Dropdown ddLevel;
    public TMP_Dropdown ddAISide;

    [SerializeField] GameObject PlayerMenu;
    [SerializeField] GameObject AIMenu;
    public void PlayGame()
    {
        if (ddTime.value == 0)
        {
            PieceManager.whiteTime = 60;
            PieceManager.blackTime = 60;
        }
        if (ddTime.value == 1)
        {
            PieceManager.whiteTime = 300;
            PieceManager.blackTime = 300;
        }
        if (ddTime.value == 2)
        {
            PieceManager.whiteTime = 900;
            PieceManager.blackTime = 900;
        }
        if (ddTime.value == 3)
        {
            PieceManager.whiteTime = 3600;
            PieceManager.blackTime = 3600;
        }

        PieceManager.AImode = false;
        SceneManager.LoadScene(1);
    }

    public void PlayAI()
    {
        PieceManager.AImode = true;

        if (ddAISide.value == 0)
            PieceManager.isAIWhite = true;
        if (ddAISide.value == 1)
            PieceManager.isAIWhite = false;

        AI.level = AI.AI_Level[ddLevel.value];

        SceneManager.LoadScene(1);
    }

    public void OpenPlayerMenu()
    {
        PlayerMenu.SetActive(true);
    }

    public void OpenAIMenu()
    {
        AIMenu.SetActive(true); 
    }

    public void CloseMenuPlayer()
    {
        PlayerMenu.SetActive(false);
    }

    public void CloseMenuAI()
    {
        AIMenu.SetActive(false);
    }
    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
