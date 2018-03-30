using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//Показывает медленно затухающий текст 
public class DisplayManager : MonoBehaviour
{
    public Text DisplayText; //Сам текст
    public float DisplayTime;
    public float FadeTime;

    private IEnumerator m_fadeAlpha;

    private static DisplayManager m_displayManager;

    public static DisplayManager Instance()
    {
        if (!m_displayManager)
        {
            m_displayManager = FindObjectOfType(typeof(DisplayManager)) as DisplayManager;
            if (!m_displayManager)
                Debug.LogError("There needs to be one active DisplayManager script on a GameObject in your scene.");
        }

        return m_displayManager;
    }

    public void DisplayMessage(string message)
    {
        DisplayText.text = message;
        SetAlpha();
    }

    private void SetAlpha()
    {
        if (m_fadeAlpha != null)
        {
            StopCoroutine(m_fadeAlpha);
        }
        m_fadeAlpha = FadeAlpha();
        StartCoroutine(m_fadeAlpha);
    }

    private IEnumerator FadeAlpha()
    {
        Color resetColor = DisplayText.color;
        resetColor.a = 1;
        DisplayText.color = resetColor;

        yield return new WaitForSeconds(DisplayTime);

        while (DisplayText.color.a > 0)
        {
            Color displayColor = DisplayText.color;
            displayColor.a -= Time.deltaTime / FadeTime;
            DisplayText.color = displayColor;
            yield return null;
        }
        yield return null;
    }
}