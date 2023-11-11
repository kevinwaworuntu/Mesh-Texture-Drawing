using UnityEngine;

public enum BrushAction {Draw, Erase}
public class DrawOn3D : MonoBehaviour
{
    [Header("BRUSH RAYCAST")]
    private Camera cam;
    [SerializeField] private Renderer targetRenderer;
    private RaycastHit hit;
    private Vector2 lastTextureCoordinate;
    private Vector2 currentTextureCoordinate;

    [Header("TEXTURE")]
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private Vector2 textureDimension = new Vector2(128,128);
    private Color[] pixels;
    private Color[] initialPixels;
    private Texture2D initialTexture2D;
    private bool isReadingInitialPixels;
    
    [Header("BRUSH PROPERTIES")]
  
    public BrushAction currentBrushAction;
    [SerializeField] private float defaultSize;
    public float currentRadius;
    private float initRadius;
    [SerializeField] private float radiusMultiplier;
    [SerializeField] private Color brushColor = Color.white;
    [SerializeField] private float brushMultiplierMin, brushMultiplierMax;

    void Start()
    {
        InitializeRenderTexture();
        cam = Camera.main;
    }

    void Update()
    {
        if(!isReadingInitialPixels)
        {
            InitialSetup();
            isReadingInitialPixels = true;
        }
        Brush();
    }

    private void InitialSetup()
    {
        initialPixels = GetRenderTexturePixels(renderTexture);
        if(initialTexture2D == null) CreateInitialTexture2D();
        currentRadius = MatchRadiusWithTextureDimension(textureDimension);    
    }

    #region BRUSH
    private void Brush()
    {
        if (!Input.GetMouseButton(0)) return;
        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit)) return;
        
        MeshCollider meshCollider = hit.collider as MeshCollider;
       
        if (targetRenderer == null || targetRenderer.sharedMaterial == null || targetRenderer.sharedMaterial.mainTexture == null || meshCollider == null)
        return;

        currentTextureCoordinate = hit.textureCoord;
        if(lastTextureCoordinate == currentTextureCoordinate) return;
        
        ModifyRenderTexturePixels(currentTextureCoordinate);
        lastTextureCoordinate = currentTextureCoordinate;
    }

    public void SwitchDrawerEraser()
    {
        if(currentBrushAction == BrushAction.Draw) SwitchBrushToErase();
        else if(currentBrushAction == BrushAction.Erase) SwitchBrushToDraw();
    }
    private void SwitchBrushToDraw() => currentBrushAction = BrushAction.Draw;
    private void SwitchBrushToErase() => currentBrushAction = BrushAction.Erase;

    private float MatchRadiusWithTextureDimension(Vector2 textureDimension)
    {
        var textureWidth = textureDimension.x;
        var textureHeight = textureDimension.y;

        float aspectRatio = (float) textureWidth / textureHeight;
        float avarageDimension = (textureWidth + textureHeight)/2;

        float scaledRadius = avarageDimension * defaultSize;
        return currentRadius = initRadius = scaledRadius;
    }

    public void ResizeBrushWithPercentage(float value)
    {
        radiusMultiplier = brushMultiplierMin + value * (brushMultiplierMax - brushMultiplierMin);
        currentRadius = initRadius * radiusMultiplier;
    }

    public void SetBrushColor(string colorString)
    {
        Color newColor;

        if (ColorUtility.TryParseHtmlString($"#{colorString}", out newColor))
        {
            brushColor = newColor;
        }
        else
        {
            Debug.LogWarning("Invalid color string: " + colorString);
        }
    }
    #endregion

    #region TEXTURE
    private void InitializeRenderTexture()
    {
        if(targetRenderer.material.mainTexture == null)
        {
            CreateRenderTexture((int) textureDimension.x, (int)textureDimension.y);
            return;
        }
        else
        {
            if(targetRenderer.material.mainTexture is Texture2D)
            {
                var originalTexture = targetRenderer.material.mainTexture as Texture2D;
                CreateRenderTexture((int)textureDimension.x, (int)textureDimension.y);
                
                RenderTexture.active = renderTexture;
                Graphics.Blit(originalTexture, renderTexture);
                RenderTexture.active = null;
            }
            else if(targetRenderer.material.mainTexture is RenderTexture)
            {
                var originalTexture = targetRenderer.material.mainTexture as RenderTexture;
                var cloneTexture = new RenderTexture(originalTexture.width, originalTexture.height, originalTexture.depth, originalTexture.format);
                renderTexture = cloneTexture;

                RenderTexture.active = renderTexture;
                Graphics.Blit(originalTexture, renderTexture);
                RenderTexture.active = null;
            }
        }
    }

    private void CreateRenderTexture(int textureWidht, int textureHeight)
    {
        renderTexture = new RenderTexture(textureWidht, textureHeight, 16, RenderTextureFormat.ARGB32);
        renderTexture.name = "tempTexture";
        renderTexture.Create();
        renderTexture.Release(); 

        targetRenderer.material.mainTexture = renderTexture;
    }

    private Color[] GetRenderTexturePixels(RenderTexture rTexture)
    {
        Texture2D tempTexture = new Texture2D(rTexture.width, rTexture.height);
        RenderTexture.active = rTexture;
        tempTexture.ReadPixels(new Rect(0, 0, rTexture.width, rTexture.height), 0, 0);
        tempTexture.Apply();

        Color[] pixels = tempTexture.GetPixels();

        RenderTexture.active = null;
        Destroy(tempTexture);

        return pixels;
    }

    private void CreateInitialTexture2D()
    {   
        initialTexture2D = new Texture2D(renderTexture.width, renderTexture.height);
        initialTexture2D.SetPixels(initialPixels);
        initialTexture2D.Apply();
    }

    private void ModifyRenderTexturePixels(Vector2 textureCoordinate)
    {
        RenderTexture.active = renderTexture;

        Texture2D tempTexture = new Texture2D(renderTexture.width, renderTexture.height);
        tempTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tempTexture.Apply();

        var circleOrigin = new Vector2(textureCoordinate.x * renderTexture.width, textureCoordinate.y * renderTexture.height);
        Color[] pixels = tempTexture.GetPixels();

        for (int x = 0; x < renderTexture.width; x++)
        {
            for (int y = 0; y < renderTexture.height; y++)
            {
                Vector2 pixelPosition = new Vector2(x, y);
                float distance = Vector2.Distance(circleOrigin, pixelPosition);
                if (distance <= currentRadius)
                {
                    int pixelIndex = y * renderTexture.width + x;
                    if(currentBrushAction == BrushAction.Draw) pixels[pixelIndex] = brushColor;
                    else if(currentBrushAction == BrushAction.Erase) pixels[pixelIndex] = initialPixels[pixelIndex];
                }
            }
        }
        tempTexture.SetPixels(pixels);
        tempTexture.Apply();

        Graphics.Blit(tempTexture, renderTexture);

        RenderTexture.active = null;
        Destroy(tempTexture);
    }
    
    public void ResetTextureToDefault()
    {
        RenderTexture.active = renderTexture;
        Graphics.Blit(initialTexture2D, renderTexture);
        RenderTexture.active = null;  
    }
    #endregion
}

