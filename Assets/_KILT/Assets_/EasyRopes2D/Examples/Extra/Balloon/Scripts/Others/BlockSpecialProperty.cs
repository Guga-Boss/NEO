using UnityEngine;
using System.Collections;

public class BlockSpecialProperty : DisposableBehaviour 
{
	#region Private Variables

	[SerializeField]
	bool m_awaysAcceptClick = false; //Ignore Global Press Property in Block
	[SerializeField]
	GameObject m_radiusParticlePrefab = null;

	GameObject _currentRadiusAura = null;

	#endregion

	#region Protected Properties

	protected GameObject CurrentRadiusAura
	{
		get
		{
			return _currentRadiusAura;
		}
		set
		{
			if(_currentRadiusAura == value)
				return;
			_currentRadiusAura = value;
		}
	}

	#endregion

	#region Public Properties

	public bool AwaysAcceptClick
	{
		get
		{
			return m_awaysAcceptClick;
		}
		set
		{
			if(m_awaysAcceptClick == value)
				return;
			m_awaysAcceptClick = value;
		}
	}

	public GameObject RadiusParticlePrefab 
	{
		get {return m_radiusParticlePrefab;} 
		set 
		{
			if(m_radiusParticlePrefab == value) 
				return; 
			m_radiusParticlePrefab = value;
		}
	}

	#endregion

	#region Unity Functions

	protected virtual void Awake()
	{
		RegisterEvents();
		ShowParticleAuraRadius();
	}

	protected virtual void Start()
	{
	}

	protected virtual void OnEnable()
	{
		RegisterEvents();
		ShowParticleAuraRadius();
	}

	protected virtual void OnDisable()
	{
		UnregisterEvents();
		HideParticleAuraRadius();
	}
	
	protected virtual void OnDestroy()
	{
		UnregisterEvents();
	}

	protected void OnPress(bool p_isDown)
	{
		if(CanBeClicked() && p_isDown && Input.GetMouseButton(0) && enabled && gameObject.activeSelf && gameObject.activeInHierarchy)
		{
			OnUserClick();
		}
	}

	#endregion
	
	#region Event Implementations
	
	public virtual void OnUserClick()
	{
	}

	#endregion

	#region Event Receivers
	
	protected virtual void HandleOnDestroyedByUser ()
	{
	}
	
	protected virtual  void HandleOnDie ()
	{
	}
	
	protected virtual void HandleOnDamaged (DamageEventArgs p_args)
	{
	}
	
	#endregion

	#region Particle Aura
	
	public virtual bool ShowParticleAuraRadius(bool p_force = false)
	{
		bool v_sucess = false;
		if(RadiusParticlePrefab != null && (p_force || _currentRadiusAura == null))
		{
			HideParticleAuraRadius();
			_currentRadiusAura = BadBlocksUtils.InstantiateEffectOverOwner(this.transform, RadiusParticlePrefab, true, false);
			v_sucess = true;
		}
		return v_sucess;
	}
	
	public virtual bool HideParticleAuraRadius()
	{
		bool v_sucess = false;
		if(_currentRadiusAura != null)
		{
			KiltUtils.DestroyImmediate(_currentRadiusAura);
			_currentRadiusAura = null;
			v_sucess = true;
		}
		return v_sucess;
	}
	
	#endregion

	#region Helper Functions

	protected virtual void RegisterEvents()
	{
		UnregisterEvents();
		Block v_block = GetComponent<Block>();
		if(v_block != null)
		{
			v_block.OnDie += HandleOnDie;
			v_block.OnDestroyedByUser += HandleOnDestroyedByUser;
			v_block.OnDamaged += HandleOnDamaged;
		}
	}

	protected virtual void UnregisterEvents()
	{
		Block v_block = GetComponent<Block>();
		if(v_block != null)
		{
			v_block.OnDie -= HandleOnDie;
			v_block.OnDestroyedByUser -= HandleOnDestroyedByUser;
			v_block.OnDamaged -= HandleOnDamaged;
		}
	}

	public virtual bool CanBeClicked()
	{
		bool v_canBeClicked = Block.GlobalPressOnly? false : true;
		if(AwaysAcceptClick)
			v_canBeClicked = true;
		return v_canBeClicked;
	}

	#endregion
}
