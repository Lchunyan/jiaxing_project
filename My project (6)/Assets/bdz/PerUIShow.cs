using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerUIShow : MonoBehaviour
{
    void Start()
    {
        Text text = transform.Find("Text").GetComponent<Text>();
        text.text = transform.parent.name;
        transform.gameObject.SetActive(false);
    }
}
