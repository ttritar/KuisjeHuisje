using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.UI;



[Serializable]
struct AnimalPrefab
{
    public Animal animal;
    public GameObject prefab;
}


public class CharacterSticker : MonoBehaviour
{
    [Header("Data")]
    private string _relationship;
    private CharacterData _charData;
    public CharacterData CharData
    {
        get { return _charData; }
        set { _charData = value; }
    }


    [SerializeField] private List<AnimalPrefab> _animalCharacterPrefabs;
    [SerializeField] private LocalizedStringTable _relationshipStringTable;
    [SerializeField] private LocalizedStringTable _emotionStringTable;
    public bool IsRoot { get; private set; } = true;

    [Header("Player Feedback")]
    [SerializeField] private GameObject _vfxCompletedGameObject;
    private Material _vfxCompletedMaterial;
    [SerializeField] private float _vfxCompletedDuration = 2.0f; 
    private const string PROGRESS = "_Progress";

    [Header("UI")] 
    [SerializeField] private Button _confirmButton;
    CounterLockedButton _confirmCounterButton;

    [Header("Events")]
    public UnityEvent OnAnimalSelected = new();
    public UnityEvent OnEmotionSelected = new();
    public UnityEvent OnRelationshipSelected = new();
    public UnityEvent<CharacterSticker> OnCharacterCompleted = new();
    public UnityEvent OnCharacterCancelled = new();
    public UnityEvent OnCharacterDestroyed = new();


    private DragAndDropPayload _payload;

    // START
    //--------------------------------------------------
    private void Awake()
    {
        CharData = gameObject.AddComponent<CharacterData>();
        _payload = GetComponent<DragAndDropPayload>();
        _relationship = _payload.PayloadMessage;
        ApplyRelationship(_payload, _relationship);

        _payload.OnPickUp.AddListener(RemoveIsRoot);

        // destinations
        var dests = GetComponentsInChildren<DragAndDropDestination>();
        if (dests.Length > 0)
        {
            foreach (var dest in dests)
            {
                dest.OnPayloadReceived.AddListener(OnPayloadReceived);
            }
        }

        // confirm button
        _confirmButton.onClick.AddListener(CheckCompletion);
        _confirmCounterButton = _confirmButton.GetComponent<CounterLockedButton>();
    }
    private void OnDestroy()
    {
        _confirmButton.onClick.RemoveListener(CheckCompletion);
        _payload.OnPickUp.RemoveListener(RemoveIsRoot);

        DeleteCharacter();
    }
    private void RemoveIsRoot()
    {
        IsRoot = false;
    }

    // EVENTS
    //--------------------------------------------------
    private void OnPayloadReceived(GameObject payloadObject, string payloadMessage)
    {
        if (payloadObject.TryGetComponent(out DragAndDropPayload payload))
        {
            // disable payload
            payload.enabled = false;
            var component = payload.GetComponent<Collider>();
            if (component != null) component.enabled = false;

            switch (payload.PayloadId)
            {
                case "Emotion":
                    ApplyEmotion(payload, payloadMessage);
                    break;

                case "Relationship":
                    ApplyRelationship(payload, payloadMessage);
                    break;

                case "Animal":
                    if (CharData.IsStringInEnum<Animal>(payloadMessage, out Animal animal))
                    {
                        ApplyAnimal(payload, animal);
                    }
                    break;

                default:
                    break;
            }
        }
    }
    public void OnPayloadDeleted(string payloadId)
    {
        switch (payloadId)
        {
            case "Emotion":
                CharData.Emotion = Emotion.COUNT;
                break;
            case "Relationship":
                CharData.Relationship = "";
                break;
            case "Animal":
                CharData.Animal = Animal.COUNT;
                break;
            default:
                break;
        }
    }

    //--- Invoke
    public void InvokeAnimalSelected()
    {
        OnAnimalSelected.Invoke();
    }



    // DATA UPDATING
    //--------------------------------------------------
    private void ApplyEmotion(DragAndDropPayload payload, string msg)
    {
        CharData.Emotion = CharData.IsStringInEnum<Emotion>(msg, out Emotion emotion) ? emotion : Emotion.COUNT;
        OnEmotionSelected.Invoke();

        //-- Player Feedback
        PlayOnCompletedFeedback();
    }

    private void ApplyRelationship(DragAndDropPayload payload, string msg)
    {
        CharData.Relationship = msg;
        OnRelationshipSelected.Invoke();

        //-- Player Feedback
        PlayOnCompletedFeedback();
    }

    private void ApplyAnimal(DragAndDropPayload payload, Animal animal)
    {
        CharData.Animal = animal;
        OnAnimalSelected.Invoke();

        //-- Player Feedback
        PlayOnCompletedFeedback();
    }

    public void CheckCompletion()
    {
        if (!CharData.IsComplete) return;

        _payload.CanMove = false;

        CharData.DefaultWorld = _charData.Emotion;

        foreach (var animalPrefab in _animalCharacterPrefabs)
        {
            if (animalPrefab.animal == CharData.Animal)
                GameManager.Instance.AddCharacter(animalPrefab.prefab, CharData);
        }

        FileLogger.Instance.LogNPCCreation(CharData);
        _confirmButton.onClick.RemoveListener(CheckCompletion);
    }
    
    // STICKER MANAGEMENT
    //--------------------------------------------------
    public void DeleteCharacter()
    {

        if(CharData.IsComplete)
        {
            OnCharacterDestroyed.Invoke();
            _confirmCounterButton.IncrementCount(-1);
        }
        else
            OnCharacterCancelled.Invoke();
    }


    // PLAYER FEEDBACK
    //--------------------------------------------------
    private void PlayOnCompletedFeedback()
    {
        if (!CharData.IsComplete)
            return;
        OnCharacterCompleted?.Invoke(this);

        // disable dests
        foreach (var dest in GetComponentsInChildren<DragAndDropDestination>())
        {
            dest.enabled = false;
        }

        // wiggle
        var wiggle = gameObject.GetComponent<StickerWiggle>();
        if (wiggle != null) wiggle.enabled = false;

        _confirmCounterButton.IncrementCount();

        // effect   
        StartCoroutine(AnimateCompletedProgress());
    }
    private IEnumerator AnimateCompletedProgress()
    {
        if (_vfxCompletedGameObject == null)
        {
            Debug.LogWarning("[CharacterSticker] _vfxCompletedGameObject is null.");
            yield break;
        }

        _vfxCompletedGameObject.SetActive(true);

        // get renderer
        var rend = _vfxCompletedGameObject.GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning("[CharacterSticker] No Renderer found on _vfxCompletedGameObject.");
            yield break;
        }

        _vfxCompletedMaterial = new Material(rend.material);
        rend.material = _vfxCompletedMaterial;

        // update
        _vfxCompletedMaterial.SetFloat(PROGRESS, 0f);

        float t = 0f;
        while (t < Mathf.Max(0.0001f, _vfxCompletedDuration))
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / _vfxCompletedDuration);

            _vfxCompletedMaterial.SetFloat(PROGRESS, normalized);

            yield return null;
        }

        _vfxCompletedMaterial.SetFloat(PROGRESS, 1f);
    }

}
