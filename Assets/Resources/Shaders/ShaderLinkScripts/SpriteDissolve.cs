using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


#if UNITY_EDITOR
using UnityEditor;
#endif
public class SpriteDissolve : MonoBehaviour
{
    [SerializeField] private Shader dissolveShader;
    [SerializeField] private Image targetImage;
    [SerializeField] private RawImage targetRawImage;
    [SerializeField,Range(0,1)] private float dissolveValue = 0;
    [SerializeField] private Color dissolveEdgeColor = Color.cyan;
    private int dissolvePropertyId = -1;
    private int dissolveEdgeColorPropertyId = -1;
    private Material _dissolveMaterial;

    private Material dissolveMaterial
    {
        get
        {
            if(_dissolveMaterial == null)
            {
                _dissolveMaterial = new Material(dissolveShader);
            }
            if (targetImage != null)
            {
                targetImage.material = _dissolveMaterial;
            }
            if(targetRawImage != null)
            {
                targetRawImage.material = _dissolveMaterial;
            }
            return _dissolveMaterial;
        }
    }

    public float DissolveValue
    {
        get
        {
            return dissolveValue;
        }
        set
        {
            if (dissolvePropertyId == -1)
            {
                FindDissolveValueProperty();
            }
            if (dissolvePropertyId != -1 && dissolveMaterial != null)
            {
                dissolveMaterial.SetFloat(dissolvePropertyId, value);
                dissolveValue = value;
            }
        }
    }
    public Color DissolveEdgeColor
    {
        get
        {
            return dissolveEdgeColor;
        }
        set
        {
            if (dissolveEdgeColorPropertyId == -1)
            {
                FindDissolveValueProperty();
            }
            if (dissolveEdgeColorPropertyId != -1 && dissolveMaterial != null)
            {
                dissolveMaterial.SetColor(dissolveEdgeColorPropertyId, value);
                dissolveEdgeColor = value;

            }
        }
    }
    private void FindDissolveValueProperty()
    {
        if (dissolveMaterial != null)
        {
            dissolvePropertyId = -1;
            dissolveEdgeColorPropertyId = -1;
            for (int i = 0; i < dissolveMaterial.shader.GetPropertyCount(); i++) 
            { 
                if (dissolveMaterial.shader.GetPropertyName(i) == "_DissolveValue")
                {
                    dissolvePropertyId = dissolveMaterial.shader.GetPropertyNameId(i);
                }
                if (dissolveMaterial.shader.GetPropertyName(i) == "_EdgeColor")
                {
                    dissolveEdgeColorPropertyId = dissolveMaterial.shader.GetPropertyNameId(i);
                }
            }
        }
    }

    protected void Update()
    {
        DissolveValue = dissolveValue;
        DissolveEdgeColor = dissolveEdgeColor;
    }
#if UNITY_EDITOR
    protected void OnValidate()
    {
        DissolveValue = dissolveValue;
        DissolveEdgeColor = dissolveEdgeColor;
    }
#endif
}
