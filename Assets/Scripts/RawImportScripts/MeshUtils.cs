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
    
    public static List<Vector4> PicaToUnityTangents (PICAVertex[] picaVertices)
    {
        var retVal = new List<Vector4> ();
        foreach (var picaVertex in picaVertices) {
            retVal.Add (new Vector4(picaVertex.Tangent.X, picaVertex.Tangent.Y, picaVertex.Tangent.Z, picaVertex.Tangent.W));
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
