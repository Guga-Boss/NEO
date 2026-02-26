using UnityEngine;

public class MyHealthBar: MonoBehaviour
{
    [Header("Referências")]
    public SpriteRenderer fillRenderer;
    [Header("Configurações")]
    public string mySortingLayer = "Default";
    void Start()
    {
        // Garante que as barras estejam na layer que você prefere
        if( fillRenderer != null )
        {
            fillRenderer.sortingLayerName = mySortingLayer;
            // O fundo também deve estar na mesma layer
            var bg = GetComponentInChildren<SpriteRenderer>();
            if( bg != null ) bg.sortingLayerName = mySortingLayer;
        }
    }

    public void SetHealth( float current, float max )
    {
        float percent = Mathf.Clamp01(current / max);
        // Alteramos apenas a escala X do preenchimento
        // Isso faz a barra "encher" ou "esvaziar" visualmente
        Vector3 newScale = fillRenderer.transform.localScale;
        newScale.x = percent;
        fillRenderer.transform.localScale = newScale;
    }
}