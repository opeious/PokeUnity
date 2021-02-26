using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PokemonTextureImporter : AssetPostprocessor
{
    private void OnPostprocessTexture (Texture2D texture)
    {
        return;
        var textureImport = assetImporter as TextureImporter;
        if (textureImport != null) {
            textureImport.maxTextureSize = 512;
        }
    }
}
