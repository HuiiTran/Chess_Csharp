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
}
