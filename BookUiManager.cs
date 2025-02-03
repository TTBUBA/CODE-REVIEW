using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookUiManager : MonoBehaviour
{
    [SerializeField] private GameObject Book;
    [SerializeField] private TextMeshProUGUI TextButton;
    [SerializeField] private TextMeshProUGUI ContainerPaper;

    private bool IsOpen = false;
    private void Update()
    {
        OpenBook();
        CloseBook();
    }
    public void OpenBook()
    {
        if (Input.GetKeyDown(KeyCode.F) && !IsOpen)
        {
            string ContainerBook = BookController.Instance.BookData.NameBook;
            Book.SetActive(true);
            TextButton.text = "Close Q";
            ContainerPaper.text = ContainerBook.ToString();
            IsOpen = true;
        }
    }

    public void CloseBook()
    {
        if(Input.GetKeyDown(KeyCode.Q) && IsOpen)
        {
            Book.SetActive(false);
            TextButton.text = "Open F";
            IsOpen = false;
        }
    }

    private void NextPage()
    {
    }

    private void PreviusPage()
    {
    }
}
