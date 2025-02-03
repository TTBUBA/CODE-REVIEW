using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookData : MonoBehaviour
{
    static public BookData Instance { get; set; }
    public string NameBook;

    private void Start()
    {
        Instance = this;
    }
}
