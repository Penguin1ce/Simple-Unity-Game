using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class MainQuestManagerEditModeTests
{
    private GameObject questObject;
    private MainQuestManager questManager;

    [SetUp]
    public void SetUp()
    {
        questObject = new GameObject("MainQuestManager Test");
        questManager = questObject.AddComponent<MainQuestManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(questObject);
    }

    [Test]
    public void AcceptMainQuest_FromInitialStage_AdvancesToKillSlimeAndRaisesEvent()
    {
        int eventCount = 0;
        int changedStage = -1;
        string changedText = null;

        questManager.QuestStageChanged += (stage, text) =>
        {
            eventCount++;
            changedStage = stage;
            changedText = text;
        };

        Assert.AreEqual(MainQuestManager.NotAcceptedStage, questManager.QuestStage);

        questManager.AcceptMainQuest();

        Assert.AreEqual(MainQuestManager.KillSlimeStage, questManager.QuestStage);
        Assert.AreEqual(1, eventCount);
        Assert.AreEqual(MainQuestManager.KillSlimeStage, changedStage);
        Assert.AreEqual("击败史莱姆", changedText);
    }

    [Test]
    public void OnEnemyKilled_InExpectedOrder_CompletesMainQuest()
    {
        questManager.AcceptMainQuest();

        questManager.OnEnemyKilled(EnemyType.Slime);
        Assert.AreEqual(MainQuestManager.KillEliteStage, questManager.QuestStage);

        questManager.OnEnemyKilled(EnemyType.Elite);
        Assert.AreEqual(MainQuestManager.KillBossStage, questManager.QuestStage);

        questManager.OnEnemyKilled(EnemyType.Boss);
        Assert.AreEqual(MainQuestManager.CompletedStage, questManager.QuestStage);
        Assert.IsTrue(questManager.IsQuestCompleted);
    }

    [Test]
    public void OnEnemyKilled_InWrongStage_DoesNotAdvanceQuest()
    {
        questManager.OnEnemyKilled(EnemyType.Slime);
        questManager.OnEnemyKilled(EnemyType.Elite);
        questManager.OnEnemyKilled(EnemyType.Boss);
        Assert.AreEqual(MainQuestManager.NotAcceptedStage, questManager.QuestStage);

        questManager.AcceptMainQuest();
        questManager.OnEnemyKilled(EnemyType.Elite);
        Assert.AreEqual(MainQuestManager.KillSlimeStage, questManager.QuestStage);

        questManager.OnEnemyKilled(EnemyType.Slime);
        Assert.AreEqual(MainQuestManager.KillEliteStage, questManager.QuestStage);

        questManager.OnEnemyKilled(EnemyType.Boss);
        Assert.AreEqual(MainQuestManager.KillEliteStage, questManager.QuestStage);
    }

    [Test]
    public void CompletedQuest_DoesNotRegressAfterAcceptOrRepeatedKills()
    {
        questManager.AcceptMainQuest();
        questManager.OnEnemyKilled(EnemyType.Slime);
        questManager.OnEnemyKilled(EnemyType.Elite);
        questManager.OnEnemyKilled(EnemyType.Boss);

        Assert.AreEqual(MainQuestManager.CompletedStage, questManager.QuestStage);

        questManager.AcceptMainQuest();
        questManager.OnEnemyKilled(EnemyType.Slime);
        questManager.OnEnemyKilled(EnemyType.Elite);
        questManager.OnEnemyKilled(EnemyType.Boss);

        Assert.AreEqual(MainQuestManager.CompletedStage, questManager.QuestStage);
    }

    [Test]
    public void GetCurrentQuestText_ReturnsChineseObjectiveForEveryStage()
    {
        AssertQuestText(MainQuestManager.NotAcceptedStage, "与村口 NPC 对话");
        AssertQuestText(MainQuestManager.KillSlimeStage, "击败史莱姆");
        AssertQuestText(MainQuestManager.KillEliteStage, "击败精英怪");
        AssertQuestText(MainQuestManager.KillBossStage, "击败 Boss");
        AssertQuestText(MainQuestManager.CompletedStage, "主线任务已完成");
    }

    [Test]
    public void QuestEnemyMarker_InitializeElite_SetsEnemyType()
    {
        GameObject markerObject = new GameObject("QuestEnemyMarker Test");
        QuestEnemyMarker marker = markerObject.AddComponent<QuestEnemyMarker>();

        try
        {
            MethodInfo initialize = typeof(QuestEnemyMarker).GetMethod(
                "Initialize",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(EnemyType) },
                null);

            Assert.NotNull(initialize, "QuestEnemyMarker should expose Initialize(EnemyType).");

            initialize.Invoke(marker, new object[] { EnemyType.Elite });

            Assert.AreEqual(EnemyType.Elite, marker.EnemyType);
        }
        finally
        {
            Object.DestroyImmediate(markerObject);
        }
    }

    private void AssertQuestText(int stage, string expectedText)
    {
        questManager.SetQuestStageForDebug(stage);

        Assert.AreEqual(expectedText, questManager.GetCurrentQuestText());
    }
}
