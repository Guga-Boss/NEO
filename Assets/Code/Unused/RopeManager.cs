using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using static ES2Settings;

[RequireComponent( typeof( LineRenderer ) )]
public class RopeManager: MonoBehaviour
{
    #region Variáveis de Configuração
    [TabGroup("Config", "Ancoragem")]
    [Required, SceneObjectsOnly] public Transform pontoA;
    [TabGroup("Config", "Ancoragem")]
    [Required, SceneObjectsOnly] public Transform pontoB;

    [TabGroup("Config", "Ajustes Visuais")]
    [Required, AssetsOnly] public GameObject nodePrefab;
    public float zOffset = 0f;
    [Range(0.01f, 1f)] public float larguraDaLinha = 0.1f;
    public Color corDaCorda = Color.white;

    [TabGroup("Config", "Corrente")]
    public bool usarSpritesDeCorrente = false;
    [ShowIf("usarSpritesDeCorrente")]
    public bool intercalarRotacao = true;

    [TabGroup("Config", "Controle de Tensão")]
    public bool distanciaAutomatica = true;
    [DisableIf("distanciaAutomatica")]
    public float distanciaEntreNos = 0.5f;
    [Range(2, 100)] public int quantidadeDeNos = 10;
    public bool pontaFinalPresa = false;

    [TabGroup("Config", "Física (Rigidez)")]
    [InfoBox("O Distance Joint trava o estiramento perto do pino. 'Max Distance Only' permite que a corda dobre, mas não estique.")]
    public bool usarDistanceJoint = true;
    [ShowIf("usarDistanceJoint")] public float travaDistancia = 0.05f;
    [ShowIf("usarDistanceJoint")] public bool apenasDistanciaMaxima = true;

    [TabGroup("Config", "Física Geral")]
    public bool colidirEntreNos = false;
    public float massaDosNos = 0.5f;
    public float arrastoLinear = 0.5f;
    public float gravidadeEscala = 1f;
    #endregion

    [SerializeField, HideInInspector]
    private List<GameObject> nosGerados = new List<GameObject>();
    private LineRenderer lineRenderer;


    [TabGroup( "Config", "Física (Rigidez)" )]
    [LabelText( "Modo de Rigidez" )]
    public enum RigidityMode { ChainBending, RigidRod }
    public RigidityMode modoRigidez = RigidityMode.ChainBending;

    [Button( ButtonSizes.Gigantic ), GUIColor( 0, 1, 0 )]
    public void GenerateRope()
    {
        ClearRope();

        // Verificação de segurança básica
        if( pontoA == null || pontoB == null || nodePrefab == null )
        {
            Debug.LogWarning( "RopeManager: Faltam referências (Ponto A, B ou Prefab)!" );
            return;
        }

        // 1. Configuração do LineRenderer (Regra: Sorting Layer "Default")
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.sortingLayerName = "Default";
        lineRenderer.startColor = corDaCorda;
        lineRenderer.endColor = corDaCorda;
        lineRenderer.startWidth = larguraDaLinha;
        lineRenderer.endWidth = larguraDaLinha;

        // 2. Cálculos de Distância
        float distReal = Vector2.Distance(pontoA.position, pontoB.position);
        float distFinal = distanciaAutomatica ? (distReal / (quantidadeDeNos - 1)) : distanciaEntreNos;
        Vector3 direcao = (pontoB.position - pontoA.position).normalized;
        Rigidbody2D ultimoRB = pontoA.GetComponent<Rigidbody2D>();

        for( int i = 0; i < quantidadeDeNos; i++ )
        {
            Vector3 pos = pontoA.position + (direcao * (i * distFinal));
            pos.z = zOffset;

            GameObject novoNo = Instantiate(nodePrefab, pos, Quaternion.identity, transform);
            novoNo.name = $"No_{i}";
            nosGerados.Add( novoNo );

            // --- Configuração Rigidbody2D ---
            Rigidbody2D rb = novoNo.GetComponent<Rigidbody2D>();
            rb.mass = massaDosNos;
            rb.linearDamping = arrastoLinear;
            rb.gravityScale = gravidadeEscala;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // --- CONFIGURAÇÃO DOS JOINTS (A CHAVE PARA NÃO ESTICAR) ---

            // 1. HingeJoint2D: Permite o balanço
            HingeJoint2D hinge = novoNo.GetComponent<HingeJoint2D>() ?? novoNo.AddComponent<HingeJoint2D>();
            hinge.connectedBody = ultimoRB;
            hinge.autoConfigureConnectedAnchor = false; // CORREÇÃO DO PRINT: Forçamos false
            hinge.connectedAnchor = Vector2.zero;        // Zera o erro de 164.202 do seu print
            hinge.anchor = Vector2.zero;

            // 2. FixedJoint2D: Trava a distância (Hard Lock)
            FixedJoint2D fixedJ = novoNo.GetComponent<FixedJoint2D>() ?? novoNo.AddComponent<FixedJoint2D>();
            fixedJ.connectedBody = ultimoRB;
            fixedJ.autoConfigureConnectedAnchor = false;
            fixedJ.connectedAnchor = Vector2.zero;
            fixedJ.anchor = Vector2.zero;

            // Frequência 0 e Damping 0 tornam a junta inquebrável e sem elasticidade
            fixedJ.dampingRatio = 0;
            fixedJ.frequency = 0;

            ultimoRB = rb;
        }

        // 4. Conectar opcionalmente ao Ponto B
        if( pontaFinalPresa ) ConectarPontaFinalManual();
    }

    private void ConectarPontaFinalManual()
    {
        Rigidbody2D rbB = pontoB.GetComponent<Rigidbody2D>();
        if( rbB != null && nosGerados.Count > 0 )
        {
            GameObject ultimoNo = nosGerados[nosGerados.Count - 1];

            FixedJoint2D fJoint = ultimoNo.AddComponent<FixedJoint2D>();
            fJoint.connectedBody = rbB;
            fJoint.autoConfigureConnectedAnchor = false;
            fJoint.connectedAnchor = Vector2.zero;
            fJoint.dampingRatio = 0;
            fJoint.frequency = 0;
        }
    }

    private void ConectarPontaFinal()
    {
        Rigidbody2D rbB = pontoB.GetComponent<Rigidbody2D>();
        if( rbB != null && nosGerados.Count > 0 )
        {
            GameObject lastNode = nosGerados[nosGerados.Count - 1];
            HingeJoint2D finalHinge = lastNode.AddComponent<HingeJoint2D>();
            finalHinge.connectedBody = rbB;
            finalHinge.autoConfigureConnectedAnchor = false;
            finalHinge.connectedAnchor = Vector2.zero;

            if( usarDistanceJoint )
            {
                DistanceJoint2D finalDist = lastNode.AddComponent<DistanceJoint2D>();
                finalDist.connectedBody = rbB;
                finalDist.autoConfigureDistance = false;
                finalDist.distance = travaDistancia;
                finalDist.maxDistanceOnly = apenasDistanciaMaxima;
            }
        }
    }

    [Button, GUIColor( 1, 0, 0 )]
    public void ClearRope()
    {
        foreach( var no in nosGerados ) { if( no != null ) DestroyImmediate( no ); }
        nosGerados.Clear();
    }

    private void Update()
    {
        if( nosGerados.Count > 0 )
        {
            RenderRope();
            if( usarSpritesDeCorrente ) UpdateChainRotation();
        }
    }

    private void UpdateChainRotation()
    {
        for( int i = 0; i < nosGerados.Count - 1; i++ )
        {
            if( nosGerados[ i ] == null || nosGerados[ i + 1 ] == null ) continue;
            Vector3 dir = nosGerados[i + 1].transform.position - nosGerados[i].transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            nosGerados[ i ].transform.rotation = Quaternion.Euler( 0, 0, angle );
        }
    }

    private void RenderRope()
    {
        if( lineRenderer == null || !lineRenderer.enabled ) return;
        lineRenderer.positionCount = nosGerados.Count + 1;
        lineRenderer.SetPosition( 0, pontoA.position );
        for( int i = 0; i < nosGerados.Count; i++ )
        {
            if( nosGerados[ i ] != null ) lineRenderer.SetPosition( i + 1, nosGerados[ i ].transform.position );
        }
    }

    #region Interações
    [TabGroup( "Config", "Interação" )]
    [Button]
    public void ShakeRope( float forca = 5f )
    {
        foreach( GameObject no in nosGerados )
        {
            Rigidbody2D rb = no.GetComponent<Rigidbody2D>();
            if( rb != null ) rb.AddForce( new Vector2( Random.Range( -1f, 1f ), Random.Range( -0.5f, 0.5f ) ) * forca, ForceMode2D.Impulse );
        }
    }

    [TabGroup( "Config", "Interação" )]
    [Button( ButtonSizes.Medium ), GUIColor( 1, 0.5f, 0 )]
    public void CutLastNodeAndApplyForce( Vector2 forcaDeCorte )
    {
        if( nosGerados.Count == 0 ) return;

        int ultimoIndex = nosGerados.Count - 1;
        GameObject ultimoNo = nosGerados[ultimoIndex];

        if( ultimoNo != null )
        {
            // Remove TODOS os joints (Hinge e Distance) para um corte real 
            Joint2D[] joints = ultimoNo.GetComponents<Joint2D>();
            foreach( var j in joints ) DestroyImmediate( j );

            Rigidbody2D rb = ultimoNo.GetComponent<Rigidbody2D>();
            if( rb != null ) rb.AddForce( forcaDeCorte, ForceMode2D.Impulse );

            nosGerados.RemoveAt( ultimoIndex ); // Remove da lista para parar o LineRenderer
        }
    }

    [TabGroup( "Config", "Interação" )]
    [Button]
    public void QuickCutLeft() => CutLastNodeAndApplyForce( new Vector2( -10, 5 ) );
    #endregion
}