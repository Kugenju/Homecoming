using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveSystem
{
    private const string KEY_UNLOCKED_CHAPTER = "UnlockedChapter";

    public static void UnlockChapter(int chapter)
    {
        PlayerPrefs.SetInt(KEY_UNLOCKED_CHAPTER, chapter);
        PlayerPrefs.Save();
    }

    public static int GetLastUnlockedChapter()
    {
        return PlayerPrefs.GetInt(KEY_UNLOCKED_CHAPTER, 0);
    }
}