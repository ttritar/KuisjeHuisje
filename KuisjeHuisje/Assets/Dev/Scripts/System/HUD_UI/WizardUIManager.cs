using UnityEngine;

public class WizardUIManager : MonoBehaviour
{
    [SerializeField] private WizardInteractionManager _wizardInteractionManager;

    // HELPER
    //--------------------------------------------------
    public void StopTalking()
    {
        _wizardInteractionManager.EndInteraction();
    }
    public void StartCleaningTutorial()
    {
        StartTutorial(TutorialManager.TutorialType.Cleaning);
    }
    public void StartAssigningTutorial()
    {
        StartTutorial(TutorialManager.TutorialType.Assigning);
    }
    public void StartThrowingTutorial()
    {
        StartTutorial(TutorialManager.TutorialType.Potion);
    }

    private void StartTutorial(TutorialManager.TutorialType type)
    {
        gameObject.SetActive(false);
        TutorialManager.Instance.StartTutorial(type);
    }
}
