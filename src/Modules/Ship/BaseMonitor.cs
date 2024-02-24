using QualityCompany.Service;
using TMPro;
using UnityEngine;

namespace QualityCompany.Modules.Ship;

internal abstract class BaseMonitor : MonoBehaviour
{
    protected ModLogger Logger;
    protected TextMeshProUGUI TextMesh;

    private void Start()
    {
        TextMesh = GetComponent<TextMeshProUGUI>();
        ResetMonitor();

        PostStart();
    }

    protected abstract void PostStart();

    protected void ResetMonitor()
    {
        TextMesh.text = "";
    }

    protected void UpdateMonitorText(string text)
    {
        TextMesh.text = text;
    }

    /// <summary>
    /// Updates the text mesh with text on the 1st line and a currency value on the 2nd line, such as loot value.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="number"></param>
    protected void UpdateMonitorText(string text, int number)
    {
        TextMesh.text = $"{text}\n${number}";
    }

    protected string GetMonitorText()
    {
        return TextMesh.text;
    }
}
