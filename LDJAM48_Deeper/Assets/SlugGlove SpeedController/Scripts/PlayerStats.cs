using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private UICtrl Controller;
    private Rigidbody Rigid;
    private float LevelTimer;
    private int Score;

    private void Start()
    {
        Controller = GameObject.FindGameObjectWithTag("UI").GetComponent<UICtrl>();
        Rigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //tick timer
        LevelTimer += Time.deltaTime;
        Controller.SetTime(LevelTimer);

        //get acceleration 
        float Acel = Rigid.velocity.magnitude;
        Controller.SetAcceleration(Mathf.RoundToInt(Acel));
    }

    public void CollectCoin(int Amt)
    {
        Score += Amt;
        Controller.SetScore(Score * 10);
    }
}
