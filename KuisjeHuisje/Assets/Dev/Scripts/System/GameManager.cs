using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GameManager : ISingleton<GameManager>
{
    [Header("Player")]
    [SerializeField] private GameObject _playerDataPrefab;
    private Emotion _defaultPlayerWorld = Emotion.Happy;

    [Header("NPCs")]
    private List<KeyValuePair<GameObject, CharacterData>> _characters = new List<KeyValuePair<GameObject, CharacterData>>();

    void OnEnable()
    {
        SceneLoader.Instance.OnMainSceneLoaded.AddListener(OnSceneLoaded);
    }


    // PLAYER MANAGEMENT
    //--------------------------------------------------
    public void SetDefaultWorld(Emotion emotion)
    {
        _defaultPlayerWorld = emotion;
    }
    private void ApplyDefaultWorld()
    {
        WorldSwitchManager.Instance.ForceSetWorld(_defaultPlayerWorld);
    }

    // CHARACTER MANAGEMENT
    //--------------------------------------------------
    public void AddCharacter(GameObject npc, CharacterData data)
    {
        _characters.Add(new KeyValuePair<GameObject, CharacterData>(npc, data));
    }
    private void ClearCharacters()
    {
        foreach (var character in _characters)
        {
            Destroy(character.Key);
        }
        _characters.Clear();
    }
    private void SpawnAllCharacters()
    {
        // Get Player
        var player = FindObjectsByType<PlayerData>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)[0];

        // Top, Bottom, Hairstyle
        if (player)
        {
            var oldDat = _playerDataPrefab.GetComponent<PlayerData>();
            player.Emotion = oldDat.Emotion;

            player.Hair = oldDat.Hair;
            player.HairColor = oldDat.HairColor;

            player.SkinColor = oldDat.SkinColor;
            
            player.Top = oldDat.Top;
            player.Bottom = oldDat.Bottom;
            player.ApplyNewData();
        }

        // NPC
        var existingNPC = FindObjectsByType<CharacterData>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
        foreach (var npc in existingNPC)
        {
            var comp = npc.GetComponent<CharacterEmotionsSwap>();
            if (!comp)
                continue;
            comp.ApplyEmotion(npc.Emotion); 

            // text
            var nameApply = npc.gameObject.GetComponent<CharacterNameApply>();
            if (nameApply)
            {
                nameApply.LocalizeString(npc.Relationship);
                nameApply.Tmp.transform.localScale *= 0.7f;
            }
        }

        foreach (var character in _characters)
        {
            var obj = Instantiate(character.Key, this.transform);

            // data
            var dataComp = obj.GetComponent<CharacterData>();
            if (dataComp == null)
                continue;
            dataComp.Animal = character.Value.Animal;
            dataComp.DefaultWorld = character.Value.DefaultWorld;
            dataComp.Emotion = character.Value.Emotion;
            dataComp.Relationship = character.Value.Relationship;

            // text
            var nameApply = obj.GetComponent<CharacterNameApply>();
            if (nameApply)
                nameApply.LocalizeString(dataComp.Relationship);


            WorldSwitchManager.Instance.SendCharacterToWorld(obj, dataComp.DefaultWorld, true, true); 
            character.Key.SetActive(true);

            var comp = character.Key.GetComponent<CharacterEmotionsSwap>();
            if (!comp)
                continue;
            comp.ApplyEmotion(dataComp.Emotion);
        }

        _characters.Clear();
    }




    // SCENE LOADING
    //--------------------------------------------------
    public void QuitGame()
    {
        Debug.Log("QUIT REQUEST RECEIVED");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    private void OnSceneLoaded()
    {
        SpawnAllCharacters();
        //ApplyDefaultWorld();
    }
}

public enum Emotion
{
    Happy,
    Sad,
    Angry,
    Scared,

    COUNT
}