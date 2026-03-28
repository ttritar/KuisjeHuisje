using UnityEngine;

[System.Serializable]
public class CharacterData : MonoBehaviour
{
    // SELECTION DATA
    public string Relationship = null;

    public Animal Animal = Animal.COUNT;

    public Emotion Emotion = Emotion.COUNT;

    // AFTER SELECTION
    public bool IsComplete => Emotion != Emotion.COUNT && Animal != Animal.COUNT && !string.IsNullOrEmpty(Relationship);


    private Emotion _defaultWorld = Emotion.COUNT;
    public Emotion DefaultWorld
    {
        get { return _defaultWorld; }
        set { _defaultWorld = value; }
    }



    // FUNCTIONALITY
    //--------------------------------------------------
    public bool IsStringInEnum<T>(string str, out T enumValue) where T : struct, System.Enum
    {
        if (System.Enum.TryParse<T>(str, out enumValue))
        {
            return true;
        }
        enumValue = default;
        return false;
    }
}

public enum Animal
{
    Cat,
    Dog,
    Frog,
    Sheep,
    Bunny,
    Bear,
    Lion,

    COUNT
}