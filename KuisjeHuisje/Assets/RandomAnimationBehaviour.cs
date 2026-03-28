using UnityEngine;

public class RandomAnimationBehaviour : StateMachineBehaviour
{
    [SerializeField] private int _min = 0;
    [SerializeField] private int _max = 3;
    [SerializeField] private string _parameterName = "randomIdleIndex";

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        int randomValue = Random.Range(_min, _max + 1);
        animator.SetInteger(_parameterName, randomValue);
    }
}
