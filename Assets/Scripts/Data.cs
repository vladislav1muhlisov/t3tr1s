using UnityEngine;
using UnityEngine.UI;


public class Data : MonoBehaviour
{
    #region References
    public Text LevelValueText;
    public Text ScoreValueText;
    public Text TimeValueText;
    public Text LinesValueText;
    #endregion

    private int m_currentLevel;
    private int m_currentScore;
    private float m_currentLevelTime;
    private int m_filledLinesCount;

    private DisplayManager m_displayManager;

    private static Data m_instance;

    public const int MaxLevel = 9;
    private const int PointsPerLine = 20;
    private const int PointsPerLineBonus = 10;

    private void Awake()
    {
        m_displayManager = DisplayManager.Instance();
    }

    public static Data Instance()
    {
        if (!m_instance)
        {
            m_instance = FindObjectOfType<Data>();
            if (!m_instance)
            {
                Debug.LogError("There is no data object on the scene!");
                return null;
            }
        }
        return m_instance;
    }

    public int CurrentLevel
    {
        get { return m_currentLevel; }
        set
        {
            if (value <= 0) m_currentLevel = 1;
            else if (value > MaxLevel) m_currentLevel = MaxLevel;
            else
            {
                if (m_currentLevel < value) m_displayManager.DisplayMessage("Level UP!");
                else if (m_currentLevel > value) m_displayManager.DisplayMessage("Level DOWN!");
                m_currentLevel = value;
                LevelValueText.text = m_currentLevel.ToString(); //Пишем также на экране
            }
        }
    }

    public float CurrentLevelTimer
    {
        get { return m_currentLevelTime; }
        set
        {
            m_currentLevelTime = value;
            TimeValueText.text = ((int)m_currentLevelTime).ToString(); //Пишем также на экране
        }
    }

    public int CurrentScore
    {
        get { return m_currentScore; }
        set
        {
            m_currentScore = value;
            ScoreValueText.text = m_currentScore.ToString(); //Пишем также на экране
        }
    }

    public int FilledLinesCount
    {
        get { return m_filledLinesCount; }
        set
        {
            m_filledLinesCount = value;
            LinesValueText.text = m_filledLinesCount.ToString(); //Пишем также на экране
        }
    }

    public void AddScore(int rowsCount) //Прибавить очки за строки
    {
        int score = PointsPerLine * rowsCount * CurrentLevel;
        if (rowsCount > 1)
        {
            score += PointsPerLineBonus * (rowsCount - 1) * (rowsCount - 1) * CurrentLevel;
            m_displayManager.DisplayMessage("Bonus X" + rowsCount);
        }
        CurrentScore += score;
    }
}
