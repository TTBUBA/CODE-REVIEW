using Photon.Pun;
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

    public void Start()
    {
        Invoke("SpawnCards", 0.3f); // Delay per assicurarsi che tutti i client siano sincronizzati
    }

    void SpawnCards()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnCard(true);  // Carte del master
        }
        else
        {
            SpawnCard(false); // Carte dell'altro client
        }
    }

    public void SpawnCard(bool isPlayer)
    {
        for (int i = 0; i < 6; i++)
        {
            Vector3 spawnPosition = isPlayer ? PointSpawnPlayer[i].transform.position : PointSpawnEnemy[i].transform.position;

            //  crea la carta
            GameObject card = PhotonNetwork.Instantiate(PrefabsCard.name, spawnPosition, Quaternion.identity);

            // Calcola il finalIsPlayer per il MasterClient
            bool finalIsPlayer = PhotonNetwork.IsMasterClient ? isPlayer : !isPlayer;

            // Sincronizza la configurazione tramite RPC
            photonView.RPC("ConfigureCardRPC", RpcTarget.All, card.GetComponent<PhotonView>().ViewID, finalIsPlayer, i);
        }
    }

    [PunRPC]
    public void ConfigureCardRPC(int viewID, bool isPlayer, int index)
    {
        PhotonView cardPhotonView = PhotonView.Find(viewID);
        if (cardPhotonView != null)
        {
            GameObject card = cardPhotonView.gameObject;

            // Imposta il genitore corretto basandosi su isPlayer
            Transform parent = isPlayer ? PointSpawnPlayer[index].transform : PointSpawnEnemy[index].transform;
            card.transform.SetParent(parent, false);
            card.transform.localPosition = Vector3.zero;
            Debug.Log(parent);

            // Configura i componenti della carta
            Movement_Card movementCard = card.GetComponent<Movement_Card>();
            Card_Display cardDisplay = card.GetComponent<Card_Display>();
     
            Random.InitState((int)PhotonNetwork.Time + index);
            int randomIndex = Random.Range(0, CardList.Count);
            if (movementCard != null && cardDisplay != null)
            {
                movementCard.SetCamera(MainCamera);
                movementCard.SetPositionCard();
                movementCard.SetObject(CardManager);
                cardDisplay.IsEnemy = !isPlayer;
                cardDisplay.Card_Info = CardList[randomIndex];
            }
        }
    }

}
