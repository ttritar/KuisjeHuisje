using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class CopyLightMap : MonoBehaviour
{
    [SerializeField] private GameObject _originalRenderer;
    void Start()
    {
        if (_originalRenderer == null)
        {
            Debug.LogError("Original Renderer is not assigned!");
            return;
        }

        MeshRenderer[] originalRenderers = _originalRenderer.GetComponentsInChildren<MeshRenderer>();
        MeshRenderer[] duplicateRenderers = GetComponentsInChildren<MeshRenderer>();

        for (int i = 0; i < originalRenderers.Length; i++)
        {
            if (i >= duplicateRenderers.Length) break;
            duplicateRenderers[i].lightmapIndex = originalRenderers[i].lightmapIndex;
            duplicateRenderers[i].lightmapScaleOffset = originalRenderers[i].lightmapScaleOffset;
        }
    }
}
