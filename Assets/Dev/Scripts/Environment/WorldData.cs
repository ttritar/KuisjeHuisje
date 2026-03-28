using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

public class WorldData : MonoBehaviour
{
    [SerializeField] private List<SplineContainer> _characterSplines;
    public SplineContainer RandomCharacterSpline => _characterSplines[Random.Range(0, _characterSplines.Count)];

    [SerializeField] private List<GameObject> _effects;
    public List<GameObject> Effects => _effects;

    [SerializeField] private List<CharacterData> _characters;
    public List<CharacterData> Characters => _characters;

    [SerializeField] private List<HouseBehaviour> _houses;
    public List<HouseBehaviour> Houses => _houses;
    [SerializeField] private AudioSource _music;
    public AudioSource Music => _music;

    // AWAKE
    //--------------------------------------------------
    public void Initialize()
    {
        _characters.AddRange(GetComponentsInChildren<CharacterData>());
        _houses.AddRange(GetComponentsInChildren<HouseBehaviour>());
    }

    // HELPER
    //--------------------------------------------------
    public void AddCharacter(CharacterData newChar)
    {
        _characters.Add(newChar);
    }
    public void RemoveCharacter(CharacterData oldChar)
    {
        _characters.Remove(oldChar);
    }
}
