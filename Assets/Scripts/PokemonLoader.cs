using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonLoader : MonoBehaviour
{
    [SerializeField] public List<GameObject> pokemonModels;
    [SerializeField] public bool rotate;
    
    private Transform pokemonHolder;
    private GameObject currentPokemon;
    
    private int index = 0;

    private void Start ()
    {
        OnChangePokemon ();
    }

    private void Update ()
    {
        if (rotate) {
            currentPokemon.transform.RotateAround (Vector3.zero, Vector3.up, 0.2f);   
        }
    }

    public void Minus ()
    {
        index--;
        if (index < 0) {
            index = pokemonModels.Count - 1;
        }
        OnChangePokemon ();
    }

    public void Plus ()
    {
        index++;
        if (index > pokemonModels.Count - 1) {
            index = 0;
        }
        OnChangePokemon ();
    }
    

    public void OnChangePokemon ()
    {
        if (currentPokemon != null) {
            Destroy (currentPokemon);
        }
        currentPokemon = Instantiate (pokemonModels[index], pokemonHolder);
        currentPokemon.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
        currentPokemon.transform.SetPositionAndRotation (Vector3.zero, Quaternion.Euler (0, 0, 0));
    }
}
