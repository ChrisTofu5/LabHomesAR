//-----------------------------------------------------------------------
// File: MonthTextController.cs
// Author: Moriyoshi Rempola
// Application: Lab Homes AR
// Programming Language: C#
// Course: Computer Science 423
// Semester: Spring 2019
// Team: Night Owls
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

public class MonthTextController : MonoBehaviour
{
    private Text monthText;
    private string[] monthArr = {"January","February","March","April",
                                 "May","June","July","August","September",
                                 "October","November","December"};

    // Start is called on the first frame. It is used here to get the text object.
    void Start()
    {
        monthText = GetComponent<Text>();
    }

    // Update is called once per frame. It is used here to set the text based on the slider.
    public void textUpdate(float value)
    {
        monthText.text = monthArr[(int)value - 1];
    }
}
