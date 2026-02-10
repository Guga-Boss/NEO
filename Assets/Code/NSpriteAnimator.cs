using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class NSpriteAnimator: MonoBehaviour
{
  /*  [Header("Frames")]
    [Tooltip("Lista de IDs do IDM para animar.")]
    public List<int> frameIds = new List<int>();

    //[ShowInInspector, ReadOnly]
    //private List<IDM.SpriteData> frames = new List<IDM.SpriteData>();

    [Header("Configuração de Animação")]
    [Tooltip("Segundos por frame")]
    public float frameTime = 0.1f;

    [Tooltip("Faz a animação voltar depois de chegar no último frame")]
    public bool pingPong = false;

    [Tooltip("Loop infinito da animação")]
    public bool loop = true;

    private SpriteRenderer sr;
    private int currentFrame = 0;
    private float timer = 0f;
    private int direction = 1;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if( sr == null )
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }

        // Garante que a Sorting Layer seja sempre "Default" conforme sua instrução
        sr.sortingLayerName = "Default";

        ResolveFrames();
    }

    private void Update()
    {
        if( frames.Count <= 1 ) return;

        timer += Time.deltaTime;
        if( timer >= frameTime )
        {
            timer -= frameTime;
            StepFrame();
            ApplyFrame();
        }
    }

    [Button( "Atualizar Frames do IDM" )]
    public void ResolveFrames()
    {
        frames.Clear();
        if( IDM.I == null )
        {
            Debug.LogWarning( "[NSpriteAnimator] IDM.I não encontrado." );
            return;
        }

        foreach( int id in frameIds )
        {
            // Busca o SpriteData pelo ID no dicionário do IDM
            //if( IDM.I.MonsterAnimationDicById.TryGetValue( id, out var data ) )
            //{
            //    frames.Add( data );
            //}
        }

        currentFrame = 0;
        direction = 1;
        ApplyFrame();
    }

    private void StepFrame()
    {
        if( frames.Count == 0 ) return;

        currentFrame += direction;

        if( currentFrame >= frames.Count || currentFrame < 0 )
        {
            if( pingPong )
            {
                direction *= -1;
                currentFrame = Mathf.Clamp( currentFrame + ( direction * 2 ), 0, frames.Count - 1 );
            }
            else if( loop )
            {
                currentFrame = 0;
            }
            else
            {
                currentFrame = Mathf.Clamp( currentFrame, 0, frames.Count - 1 );
            }
        }
    }

    private void ApplyFrame()
    {
        if( frames.Count == 0 || sr == null ) return;

        var currentData = frames[currentFrame];
        if( currentData != null )
        {
            sr.sprite = currentData.sprite;
            //sr.color = currentData.baseColor;
        }
    }*/
}