using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

    
    [Serializable]
    public class DexEntry : ScriptableObject
    {
        
        public bool parse (JObject jObject)
        {
            Number = Int32.Parse (jObject["num"].ToString());
            Name = jObject["name"].ToString ();

            Types = new List<PokemonData.PokemonTypes> ();
            var failure = false;
            
            var jTypes = jObject["types"] as JArray;
            foreach (var jType in jTypes) {
                if (!Enum.TryParse (jType.ToString (), out PokemonData.PokemonTypes pokemonType)) {
                    Debug.LogError (Name + "failed");
                    failure = true;
                } else {
                    Types.Add (pokemonType);   
                }
            }

            var jRatio = jObject["genderRatio"] as JObject;
            genderRatio = new PokemonData.GenderRatio ();
            if (jRatio == null) {
                genderRatio.Male = 0.5f;
                genderRatio.Female = 0.5f;
            } else {
                float.TryParse (jRatio["M"].ToString(), out var maleRatio);
                genderRatio.Male = maleRatio;
                float.TryParse (jRatio["F"].ToString(), out var femaleRatio);
                genderRatio.Female = femaleRatio;
                if (genderRatio.Male == 0) {
                    
                }
            }
            // genderRatio.Male = 

            return failure;
        }
        
        public int Number;
        public string Name;
        public List<PokemonData.PokemonTypes> Types;
        public PokemonData.GenderRatio genderRatio;
        public string baseStats;
        public string abilities;
        public string heighttm;
        public string weightkg;
        public string color;
        public string evos;
        public string eggGroups;
        public string prevo;
        public string evoLevel;
        public string otherFormes;
        public string formeOrder;
        public string canGigantamax;
        public string baseSpecies;
        public string forme;
        public string requiredItem;
        public string changesForm;
        public string evoType;
        public string gender;
        public string gen;
        public string evoitem;
        public string evoCondition;
        public string canHatch;
        public string evoMove;
        public string baseForme;
        public string cosmeticFormes;
        public string maxHp;
        public string requiredAbility;
        public string battleOnly;
        public string requiredMove;
        public string requiredItems;
        public string cannotDynamax;

    }

