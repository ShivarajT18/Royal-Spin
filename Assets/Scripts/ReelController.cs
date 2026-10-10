using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Require the components needed to display and mask the reel.
[RequireComponent(typeof(RectTransform), typeof(Image), typeof(Mask))]
public sealed class ReelController : MonoBehaviour
{
    // Number of symbol cells created in the scrolling strip.
    private const int StripLength = 36;

    // Index of the target symbol in the strip.
    private const int TargetCenterIndex = 34;

    // Final strip position used when the reel stops.
    private const int FirstVisibleIndexAtRest = 34;

    [Header("Symbol Layout")]

    // Controls the size of each symbol's artwork.
    [SerializeField, Range(0.8f, 1.2f)]
    private float symbolSize = 1f;

    // Controls the vertical distance between symbol rows.
    [SerializeField, Range(1f, 2.5f)]
    private float rowSpacing = 1.5f;

    [Header("Symbols")]

    // Sprites used to display the slot machine symbols.
    [SerializeField]
    private Sprite[] symbolSprites;

    // RectTransform of the visible reel window.
    private RectTransform viewport;

    // RectTransform containing all scrolling symbol cells.
    private RectTransform strip;

    // Stores the Image component for each symbol cell.
    private readonly List<Image> cells =
        new List<Image>(StripLength);

    // Height of the visible reel window.
    private float cellHeight;

    // Vertical distance between consecutive symbol rows.
    private float rowStep;

    // Tracks whether the strip has reached its resting position.
    private bool stripIsAtRestEnd;

    // Index of the symbol currently selected by the reel.
    public int CurrentSymbolIndex { get; private set; }

    // Indicates whether this reel is currently spinning.
    public bool IsSpinning { get; private set; }

    // Initializes the reel using the symbol sprites supplied
    // by SlotGameManager.
    public void Initialize(Sprite[] sprites)
    {
        symbolSprites = sprites;
        viewport = GetComponent<RectTransform>();

        // Stop initialization if no symbol sprites are assigned.
        if (symbolSprites == null || symbolSprites.Length == 0)
        {
            Debug.LogError(
                "ReelController: No symbol sprites assigned.", this);
            return;
        }

        // Update UI layout calculations before creating the strip.
        Canvas.ForceUpdateCanvases();

        // Calculate the height of one visible symbol area.
        cellHeight = Mathf.Max(1f, viewport.rect.height);

        // Calculate the vertical spacing between symbol rows.
        rowStep = cellHeight * rowSpacing;

        // Create all symbol cells and their artwork.
        BuildStrip();

        // Set the initial symbol index and resting state.
        CurrentSymbolIndex = 0;
        stripIsAtRestEnd = false;
    }

    // Creates the scrolling strip and its symbol artwork.
    private void BuildStrip()
    {
        if (strip != null)
        {
            // Remove previously created symbol cells.
            for (int i = strip.childCount - 1; i >= 0; i--)
            {
                GameObject child = strip.GetChild(i).gameObject;

                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }
        }
        else
        {
            // Create the parent object for all symbol cells.
            GameObject stripObject = new GameObject(
                "Reel Strip", typeof(RectTransform));

            stripObject.transform.SetParent(transform, false);
            strip = stripObject.GetComponent<RectTransform>();
        }

        // Configure the strip to grow downward from its top edge.
        strip.anchorMin = new Vector2(0f, 1f);
        strip.anchorMax = new Vector2(1f, 1f);
        strip.pivot = new Vector2(0.5f, 1f);
        strip.anchoredPosition = Vector2.zero;

        // Set the total height of the scrolling strip.
        strip.sizeDelta =
            new Vector2(0f, rowStep * StripLength);

        // Clear references to any previously created cells.
        cells.Clear();

        // Create each symbol cell in the strip.
        for (int i = 0; i < StripLength; i++)
        {
            GameObject cellObject = new GameObject(
                "Symbol " + i.ToString("00"),
                typeof(RectTransform));

            cellObject.transform.SetParent(strip, false);

            RectTransform cellRect =
                cellObject.GetComponent<RectTransform>();

            // Anchor each cell across the width of the strip.
            cellRect.anchorMin = new Vector2(0f, 1f);
            cellRect.anchorMax = new Vector2(1f, 1f);
            cellRect.pivot = new Vector2(0.5f, 1f);

            // Position each cell below the previous one.
            cellRect.anchoredPosition =
                new Vector2(0f, -i * rowStep);

            cellRect.sizeDelta =
                new Vector2(0f, rowStep);

            // Create a child object for the symbol image.
            GameObject artworkObject = new GameObject(
                "Symbol Artwork",
                typeof(RectTransform),
                typeof(Image));

            artworkObject.transform.SetParent(
                cellObject.transform, false);

            RectTransform artworkRect =
                artworkObject.GetComponent<RectTransform>();

            // Center the artwork inside its symbol cell.
            artworkRect.anchorMin = new Vector2(0.5f, 0.5f);
            artworkRect.anchorMax = new Vector2(0.5f, 0.5f);
            artworkRect.pivot = new Vector2(0.5f, 0.5f);
            artworkRect.anchoredPosition = Vector2.zero;

            // Size the artwork relative to the reel window.
            artworkRect.sizeDelta = new Vector2(
                viewport.rect.width * 0.75f * symbolSize,
                cellHeight * 0.75f * symbolSize);

            Image image = artworkObject.GetComponent<Image>();

            // Preserve the sprite proportions and prevent it
            // from intercepting UI pointer clicks.
            image.preserveAspect = true;
            image.raycastTarget = false;

            // Assign a random starting symbol.
            image.sprite = RandomSymbol();

            // Store the image so it can be updated during spinning.
            cells.Add(image);
        }

        // Start the strip at its initial position.
        strip.anchoredPosition = Vector2.zero;
    }

    // Spins the reel toward the requested symbol index.
    // Duration controls the spin time; delay staggers reel starts.
    public IEnumerator SpinTo(
        int targetSymbolIndex, float duration, float delay)
    {
        // Stop if the sprites or symbol cells are not ready.
        if (symbolSprites == null ||
            symbolSprites.Length == 0 ||
            cells.Count != StripLength)
        {
            yield break;
        }

        // Wait before starting this reel, if a delay was requested.
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        IsSpinning = true;

        // Recalculate dimensions in case the UI layout changed.
        cellHeight = Mathf.Max(1f, viewport.rect.height);
        rowStep = cellHeight * rowSpacing;

        strip.sizeDelta =
            new Vector2(0f, rowStep * StripLength);

        // Keep the target index within the available sprite range.
        targetSymbolIndex = Mathf.Clamp(
            targetSymbolIndex, 0, symbolSprites.Length - 1);

        // Reset the strip rows and randomize their displayed symbols.
        for (int i = 0; i < StripLength; i++)
        {
            RectTransform row =
                (RectTransform)cells[i].transform.parent;

            row.anchoredPosition =
                new Vector2(0f, -i * rowStep);

            row.sizeDelta =
                new Vector2(0f, rowStep);

            cells[i].sprite = RandomSymbol();
        }

        // Place the selected outcome at the target strip index.
        cells[TargetCenterIndex].sprite =
            symbolSprites[targetSymbolIndex];

        // Reset the strip before starting its animation.
        strip.anchoredPosition = Vector2.zero;

        // Calculate how far the strip must move to reach its
        // final resting position.
        float totalDistance =
            FirstVisibleIndexAtRest * rowStep;

        float elapsed = 0f;

        // Prevent zero or extremely short animation durations.
        float safeDuration = Mathf.Max(0.25f, duration);

        // Animate the strip until the spin duration has elapsed.
        while (elapsed < safeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            // Calculate normalized animation progress.
            float t = Mathf.Clamp01(elapsed / safeDuration);

            // Apply easing for acceleration, cruising, and slowing.
            float eased = EaseInCruiseOut(t);

            // Move the strip vertically.
            strip.anchoredPosition =
                new Vector2(0f, totalDistance * eased);

            yield return null;
        }

        // Snap to the exact final position to avoid drift.
        strip.anchoredPosition =
            new Vector2(0f, totalDistance);

        // Record the result and mark the reel as stopped.
        CurrentSymbolIndex = targetSymbolIndex;
        stripIsAtRestEnd = true;
        IsSpinning = false;
    }

    // Returns a randomly selected symbol sprite.
    private Sprite RandomSymbol()
    {
        return symbolSprites[
            Random.Range(0, symbolSprites.Length)];
    }

    // Produces an easing curve with an initial ramp-up,
    // a sustained middle section, and a smooth slowdown.
    private static float EaseInCruiseOut(float t)
    {
        if (t < 0.16f)
        {
            // Acceleration phase.
            float ramp = t / 0.16f;
            return 0.16f * ramp * ramp;
        }

        if (t < 0.72f)
        {
            // Cruise phase.
            return 0.16f +
                ((t - 0.16f) / 0.56f) * 0.67f;
        }

        // Deceleration phase.
        float stop = (t - 0.72f) / 0.28f;

        return 0.83f +
            0.17f * (1f - Mathf.Pow(1f - stop, 3f));
    }
}