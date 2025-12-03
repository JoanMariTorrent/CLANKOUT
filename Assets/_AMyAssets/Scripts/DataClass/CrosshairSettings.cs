using UnityEngine;

[System.Serializable]
public class CrosshairSettings
{
    public Color crosshairColor = Color.white;
    public bool useCenterDot = true;
    public float centerDotSize = 1.0f;
    public float centerDotOpacity = 1.0f;

    public bool showInnerLines = true;
    public float innerOpacity = 1f;
    public float innerLenght = 6f;
    public float innerThickness = 2f;
    public float innerOffset = 3f;
}
