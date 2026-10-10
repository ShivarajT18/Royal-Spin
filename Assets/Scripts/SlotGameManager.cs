using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class SlotGameManager : MonoBehaviour
{
    // Starting balance for a new practice session.
    private const int StartingCredits = 10000;

    [Header("Reels and symbol sprites")]

    // References to the three reel controllers.
    public ReelController[] Reels;

    // Available symbol sprites used by the reels.
    public Sprite[] SymbolSprites;

    [Header("HUD text")]

    // UI labels that display game information.
    public Text BalanceLabel;
    public Text BetLabel;
    public Text ResultLabel;
    public Text ResultDetailLabel;
    public Text SpinCountLabel;
    public Text WinCountLabel;
    public Text BestWinLabel;

    [Header("Controls")]

    // Main lever button used to start a spin.
    public Button LeverButton;

    // Buttons used to select different bet amounts.
    public Button[] BetButtons;

    // Button used to reset the current session.
    public Button ResetButton;

    [Header("Audio")]

    // Handles background music and game sound effects.
    public GameAudioManager AudioManager;

    // Available bet amounts.
    private readonly int[] betOptions = { 10, 50, 100 };

    // Payout multipliers for the symbol indexes.
    // Index 0 = Sevens, 1 = Cherries, 2 = Bells, 3 = Bar.
    private readonly int[] payoutMultipliers = { 25, 6, 8, 10 };

    // Current session data.
    private int balance = StartingCredits;
    private int currentBet = 10;
    private int spinCount;
    private int winCount;
    private int bestWin;

    // Prevents another spin while the reels are moving.
    private bool isSpinning;

    private void Start()
    {
        // Update UI layout before initializing the reels.
        Canvas.ForceUpdateCanvases();

        // Find the audio manager automatically if not assigned.
        if (AudioManager == null)
            AudioManager = FindObjectOfType<GameAudioManager>();

        // Initialize each reel with the available symbol sprites.
        if (Reels != null)
        {
            foreach (ReelController reel in Reels)
            {
                if (reel != null)
                    reel.Initialize(SymbolSprites);
            }
        }

        // Display the initial balance and session statistics.
        UpdateHud();

        // Display the initial game instructions.
        SetResult(
            "Ready when you are",
            "Match all three center symbols to win.");
    }

    // Selects a bet using its index in the betOptions array.
    // For example: 0 = 10, 1 = 50, 2 = 100 credits.
    public void SetBet(int optionIndex)
    {
        // Prevent bet changes during a spin or invalid selection.
        if (isSpinning ||
            optionIndex < 0 ||
            optionIndex >= betOptions.Length)
            return;

        // Apply the selected bet and refresh the UI.
        currentBet = betOptions[optionIndex];
        UpdateHud();
    }

    // Starts a new spin when the lever is activated.
    public void Spin()
    {
        // Ignore additional spin requests while spinning.
        if (isSpinning)
            return;

        // Make sure the player can afford the selected bet.
        if (balance < currentBet)
        {
            SetResult(
                "Not enough credits",
                "Press RESET to start a fresh practice session.");
            return;
        }

        // Validate that three reels and enough symbols exist.
        if (Reels == null ||
            Reels.Length != 3 ||
            SymbolSprites == null ||
            SymbolSprites.Length < 4)
        {
            SetResult(
                "Scene needs setup",
                "Assign three reels and at least four symbol sprites.");
            return;
        }

        // Check that every reel reference has been assigned.
        foreach (ReelController reel in Reels)
        {
            if (reel == null)
            {
                SetResult(
                    "Scene needs setup",
                    "Assign all three Reel Controller references.");
                return;
            }
        }

        // Lock the controls and charge the player for the spin.
        isSpinning = true;
        balance -= currentBet;
        spinCount++;

        // Randomly select an outcome for each reel.
        int[] outcomes = new int[Reels.Length];

        for (int i = 0; i < outcomes.Length; i++)
            outcomes[i] = Random.Range(0, SymbolSprites.Length);

        // Play the spinning sound effect.
        if (AudioManager != null)
            AudioManager.PlaySpin();

        // Display a message while the reels are moving.
        SetResult(
            "Spinning...",
            "The reels are finding their place.");

        UpdateHud();

        // Animate the reels and calculate the result afterward.
        StartCoroutine(ResolveSpin(outcomes));
    }

    // Spins all reels, checks the result, and awards any payout.
    private IEnumerator ResolveSpin(int[] outcomes)
    {
        // Start each reel with a slightly different duration and delay.
        for (int i = 0; i < Reels.Length; i++)
        {
            StartCoroutine(
                Reels[i].SpinTo(
                    outcomes[i],
                    1.55f + i * 0.23f,
                    i * 0.18f));
        }

        // Wait for the reel animations to finish.
        yield return new WaitForSecondsRealtime(2.6f);

        // Play a reel-stop sound for each of the three reels.
        if (AudioManager != null)
        {
            AudioManager.PlayReelStop();
            AudioManager.PlayReelStop();
            AudioManager.PlayReelStop();
        }

        // Read the final symbol index from each reel.
        int first = Reels[0].CurrentSymbolIndex;
        int second = Reels[1].CurrentSymbolIndex;
        int third = Reels[2].CurrentSymbolIndex;

        // A win occurs when all three symbols match.
        bool isWin = first == second && second == third;

        if (isWin)
        {
            // Use the matching symbol to determine the payout.
            int symbolIndex = first;

            int multiplier = payoutMultipliers[
                Mathf.Clamp(
                    symbolIndex, 0, payoutMultipliers.Length - 1)];

            // Calculate the reward based on the current bet.
            int payout = currentBet * multiplier;

            // Update the player's balance and statistics.
            balance += payout;
            winCount++;
            bestWin = Mathf.Max(bestWin, payout);

            // Get a readable name for the winning symbol.
            string symbolName = SymbolName(symbolIndex);

            // Play the appropriate winning sound.
            if (AudioManager != null)
            {
                if (symbolIndex == 0)
                    AudioManager.PlayJackpot();
                else
                    AudioManager.PlayWin();
            }

            // Display the win message and payout details.
            SetResult(
                symbolIndex == 0
                    ? "JACKPOT! TRIPLE SEVENS!"
                    : "TRIPLE " + symbolName + "!",
                "+" + payout.ToString("N0") +
                " credits - pays x" + multiplier);
        }
        else
        {
            // Display a message when the symbols do not match.
            SetResult(
                "No match this time",
                "Match all three center symbols on the payline.");
        }

        // Allow the next spin and refresh all HUD information.
        isSpinning = false;
        UpdateHud();
    }

    // Restores the starting credits and clears session statistics.
    public void ResetSession()
    {
        // Do not reset the game while the reels are spinning.
        if (isSpinning)
            return;

        balance = StartingCredits;
        currentBet = betOptions[0];
        spinCount = 0;
        winCount = 0;
        bestWin = 0;

        // Restore the initial result message.
        SetResult(
            "Ready when you are",
            "Line up three matching symbols to win.");

        UpdateHud();
    }

    // Updates the balance, bet, statistics, and button states.
    private void UpdateHud()
    {
        // Display the current credit balance.
        if (BalanceLabel != null)
            BalanceLabel.text = balance.ToString("N0") + "";

        // Display the selected bet per spin.
        if (BetLabel != null)
            BetLabel.text =
                "BET: " + currentBet.ToString("N0") +
                " CREDITS / SPIN";

        // Display the total number of spins.
        if (SpinCountLabel != null)
            SpinCountLabel.text = "SPINS  " + spinCount;

        // Display the number of winning spins.
        if (WinCountLabel != null)
            WinCountLabel.text = "WINS  " + winCount;

        // Display the largest single payout of the session.
        if (BestWinLabel != null)
            BestWinLabel.text = "BEST WIN  " +
                (bestWin > 0 ? bestWin.ToString("N0") : "-");

        // Disable the lever while spinning or when the bet
        // cannot be covered by the remaining balance.
        if (LeverButton != null)
            LeverButton.interactable =
                !isSpinning && balance >= currentBet;

        // Prevent bet changes during a spin.
        if (BetButtons != null)
        {
            for (int i = 0; i < BetButtons.Length; i++)
            {
                if (BetButtons[i] != null)
                    BetButtons[i].interactable = !isSpinning;
            }
        }

        // Prevent resetting the session during a spin.
        if (ResetButton != null)
            ResetButton.interactable = !isSpinning;
    }

    // Updates the main result message and its supporting detail.
    private void SetResult(string title, string detail)
    {
        if (ResultLabel != null)
            ResultLabel.text = title;

        if (ResultDetailLabel != null)
            ResultDetailLabel.text = detail;
    }

    // Converts a symbol index into its display name.
    private static string SymbolName(int index)
    {
        switch (index)
        {
            case 0: return "SEVENS";
            case 1: return "CHERRIES";
            case 2: return "BELLS";
            case 3: return "BAR";
            default: return "SYMBOLS";
        }
    }
}