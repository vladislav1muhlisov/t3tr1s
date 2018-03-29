using System.Collections.Generic;
using UnityEngine;

public enum RotationType { Full, Half, NoRotation }

public class Tetromino : MonoBehaviour
{
    public RotationType RotationType; //Тип вращения фигуры (имеет ли смысл вообще её поворачивать)

    public delegate void TetraminoEvent(Tetromino tetromino);
    public event TetraminoEvent LandedEvent = delegate { }; //Событие приземления фигуры
    public List<Transform> Minoes; //отдельные квадратики данной фигуры

    private Field m_field;

    //Номера строк, которые занимает мино. Для проверки после приземления
    public List<int> GetRows()
    {
        List<int> rows = new List<int>();
        foreach (Transform mino in Minoes)
        {
            int row = (int)Mathf.Round(mino.position.y);
            if (!rows.Contains(row)) rows.Add(row);
        }
        return rows;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D)) StartCoroutine(KeyHolding.HoldingWithDelay(KeyCode.D, MoveRight));
        if (Input.GetKeyDown(KeyCode.RightArrow)) StartCoroutine(KeyHolding.HoldingWithDelay(KeyCode.RightArrow, MoveRight));

        if (Input.GetKeyDown(KeyCode.A)) StartCoroutine(KeyHolding.HoldingWithDelay(KeyCode.A, MoveLeft));
        if (Input.GetKeyDown(KeyCode.LeftArrow)) StartCoroutine(KeyHolding.HoldingWithDelay(KeyCode.LeftArrow, MoveLeft));

        if (Input.GetKeyDown(KeyCode.S)) StartCoroutine(KeyHolding.HoldingWithDelay(KeyCode.S, MoveDown));
        if (Input.GetKeyDown(KeyCode.DownArrow)) StartCoroutine(KeyHolding.HoldingWithDelay(KeyCode.DownArrow, MoveDown));

        if (Input.GetKeyDown(KeyCode.Space)) Rotate();
    }

    private void Awake()
    {
        Minoes = new List<Transform>();
        foreach (Transform mino in transform) Minoes.Add(mino); //Получить отдельные дочерние мино
        m_field = Field.Instance();
        m_field.TimerTick += MoveDown; //На каждый тик таймера смещать вниз
    }

    private bool IsPositionValid()
    {
        foreach (Transform mino in Minoes)
            if (!m_field.IsSingleCellValid(Field.RoundPosition(mino.position)))
                return false;
        return true;
    }

    private void MoveRight()
    {
        transform.position += new Vector3(1, 0, 0);
        if (!IsPositionValid()) transform.position += new Vector3(-1, 0, 0);
    }

    private void MoveLeft()
    {
        transform.position += new Vector3(-1, 0, 0);
        if (!IsPositionValid()) transform.position += new Vector3(1, 0, 0);
    }

    private void MoveDown()
    {
        transform.position += new Vector3(0, -1, 0);
        if (!IsPositionValid())
        {
            m_field.TimerTick -= MoveDown; //Фигура приземлилась и не будет больше реагировать на таймер
            transform.position += new Vector3(0, 1, 0);
            LandedEvent(this);
        }
    }

    private void Rotate()
    {
        if (RotationType != RotationType.NoRotation)
        {
            Vector3 rotation = new Vector3(); //Направление поворота фигуры

            if (RotationType == RotationType.Half) //Фигуры с неполным вращением
            {
                if (transform.rotation.eulerAngles.z >= 90)
                    rotation = new Vector3(0, 0, -90);
                else
                    rotation = new Vector3(0, 0, 90);
            }
            else if (RotationType == RotationType.Full) //Фигуры с полным вращением
            {
                rotation = new Vector3(0, 0, 90);
            }

            transform.Rotate(rotation); //Поворачиваем
            if (!IsPositionValid()) //Если поворот некорректен,
            {
                Vector3 startPosition = transform.position; //резервируем позицию

                const int maxOffset = 2;
                //Пробуем сдвинуть вправо или влево поочерёдно
                for (int offset = 1; offset <= maxOffset; offset++)
                {
                    transform.position = startPosition - new Vector3(offset, 0, 0);
                    if (IsPositionValid()) return;
                    transform.position = startPosition + new Vector3(offset, 0, 0);
                    if (IsPositionValid()) return;
                }
                //В противном случае возвращаем фигуру в начальное положение
                transform.position = startPosition;
                transform.Rotate(-rotation);
            }
        }
    }
}
