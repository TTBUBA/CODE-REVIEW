using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class CardSpawner : MonoBehaviourPunCallbacks
{
    public GameObject PrefabsCard;
    public GameObject[] PointSpawnPlayer;
    public GameObject[] PointSpawnEnemy;
    public List<Card_Info> CardList;

    public Camera MainCamera;
    public CardManager CardManager;

    private List<GameObject> spawnedCards = new List<GameObject>();

    
    public void Start()
    {
        // Solo il Master Client spawna le carte
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnCards();
        }
    }

    // Metodo per spawnare le carte per il giocatore e il nemico
    private void SpawnCards()
    {
        // Spawn delle carte per il giocatore
        for (int i = 0; i < 1; i++)
        {
            GameObject card = PhotonNetwork.Instantiate(PrefabsCard.name, PointSpawnPlayer[i].transform.position, Quaternion.identity);
            ConfigureCard(card, PointSpawnPlayer[i].transform, false); // Configura la carta per il giocatore
        }

        // Spawn delle carte per il nemico
        for (int i = 0; i < 1; i++)
        {
            GameObject card = PhotonNetwork.Instantiate(PrefabsCard.name, PointSpawnEnemy[i].transform.position, Quaternion.identity);
            ConfigureCard(card, PointSpawnEnemy[i].transform, true); // Configura la carta per il nemico
        }
    }

    // Metodo per configurare una carta
    private void ConfigureCard(GameObject card, Transform parent, bool isEnemy)
    {
        Movement_Card movementCard = card.GetComponent<Movement_Card>();
        Card_Display cardDisplay = card.GetComponent<Card_Display>();

        if (movementCard != null && cardDisplay != null)
        {
            int randomIndex = Random.Range(0, CardList.Count);
            movementCard.SetCamera(MainCamera); // imposta la telecamera per il movimento
            movementCard.SetPositionCard(); // imposta la posizione della carta
            movementCard.SetObject(CardManager); // collega il cardManager alla carta

            cardDisplay.IsEnemy = isEnemy; // indica se la carta è per il nemico
            cardDisplay.Card_Info = CardList[randomIndex]; // assegna informazioni sulla carta in modo casuale
        }

        // dopo la configurazione, imposta il genitore della carta
        card.transform.SetParent(parent, false);
        spawnedCards.Add(card); // Aggiunge la carta alla lista delle carte spawnate
    }

    // Metodo che si attiva quando un nuovo giocatore entra nella stanza
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SyncCardsWithNewPlayer(newPlayer); // sincronizza le carte con il nuovo giocatore
            photonView.RPC("SyncEnvironment", newPlayer, MainCamera.GetComponent<PhotonView>().ViewID, CardManager.GetComponent<PhotonView>().ViewID);
        }
    }

    // Metodo per sincronizzare le carte con il nuovo giocatore
    private void SyncCardsWithNewPlayer(Player newPlayer)
    {
        foreach (GameObject card in spawnedCards)
        {
            Card_Display cardDisplay = card.GetComponent<Card_Display>();
            Vector3 position = card.transform.position;
            int cardId = cardDisplay.Card_Info.cardId;
            bool isEnemy = cardDisplay.IsEnemy;

            // sincronizza ogni carta con il nuovo giocatore
            photonView.RPC("SyncCard", newPlayer, position, cardId, isEnemy, card.GetComponent<PhotonView>().ViewID);
        }
    }

    // RPC per sincronizzare una carta su tutti i client
    [PunRPC]
    private void SyncCard(Vector3 position, int cardId, bool isEnemy, int viewID)
    {
        GameObject card = PhotonView.Find(viewID).gameObject;
        if (card == null)
        {
            Debug.LogError("Card not found with ViewID: " + viewID);
            return;
        }

        Card_Display cardDisplay = card.GetComponent<Card_Display>();
        Movement_Card movementCard = card.GetComponent<Movement_Card>();

        // Configura la carta per il nuovo client
        cardDisplay.IsEnemy = isEnemy;
        cardDisplay.Card_Info = CardList.Find(c => c.cardId == cardId);

        Transform parent = isEnemy ? PointSpawnEnemy[0].transform : PointSpawnPlayer[0].transform;

        if (movementCard != null)
        {
            movementCard.SetCamera(MainCamera);
            movementCard.SetObject(CardManager);
            movementCard.SetPositionCard();
        }

        // Imposta il genitore della carta
        card.transform.SetParent(parent, false);

        spawnedCards.Add(card); // Aggiungiamo la carta alla lista delle carte spawnate
    }

    // RPC per sincronizzare telecamera e CardManager
    [PunRPC]
    private void SyncEnvironment(int cameraViewID, int cardManagerViewID)
    {
        MainCamera = PhotonView.Find(cameraViewID).GetComponent<Camera>();
        CardManager = PhotonView.Find(cardManagerViewID).GetComponent<CardManager>();
    }
}
