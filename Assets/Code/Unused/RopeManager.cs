using PathologicalGames;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class RopeManager: MonoBehaviour
{
    public static RopeManager I;

    #region Rope Database
    [Title("Rope Bank")]
    [TableList]
    public List<RopeConfig> ropeBank = new List<RopeConfig>(); // Configure your types here
    public enum Type
    {
        NONE = -1, CHAIN, FISHING_LINE,
    }
    #endregion

    #region Global Wind Settings
    [Title("Global Wind")]
    public bool isWindActive = true;
    public float windIntensity = 2f;
    public float windFrequency = 0.5f;
    public float ZPosition = -3f;

    public static Vector2 globalWindDirection; // Public so instances can read it
    #endregion

    private void Awake()
    {
        I = this;
    }
    [Title("Timed Wind Settings")]
    public float changeInterval = 5f;
    private float timer;
    private float windOffset;
    public float WindIntensityMin  = .1f;
    public float WindIntensityMax  = 2f;

    private void FixedUpdate()
    {
        if( !isWindActive )
        {
            globalWindDirection = Vector2.zero;
            return;
        }

        // Timer para pular para uma nova posição no Noise
        timer += Time.fixedDeltaTime;
        if( timer >= changeInterval || Map.I.AdvanceTurn )
        {
            timer = 0;
            // O segredo está aqui: pulamos para um valor longe no Perlin
            // Isso muda a direção "base", mas o ruído continua rodando a partir daí
            windOffset = Random.Range( 0f, 1000f );

            // Opcional: variar a intensidade a cada mudança brusca
            windIntensity = Random.Range( WindIntensityMin, WindIntensityMax );
        }

        // Somamos o tempo atual com o offset aleatório
        float time = (Time.time + windOffset) * windFrequency;

        // O Noise continua funcionando, garantindo que a corda não fique estática
        float windAngle = Mathf.PerlinNoise(time, 0f) * Mathf.PI * 2f;
        float variableForce = Mathf.PerlinNoise(time, time) * windIntensity;

        globalWindDirection = new Vector2( Mathf.Cos( windAngle ), Mathf.Sin( windAngle ) ) * variableForce;
    }

    // --- STATIC SPAWNER ---
    public static Rope SpawnRope( Type type, Transform origin, Transform destination )
    {
        // Find config in list without dictionary
        RopeConfig config = I.ropeBank[0];
        for( int i = 0; i < I.ropeBank.Count; i++ )
        {
            if( I.ropeBank[ i ].type == type )
            {
                config = I.ropeBank[ i ];
                break;
            }
        }

        // 1. Get from your PoolManager
        Transform tr = PoolManager.Pools["Pool"].Spawn("Rope");

        // 2. Calculate required nodes based on distance (Protegido contra divisão por zero)
        float dist = Vector2.Distance(origin.position, destination.position);
        float safeNodeDist = config.nodeDistance > 0f ? config.nodeDistance : 0.25f;
        int calcNodes = Mathf.Max(2, Mathf.CeilToInt(dist / safeNodeDist));


        calcNodes = 20;


        // 3. Setup the instance data
        Rope rope = tr.GetComponent<Rope>();
        if( rope == null ) rope = tr.gameObject.AddComponent<Rope>();

        // Usamos calcNodes novamente, garantindo que o número de elos seja perfeito 
        rope.Setup( config, origin, destination, calcNodes );

        return rope;
    }

    // Call this before returning to pool 
    public static void DespawnRope( Rope rope )
    {
        if( rope == null ) return;
        rope.Cleanup();
        PoolManager.Pools[ "Pool" ].Despawn( rope.transform );
    }

    [System.Serializable]
    public class RopeConfig
    {
        [HorizontalGroup("Split", 0.5f)]
        [BoxGroup("Split/General")]
        [EnumPaging, HideLabel]
        public Type type;

        [BoxGroup("Split/General")]
        [LabelWidth(50)]
        public string id; // Used to identify the config

        [TabGroup("Settings", "Visuals", Icon = SdfIconType.Brush)]
        [AssetsOnly]
        public GameObject linkPrefab;

        [TabGroup("Settings", "Visuals")]
        public Vector3 linkScale = new Vector3(1, 1, 1);

        [TabGroup("Settings", "Visuals")]
        public Color ropeColor = Color.white;

        [TabGroup("Settings", "Visuals")]
        [MinValue(0)]
        public float lineWidth;

        [TabGroup("Settings", "Physics", Icon = SdfIconType.Activity)]
        public Vector2 gravity;

        [TabGroup("Settings", "Physics")]
        [Range(5, 100)]
        public int stiffness = 20;

        [TabGroup("Settings", "Physics")]
        [Range(0f, 1f)]
        public float damping = 0.95f;

        [TabGroup("Settings", "Dynamic", Icon = SdfIconType.Layers)]
        [InfoBox("Multiplicador de distância para o cálculo automático de nodes.")]
        [Range(0f, 10f)]
        public float AutoLengthMultiplier = 1;

        [TabGroup("Settings", "Dynamic")]
        [MinValue(0.01f)]
        public float nodeDistance = 0.25f;
    }
}