
using System.Collections;
using UnityEngine;

public class LeverPositionToggle : MonoBehaviour
{
    public GameObject upLeverImage;
    public GameObject downLeverImage;
    public SlotGameManager manager;

    public float downDuration = 0.5f;

    private bool isPulling = false;

    void Start()
    {
        upLeverImage.SetActive(true);
        downLeverImage.SetActive(false);
    }

    public void ToggleLever()
    {
        if (isPulling)
            return;

        StartCoroutine(PullAndReturn());
    }

    private IEnumerator PullAndReturn()
    {
        isPulling = true;

        // Show DOWN image
        upLeverImage.SetActive(false);
        downLeverImage.SetActive(true);

        // Spin reels
        if (manager != null)
            manager.Spin();

        // Hold DOWN briefly
        yield return new WaitForSecondsRealtime(downDuration);

        // Automatically return to UP
        downLeverImage.SetActive(false);
        upLeverImage.SetActive(true);

        isPulling = false;
    }
}
