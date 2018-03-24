using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RotationType { full, half, noRotation }

public class Tetromino : MonoBehaviour
{
    public RotationType rotationType; //Тип вращения фигуры (имеет ли смысл вообще её поворачивать)

    public delegate void TetraminoEvent(Tetromino tetromino);
    public event TetraminoEvent LandedEvent = delegate { }; //Событие приземления фигуры
    public List<Transform> minoes; //отдельные квадратики данной фигуры

    private Field field;

    //Номера строк, которые занимает мино. Для проверки после приземления
    public List<int> GetRows()
    {
        List<int> rows = new List<int>();
        foreach (Transform mino in minoes)
        {
            int row = (int)Mathf.Round(mino.position.y);
            if (!rows.Contains(row)) rows.Add(row);
        }
        return rows;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            MoveRight();
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            MoveLeft();
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            MoveDown();
        if (Input.GetKeyDown(KeyCode.Space))
            Rotate();
    }

    private void Awake()
    {
        minoes = new List<Transform>();
        foreach (Transform mino in transform) minoes.Add(mino); //Получить отдельные дочерние мино
        field = Field.Instance();
        field.TimerTick += MoveDown; //На каждый тик таймера смещать вниз
    }

    private void OnDestroy()
    {
        field.TimerTick -= MoveDown;
    }

    private bool IsPlaceFilled()
    {
        foreach (Transform mino in minoes)
            if (field.IsSingleCellFilled(field.RoundPosition(mino.position)))
                return true;
        return false;
    }

    private void MoveRight()
    {
        transform.position += new Vector3(1, 0, 0);
        if (IsPlaceFilled()) transform.position += new Vector3(-1, 0, 0);
    }

    private void MoveLeft()
    {
        transform.position += new Vector3(-1, 0, 0);
        if (IsPlaceFilled()) transform.position += new Vector3(1, 0, 0);
    }

    private void MoveDown()
    {
        transform.position += new Vector3(0, -1, 0);
        if (IsPlaceFilled())
        {
            transform.position += new Vector3(0, 1, 0);
            LandedEvent(this);
        }
    }

    private void Rotate()
    {
        if (rotationType != RotationType.noRotation)
        {
            if (rotationType == RotationType.half) //Фигуры с неполным вращением
            {
                if (transform.rotation.eulerAngles.z >= 90)
                {
                    transform.Rotate(0, 0, -90);
                    if (IsPlaceFilled()) transform.Rotate(0, 0, 90);
                }
                else
                {
                    transform.Rotate(0, 0, 90);
                    if (IsPlaceFilled()) transform.Rotate(0, 0, -90);
                }
            }
            else if (rotationType == RotationType.full) //Фигуры с полным вращением
            {
                transform.Rotate(0, 0, 90);
                if (IsPlaceFilled()) transform.Rotate(0, 0, -90);
            }
        }
    }
}
