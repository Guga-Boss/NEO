using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class NSprite: MonoBehaviour
{
    [BoxGroup("Configurações")]
    [OnValueChanged("OnColChanged")]
    public ESpriteCol collection;

    [BoxGroup("Configurações"), Required, OnValueChanged("UpdateVisuals")]
    [SerializeField]
    [ValueDropdown("GetSpritesFromCol")]
    public Sprite sprite;

    [BoxGroup("Configurações")]
    [ReadOnly] public string spriteName;

    [BoxGroup("Configurações"), OnValueChanged("UpdateVisuals")]
    public Color baseColor = Color.white;
    public Color color
    {
        get => baseColor;
        set
        {
            baseColor = value;
            if( Image ) Image.color = baseColor;
            if( Render ) Render.color = baseColor;
        }
    }

    [BoxGroup("Layout")]
    [OnValueChanged("UpdateVisuals")]
    public Vector2 scale = Vector2.one;

    [BoxGroup("Layout")]
    [OnValueChanged("UpdateVisuals")]
    public bool preserveAspect = true;

    public SpriteRenderer Render;
    public Image Image;

#if UNITY_EDITOR
    // Altere a linha 38 para:
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if( this != null ) // Verifica se o objeto ainda existe
                UpdateVisuals();
        };
    }
#endif

    [BoxGroup("Layout")]
    [OnValueChanged("SyncSpriteFromId")] // Agora chama a função que busca o ID
    public int TkSpriteId = -1;

    // Função nova adicionada:
    private void SyncSpriteFromId()
    {
        spriteId = TkSpriteId; // Dispara a lógica da propriedade spriteId
    }

    private void OnColChanged()
    {
        if( collection == ESpriteCol.NONE ) return;
        sprite = null;
        spriteName = "";
        UpdateVisuals();
    }


    [BoxGroup("Configurações")]
    //[ShowIf("collection", ESpriteCol.NONE)] // Só aparece se a coleção for NONE
    [OnValueChanged("OnManualSpriteChanged")]
    public Sprite ManualSprite;

    private void OnManualSpriteChanged()
    {
        // 1. Atualiza a variável 'sprite' original (a que está bloqueada pelo Odin)
        sprite = ManualSprite;
        // 2. Atualiza o nome do sprite
        spriteName = ManualSprite != null ? ManualSprite.name : "";
        // 3. Força o Sprite Renderer a atualizar instantaneamente e salva na Prefab
        UpdateVisuals();
    }

    private IEnumerable<Sprite> GetSpritesFromCol()
    {
        if( IDM.I != null && collection != ESpriteCol.NONE )
        {
            if( IDM.I.CollectionsDic.TryGetValue( collection, out var col ) )
            {
                return col.Sprites;
            }
        }
        return null;
    }

    public void ApplyToRenderer( SpriteRenderer renderer )
    {
        if( renderer == null ) return;
        renderer.sprite = sprite;
        renderer.color = baseColor;
        UpdateVisuals();
    }

    private int _spriteId = -2; // Valor impossível para garantir que o primeiro load ocorra
    public int spriteId
    {
        get => _spriteId;
        set
        {
            _spriteId = value;
            TkSpriteId = value;

            if( IDM.I == null || collection == ESpriteCol.NONE ) return;

            Sprite sp = IDM.I.GetSpriteById(collection, _spriteId);
            if( sp != null )
            {
                sprite = sp;
                spriteName = sp.name;
                UpdateVisuals();
            }
        }
    }
    private void ValidateButtonSupport()
    {
        // Se tem Botão mas não tem alvo para o mouse...
        var btn = GetComponent<Button>();
        if( btn != null && btn.targetGraphic == null )
        {
            // ...cria ou busca uma Image
            var img = GetComponent<Image>();
            if( img == null )
            {
                img = gameObject.AddComponent<Image>();
                img.color = new Color( 0, 0, 0, 0 ); // Transparente
                img.raycastTarget = true;          // Detectável pelo Mouse
            }
            btn.targetGraphic = img; // Conecta ao botão
        }
    }
    public void UpdateVisuals()
    {
        if( Image )
        {
            //Image.sprite = sprite;
            //Image.color = baseColor;
            //if( sprite == null ) return;
            //spriteName = sprite.name;
            //Image.type = Image.Type.Sliced;
            //Vector2 nativeSize = sprite.bounds.size;
            //if( preserveAspect && nativeSize.y != 0 )
            //{
            //    float ratio = nativeSize.x / nativeSize.y;
            //    Image.rectTransform.sizeDelta = new Vector2( nativeSize.x * scale.x, ( nativeSize.x * scale.x ) / ratio );
            //}
            //else
            //{
            //    Image.rectTransform.sizeDelta = new Vector2( nativeSize.x * scale.x, nativeSize.y * scale.y );
            //}
            return;
        }

        if( this == null || Render == null ) return; // previne chamadas em objetos destruído

        // Chama a correção do botão logo em seguida
        ValidateButtonSupport();

        // 🟢 ADICIONE ESTAS 4 LINHAS AQUI:
        // Se o NSprite estiver bloqueado/nulo, mas o SpriteRenderer tiver uma imagem,
        // o script vai "adotar" essa imagem em vez de apagá-la.
        if( Application.isPlaying == false )
        if( sprite == null && Render.sprite != null )
        {
            sprite = Render.sprite;
        }

        Render.sprite = sprite;
        Render.color = baseColor;
        if( sprite == null ) return;
        spriteName = sprite.name;

        Render.drawMode = SpriteDrawMode.Sliced;
        Vector2 nativeSize = sprite.bounds.size;

        if( preserveAspect && nativeSize.y != 0 )
        {
            float ratio = nativeSize.x / nativeSize.y;
            Render.size = new Vector2( nativeSize.x * scale.x, ( nativeSize.x * scale.x ) / ratio );
        }
        else
        {
            Render.size = new Vector2( nativeSize.x * scale.x, nativeSize.y * scale.y );
        }





#if UNITY_EDITOR
        // MÁGICA AQUI: Informa ao Unity que o componente mudou e deve ser salvo na Prefab!
        if( !Application.isPlaying )
        {
            UnityEditor.EditorUtility.SetDirty( this );
            if( Render != null ) UnityEditor.EditorUtility.SetDirty( Render );
        }
#endif


    }

#if UNITY_EDITOR
    private static ESpriteCol GetCollectionFromTk2d( string tkColName )
    {
        string name = tkColName.ToUpper();
        if( name.Contains( "MONSTER" ) ) return ESpriteCol.MONSTER_ANIM;
        if( name.Contains( "ITEM" ) ) return ESpriteCol.ITEM;
        if( name.Contains( "BUILDING" ) ) return ESpriteCol.BUILDING;
        if( name.Contains( "MAP" ) || name.Contains( "PLAY" ) ) return ESpriteCol.MAP_PLAY;
        return ESpriteCol.NONE;
    }

    [InitializeOnLoadMethod]
    private static void InitMigrationActions()
    {
        Debug.Log( "🔌 [NSprite] Inicializando Ações de Migração (tk2d + NGUI)..." );

        // --- TK2D ACTIONS ---
        tk2dSpriteEditor.ConvertAction = ( obj ) =>
        {
            var sprite = obj as tk2dSprite;
            if( sprite != null ) NSprite.Convert( sprite );
        };

        tk2dSpriteEditor.FinalizeAction = ( obj ) =>
        {
            var sprite = obj as tk2dSprite;
            if( sprite != null ) NSprite.Finalize( sprite );
        };

        // Dentro de InitMigrationActions()
        tk2dTextMeshEditor.ConvertTextAction = ( obj ) =>
        {
            var text = obj as tk2dTextMesh;
            if( text != null ) NSprite.ConvertTk2dText( text );
        };

        tk2dTextMeshEditor.FinalizeTextAction = ( obj ) =>
        {
            var text = obj as tk2dTextMesh;
            if( text != null ) NSprite.FinalizeTk2dText( text );
        };
    }   

    public static void Convert( tk2dSprite tkSprite )
    {
        if( tkSprite == null || tkSprite.Collection == null ) return;

        string colNameTk = tkSprite.Collection.spriteCollectionName.Replace("Sprite Collection", "").Trim();
        ESpriteCol targetEnum = GetCollectionFromTk2d(colNameTk);
        int targetId = tkSprite.spriteId;

        IDM.InitSingleton();
        if( IDM.I == null ) return;

        Sprite sp = IDM.I.GetSpriteById(targetEnum, targetId);

        if( sp != null )
        {
            string childName = "Unity_Migration_Preview";
            Transform childTransform = tkSprite.transform.Find(childName);
            GameObject childGO;

            if( childTransform == null )
            {
                childGO = new GameObject( childName );
                childGO.transform.SetParent( tkSprite.transform );
                childGO.transform.localPosition = new Vector3( 0, 0, -0.01f );
                childGO.transform.localRotation = Quaternion.identity;
                childGO.transform.localScale = Vector3.one;
            }
            else
            {
                childGO = childTransform.gameObject;
            }

            // --- GARANTINDO A CRIAÇÃO ---

            // 1. NSprite
            NSprite nsComp = childGO.GetComponent<NSprite>();
            if( nsComp == null ) nsComp = childGO.AddComponent<NSprite>();

            // 2. SpriteRenderer (O motor visual do Unity)
            SpriteRenderer sr = childGO.GetComponent<SpriteRenderer>();
            if( sr == null ) sr = childGO.AddComponent<SpriteRenderer>();

            // Atribuição de dados
            nsComp.collection = targetEnum;
            nsComp._spriteId = targetId;
            nsComp.sprite = sp;
            nsComp.baseColor = tkSprite.color;
            nsComp.spriteName = sp.name;
            nsComp.scale = (Vector2) tkSprite.scale;

            sr.sprite = sp; // Essencial para o componente "existir" visualmente
            sr.sortingOrder = tkSprite.SortingOrder;
            sr.sortingLayerName = "Default"; // Conforme sua preferência de projeto
            sr.color = tkSprite.color;

            nsComp.UpdateVisuals();

            // Se estiver rodando isso via botão no Editor (Custom Inspector)
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty( childGO );
#endif

            tkSprite.enabled = false; // Desliga o antigo para ver o novo
            Debug.Log( $"<color=green>Convertido com sucesso:</color> {childGO.name} em {tkSprite.name}" );
        }
        else
        {
            Debug.LogError( $"Sprite não encontrado no IDM para o ID: {targetId}" );
        }
    }

    [Button( "FINAL CONVERT (INSTANCE ONLY)" ), GUIColor( 0, 0.8f, 1f )]
    public static void Finalize( tk2dSprite tkSprite )
    {
        if( tkSprite == null || tkSprite.Collection == null ) return;

        GameObject mainGO = tkSprite.gameObject;
        IDM.InitSingleton();
        if( IDM.I == null ) return;

        string colNameTk = tkSprite.Collection.spriteCollectionName.Replace("Sprite Collection", "").Trim();
        ESpriteCol targetEnum = GetCollectionFromTk2d(colNameTk);
        int targetId = tkSprite.spriteId;

        Sprite sp = IDM.I.GetSpriteById(targetEnum, targetId);
        if( sp == null ) return;

        Color colorToUse = tkSprite.color;
        Transform previewTransform = mainGO.transform.Find("Unity_Migration_Preview");
        if( previewTransform != null )
        {
            NSprite previewNS = previewTransform.GetComponent<NSprite>();
            if( previewNS != null ) colorToUse = previewNS.baseColor;
            Undo.DestroyObjectImmediate( previewTransform.gameObject );
        }

        Vector2 sc = (Vector2)tkSprite.scale;
        int so = tkSprite.SortingOrder;

        EditorApplication.delayCall += () =>
        {
            if( mainGO == null ) return;

            // --- UNPACK PREFAB ---
            if( PrefabUtility.IsPartOfPrefabInstance( mainGO ) )
            {
                GameObject rootPrefab = PrefabUtility.GetNearestPrefabInstanceRoot(mainGO);
                if( rootPrefab != null )
                    PrefabUtility.UnpackPrefabInstance( rootPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction );
            }

            // --- DEEP CLEAN ---
            var tk = mainGO.GetComponent<tk2dSprite>();
            var mr = mainGO.GetComponent<MeshRenderer>();
            var mf = mainGO.GetComponent<MeshFilter>();

            if( tk != null ) Undo.DestroyObjectImmediate( tk );
            if( mr != null ) Undo.DestroyObjectImmediate( mr );
            if( mf != null ) Undo.DestroyObjectImmediate( mf );

            GameObjectUtility.RemoveMonoBehavioursWithMissingScript( mainGO );

            // --- ADD NEW COMPONENTS ---
            mainGO.transform.localScale = Vector3.one;
            SpriteRenderer newSR = mainGO.AddComponent<SpriteRenderer>();
            NSprite newNS = mainGO.AddComponent<NSprite>();

            newSR.sortingOrder = so;
            newSR.sortingLayerName = "Default";
            newSR.sprite = sp;
            newSR.color = colorToUse;

            newNS.Render = newSR;
            newNS.collection = targetEnum;
            newNS._spriteId = targetId;
            newNS.sprite = sp;
            newNS.spriteName = sp.name;
            newNS.baseColor = colorToUse;
            newNS.scale = sc;
            newNS.UpdateVisuals();

            // --- LÓGICA DE VÍNCULO AO ROOT (UNIT) ---
            // Pegamos o root absoluto ou o pai da prefab
            GameObject prefabRoot = PrefabUtility.IsPartOfPrefabInstance(mainGO)
             ? PrefabUtility.GetOutermostPrefabInstanceRoot(mainGO)
             : mainGO.transform.root.gameObject;

            //if( prefabRoot != null )
            //{
            //    Unit unitScript = prefabRoot.GetComponent<Unit>();
            //    if( unitScript == null ) unitScript = mainGO.GetComponentInParent<Unit>();

            //    if( unitScript != null )
            //    {
            //        // Atribuição ao campo específico que você indicou
            //        unitScript.Control.RestingRadiusSprite = newNS;

            //        EditorUtility.SetDirty( unitScript );
            //        PrefabUtility.RecordPrefabInstancePropertyModifications( unitScript );
            //        Debug.Log( $"[NSprite] Sprite vinculado ao Unit Root: {prefabRoot.name}" );
            //    }
            //}

            EditorUtility.SetDirty( mainGO );
            Debug.Log( $"[NSprite] Finalizado e Vinculado: {mainGO.name}" );
        };
    }

    public static void ConvertTk2dText( tk2dTextMesh tkText )
    {
        if( tkText == null ) return;

        string childName = "TMP_Migration_Preview_TK2D";
        Transform childTransform = tkText.transform.Find(childName);
        GameObject childGO = childTransform == null ? new GameObject(childName) : childTransform.gameObject;

        childGO.transform.SetParent( tkText.transform );

        // 1. Transform e Pivot (Exatamente como na sua tela)
        childGO.transform.localPosition = new Vector3( -0.02f, 0f, -0.01f );
        childGO.transform.localRotation = Quaternion.identity;
        childGO.transform.localScale = new Vector3( 0.02f, 0.02f, 0.02f );
        childGO.layer = tkText.gameObject.layer;

        TextMeshPro tmp = childGO.GetComponent<TextMeshPro>() ?? childGO.AddComponent<TextMeshPro>();

        // Dimensões do RectTransform e Pivot
        tmp.rectTransform.sizeDelta = new Vector2( 50f, 5f );
        tmp.rectTransform.pivot = new Vector2( 0f, 0f );

        // 2. Text Input e Main Settings
        tmp.text = tkText.text;
        tmp.fontSize = 65f;
        tmp.color = Color.white;

        // 3. Color Gradient (Vertical)
        tmp.fontStyle = FontStyles.Bold; // Ativa o botão 'B' no Inspector
        tmp.enableVertexGradient = true;
        ColorUtility.TryParseHtmlString( "#FFFFFFFF", out Color topColor );
        ColorUtility.TryParseHtmlString( "#F8A110FF", out Color bottomColor );
        // Aplica as cores: TopLeft, TopRight, BottomLeft, BottomRight
        tmp.colorGradient = new VertexGradient( topColor, topColor, bottomColor, bottomColor );

        // 4. Spacing, Alignment e Wrapping
        tmp.characterSpacing = -7f;
        tmp.alignment = TextAlignmentOptions.BottomLeft; // Left + Bottom
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;

        // 5. Material e Shader Settings (A Magia do Visual)
        Material tempMat = tmp.fontMaterial;

        // Configurações de Face
        // Usando uma cor aproximada para o HDR amarelo/laranja da Face
        ColorUtility.TryParseHtmlString( "#F8C544", out Color faceColor );
        tempMat.SetColor( ShaderUtilities.ID_FaceColor, faceColor );
        tempMat.SetFloat( ShaderUtilities.ID_OutlineSoftness, 0f );
        tempMat.SetFloat( ShaderUtilities.ID_FaceDilate, 0.112f );

        // Configurações de Underlay (Sombra dura)
        tempMat.EnableKeyword( "UNDERLAY_ON" ); // Liga a checkbox na marra
        tempMat.SetColor( ShaderUtilities.ID_UnderlayColor, Color.black );
        tempMat.SetFloat( ShaderUtilities.ID_UnderlayOffsetX, 1f );
        tempMat.SetFloat( ShaderUtilities.ID_UnderlayOffsetY, -1f );
        tempMat.SetFloat( ShaderUtilities.ID_UnderlayDilate, 1f );
        tempMat.SetFloat( ShaderUtilities.ID_UnderlaySoftness, 0f );

        tmp.fontMaterial = tempMat;

        // 6. Mesh Renderer e Atualização
        MeshRenderer mr = childGO.GetComponent<MeshRenderer>();
        if( mr != null )
        {
            mr.sortingLayerName = "Default"; // Mantendo o padrão que você definiu
            mr.sortingOrder = tkText.SortingOrder + 1; // Preview fica na frente
        }

        tmp.ForceMeshUpdate();

        Debug.Log( $"[NSprite] Preview TMP c/ Calibração Perfeita criado em: {tkText.name}" );
    }

    public static void FinalizeTk2dText( tk2dTextMesh tkText )
    {
        if( tkText == null ) return;

        GameObject root = tkText.gameObject;
        Transform preview = root.transform.Find("TMP_Migration_Preview_TK2D");

        if( preview == null )
        {
            Debug.LogError( "Preview não encontrado! Converta antes de finalizar." );
            return;
        }

        // --- CAPTURA TUDO DO PREVIEW ---
        TextMeshPro p = preview.GetComponent<TextMeshPro>();

        string txt = p.text;
        Color col = p.color;
        float fSize = p.fontSize;
        FontStyles fStyle = p.fontStyle;
        float cSpacing = p.characterSpacing;
        TextAlignmentOptions align = p.alignment;
        Vector2 sDelta = p.rectTransform.sizeDelta;
        Vector2 piv = p.rectTransform.pivot;

        // CORREÇÃO DA ESCALA: Pegamos a escala real que estamos vendo no mundo
        Vector3 worldScaleGoal = preview.lossyScale;
        Vector3 lPosOffset = preview.localPosition;

        bool useGrad = p.enableVertexGradient;
        VertexGradient grad = p.colorGradient;
        Material sourceMat = p.fontMaterial;
        int sorting = tkText.SortingOrder;

        Undo.DestroyObjectImmediate( preview.gameObject );

        EditorApplication.delayCall += () =>
        {
            if( root == null ) return;

            var mr = root.GetComponent<MeshRenderer>();
            var mf = root.GetComponent<MeshFilter>();

            Undo.DestroyObjectImmediate( tkText );
            if( mr != null ) Undo.DestroyObjectImmediate( mr );
            if( mf != null ) Undo.DestroyObjectImmediate( mf );

            TextMeshPro finalTmp = root.AddComponent<TextMeshPro>();

            finalTmp.text = txt;
            finalTmp.color = col;
            finalTmp.fontSize = fSize;
            finalTmp.fontStyle = fStyle;
            finalTmp.characterSpacing = cSpacing;
            finalTmp.alignment = align;
            finalTmp.rectTransform.sizeDelta = sDelta;
            finalTmp.rectTransform.pivot = piv;
            finalTmp.enableVertexGradient = useGrad;
            finalTmp.colorGradient = grad;
            finalTmp.fontMaterial = new Material( sourceMat );

            // --- APLICAÇÃO DA ESCALA CORRIGIDA ---
            // Se o objeto tem um pai, precisamos ajustar a escala local para manter a escala de mundo
            if( root.transform.parent != null )
            {
                root.transform.localScale = new Vector3(
                    worldScaleGoal.x / root.transform.parent.lossyScale.x,
                    worldScaleGoal.y / root.transform.parent.lossyScale.y,
                    worldScaleGoal.z / root.transform.parent.lossyScale.z
                );
            }
            else
            {
                root.transform.localScale = worldScaleGoal;
            }

            // Aplica o deslocamento visual
            root.transform.position += root.transform.right * lPosOffset.x + root.transform.up * lPosOffset.y;

            MeshRenderer newMr = root.GetComponent<MeshRenderer>();
            if( newMr != null )
            {
                newMr.sortingLayerName = "Default";
                newMr.sortingOrder = sorting;
            }


            // --- LOGICA DE VINCULO NO FINALIZE ---

            // 1. Encontra a Raiz da Prefab (O pai de todos os ramos)
            GameObject prefabRoot = null;

            if( PrefabUtility.IsPartOfPrefabInstance( root ) )
            {
                // Se for uma instância na cena, pega o pai da prefab inteira
                prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot( root );
            }
            else
            {
                // Se não for prefab, ou for no modo de edição de asset, pega o root da hierarquia
                prefabRoot = root.transform.root.gameObject;
            }

            if( prefabRoot != null )
            {
                // 2. Busca o componente Unit na raiz encontrada
                Unit unitScript = prefabRoot.GetComponent<Unit>();

                // Fallback: se o script Unit não estiver exatamente no root, 
                // mas em algum lugar acima do texto, o GetComponentInParent acha.
                if( unitScript == null ) unitScript = root.GetComponentInParent<Unit>();

                if( unitScript != null )
                {
                    // 3. ATRIBUIÇÃO (Substitua 'seuCampoDeTexto' pelo nome da variável na classe Unit)
                    unitScript.LevelTxt = finalTmp;

                    // 4. Avisa o Unity que houve mudança para salvar no Prefab/Cena
                    EditorUtility.SetDirty( unitScript );

                    // Se for Prefab, registra a mudança para o botão "Apply" do Unity detectar
                    PrefabUtility.RecordPrefabInstancePropertyModifications( unitScript );

                    Debug.Log( $"[NSprite] Texto vinculado ao Root da Prefab: {prefabRoot.name}" );
                }
                else
                {
                    Debug.LogWarning( $"[NSprite] Root {prefabRoot.name} encontrado, mas não contém o script 'Unit'." );
                }
            }


                EditorUtility.SetDirty( root );
            Debug.Log( $"[NSprite] {root.name} finalizado com Escala de Mundo preservada ({worldScaleGoal})!" );
        };
    }

    // Mapeamento preciso de Anchor para TMP
    private static (TextAlignmentOptions alignment, Vector2 pivot) MapUnityAnchorToTMP( TextAnchor anchor )
    {
        switch( anchor )
        {
        case TextAnchor.UpperLeft: return (TextAlignmentOptions.TopLeft, new Vector2( 0, 1 ));
        case TextAnchor.UpperCenter: return (TextAlignmentOptions.Top, new Vector2( 0.5f, 1 ));
        case TextAnchor.UpperRight: return (TextAlignmentOptions.TopRight, new Vector2( 1, 1 ));
        case TextAnchor.MiddleLeft: return (TextAlignmentOptions.Left, new Vector2( 0, 0.5f ));
        case TextAnchor.MiddleCenter: return (TextAlignmentOptions.Center, new Vector2( 0.5f, 0.5f ));
        case TextAnchor.MiddleRight: return (TextAlignmentOptions.Right, new Vector2( 1, 0.5f ));
        case TextAnchor.LowerLeft: return (TextAlignmentOptions.BottomLeft, new Vector2( 0, 0 ));
        case TextAnchor.LowerCenter: return (TextAlignmentOptions.Bottom, new Vector2( 0.5f, 0 ));
        case TextAnchor.LowerRight: return (TextAlignmentOptions.BottomRight, new Vector2( 1, 0 ));
        default: return (TextAlignmentOptions.Center, new Vector2( 0.5f, 0.5f ));
        }
    }
    public void SetSprite( ESpriteCol col, int id )
    {
        collection = col;
        spriteId = id;
    }
#endif
}