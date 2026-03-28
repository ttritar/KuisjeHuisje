using System;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PotionThrower : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _hitMask;
    [SerializeField][Range(0, 90)] private float _throwAngle;
    [SerializeField] private float _dstToCamZ = 0.5f;
    [SerializeField] private float _dstToCamY = -0.5f;

    [Header("Aim Assist")]
    [SerializeField] private bool _throwOnSelect = true;
    [SerializeField] private float _assistRadius = 5f;
    [SerializeField] private float _maxAssistAngle = 45f;
    [SerializeField] private LayerMask _npcMask;
	[SerializeField] private float _offsetTargetY;


	[Header("Visualization")]
    [SerializeField] private TrajectoryVisualizer _visualizer;
    private CharacterAIManager _targetCharacter;
    [SerializeField] private GameObject _potionSplashVFX;

    // Potions
    private GameObject _selectedPotion;
    private PhysicsBody _selectedPotionPhysics;
    private PotionBehaviour _selectedPotionBehaviour;
    private GameObject _spawnedPotion;
    private CharacterAIManager _targetAI;
    private GameObject _spawnedVFX;
    public bool CanThrowPotion
    {
        get
        {
            if (WorldSwitchManager.Instance.IsTransitioning)
                return false;
            return _spawnedPotion == null;
        }
    }
    [Header("Events")]
    public UnityEvent OnPotionSelected = new();
    public UnityEvent OnPotionDeselected = new();
    public UnityEvent OnPotionThrown = new();

    private bool _validTarget = false;
    private Vector3 _targetPosition = Vector3.zero;
    private Vector3 _spawnPosition = Vector3.zero;

    // MECHANIC
    //--------------------------------------------------
    private void ThrowPotion()
    {
        OnPotionThrown?.Invoke();

        _spawnedPotion = Instantiate(_selectedPotion, _visualizer.transform.position, _camera.transform.rotation);
        var potion = _spawnedPotion.GetComponent<PotionBehaviour>();
        potion.TargetAI = _targetAI;
		potion.OnPotionOnPlayerUsed += HandlePotionOnPlayerUsed;
        potion.OnPotionUsed += HandlePotionUsed;
        potion.OnPotionNotUsed += HandlePotionNotUsed;
		var pb = _spawnedPotion.GetComponent<PhysicsBody>();

        var vel = _selectedPotionPhysics.GetVelocityFromAngle(_spawnedPotion.transform.position, _targetPosition, _throwAngle);
        pb.AddImpulse(vel);

        DeselectPotion();
    }
	private void HandlePotionOnPlayerUsed(object sender, PotionVFXArgs e)
	{

		_spawnedVFX = Instantiate(
			_potionSplashVFX,
			_potionSplashVFX.transform.position,
			_potionSplashVFX.transform.rotation
		);

		ParticleSystem ps = _spawnedVFX.GetComponentInChildren<ParticleSystem>();
		if (ps != null)
		{
			var main = ps.main;
			main.startColor = e.PotionColor;   
			ps.Play();
		}
	}
    private void HandlePotionNotUsed(object sender, PotionVFXArgs e)
    {
        Destroy(_spawnedVFX);
	}

	private void HandlePotionUsed(object sender, PotionVFXArgs e)
	{

		_spawnedVFX = Instantiate(
			_potionSplashVFX,
			e.HitPosition,
			_potionSplashVFX.transform.rotation
		);

		ParticleSystem ps = _spawnedVFX.GetComponentInChildren<ParticleSystem>();
		if (ps != null)
		{
			var main = ps.main;
			main.startColor = e.PotionColor;
			ps.Play();
		}
	}
	// HELPERS
	//--------------------------------------------------
	public void SelectPotion(GameObject pot)
    {
        if (!CanThrowPotion)
            return;

        _selectedPotionBehaviour = pot.GetComponent<PotionBehaviour>();

        if (_selectedPotionBehaviour == null) return;

        GetComponent<PlayerInput>().SwitchCurrentActionMap("PotionThrowing");
        _selectedPotion = pot;
        _selectedPotionPhysics = pot.GetComponent<PhysicsBody>();
        OnPotionSelected.Invoke();

        if (!_throwOnSelect) return;
        _targetPosition = gameObject.transform.position + gameObject.transform.up * _offsetTargetY;
        ThrowPotion();
    }
    public void DeselectPotion()
    {
        _targetAI = null;
        _selectedPotion = null;
        _selectedPotionPhysics = null;
        _selectedPotionBehaviour = null;
        _visualizer.gameObject.SetActive(false);
        GetComponent<PlayerInput>().SwitchCurrentActionMap("MainInput");
        OnPotionDeselected.Invoke();
    }

    // START
    //--------------------------------------------------
    private void Awake()
    {
        _visualizer.gameObject.SetActive(false);
    }

    // LOOP
    //--------------------------------------------------
    private void Update()
    {
        _spawnPosition = _camera.transform.position + _camera.transform.forward * _dstToCamZ
                                                    + _camera.transform.up * _dstToCamY;
        _visualizer.transform.position = _spawnPosition;

        if (!_validTarget || _throwOnSelect) return;
        Vector3 snapped = FindClosestTarget(_spawnPosition, _targetPosition);
        _targetPosition = snapped;
        VisualizeTrajectory();
    }

    // VISUALIZATION
    //--------------------------------------------------
    private void VisualizeTrajectory()
    {
        _visualizer.gameObject.SetActive(true);

        Vector3 vel = _selectedPotionPhysics.GetVelocityFromAngle(_spawnPosition, _targetPosition, _throwAngle);
        if (vel == Vector3.zero)
        {
            _visualizer.gameObject.SetActive(false);
            return;
        }

        _visualizer.DrawTrajectory(_spawnPosition, vel, _selectedPotionPhysics.AttractorPosition, _selectedPotionPhysics.GravityStrength);
    }

    // INPUT
    //--------------------------------------------------
    public void OnExit(InputAction.CallbackContext ctx)
    {
        DeselectPotion();
        Debug.Log("OnExit");
    }
    public void OnCancel(InputAction.CallbackContext ctx)
    {
        DeselectPotion();
        Debug.Log("OnCancel");
    }
    public void OnAim(InputAction.CallbackContext ctx)
    {
        if (!_selectedPotion || !_selectedPotionPhysics || !_selectedPotionBehaviour) return;
        if (_throwOnSelect) return;

        var pos = ctx.ReadValue<Vector2>();
        Ray ray = _camera.ScreenPointToRay(pos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _hitMask))
        {
            _validTarget = true;
            _targetPosition = hit.point;
        }
        else
        {
            _validTarget = false;
            _visualizer.gameObject.SetActive(false);
        }
    }
    public void OnThrow(InputAction.CallbackContext ctx)
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (!_validTarget) return;
        if (_selectedPotion == null || _selectedPotionPhysics == null || _selectedPotionBehaviour == null) return;

        if (!_throwOnSelect && ctx.performed)
            ThrowPotion();
    }

    // AIMBOT
    //--------------------------------------------------
    private Vector3 FindClosestTarget(Vector3 origin, Vector3 target)
    {
        Collider[] npcs = Physics.OverlapSphere(origin, _assistRadius, _npcMask);
        if (npcs == null || npcs.Length == 0)
        {
            _targetAI = null;
            return target;
        }

        Vector3 playerDir = (target - origin).normalized;
        Collider closest = null;
        float minDist = float.MaxValue;

        foreach (var col in npcs)
        {
            if (col == null)
                continue;

            Vector3 toNPC = col.transform.position - origin;
            float angle = Vector3.Angle(playerDir, toNPC.normalized);
            if (angle > _maxAssistAngle)
                continue;

            float dist = toNPC.magnitude;
            if (!(dist < minDist))
                continue;
            minDist = dist;
            closest = col;
        }

        _targetAI = closest != null ? closest.GetComponent<CharacterAIManager>() : null;
        return closest != null ? closest.transform.position : target;
    }
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (_camera == null) return;

        Vector3 origin = _camera.transform.position + _camera.transform.forward * _dstToCamZ
                                                    + _camera.transform.up * _dstToCamY;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, _assistRadius);

        Collider[] npcs = Physics.OverlapSphere(origin, _assistRadius, _npcMask);
        Gizmos.color = Color.yellow;
        foreach (var npc in npcs)
        {
            if (npc == null) continue;
            Gizmos.DrawLine(origin, npc.transform.position);
            Gizmos.DrawSphere(npc.transform.position, 0.1f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_targetPosition, 0.08f);
    }

}