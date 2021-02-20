using System;
using System.Collections.Generic;
using System.Linq;
using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Formats.CtrH3D.Model.Mesh;
using SPICA.WinForms.Formats;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class RawImporter : MonoBehaviour
{
    [MenuItem("MyMenu/Testing")]
    static void TestImportRaw()
    {
        var scene = new H3D();

        int OpenFiles = 0;

        var FileNames = new []{"Assets/Raw/Models/0001 - Bulbasaur.bin"};
        foreach (string FileName in FileNames)
        {
            H3DDict<H3DBone> Skeleton = null;

            if (scene.Models.Count > 0) Skeleton = scene.Models[0].Skeleton;

            H3D Data = FormatIdentifier.IdentifyAndOpen(FileName, Skeleton);

            if (Data != null)
            {
                scene.Merge(Data);
                GenerateMeshInUnityScene (Data);
            }
        }
    }

    private void Awake ()
    {
        TestImportRaw ();
    }

    static void GenerateMeshInUnityScene (H3D h3DScene)
    {
        var toBeDestroyed = GameObject.Find ("Test");
        if (toBeDestroyed != null) {
            DestroyImmediate (toBeDestroyed);
        }

        var sceneGo = new GameObject("Test");
        
        var meshFilter = sceneGo.AddComponent<MeshFilter> ();
        var meshRender = sceneGo.AddComponent<MeshRenderer> ();

        var whiteMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Scripts/RawImportScripts/TestMat.mat");
        meshRender.material = whiteMat;

        var h3DModel = h3DScene.Models[0];

        var mesh = new Mesh ();
        var unityMeshVertices = new List<Vector3> ();
        var unityMeshTangents = new List<Vector4> ();
        var unityMeshNormals = new List<Vector3> ();
        var listOfTriangles = new List<List<ushort>> ();
        foreach (var h3DMesh in h3DModel.Meshes) {
            unityMeshVertices.AddRange (MeshUtils.PicaToUnityVertex (h3DMesh.GetVertices ()));
            unityMeshNormals.AddRange (MeshUtils.PicaToUnityNormals (h3DMesh.GetVertices ()));
            unityMeshTangents.AddRange (MeshUtils.PicaToUnityTangents (h3DMesh.GetVertices ()));
            
            var combinedTrisForSubMesh = new List<ushort> ();
            foreach (var subH3DMesh in h3DMesh.SubMeshes) {
                combinedTrisForSubMesh.AddRange (subH3DMesh.Indices);
            }
            listOfTriangles.Add (combinedTrisForSubMesh);
        }
        
        mesh.subMeshCount = listOfTriangles.Count;
        mesh.vertices = unityMeshVertices.ToArray ();
        mesh.normals = unityMeshNormals.ToArray ();
        mesh.tangents = unityMeshTangents.ToArray ();
        for (int i = 0; i < listOfTriangles.Count; i++) {
            mesh.SetTriangles (listOfTriangles[i], i);
        }
        meshFilter.mesh = mesh;


        var rawMeshRenderer = sceneGo.AddComponent<RawMeshRenderer> ();
        rawMeshRenderer.vertices = new Dictionary<Color32, List<Vector3>> ();
        
        
        //
        // foreach (var h3DMesh in h3DModel.Meshes) {
        //     if (h3DMesh.Type == H3DMeshType.Silhouette) continue;
        //     Debug.LogError (h3DMesh.MaterialIndex);
        //     var gizmoColor = new Color32 (
        //         (byte)Random.Range (0,255),
        //         (byte)Random.Range (0,255),
        //         (byte)Random.Range (0,255),
        //         255
        //     );
        //     rawMeshRenderer.vertices[gizmoColor] = MeshUtils.PicaToUnityVertex (h3DMesh.GetVertices ());
        // }


        // var subList0 = h3DModel.MeshesLayer0.OrderBy (x => x.Priority);
        // var subList1 = h3DModel.MeshesLayer1.OrderBy (x => x.Priority);
        // var subList2 = h3DModel.MeshesLayer2.OrderBy (x => x.Priority);
        // var subList3 = h3DModel.MeshesLayer3.OrderBy (x => x.Priority);
        // var subLists = new[]{subList0, subList1, subList2, subList3};
        // foreach (var subList in subLists) {
        //     var gizmoColor = new Color32 (
        //         (byte)Random.Range (0,255),
        //         (byte)Random.Range (0,255),
        //         (byte)Random.Range (0,255),
        //         255
        //         );
        //     var listOfVertices = new List<Vector3> ();
        //     foreach (var subListMesh in subList) {
        //         listOfVertices.AddRange (MeshUtils.PicaToUnityVertex (subListMesh.GetVertices ()));
        //     }
        //     rawMeshRenderer.vertices[gizmoColor] = listOfVertices;
        // }
        
    }
}