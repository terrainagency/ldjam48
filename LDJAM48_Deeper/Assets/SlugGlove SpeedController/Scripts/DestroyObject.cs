using UnityEngine;
using System.Collections;

//basic utility class to destroy objects on startup
public class DestroyObject : MonoBehaviour 
{
    [Header("Destruction")]
	public float delay;				//delay before object is destroyed
	public bool destroyChildren;	//should the children be detached (and kept alive) before object is destroyed?

    public bool DetatchFromParent;

    [Header("Shrinking")]
    public bool shrink;
    public float ShrinkSpeed = 4f;

    public Vector3 startScale = new Vector3(1, 1, 1);

    private float timer = 0;


    [Header("Effects")]
    public GameObject CreateOnDeath;
    public GameObject DetatchOnDeath;

    void Start()
	{
        //detatch from parent
        if (DetatchFromParent)
            transform.parent = null;

		//detach children
		if (!destroyChildren)
			transform.DetachChildren();	
	}

    void FixedUpdate()
    {
        if (delay > 0)
        {
            delay -= Time.deltaTime;
        }
        else
        {
            timer += Time.deltaTime * ShrinkSpeed;

            if(shrink)
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, timer);

            if (timer > 1)
            {
                if(DetatchOnDeath)
                {
                    DetatchOnDeath.SetActive(true);
                    DetatchOnDeath.transform.parent = null;
                }
                if (CreateOnDeath)
                    Instantiate(CreateOnDeath, transform.position, Quaternion.identity);

                Destroy(gameObject);
            }
        }
    }
}