using System;
using UnityEngine;

[RequireComponent( typeof( LineRenderer ) )]
public class Rope: MonoBehaviour
{
    [HideInInspector] public RopeManager.RopeConfig config;
    public Transform pointA;
    public Transform pointB;
    public RopeNode[] nodes;
    public Transform[] visualLinks;
    public SpriteRenderer[] linkRenderers;
    public LineRenderer line;

    public void Setup( RopeManager.RopeConfig ropeConfig, Transform start, Transform end, int nodeCount )
    {
        // 1. SALVA A CORDA ANTIGA ANTES DE LIMPAR
        RopeNode[] oldNodes = nodes;

        // Limpa os elos visuais antigos
        Cleanup();

        config = ropeConfig;
        pointA = start;
        pointB = end;

        line = GetComponent<LineRenderer>();
        line.sortingLayerName = "Default";
        line.useWorldSpace = true;
        line.enabled = config.lineWidth > 0;

        nodes = new RopeNode[ nodeCount ];
        visualLinks = new Transform[ nodeCount - 1 ];
        linkRenderers = new SpriteRenderer[ nodeCount - 1 ];

        Vector2 startPos = pointA.position;
        float safeDistance = config.nodeDistance > 0f ? config.nodeDistance : 0.25f;

        for( int i = 0; i < nodeCount; i++ )
        {
            // --- LÓGICA DE PRESERVAÇÃO (FIM DO CHACOALHÃO) ---
            if( oldNodes != null && i < oldNodes.Length )
            {
                // Reaproveita o nó antigo exatamente onde ele estava (mantém a inércia e a curva)
                nodes[ i ] = oldNodes[ i ];
            }
            else if( oldNodes != null && oldNodes.Length > 0 )
            {
                // Se a corda cresceu, os nós novos nascem suavemente na ponta do último nó conhecido
                nodes[ i ] = new RopeNode( oldNodes[ oldNodes.Length - 1 ].posNow );
            }
            else
            {
                // Se é a primeira vez que a corda é criada, nasce no Ponto A
                nodes[ i ] = new RopeNode( startPos );
            }
            // --------------------------------------------------

            // Instancia os visuais (Elos)
            if( i < nodeCount - 1 && config.linkPrefab != null )
            {
                GameObject go = Instantiate(config.linkPrefab, transform);
                visualLinks[ i ] = go.transform;
                var sr = go.GetComponent<SpriteRenderer>();
                if( sr )
                {
                    sr.color = config.ropeColor;
                    linkRenderers[ i ] = sr;
                }
            }
            startPos.y -= safeDistance;
        }
    }

    private void FixedUpdate()
    {
        UpdateDestruction();
        if( pointA == null || pointB == null ) return;

        // --- LÓGICA DE REBUILD REALTIME ---
        float dist = Vector2.Distance(pointA.position, pointB.position);
        float safeNodeDist = config.nodeDistance > 0f ? config.nodeDistance : 0.25f;

        // Calcula quantos nodes deveriam existir agora
        int targetNodeCount = Mathf.Max(2, Mathf.CeilToInt((dist / safeNodeDist) * config.AutoLengthMultiplier ) );
        if( targetNodeCount < 3 ) targetNodeCount = 3;

        // Reconstrói se for a primeira vez OU se a diferença de tamanho for maior que 1 para QUALQUER tipo de corda
        if( nodes == null || Mathf.Abs( nodes.Length - targetNodeCount ) > 1 )
        {
            Setup( config, pointA, pointB, targetNodeCount );
            // Sem o 'return' aqui, a física roda no mesmo frame e evita o chacoalhão!
        }

        // --- FÍSICA (VERLET) ---
        float dt = Time.fixedDeltaTime;
        Vector2 wind = RopeManager.globalWindDirection;

        for( int i = 0; i < nodes.Length; i++ )
        {
            var n = nodes[i];
            Vector2 velocity = (n.posNow - n.posOld) * config.damping;
            n.posOld = n.posNow;
            n.posNow += velocity + ( config.gravity + wind ) * dt;
            nodes[ i ] = n;
        }

        for( int j = 0; j < config.stiffness; j++ )
        {
            nodes[ 0 ].posNow = pointA.position;
            nodes[ nodes.Length - 1 ].posNow = pointB.position;

            for( int i = 0; i < nodes.Length - 1; i++ )
            {
                var n1 = nodes[i];
                var n2 = nodes[i + 1];
                float d = Vector2.Distance(n1.posNow, n2.posNow);
                if( d == 0 ) continue;
                float error = (d - config.nodeDistance) / d;
                Vector2 adj = (n1.posNow - n2.posNow) * 0.5f * error;
                if( i != 0 ) n1.posNow -= adj;
                n2.posNow += adj;
                nodes[ i ] = n1;
                nodes[ i + 1 ] = n2;
            }
        }
    }

    private void UpdateDestruction()
    {
        if( config.type == RopeManager.Type.FISHING_LINE )
        {
            if( Map.I.FishingMode == EFishingPhase.NO_FISHING )
                RopeManager.DespawnRope( this );
        }

        // 1. Verifica se as âncoras foram perdidas ou se o objeto deve ser destruído
        if( config.type == RopeManager.Type.CHAIN )
        if( pointA == null || pointB == null )
            {
                // Se um dos pontos sumiu (ex: inimigo morreu), limpamos a corrente
                RopeManager.DespawnRope( this );
                return;
            }
    }        

    private void LateUpdate()
    {
        if( nodes == null || nodes.Length == 0 ) return;

        // Renderiza a linha
        if( line != null && line.enabled )
        {
            line.startColor = line.endColor = config.ropeColor;
            line.startWidth = line.endWidth = config.lineWidth;
            line.positionCount = nodes.Length;
            for( int i = 0; i < nodes.Length; i++ )
                line.SetPosition( i, new Vector3( nodes[ i ].posNow.x, nodes[ i ].posNow.y, RopeManager.I.ZPosition ) );
        }

        // Renderiza os Sprites (Elos)
        for( int i = 0; i < visualLinks.Length; i++ )
        {
            var link = visualLinks[i];
            if( link == null ) continue;

            Vector2 p1 = nodes[i].posNow;
            Vector2 p2 = nodes[i + 1].posNow;
            Vector2 dir = p2 - p1;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            link.SetPositionAndRotation( new Vector3( p1.x, p1.y, RopeManager.I.ZPosition ), Quaternion.Euler( 0, 0, angle ) );
            link.localScale = config.linkScale == Vector3.zero ? Vector3.one : config.linkScale;
            if( linkRenderers[ i ] != null ) linkRenderers[ i ].color = config.ropeColor;
        }
    }

    public void Cleanup()
    {
        if( visualLinks != null )
        {
            foreach( var link in visualLinks )
                if( link != null ) Destroy( link.gameObject );
        }
    }

    [System.Serializable]
    public struct RopeNode
    {
        public Vector2 posNow, posOld;
        public RopeNode( Vector2 pos ) { posNow = posOld = pos; }
    }
}