using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinCtrl : MonoBehaviour
{
    public GameObject CollectFx;

    private void OnTriggerEnter(Collider other)
    {
        //check for player
        PlayerStats Stats = other.GetComponent<PlayerStats>();

        if (!Stats)
            return;

        //collect this coin
        Stats.CollectCoin(1);

        //create collection fx
        if(CollectFx)
        {
            //activate effect
            CollectFx.SetActive(true);
            //detatch collection
            CollectFx.transform.parent = null;
        }

        //destroy this coin
        Destroy(this.gameObject);
    }
}
