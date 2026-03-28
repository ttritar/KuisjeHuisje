using UnityEngine;

public class CharacterStickerHighlight : MonoBehaviour
{
    [SerializeField] private GameObject _animalHighlight;
    [SerializeField] private GameObject _emotionHighlight;
    [SerializeField] private GameObject _relationshipHighlight;

    public void ShowAnimalHighlight()
    {
        _animalHighlight.SetActive(true);
        _emotionHighlight.SetActive(false);
        _relationshipHighlight.SetActive(false);
    }
    public void ShowEmotionHighlight()
    {
        _animalHighlight.SetActive(false);
        _emotionHighlight.SetActive(true);
        _relationshipHighlight.SetActive(false);
    }
    public void ShowRelationshipHighlight()
    {
        _animalHighlight.SetActive(false);
        _emotionHighlight.SetActive(false);
        _relationshipHighlight.SetActive(true);
    }
}
