using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MultiplayerCircleBehavior : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int playerOwner = 1;

    private float spawnTime;
    private bool destroyed = false;
    private int currentTaps = 0;
    private int requiredTaps = 1;
    private Image circleImage;
    private MultiplayerTimerManager timerManager;
    private MultiplayerManager multiplayerManager;
    private bool tagSet = false;

    public int PlayerOwner
    {
        get { return playerOwner; }
        set { playerOwner = value; }
    }

    private void Start()
    {
        spawnTime = Time.time;

        Destroy(gameObject, lifetime);

        circleImage = GetComponent<Image>();

        timerManager = FindObjectOfType<MultiplayerTimerManager>();
        if (timerManager == null)
        {
            Debug.LogWarning("MultiplayerCircleBehavior: MultiplayerTimerManager not found in scene!");
        }

        multiplayerManager = FindObjectOfType<MultiplayerManager>();
        if (multiplayerManager == null)
        {
            Debug.LogWarning("MultiplayerCircleBehavior: MultiplayerManager not found in scene!");
        }

        try
        {
            gameObject.tag = "Circle";
            tagSet = true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("MultiplayerCircleBehavior: Cannot set tag 'Circle': " + e.Message);
            tagSet = false;
        }

        if (circleImage != null)
        {
            Color circleColor = circleImage.color;

            if (IsColorSimilar(circleColor, Color.red))
            {
                requiredTaps = 1;
            }
            else if (IsColorSimilar(circleColor, Color.blue))
            {
                requiredTaps = 2;
            }
            else if (IsColorSimilar(circleColor, Color.green))
            {
                requiredTaps = 3;
            }
            else
            {
                requiredTaps = 1;
            }

            Debug.Log($"MultiplayerCircleBehavior: Circle spawned for Player {playerOwner} with color {circleColor}, requiring {requiredTaps} taps");
        }
    }

    private bool IsColorSimilar(Color a, Color b, float tolerance = 0.1f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (destroyed)
        {
            return;
        }

        if (timerManager == null)
        {
            timerManager = FindObjectOfType<MultiplayerTimerManager>();
        }

        bool testActive = timerManager != null && timerManager.IsTestActive();

        if (!testActive)
        {
            Debug.Log("MultiplayerCircleBehavior: Click ignored - test not active");
            return;
        }

        currentTaps++;
        Debug.Log($"MultiplayerCircleBehavior: Circle tapped for Player {playerOwner}! Current taps: {currentTaps}/{requiredTaps}");

        if (currentTaps >= requiredTaps)
        {
            destroyed = true;

            float reactionTimeInSeconds = Time.time - spawnTime;
            int reactionTimeInMS = Mathf.RoundToInt(reactionTimeInSeconds * 1000);

            Debug.Log($"MultiplayerCircleBehavior: Circle completed for Player {playerOwner} with {requiredTaps} taps! Reaction time: {reactionTimeInMS} ms");

            if (multiplayerManager == null)
            {
                multiplayerManager = MultiplayerManager.Instance;
            }

            if (multiplayerManager != null)
            {
                if (playerOwner == 1)
                {
                    multiplayerManager.AddPlayer1ReactionTime(reactionTimeInMS);
                    multiplayerManager.IncrementPlayer1Score();
                }
                else if (playerOwner == 2)
                {
                    multiplayerManager.AddPlayer2ReactionTime(reactionTimeInMS);
                    multiplayerManager.IncrementPlayer2Score();
                }

                Debug.Log($"MultiplayerCircleBehavior: Recorded reaction time {reactionTimeInMS}ms for Player {playerOwner}");
            }
            else
            {
                Debug.LogWarning("MultiplayerCircleBehavior: MultiplayerManager not found - couldn't record reaction time");
            }

            Destroy(gameObject);
        }
        else
        {
            if (circleImage != null)
            {
                float darkenAmount = 0.2f * currentTaps / requiredTaps;
                circleImage.color = new Color(
                    circleImage.color.r - darkenAmount,
                    circleImage.color.g - darkenAmount,
                    circleImage.color.b - darkenAmount,
                    circleImage.color.a
                );
            }
        }
    }
}