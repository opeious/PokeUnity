using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class PokemonAssetImporter : AssetPostprocessor
{
    [MenuItem("MyMenu/Update Pokemon Loader with Models")]
    static void UpdatePokemonLoader()
    {
        var x = AssetDatabase.FindAssets ("t:Model");
        var y = new List<GameObject> ();
        foreach (var z in x) {
            y.Add(AssetDatabase.LoadAssetAtPath<GameObject> (AssetDatabase.GUIDToAssetPath (z)));
        }
        var loader = Object.FindObjectOfType<PokemonLoader> ();
        loader.pokemonModels = y;
    }
     
    [MenuItem("MyMenu/Do Something")]
    static void DoSomething()
    {
        // Debug.Log("Set all textures to mirror U and repeat v...");
        // var x = Directory.GetDirectories ("Assets/Pokemon");
        // foreach (var folder in x) {
        //     var y = Directory.GetFiles (folder);
        //     foreach (var z in y) {
        //         if (z.Contains (".tga") && !z.Contains (".meta")) {
        //             // Debug.LogError (z);
        //             var importer = (TextureImporter) AssetImporter.GetAtPath (z);
        //             importer.wrapModeU = TextureWrapMode.Mirror;
        //             importer.wrapModeV = TextureWrapMode.Repeat;
        //             AssetDatabase.ImportAsset (z, ImportAssetOptions.ForceUpdate);
        //         }
        //     }
        // }

        // Debug.Log("Delete amie and normal textures...");
        // var x = Directory.GetDirectories ("Assets/Pokemon");
        // foreach (var folder in x) {
        //     var y = Directory.GetFiles (folder);
        //     foreach (var z in y) {
        //         if (z.Contains ("_amie") || z.Contains ("Nor.tga") || z.Contains ("Nor_bak.tga")) {
        //             Debug.LogError (z.ToString ());
        //             // File.Delete (z);
        //         }
        //     }
        // }
    }
    
    private void HandleAnimationClips (ModelImporter modelImporter)
    {
        
        var animClips = modelImporter.defaultClipAnimations;
        foreach (var animClip in animClips) {
            animClip.loopTime = true;
        }

        modelImporter.clipAnimations = animClips;
    }

    private void MoveToAppropriateFolder (ModelImporter modelImporter)
    {
        var pathArray = modelImporter.assetPath.Split ('/');
        var processingCurrently = pathArray.Last ().Split ('.')[0];
        var newFolderPath = "Assets/Pokemon/" + processingCurrently;
        var newFilePath = newFolderPath + '/' + pathArray.Last ();
        
        try {
            System.IO.Directory.CreateDirectory (newFolderPath);
        } catch {
            // ignore if already there
        }
        
        var filesInNewPath = System.IO.Directory.GetFiles (newFolderPath);
        if (!filesInNewPath.Contains (pathArray.Last ())) {
            AssetDatabase.MoveAsset (modelImporter.assetPath, newFilePath);
        }
    }

    private void FixTexture (string texPath)
    {
        TextureImporter importer = (TextureImporter) AssetImporter.GetAtPath (texPath);
        if (texPath.Contains ("Nor.tga")) {
            // Texture2D texture2D = (Texture2D) AssetDatabase.LoadAssetAtPath (texPath, typeof(Texture2D));
            importer.textureType = TextureImporterType.NormalMap;
        }

        importer.wrapModeU = TextureWrapMode.Mirror;
        importer.wrapModeV = TextureWrapMode.Repeat;
        AssetDatabase.ImportAsset (texPath, ImportAssetOptions.ForceUpdate);
    }

    private void HandleTextures (GameObject g, ModelImporter modelImporter)
    {
        var pathArray = modelImporter.assetPath.Split ('/');
        var processingCurrently = pathArray.Last ().Split ('.')[0];
        var materialFolderPath = "Assets/Pokemon/" + processingCurrently + "/";
        var textureFolderPath = "Assets/Pokemon/" + processingCurrently + "/";

        var renderer = g.GetComponentInChildren<SkinnedMeshRenderer> ();
        foreach (var mat in renderer.sharedMaterials) {
            if (!System.IO.Directory.Exists (materialFolderPath)) {
                Directory.CreateDirectory (materialFolderPath);
            }
            var matPath = materialFolderPath + mat.name.Split ('.')[0] + ".mat";
            Material newMat;
            var created = false;
            if (System.IO.File.Exists (matPath)) {
                newMat = AssetDatabase.LoadAssetAtPath<Material> (matPath);
            } else {
                created = true;
                newMat = new Material (Shader.Find ("Shader Graphs/LitPokemonShader"));
            }

            

            var textureAssetPath = textureFolderPath + mat.name.Split (' ')[0] + ".png";
            if (!textureAssetPath.Contains (".tga")) {
                textureAssetPath = textureAssetPath.Replace (".png", ".tga.png");
            }

            var iris = false;
            if (textureAssetPath.Contains ("Iris")) {
                // if (!created) {
                //     File.Delete (matPath);
                //     created = true;
                // }
                newMat = new Material (Shader.Find ("Shader Graphs/LitPokemonIrisShader"));
                iris = true;
            }
            

            if (File.Exists (textureAssetPath)) {
                FixTexture (textureAssetPath);
            }
            Texture2D texture2D = (Texture2D) AssetDatabase.LoadAssetAtPath (textureAssetPath, typeof(Texture2D));
            newMat.SetTexture ("_BaseMap", texture2D);
            if (iris) {
                newMat.SetVector ("_BaseMapTiling", new Vector4 (4,4,0,0));
                newMat.SetVector ("_BaseMapOffset", new Vector4 (1,0,0,0));
            }
            

            var detailAssetPath = textureAssetPath.Replace ("1.tga", "2.tga");
            if (File.Exists (detailAssetPath)) {
                FixTexture (detailAssetPath);
            }
            texture2D = (Texture2D) AssetDatabase.LoadAssetAtPath (detailAssetPath, typeof(Texture2D));
            if (texture2D != null) {
                newMat.SetTexture ("_DetailMap", texture2D);
            }
            
            var normalAssetPath = textureAssetPath.Replace ("1.tga", "Nor.tga");
            if (File.Exists (normalAssetPath)) {
                FixTexture (normalAssetPath);
            }
            texture2D = (Texture2D) AssetDatabase.LoadAssetAtPath (normalAssetPath, typeof(Texture2D));
            if (texture2D != null) {
                newMat.SetTexture ("_NormalMap", texture2D);
            }

            if (iris) {
                var maskAssetPath = textureAssetPath.Replace ("Iris", "Eye");
                texture2D = (Texture2D) AssetDatabase.LoadAssetAtPath (maskAssetPath, typeof(Texture2D));
                if (texture2D != null) {
                    newMat.SetTexture ("_MaskMap", texture2D);
                }
                newMat.SetFloat ("_AlphaClipThreshold", 0.1f);
            }
            
            
            if (created) {
                AssetDatabase.CreateAsset (newMat, matPath);
            }
            AssetDatabase.SaveAssets ();

            


            assetImporter.AddRemap (new AssetImporter.SourceAssetIdentifier (mat), newMat);   
        }
        
        // //Fixing one off cases
        // if (processingCurrently == "0001 - Bulbasaur") {
        //     assetImporter.AddRemap (new AssetImporter.SourceAssetIdentifier (renderer.sharedMaterials[3]),
        //         AssetDatabase.LoadAssetAtPath<Material> (
        //             "Assets/Resources/Pokemons/Materials/0001 - Bulbasaur/pm0001_00_Iris1.mat"));
        //     assetImporter.AddRemap (new AssetImporter.SourceAssetIdentifier (renderer.sharedMaterials[2]),
        //         AssetDatabase.LoadAssetAtPath<Material> (
        //             "Assets/Resources/Pokemons/Materials/0001 - Bulbasaur/pm0001_00_Eye1.mat"));
        // }
        AssetDatabase.SaveAssets ();
    }
    
    private void OnPostprocessModel (GameObject g)
    {
        return;
        var modelImporter = assetImporter as ModelImporter;
        if (modelImporter == null) return;

        HandleAnimationClips (modelImporter);
        MoveToAppropriateFolder (modelImporter);
        HandleTextures (g, modelImporter);
    }
}
