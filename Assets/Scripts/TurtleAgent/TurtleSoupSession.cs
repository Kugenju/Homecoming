using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TurtleSoupSession
{
    public TurtleSoupPuzzle currentPuzzle;
    public int questionCount = 0;
    public bool hasDeclared = false;
    public int finalQuestionIndex = 0; // 0=未开始, 1=第一问, 2=第二问
    public bool isGameOver = false;

    public void Reset()
    {
        questionCount = 0;
        hasDeclared = false;
        finalQuestionIndex = 0;
        isGameOver = false;
    }
}