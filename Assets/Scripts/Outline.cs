using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class Outline : MonoBehaviour
{
    private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

    [Header("Shaders")]
    [SerializeField]
    private Shader _maskShader;
    
    [SerializeField]
    private Shader _fillShader;

    [Header("Settings")]
    [SerializeField]
    private Color outlineColor = Color.yellow;

    [SerializeField, Range(0f, 10f)]
    private float outlineWidth = 4f;

    private Renderer[] renderers;
    private Material outlineMaskMaterial;
    private Material outlineFillMaterial;

    public Color OutlineColor
    {
        get { return outlineColor; }
        set
        {
            outlineColor = value;
            if (outlineFillMaterial != null)
            {
                outlineFillMaterial.SetColor("_OutlineColor", outlineColor);
            }
        }
    }

    public float OutlineWidth
    {
        get { return outlineWidth; }
        set
        {
            outlineWidth = value;
            if (outlineFillMaterial != null)
            {
                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
            }
        }
    }

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        if (_maskShader == null || _fillShader == null)
        {
            Debug.LogError("[Outline] Шейдеры не назначены в инспекторе!");
            enabled = false;
            return;
        }

        outlineMaskMaterial = new Material(_maskShader);
        outlineFillMaterial = new Material(_fillShader);

        outlineMaskMaterial.name = "OutlineMask (Instance)";
        outlineFillMaterial.name = "OutlineFill (Instance)";

        // Настраиваем материал
        outlineFillMaterial.SetColor("_OutlineColor", outlineColor);
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);

        LoadSmoothNormals();
    }

    void OnEnable()
    {
        if (outlineMaskMaterial == null || outlineFillMaterial == null)
            return;

        foreach (var renderer in renderers)
        {
            var materials = renderer.sharedMaterials.ToList();
            materials.Add(outlineMaskMaterial);
            materials.Add(outlineFillMaterial);
            renderer.materials = materials.ToArray();
        }
    }

    void OnDisable()
    {
        if (renderers == null)
            return;

        foreach (var renderer in renderers)
        {
            if (renderer == null) continue;
            
            var materials = renderer.sharedMaterials.ToList();
            materials.Remove(outlineMaskMaterial);
            materials.Remove(outlineFillMaterial);
            renderer.materials = materials.ToArray();
        }
    }

    void OnDestroy()
    {
        if (outlineMaskMaterial != null)
            Destroy(outlineMaskMaterial);
        if (outlineFillMaterial != null)
            Destroy(outlineFillMaterial);
    }

    void LoadSmoothNormals()
    {
        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            if (!registeredMeshes.Add(meshFilter.sharedMesh))
                continue;

            var smoothNormals = SmoothNormals(meshFilter.sharedMesh);
            meshFilter.sharedMesh.SetUVs(3, smoothNormals);

            var renderer = meshFilter.GetComponent<Renderer>();
            if (renderer != null)
            {
                CombineSubmeshes(meshFilter.sharedMesh, renderer.sharedMaterials.Length);
            }
        }

        foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (!registeredMeshes.Add(skinnedMeshRenderer.sharedMesh))
                continue;

            skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];
            CombineSubmeshes(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials.Length);
        }
    }

    List<Vector3> SmoothNormals(Mesh mesh)
    {
        var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);
        var smoothNormals = new List<Vector3>(mesh.normals);

        foreach (var group in groups)
        {
            if (group.Count() == 1)
                continue;

            var smoothNormal = Vector3.zero;
            foreach (var pair in group)
            {
                smoothNormal += smoothNormals[pair.Value];
            }
            smoothNormal.Normalize();

            foreach (var pair in group)
            {
                smoothNormals[pair.Value] = smoothNormal;
            }
        }

        return smoothNormals;
    }

    void CombineSubmeshes(Mesh mesh, int materialsLength)
    {
        if (mesh.subMeshCount == 1)
            return;

        if (mesh.subMeshCount > materialsLength)
            return;

        mesh.subMeshCount++;
        mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
    }
}
