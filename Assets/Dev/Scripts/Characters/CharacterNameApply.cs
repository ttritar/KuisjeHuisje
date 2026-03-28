using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class CharacterNameApply : MonoBehaviour
{
    [SerializeField] private LocalizeStringEvent _relationshipLocalizeEvent;
    [SerializeField] private LocalizedStringTable _relationshipStringTable;
    public TextMeshPro Tmp => _tmp;
    [SerializeField] private TextMeshPro _tmp;



    // LOCALIZATION
    //--------------------------------------------------
    public void LocalizeString(string key)
    {
        if (_relationshipStringTable != null && _relationshipStringTable.GetTable().GetEntry(key) != null)
        {
            var localizedEmotion = new LocalizedString(_relationshipStringTable.TableReference, key);
            _relationshipLocalizeEvent.enabled = true;
            _relationshipLocalizeEvent.StringReference = localizedEmotion;
        }
        else
        {
            _relationshipLocalizeEvent.enabled = false;
            if (_tmp != null)
                _tmp.text = key;
        }
    }

}
