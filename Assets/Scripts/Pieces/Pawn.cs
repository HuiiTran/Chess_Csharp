using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pawn : BasePiece
{
    public override void Setup(bool newIsWhite, PieceManager newPM)
    {
        base.Setup(newIsWhite, newPM);
        movement = isWhite ? new Vector3Int(0, 1, 1) : new Vector3Int(0, -1, -1);
        hasMoved = false;
        GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/WhitePawn");

    }
}
