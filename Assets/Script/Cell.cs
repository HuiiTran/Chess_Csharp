using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CellState
{
    NONE,
    FRIEND,
    ENEMY,
    FREE,
    PASSANT, 
    CHECK,
    CHECK_ENEMY,
    CHECK_FRIEND
}

public class Cell : MonoBehaviour
{
    public Image outlineImage;

    [HideInInspector]
    public Vector2Int boardPosition = Vector2Int.zero;
    [HideInInspector]
    public Board board = null;
    [HideInInspector]
    public RectTransform rectTransform = null;
    [HideInInspector]
    public BasePiece currentPiece;
    [HideInInspector]
    public Pawn enPassant;


    public void Setup(Vector2Int newBoardPosition, Board newBoard)
    {
        boardPosition = newBoardPosition;
        board = newBoard;

        rectTransform = GetComponent<RectTransform>();
        outlineImage.enabled = false;

        enPassant = null;
    }

    /// <summary>
    /// Remove piece positioned on the cell
    /// </summary>
    public void RemovePiece()
    {
        if (currentPiece != null)
        {
            currentPiece.Kill();
        }
        if(enPassant != null)
        {
            enPassant.Kill();
        }
    }

    /// <summary>
    /// Get state of a cell
    /// </summary>
    /// <param name="checkingPiece"></param>
    /// <returns></returns>
    public CellState GetState(BasePiece checkingPiece)
    {
        //check
        if (!checkingPiece.GetPieceManager().checkVerificationInProcess)
        {
            checkingPiece.GetPieceManager().checkVerificationInProcess = true;

            BasePiece originalCurrentPiece = currentPiece;
            Cell originalCurrentCell = checkingPiece.currentCell;

            currentPiece = checkingPiece;
            checkingPiece.currentCell.currentPiece = null;
            checkingPiece.currentCell = this;

            if (checkingPiece.isCheckVerif(!checkingPiece.isWhite))
            {
                checkingPiece.GetPieceManager().checkVerificationInProcess = false;

                currentPiece = originalCurrentPiece;
                checkingPiece.currentCell = originalCurrentCell;
                checkingPiece.currentCell.currentPiece = checkingPiece;

                //other
                if (currentPiece != null )
                {
                    // if friend
                    if (checkingPiece.isWhite == currentPiece.isWhite && checkingPiece.GetPieceManager().getKing(checkingPiece.isWhite).isCheck)
                    {
                        return CellState.CHECK_FRIEND;
                    }
                    // if enemy
                    else
                    {
                        return CellState.CHECK_ENEMY;
                    }
                }
                return CellState.CHECK;
            }

            currentPiece = originalCurrentPiece;
            checkingPiece.currentCell = originalCurrentCell;
            checkingPiece.currentCell.currentPiece = checkingPiece;

            checkingPiece.GetPieceManager().checkVerificationInProcess = false;
        }        

        //other
        if (currentPiece != null)
        {
            // if friend
            if (checkingPiece.isWhite == currentPiece.isWhite)
            {
                return CellState.FRIEND;
            }
            // if enemy
            else
            {
                return CellState.ENEMY;
            }
        }
        else
        {
            if(enPassant != null)
            {
                return CellState.PASSANT;
            }
        }
        return CellState.FREE;
    }
}