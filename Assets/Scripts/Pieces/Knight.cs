using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Knight : BasePiece
{
    public override void Setup(bool newIsWhite, PieceManager newPM)
    {
        base.Setup(newIsWhite, newPM);

        movement = new Vector3Int(1, 1, 1);
        GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/WhiteKnight");
    }
    private void MatchesState(Cell target)
    {
        CellState targetState = target.GetState(this);

        if(targetState != CellState.FRIEND && targetState != CellState.CHECK && targetState != CellState.CHECK_ENEMY && targetState != CellState.CHECK_FRIEND)
        {
            if( !pieceManager.checkVerificationInProcess)
            {
                if(targetState == CellState.ENEMY)
                {
                    target.outlineImage.GetComponent<Image>().color = new Color(1, 0, 0, (float)0.5);
                }
                else
                {
                    target.outlineImage.GetComponent<Image>().color = new Color(0, 1, 0, (float)0.5);
                }
            }
            addPossibleCell(target);
        }
    }
    //set L shape move
    public void CreateCellPath (int yDirection)
    {
        int currentX = currentCell.boardPosition.x;
        int currentY = currentCell.boardPosition.y;
        Cell targetCell;
        //high left
        try
        {
            targetCell = currentCell.board.allCells[currentX - 2][currentY + 1 * yDirection];
            MatchesState(targetCell);
        }
        catch (Exception e) { e.ToString(); }

        //low left
        try
        {
            targetCell = currentCell.board.allCells[currentX - 1][currentY + 2 * yDirection];
            MatchesState(targetCell);
        }
        catch (Exception e) { e.ToString(); }

        //high right
        try
        {
            targetCell = currentCell.board.allCells[currentX + 2][currentY + 1 * yDirection];
            MatchesState(targetCell);
        }
        catch (Exception e) { e.ToString(); }
        //low right
        try
        {
            targetCell = currentCell.board.allCells[currentX + 1][currentY + 2 * yDirection];
            MatchesState(targetCell);
        }
        catch (Exception e) { e.ToString(); }
    }
    protected override void CheckPathing()
    {
        //check higher
        CreateCellPath(1);
        //check lower
        CreateCellPath(-1);
    }
}
