using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public static class KeyHolding
{
    private const float HoldingDelay = 0.4f; //Задержка при удержании клавиши
    private const float HoldingPeriod = 0.025f; //Период вызова метода


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

            if (holdingDelayTimer > HoldingDelay)
                while (true) //Цикл самого удержания клавиши
                {
                    if (!Input.GetKey(keyCode)) yield break;
                    holdingTimer += Time.deltaTime;

                    if (holdingTimer > HoldingPeriod)
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
