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
        foreach (var picaVertex in picaVertices) {
            retVal.Add (new BoneWeight {
                boneIndex0 = picaVertex.Indices.b0,
                boneIndex1 = picaVertex.Indices.b1,
                boneIndex2 = picaVertex.Indices.b2,
                boneIndex3 = picaVertex.Indices.b3,
                weight0 = picaVertex.Weights.w0,
                weight1 = picaVertex.Weights.w1,
                weight2 = picaVertex.Weights.w2,
                weight3 = picaVertex.Weights.w3,
            });
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

    public static List<int> IndicesToTriangles (ushort[] indices) => indices.Select (index => (int) index).ToList ();
}
