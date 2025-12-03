using Unity.VisualScripting;
using UnityEngine;

public class CrosshairMenu : MonoBehaviour
{
    [SerializeField] private CrosshairController crosshairController;
    public void OnThicknessChanged(float newValue)
    { 
        crosshairController.settings.innerThickness = newValue;
        crosshairController.UpdateCrosshair();
    }

    public void OnInnerOpacityChanged(float newValue)
    {
        crosshairController.settings.innerOpacity = newValue;
        crosshairController.UpdateCrosshair();
    }
}
