using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using SPICA.Formats.Common;
using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Formats.CtrH3D.Model.Mesh;
using SPICA.Formats.Generic.COLLADA;
using SPICA.PICA.Commands;
using SPICA.PICA.Converters;
using SPICA.WinForms.Formats;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEditor.Animations.Rigging;
using Random = UnityEngine.Random;

public class RawImporter2 : MonoBehaviour
{
    [MenuItem("MyMenu/Testing")]
    private static void TestImportRaw()
    {
        var scene = new H3D();

        var openFiles = 0;

        var fileNames = new []{"Assets/Raw/Models/0001 - Bulbasaur.bin","Assets/Raw/Textures/0001 - Bulbasaur.bin"};
        foreach (var fileName in fileNames)
        {
            H3DDict<H3DBone> skeleton = null;

            if (scene.Models.Count > 0) skeleton = scene.Models[0].Skeleton;

            var data = FormatIdentifier.IdentifyAndOpen(fileName, skeleton);

            if (data != null)
            {
                scene.Merge(data);
            }
        }
        
        GenerateMeshInUnityScene (scene);
        GenerateTextureFiles (scene);
    }

    private static void GenerateTextureFiles (H3D h3DScene)
    {
        foreach (var h3DMaterial in h3DScene.Models[0].Materials) {
            var color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            var width = h3DMaterial.Texture0.Width;
            var height = h3DMaterial.Texture0.Height;
            // h3DTexture.Format\
            var colorArray = new List<Color32> ();
            var buffer = h3DMaterial.Texture0.ToRGBA ();
            for (int i = 0; i < buffer.Length; i += 4) {
                var col = new Color32 ((byte)buffer[i + 0], buffer[i + 1], buffer[i + 2],
                    buffer[i + 3]);
                colorArray.Add (col);
            }
            var texture = new Texture2D (width, height, TextureFormat.ARGB32, false) {name = h3DMaterial.Name + "m"};
            int colorCounter = 0;
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    // color = ((x & y) != 0 ? Color.white : Color.gray);
                    texture.SetPixel(x, y, colorArray[colorCounter++]);
                }
            }

            // texture.LoadImage (h3DTexture.RawBuffer);
            // texture.LoadRawTextureData (h3DTexture.RawBuffer);
            texture.Apply();
            File.WriteAllBytes ("Assets/Raw/test/" + texture.name + ".png", texture.EncodeToPNG ());
        }
        
        var textureDict = new Dictionary<string, Texture2D> ();
        foreach (var h3DTexture in h3DScene.Textures) {
            var color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            var width = h3DTexture.Width;
            var height = h3DTexture.Height;

            var colorArray = new List<Color32> ();
            var buffer = h3DTexture.ToRGBA ();
            for (var i = 0; i < buffer.Length; i += 4) {
                var col = new Color32 ((byte)buffer[i + 0], buffer[i + 1], buffer[i + 2],
                    buffer[i + 3]);
                colorArray.Add (col);
            }
            var texture = new Texture2D (width, height, TextureFormat.ARGB32, false) {name = h3DTexture.Name};
            var colorCounter = 0;
            for (var y = 0; y < texture.height; y++)
            {
                for (var x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, y, colorArray[colorCounter++]);
                }
            }

            texture.Apply();
            File.WriteAllBytes ("Assets/Raw/test/" + texture.name + ".png", texture.EncodeToPNG ());
        }
        AssetDatabase.Refresh();
    }

    
    // public static class TextureExtensions {
    //     public static TextureFormat ToGFTextureFormat(this TextureFormat Format) {
    //         switch (Format) {
    //             case PICATextureFormat.RGB565: return TextureFormat.RGB565;
    //             case PICATextureFormat.RGB8: return TextureFormat.RGB8;
    //             case PICATextureFormat.RGBA8: return TextureFormat.RGBA8;
    //             case PICATextureFormat.RGBA4: return TextureFormat.RGBA4;
    //             case PICATextureFormat.RGBA5551: return TextureFormat.RGBA5551;
    //             case PICATextureFormat.LA8: return TextureFormat.LA8;
    //             case PICATextureFormat.HiLo8: return TextureFormat.HiLo8;
    //             case PICATextureFormat.L8: return TextureFormat.L8;
    //             case PICATextureFormat.A8: return TextureFormat.A8;
    //             case PICATextureFormat.LA4: return TextureFormat.LA4;
    //             case PICATextureFormat.L4: return TextureFormat.L4;
    //             case PICATextureFormat.A4: return TextureFormat.A4;
    //             case PICATextureFormat.ETC1: return TextureFormat.ETC1;
    //             case PICATextureFormat.ETC1A4: return TextureFormat.ETC1A4;
    //
    //             default: throw new ArgumentException("Invalid format!");
    //         }
    //     }
    // }
    
    private static void GenerateMeshInUnityScene (H3D h3DScene)
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

                            switch (boneIndexInVertex) {
                                case 0:
                                    vertexBoneWeight.weight0 = weight;
                                    vertexBoneWeight.boneIndex0 = bIndex;
                                    break;
                                case 1:
                                    vertexBoneWeight.weight1 = weight;
                                    vertexBoneWeight.boneIndex1 = bIndex;
                                    break;
                                case 2:
                                    vertexBoneWeight.weight2 = weight;
                                    vertexBoneWeight.boneIndex2 = bIndex;
                                    break;
                                case 3:
                                    vertexBoneWeight.weight3 = weight;
                                    vertexBoneWeight.boneIndex3 = bIndex;
                                    break;
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
                mesh.bindposes = bonesTransform
                    .Select (t => t.worldToLocalMatrix * bonesTransform[0].localToWorldMatrix).ToArray ();
            
                meshFilter.sharedMesh = mesh;
                SaveMeshAtPath (mesh, "Assets/Raw/test/" + subMeshName + ".asset");
            }
        }
        DestroyImmediate (emptyGo);
    }

    
    
    private const float RadToDegConstant = (float)((1 / Math.PI) * 180);

    private static void SpawnBones (SkeletonUtils.SkeletonNode root, GameObject parentGo, GameObject nodeGo)
    {
        var rootGo = Instantiate (nodeGo, parentGo.transform);
        rootGo.transform.localScale = root.Scale;

        var positionAxes = new Vector3 (-1, 1, 1);
        var positionVector = root.Translation;
        rootGo.transform.localPosition = new Vector3 {
            x = positionAxes.x * positionVector.x,
            y = positionAxes.y * positionVector.y,
            z = positionAxes.z * positionVector.z
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

    private static void SaveMeshAtPath ([NotNull] Mesh mesh, string path)
    {
        if (mesh == null) throw new ArgumentNullException (nameof(mesh));
        if (File.Exists (path)) {
            File.Delete (path);
        }
        AssetDatabase.CreateAsset (mesh, path);
        AssetDatabase.SaveAssets ();
    }
}