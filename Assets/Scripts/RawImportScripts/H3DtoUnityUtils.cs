﻿using System;
using System.Collections.Generic;
using System.Linq;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Formats.CtrH3D.Model.Material;
using SPICA.PICA.Commands;
using SPICA.PICA.Converters;
using UnityEngine;

namespace ExtensionMethods
{
    public static class H3DMaterialExtensions
    {
        public static int GetTextureIndex (this H3DMaterial h3DMaterial, string name) => h3DMaterial.Texture0Name == name ? 0 : h3DMaterial.Texture1Name == name ? 1 : 2;

        public static IEnumerable<string> TextureNames (this H3DMaterial h3DMaterial) => new List<string> {
            h3DMaterial.Texture0Name,h3DMaterial.Texture1Name,h3DMaterial.Texture2Name,
        };
    }
}

public static class TextureUtils
{
    public static Texture2D FlipTexture(Texture2D original){
        var flipped = new Texture2D(original.width,original.height);
        var xN = original.width;
        var yN = original.height;

        for(var i=0;i<xN;i++){
            for(var j=0;j<yN;j++){
                flipped.SetPixel(xN-i-1, j, original.GetPixel(i,j));
            }
        }
        flipped.Apply();
        return flipped;
    }
    
    public class H3DTextureRepresentation
    {
        public H3DTextureCoord TextureCoord;
        public H3DTextureMapper TextureMapper;
    }

    public static TextureWrapMode PicaToUnityTextureWrapMode (PICATextureWrap picaTextureWrap)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (picaTextureWrap) {
            case PICATextureWrap.Repeat:
                return TextureWrapMode.Repeat;
            case PICATextureWrap.Mirror:
                return TextureWrapMode.Mirror;
            case PICATextureWrap.ClampToEdge:
                return TextureWrapMode.Clamp;
            default:
                return TextureWrapMode.Mirror;
        }
    }
}

public static class MeshUtils
{
    public static IEnumerable<Vector3> PicaToUnityVertex (IEnumerable<PICAVertex> picaVertices) => picaVertices.Select (picaVertex => new Vector3 (picaVertex.Position.X, picaVertex.Position.Y, picaVertex.Position.Z)).ToList ();

    public static IEnumerable<Vector4> PicaToUnityTangents (IEnumerable<PICAVertex> picaVertices) => picaVertices.Select (picaVertex => new Vector4 (picaVertex.Tangent.X, picaVertex.Tangent.Y, picaVertex.Tangent.Z, picaVertex.Tangent.W)).ToList ();

    public static IEnumerable<Vector2> PicaToUnityUV (IEnumerable<PICAVertex> picaVertices) => picaVertices.Select (picaVertex => new Vector2 (picaVertex.TexCoord0.X, picaVertex.TexCoord0.Y)).ToList ();

    public static IEnumerable<Vector3> PicaToUnityNormals (IEnumerable<PICAVertex> picaVertices) => picaVertices.Select (picaVertex => new Vector3 (picaVertex.Normal.X, picaVertex.Normal.Y, picaVertex.Normal.Z)).ToList ();
}

public static class VectorExtensions
{
    public static Vector3 CastNumericsVector3 (System.Numerics.Vector3 newValues)
    {
        var vector3 = new Vector3 {x = newValues.X, y = newValues.Y, z = newValues.Z};
        return vector3;
    }
      
    public static Vector3 CastNumericsVector3 (System.Numerics.Vector4 newValues)
    {
        var vector3 = new Vector3 {x = newValues.X, y = newValues.Y, z = newValues.Z};
        return vector3;
    }
        
    public static Vector4 CastNumericsVector4 (System.Numerics.Vector4 newValues)
    {
        var vector4 = new Vector4 {x = newValues.X, y = newValues.Y, z = newValues.Z, w = newValues.W};
        return vector4;
    }
    
    public static Vector3 GetAxisFromRotation(Vector4 vector4)
    {
        return new UnityEngine.Vector3(vector4.x * -1,vector4.y * -1,vector4.z * -1);
    }

    public static float GetScalarFromRotation (Vector4 vector4) => vector4.w;
}

public static class SkeletonUtils
{
    public static SkeletonNode GenerateSkeletonForModel (H3DModel mdl)
    {
        if ((mdl.Skeleton?.Count ?? 0) <= 0) return null;
        var rootNode = new SkeletonNode ();
        var childBones = new Queue<Tuple<H3DBone, SkeletonNode>>();

        childBones.Enqueue(Tuple.Create(mdl.Skeleton[0], rootNode));
            
        while (childBones.Count > 0)
        {
            var (item1, item2) = childBones.Dequeue();

            var bone = item1;

            item2.Name = bone.Name;
            item2.SetBoneEuler (
                VectorExtensions.CastNumericsVector3 (bone.Translation),
                   VectorExtensions.CastNumericsVector3 (bone.Rotation),
                    VectorExtensions.CastNumericsVector3 (bone.Scale)
                );

            foreach (var b in mdl.Skeleton)
            {
                if (b.ParentIndex == -1) continue;

                var parentBone = mdl.Skeleton[b.ParentIndex];

                if (parentBone != bone) continue;
                
                var node = new SkeletonNode();

                childBones.Enqueue(Tuple.Create(b, node));

                if (item2.Nodes == null) item2.Nodes = new List<SkeletonNode>();

                item2.Nodes.Add(node);
            }
        }
        return rootNode;
    }

    public class SkeletonNode
    {
        public string Name;
        
        public Vector3   Translation;
        public Vector4[] Rotation;
        public Vector3   Scale;

        public List<SkeletonNode> Nodes;
        
        public void SetBoneEuler(Vector3 t, Vector3 r, Vector3 s)
        {
            Rotation = new Vector4[3];
            Translation = t;
            Rotation[0] = new Vector4(0, 0, 1, RadToDeg(r.z));
            Rotation[1] = new Vector4(0, 1, 0, RadToDeg(r.y));
            Rotation[2] = new Vector4(1, 0, 0, RadToDeg(r.x));
            Scale = s;
        }
    }

    private const float RadToDegConstant = (float)((1 / Math.PI) * 180);
    private static float RadToDeg(float radians)
    {
        return radians * RadToDegConstant;
    }
}