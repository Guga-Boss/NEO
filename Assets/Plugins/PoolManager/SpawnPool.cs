using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace PathologicalGames
{
    /// <summary>
    /// Smart Tag component for ultra-fast O(1) despawning
    /// </summary>
    public class ModernPoolItem: MonoBehaviour
    {
        public string poolKey; // Stores the dictionary key
    }

    [AddComponentMenu( "Pooling System/Create Spawn Pool" )]
    public sealed class SpawnPool: MonoBehaviour, IList<Transform>
    {
        public Dictionary<string, ObjectPool<GameObject>> _modernPools = new Dictionary<string, ObjectPool<GameObject>>(); // Modern Unity engine

        #region Inspector Parameters
        public string poolName = ""; // Inspector name 
        public bool matchPoolScale = false; // Scale flag
        public bool matchPoolLayer = false; // Layer flag
        public bool dontReparent = false; // Reparent flag

        public bool dontDestroyOnLoad
        {
            get { return this._dontDestroyOnLoad; } // Getter
            set
            {
                this._dontDestroyOnLoad = value; // Setter
                if( this.group != null )
                    Object.DontDestroyOnLoad( this.group.gameObject ); // Make immortal
            }
        }
        public bool _dontDestroyOnLoad = false; // Backing field

        public bool logMessages = false; // Debug flag
        public List<PrefabPool> _perPrefabPoolOptions = new List<PrefabPool>(); // Inspector data
        public Dictionary<object, bool> prefabsFoldOutStates = new Dictionary<object, bool>(); // Editor GUI state
        #endregion

        #region Public Code-only Parameters
        public float maxParticleDespawnTime = 300; // Timeout
        public Transform group { get; private set; } // Root transform
        public PrefabsDict prefabs = new PrefabsDict(); // Cache
        public Dictionary<object, bool> _editorListItemStates = new Dictionary<object, bool>(); // Editor GUI state

        public Dictionary<string, PrefabPool> prefabPools
        {
            get
            {
                var dict = new Dictionary<string, PrefabPool>(); // Create map
                for( int i = 0; i < this._perPrefabPoolOptions.Count; i++ )
                {
                    if( this._perPrefabPoolOptions[ i ].prefab != null )
                        dict[ this._perPrefabPoolOptions[ i ].prefab.name ] = this._perPrefabPoolOptions[ i ]; // Map data
                }
                return dict; // Return map
            }
        }
        #endregion

        #region Private Properties
        internal List<Transform> _spawned = new List<Transform>(); // Legacy list for compatibility
        private HashSet<Transform> _spawnedHash = new HashSet<Transform>(); // Ultra-fast lookup set O(1)
        #endregion

        #region Constructor and Init
        private void Awake()
        {
            if( this._dontDestroyOnLoad ) Object.DontDestroyOnLoad( this.gameObject ); // Set persistence
            this.group = this.transform; // Cache transform

            if( string.IsNullOrEmpty( this.poolName ) )
            {
                this.poolName = this.group.name.Replace( "Pool", "" ).Replace( "(Clone)", "" ); // Clean name
            }

            if( this.logMessages )
                Debug.Log( $"SpawnPool {this.poolName}: Initializing Modern Pools.." ); // Log init

            foreach( PrefabPool prefPool in this._perPrefabPoolOptions )
            {
                if( prefPool.prefab == null ) continue; // Skip empty

                GameObject prefabGo = prefPool.prefab.gameObject; // Cache gameobject
                string prefabName = prefPool.prefab.name; // Cache name

                if( _modernPools.ContainsKey( prefabName ) ) continue; // Skip duplicates

                this.prefabs._Add( prefabName, prefPool.prefab ); // Cache reference

                var pool = new ObjectPool<GameObject>(
                    createFunc: () => {
                        GameObject inst = Instantiate(prefabGo, this.group); // Instantiate already parented
                        
                        var sr = inst.GetComponent<SpriteRenderer>(); // Find sprite
                        if (sr != null) sr.sortingLayerName = "Default"; // Force correct rendering layer

                        var ps = inst.GetComponent<ParticleSystem>(); // Find particles
                        if (ps != null)
                        {
                            ps.Emit(1); // Force memory allocation to prevent shader spikes
                            ps.Clear(); // Clear visual residue
                        }

                        var poolItem = inst.AddComponent<ModernPoolItem>(); // Add smart tag
                        poolItem.poolKey = prefabName; // Set fast lookup key

                        return inst; // Return new object
                    },
                    actionOnGet: (obj) => obj.SetActive(true), // Enable
                    actionOnRelease: (obj) => obj.SetActive(false), // Disable
                    actionOnDestroy: (obj) => Destroy(obj), // Cleanup
                    collectionCheck: false, // Performance mode
                    defaultCapacity: prefPool.preloadAmount, // Initial size
                    maxSize: prefPool.limitInstances ? prefPool.limitAmount : 10000 // Memory limit
                );

                _modernPools.Add( prefabName, pool ); // Register pool

                if( prefPool.preloadAmount > 0 )
                {
                    GameObject[] tempArray = new GameObject[prefPool.preloadAmount]; // Temporary array
                    for( int i = 0; i < prefPool.preloadAmount; i++ )
                        tempArray[ i ] = pool.Get(); // Warm up creation

                    for( int i = 0; i < prefPool.preloadAmount; i++ )
                        pool.Release( tempArray[ i ] ); // Return immediately
                }
            }

            PoolManager.Pools.Add( this ); // Register to global manager
        }

        private void OnDestroy()
        {
            PoolManager.Pools.Remove( this ); // Unregister
            this.StopAllCoroutines(); // Stop tasks
            this._spawned.Clear(); // Clear list
            this._spawnedHash.Clear(); // Clear hash
            this.prefabs._Clear(); // Clear cache
            this._modernPools.Clear(); // Clear pools
        }

        public void CreatePrefabPool( PrefabPool prefabPool )
        {
            if( prefabPool.prefab == null ) return; // Guard clause
            string pName = prefabPool.prefab.name; // Cache name

            if( !_modernPools.ContainsKey( pName ) )
            {
                this._perPrefabPoolOptions.Add( prefabPool ); // Register options
                this.prefabs._Add( pName, prefabPool.prefab ); // Cache prefab

                var pool = new ObjectPool<GameObject>(
                    createFunc: () => {
                        GameObject inst = Instantiate(prefabPool.prefab.gameObject, this.group); // Instantiate parented
                        
                        var sr = inst.GetComponent<SpriteRenderer>(); // Find sprite
                        if (sr != null) sr.sortingLayerName = "Default"; // Force correct layer
                        
                        var ps = inst.GetComponent<ParticleSystem>(); // Find particles
                        if (ps != null)
                        {
                            ps.Emit(1); // Prevent spikes
                            ps.Clear(); // Clear graphics
                        }

                        var poolItem = inst.AddComponent<ModernPoolItem>(); // Add smart tag
                        poolItem.poolKey = pName; // Store key

                        return inst; // Return instance
                    },
                    actionOnGet: (obj) => obj.SetActive(true), // Enable
                    actionOnRelease: (obj) => obj.SetActive(false), // Disable
                    actionOnDestroy: (obj) => Destroy(obj), // Destroy
                    collectionCheck: false, // Performance
                    defaultCapacity: prefabPool.preloadAmount, // Capacity
                    maxSize: prefabPool.limitInstances ? prefabPool.limitAmount : 10000 // Limit
                );

                _modernPools.Add( pName, pool ); // Add to dict
            }
        }

        public void Add( Transform instance, string prefabName, bool despawn, bool parent )
        {
            if( despawn ) instance.gameObject.SetActive( false ); // Turn off
            if( parent ) instance.SetParent( this.group ); // Parent it

            if( !despawn && this._spawnedHash.Add( instance ) )
                this._spawned.Add( instance ); // Add to trackers
        }
        #endregion

        #region Pool Functionality
        public Transform Spawn( Transform prefab, Vector3 pos, Quaternion rot, Transform parent )
        {
            Transform inst; // Transform ref

            if( _modernPools.TryGetValue( prefab.name, out var pool ) )
            {
                GameObject go = pool.Get(); // Fetch fast
                inst = go.transform; // Get component

                inst.position = pos; // Set position
                inst.rotation = rot; // Set rotation

                if( parent != null ) inst.SetParent( parent ); // Reparent custom
                else if( !this.dontReparent && inst.parent != this.group ) inst.SetParent( this.group ); // Reparent default

                if( this.matchPoolScale ) inst.localScale = Vector3.one; // Fix scale
                if( this.matchPoolLayer ) inst.gameObject.layer = this.gameObject.layer; // Fix layer

                if( this._spawnedHash.Add( inst ) )
                    this._spawned.Add( inst ); // Track instance fast O(1)

                return inst; // Return ready
            }

            Debug.LogWarning( $"Prefab {prefab.name} not in Modern Pool. Using Instantiate..." ); // Warn missing
            inst = Object.Instantiate( prefab, pos, rot ) as Transform; // Fallback
            if( parent != null ) inst.SetParent( parent ); // Set parent
            else inst.SetParent( this.group ); // Set group parent

            return inst; // Return fallback
        }

        public Transform Spawn( Transform prefab, Vector3 pos, Quaternion rot ) => this.Spawn( prefab, pos, rot, null ); // Overload
        public Transform Spawn( Transform prefab ) => this.Spawn( prefab, Vector3.zero, Quaternion.identity ); // Overload
        public Transform Spawn( Transform prefab, Transform parent ) => this.Spawn( prefab, Vector3.zero, Quaternion.identity, parent ); // Overload

        #region GameObject Overloads
        public Transform Spawn( GameObject prefab, Vector3 pos, Quaternion rot, Transform parent ) => Spawn( prefab.transform, pos, rot, parent ); // Overload
        public Transform Spawn( GameObject prefab, Vector3 pos, Quaternion rot ) => Spawn( prefab.transform, pos, rot ); // Overload
        public Transform Spawn( GameObject prefab ) => Spawn( prefab.transform ); // Overload
        public Transform Spawn( GameObject prefab, Transform parent ) => Spawn( prefab.transform, parent ); // Overload
        #endregion

        public Transform Spawn( string prefabName ) => this.Spawn( this.prefabs[ prefabName ] ); // Overload
        public Transform Spawn( string prefabName, Transform parent ) => this.Spawn( this.prefabs[ prefabName ], parent ); // Overload
        public Transform Spawn( string prefabName, Vector3 pos, Quaternion rot ) => this.Spawn( this.prefabs[ prefabName ], pos, rot ); // Overload
        public Transform Spawn( string prefabName, Vector3 pos, Quaternion rot, Transform parent ) => this.Spawn( this.prefabs[ prefabName ], pos, rot, parent ); // Overload

        public AudioSource Spawn( AudioSource prefab, Vector3 pos, Quaternion rot ) => this.Spawn( prefab, pos, rot, null ); // Overload
        public AudioSource Spawn( AudioSource prefab ) => this.Spawn( prefab, Vector3.zero, Quaternion.identity, null ); // Overload
        public AudioSource Spawn( AudioSource prefab, Transform parent ) => this.Spawn( prefab, Vector3.zero, Quaternion.identity, parent ); // Overload

        public AudioSource Spawn( AudioSource prefab, Vector3 pos, Quaternion rot, Transform parent )
        {
            Transform inst = Spawn(prefab.transform, pos, rot, parent); // Base spawn
            if( inst == null ) return null; // Guard

            var src = inst.GetComponent<AudioSource>(); // Get audio
            src.Play(); // Play sound
            this.StartCoroutine( this.ListForAudioStop( src ) ); // Wait for end
            return src; // Return
        }

        public ParticleSystem Spawn( ParticleSystem prefab, Vector3 pos, Quaternion rot ) => Spawn( prefab, pos, rot, null ); // Overload

        public ParticleSystem Spawn( ParticleSystem prefab, Vector3 pos, Quaternion rot, Transform parent )
        {
            Transform inst = this.Spawn(prefab.transform, pos, rot, parent); // Base spawn
            if( inst == null ) return null; // Guard

            var emitter = inst.GetComponent<ParticleSystem>(); // Get particles
            this.StartCoroutine( this.ListenForEmitDespawn( emitter ) ); // Wait for end
            return emitter; // Return
        }

        public void Despawn( Transform instance )
        {
            if( this._spawnedHash.Remove( instance ) )
                this._spawned.Remove( instance ); // Untrack instantly O(1)

            var poolItem = instance.GetComponent<ModernPoolItem>(); // Get smart tag

            if( poolItem != null && _modernPools.TryGetValue( poolItem.poolKey, out var pool ) )
            {
                pool.Release( instance.gameObject ); // Safely return to pool
            }
            else
            {
                Object.Destroy( instance.gameObject ); // Fallback destroy
            }
        }

        public void Despawn( Transform instance, Transform parent )
        {
            instance.parent = parent; // Reparent
            this.Despawn( instance ); // Despawn
        }

        public void Despawn( Transform instance, float seconds )
        {
            this.StartCoroutine( this.DoDespawnAfterSeconds( instance, seconds, false, null ) ); // Delayed despawn
        }

        public void Despawn( Transform instance, float seconds, Transform parent )
        {
            this.StartCoroutine( this.DoDespawnAfterSeconds( instance, seconds, true, parent ) ); // Delayed despawn with parent
        }

        private IEnumerator DoDespawnAfterSeconds( Transform instance, float seconds, bool useParent, Transform parent )
        {
            GameObject go = instance.gameObject; // Cache gameobject
            while( seconds > 0 )
            {
                yield return null; // Wait frame
                if( !go.activeInHierarchy ) yield break; // Break if disabled
                seconds -= Time.deltaTime; // Decrease timer
            }

            if( useParent ) this.Despawn( instance, parent ); // Call method
            else this.Despawn( instance ); // Call method
        }

        public void DespawnAll()
        {
            var spawned = new List<Transform>(this._spawned); // Copy active list
            for( int i = 0; i < spawned.Count; i++ )
                this.Despawn( spawned[ i ] ); // Despawn each safely
        }

        public bool IsSpawned( Transform instance )
        {
            return this._spawnedHash.Contains( instance ); // Instant check O(1)
        }
        #endregion

        #region Utility Functions
        public PrefabPool GetPrefabPool( Transform prefab )
        {
            foreach( var p in this._perPrefabPoolOptions )
                if( p.prefab == prefab ) return p; // Find match
            return null; // Not found
        }

        public PrefabPool GetPrefabPool( GameObject prefab )
        {
            foreach( var p in this._perPrefabPoolOptions )
                if( p.prefab != null && p.prefab.gameObject == prefab ) return p; // Find match
            return null; // Not found
        }

        public Transform GetPrefab( Transform instance )
        {
            var tag = instance.GetComponent<ModernPoolItem>(); // Read smart tag
            if( tag != null && this.prefabs.TryGetValue( tag.poolKey, out Transform pref ) ) return pref; // Return original
            return null; // Not found
        }

        public GameObject GetPrefab( GameObject instance )
        {
            Transform t = GetPrefab(instance.transform); // Call base
            return t != null ? t.gameObject : null; // Return GO
        }

        private IEnumerator ListForAudioStop( AudioSource src )
        {
            yield return null; // Safety frame
            while( src.isPlaying ) yield return null; // Wait until done
            this.Despawn( src.transform ); // Despawn
        }

        private IEnumerator ListenForEmitDespawn( ParticleSystem emitter )
        {
            yield return null; // Safety frame
            yield return new WaitForEndOfFrame(); // Safety end of frame

            float safetimer = 0; // Prevent infinite loop
            while( emitter.particleCount > 0 )
            {
                safetimer += Time.deltaTime; // Count time
                if( safetimer > this.maxParticleDespawnTime ) break; // Timeout
                yield return null; // Wait frame
            }
            this.Despawn( emitter.transform ); // Despawn
        }
        #endregion

        #region IList Legacy Support
        public override string ToString()
        {
            var name_list = new List<string>(); // Format string
            foreach( Transform item in this._spawned ) name_list.Add( item.name ); // Collect names
            return string.Join( ", ", name_list.ToArray() ); // Join string
        }

        public Transform this[ int index ]
        {
            get { return this._spawned[ index ]; } // Read only access
            set { throw new System.NotImplementedException( "Read-only." ); } // Denied
        }

        public void Add( Transform item ) { throw new System.NotImplementedException( "Use Spawn()" ); } // Denied
        public void Remove( Transform item ) { throw new System.NotImplementedException( "Use Despawn()" ); } // Denied
        public bool Contains( Transform item ) { return this._spawnedHash.Contains( item ); } // Implemented fast check
        public void CopyTo( Transform[ ] array, int arrayIndex ) { this._spawned.CopyTo( array, arrayIndex ); } // Supported
        public int Count { get { return this._spawned.Count; } } // Supported

        public IEnumerator<Transform> GetEnumerator() { return this._spawned.GetEnumerator(); } // Supported
        IEnumerator IEnumerable.GetEnumerator() { return this._spawned.GetEnumerator(); } // Supported

        public int IndexOf( Transform item ) { throw new System.NotImplementedException(); } // Denied
        public void Insert( int index, Transform item ) { throw new System.NotImplementedException(); } // Denied
        public void RemoveAt( int index ) { throw new System.NotImplementedException(); } // Denied
        public void Clear() { throw new System.NotImplementedException(); } // Denied
        public bool IsReadOnly { get { return true; } } // Supported
        bool ICollection<Transform>.Remove( Transform item ) { throw new System.NotImplementedException(); } // Denied
        #endregion
    }

    [System.Serializable]
    public class PrefabPool
    {
        public Transform prefab; // The prefab
        public int preloadAmount = 1; // Amount to warm
        public bool preloadTime = false; // Legacy param
        public int preloadFrames = 2; // Legacy param
        public float preloadDelay = 0; // Legacy param
        public bool limitInstances = false; // Flag for max
        public int limitAmount = 100; // Max allowed
        public bool limitFIFO = false; // Legacy param
        public bool cullDespawned = false; // Legacy param
        public int cullAbove = 50; // Legacy param
        public int cullDelay = 60; // Legacy param
        public int cullMaxPerPass = 5; // Legacy param
        public bool _logMessages = false; // Legacy param

        public PrefabPool() { } // Constructor
        public PrefabPool( Transform prefab ) { this.prefab = prefab; } // Constructor
    }

    public class PrefabsDict: IDictionary<string, Transform>
    {
        private Dictionary<string, Transform> _prefabs = new Dictionary<string, Transform>(); // Internal map

        public override string ToString()
        {
            var keysArray = new string[this._prefabs.Count]; // Name array
            this._prefabs.Keys.CopyTo( keysArray, 0 ); // Copy keys
            return string.Format( "[{0}]", string.Join( ", ", keysArray ) ); // Return formatted
        }

        internal void _Add( string prefabName, Transform prefab ) { this._prefabs[ prefabName ] = prefab; } // Internal add
        internal bool _Remove( string prefabName ) { return this._prefabs.Remove( prefabName ); } // Internal remove
        internal void _Clear() { this._prefabs.Clear(); } // Internal clear

        public int Count { get { return this._prefabs.Count; } } // Return count
        public bool ContainsKey( string prefabName ) { return this._prefabs.ContainsKey( prefabName ); } // Check key
        public bool TryGetValue( string prefabName, out Transform prefab ) { return this._prefabs.TryGetValue( prefabName, out prefab ); } // Get safely

        public Transform this[ string key ]
        {
            get
            {
                if( this._prefabs.TryGetValue( key, out Transform prefab ) ) return prefab; // Return match
                throw new KeyNotFoundException( $"Prefab '{key}' not found." ); // Crash if missing
            }
            set { throw new System.NotImplementedException( "Read-only." ); } // Denied
        }

        public ICollection<string> Keys { get { return this._prefabs.Keys; } } // Get keys
        public ICollection<Transform> Values { get { return this._prefabs.Values; } } // Get values
        private bool IsReadOnly { get { return true; } } // Status
        bool ICollection<KeyValuePair<string, Transform>>.IsReadOnly { get { return true; } } // Status

        public void Add( string key, Transform value ) { throw new System.NotImplementedException( "Read-Only" ); } // Denied
        public bool Remove( string prefabName ) { throw new System.NotImplementedException( "Read-Only" ); } // Denied
        public bool Contains( KeyValuePair<string, Transform> item ) { throw new System.NotImplementedException(); } // Denied
        public void Add( KeyValuePair<string, Transform> item ) { throw new System.NotImplementedException( "Read-only" ); } // Denied
        public void Clear() { throw new System.NotImplementedException(); } // Denied
        public bool Remove( KeyValuePair<string, Transform> item ) { throw new System.NotImplementedException( "Read-only" ); } // Denied
        public void CopyTo( KeyValuePair<string, Transform>[ ] array, int arrayIndex ) { throw new System.NotImplementedException(); } // Denied
        void ICollection<KeyValuePair<string, Transform>>.CopyTo( KeyValuePair<string, Transform>[ ] array, int arrayIndex ) { throw new System.NotImplementedException(); } // Denied

        public IEnumerator<KeyValuePair<string, Transform>> GetEnumerator() { return this._prefabs.GetEnumerator(); } // Iterator
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return this._prefabs.GetEnumerator(); } // Iterator
    }
}