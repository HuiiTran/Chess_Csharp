using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BasePiece : EventTrigger
{
    //Variables
    [HideInInspector]
    public bool isWhite;
    
    public bool hasMoved = false;

    [HideInInspector]
    static int cellPadding = 10;

    protected Cell originalCell = null;

    [HideInInspector]
    public Cell currentCell = null;

    protected RectTransform rt = null;
    protected PieceManager pieceManager;

    protected Vector3Int movement = Vector3Int.one;
    protected List<Cell> highlightedCells = new List<Cell>();
    protected List<Cell> attackedCells = new List<Cell>();

    private Cell targetCell = null;

    public bool inDrag = false;

    public PieceManager GetPieceManager()
    {
        return pieceManager;
    }
    public static int CellPadding {get => cellPadding;}
    public Cell TargetCell {get => targetCell; set => targetCell = value;}


    //////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////Logic/////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////
    public virtual void Kill()
    {
        currentCell.currentPiece = null;
        gameObject.SetActive(false);
    }


    //init piece
    public virtual void Setup(bool newIsWhite, PieceManager newPM)
    {
        inDrag = false;
        pieceManager = newPM;
        isWhite = newIsWhite;
        hasMoved = false;

        rt = GetComponent<RectTransform>();

        //Set color
        if(isWhite)
            GetComponent<Image>().color = Color.white;
        else
            GetComponent<Image>().color = Color.gray;//new Color(0, 0, 0,(float)0.65);
    }


    //init placement
    public void PlaceInit(Cell newCell)
    {
        currentCell = newCell;
        originalCell = newCell;
        currentCell.currentPiece = this;

        transform.position = newCell.transform.position;
        gameObject.SetActive(true);
    }


    //Move
    public virtual void Move()
    {
        //sounds
        AudioClip clip = null;
        if (targetCell.currentPiece == null)
        {
            clip = (AudioClip)Resources.Load("Sound/move");
        }
        else
        {
            clip = (AudioClip)Resources.Load("Sound/capture");
        }
    }


    //add cell to list
    protected void addPossibleCell(Cell possibleCell)
    {
        if (pieceManager.checkVerificationInProcess)
            attackedCells.Add(possibleCell);
        else
            highlightedCells.Add(possibleCell);
    }

    //Check posibility of a piece
    private void CreateCellPath(int xDirection, int yDirection, int movement)
    {
        //get current position
        int currentX = currentCell.boardPosition.x;
        int currentY = currentCell.boardPosition.y;
        //loop throught each cell
        for (int i = 1; i <= movement; i++)
        {
            currentX += xDirection;
            currentY += yDirection;

            if(currentX < 0 || currentY < 0 || currentX > currentCell.board.Column - 1 || currentY > currentCell.board.Row - 1)
                continue;
            Cell targeted = currentCell.board.allCells[currentX][currentY];

            CellState state = targeted.GetState(this);
            if (state != CellState.FRIEND && state != CellState.CHECK && state != CellState.CHECK_ENEMY && state != CellState.CHECK_FRIEND)
            {
                if(!pieceManager.checkVerificationInProcess)
                {
                    if (state == CellState.ENEMY) //set color for capture piece
                    {
                        targeted.outlineImage.GetComponent<Image>().color = new Color(1, 0, 0, (float)0.5);
                    }
                    else
                    {
                        targeted.outlineImage.GetComponent<Image>().color = new Color(0, 1, 0, (float)0.5);
                    }

                    addPossibleCell(targeted);
                }
                if (state == CellState.ENEMY || state == CellState.FRIEND || state == CellState.CHECK_ENEMY || state == CellState.CHECK_FRIEND)
                break;
            }   
        }
    }


    // check legal move
    protected virtual void CheckPathing()
    {
        // Horizontal
        CreateCellPath(1, 0, movement.x);
        CreateCellPath(-1, 0, movement.x);

        // Vertical 
        CreateCellPath(0, 1, movement.y);
        CreateCellPath(0, -1, movement.y);

        // Upper diagonal
        CreateCellPath(1, 1, movement.z);
        CreateCellPath(-1, 1, movement.z);

        // Lower diagonal
        CreateCellPath(-1, -1, movement.z);
        CreateCellPath(1, -1, movement.z);

    }
    //drag piece
    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        inDrag = true;

        // Test for cells
        CheckPathing();

        // Show valid cells
        ShowCellsHighlight();

        transform.position = Input.mousePosition;
        transform.SetAsLastSibling();
    }
    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);

        // Follow pointer
        transform.position += (Vector3)eventData.delta;
    }
    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        inDrag = false;

        // Get target cell
        targetCell = null;
        foreach (Cell cell in highlightedCells)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(cell.rectTransform, Input.mousePosition))
            {
                targetCell = cell;
                break;
            }
        }

        // Return to his original position
        if (!targetCell || pieceManager.gameState != GameState.INGAME)
        {
            transform.position = currentCell.transform.position; // gameObject
        }
        else
        {
            Move();                   
        }

        // clear highlight
        ClearCellsHighlight();
       
    }

    //highlight cell
    protected void ShowCellsHighlight()
    {
        foreach (Cell cell in highlightedCells)
            cell.outlineImage.enabled = true;
    }
    //clear highligh cell
    protected void ClearCellsHighlight()
    {
        foreach (Cell cell in highlightedCells)
            cell.outlineImage.enabled = false;

        highlightedCells.Clear();
    }

    public void Reset()
    {
        Kill();
        PlaceInit(originalCell);
    }


    public bool isCheckVerif(bool AttakingSideIsWhite)
    {
        foreach (List<Cell> row in currentCell.board.allCells)
        {
            foreach(Cell boardCell in row)
            {
                BasePiece pieceBoard = boardCell.currentPiece;
                if(pieceBoard != null && pieceBoard.isWhite == AttakingSideIsWhite)
                {
                    King targetKing = pieceManager.getKing(!AttakingSideIsWhite);

                    pieceBoard.CheckPathing();
                    foreach (Cell cell in pieceBoard.attackedCells)
                    {
                        if(cell.boardPosition.x == targetKing.currentCell.boardPosition.x &&
                            cell.boardPosition.y == targetKing.currentCell.boardPosition.y)
                        {
                            pieceBoard.ClearAttackedCell();
                            return true;
                        }
                    }
                    pieceBoard.ClearAttackedCell();
                }
            }
        }
        return false;
    }
    
    public void ClearAttackedCell()
    {
        attackedCells.Clear();
    }

   
}
