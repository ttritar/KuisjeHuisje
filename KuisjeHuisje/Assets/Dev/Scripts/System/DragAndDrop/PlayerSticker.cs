using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class PlayerSticker : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    public PlayerData ChildData
    {
        get { return _childData; }
        set { _childData = value; }
    }

    private PlayerData _childData = null;

    [Header("Default Values")]
    [SerializeField] private SkinColor _defaultSkinTone = SkinColor.White;
    [SerializeField] private HairColor _defaultHairColor = HairColor.Black;
    [SerializeField] private Emotion _defaultEmotion = Emotion.Happy;
    [SerializeField] private Top _defaultTop = Top.TShirt;
    [SerializeField] private Bottom _defaultBottom = Bottom.Shorts;
    [SerializeField] private Hair _defaultHair = Hair.Bald;

    [Header("Localization")]
    [SerializeField] private LocalizedStringTable _emotionStringTable;

    [Header("Events")]
    [SerializeField] public UnityEvent<Emotion> OnEmotionChanged = new();
    [SerializeField] public UnityEvent OnHairSelected = new();
    [SerializeField] public UnityEvent OnTopSelected = new();
    [SerializeField] public UnityEvent OnBottomSelected = new();
    [SerializeField] public UnityEvent<PlayerSticker> OnChildCompleted = new();

    private DragAndDropPayload _payload;


    // START
    //--------------------------------------------------
    private void Awake()
    {
        ChildData = _playerPrefab.GetComponent<PlayerData>();

        if (!ChildData)
            return;

        // defaults
        ChildData.Emotion = _defaultEmotion;

        ChildData.Hair = _defaultHair;
        ChildData.HairColor = _defaultHairColor;

        ChildData.SkinColor = _defaultSkinTone;
        ChildData.Bottom = _defaultBottom;

        ChildData.Top = _defaultTop;

        var dests = GetComponentsInChildren<DragAndDropDestination>();
        if (dests.Length > 0)
        {
            foreach (var dest in dests)
            {
                dest.OnPayloadReceived.AddListener(OnPayloadReceived);
            }
        }
    }

    // EVENTS
    //--------------------------------------------------
    private void OnPayloadReceived(GameObject payloadObject, string payloadMessage)
    {
        if (payloadObject.TryGetComponent(out DragAndDropPayload payload))
        {
            switch (payload.PayloadId)
            {
                case "Hair":
                    ApplyHair(payload, payloadMessage);
                    break;
                case "Top":
                    ApplyTop(payload, payloadMessage);
                    break;
                case "Bottom":
                    ApplyBottom(payload, payloadMessage);
                    break;

                default:
                    break;
            }
        }
    }

    private void OnStickerRemoved(DragAndDropPayload payload, string payloadMessage)
    {
        switch (payload.PayloadId)
        {
            case "Hair":
                ClearHair(payload, payloadMessage);
                break;
            case "Top":
                ClearTop(payload, payloadMessage);
                break;
            case "Bottom":
                ClearBottom(payload, payloadMessage);
                break;
            default:
                break;
        }
    }


    // DATA UPDATING
    //--------------------------------------------------
    private void ApplyHair(DragAndDropPayload payload, string msg)
    {
        ChildData.Hair = ChildData.IsStringInEnum<Hair>(msg, out Hair hair) ? hair : Hair.Bald;
        OnHairSelected?.Invoke();
    }
    private void ClearHair(DragAndDropPayload payload, string msg)
    {
        ChildData.Hair = Hair.Bald;
    }

    private void ApplyTop(DragAndDropPayload payload, string msg)
    {
        ChildData.Top = ChildData.IsStringInEnum<Top>(msg, out Top top) ? top : Top.TShirt;
        OnTopSelected?.Invoke();
    }
    private void ClearTop(DragAndDropPayload payload, string msg)
    {
        ChildData.Top = Top.TShirt;
    }

    private void ApplyBottom(DragAndDropPayload payload, string msg)
    {
        ChildData.Bottom = ChildData.IsStringInEnum<Bottom>(msg, out Bottom bottom) ? bottom : Bottom.Shorts;
        OnBottomSelected?.Invoke();
    }
    private void ClearBottom(DragAndDropPayload payload, string msg)
    {
        ChildData.Bottom = Bottom.Shorts;
    }

    // SKIN COLOR
    private void ApplySkin(SkinColor color)
    {
        ChildData.SkinColor = color;
    }
    public void SetSkinColorWhite()
    {
        ApplySkin(SkinColor.White);
    }
    public void SetSkinColorMedium()
    {
        ApplySkin(SkinColor.Medium);
    }
    public void SetSkinColorBrown()
    {
        ApplySkin(SkinColor.Brown);
    }
    public void SetSkinColorDarkBrown()
    {
        ApplySkin(SkinColor.DarkBrown);
    }

    // HAIR COLOR
    private void ApplyHairColor(HairColor color)
    {
        ChildData.HairColor = color;
    }
    public void SetHairColorBlack()
    {
        ApplyHairColor(HairColor.Black);
    }
    public void SetHairColorBrown()
    {
        ApplyHairColor(HairColor.Brown);
    }
    public void SetHairColorBlonde()
    {
        ApplyHairColor(HairColor.Blonde);
    }
    public void SetHairColorRed()
    {
        ApplyHairColor(HairColor.Red);
    }

    // EMOTION 
    private void ApplyEmotion(Emotion emotion)
    {
        ChildData.Emotion = emotion;
        OnEmotionChanged.Invoke(emotion);
    }
    public void PreviousEmotion()
    {
        int count = (int)Emotion.COUNT;
        int newIndex = ((int)_childData.Emotion - 1 + count) % count;
        ApplyEmotion((Emotion)newIndex);
    }
    public void NextEmotion()
    {
        int count = (int)Emotion.COUNT;
        int newIndex = ((int)_childData.Emotion + 1) % count;
        ApplyEmotion((Emotion)newIndex);
    }

    // COMPLETE
    public void CompleteChild()
    {
        GameManager.Instance.SetDefaultWorld(ChildData.Emotion);
        FileLogger.Instance.LogPlayerCustomization(ChildData);

        OnChildCompleted?.Invoke(this);
    }



    // STICKER MANAGEMENT
    //--------------------------------------------------
    public void DeleteCharacter()
    {
        Destroy(this.gameObject);
    }
}
