using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum TetrominoType { I, J, L, O, S, T, Z } //Виды тетраминошек


public class Field : MonoBehaviour
{
    #region References
    public GameObject fieldObject;
    public GameObject I_Prefab;
    public GameObject J_Prefab;
    public GameObject L_Prefab;
    public GameObject O_Prefab;
    public GameObject S_Prefab;
    public GameObject T_Prefab;
    public GameObject Z_Prefab;
    public Transform previewLocation;

    public Text testText;
    #endregion

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    private readonly int FIGURES_COUNT = System.Enum.GetNames(typeof(TetrominoType)).Length; //Общее число фигур

    private const float TIME_DEFAULT_TICK = 1f; //Период таймера в секундах (на первом уровне)
    private const float TIME_LEVEL = 15.99f; //Время каждого уровня
    //Размеры поля
    private const int FIELD_HEIGHT = 25;
    private const int FIELD_WIDTH = 14;
    //Координаты появления новых тетраминошек
    private const int NEW_TETRO_COORD_X = FIELD_WIDTH / 2;
    private const int NEW_TETRO_COORD_Y = FIELD_HEIGHT + 1;
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    private static Field instance;

    private Preview preview; //Preview script
    private Data data; //Данные: очки, линии и т. д.
    //Сетка с ссылками на квадратики (+5 для самой длинной фигуры, которая только начинает падать и пока еще за пределами)
    private Transform[,] grid = new Transform[FIELD_WIDTH, FIELD_HEIGHT + 5];
    private TetrominoType nextTetromino;


    public static Field Instance()
    {
        if (!instance)
        {
            instance = FindObjectOfType<Field>();
            if (!instance)
            {
                Debug.LogError("There is no field object on the scene!");
                return null;
            }
        }
        return instance;
    }

    public Vector3 RoundPosition(Vector3 pos)
    {
        return new Vector3((int)Mathf.Round(pos.x), (int)Mathf.Round(pos.y));
    }

    public bool IsSingleCellInsideFieldArea(Vector3 position)
    {
        return
            position.x >= 0 && position.x < FIELD_WIDTH && position.y >= 0;
    }

    public bool IsSingleCellFilled(Vector3 position)
    {
        return
            !IsSingleCellInsideFieldArea(position)
            || grid[(int)position.x, (int)position.y] != null;
    }

    //Событие переключения таймера
    public delegate void GameEvent();
    public event GameEvent TimerTick = delegate { };


    private void Start()
    {
        preview = previewLocation.GetComponent<Preview>();
        data = Data.Instance();
        //DrawField();
        StartNewGame();
        StartCoroutine(Timer());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
            data.CurrentLevel++;
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            data.CurrentLevel--;
    }

    private void StartNewGame()
    {
        data.CurrentLevelTime = TIME_LEVEL;
        data.CurrentLevel = 1;
        data.FilledLinesCount = 0;
        data.CurrentScore = 0;

        InstantiateNewTetromino(GetNextTetrominoType());
        nextTetromino = GetNextTetrominoType();
        preview.ShowPreview(nextTetromino);
    }

    private void FinishGame()
    {
        for (int i = 0; i < grid.GetLength(0); i++)
            for (int j = 0; j < grid.GetLength(1); j++)
                if (grid[i, j] != null)
                {
                    Destroy(grid[i, j].gameObject);
                    grid[i, j] = null;
                }
        StartNewGame();
        DisplayManager.Instance().DisplayMessage("You lose!");
    }

    private bool IsLost()
    {
        for (int i = 0; i < FIELD_WIDTH; i++)
            if (grid[i, FIELD_HEIGHT - 1] != null) return true;
        return false;
    }

    //на случай изменения размера поля
    private void DrawField() { }

    //Заполнена ли строка
    private bool IsRowFilled(int row)
    {
        for (int i = 0; i < FIELD_WIDTH; i++)
            if (grid[i, row] == null)
                return false;
        return true;
    }

    //Удалить строку
    private void DeleteRow(int row)
    {
        for (int i = 0; i < FIELD_WIDTH; i++)
        {
            if (grid[i, row] != null)
            {
                Destroy(grid[i, row].gameObject);
                grid[i, row] = null;
            }
            //Смещаем вниз всё, что лежало выше
            for (int j = row + 1; j < FIELD_HEIGHT; j++)
            {
                while (grid[i, j] != null)
                {
                    grid[i, j - 1] = grid[i, j];
                    grid[i, j] = null;
                    grid[i, j - 1].position += new Vector3(0, -1);
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

        InstantiateNewTetromino(nextTetromino);
        nextTetromino = GetNextTetrominoType();
        preview.ShowPreview(nextTetromino);

        int filledRowsCount = CheckAndDeleteFilledRows(rows);
        data.AddScore(filledRowsCount);
        data.FilledLinesCount += filledRowsCount;

    }

    //Перестаём управлять фигурой
    private void DropTetromino(Tetromino tetromino)
    {
        foreach (Transform mino in tetromino.minoes)
        {
            grid[(int)Mathf.Round(mino.position.x), (int)Mathf.Round(mino.position.y)] = mino;
            mino.parent = fieldObject.transform; //Делаем мино child'ами игрового поля
        }
        Destroy(tetromino.gameObject);
    }

    private TetrominoType GetNextTetrominoType()
    {
        return (TetrominoType)Random.Range(0, FIGURES_COUNT);
    }

    private void InstantiateNewTetromino(TetrominoType tetromino)
    {
        GameObject newTetromino = null;
        switch (tetromino)
        {
            case TetrominoType.I: newTetromino = Instantiate(I_Prefab, transform); break;
            case TetrominoType.J: newTetromino = Instantiate(J_Prefab, transform); break;
            case TetrominoType.L: newTetromino = Instantiate(L_Prefab, transform); break;
            case TetrominoType.O: newTetromino = Instantiate(O_Prefab, transform); break;
            case TetrominoType.S: newTetromino = Instantiate(S_Prefab, transform); break;
            case TetrominoType.T: newTetromino = Instantiate(T_Prefab, transform); break;
            case TetrominoType.Z: newTetromino = Instantiate(Z_Prefab, transform); break;
        }
        newTetromino.transform.localPosition = new Vector3(NEW_TETRO_COORD_X, NEW_TETRO_COORD_Y);
        newTetromino.GetComponent<Tetromino>().LandedEvent += OnTetrominoLandedReaction;
    }


    private float currentTimerValue;

    IEnumerator Timer()
    {
        currentTimerValue = 0;
        while (true) //Бесконечный цикл
        {
            data.CurrentLevelTime -= Time.deltaTime;
            currentTimerValue += Time.deltaTime;
            if (currentTimerValue >= TIME_DEFAULT_TICK / data.CurrentLevel) //Тик таймера
            {
                currentTimerValue = 0;
                if (data.CurrentLevelTime < 0) //Новый уровень
                {
                    data.CurrentLevelTime = TIME_LEVEL;
                    data.CurrentLevel++;
                }
                TimerTick();
            }
            yield return null;
        }
    }
}
