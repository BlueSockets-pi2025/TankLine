using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] float remTime;
    [SerializeField] GameObject xPanel; 

    void Start()
    {
        timerText.text = "00:00";
    }

    void Update()
    {
        if (xPanel.activeSelf)
        {
            remTime -= Time.deltaTime;

            if (remTime < 6 && remTime > 0)
            {
                timerText.color = new Color(1f, 0.65f, 0f);
            }

            if (remTime < 1)
            {
                timerText.color = Color.red;
            }
            if (remTime < 0)
            {
                remTime=0;
                timerText.color = Color.red;
            }

            if (remTime >= 10)
            {
                int min = Mathf.FloorToInt(remTime / 60);
                int sec = Mathf.FloorToInt(remTime % 60);
                timerText.text = string.Format("{0:00}:{1:00}", min, sec);
            }
            else
            {
                int sec = Mathf.FloorToInt(remTime);
                int ms = Mathf.FloorToInt((remTime - sec) * 100);
                timerText.text = string.Format("{0:0},{1:00}", sec, ms);
            }
        }
    }
}
