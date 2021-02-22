using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SPICA.Formats.Common;
using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Formats.CtrH3D.Model.Mesh;
using SPICA.Formats.Generic.COLLADA;
using SPICA.PICA.Converters;
using SPICA.WinForms.Formats;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;
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
        
        var test = new DAE(h3DScene, 0);
        
        //TODO: Material setup
        var whiteMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Scripts/RawImportScripts/TestMat.mat");

        var meshCounter = 0;
        
        SpawnBones (test.library_visual_scenes[0].node[0], sceneGo, emptyGo);

        var RootBoneId = string.Empty;
        //
        // if ((h3DModel.Skeleton?.Count ?? 0) > 0)
        // {
        //     
        //     var ChildBones = new Queue<Tuple<H3DBone, SkeletonUtils.SkeletonNode>>();
        //
        //     var RootNode = new SkeletonUtils.SkeletonNode();
        //
        //     ChildBones.Enqueue(Tuple.Create(h3DModel.Skeleton[0], RootNode));
        //
        //     RootBoneId = $"#{h3DModel.Skeleton[0].Name}_bone_id";
        //
        //     while (ChildBones.Count > 0)
        //     {
        //         var Bone_Node = ChildBones.Dequeue();
        //
        //         var Bone = Bone_Node.Item1;
        //
        //         Bone_Node.Item2.id   = $"{Bone.Name}_bone_id";
        //         Bone_Node.Item2.name = Bone.Name;
        //         Bone_Node.Item2.sid  = Bone.Name;
        //         Bone_Node.Item2.type = SkeletonUtils.MeshUtilsSkeletonNodeType.JOINT;
        //         Bone_Node.Item2.Translation =  VectorExtensions.CastNumericsVector3 (Bone.Translation);
        //         // Bone_Node.Item2.SetBoneEuler(Bone.Translation, Bone.Rotation, Bone.Scale);
        //         var rotVec = new Vector3 (RadToDegConstant * Bone.Rotation.X,
        //             RadToDegConstant * Bone.Rotation.Y, RadToDegConstant * Bone.Rotation.Z);
        //         Bone_Node.Item2.Rotation = rotVec;
        //         Bone_Node.Item2.Scale = VectorExtensions.CastNumericsVector3 (Bone.Scale);
        //
        //
        //         foreach (H3DBone B in h3DModel.Skeleton)
        //         {
        //             if (B.ParentIndex == -1) continue;
        //
        //             H3DBone ParentBone = h3DModel.Skeleton[B.ParentIndex];
        //
        //             if (ParentBone == Bone)
        //             {
        //                 SkeletonUtils.SkeletonNode Node = new SkeletonUtils.SkeletonNode();
        //
        //                 ChildBones.Enqueue(Tuple.Create(B, Node));
        //
        //                 if (Bone_Node.Item2.Nodes == null) Bone_Node.Item2.Nodes = new List<SkeletonUtils.SkeletonNode>();
        //
        //                 Bone_Node.Item2.Nodes.Add(Node);
        //             }
        //         }
        //     }
        //
        //     SpawnBones(RootNode, sceneGo, emptyGo);
        // }
        //
        //
        
        
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
            var unityVertexBones = new List<BoneWeight> ();

            var picaVertices = h3DMesh.GetVertices ();
            unityMeshVertices.AddRange (MeshUtils.PicaToUnityVertex (picaVertices));
            unityMeshNormals.AddRange (MeshUtils.PicaToUnityNormals (picaVertices));
            unityMeshTangents.AddRange (MeshUtils.PicaToUnityTangents (picaVertices));
            unityMeshUV.AddRange (MeshUtils.PicaToUnityUV (picaVertices));
            unityVertexBones.AddRange (MeshUtils.PicaToUnityBoneWeights (picaVertices));
            
            List<BoneWeight1> currentMeshBoneWeights = new List<BoneWeight1> ();
            
            foreach (var subH3DMesh in h3DMesh.SubMeshes) {
                unityMeshTriangles.AddRange (subH3DMesh.Indices);
            }

            
            mesh.subMeshCount = 1;
            mesh.vertices = unityMeshVertices.ToArray ();
            mesh.normals = unityMeshNormals.ToArray ();
            mesh.tangents = unityMeshTangents.ToArray ();
            mesh.uv = unityMeshUV.ToArray ();
            mesh.SetTriangles (unityMeshTriangles ,0);
            
            mesh.boneWeights = unityVertexBones.ToArray (); 

            var meshRenderer = modelGo.AddComponent<SkinnedMeshRenderer> ();
            meshRenderer.quality = SkinQuality.Bone4;
            meshRenderer.material = whiteMat;
            meshRenderer.sharedMesh = mesh;
            var bonesTransform = sceneGo.transform.GetChild (0).GetComponentsInChildren<Transform> ();
            meshRenderer.rootBone = bonesTransform[0];
            meshRenderer.bones = bonesTransform;
            meshRenderer.updateWhenOffscreen = true;
            var bindPoses = new List<Matrix4x4> ();
            for (int i = 0; i < bonesTransform.Length; i++) {
                bindPoses.Add (bonesTransform[i].worldToLocalMatrix * bonesTransform[0].localToWorldMatrix);
            }
            mesh.bindposes = bindPoses.ToArray ();
            
            meshFilter.sharedMesh = mesh;
            SaveMeshAtPath (mesh, "Assets/Raw/test/test" + meshCounter++ + ".asset");
        }
        DestroyImmediate (emptyGo);
    }
    
    private const float RadToDegConstant = (float)((1 / Math.PI) * 180);

    public static void SpawnBones (DAENode root, GameObject parentGo, GameObject nodeGo)
    {
        var rootGo = Instantiate (nodeGo, parentGo.transform);
        rootGo.transform.localScale = DAEUtils.ToUnityVector3 (root.Scale);
        
        var postionAxises = new Vector3 (-1, 1, 1);
        var postionVector =  DAEUtils.ToUnityVector3 (root.Translation);
        rootGo.transform.localPosition = new Vector3 {
            x = postionAxises.x * postionVector.x,
            y = postionAxises.y * postionVector.y,
            z = postionAxises.z * postionVector.z
        };
        foreach (var singleRotation in root.Rotation) {
            var rotationVector = DAEUtils.GetAxisFromRotation (singleRotation);
            rootGo.transform.Rotate (rotationVector, DAEUtils.GetScalarFromRotation (singleRotation));
        }
        
        rootGo.name = root.name;
        if (root.Nodes == null)
            return;
        foreach (var singleNode in root.Nodes) {
            SpawnBones (singleNode, rootGo, nodeGo);
        }
    }
    
    public static void SpawnBones (SkeletonUtils.SkeletonNode root, GameObject parentGo, GameObject nodeGo)
    {
        do {
            var rotQuat = Quaternion.Euler (root.Rotation);
            var rootGo = Instantiate (nodeGo, parentGo.transform);
            rootGo.transform.position = root.Translation;
            rootGo.transform.Rotate (new Vector3 (1,0,0), root.Rotation.x);
            rootGo.transform.Rotate (new Vector3 (0,1,0), root.Rotation.y);
            rootGo.transform.Rotate (new Vector3 (0,0,1), root.Rotation.z);
            
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