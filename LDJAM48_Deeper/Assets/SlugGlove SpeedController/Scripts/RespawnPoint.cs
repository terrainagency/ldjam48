using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    public Material ActiveMat;
    public Material PassiveMat;
    private MeshRenderer Msh;

    //get mesh renderer
    private void Start()
    {
        Msh = GetComponent<MeshRenderer>();
        SetOff();
    }
    private void OnTriggerEnter(Collider other)
    {
        //check if the player has collided
        PlayerMovement Ply = other.GetComponent<PlayerMovement>();

        if (!Ply)
            return;

        //if so set active and set the players respawn point
        Ply.CheckPointSet(this);
        SetOn();
    }

    //set the active material off
    public void SetOff()
    {
        Msh.material = PassiveMat;
    }
    //set the active material on
    public void SetOn()
    {
        Msh.material = ActiveMat;
    }
}
