using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SPICA.Formats.Common;
using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Model;
using SPICA.PICA.Converters;
using SPICA.WinForms.Formats;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class RawImporter : MonoBehaviour
{
    // [MenuItem("MyMenu/Testing")]
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
        
        var h3DModel = h3DScene.Models[0];
        
        var emptyGo = new GameObject("EmptyGo");
        var sceneGo = new GameObject("Test");

        var modelGo = Instantiate (emptyGo, sceneGo.transform);
        modelGo.name = "Model";
        var meshFilter = modelGo.AddComponent<MeshFilter> ();
        
        var skeletonGo = Instantiate (emptyGo, sceneGo.transform);
        skeletonGo.name = "Model_Skeleton";
        var RootBoneId = "";
        bool skeletonPresent = false;
        if ((h3DModel.Skeleton?.Count ?? 0) > 0) {
            skeletonPresent = true;
            var ChildBones = new Queue<Tuple<H3DBone, SkeletonUtils.SkeletonNode>>();
            var RootNode = new SkeletonUtils.SkeletonNode ();
            ChildBones.Enqueue(Tuple.Create(h3DModel.Skeleton[0], RootNode));
            
            RootBoneId = $"#{h3DModel.Skeleton[0].Name}_bone_id";

            while (ChildBones.Count > 0)
            {
                Tuple<H3DBone, SkeletonUtils.SkeletonNode> Bone_Node = ChildBones.Dequeue();

                H3DBone Bone = Bone_Node.Item1;

                Bone_Node.Item2.id   = $"{Bone.Name}_bone_id";
                Bone_Node.Item2.name = Bone.Name;
                Bone_Node.Item2.sid  = Bone.Name;
                Bone_Node.Item2.type = SkeletonUtils.MeshUtilsSkeletonNodeType.JOINT;
                Bone_Node.Item2.Scale = VectorExtensions.CastNumericsVector3(Bone.Scale);
                Bone_Node.Item2.Translation = VectorExtensions.CastNumericsVector3 (Bone.Translation);
                Bone_Node.Item2.Rotation = VectorExtensions.CastNumericsVector3 (Bone.Rotation);

                foreach (H3DBone B in h3DModel.Skeleton)
                {
                    if (B.ParentIndex == -1) continue;

                    H3DBone ParentBone = h3DModel.Skeleton[B.ParentIndex];

                    if (ParentBone == Bone)
                    {
                        SkeletonUtils.SkeletonNode Node = new SkeletonUtils.SkeletonNode();

                        ChildBones.Enqueue(Tuple.Create(B, Node));

                        if (Bone_Node.Item2.Nodes == null) Bone_Node.Item2.Nodes = new List<SkeletonUtils.SkeletonNode>();

                        Bone_Node.Item2.Nodes.Add(Node);
                    }
                }
            }
            SpawnBones (RootNode, skeletonGo, emptyGo);
            
        }
        
        var whiteMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Scripts/RawImportScripts/TestMat.mat");

        // foreach (var h3DMesh in h3DModel.Meshes) {
        //     var matIndex = h3DModel.Materials[h3DMesh.MaterialIndex];
        //
        //     int VtxCount = 1, FAOffset = 0;
        //     if (h3DMesh.VertexStride > 0) {
        //         VtxCount = h3DMesh.RawBuffer.Length / h3DMesh.VertexStride;
        //         FAOffset = h3DMesh.RawBuffer.Length;
        //     }
        // }
        //
        //
        //
        var mesh = new Mesh ();
        
        
        
        // SubMesh approach
        var unityMeshVertices = new List<Vector3> ();
        var unityMeshTangents = new List<Vector4> ();
        var unityMeshNormals = new List<Vector3> ();
        var unityMeshUV = new List<Vector2> ();
        var unityMeshBones = new List<BoneWeight> ();
        var listOfTriangles = new List<List<ushort>> ();
        var matSubMeshDict = new Dictionary<ushort, List<ushort>> ();
        foreach (var h3DMesh in h3DModel.Meshes) {
            // var picaVertices = MeshTransform.GetWorldSpaceVertices(h3DModel.Skeleton, h3DMesh);
            var picaVertices = h3DMesh.GetVertices();
            // var picaVertices = MeshTransform.GetVerticesList (h3DModel.Skeleton, h3DMesh).ToArray ();
            unityMeshVertices.AddRange (MeshUtils.PicaToUnityVertex (picaVertices));
            
            unityMeshNormals.AddRange (MeshUtils.PicaToUnityNormals (picaVertices));
            unityMeshTangents.AddRange (MeshUtils.PicaToUnityTangents (picaVertices));
            unityMeshUV.AddRange (MeshUtils.PicaToUnityUV (picaVertices));
            
            // unityMeshBones.AddRange (MeshUtils.PicaToUnityBoneWeights (picaVertices));

            var combinedTrisForSubMesh = new List<ushort> ();
            List<BoneWeight1> boneWeight1s = new List<BoneWeight1> ();
            foreach (var subH3DMesh in h3DMesh.SubMeshes) {
                foreach (var VARIABLE in subH3DMesh.BoneIndices) {
                    // Debug.LogError(VARIABLE);
                    boneWeight1s.Add (new BoneWeight1 {
                        boneIndex = Convert.ToInt32(subH3DMesh.BoneIndices),
                        weight = 1f
                    });
                }
                byte[] bonesPerVertex = new byte[20];
                var bonesPerVertexArray = new NativeArray<byte> (bonesPerVertex, Allocator.Temp);
                var weightsArray = new NativeArray<BoneWeight1> (boneWeight1s.ToArray (), Allocator.Temp);
                combinedTrisForSubMesh.AddRange (subH3DMesh.Indices);
            }
            if (matSubMeshDict.ContainsKey (h3DMesh.MaterialIndex)) {
                matSubMeshDict[h3DMesh.MaterialIndex].AddRange (combinedTrisForSubMesh);
            } else {
                matSubMeshDict[h3DMesh.MaterialIndex] = combinedTrisForSubMesh;
            }

            listOfTriangles.Add (combinedTrisForSubMesh);
        }
        
        mesh.subMeshCount = matSubMeshDict.Count;
        mesh.vertices = unityMeshVertices.ToArray ();
        mesh.normals = unityMeshNormals.ToArray ();
        mesh.tangents = unityMeshTangents.ToArray ();
        mesh.uv = unityMeshUV.ToArray ();
        mesh.boneWeights = unityMeshBones.ToArray ();
        int counter = 0; //temp
        foreach (var kvp in matSubMeshDict) {
            mesh.SetTriangles (kvp.Value, counter++);
        }
        // for (int i = 0; i < listOfTriangles.Count; i++) {
        //     mesh.SetTriangles (listOfTriangles[i], i);
        // }
        // mesh.RecalculateBounds ();
        // mesh = ClearBlanks (mesh);
        
        if (skeletonPresent) {
            var meshRender = modelGo.AddComponent<SkinnedMeshRenderer> ();
            meshRender.material = whiteMat;
            meshRender.sharedMesh = mesh;
            meshRender.updateWhenOffscreen = true;
            
            var rootBoneTransform =  skeletonGo.transform.GetChild (0);
            meshRender.bones = rootBoneTransform.GetComponentsInChildren<Transform> ();
            meshRender.rootBone = rootBoneTransform;
            
            var unityBindPoses = new List<Matrix4x4>();
            foreach (var bone in meshRender.bones) {
                unityBindPoses.Add (bone.worldToLocalMatrix * rootBoneTransform.localToWorldMatrix);
            }
            // mesh.SetBoneWeights ();
            mesh.bindposes = unityBindPoses.ToArray ();
        } else {
            var meshRender = modelGo.AddComponent<MeshRenderer> ();
            meshRender.material = whiteMat;
        }
        meshFilter.sharedMesh = mesh;
        SaveMeshAtPath (meshFilter.mesh, "Assets/Raw/test.asset");
        DestroyImmediate (emptyGo);
        
        
        
        //Gizmos for vertices
        //var rawMeshRenderer = sceneGo.AddComponent<RawMeshRenderer> ();
        //rawMeshRenderer.vertices = new Dictionary<Color32, List<Vector3>> ();
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
    
    static Mesh ClearBlanks(Mesh mesh)
    {
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uv = mesh.uv;
        Vector3[] normals = mesh.normals;
        List<Vector3> vertList = vertices.ToList();
        List<Vector2> uvList = uv.ToList();
        List<Vector3> normalsList = normals.ToList();
        List<int> trianglesList = triangles.ToList();
 
        int testVertex = 0;
 
        while (testVertex < vertList.Count)
        {
            if (trianglesList.Contains(testVertex))
            {
                Debug.Log(testVertex);
                testVertex++;
            }
            else
            {
                vertList.RemoveAt(testVertex);
                uvList.RemoveAt(testVertex);
                normalsList.RemoveAt(testVertex);
 
                for (int i = 0; i < trianglesList.Count; i++)
                {
                    if (trianglesList[i] > testVertex)
                        trianglesList[i]--;
                }
            }
        }
 
        triangles = trianglesList.ToArray();
        vertices = vertList.ToArray();
        uv = uvList.ToArray();
        normals = normalsList.ToArray();
       
        mesh.triangles = triangles;
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.normals = normals;
        return mesh;
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