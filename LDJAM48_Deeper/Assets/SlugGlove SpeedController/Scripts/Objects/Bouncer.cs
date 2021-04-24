using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncer : MonoBehaviour
{
    public GameObject BounceFx;
    public float Force;

    private void OnCollisionEnter(Collision collision)
    {
        //check if player
        PlayerMovement Ply = collision.gameObject.GetComponent<PlayerMovement>();
        if (!Ply)
            return;

        Vector3 Direction = (Ply.transform.position - transform.position).normalized;
        Ply.BounceForce(Force, Direction);

        //activate visual effect, and detatch
        if (BounceFx)
        {
            GameObject Obj = Instantiate(BounceFx, transform.position, Quaternion.identity);
            Obj.SetActive(true);
            Obj.transform.parent = null;
        }
    }
}
