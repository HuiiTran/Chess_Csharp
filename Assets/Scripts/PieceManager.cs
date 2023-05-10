using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GameState
{
    INGAME,
    WHITE_WIN,
    BLACK_WIN,
    DRAW,
    NULL
}
public class PieceManager : MonoBehaviour
{

    public TMP_Text result;
    [HideInInspector]
    public bool isKingAlive;

    private Board chessBoard;

    [HideInInspector]
    public GameState gameState;

    [HideInInspector]
    public new AudioSource audio;

    public GameObject piecePrefab;

    private List<BasePiece> whitePieces = null;
    private List<BasePiece> blackPieces = null;

    [HideInInspector]
    public Cell enPassantCell = null;

    [HideInInspector]
    public King whiteKing = null;

    [HideInInspector]
    public King blackKing = null;

    [HideInInspector]
    public bool checkVerificationInProcess = false;

    public static int promotionStatus = 0;
    public static bool isBeingPromotion = true;

    public ClockManager clockManager;
    public static float blackTime = 90;
    public static float whiteTime = 90;

    public static bool AImode = true; //true;
    public static bool isAIWhite = false;
    public AI stockfish = null;

    [HideInInspector]
    public bool AITurn = false;




    private string[] pieceOrder = { "P", "P", "P", "P", "P", "P", "P", "P",
        "R", "KN", "B", "Q", "K", "B", "KN", "R" };
    // private string[] pieceOrder = { "P", "P", "P", "P", "P", "P", "P", "P",
    //     "R", "KN", "B", "Q", "K", "B", "KN", "R" };


    private Dictionary<string, Type> pieceDico = new Dictionary<string, Type>()
    {
        {"P", typeof(Pawn)},
        {"R", typeof(Rook)},
        {"KN", typeof(Knight)},
        {"B", typeof(Bishop)},
        {"K", typeof(King)},
        {"Q", typeof(Queen)}
    };
    public Dictionary<string, int> coordA = new Dictionary<string, int>()
    {
        {"a", 0},
        {"b", 1},
        {"c", 2},
        {"d", 3},
        {"e", 4},
        {"f", 5},
        {"g", 6},
        {"h", 7}
    };

    public Dictionary<string, int> coordB = new Dictionary<string, int>()
    {
        {"1", 0},
        {"2", 1},
        {"3", 2},
        {"4", 3},
        {"5", 4},
        {"6", 5},
        {"7", 6},
        {"8", 7}
    };

    public Dictionary<int, string> posA = new Dictionary<int, string>()
    {
        {0, "a"},
        {1, "b"},
        {2, "c"},
        {3, "d"},
        {4, "e"},
        {5, "f"},
        {6, "g"},
        {7, "h"},
    };

    public Dictionary<int, string> posB = new Dictionary<int, string>()
    {
        {0, "1"},
        {1, "2"},
        {2, "3"},
        {3, "4"},
        {4, "5"},
        {5, "6"},
        {6, "7"},
        {7, "8"},
    };
    

    //AI mode
    private IEnumerator showAIMoveCoroutine()
    {
        AITurn = true;
        string best = stockfish.GetBestMove();
        yield return new WaitForSeconds((float)2);

        string depA = best.Substring(0,1);
        string depB = best.Substring(1,1);
        string arrA = best.Substring(2,1);
        string arrB = best.Substring(3,1);

        Cell dep = chessBoard.allCells[coordA[depA]][coordB[depB]];
        Cell targ = chessBoard.allCells[coordA[arrA]][coordB[arrB]];
        //AI promotion to queen
        if(dep.currentPiece.GetType() == typeof(Pawn) && (coordB[arrB] == 0 || coordB[arrB] == 7))
        {
            best += "q";
        }

        Debug.Log(best);

        stockfish.setAImove(best);

        dep.currentPiece.TargetCell = targ;
        dep.currentPiece.Move();
        AITurn = false;

        if (GameState.INGAME == gameState)
        {
            if (isAIWhite)
            {
                SetInteractive(blackPieces, true);
            }
            else
            {
                SetInteractive(whitePieces, true);
            }
            clockManager.setTurn(!isAIWhite);
        }
    }

    //
    private List<BasePiece> CreatePieces(bool isWhite, Board board)
    {
        List<BasePiece> pieceList = new List<BasePiece>();

        float board_width = board.GetComponent<RectTransform>().rect.width;
        float board_height = board.GetComponent<RectTransform>().rect.height;

        for (int i = 0; i < pieceOrder.Length; i++)
        {
            GameObject newPieceObject = Instantiate(piecePrefab);
            newPieceObject.transform.SetParent(transform);

            newPieceObject.transform.localScale = new Vector3(1, 1, 1);
            newPieceObject.transform.localRotation = Quaternion.identity;

            
            float piece_width = board_width / board.Column - BasePiece.CellPadding;
            float piece_height = board_height / board.Row - BasePiece.CellPadding;
            newPieceObject.GetComponent<RectTransform>().sizeDelta = new Vector2(piece_width, piece_height);
            

            string key = pieceOrder[i];
            Type pieceType = pieceDico[key];

            BasePiece newPiece = (BasePiece)newPieceObject.AddComponent(pieceType);
            pieceList.Add(newPiece);

            if (pieceDico[key] == typeof(King))
            {
                if (isWhite)
                    whiteKing = (King) newPiece;
                else
                    blackKing = (King) newPiece;
            }

            newPiece.Setup(isWhite, this);
        }

        return pieceList;
    }
    //Place piece on board
    private void PlacePieces(string pawnRow, string royaltyRow, List<BasePiece> pieces, Board board)
    {
        for (int i = 0; i < board.Column; i++)
        {
            pieces[i].PlaceInit(board.allCells[i][coordB[pawnRow]]);
            pieces[i + 8].PlaceInit(board.allCells[i][coordB[royaltyRow]]);
        }
    }


    //set up
    public void Setup(Board board)
    {
        audio = gameObject.AddComponent<AudioSource>();

        chessBoard = board;
        gameState = GameState.INGAME;

        

        isKingAlive = true;

        whitePieces = CreatePieces(true, chessBoard);
        blackPieces = CreatePieces(false, chessBoard);

        PlacePieces("2", "1", whitePieces, board);
        PlacePieces("7", "8", blackPieces, board);

        SetInteractive(whitePieces, false);
        SetInteractive(blackPieces, false);

        enPassantCell = null;
        checkVerificationInProcess = false;
        clockManager.Setup(whiteTime, blackTime, this);
        
        if(AImode)
        {
            stockfish.Setup();
            if (isAIWhite)
            {
                StartCoroutine(showAIMoveCoroutine());
                clockManager.displayBlack.text = "Player";
                clockManager.displayWhite.text = "AI level" + AI.AI_Game_Level[AI.level];
            }
            else
            {
                SetInteractive(whitePieces, true);
                clockManager.displayBlack.text = "AI level" + AI.AI_Game_Level[AI.level];
                clockManager.displayWhite.text = "Player";
            }
        }
        else
            SetInteractive(whitePieces, true);            
 
    }

    public void ResetGame()
    {
        if (AITurn)
            return;
        gameState = GameState.INGAME;

        result.text = "";

        foreach (List<Cell> row in chessBoard.allCells)
        {
            foreach (Cell boardCell in row)
            {
                boardCell.outlineImage.enabled = false;
                if (boardCell.currentPiece != null)
                {
                    boardCell.currentPiece.Kill();
                }
                boardCell.enPassant = null;
            }
        }

        whitePieces.Clear();
        blackPieces.Clear();

        whitePieces = CreatePieces(true, chessBoard);
        blackPieces = CreatePieces(false, chessBoard);

       
            
        PlacePieces("2", "1", whitePieces, chessBoard);
        PlacePieces("7", "8", blackPieces, chessBoard);

        SetInteractive(whitePieces, false);
        SetInteractive(blackPieces, false);

        enPassantCell = null;
        isKingAlive = true;

        clockManager.Setup(whiteTime, blackTime, this);

        checkVerificationInProcess = false;

        if(AImode)
        {
            stockfish.Close();
            stockfish.Setup();
            if (isAIWhite)
            {
                StartCoroutine(showAIMoveCoroutine());
                clockManager.displayBlack.text = "Player";
                clockManager.displayWhite.text = "AI level" + AI.AI_Game_Level[AI.level];
            }
            else
            {
                SetInteractive(whitePieces, true);
                clockManager.displayBlack.text = "AI level" + AI.AI_Game_Level[AI.level];
                clockManager.displayWhite.text = "Player";
            }
        }
        else
            SetInteractive(whitePieces, true);
    }


    //make piece dragable
    private void SetInteractive(List<BasePiece> pieces, bool state)
    {
        foreach(BasePiece piece in pieces)
        {
            if (piece.inDrag)
                piece.OnEndDrag(null);
            piece.enabled = state;
        }
    }
    public King getKing(bool isWhite)
    {
        if (isWhite)
            return whiteKing;
        else
            return blackKing;
    }

    //create turn
    public void SetTurn(bool isWhiteTurn)
    {
        if (AImode)
        {
            clockManager.setTurn(isWhiteTurn);
            SetInteractive(whitePieces, false);
            SetInteractive(blackPieces, false);
            StartCoroutine(showAIMoveCoroutine());
        }
        else
        {
            if(isKingAlive == false)
                return;
            
            SetInteractive(whitePieces, isWhiteTurn);
            SetInteractive(blackPieces, !isWhiteTurn);
            
            if (clockManager.launched == false)
            {
                clockManager.StartClocks();
            }
            clockManager.setTurn(isWhiteTurn);
        }
    }

    public void ShowResult()
    {
        audio.PlayOneShot((AudioClip)Resources.Load("Sounds/wingame"));
        SetInteractive(whitePieces,false);
        SetInteractive(blackPieces,false);

        clockManager.StopClocks();

        clockManager.highlightClockW.SetActive(false);
        clockManager.highlightClockB.SetActive(false);

        result.enabled = false;

        StartCoroutine(showResultCoroutine());
    }

    private IEnumerator showResultCoroutine()
    {
        yield return new WaitForSeconds((float)2.1);
        if (gameState == GameState.BLACK_WIN)
        {
            result.text = "Black wins";
            clockManager.highlightClockB.SetActive(true);
            clockManager.highlightClockB.GetComponent<Image>().color = new Color(1, (float)0.6816, 0, 1);
        }
        if (gameState == GameState.WHITE_WIN)
        {
            result.text = "White wins";
            clockManager.highlightClockW.SetActive(true);
            clockManager.highlightClockW.GetComponent<Image>().color = new Color(1, (float)0.6816, 0, 1);

        }
        if (gameState == GameState.DRAW)
        {
            result.text = "DRAW !";
        }
        result.enabled = true;
    }




    private IEnumerator waitToPromo(Pawn pawn, Cell BeforeCell)
    {
        yield return new WaitUntil(()=> isBeingPromotion == false);
        // Debug.Log("hi");
        ChangeToPiece(pawn, BeforeCell);
        promotionStatus = 0;
        isBeingPromotion = true;
    }
    //Pawn promotion to queen ? or to other pieces
    [SerializeField] GameObject pauseMenu;
    public void ChangeToPiece(Pawn pawn, Cell BeforeCell)
    {
        switch(promotionStatus)
        {
            case 1:
                {
                pawn.currentCell.RemovePiece();
                GameObject newPieceObject = Instantiate(piecePrefab);
                newPieceObject.transform.SetParent(transform);

                newPieceObject.transform.localScale = new Vector3(1, 1, 1);
                newPieceObject.transform.localRotation = Quaternion.identity;

                float board_width = BeforeCell.board.GetComponent<RectTransform>().rect.width;
                float board_height = BeforeCell.board.GetComponent<RectTransform>().rect.height;

                float piece_width = board_width / BeforeCell.board.Column - BasePiece.CellPadding;
                float piece_height = board_height / BeforeCell.board.Row - BasePiece.CellPadding;
                newPieceObject.GetComponent<RectTransform>().sizeDelta = new Vector2(piece_width, piece_height);
                Queen queen = (Queen)newPieceObject.AddComponent(typeof(Queen));

                //base.pieceManager.pieceList.Add(newPiece);//

                queen.Setup(pawn.isWhite, this);
                
                //queen.PlaceInit(promotionCell);//

                queen.TargetCell = pawn.currentCell;
                queen.currentCell = BeforeCell;
                queen.currentCell.currentPiece = queen;
                queen.Move();
                if (pawn.isWhite)
                {
                    whitePieces.Remove(pawn);
                    whitePieces.Add(queen);
                } 
                else
                {
                    blackPieces.Remove(pawn);
                    blackPieces.Add(queen);
                }
                queen.gameObject.SetActive(true);   

                if (pawn.isWhite)
                {
                    SetInteractive(whitePieces,false);
                }
                else
                {
                    SetInteractive(blackPieces, false);
                }  
                break;
                }
            case 2:
                {
                pawn.currentCell.RemovePiece();

                GameObject newPieceObject = Instantiate(piecePrefab);
                newPieceObject.transform.SetParent(transform);

                newPieceObject.transform.localScale = new Vector3(1, 1, 1);
                newPieceObject.transform.localRotation = Quaternion.identity;

                float board_width = BeforeCell.board.GetComponent<RectTransform>().rect.width;
                float board_height = BeforeCell.board.GetComponent<RectTransform>().rect.height;

                float piece_width = board_width / BeforeCell.board.Column - BasePiece.CellPadding;
                float piece_height = board_height / BeforeCell.board.Row - BasePiece.CellPadding;
                newPieceObject.GetComponent<RectTransform>().sizeDelta = new Vector2(piece_width, piece_height);

                Knight knight = (Knight)newPieceObject.AddComponent(typeof(Knight));
                
                //base.pieceManager.pieceList.Add(newPiece);//


                knight.Setup(pawn.isWhite, this);
                //rook.PlaceInit(promotionCell);//
                knight.TargetCell = pawn.currentCell;
                knight.currentCell = BeforeCell;
                knight.currentCell.currentPiece = knight;
                knight.Move();
                if (pawn.isWhite)
                {
                    whitePieces.Remove(pawn);
                    whitePieces.Add(knight);
                } 
                else
                {
                    blackPieces.Remove(pawn);
                    blackPieces.Add(knight);
                }
                knight.gameObject.SetActive(true);   

                if (pawn.isWhite)
                {
                    SetInteractive(whitePieces,false);
                }
                else
                {
                    SetInteractive(blackPieces, false);
                }  
                break;
                }
            case 3:
            {
                {
                pawn.currentCell.RemovePiece();

                GameObject newPieceObject = Instantiate(piecePrefab);
                newPieceObject.transform.SetParent(transform);

                newPieceObject.transform.localScale = new Vector3(1, 1, 1);
                newPieceObject.transform.localRotation = Quaternion.identity;

                float board_width = BeforeCell.board.GetComponent<RectTransform>().rect.width;
                float board_height = BeforeCell.board.GetComponent<RectTransform>().rect.height;

                float piece_width = board_width / BeforeCell.board.Column - BasePiece.CellPadding;
                float piece_height = board_height / BeforeCell.board.Row - BasePiece.CellPadding;
                newPieceObject.GetComponent<RectTransform>().sizeDelta = new Vector2(piece_width, piece_height);

                Bishop bishop = (Bishop)newPieceObject.AddComponent(typeof(Bishop));
                
                //base.pieceManager.pieceList.Add(newPiece);//


                bishop.Setup(pawn.isWhite, this);
                //rook.PlaceInit(promotionCell);//
                bishop.TargetCell = pawn.currentCell;
                bishop.currentCell = BeforeCell;
                bishop.currentCell.currentPiece = bishop;
                bishop.Move();
                if (pawn.isWhite)
                {
                    whitePieces.Remove(pawn);
                    whitePieces.Add(bishop);
                } 
                else
                {
                    blackPieces.Remove(pawn);
                    blackPieces.Add(bishop);
                }
                bishop.gameObject.SetActive(true);    
                if (pawn.isWhite)
                {
                    SetInteractive(whitePieces,false);
                }
                else
                {
                    SetInteractive(blackPieces, false);
                } 
                break;
                }
            }
            case 4:
                {
                pawn.currentCell.RemovePiece();

                GameObject newPieceObject = Instantiate(piecePrefab);
                newPieceObject.transform.SetParent(transform);

                newPieceObject.transform.localScale = new Vector3(1, 1, 1);
                newPieceObject.transform.localRotation = Quaternion.identity;

                float board_width = BeforeCell.board.GetComponent<RectTransform>().rect.width;
                float board_height = BeforeCell.board.GetComponent<RectTransform>().rect.height;

                float piece_width = board_width / BeforeCell.board.Column - BasePiece.CellPadding;
                float piece_height = board_height / BeforeCell.board.Row - BasePiece.CellPadding;
                newPieceObject.GetComponent<RectTransform>().sizeDelta = new Vector2(piece_width, piece_height);

                Rook rook = (Rook)newPieceObject.AddComponent(typeof(Rook));
                
                //base.pieceManager.pieceList.Add(newPiece);//


                rook.Setup(pawn.isWhite, this);
                //rook.PlaceInit(promotionCell);//
                rook.TargetCell = pawn.currentCell;
                rook.currentCell = BeforeCell;
                rook.currentCell.currentPiece = rook;
                rook.Move();
                if (pawn.isWhite)
                {
                    whitePieces.Remove(pawn);
                    whitePieces.Add(rook);
                } 
                else
                {
                    blackPieces.Remove(pawn);
                    blackPieces.Add(rook);
                }
                rook.gameObject.SetActive(true);
                
                if (pawn.isWhite)
                {
                    SetInteractive(whitePieces,false);
                }
                else
                {
                    SetInteractive(blackPieces, false);
                }
                break;
                }
            default:
                {
                pawn.currentCell.RemovePiece();
                GameObject newPieceObject = Instantiate(piecePrefab);
                newPieceObject.transform.SetParent(transform);

                newPieceObject.transform.localScale = new Vector3(1, 1, 1);
                newPieceObject.transform.localRotation = Quaternion.identity;

                float board_width = BeforeCell.board.GetComponent<RectTransform>().rect.width;
                float board_height = BeforeCell.board.GetComponent<RectTransform>().rect.height;

                float piece_width = board_width / BeforeCell.board.Column - BasePiece.CellPadding;
                float piece_height = board_height / BeforeCell.board.Row - BasePiece.CellPadding;
                newPieceObject.GetComponent<RectTransform>().sizeDelta = new Vector2(piece_width, piece_height);
                Queen queen = (Queen)newPieceObject.AddComponent(typeof(Queen));

                //base.pieceManager.pieceList.Add(newPiece);//

                queen.Setup(pawn.isWhite, this);
                
                //queen.PlaceInit(promotionCell);//

                queen.TargetCell = pawn.currentCell;
                queen.currentCell = BeforeCell;
                queen.currentCell.currentPiece = queen;
                queen.Move();
                if (pawn.isWhite)
                {
                    whitePieces.Remove(pawn);
                    whitePieces.Add(queen);
                } 
                else
                {
                    blackPieces.Remove(pawn);
                    blackPieces.Add(queen);
                }
                queen.gameObject.SetActive(true);   

                if (pawn.isWhite)
                {
                    SetInteractive(whitePieces,false);
                }
                else
                {
                    SetInteractive(blackPieces, false);
                }       
                break;
                }
        }
    }
    
    public void PawnPromotion(Pawn pawn, Cell BeforeCell)
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        
        StartCoroutine(waitToPromo(pawn, BeforeCell));
        
        promotionStatus = 0;
        isBeingPromotion = true;
    }
}
