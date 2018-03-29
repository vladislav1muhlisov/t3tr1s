using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;


public enum TetrominoType { I, J, L, O, S, T, Z } //Виды тетраминошек


public class Field : MonoBehaviour
{
    #region References
    public GameObject PrefabI;
    public GameObject PrefabJ;
    public GameObject PrefabL;
    public GameObject PrefabO;
    public GameObject PrefabS;
    public GameObject PrefabT;
    public GameObject PrefabZ;
    public Transform previewLocation;
    public Transform minoesLocation;
    #endregion

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    private static readonly int m_figuresCount = System.Enum.GetNames(typeof(TetrominoType)).Length;

    private const float TimeDefaultTick = 1f; //Период таймера в секундах (на первом уровне)
    private const float TimeLevel = 25.99f; //Время каждого уровня
    //Размеры поля
    private const int FieldHeight = 25;
    private const int FieldWidth = 14;
    //Координаты появления новых тетраминошек
    private const int NewTetroCoordX = FieldWidth / 2;
    private const int NewTetroCoordY = FieldHeight + 1;
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    private static Field m_instance;

    private Preview m_preview; //Preview script
    private Data m_data; //Данные: очки, линии и т. д.
    //Сетка с ссылками на квадратики (+5 для самой длинной фигуры, которая только начинает падать и пока еще за пределами)
    private readonly Transform[,] m_grid = new Transform[FieldWidth, FieldHeight + 5];
    private TetrominoType m_nextTetromino;


    public static Field Instance()
    {
        if (!m_instance)
        {
            m_instance = FindObjectOfType<Field>();
            if (!m_instance)
            {
                Debug.LogError("There is no field object on the scene!");
                return null;
            }
        }
        return m_instance;
    }

    public static Vector3 RoundPosition(Vector3 pos)
    {
        return new Vector3((int)Mathf.Round(pos.x), (int)Mathf.Round(pos.y));
    }

    public bool IsSingleCellInsideFieldArea(Vector3 position)
    {
        return
            position.x >= 0 && position.x < FieldWidth && position.y >= 0;
    }

    public bool IsSingleCellValid(Vector3 position)
    {
        if (!IsSingleCellInsideFieldArea(position))
            return false;
        else return m_grid[(int)position.x, (int)position.y] == null;
    }

    //Событие переключения таймера
    public delegate void GameEvent();
    public event GameEvent TimerTick = delegate { };


    private void Start()
    {
        m_preview = previewLocation.GetComponent<Preview>();
        m_data = Data.Instance();
        //DrawField();
        StartNewGame();
        StartCoroutine(Timer());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
            m_data.CurrentLevel++;
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            m_data.CurrentLevel--;
    }

    private void StartNewGame()
    {
        m_data.CurrentLevelTimer = TimeLevel;
        m_data.CurrentLevel = 1;
        m_data.FilledLinesCount = 0;
        m_data.CurrentScore = 0;

        InstantiateNewTetromino(GetNextTetrominoType());
        m_nextTetromino = GetNextTetrominoType();
        m_preview.ShowPreview(m_nextTetromino);
    }

    private void FinishGame()
    {
        for (int i = 0; i < m_grid.GetLength(0); i++)
            for (int j = 0; j < m_grid.GetLength(1); j++)
                if (m_grid[i, j] != null)
                {
                    Destroy(m_grid[i, j].gameObject);
                    m_grid[i, j] = null;
                }
        StartNewGame();
        DisplayManager.Instance().DisplayMessage("You lose!");
    }

    private bool IsLost()
    {
        for (int i = 0; i < FieldWidth; i++)
            if (m_grid[i, FieldHeight - 1] != null) return true;
        return false;
    }

    //на случай изменения размера поля
    private void DrawField() { }

    //Заполнена ли строка
    private bool IsRowFilled(int row)
    {
        for (int i = 0; i < FieldWidth; i++)
            if (m_grid[i, row] == null)
                return false;
        return true;
    }

    //Удалить строку
    private void DeleteRow(int row)
    {
        for (int i = 0; i < FieldWidth; i++)
        {
            if (m_grid[i, row] != null)
            {
                Destroy(m_grid[i, row].gameObject);
                m_grid[i, row] = null;
            }
            //Смещаем вниз всё, что лежало выше
            for (int j = row + 1; j < FieldHeight; j++)
            {
                while (m_grid[i, j] != null)
                {
                    m_grid[i, j - 1] = m_grid[i, j];
                    m_grid[i, j] = null;
                    m_grid[i, j - 1].position += new Vector3(0, -1);
                }
            }
        }
    }

    //Удаляет заполненные строки и возвращает число удалённых строк
    private int CheckAndDeleteFilledRows(List<int> rows)
    {
        int filledRowsCount = 0;
        rows.Sort();
        //Начинаем с верхней строки
        for (int i = rows.Count - 1; i >= 0; i--)
            if (IsRowFilled(rows[i]))
            {
                DeleteRow(rows[i]);
                filledRowsCount++;
            }
        return filledRowsCount;
    }

    //Реакция поля на приземление фигуры
    private void OnTetrominoLandedReaction(Tetromino tetromino)
    {
        List<int> rows = tetromino.GetRows(); //Получаем строки, в которые упала фигура
        DropTetromino(tetromino);

        if (IsLost())
        {
            FinishGame();
            return;
        }

        InstantiateNewTetromino(m_nextTetromino);
        m_nextTetromino = GetNextTetrominoType();
        m_preview.ShowPreview(m_nextTetromino);

        int filledRowsCount = CheckAndDeleteFilledRows(rows);
        m_data.AddScore(filledRowsCount);
        m_data.FilledLinesCount += filledRowsCount;

    }

    //Перестаём управлять фигурой
    private void DropTetromino(Tetromino tetromino)
    {
        foreach (Transform mino in tetromino.Minoes)
        {
            m_grid[(int)Mathf.Round(mino.position.x), (int)Mathf.Round(mino.position.y)] = mino;
            mino.parent = minoesLocation; //Делаем мино child'ами игрового поля
        }
        Destroy(tetromino.gameObject);
    }

    private TetrominoType GetNextTetrominoType()
    {
        return (TetrominoType)Random.Range(0, m_figuresCount);
    }

    private void InstantiateNewTetromino(TetrominoType tetromino)
    {
        GameObject newTetromino;
        switch (tetromino)
        {
            case TetrominoType.I: newTetromino = Instantiate(PrefabI, transform); break;
            case TetrominoType.J: newTetromino = Instantiate(PrefabJ, transform); break;
            case TetrominoType.L: newTetromino = Instantiate(PrefabL, transform); break;
            case TetrominoType.O: newTetromino = Instantiate(PrefabO, transform); break;
            case TetrominoType.S: newTetromino = Instantiate(PrefabS, transform); break;
            case TetrominoType.T: newTetromino = Instantiate(PrefabT, transform); break;
            case TetrominoType.Z: newTetromino = Instantiate(PrefabZ, transform); break;
            default: newTetromino = Instantiate(PrefabI, transform); break;
        }

        newTetromino.transform.localPosition = new Vector3(NewTetroCoordX, NewTetroCoordY);
        newTetromino.GetComponent<Tetromino>().LandedEvent += OnTetrominoLandedReaction;
    }


    private float m_currentTimerValue;

    private IEnumerator Timer()
    {
        m_currentTimerValue = 0;
        while (true) //Бесконечный цикл
        {
            m_data.CurrentLevelTimer -= Time.deltaTime;
            m_currentTimerValue += Time.deltaTime;
            if (m_currentTimerValue >= CurrentLevelDuration()) //Тик таймера
            {
                m_currentTimerValue = 0;
                if (m_data.CurrentLevelTimer < 0) //Новый уровень
                {
                    m_data.CurrentLevelTimer = TimeLevel;
                    m_data.CurrentLevel++;
                }
                TimerTick();
            }
            yield return null;
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private float CurrentLevelDuration()
    {
        return TimeDefaultTick * (Data.MaxLevel + 1 - m_data.CurrentLevel) / (Data.MaxLevel + 1);
    }
}
