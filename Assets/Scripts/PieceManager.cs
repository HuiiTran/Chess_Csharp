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
    PAT,
    NULLE
}
public class PieceManager : MonoBehaviour
{
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

    private string[] pieceOrder = { "P", "P", "P", "P", "P", "P", "P", "P",
        "R", "KN", "B", "Q", "K", "B", "KN", "R" };


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

    
    private void SetInteractive(List<BasePiece> pieces, bool state)
    {
        foreach(BasePiece piece in pieces)
        {
            if (piece.inDrag)
                piece.OnEndDrag(null);
            piece.enabled = state;
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


        SetInteractive(whitePieces, true);            
 
    }
    public King getKing(bool isWhite)
    {
        if (isWhite)
            return whiteKing;
        else
            return blackKing;
    }
}
