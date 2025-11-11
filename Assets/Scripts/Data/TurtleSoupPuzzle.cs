using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Turtle Soup", menuName = "Game/Turtle Soup Puzzle")]
public class TurtleSoupPuzzle : ScriptableObject
{
    public string soupSurface;      // Ã¿√Ê
    public string soupBottom;       // Ã¿µ◊
    public string hint1;
    public string hint2;

    [TextArea] public string finalQuestion1;
    public string[] correctAnswers1;

    [TextArea] public string finalQuestion2;
    public string[] correctAnswers2;
}
