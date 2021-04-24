using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICtrl : MonoBehaviour
{
    [Header("Accel")]
    public Text AccelerationTxt;
    [Header("Time")]
    public Text TimeTxt;
    [Header("Score")]
    public Text ScoreTxt;

    public void SetAcceleration(float Amt)
    {
        if (!AccelerationTxt)
            return;

        AccelerationTxt.text = Amt.ToString();
    }

    public void SetTime(float Time)
    {
        if (!TimeTxt)
            return;

        float Minutes = Mathf.FloorToInt(Time / 60);
        float Seconds = Mathf.FloorToInt(Time % 60);
        float MilSeconds = Mathf.FloorToInt(Time * 1000f) % 1000;
        string Final = string.Format("{0:00}'{1:00}'{2:00}", Minutes, Seconds, MilSeconds);

        TimeTxt.text = Final;
    }

    public void SetScore(int Amt)
    {
        if (!ScoreTxt)
            return;

        ScoreTxt.text = Amt.ToString();
    }
}
