using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RawMeshRenderer : MonoBehaviour
{
    public Dictionary<Color32, List<Vector3>> vertices;
    
    public void Awake ()
    {
        
    }

    private void OnDrawGizmos ()
    {
        if (vertices == null) {
            return;
        }

        foreach (var kvp in vertices) {
            Gizmos.color = kvp.Key;
            for (int i = 0; i < kvp.Value.Count; i++) {
                Gizmos.DrawSphere(kvp.Value[i], 1f);
            }   
        }
    }
}
