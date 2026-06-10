using UnityEngine;

public class QuestDebugTester : MonoBehaviour
{
    private bool _showPanel = true;

    private void OnGUI()
    {
        if (!_showPanel) return;

        int panelW = 300;
        int panelH = 280;
        int panelX = Screen.width - panelW - 10;
        int panelY = 10;

        Rect panelRect = new Rect(panelX, panelY, panelW, panelH);
        GUI.Box(panelRect, "Quest Debug Tester");
        GUI.BeginGroup(panelRect);

        int curY = 25;
        int pad = 10;
        int innerW = panelW - pad * 2;
        int btnH = 28;

        GUI.Label(new Rect(pad, curY, innerW, 20), "=== Quest Stage ===");
        curY += 22;

        if (MainQuestManager.Instance == null)
        {
            GUI.Label(new Rect(pad, curY, innerW, 20), "MainQuestManager not found.");
            GUI.EndGroup();
            return;
        }

        GUI.Label(new Rect(pad, curY, innerW, 20),
            string.Format("Current Stage: {0} ({1})",
                MainQuestManager.Instance.QuestStage,
                MainQuestManager.Instance.IsQuestCompleted ? "Completed" : "In Progress"));
        curY += 24;

        GUI.Label(new Rect(pad, curY, innerW, 20),
            string.Format("Quest Text: {0}", MainQuestManager.Instance.GetCurrentQuestText()));
        curY += 24;

        curY += 8;
        GUI.Label(new Rect(pad, curY, innerW, 20), "--- Advance Stage ---");
        curY += 22;

        for (int stage = 0; stage <= MainQuestManager.CompletedStage; stage++)
        {
            bool isCurrent = MainQuestManager.Instance.QuestStage == stage;

            GUI.enabled = !isCurrent;
            if (GUI.Button(new Rect(pad, curY, innerW, btnH),
                string.Format("Stage {0}{1}", stage, isCurrent ? " (current)" : "")))
            {
                MainQuestManager.Instance.SetQuestStageForDebug(stage);
            }
            GUI.enabled = true;
            curY += btnH + 3;
        }

        curY += 8;
        if (GUI.Button(new Rect(pad, curY, innerW, btnH), "Reset Quest"))
        {
            MainQuestManager.Instance.SetQuestStageForDebug(MainQuestManager.NotAcceptedStage);
        }

        GUI.EndGroup();
    }
}
