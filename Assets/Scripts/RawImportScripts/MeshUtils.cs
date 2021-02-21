using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SPICA.PICA.Converters;
using UnityEngine;

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

    public static List<BoneWeight> PicaToUnityBoneWeights (PICAVertex[] picaVertices)
    {
        var retVal = new List<BoneWeight> ();
        int maxIndex = 0;
        foreach (var picaVertex in picaVertices) {
            var singleBoneWeight = new BoneWeight ();
            if (picaVertex.Weights.w0 > 0) {
                singleBoneWeight.weight0 = picaVertex.Weights.w0;
                singleBoneWeight.boneIndex0 = picaVertex.Indices.b0;
            }
            if (picaVertex.Weights.w1 > 0) {
                singleBoneWeight.weight1 = picaVertex.Weights.w1;
                singleBoneWeight.boneIndex1 = picaVertex.Indices.b1;
            }
            if (picaVertex.Weights.w2 > 0) {
                singleBoneWeight.weight2 = picaVertex.Weights.w2;
                singleBoneWeight.boneIndex2 = picaVertex.Indices.b2;
            }
            if (picaVertex.Weights.w3 > 0) {
                singleBoneWeight.weight3 = picaVertex.Weights.w3;
                singleBoneWeight.boneIndex3 = picaVertex.Indices.b3;
            }
            retVal.Add (singleBoneWeight);
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
        
    public static Vector4 CastNumericsVector4 (System.Numerics.Vector4 newValues)
    {
        var vector4 = new Vector4 {x = newValues.X, y = newValues.Y, z = newValues.Z, w = newValues.W};
        return vector4;
    }
        
}

public static class SkeletonUtils
{
    public class SkeletonNode
    {
        public string id;
        public string name;
        public string sid;
        public MeshUtilsSkeletonNodeType type = MeshUtilsSkeletonNodeType.NODE;
        
        public Vector3   Translation;
        public Vector3 Rotation;
        public Vector3   Scale;

        public List<SkeletonNode> Nodes;
    }

    public enum MeshUtilsSkeletonNodeType
    {
        NODE,
        JOINT
    }
}