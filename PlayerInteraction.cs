using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Vehicles")]
    [SerializeField] private GameObject ButtonEnter;
    [SerializeField] private TextMeshProUGUI ButtonText;
    [SerializeField] private GameObject Player;
    [SerializeField] private GameObject Camera;
    [SerializeField] private Transform PoinSetPlayer;
    [SerializeField] private Transform PoinExitPlayer;
    [SerializeField] private bool CarCollision;


    // Update is called once per frame
    void Update()
    {
        CheckForVehicle();
        EnterVehicle();
        ExitVehicle();
        PlayerFollowVehicle();
    }

    private void CheckForVehicle()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 2))
        {
            if (hit.collider.CompareTag("Vehicle"))
            {
                ButtonEnter.SetActive(true);
                CarCollision = true;
            }
            else
            {
                ButtonEnter.SetActive(false);
                CarCollision = false;
            }
        }
        else
        {
            //ButtonEnter.SetActive(false);
            CarCollision = false;
        }
    }
    
    private void EnterVehicle()
    {
        if (Input.GetKeyUp(KeyCode.E) && CarCollision)
        {
            CharacterController Controller = Player.GetComponent<CharacterController>();

            CarController.Instance.isPlayerInside = true;
            Player_Movement.Instance.ActiveMovement = false;
            Controller.enabled = false;
            Player.transform.position = PoinSetPlayer.transform.position;
            ButtonText.text = "Exit Esc";
            CarCollision = false;
            ButtonEnter.SetActive(true);
        }
    }

    private void ExitVehicle()
    {
        if (Input.GetKeyUp(KeyCode.Escape) && !CarCollision)
        {
            CharacterController Controller = Player.GetComponent<CharacterController>();

            CarController.Instance.isPlayerInside = false;
            Player_Movement.Instance.ActiveMovement = true;
            Player.transform.position = PoinExitPlayer.transform.position;
            Controller.enabled = true;
            ButtonEnter.SetActive(false);
            ButtonText.text = "Enter E";
        }
    }

    private void PlayerFollowVehicle()
    {
        if (CarController.Instance.isPlayerInside)
        {
            Player.transform.position = PoinSetPlayer.transform.position;
        }
    }
}
