//-----------------------------------------------------------------------
// File: BlindsAnimator.cs
// Author: Moriyoshi Rempola
// Application: Lab Homes AR
// Programming Language: C#
// Course: Computer Science 423
// Semester: Spring 2019
// Team: Night Owls
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

public class BlindsAnimator : MonoBehaviour
{
    private Slider slider; // Assign the UI slider from the scene to this
    private Animator anim;

    // Start is called on the first frame. It is used here to get the slider object.
    void Start()
    {
        slider = GameObject.Find("Slider").GetComponent<Slider>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame. It is used here to move the blinds up and down based on the slider.
    public void Update()
    {
        switch ((int)slider.value)
        {
            case 1:
                // close
                anim.SetFloat("speedMult", 1.0f);
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Blinds Up"))
                {
                    anim.SetFloat("speedMult", -1.0f);
                }
                anim.SetBool("down", true);
                anim.SetBool("up", false);
                break;
            case 2:
                // close
                anim.SetFloat("speedMult", 1.0f);
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Blinds Up"))
                {
                    anim.SetFloat("speedMult", -1.0f);
                }
                anim.SetBool("down", true);
                anim.SetBool("up", false);
                break;
            case 3:
                // close
                anim.SetFloat("speedMult", 1.0f);
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Blinds Up"))
                {
                    anim.SetFloat("speedMult", -1.0f);
                }
                anim.SetBool("down", true);
                anim.SetBool("up", false);
                break;
            case 4:
                // close
                anim.SetFloat("speedMult", 1.0f);
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Blinds Up"))
                {
                    anim.SetFloat("speedMult", -1.0f);
                }
                anim.SetBool("down", true);
                anim.SetBool("up", false);
                break;
            case 5:
                // open
                anim.SetFloat("speedMult", 1.0f);
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Blinds Down"))
                {
                    anim.SetFloat("speedMult", -1.0f);
                }
                anim.SetBool("up", true);
                anim.SetBool("down", false);
                break;
            case 6:
                // open
                anim.SetFloat("speedMult", 1.0f);
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Blinds Down"))
                {
                    anim.SetFloat("speedMult", -1.0f);
                }
                anim.SetBool("up", true);
                anim.SetBool("down", false);
                break;
            case 7:
                // open
                anim.SetFloat("speedMult", 1.0f);
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Blinds Down"))
                {
                    anim.SetFloat("speedMult", -1.0f);
                }
                anim.SetBool("up", true);
                anim.SetBool("down", false);
                break;
            case 8:
                // open
                anim.SetFloat("speedMult", 1.0f);
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Blinds Down"))
                {
                    anim.SetFloat("speedMult", -1.0f);
                }
                anim.SetBool("up", true);
                anim.SetBool("down", false);
                break;
            case 9:
                // open
                anim.SetFloat("speedMult", 1.0f);
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Blinds Down"))
                {
                    anim.SetFloat("speedMult", -1.0f);
                }
                anim.SetBool("up", true);
                anim.SetBool("down", false);
                break;
            case 10:
                // open
                anim.SetFloat("speedMult", 1.0f);
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Blinds Down"))
                {
                    anim.SetFloat("speedMult", -1.0f);
                }
                anim.SetBool("up", true);
                anim.SetBool("down", false);
                break;
            case 11:
                // close
                anim.SetFloat("speedMult", 1.0f);
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Blinds Up"))
                {
                    anim.SetFloat("speedMult", -1.0f);
                }
                anim.SetBool("down", true);
                anim.SetBool("up", false);
                break;
            case 12:
                // close
                anim.SetFloat("speedMult", 1.0f);
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Blinds Up"))
                {
                    anim.SetFloat("speedMult", -1.0f);
                }
                anim.SetBool("down", true);
                anim.SetBool("up", false);
                break;
            default:
                break;
        }
    }
}
