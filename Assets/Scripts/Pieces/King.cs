using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class King : BasePiece
{
    [HideInInspector]
    public bool isCheck = false;

    public override void Setup(bool newIsWhite, PieceManager newPM)
    {
        base.Setup(newIsWhite, newPM);
        isCheck = false;
        movement = new Vector3Int(1, 1, 1);
        
        GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/King");
    
    }
    public void setCheck(bool state)
    {
        isCheck = state;
        if(state)
        {
            currentCell.outlineImage.GetComponent<Image>().color = new Color(1, (float)0.5, (float)0.2, (float)0.5);
            currentCell.outlineImage.enabled = true;
        }
        else
        {
            currentCell.outlineImage.GetComponent<Image>().color = new Color(1, 0, 0, (float)0.0);
            currentCell.outlineImage.enabled = false;
        }
    }

    public override void Kill()
    {
        base.Kill();

        pieceManager.isKingAlive = false;
    }

    protected override void CheckPathing()
    {
        base.CheckPathing();
        if(hasMoved == false && pieceManager.checkVerificationInProcess == false)
        {
            Cell cellA = currentCell.board.allCells[0][currentCell.boardPosition.y];
            Cell cellB = currentCell.board.allCells[1][currentCell.boardPosition.y];
            Cell cellC = currentCell.board.allCells[2][currentCell.boardPosition.y];
            Cell cellD = currentCell.board.allCells[3][currentCell.boardPosition.y];
            Cell cellF = currentCell.board.allCells[5][currentCell.boardPosition.y];
            Cell cellG = currentCell.board.allCells[6][currentCell.boardPosition.y];
            Cell cellH = currentCell.board.allCells[7][currentCell.boardPosition.y];

            // Rook state and b state
            pieceManager.checkVerificationInProcess = true;
            CellState stateRookA = cellA.GetState(this);
            CellState stateRookH = cellH.GetState(this);
            CellState stateBCell = cellB.GetState(this);
            pieceManager.checkVerificationInProcess = false;



            // Long castle
            if (stateRookA == CellState.FRIEND && cellA.currentPiece.hasMoved == false &&
                stateBCell == CellState.FREE && cellC.GetState(this) == CellState.FREE &&
                 cellD.GetState(this) == CellState.FREE && isCheck == false)
            {
                highlightedCells.Add(cellC);
            }
            // Short castle
            if(stateRookH == CellState.FRIEND && cellH.currentPiece.hasMoved == false &&
                cellF.GetState(this) == CellState.FREE && cellG.GetState(this) == CellState.FREE
                && isCheck == false)
            {
                highlightedCells.Add(cellG);
            }
        }
        
        
    }
}
