using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class TextDisplay : MonoBehaviour
{
    [Title("Text Settings")] 
    [LabelText("Text Content")] [Tooltip("The text to display.")]
    [TextArea]
    public string text = "Hello, Odin + TextMeshPro!";

    [LabelText("Text Color")] [ColorPalette]
    public Color textColor = Color.white;

    [LabelText("Font Size")] [Range(1, 100)]
    public float textSize = 36f;

    [LabelText("Position Offset")] [Tooltip("Adjust the local position offset of the text.")]
    public Vector3 offset = new Vector3(0, 2, 0);

    private TextMeshPro _textMeshPro;

    [Button("Update Text"), GUIColor(0.4f, 0.8f, 1f)]
    private void UpdateText()
    {
        if (_textMeshPro == null)
        {
            InitializeTextMeshPro();
        }

        _textMeshPro.text = text;
        _textMeshPro.color = textColor;
        _textMeshPro.fontSize = textSize;
        _textMeshPro.transform.localPosition = offset;

    }

    private void Awake()
    {
        InitializeTextMeshPro();
    }

    private void OnValidate()
    {
        UpdateText();
    }

    private void InitializeTextMeshPro()
    {
        if (_textMeshPro == null)
        {
            _textMeshPro = GetComponent<TextMeshPro>();
            if (_textMeshPro == null)
            {
                _textMeshPro = gameObject.AddComponent<TextMeshPro>();
            }
        }

        _textMeshPro.text = text;
        _textMeshPro.color = textColor;
        _textMeshPro.fontSize = textSize;
        _textMeshPro.alignment = TextAlignmentOptions.Center;
    }
}