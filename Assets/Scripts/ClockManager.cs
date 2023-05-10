using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClockManager : MonoBehaviour
{
    [HideInInspector]
    public bool launched = false;

    private Timer clockWhite;
    private Timer clockBlack;

    public TMP_Text displayWhite;
    public TMP_Text displayBlack;

    public GameObject highlightClockW;
    public GameObject highlightClockB;

    private PieceManager pm = null;

    private bool isWhiteTurn = true;

    
    public void Setup(float whiteTime, float blackTime, PieceManager newPm)
    {

        pm = newPm;
        launched = false;

        clockWhite = new Timer();
        clockBlack = new Timer();

        clockWhite.Setup(whiteTime, displayWhite);
        clockBlack.Setup(blackTime, displayBlack);

        highlightClockW.SetActive(true);
        highlightClockB.SetActive(false);
        

        highlightClockW.GetComponent<Image>().color =  new Color((float)0.36, (float)0.68, (float)0.27, 1);
        highlightClockB.GetComponent<Image>().color =  new Color((float)0.36, (float)0.68, (float)0.27, 1);
    }

    public void StartClocks()
    {
        clockWhite.Start();
        clockBlack.Start();
        launched = true;
    }
    

    // Update is called once per frame
    void Update()
    {
        if(launched)
        {
            if (isWhiteTurn == true)
            {
                clockWhite.Update();
            }
            else
            {
                clockBlack.Update();
            }
            if (clockBlack.runOut)
            {
                pm.gameState = GameState.WHITE_WIN;
                pm.ShowResult();
                launched = false;
            }
            if (clockWhite.runOut)
            {
                pm.gameState = GameState.BLACK_WIN;
                pm.ShowResult();
                launched = false;
            }
        }
    }

    public void StopClocks()
    {
        clockWhite.Stop();
        clockBlack.Stop();        
    }
    public void setTurn(bool isWhiteTurn)
    {
        this.isWhiteTurn = isWhiteTurn;
        highlightClockW.SetActive(isWhiteTurn);
        highlightClockB.SetActive(!isWhiteTurn);
    }
    public void changeTurn()
    {
        isWhiteTurn = !isWhiteTurn;
        highlightClockW.SetActive(!highlightClockW.activeSelf);
        highlightClockB.SetActive(!highlightClockB.activeSelf);
        
    }
}
