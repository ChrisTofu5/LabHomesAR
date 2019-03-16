using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonthTextController : MonoBehaviour
{
    Text monthText;
    string[] monthArr = {"January","Febuary","March","April",
                        "May","June","July","August","September",
                        "October","November","December" };
    // Start is called before the first frame update
    void Start()
    {
        monthText = GetComponent<Text>();
    }

    // Update is called once per frame
    public void textUpdate(float value)
    {
        monthText.text = monthArr[(int)value - 1];
    }
}
