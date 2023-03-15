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
    
    public override void Kill()
    {
        base.Kill();

        pieceManager.isKingAlive = false;
    }
}
