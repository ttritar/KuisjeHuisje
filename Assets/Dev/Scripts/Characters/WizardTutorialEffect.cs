using UnityEngine;
using UnityEngine.Events;

public class WizardTutorialEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem _effect;
    [SerializeField] private TutorialManager _tutorialManager;

    [SerializeField] public UnityEvent OnEffectPlayed;
    public void PlayEffect()
    {
        if (_effect != null)
        {
            if(_tutorialManager.IsTutorialRunning)
            {
                _effect.Play();
                OnEffectPlayed?.Invoke();
            }
        }
    }
}
