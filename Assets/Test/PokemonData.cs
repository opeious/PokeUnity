using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public class PokemonData : MonoBehaviour
{
    [MenuItem ("Pkmn/Test")]
    private static void ImportPokemonAction ()
    {
        var x = JsonConvert.DeserializeObject (File.ReadAllText ("Assets/Test/test.json"));
        var prop = (x as JObject).Properties ();

        HashSet<string> propes = new HashSet<string> ();
        var dir = "Assets/Test/PokeDex";
        if(Directory.Exists (dir))
        {
            Directory.Delete (dir,true);
        }

        Directory.CreateDirectory (dir);



        int count = 0;
        foreach (var z in prop) {
            // Debug.LogError (z.ToString());
            // var a = JsonConvert.DeserializeObject<DexEntry> (z.ToString ());
            // var de = new DexEntry ();
            // de.Name = (string) z.Value["name"];
            // de.Num = (string) z.Value["num"];
            // Debug.LogError ((Dictionary<string,string>) z.Value);
            var a = (JObject) z.Value;
            // Debug.LogError (a["name"]);
            var de = ScriptableObject.CreateInstance<DexEntry> ();
            if (de.parse (a)) {
                Debug.LogError (z.ToString());
                AssetDatabase.CreateAsset(de, dir + "/" + count + ".asset");    
            }
         
            //TODO:
            // AssetDatabase.CreateAsset(de, dir + "/" + count + ".asset");
            
            if (count == 10) {
                // break;
            }

            count++;
            if (a["name"].ToString() == "MissingNo.") {
                break;
            }
        }
        AssetDatabase.SaveAssets ();
    }

    public class PokeDex
    {
        public string EntryName;
        public DexEntry EntryData;
    }

    [Serializable]
    public class GenderRatio
    {
        public float Male;
        public float Female;
    }

    public enum PokemonTypes
    {
        Normal,
        Fire,
        Water,
        Grass,
        Electric,
        Ice,
        Fighting,
        Poison,
        Ground,
        Flying,
        Psychic,
        Bug,
        Rock,
        Ghost,
        Dark,
        Dragon,
        Steel,
        Fairy,
        Bird //Missing No. uses this
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
