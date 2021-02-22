﻿using System;
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
            }
        }
        
        GenerateMeshInUnityScene (scene);
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

        var skeletonRoot = SkeletonUtils.GenerateSkeletonForModel (h3DModel);
        if (skeletonRoot == null) {
            //Skeleton not present in model
        } else {
            SpawnBones (skeletonRoot, sceneGo, emptyGo);   
        }

        foreach (var h3DMesh in h3DModel.Meshes) {
            if (h3DMesh.Type == H3DMeshType.Silhouette) continue;
            
            var picaVertices = MeshTransform.GetWorldSpaceVertices (h3DModel.Skeleton, h3DMesh);
            foreach (var subMesh in h3DMesh.SubMeshes) {
                var subMeshName = h3DModel.MeshNodesTree.Find (h3DMesh.NodeIndex) + "_" +
                                  h3DModel.Meshes.IndexOf (h3DMesh) + "_" + h3DMesh.SubMeshes.IndexOf (subMesh);
                var modelGo = Instantiate (emptyGo, sceneGo.transform);
                modelGo.name = subMeshName;
                
                var meshFilter = modelGo.AddComponent<MeshFilter> ();
                var mesh = new Mesh ();

                var unityMeshPositions = new List<Vector3> ();
                var unityMeshTangents = new List<Vector4> ();
                var unityMeshNormals = new List<Vector3> ();
                var unityMeshUV = new List<Vector2> ();
                var unityMeshTriangles = new List<ushort> ();

                unityMeshPositions.AddRange (MeshUtils.PicaToUnityVertex (picaVertices));
                unityMeshNormals.AddRange (MeshUtils.PicaToUnityNormals (picaVertices));
                unityMeshTangents.AddRange (MeshUtils.PicaToUnityTangents (picaVertices));
                unityMeshUV.AddRange (MeshUtils.PicaToUnityUV (picaVertices));
                unityMeshTriangles.AddRange (subMesh.Indices);

                var unityVertexBones = new List<BoneWeight> ();

                if (subMesh.Skinning == H3DSubMeshSkinning.Smooth) {
                    foreach (var picaVertex in picaVertices) {
                        var vertexBoneWeight = new BoneWeight ();
                        for (var boneIndexInVertex = 0; boneIndexInVertex < 4; boneIndexInVertex++) {
                            var bIndex = picaVertex.Indices[boneIndexInVertex];
                            var weight = picaVertex.Weights[boneIndexInVertex];

                            if (weight == 0) break;

                            if (bIndex < subMesh.BoneIndices.Length && bIndex > -1)
                                bIndex = subMesh.BoneIndices[bIndex];
                            else
                                bIndex = 0;

                            if (boneIndexInVertex == 0) {
                                vertexBoneWeight.weight0 = weight;
                                vertexBoneWeight.boneIndex0 = bIndex;
                            } else if (boneIndexInVertex == 1) {
                                vertexBoneWeight.weight1 = weight;
                                vertexBoneWeight.boneIndex1 = bIndex;
                            } else if (boneIndexInVertex == 2) {
                                vertexBoneWeight.weight2 = weight;
                                vertexBoneWeight.boneIndex2 = bIndex;
                            } else if (boneIndexInVertex == 3) {
                                vertexBoneWeight.weight3 = weight;
                                vertexBoneWeight.boneIndex3 = bIndex;
                            }
                        }
                        unityVertexBones.Add (vertexBoneWeight);
                    }
                } else {
                    foreach (var picaVertex in picaVertices) {
                        var bIndex = picaVertex.Indices[0];

                        if (bIndex < subMesh.BoneIndices.Length && bIndex > -1)
                            bIndex = subMesh.BoneIndices[bIndex];
                        else
                            bIndex = 0;

                        var vertexBoneWeight = new BoneWeight {
                            boneIndex0 = bIndex,
                            weight0 = 1
                        };
                        unityVertexBones.Add (vertexBoneWeight);
                    }
                }
                
                
                mesh.subMeshCount = 1;
                mesh.vertices = unityMeshPositions.ToArray ();
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
                SaveMeshAtPath (mesh, "Assets/Raw/test/" + subMeshName + ".asset");
            }
        }
        DestroyImmediate (emptyGo);
    }

    
    
    private const float RadToDegConstant = (float)((1 / Math.PI) * 180);

    public static void SpawnBones (SkeletonUtils.SkeletonNode root, GameObject parentGo, GameObject nodeGo)
    {
        var rootGo = Instantiate (nodeGo, parentGo.transform);
        rootGo.transform.localScale = root.Scale;
        
        var postionAxises = new Vector3 (-1, 1, 1);
        var postionVector = root.Translation;
        rootGo.transform.localPosition = new Vector3 {
            x = postionAxises.x * postionVector.x,
            y = postionAxises.y * postionVector.y,
            z = postionAxises.z * postionVector.z
        };
        foreach (var singleRotation in root.Rotation) {
            var rotationVector = VectorExtensions.GetAxisFromRotation (singleRotation);
            rootGo.transform.Rotate (rotationVector, VectorExtensions.GetScalarFromRotation (singleRotation));
        }
        
        rootGo.name = root.Name;
        if (root.Nodes == null)
            return;
        foreach (var singleNode in root.Nodes) {
            SpawnBones (singleNode, rootGo, nodeGo);
        }
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