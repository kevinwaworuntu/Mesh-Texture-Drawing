using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIDrawOn3D : MonoBehaviour
{
    [Header("BRUSH REFERENCE")]
    private DrawOn3D drawOn3D;
    [SerializeField] private TextMeshProUGUI brushActionDisplayTMP;
    private BrushAction lastBrushAction;

    [Header("TARGET RENDERER REFERENCE")]
    [SerializeField] private Transform targetRenderer;

    private void Awake()
    {
        drawOn3D = FindObjectOfType<DrawOn3D>();
        UpdateBrushActionTMP();
        lastBrushAction =  drawOn3D.currentBrushAction;
    }
    
    private void Update()
    {
        if(lastBrushAction != drawOn3D.currentBrushAction)
        {
            UpdateBrushActionTMP();
            lastBrushAction = drawOn3D.currentBrushAction;
        }
    }
    private void UpdateBrushActionTMP()
    {
        if(drawOn3D.currentBrushAction == BrushAction.Draw) brushActionDisplayTMP.text = "Draw Mode";
        else if(drawOn3D.currentBrushAction == BrushAction.Erase) brushActionDisplayTMP.text = "Erase Mode";
    }

    public void ChangeTargetRendererRotation(Slider slider)
    {
        var targetAngle = slider.value * 360;
        targetRenderer.eulerAngles = new Vector3(targetRenderer.eulerAngles.x, targetAngle,targetRenderer.eulerAngles.z);
    }

    public void ResizeBrushSizeWithSlider(Slider slider)
    {
        drawOn3D.ResizeBrushWithPercentage(slider.value);
    }
}
