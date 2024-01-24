using TMPro;
using UnityEngine;

namespace AdvancedCompany.Components;

public abstract class BaseMonitor : MonoBehaviour
{
    protected TextMeshProUGUI _textMesh;

    public void Start()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
        Logger.LogDebug(_textMesh);
        ResetMonitor();

        PostStart();
    }

    protected abstract void PostStart();

    protected void ResetMonitor()
    {
        _textMesh.text = "";
    }

    protected void UpdateMonitorText(string text)
    {
        _textMesh.text = text;
    }

    /// <summary>
    /// Updates the text mesh with text on the 1st line and a currency value on the 2nd line, such as loot value.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="number"></param>
    protected void UpdateMonitorText(string text, int number)
    {
        _textMesh.text = $"{text}\n${number}";
    }
}
