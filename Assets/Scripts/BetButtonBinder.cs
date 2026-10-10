
using UnityEngine;

/// <summary>
/// Connects a bet button to a specific bet amount.
/// </summary>
public sealed class BetButtonBinder : MonoBehaviour
{
    public SlotGameManager Manager;
    public int OptionIndex;

    public void SelectBet()
    {
        if (Manager != null)
        {
            Manager.SetBet(OptionIndex);
        }
        else
        {
            Debug.LogError("SlotGameManager is not assigned!");
        }
    }
}
