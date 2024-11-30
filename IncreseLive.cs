using UnityEngine;

[CreateAssetMenu(fileName = "LifeIncrese")]
public class IncreseLive : CardAction
{
    public int lifeIncrese;

    // Esegue l'azione
    public override void Execute(Card_Info card, CardManager cardManager)
    {
        cardManager.Life_Player += lifeIncrese;
        Debug.Log($"La carta {card.NameCard} vita recupera{lifeIncrese}");
    }
}

