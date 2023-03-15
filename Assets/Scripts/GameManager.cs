using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Board board;

    public PieceManager pieceManager;
    // Start is called before the first frame update
    void Start()
    {
        board.Create();

        pieceManager.Setup(board);
    }

    
}
