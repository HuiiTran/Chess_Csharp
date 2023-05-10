using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public Board board;

    public PieceManager pieceManager;


    [SerializeField] GameObject pauseMenu;

    [SerializeField] GameObject pauseMenuSelect;
    // Start is called before the first frame update
    void Start()
    {
        board.Create();

        pieceManager.Setup(board);
    }

    public void Reverse()
    {
        board.transform.localRotation *= Quaternion.Euler(180, 180, 0);
        foreach( List<Cell> row in board.allCells)
        {
            foreach( Cell boardCell in row)
            {
                if( boardCell.currentPiece != null)
                    boardCell.currentPiece.PlaceInit(boardCell);
            }
        }
    }
    public void Reload()
    {

        pieceManager.ResetGame();
        pauseMenuSelect.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Pause()
    {
        pauseMenuSelect.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        pauseMenuSelect.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Home()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }

    public void ChooseQueen()
    {
        PieceManager.promotionStatus = 1;
        PieceManager.isBeingPromotion = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ChooseKnight()
    {
        PieceManager.promotionStatus = 2;
        PieceManager.isBeingPromotion = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ChooseBishop()
    {
        PieceManager.promotionStatus = 3;
        PieceManager.isBeingPromotion = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ChooseRook()
    {
        PieceManager.promotionStatus = 4;
        PieceManager.isBeingPromotion = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }
}
