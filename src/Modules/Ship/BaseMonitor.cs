using QualityCompany.Service;
using TMPro;
using UnityEngine;

namespace QualityCompany.Modules.Ship;

internal abstract class BaseMonitor : MonoBehaviour
{
    protected ACLogger _logger;
    protected TextMeshProUGUI _textMesh;

    public void Start()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
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

    protected string GetMonitorText()
    {
        return _textMesh.text;
    }
}
