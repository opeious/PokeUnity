using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SPICA.Formats.Common;
using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Formats.CtrH3D.Model.Mesh;
using SPICA.PICA.Converters;
using SPICA.WinForms.Formats;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class RawImporter2 : MonoBehaviour
{
    [MenuItem("MyMenu/Testing")]
    static void TestImportRaw()
    {
        var scene = new H3D();

        int OpenFiles = 0;

        var FileNames = new []{"Assets/Raw/Models/0011 - Squirtle.bin"};
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

    static void GenerateMeshInUnityScene (H3D h3DScene)
    {
        //To be removed after testing
        var toBeDestroyed = GameObject.Find ("Test");
        if (toBeDestroyed != null) {
            DestroyImmediate (toBeDestroyed);
        }
        
        var h3DModel = h3DScene.Models[0];
        
        var emptyGo = new GameObject("EmptyGo");
        var sceneGo = new GameObject("Test");
        
        
        //TODO: Material setup
        var whiteMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Scripts/RawImportScripts/TestMat.mat");

        var meshCounter = 0;
        foreach (var h3DMesh in h3DModel.Meshes) {
            
            //TODO: Skeleton import

            var modelGo = Instantiate (emptyGo, sceneGo.transform);
            modelGo.name = h3DModel.Name + meshCounter;
            var meshFilter = modelGo.AddComponent<MeshFilter> ();
            var mesh = new Mesh ();
            var unityMeshVertices = new List<Vector3> ();
            var unityMeshTangents = new List<Vector4> ();
            var unityMeshNormals = new List<Vector3> ();
            var unityMeshUV = new List<Vector2> ();
            var unityMeshTriangles = new List<ushort> ();
            var unityVertexBones = new List<float> ();

            var picaVertices = h3DMesh.GetVertices ();
            unityMeshVertices.AddRange (MeshUtils.PicaToUnityVertex (picaVertices));
            unityMeshNormals.AddRange (MeshUtils.PicaToUnityNormals (picaVertices));
            unityMeshTangents.AddRange (MeshUtils.PicaToUnityTangents (picaVertices));
            unityMeshUV.AddRange (MeshUtils.PicaToUnityUV (picaVertices));
            
            List<BoneWeight1> currentMeshBoneWeights = new List<BoneWeight1> ();
            
            var bonesPerVertexArray = new NativeArray<byte> ();
            var weightsArray = new NativeArray<BoneWeight1> ();
            foreach (var subH3DMesh in h3DMesh.SubMeshes) {
                unityMeshTriangles.AddRange (subH3DMesh.Indices);

                var vertexWeightage = new List<byte> ();
                foreach (var picaVertex in picaVertices) {
                    vertexWeightage.Add (1);
                    foreach (var singleBoneIndex in subH3DMesh.BoneIndices) {
                        currentMeshBoneWeights.Add (new BoneWeight1 {
                            boneIndex = Convert.ToInt32 (singleBoneIndex),
                            weight = 1f
                        });
                        break;
                    }
                }
                bonesPerVertexArray = new NativeArray<byte> (vertexWeightage.ToArray (), Allocator.Temp);
                weightsArray = new NativeArray<BoneWeight1> (currentMeshBoneWeights.ToArray (), Allocator.Temp);
            }

            
            mesh.subMeshCount = 1;
            mesh.vertices = unityMeshVertices.ToArray ();
            mesh.normals = unityMeshNormals.ToArray ();
            mesh.tangents = unityMeshTangents.ToArray ();
            mesh.uv = unityMeshUV.ToArray ();
            mesh.SetTriangles (unityMeshTriangles ,0);
            mesh.SetBoneWeights (bonesPerVertexArray , weightsArray);

            
            var meshRenderer = modelGo.AddComponent<SkinnedMeshRenderer> ();
            meshRenderer.material = whiteMat;
            meshRenderer.sharedMesh = mesh;
            meshFilter.sharedMesh = mesh;
            SaveMeshAtPath (meshFilter.mesh, "Assets/Raw/test/test" + meshCounter++ + ".asset");
        }
        DestroyImmediate (emptyGo);
    }
    
    private const float RadToDegConstant = (float)((1 / Math.PI) * 180);

    public static void SpawnBones (SkeletonUtils.SkeletonNode root, GameObject parentGo, GameObject nodeGo)
    {
        do {
            var rotQuat = new Quaternion ();
            rotQuat.SetEulerRotation (root.Rotation.x, root.Rotation.y, root.Rotation.z);
            var rootGo = Instantiate (nodeGo, root.Translation, 
                rotQuat, parentGo.transform);
            
            rootGo.name = root.name;
            if (root.Nodes != null) {
                foreach (var node in root.Nodes) {
                    SpawnBones (node, rootGo, nodeGo);
                }
                root.Nodes = null;   
            }
        } while (root.Nodes != null);
    }

    public static void SaveMeshAtPath (Mesh mesh, string path)
    {
        if (File.Exists (path)) {
            File.Delete (path);
        }
        AssetDatabase.CreateAsset (mesh, path);
        AssetDatabase.SaveAssets ();
    }
}