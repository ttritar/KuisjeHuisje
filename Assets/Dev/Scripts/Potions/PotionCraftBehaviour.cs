using UnityEngine;
using UnityEngine.UI;

public class PotionCraftBehaviour : MonoBehaviour
{
    [SerializeField] private PotionThrower _thrower;

    // EVENTS
    //--------------------------------------------------
    public void OnEmptyBottlesChanges(int count)
    {
        if (count > 0)
        {
            UpdateButtonInteract(true);
            return;
        }
        UpdateButtonInteract(false);
    }

    private void UpdateButtonInteract(bool opt)
    {
        var buttons = GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
            button.interactable = opt;
        }
    }
    // CRAFTING
    //--------------------------------------------------
    public void CraftHappyPotion()
    {
        //_thrower.AddHappyPotion();
    }
    public void CraftSadPotion()
    {
        //_thrower.AddSadPotion();
    }
    public void CraftAngryPotion()
    {
        //_thrower.AddAngryPotion();
    }
    public void CraftScaredPotion()
    {
        //_thrower.AddScaredPotion();
    }
}
