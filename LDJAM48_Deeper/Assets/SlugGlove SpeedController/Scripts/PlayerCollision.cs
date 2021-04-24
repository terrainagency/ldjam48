using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    public float FloorSize; //how large the floor check is
    public float FloorDis; //how large the detection for the floors is
    public float bottomOffset; //offset from player centre
    public float WallCheckDistance; //how large the detection for the walls is
    public float UpOffset; //offset from the players centre 

    public float LedgeGrabForwardPos; //the position in front of the player where we check for ledges
    public float LedgeGrabUpwardsPos;//the position in above of the player where we check for ledges
    public float LedgeGrabDistance; //the distance the ledge can be from our raycast before we grab it (this is projects from the top of the wall grab position, downwards

    public LayerMask FloorLayers; //what layers we can stand on
    public LayerMask WallLayers;  //what layers we can wall run on
    public LayerMask LedgeGrabLayers; //what layers we will grab onto

    private float ColRadius;

    private void Start()
    {
        SphereCollider Col = GetComponent<SphereCollider>();

        if(!Col)
        {
            CapsuleCollider Cap = GetComponent<CapsuleCollider>();

            if (!Cap)
                ColRadius = 0.5f;
            else
                ColRadius = Cap.radius;
        }
        else
        {
            ColRadius = Col.radius;
        }
    }

    //check if there is floor below us
    public Vector3 CheckFloor(Vector3 Direction)
    {
        Vector3 Pos = transform.position + (Direction * bottomOffset);
        Collider[] hitColliders = Physics.OverlapSphere(Pos, FloorSize, FloorLayers);
        if (hitColliders.Length > 0)
        {
            //we are on the ground get an angle
            RaycastHit hit;
            if (Physics.Raycast(transform.position + (-transform.up * (bottomOffset - 0.3f)), Direction, out hit, FloorDis, FloorLayers))
            {
                //we are on the ground
                return hit.normal;
            }

            //we hit the floor but missed a check, return up
            return Vector3.up;
        }

        return Vector3.zero;
    }

    //check if there is a wall in the direction we are pressing
    public Vector3 CheckWall(Vector3 Direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + (transform.up * UpOffset), Direction, out hit, WallCheckDistance, WallLayers))
        {
            //we are on the ground
            return hit.normal;
        }

        return Vector3.zero;
    }

    public Vector3 CheckLedges(Vector3 ForwardDir, Vector3 UpwardsDir)
    {
        Vector3 RayPos = transform.position + (ForwardDir * LedgeGrabForwardPos) + (UpwardsDir * LedgeGrabUpwardsPos);

        RaycastHit hit;
        if (Physics.Raycast(RayPos, -transform.up, out hit, LedgeGrabDistance, LedgeGrabLayers))
        {
            Vector3 Pos = hit.point + (Vector3.up * 0.5f);
            Collider[] hitColliders = Physics.OverlapSphere(Pos, ColRadius - 0.003f, FloorLayers);
            if (hitColliders.Length <= 0) //no collision so we can pullup
            {
                //we can pull up
                return hit.point;
            }

            return Vector3.zero;
        }


        return Vector3.zero;
    }

    void OnDrawGizmosSelected()
    {
        //floor check
        Gizmos.color = Color.red;
        Vector3 Pos = transform.position + (-transform.up * bottomOffset);
        Gizmos.DrawLine(transform.position, Pos + (-transform.up * FloorDis));
        //floor check
        Gizmos.color = Color.red;
        Pos = transform.position + (-transform.up * bottomOffset);
        Gizmos.DrawSphere(Pos,FloorSize);
        //wall check
        Gizmos.color = Color.green;
        Vector3 Pos2 = transform.position + (transform.up * UpOffset);
        Gizmos.DrawLine(Pos2, Pos2 + (transform.forward * WallCheckDistance));
        //Ledge check
        Gizmos.color = Color.blue;
        Vector3 Pos4 = transform.position + (transform.forward * LedgeGrabForwardPos) + (transform.up * LedgeGrabUpwardsPos);
        Gizmos.DrawLine(Pos4, Pos4 + (-transform.up * LedgeGrabDistance));
    }
}
