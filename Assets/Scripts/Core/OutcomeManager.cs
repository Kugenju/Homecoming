using UnityEngine;

public class OutcomeManager : MonoBehaviour
{
    public static OutcomeManager Instance { get; private set; }
    private int targetMusicIndex = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// ����ѧ��Σ�յȼ�������������Ӧ�������ͼ
    /// </summary>
    public void EvaluateAndTriggerFinalEnding()
    {
        int highDangerCount = GameStateTracker.Instance.CountStudentsWithDangerAtLeast(4);
        string graphId = "Ending_Happiness"; // Ĭ�Ͻ��
        targetMusicIndex = 2;
        if (highDangerCount >= 5)
        {
            graphId = "Ending_Immortality";
            targetMusicIndex = 1;
        }          
        else if (highDangerCount > 3)
        {
            graphId = "Ending_Pain";
            targetMusicIndex = 3;
        }
            
            

        TriggerEndingByNarrativeGraph(graphId);
    }

    /// <summary>
    /// ����ָ��ID�Ľ������ͼ�����ţ��� startNodeId ��ʼ��
    /// </summary>
    public void TriggerEndingByNarrativeGraph(string graphId)
    {
        // 1. ������֣����ڳɾ�/��¼��
        GameStateTracker.Instance.UnlockEnding(graphId);

        // 2. ��������ͼ
        var graph = Resources.Load<NarrativeGraph>($"NarrativeGraphs/{graphId}");
        if (graph == null)
        {
            Debug.LogError($"[OutcomeManager] δ�ҵ��������ͼ: NarrativeGraphs/{graphId}");
            // ���˵�Ĭ�Ͻ��
            graph = Resources.Load<NarrativeGraph>("NarrativeGraphs/Ending_Happiness");
            if (graph == null)
            {
                Debug.LogError("Ĭ�Ͻ��Ҳȱʧ���������˵�");
                GameFlowController.Instance.EnterMainMenu();
                return;
            }
        }
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.ChangeMusicByIndex(targetMusicIndex);
            Debug.Log($"{targetMusicIndex}");
        }
        // 3. ���� DialogueManager ���ţ������������̣�
        DialogueManager.Instance.LoadAndPlayGraph(graph);
    }
}