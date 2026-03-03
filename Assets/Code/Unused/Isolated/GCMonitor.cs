using UnityEngine;
using Unity.Profiling;
using System.Collections.Generic;

/// <summary>
/// Monitor avançado de alocação de memória (GC) por frame.
/// Otimizado para não gerar lixo (Zero-Garbage) durante a coleta.
/// </summary>
public class GCMonitorAdvanced: MonoBehaviour
{
    [Header("Configurações de Log")]
    [Tooltip("Intervalo em segundos entre cada relatório automático no console.")]
    public float logInterval = 1f;

    [Tooltip("Multiplicador para ignorar picos (spikes). Valores acima de (Média * SpikeFactor) são descartados no cálculo da média filtrada.")]
    public float spikeFactor = 2f;

    // Recorders da API do Profiler do Unity (Baixo nível, alta precisão)
    private ProfilerRecorder gcRecorder;
    private ProfilerRecorder systemMemoryRecorder;

    private float timer = 0f;
    private bool isActive = false;

    // Lista pré-alocada para evitar Resize e Garbage Collection durante a execução
    private List<long> gcFrames = new List<long>(2048);

    #region Ciclo de Vida
    void OnEnable()
    {
        // Inicializa os gravadores de memória do sistema
        gcRecorder = ProfilerRecorder.StartNew( ProfilerCategory.Memory, "GC Allocated In Frame" );
        systemMemoryRecorder = ProfilerRecorder.StartNew( ProfilerCategory.Memory, "System Used Memory" );
    }

    void OnDisable()
    {
        // Libera os recursos dos gravadores para evitar memory leak do Profiler
        gcRecorder.Dispose();
        systemMemoryRecorder.Dispose();
    }
    #endregion

    void Update()
    {
        HandleInput();

        if( !isActive ) return;

        // Coleta o valor bruto alocado neste frame específico (em Bytes)
        long gcThisFrame = gcRecorder.LastValue;
        gcFrames.Add( gcThisFrame );

        // Controle de tempo para o log automático
        timer += Time.unscaledDeltaTime;
        if( timer >= logInterval )
        {
            LogStatistics( false ); // Log Automático
            timer = 0f;
        }
    }

    /// <summary>
    /// Gerencia a lógica de entrada: G para ativar, Alt + G para desativar.
    /// </summary>
    private void HandleInput()
    {
        bool isAltPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

        // Se apertar G
        if( Input.GetKeyDown( KeyCode.G ) )
        {
            // Caso ALT esteja pressionado e o monitor esteja ATIVO -> DESATIVA
            if( isAltPressed && isActive )
            {
                isActive = false;
                Debug.Log( "<color=red><b>[GC Monitor] DESATIVADO</b></color>" );
                gcFrames.Clear();
            }
            // Caso ALT NÃO esteja pressionado e o monitor esteja DESATIVADO -> ATIVA
            else if( !isAltPressed && !isActive )
            {
                isActive = true;
                timer = 0f;
                gcFrames.Clear();
                Debug.Log( "<color=green><b>[GC Monitor] ATIVADO</b></color>" );
            }
            // Caso G seja apertado sozinho enquanto ATIVO -> LOG MANUAL
            else if( !isAltPressed && isActive )
            {
                LogManual();
            }
        }
    }

    /// <summary>
    /// Processa os dados acumulados e exibe no console.
    /// Usa loops manuais para garantir que o monitoramento não gere GC.
    /// </summary>
    private void LogStatistics( bool isManual )
    {
        int totalFrames = gcFrames.Count;
        if( totalFrames == 0 ) return;

        // 1. Cálculo da Média Bruta (para servir de base ao filtro de spikes)
        long totalSum = 0;
        for( int i = 0; i < totalFrames; i++ )
            totalSum += gcFrames[ i ];

        double rawAvg = (double)totalSum / totalFrames;

        // 2. Filtro de Spikes e busca de Extremos (Min/Max)
        // Valores muito altos (spikes de carregamento) distorcem a média de performance real
        long filteredSum = 0;
        int filteredCount = 0;
        long min = long.MaxValue;
        long max = 0;

        for( int i = 0; i < totalFrames; i++ )
        {
            long val = gcFrames[i];

            // Critério de filtragem: valor deve ser menor que a média * fator
            if( val <= rawAvg * spikeFactor )
            {
                filteredSum += val;
                filteredCount++;
                if( val < min ) min = val;
                if( val > max ) max = val;
            }
        }

        // 3. Preparação dos dados finais
        if( filteredCount == 0 ) return;

        long avgFiltered = filteredSum / filteredCount;
        long totalSystemMemMB = systemMemoryRecorder.LastValue / (1024 * 1024);

        // Determina o cabeçalho do log
        string header = isManual ? "MANUAL" : "AUTOMÁTICO";

        Debug.Log( $"<b>[GC {header}]</b>\n" +
                  $"Amostras: {totalFrames} frames (Filtrados: {filteredCount})\n" +
                  $"Média: <color=yellow>{avgFiltered} B</color> | Min: {min} B | Max: {max} B\n" +
                  $"Memória do Sistema: {totalSystemMemMB} MB" );

        // Limpa a lista para o próximo ciclo sem desalocar a capacidade da memória
        gcFrames.Clear();
    }

    private void LogManual()
    {
        long gcNow = gcRecorder.LastValue;
        long totalMemNowMB = systemMemoryRecorder.LastValue / (1024 * 1024);
        Debug.Log( $"<color=cyan><b>[GC Snapshot]</b></color> Frame Atual: {gcNow} Bytes | Sistema: {totalMemNowMB} MB" );
    }
}