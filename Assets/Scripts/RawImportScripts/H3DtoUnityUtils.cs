using System;
using System.Collections.Generic;
using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Formats.CtrH3D.Model.Material;
using SPICA.PICA.Converters;
using UnityEngine;

namespace ExtensionMethods
{
    public static class H3DMaterialExtensions
    {
        public static int GetTextureIndex (this H3DMaterial h3DMaterial, string name)
        {
            if (h3DMaterial.Texture0Name == name) {
                return 0;
            }
            if (h3DMaterial.Texture1Name == name) {
                return 1;
            }
            return 2;
        }
        
        public static List<string> TextureNames (this H3DMaterial h3DMaterial) => new List<string> {
            h3DMaterial.Texture0Name,h3DMaterial.Texture1Name,h3DMaterial.Texture2Name,
        };
        
    }
}

public class TextureUtils
{
    
}

public class MeshUtils
{
    public static List<Vector3> PicaToUnityVertex (PICAVertex[] picaVertices)
    {
        var retVal = new List<Vector3> ();
        foreach (var picaVertex in picaVertices) {
            retVal.Add (new Vector3(picaVertex.Position.X, picaVertex.Position.Y, picaVertex.Position.Z));
        }
        return retVal;
    }

    public static List<Vector4> PicaToUnityTangents (PICAVertex[] picaVertices)
    {
        var retVal = new List<Vector4> ();
        foreach (var picaVertex in picaVertices) {
            retVal.Add (new Vector4(picaVertex.Tangent.X, picaVertex.Tangent.Y, picaVertex.Tangent.Z, picaVertex.Tangent.W));
        }
        return retVal;
    }
    
    public static List<Vector2> PicaToUnityUV (PICAVertex[] picaVertices)
    {
        var retVal = new List<Vector2> ();
        foreach (var picaVertex in picaVertices) {
            retVal.Add (new Vector2(picaVertex.TexCoord0.X, picaVertex.TexCoord0.Y));
        }
        return retVal;
    }
    
    public static List<Vector3> PicaToUnityNormals (PICAVertex[] picaVertices)
    {
        var retVal = new List<Vector3> ();
        foreach (var picaVertex in picaVertices) {
            retVal.Add (new Vector3(picaVertex.Normal.X, picaVertex.Normal.Y, picaVertex.Normal.Z));
        }
        return retVal;
    }

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