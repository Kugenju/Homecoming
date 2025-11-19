using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TurtleSoupGameManager : MonoBehaviour
{
    public TurtleSoupPuzzle[] allPuzzles;
    public TurtleSoupSession session = new TurtleSoupSession();

    public delegate void OnMessage(string message);
    public static event OnMessage OnNewMessage;

    public void OnPlayerWin() => MiniGameEvents.OnMiniGameFinished?.Invoke(true);
    public void OnPlayerLose() => MiniGameEvents.OnMiniGameFinished?.Invoke(false);

    private void Start()
    {
        if (allPuzzles.Length == 0)
        {
            Debug.LogError("未配置海龟汤题库！");
        }
    }

    public void HandleUserInput(string input)
    {
        if (session.isGameOver) return;

        input = input.Trim();

        if (input == "开始游戏")
        {
            StartNewGame();
            return;
        }

        if (session.hasDeclared)
        {
            HandleFinalAnswer(input);
            return;
        }

        session.questionCount++;

        // 第10问给提示
        if (session.questionCount == 10)
        {
            string hint = Random.value > 0.5f ? session.currentPuzzle.hint1 : session.currentPuzzle.hint2;
            SendGameMessage($"提示：{hint}");
        }

        // 超过15问失败
        if (session.questionCount > 15)
        {
            EndGame(false);
            return;
        }

        if (input.Contains("我知道了"))
        {
            session.hasDeclared = true;
            AskFinalQuestion(1);
            return;
        }

        // 调用 Qwen 判断
        StartCoroutine(QwenAPI.Instance.JudgeYesNo(input, session.currentPuzzle.soupBottom, (answer) =>
        {
            SendGameMessage(answer);
        }));
    }

    //    private void StartNewGame()
    //    {
    //        session.Reset();
    //        session.currentPuzzle = allPuzzles[Random.Range(0, allPuzzles.Length)];

    //        string intro = $@"汤面：
    //{session.currentPuzzle.soupSurface}

    //现在请你通过提问，还原出故事的全貌。注意，问题的答案只能为'是’或‘否’，否则，我会拒绝回答。在你觉得已经还原全貌后，请对我说“我知道了。”我会问你两个问题，如果你全部答对，则通关；只要答错一个，你就输了。如果你问了十五个问题还没能对我说“我知道了。”那你也输啦！";

    //        SendGameMessage(intro);
    //    }
    private void StartNewGame()
    {
        session.Reset();
        session.currentPuzzle = allPuzzles[Random.Range(0, allPuzzles.Length)];

        // 更新左侧面板的汤面
        var ui = FindObjectOfType<TurtleSoupUI>();
        ui?.UpdateSoupSurface(session.currentPuzzle.soupSurface);

        string intro = @"选好题目了吗，那我要开始喽。记住：只能问是非问题，否则你就要浪费一次机会了，嘿嘿。";
        SendGameMessage(intro);
    }

    private void AskFinalQuestion(int index)
    {
        session.finalQuestionIndex = index;
        if (index == 1)
        {
            SendGameMessage(session.currentPuzzle.finalQuestion1);
        }
        else if (index == 2)
        {
            SendGameMessage(session.currentPuzzle.finalQuestion2);
        }
    }

    private void HandleFinalAnswer(string answer)
    {
        bool isCorrect = false;
        if (session.finalQuestionIndex == 1)
        {
            isCorrect = IsAnswerMatch(answer, session.currentPuzzle.correctAnswers1);
            if (isCorrect)
            {
                AskFinalQuestion(2);
            }
            else
            {
                EndGame(false);
            }
        }
        else if (session.finalQuestionIndex == 2)
        {
            isCorrect = IsAnswerMatch(answer, session.currentPuzzle.correctAnswers2);
            EndGame(isCorrect);
        }
    }

    private bool IsAnswerMatch(string userAnswer, string[] correctList)
    {
        string cleanInput = userAnswer.Trim().ToLower();
        foreach (string correct in correctList)
        {
            if (cleanInput.Contains(correct.ToLower()) || correct.ToLower().Contains(cleanInput))
                return true;
        }
        return false;
    }

    private void EndGame(bool win)
    {
        session.isGameOver = true;
        if (win)
        {
            SendGameMessage("你赢了。\n\n汤底：" + session.currentPuzzle.soupBottom);
            //等待五秒后触发胜利事件
            StartCoroutine(DelayedWin(5f));
        }
        else
        {
            SendGameMessage("你输了。\n\n汤底：" + session.currentPuzzle.soupBottom);
            OnPlayerLose();
        }
    }

    private System.Collections.IEnumerator DelayedWin(float second)
    {
        yield return new WaitForSeconds(second);
        OnPlayerWin();
    }

    private void SendGameMessage(string msg)
    {
        OnNewMessage?.Invoke(msg);
        Debug.Log("[海龟汤] " + msg);
    }
}