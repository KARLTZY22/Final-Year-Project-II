using UnityEngine;

public class Scene2TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialMessage
    {
        public string title;
        [TextArea(3, 10)]
        public string message;
        public bool pauseGame = true;
        public AudioClip voiceOver;
    }

    public TutorialMessage[] scene2Tutorials;
    private bool hasShownIntro = false;

    void Start()
    {
        StartScene2Tutorials();
    }

    void StartScene2Tutorials()
    {
        if (!hasShownIntro)
        {
            foreach (var tutorial in scene2Tutorials)
            {
                TutorialManager.Instance.QueueMessage(
                    tutorial.title,
                    tutorial.message,
                    tutorial.voiceOver,
                    tutorial.pauseGame
                );
            }
            hasShownIntro = true;
        }
    }

    // Method to show boss tutorial
    public void ShowBossTutorial()
    {
        TutorialManager.Instance.QueueMessage(
            "Boss Battle",
            "Defeat the boss using your new weapons. Watch out for its powerful attacks!",
            null,
            true
        );
    }

    // Method to show machine tutorial
    public void ShowMachineTutorial()
    {
        TutorialManager.Instance.QueueMessage(
            "Destroy the Machine",
            "The machine must be destroyed to complete your mission.",
            null,
            true
        );
    }
}
