using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

public class Movement_Card : MonoBehaviourPunCallbacks, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Camera Camera;
    public Vector3 LastPosition;
    public bool CardRelease;
    public Card_Info CardSelection;
    [SerializeField] private Card_Display card_Display;
    [SerializeField] private CardManager cardManager;
    private PhotonView photonView;
    private RectTransform rectTransform;
    private bool isDragging = false;

    public void Start()
    {
        photonView = GetComponent<PhotonView>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetObject(CardManager cardManager)
    {
        card_Display = GetComponent<Card_Display>();
        this.cardManager = cardManager;
    }

    public void SetPositionCard()
    {
        rectTransform = GetComponent<RectTransform>();
        Vector3 worldPosition = rectTransform.TransformPoint(Vector3.zero);
        worldPosition.z = 0;
        LastPosition = worldPosition;
    }

    public void SetCamera(Camera camera)
    {
        Camera = camera;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!photonView.IsMine) return;

        if (card_Display.IsEnemy)
        {
            cardManager.cardSelectEnemy = card_Display.Card_Info;
        }
        else
        {
            cardManager.cardSelectPlayer = card_Display.Card_Info;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!photonView.IsMine) return;

        isDragging = true;
        Vector3 posMouse = Camera.ScreenToWorldPoint(Input.mousePosition);
        posMouse.z = 0;

        // Update local position
        transform.position = posMouse;

        // Sync the position across all clients
        photonView.RPC("SyncPosition", RpcTarget.All, posMouse);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!photonView.IsMine) return;

        isDragging = false;

        // Check if the card should be placed or reset
        if (card_Display.IsEnemy && cardManager.allLightsOffEnemy ||
            !card_Display.IsEnemy && cardManager.allLightsOffPlayer)
        {
            ResetCardPosition();
            return;
        }

        Vector2 posMouse = Camera.ScreenToWorldPoint(Input.mousePosition);
        if (TryPositionCard(posMouse, card_Display.IsEnemy ? "PlaceCardEnemy" : "PlaceCardPlayer"))
        {
            // Sync final position and card action across all clients
            photonView.RPC("SyncFinalPosition", RpcTarget.All, transform.position);
            photonView.RPC("SyncCardAction", RpcTarget.All);
        }
        else
        {
            ResetCardPosition();
            photonView.RPC("SyncPosition", RpcTarget.All, LastPosition);
        }
    }

    [PunRPC]
    private void SyncPosition(Vector3 newPosition)
    {
        if (!photonView.IsMine)
        {
            transform.position = newPosition;
        }
    }

    [PunRPC]
    private void SyncFinalPosition(Vector3 finalPosition)
    {
        transform.position = finalPosition;
        CardRelease = true;
    }

    [PunRPC]
    private void SyncCardAction()
    {
        if (!photonView.IsMine)
        {
            ExecuteCardAction();
        }
    }

    private bool TryPositionCard(Vector2 position, string targetType)
    {
        Collider2D hitCollider = Physics2D.OverlapPoint(position);
        if (hitCollider != null && hitCollider.CompareTag(targetType))
        {
            transform.position = hitCollider.transform.position;
            CardRelease = true;
            return true;
        }
        return false;
    }

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

    private void ResetCardPosition()
    {
        transform.position = LastPosition;
        photonView.RPC("SyncPosition", RpcTarget.All, LastPosition);
    }
}
