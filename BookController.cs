using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BookController : MonoBehaviour
{
    static public BookController Instance { get; set; }
    [SerializeField] private GameObject Button_OpenBook;
    [SerializeField] public BookData BookData; // current bookdata
    Ray Ray;
    RaycastHit Hit;
    private void Awake()
    {
        Instance = this;
    }
    // Update is called once per frame
    void Update()
    {
        // Get mouse position
        Vector3 MousePos = Input.mousePosition;
        Ray = Camera.main.ScreenPointToRay(MousePos);// Create ray from camera to mouse position

        if (Physics.Raycast(Ray, out Hit, 5f))
        {
            if (Hit.collider.gameObject)
            {
                Button_OpenBook.SetActive(true);
                BookData = Hit.collider.gameObject.GetComponent<BookData>();
            }
        }
        else
        {
            Button_OpenBook.SetActive(false);
        }
    }
}
