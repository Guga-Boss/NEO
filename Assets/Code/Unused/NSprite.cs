using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;                                                                       // CORREÇÃO: Impede erro de compilação na Build Final
#endif

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
        if( renderer == null ) return;                                                   // Prevent null reference
        renderer.sprite = sprite;                                                        // Apply sprite
        renderer.color = baseColor;                                                      // Apply color
        UpdateVisuals();                                                                 // Update bounds
    }

    private int _spriteId = -2;                                                          // Valor impossível para garantir que o primeiro load ocorra
    public int spriteId
    {
        get => _spriteId;
        set
        {
            _spriteId = value;
            TkSpriteId = value;

            if( IDM.I == null || collection == ESpriteCol.NONE ) return;                 // Skip if no manager or collection

            Sprite sp = IDM.I.GetSpriteById( collection, _spriteId );
            if( sp != null )
            {
                sprite = sp;
#if UNITY_EDITOR
                // 🟢 SOLUÇÃO DOS 128 BYTES NO PROFILER:
                // Bloqueamos a leitura da string durante o Play Mode do Editor.
                if( Application.isPlaying == false )
                {
                    spriteName = sp.name;
                }
#endif
                UpdateVisuals();
            }
        }
    }

    // --- ZERO GC CACHE ---
    private Button _cachedButton;
    private Image _cachedImage;
    private bool _buttonChecked = false;

    private void ValidateButtonSupport()
    {
        if( !_buttonChecked )                                                            // OTIMIZAÇÃO: Só busca os componentes uma única vez!
        {
            _cachedButton = GetComponent<Button>();
            _cachedImage = GetComponent<Image>();
            _buttonChecked = true;
        }

        // Se tem Botão mas não tem alvo para o mouse...
        if( _cachedButton != null && _cachedButton.targetGraphic == null )
        {
            // ...cria ou busca uma Image
            if( _cachedImage == null )
            {
                _cachedImage = gameObject.AddComponent<Image>();
                _cachedImage.color = new Color( 0, 0, 0, 0 );                            // Transparente
                _cachedImage.raycastTarget = true;                                       // Detectável pelo Mouse
            }
            _cachedButton.targetGraphic = _cachedImage;                                  // Conecta ao botão
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

        if( this == null || Render == null ) return;                                     // previne chamadas em objetos destruído

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

        if( Render.sprite != sprite ) Render.sprite = sprite;                            // OTIMIZAÇÃO: Só atribui se for diferente
        if( Render.color != baseColor ) Render.color = baseColor;                        // OTIMIZAÇÃO: Só atribui se for diferente

        if( sprite == null ) return;                                                     // Stop if no sprite

        // spriteName = sprite.name;                                                     // ALERTA DE GC ALLOC: Descomente só se precisar muito dessa string

        if( Render.drawMode != SpriteDrawMode.Sliced )                                   // OTIMIZAÇÃO: Só altera drawmode se diferente
            Render.drawMode = SpriteDrawMode.Sliced;

        Vector2 nativeSize = sprite.bounds.size;

        Vector2 targetSize;
        if( preserveAspect && nativeSize.y != 0 )
        {
            float ratio = nativeSize.x / nativeSize.y;
            targetSize = new Vector2( nativeSize.x * scale.x, ( nativeSize.x * scale.x ) / ratio );
        }
        else
        {
            targetSize = new Vector2( nativeSize.x * scale.x, nativeSize.y * scale.y );
        }

        if( Render.size != targetSize ) Render.size = targetSize;                        // OTIMIZAÇÃO: Só altera o size se for diferente

#if UNITY_EDITOR
        // MÁGICA AQUI: Informa ao Unity que o componente mudou e deve ser salvo na Prefab!
        if( !Application.isPlaying )
        {
            UnityEditor.EditorUtility.SetDirty( this );
            if( Render != null ) UnityEditor.EditorUtility.SetDirty( Render );
        }
#endif
    }

    // CORREÇÃO: Função liberada do Editor-Only para ser acessada pelas lógicas do jogo compilado
    public void SetSprite( ESpriteCol col, int id )
    {
        collection = col;
        spriteId = id;
    }
}