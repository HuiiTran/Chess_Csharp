using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    System.Diagnostics.Process process = null;
    public static int level = 0;
    string lastFEN;

    public static Dictionary<int, int> AI_Level = new Dictionary<int, int>()
    {
        {0,0},
        {1,5},
        {2,20}
    };

    public static Dictionary<int, int> AI_Game_Level = new Dictionary<int, int>()
    {
        {0,1},
        {5,2},
        {20,3}
    };

    public void Setup()
    {
        process = new System.Diagnostics.Process();
        process.StartInfo.FileName = Application.dataPath + "/Resources/AI/stockfish_13/stockfish_13_win_x64.exe";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        process.StandardInput.WriteLine("setoption Name Skill Level value " + level);
        process.StandardInput.WriteLine("position startpos");

        lastFEN = GetFEN();
    }

    public void Close()
    {
        process.Close();
    }

    public string GetBestMove()
    {
        string setupString = "position fen " + lastFEN;
        process.StandardInput.WriteLine(setupString);

        string processString = "go movetime 1";
        process.StandardInput.WriteLine(processString);

        string bestMoveInAlgebraicNotation = "";
        do
        {
            bestMoveInAlgebraicNotation = process.StandardOutput.ReadLine();
        } while (!bestMoveInAlgebraicNotation.Contains("bestmove"));

        bestMoveInAlgebraicNotation = bestMoveInAlgebraicNotation.Substring(9, 4);

        return bestMoveInAlgebraicNotation;
    }

    public string GetFEN()
    {
        process.StandardInput.WriteLine("d");
        string output = "";
        do
        {
            output = process.StandardOutput.ReadLine();
        }
        while (!output.Contains("Fen"));

        output = output.Substring(5);
        return output;
    }

    public void setAImove(string move)
    {
        string setupString = "position fen " + lastFEN + " moves " + move;
        process.StandardInput.WriteLine(setupString);
        lastFEN = GetFEN();
    }
}
