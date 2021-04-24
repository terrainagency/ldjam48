using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableTile : MonoBehaviour
{
    public GameObject DestroyFX;

    private void OnCollisionEnter(Collision collision)
    {
        //check if player
        PlayerMovement Ply = collision.gameObject.GetComponent<PlayerMovement>();
        if (!Ply)
            return;

        //activate visual effect, and detatch
        if(DestroyFX)
        {
            DestroyFX.SetActive(true);
            DestroyFX.transform.parent = null;
        }

        //destroy self
        DestroyObject Dsty = GetComponent<DestroyObject>();

        if (!Dsty)
            Destroy(this.gameObject);
        else
        {
            Dsty.enabled = true;
            Destroy(this);
        }
    }
}
