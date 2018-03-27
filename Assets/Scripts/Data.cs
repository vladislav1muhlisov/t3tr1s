using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Data : MonoBehaviour
{
    #region References
    public Text levelValueText;
    public Text scoreValueText;
    public Text timeValueText;
    public Text linesValueText;
    #endregion

    private int currentLevel;
    private int currentScore;
    private float currentLevelTime;
    private int filledLinesCount;

    private DisplayManager displayManager;

    private static Data instance;

    public const int MAX_LEVEL = 9;
    private const int POINTS_PER_LINE = 20;
    private const int POINTS_PER_LINE_BONUS = 10;

    private void Awake()
    {
        displayManager = DisplayManager.Instance();
    }

    public static Data Instance()
    {
        if (!instance)
        {
            instance = FindObjectOfType<Data>();
            if (!instance)
            {
                Debug.LogError("There is no data object on the scene!");
                return null;
            }
        }
        return instance;
    }

    public int CurrentLevel
    {
        get { return currentLevel; }
        set
        {
            if (value <= 0) currentLevel = 1;
            else if (value > MAX_LEVEL) currentLevel = MAX_LEVEL;
            else
            {
                if (currentLevel < value) displayManager.DisplayMessage("Level UP!");
                else if (currentLevel > value) displayManager.DisplayMessage("Level DOWN!");
                currentLevel = value;
                levelValueText.text = currentLevel.ToString(); //Пишем также на экране
            }
        }
    }

    public float CurrentLevelTimer
    {
        get { return currentLevelTime; }
        set
        {
            currentLevelTime = value;
            timeValueText.text = ((int)currentLevelTime).ToString(); //Пишем также на экране
        }
    }

    public int CurrentScore
    {
        get { return currentScore; }
        set
        {
            currentScore = value;
            scoreValueText.text = currentScore.ToString(); //Пишем также на экране
        }
    }

    public int FilledLinesCount
    {
        get { return filledLinesCount; }
        set
        {
            filledLinesCount = value;
            linesValueText.text = filledLinesCount.ToString(); //Пишем также на экране
        }
    }

    public void AddScore(int rowsCount) //Прибавить очки за строки
    {
        int score = POINTS_PER_LINE * rowsCount * CurrentLevel;
        if (rowsCount > 1)
        {
            score += POINTS_PER_LINE_BONUS * (rowsCount - 1) * (rowsCount - 1) * CurrentLevel;
            displayManager.DisplayMessage("Bonus X" + rowsCount.ToString());
        }
        CurrentScore += score;
    }
}
