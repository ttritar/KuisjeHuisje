using UnityEngine;

public class BottleInteractBehaviour : MonoBehaviour, IInteractable
{
    public SpawnerBehaviour Spawner { get; set; }

    // INTERACTION
    //--------------------------------------------------
    public void Interact(GameObject interactor)
    {
        var potionThrower = interactor.GetComponent<PotionThrower>();
        if (!potionThrower)
            return;

        // potionThrower.AddEmptyBottle();
        Spawner?.ClearSpawner();
    }
    public bool CanInteract(GameObject interactor)
    {
        return true;
    }
}
