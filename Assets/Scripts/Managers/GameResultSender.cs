using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Ponte entre o C# e o JavaScript (GamePlugin.jslib).
/// Coloque este script em um GameObject chamado "GameResultSender" na cena.
/// </summary>
    public class GameResultSender : MonoBehaviour
{
    public static GameResultSender instance;

    [DllImport("__Internal")]
    private static extern void EnviarResultadoFase(string json);

    void Awake()
    {
        instance = this;
    }
    public void IniciarNovaPartida()
    {
        partidaId = System.Guid.NewGuid().ToString();
        tentativasPorFase.Clear();
    }
    /// <summary>
    /// Envia os dados da fase para a plataforma via fetch no JavaScript.
    /// </summary>
    private string partidaId;
    private Dictionary<int, int> tentativasPorFase = new Dictionary<int, int>();
    private string GetTipoOperacao(int fase)
    {
   
        switch (fase)
        {
            case 1: return "Adicao";
            case 2: return "Subtracao";
            case 3: return "Divisao";
            case 4: return "Multiplicacao";
            default: return "Desconhecido";
        }
    }

    [DllImport("__Internal")]
    private static extern void EnviarQuestao(string json);

    public void Enviar(int fase, int pontuacao, int acertos, int erros,
                       int aproveitamento, int tempoTotal,
                       string operacoesErradasJson, bool concluiuFase)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string tipoOperacao = GetTipoOperacao(fase);
        string json = $"{{" +
            $"\"fase\":{fase}," +
            $"\"tipo_operacao\":\"{tipoOperacao}\"," +
            $"\"pontuacao\":{pontuacao}," +
            $"\"acertos\":{acertos}," +
            $"\"erros\":{erros}," +
            $"\"aproveitamento\":{aproveitamento}," +
            $"\"tempo_total\":{tempoTotal}," +
            $"\"operacoes_erradas\":{operacoesErradasJson}," +
            $"\"concluiu_fase\":{(concluiuFase ? "true" : "false")}" +
        $"}}";

        Debug.Log("[GameResultSender] Enviando: " + json);
        EnviarResultadoFase(json);
#else
        Debug.Log("[GameResultSender] (editor) Envio ignorado fora do WebGL.");
#endif
    }

    public void IncrementarTentativa(int fase)
    {
        if (!tentativasPorFase.ContainsKey(fase)) tentativasPorFase[fase] = 1;
        tentativasPorFase[fase]++;
    }

    private int GetTentativa(int fase)
    {
        return tentativasPorFase.TryGetValue(fase, out int t) ? t : 1;
    }

    public void EnviarQuestaoAtual(int fase, string operacao, int numero,
        string conta, string respostaCorreta, string respostaAluno,
        bool acertou, float tempoSegundos)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    int tentativa = GetTentativa(fase);
    string contaEscapada = conta.Replace("\"", "\\\"");
    string json = $"{{" +
        $"\"partida_id\":\"{partidaId}\"," +
        $"\"fase\":{fase}," +
        $"\"operacao\":\"{operacao}\"," +
        $"\"tentativa\":{tentativa}," +
        $"\"numero\":{numero}," +
        $"\"conta\":\"{contaEscapada}\"," +
        $"\"resposta_correta\":\"{respostaCorreta}\"," +
        $"\"resposta_aluno\":\"{respostaAluno}\"," +
        $"\"acertou\":{(acertou ? "true" : "false")}," +
        $"\"tempo_segundos\":{tempoSegundos.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)}" +
    $"}}";
    Debug.Log("[GameResultSender] Enviando questao: " + json);
    EnviarQuestao(json);
#else
        Debug.Log("[GameResultSender] (editor) Envio de questao ignorado fora do WebGL.");
#endif
    }
}