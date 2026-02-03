using System.Collections.Generic;
using UnityEngine;

// Generates a simple upright cone mesh at edit/runtime for arrow heads.
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
public class ProceduralCone : MonoBehaviour
{
    [SerializeField] private float radius = 0.015f;
    [SerializeField] private float height = 0.05f;
    [SerializeField, Range(3, 64)] private int segments = 18;

    private MeshFilter meshFilter;
    private bool generatedOnce;

    private void Awake()
    {
        EnsureMesh();
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        // Delay to avoid running during import/validate where SendMessage is blocked.
        UnityEditor.EditorApplication.delayCall += GenerateIfActive;
#else
        EnsureMesh();
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall -= GenerateIfActive;
#endif
        generatedOnce = false;
    }

    private void GenerateIfActive()
    {
        if (this == null || !isActiveAndEnabled)
        {
            return;
        }
        EnsureMesh();
    }

    private void EnsureMesh()
    {
        if (radius <= 0f || height <= 0f || segments < 3)
        {
            return;
        }

        if (meshFilter == null && !TryGetComponent(out meshFilter))
        {
            // If a MeshFilter was stripped by import, bail out quietly to avoid OnValidate spam.
            return;
        }

        var mesh = meshFilter.sharedMesh;
        if (mesh == null)
        {
            mesh = new Mesh { name = "ProceduralCone" };
            meshFilter.sharedMesh = mesh;
        }
        else
        {
            mesh.Clear();
            mesh.name = "ProceduralCone";
        }

        BuildCone(mesh);
        generatedOnce = true;
    }

    private void BuildCone(Mesh mesh)
    {
        var verts = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var indices = new List<int>();

        // Tip vertex
        verts.Add(new Vector3(0f, height, 0f));
        normals.Add(Vector3.up);
        uvs.Add(new Vector2(0.5f, 1f));

        // Base center
        verts.Add(Vector3.zero);
        normals.Add(Vector3.down);
        uvs.Add(new Vector2(0.5f, 0f));

        float angleStep = Mathf.PI * 2f / segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = angleStep * i;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            var pos = new Vector3(x, 0f, z);

            verts.Add(pos);

            // Normal approximated per vertex for smooth shading
            var sideNormal = new Vector3(x, radius, z).normalized;
            normals.Add(sideNormal);

            uvs.Add(new Vector2((float)i / segments, 0f));
        }

        // Side triangles
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            int tip = 0;
            int baseVert = 2 + i;
            int baseNext = 2 + next;
            indices.Add(tip);
            indices.Add(baseVert);
            indices.Add(baseNext);
        }

        // Base triangles
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            int center = 1;
            int baseVert = 2 + i;
            int baseNext = 2 + next;
            indices.Add(center);
            indices.Add(baseNext);
            indices.Add(baseVert);
        }

        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateBounds();
    }
}
