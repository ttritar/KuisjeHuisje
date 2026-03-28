using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PhysicsBody))]
[RequireComponent(typeof(Collider))]
public class PotionBehaviour : MonoBehaviour
{
	[Header("Potion Data")]
	[SerializeField] private Emotion _type;
	[SerializeField] private bool _npcInteraction = false;
	[SerializeField] private Color _color = Color.white;
	public Emotion Type => _type;
	public CharacterAIManager TargetAI { get; set; }
	public event EventHandler<PotionVFXArgs> OnPotionOnPlayerUsed;
	public event EventHandler<PotionVFXArgs> OnPotionUsed;
	public event EventHandler<PotionVFXArgs> OnPotionNotUsed;

	private bool _isProcessing = false;

	[Header("SFX")]
    [SerializeField] private RandomEffect _transitionSFX;

    [Header("Events")]
    [SerializeField] public UnityEvent OnPotionHit = new();
	[SerializeField] public UnityEvent OnPotionMiss = new();

    // START
    //--------------------------------------------------
    private void Start()
	{
		if (TargetAI && !TargetAI.IsFollowing)
			TargetAI.FreezeAI();
    }
    // FUNCTIONALITY
    //--------------------------------------------------
    private void OnCollisionEnter(Collision other)
	{
		if (_isProcessing) return;

		_isProcessing = true;

		Vector3 hitPoint = other.contacts[0].point;
		GameObject hitObject = other.gameObject;


		if (hitObject.CompareTag("Player"))
		{
			OnPotionOnPlayerUsed?.Invoke(this, new PotionVFXArgs(_color, hitPoint));
			StartCoroutine(PotionDelay(0.5f, hitObject));
            OnPotionHit?.Invoke();
            PlayTransitionSFX();
        }
		else if (_npcInteraction && hitObject.CompareTag("NPC"))
		{
			OnPotionUsed?.Invoke(this, new PotionVFXArgs(_color, hitPoint));
			WorldSwitchManager.Instance.SendCharacterToWorld(hitObject, _type, true);
			Destroy(gameObject);
            OnPotionHit?.Invoke();

            // anim

        }
        else
		{
			OnPotionUsed?.Invoke(this, new PotionVFXArgs(_color, hitPoint));
			StartCoroutine(PotionDelay(0.5f, hitObject));
            OnPotionMiss?.Invoke();
        }

    }
	// DELAYED EFFECT
	//--------------------------------------------------
	private IEnumerator PotionDelay(float delay, GameObject hitObject)
	{
		Destroy(GetComponentInChildren<MeshRenderer>());
		yield return new WaitForSeconds(delay);

		if (hitObject.CompareTag("Player"))
			WorldSwitchManager.Instance.SendPlayerToWorld(hitObject, _type);

		Destroy(gameObject);
	}

	// ON DESTROY
	//--------------------------------------------------
	private void OnDestroy()
	{
		OnPotionNotUsed?.Invoke(this, new PotionVFXArgs(_color, Vector3.zero));
	}


    // PLAYER FEEDBACK
    //--------------------------------------------------
    private void PlayTransitionSFX()
    {
        if (_transitionSFX != null)
        {
			_transitionSFX.transform.SetParent(null);
            _transitionSFX.Play();
            Destroy(_transitionSFX.gameObject, 10f);
        }
	}
}


public class PotionVFXArgs : EventArgs
{
	public Color PotionColor { get; private set; }
	public Vector3 HitPosition { get; private set; }
	public PotionVFXArgs(Color color, Vector3 position)
	{
		PotionColor = color;
		HitPosition = position;
	}
}

