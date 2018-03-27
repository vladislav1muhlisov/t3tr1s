using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class KeyHolding
{
    private const float HOLDING_DELAY = 0.4f; //Задержка при удержании клавиши
    private const float HOLDING_PERIOD = 0.2f; //Период вызова метода


    /// <summary>
    /// Корутина, вызывающая определённый метод при определённой зажатой клавише. Версия с задержкой.
    /// </summary>
    /// <param name="keyCode">Код клавиши</param>
    /// <param name="action">Метод, который нужно вызывать при зажатой клавише</param>
    public static IEnumerator HoldingWithDelay(KeyCode keyCode, UnityAction action)
    {
        action();
        float holdingDelayTimer = 0;
        float holdingTimer = 0;

        while (true) //Цикл ожидания задержки
        {
            if (!Input.GetKey(keyCode)) yield break; //Выйти из цикла, если клавиша больше не "залипает"
            holdingDelayTimer += Time.deltaTime;

            if (holdingDelayTimer > HOLDING_DELAY)
                while (true) //Цикл самого удержания клавиши
                {
                    if (!Input.GetKey(keyCode)) yield break;
                    holdingTimer += Time.deltaTime;

                    if (holdingDelayTimer > HOLDING_PERIOD)
                    {
                        action();
                        holdingTimer = 0;
                    }
                    yield return null;
                }
            yield return null;
        }
    }
}
