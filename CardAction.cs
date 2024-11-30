using UnityEngine;

//Classe astratta che rappresenta un'azione generica della carta
public abstract class CardAction : ScriptableObject
{
    public abstract void Execute(Card_Info card, CardManager cardManager); // Riceve le informazioni della carta e cardManager della carte come parametri
}
