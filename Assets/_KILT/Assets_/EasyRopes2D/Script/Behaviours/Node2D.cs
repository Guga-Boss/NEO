using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Node2D : MonoBehaviour
{
    #region Public Properties

    Rope2D _ropeParent = null;
    public Rope2D RopeParent
    {
        get
        {
            if( _ropeParent == null )
                _ropeParent = GetComponentInParent<Rope2D>();
            return _ropeParent;
        }
    }

    #endregion

    bool _needApplyIgnore = true;
    protected virtual bool NeedApplyIgnore
    {
        get
        {
            return _needApplyIgnore;
        }
        set
        {
            if( _needApplyIgnore == value )
                return;
            _needApplyIgnore = value;
        }
    }

    bool _started = false;
    protected virtual bool Started
    {
        get
        {
            return _started;
        }
        set
        {
            if( _started == value )
                return;
            _started = value;
        }
    }

    // ADD THIS: Track current coroutine
    private Coroutine _currentCollisionCoroutine;

    #region Unity Functions

    protected virtual void OnEnable()
    {
        if( Started )
            ApplyIgnoreRopeCollision();
    }

    protected virtual void Start()
    {
        if( !Started )
        {
            Started = true;
            ApplyIgnoreRopeCollision();
        }
    }

    protected virtual void OnDisable()
    {
        NeedApplyIgnore = true;
        // Stop coroutine when disabled
        if( _currentCollisionCoroutine != null )
        {
            StopCoroutine( _currentCollisionCoroutine );
            _currentCollisionCoroutine = null;
        }
    }

    // ADD THIS METHOD: Clean up coroutines when object is destroyed
    protected virtual void OnDestroy()
    {
        if( _currentCollisionCoroutine != null )
        {
            StopCoroutine( _currentCollisionCoroutine );
            _currentCollisionCoroutine = null;
        }
    }

    #endregion

    #region Helper Functions

    // MODIFIED METHOD: Add coroutine management
    public virtual void ApplyIgnoreRopeCollision( bool p_force = false )
    {
        // Stop previous coroutine if still running
        if( _currentCollisionCoroutine != null )
        {
            StopCoroutine( _currentCollisionCoroutine );
            _currentCollisionCoroutine = null;
        }

        _currentCollisionCoroutine = StartCoroutine( RequestApplyIgnoreRopeCollision( p_force ) );
    }

    // MODIFIED METHOD: Fixed version with cancellation checks
    protected virtual IEnumerator RequestApplyIgnoreRopeCollision( bool p_force )
    {
        // Step 1: Initial cancellation check
        if( !this || !gameObject.activeInHierarchy )
        {
            Debug.Log( "Rope collision cancelled - object inactive" );
            yield break;
        }

        if( RopeParent != null && ( NeedApplyIgnore || p_force ) )
        {
            NeedApplyIgnore = false;

            // Step 2: Cache components before any yields
            List<Collider2D> v_selfColliders = new List<Collider2D>( this.GetNonMarkedComponentsInChildren<Collider2D>() );
            v_selfColliders.MergeList( new List<Collider2D>( this.GetNonMarkedComponentsInParent<Collider2D>( false, false ) ) );

            // Step 3: Cache nodes list before yielding
            List<GameObject> v_nodes = new List<GameObject>();
            if( RopeParent != null )
            {
                v_nodes.AddRange( RopeParent.Nodes );
                v_nodes.MergeList( RopeParent.GetPluggedObjectsInRope() );
            }

            // Step 4: Process each node with safety checks
            for( int i = 0; i < v_nodes.Count; i++ )
            {
                GameObject v_object = v_nodes[ i ];

                // Check cancellation every iteration
                if( !this || !gameObject.activeInHierarchy || RopeParent == null )
                {
                    Debug.Log( "Rope collision setup cancelled mid-process" );
                    yield break;
                }

                if( v_object != null )
                {
                    yield return null; // Prevent lag

                    // Re-check after yield
                    if( !this || !gameObject.activeInHierarchy || v_object == null )
                        continue;

                    try
                    {
                        Collider2D[] list = v_object.GetNonMarkedComponentsInChildren<Collider2D>();
                        if( list == null )
                            continue;

                        List<Collider2D> v_otherColliders = new List<Collider2D>( list );
                        v_otherColliders.MergeList( new List<Collider2D>( v_object.GetNonMarkedComponentsInParent<Collider2D>( false, false ) ) );

                        foreach( Collider2D v_selfCollider in v_selfColliders )
                        {
                            if( v_selfCollider != null && !v_selfCollider.Equals( null ) )
                            {
                                foreach( Collider2D v_otherCollider in v_otherColliders )
                                {
                                    if( v_otherCollider != null && !v_otherCollider.Equals( null ) )
                                        Physics2D.IgnoreCollision( v_selfCollider, v_otherCollider );
                                }
                            }
                        }
                    }
                    catch( System.Exception e )
                    {
                        Debug.LogWarning( "Rope collision error: " + e.Message );
                        continue;
                    }
                }
            }
        }

        // Clear the coroutine reference when done
        _currentCollisionCoroutine = null;
    }

    public bool Cut()
    {
        if( RopeParent != null )
        {
            return RopeParent.CutNode( this.gameObject );
        }
        else
            KiltUtils.DestroyImmediate( this.gameObject );

        return true;
    }

    #endregion
}