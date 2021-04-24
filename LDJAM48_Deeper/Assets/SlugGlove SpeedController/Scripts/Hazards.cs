using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazards : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        //check if player
        PlayerMovement Ply = collision.gameObject.GetComponent<PlayerMovement>();
        if (!Ply)
            return;

        Ply.Damage();
    }
}
