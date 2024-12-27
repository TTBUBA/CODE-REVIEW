using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

public class Movement_Card : MonoBehaviourPunCallbacks, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Camera Camera;
    public Vector3 LastPosition;  // Ultima posizione della carta
    public bool CardRelase;  // Indica se la carta è stata rilasciata
    public Card_Info CardSelection;
    [SerializeField] private Card_Display card_Display;  // Componente per visualizzare la carta
    [SerializeField] private CardManager cardManager;  // Gestore delle carte
    private PhotonView photonView;
    private RectTransform rectTransform;
    private bool isDragging = false;

    // Metodo chiamato all'inizio per inizializzare i componenti
    public void Start()
    {
        photonView = GetComponent<PhotonView>();
        rectTransform = GetComponent<RectTransform>();
    }

    // Metodo per impostare il gestore delle carte
    public void SetObject(CardManager CardManager)
    {
        card_Display = GetComponent<Card_Display>();
        cardManager = CardManager;
    }

    // Metodo per impostare la posizione iniziale della carta
    public void SetPositionCard()
    {
        rectTransform = GetComponent<RectTransform>();
        Vector3 worldPosition = rectTransform.TransformPoint(Vector3.zero);
        worldPosition.z = 0;
        LastPosition = worldPosition;
    }

    // Metodo per impostare la telecamera
    public void SetCamera(Camera camera)
    {
        Camera = camera;
    }

    // Metodo chiamato quando si clicca sulla carta
    public void OnPointerClick(PointerEventData eventData)
    {
        // Controlla se la carta appartiene al giocatore locale
        if (!photonView.IsMine) return;

        if (card_Display.IsEnemy)  // Se la carta appartiene al nemico
        {
            cardManager.cardSelectEnemy = card_Display.Card_Info;
        }
        else  // Se la carta appartiene al giocatore
        {
            cardManager.cardSelectPlayer = card_Display.Card_Info;
        }
    }

    // Metodo chiamato durante il trascinamento della carta
    public void OnDrag(PointerEventData eventData)
    {
        // Controlla se la carta appartiene al giocatore locale
        if (!photonView.IsMine) return;

        isDragging = true;
        Vector3 posMouse = Camera.ScreenToWorldPoint(Input.mousePosition);
        posMouse.z = 0;

        // Aggiorna la posizione localmente
        transform.position = posMouse;

        // Sincronizza la posizione con altri client
        photonView.RPC("SyncPosition", RpcTarget.All, posMouse);
    }

    // Metodo chiamato quando si rilascia la carta
    public void OnEndDrag(PointerEventData eventData)
    {
        // Controlla se la carta appartiene al giocatore locale
        if (!photonView.IsMine) return;

        isDragging = false;

        // Se le luci del nemico o del giocatore sono spente, ripristina la posizione della carta
        if (card_Display.IsEnemy && cardManager.allLightsOffEnemy || !card_Display.IsEnemy && cardManager.allLightsOffPlayer)
        {
            ResetCardPosition();
            return;
        }

        Vector2 posMouse = Camera.ScreenToWorldPoint(Input.mousePosition);
        // Prova a posizionare la carta, se fallisce ripristina la posizione originale
        if (tryPositionCard(posMouse, card_Display.IsEnemy ? "PlaceCardEnemy" : "PlaceCardPlayer"))
        {
            // Sincronizza la posizione finale e l'azione della carta
            photonView.RPC("SyncFinalPosition", RpcTarget.All, transform.position);
            photonView.RPC("SyncCardAction", RpcTarget.All);
        }
        else
        {
            ResetCardPosition();
            photonView.RPC("SyncPosition", RpcTarget.All, LastPosition);
        }
    }

    // Metodo per sincronizzare la posizione della carta
    [PunRPC]
    private void SyncPosition(Vector3 newPosition)
    {
        // Aggiorna la posizione solo se non è la carta del giocatore locale
        if (!photonView.IsMine)
        {
            transform.position = newPosition;
        }
    }

    // Metodo per sincronizzare la posizione finale della carta
    [PunRPC]
    private void SyncFinalPosition(Vector3 finalPosition)
    {
        transform.position = finalPosition;
        CardRelase = true;
    }

    // Metodo per sincronizzare l'azione della carta
    [PunRPC]
    private void SyncCardAction()
    {
        ExecuteCardAction();
    }

    // Metodo per provare a posizionare la carta
    private bool tryPositionCard(Vector2 position, string TargetType)
    {
        Collider2D hitcollider = Physics2D.OverlapPoint(position);
        if (hitcollider != null && hitcollider.CompareTag(TargetType))
        {
            transform.position = hitcollider.transform.position;
            CardRelase = true;
            return true;
        }
        return false;
    }

    // Metodo per eseguire l'azione della carta
    private void ExecuteCardAction()
    {
        if (!card_Display.IsEnemy)
        {
            cardManager.DescreseLightPlayer();
            cardManager.cardSelectPlayer.cardAction?.Execute(cardManager.cardSelectPlayer, cardManager);
        }
        else
        {
            cardManager.DescreseLightEnemy();
            cardManager.cardSelectEnemy.cardAction?.Execute(cardManager.cardSelectEnemy, cardManager);
        }

        if (cardManager.CardRelese >= 1)
        {
            cardManager.OnbothCardPlace();
        }
        cardManager.CardRelese++;
    }

    // Metodo per ripristinare la posizione della carta
    private void ResetCardPosition()
    {
        transform.position = LastPosition;
        photonView.RPC("SyncPosition", RpcTarget.All, LastPosition);
    }
}
