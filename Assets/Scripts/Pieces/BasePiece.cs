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
    protected List<Cell> previousCells = new List<Cell>();

    private Cell targetCell = null;

    public bool inDrag = false;
    public bool inClick = false;

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
        inClick = false;
        inDrag = false;
        pieceManager = newPM;
        isWhite = newIsWhite;
        hasMoved = false;

        rt = GetComponent<RectTransform>();

        //Set color
        if(isWhite)
            GetComponent<Image>().color = Color.white;
        else
            GetComponent<Image>().color = new Color(0, 0, 0,(float)0.65);
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
            clip = (AudioClip)Resources.Load("Sounds/move-self");
        }
        else
        {
            clip = (AudioClip)Resources.Load("Sounds/capture");
        }
        // Disable check
        if (pieceManager.getKing(isWhite).isCheck)
        {
            pieceManager.getKing(isWhite).setCheck(false);
        }

        targetCell.RemovePiece();

        bool castling = false;
        //  castle
        if (currentCell.currentPiece.GetType() == typeof(King) && currentCell.currentPiece.hasMoved == false)
        {
            if(targetCell.boardPosition.x == 2)
            {
                BasePiece rook = currentCell.board.allCells[0][currentCell.boardPosition.y].currentPiece;
                rook.targetCell = currentCell.board.allCells[3][currentCell.boardPosition.y];
                rook.Move();
                castling = true;
            }
            else if(targetCell.boardPosition.x == 6)
            {
                BasePiece rook = currentCell.board.allCells[7][currentCell.boardPosition.y].currentPiece;
                rook.targetCell = currentCell.board.allCells[5][currentCell.boardPosition.y];
                rook.Move();
                castling = true;
            }
        }

        currentCell.currentPiece = null;
        currentCell = targetCell;
        currentCell.currentPiece = this;

        transform.position = currentCell.transform.position;
        targetCell = null;

        hasMoved = true;

        if(pieceManager.enPassantCell != null)
        {
            pieceManager.enPassantCell.enPassant = null;
            pieceManager.enPassantCell = null;
        }

        pieceManager.checkVerificationInProcess = true;
        if (isCheckVerif(isWhite))
        {
            pieceManager.getKing(!isWhite).setCheck(true);
            clip = (AudioClip)Resources.Load("Sounds/checksound");
        }
        pieceManager.checkVerificationInProcess = false;

        //check if checkmate
        CheckGameOver(!isWhite);

        //sound
        if (pieceManager.gameState != GameState.INGAME)
            clip = null;
        if (clip != null)
            pieceManager.audio.PlayOneShot(clip);

        // change turn
        if (!pieceManager.AITurn && pieceManager.gameState == GameState.INGAME && !castling)
        {
            pieceManager.SetTurn(!isWhite);
        }
    }

    public bool PossibleMove(bool isWhite)
    {
        foreach (List<Cell> row in currentCell.board.allCells)
        {
            foreach (Cell boardCell in row)
            {
                BasePiece piece = boardCell.currentPiece;
                if (piece != null && piece.isWhite == isWhite)
                {
                    piece.CheckPathing();
                    if (piece.highlightedCells.Count > 0)
                    {
                        piece.highlightedCells.Clear();
                        return true;
                    }
                    piece.highlightedCells.Clear();
                }
            }
        }
        return false;
    }
    
    //add cell to list
    protected void addPossibleCell(Cell possibleCell)
    {
        if (pieceManager.checkVerificationInProcess)
            attackedCells.Add(possibleCell);
        else
            highlightedCells.Add(possibleCell);
    }
    // //previous move
    // protected void addPreviousCell(Cell previousCell)
    // {
    //     previousCells.Add(previousCell);
    // }

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
                        targeted.outlineImage.GetComponent<Image>().color = new Color(1, 0, 0, (float)0.5);//red
                    }
                    else
                    {
                        targeted.outlineImage.GetComponent<Image>().color = new Color(0, 1, 0, (float)0.5);//green
                    }

                    
                }
                addPossibleCell(targeted);
                
            } 
            if (state == CellState.ENEMY || state == CellState.FRIEND || state == CellState.CHECK_ENEMY || state == CellState.CHECK_FRIEND)
                break;  
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


    // //show previous move
    // protected void ShowPreviousHightlight()
    // {
    //     foreach (Cell cell in previousCells)
    //         cell.outlineImage.enabled = true;
    // }
    // //clear privous move
    // protected void ClearPreviousHightlight()
    // {
    //     foreach (Cell cell in previousCells)
    //         cell.outlineImage.enabled = false;
    //     previousCells.Clear();
    // }


    //click
    // public override void OnPointerDown(PointerEventData data)
    // {
        
    //     inClick = true;
    //     // Test for cells
    //     CheckPathing();

    //     // Show valid cells
    //     ShowCellsHighlight();
 
    // }


    // public override void OnPointerExit(PointerEventData data)
    // {
    //     inClick = false;
    //     ClearCellsHighlight();
    // }

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

        
        if (!targetCell || pieceManager.gameState != GameState.INGAME)
        {
            transform.position = currentCell.transform.position; 
        }
        else
        {
            if (PieceManager.AImode)
            {
                string move = "";

                move += pieceManager.posA[currentCell.boardPosition.x];
                move += pieceManager.posB[currentCell.boardPosition.y];
                
               
                move += pieceManager.posA[targetCell.boardPosition.x];
                move += pieceManager.posB[targetCell.boardPosition.y];

                

                // If promotion
                if (this.GetType() == typeof(Pawn) && (TargetCell.boardPosition.y == 0 || TargetCell.boardPosition.y == 7))
                {
                    move += "q";
                }
                Debug.Log(move);
                pieceManager.stockfish.setAImove(move);
                Move();

            }
            else
            {
                Move(); 
            }     
        }
        ClearCellsHighlight();
       
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

    public void CheckGameOver(bool isWhite)
    {
        if( !PossibleMove(isWhite))
        {
            if(pieceManager.getKing(isWhite).isCheck)
            {
                if (isWhite)
                {
                    pieceManager.gameState = GameState.BLACK_WIN;
                }
                else
                {
                    pieceManager.gameState = GameState.WHITE_WIN;
                }
            }
            else
            {
                pieceManager.gameState = GameState.DRAW;
            }
            //Debug.Log(pieceManager.gameState);
            pieceManager.ShowResult();
        }
    }

}
