using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupProximityGoal : TrialGoal
{
    //public int robotIndex = 0;
    public double radius = 1;

    public GameObject[] fufiller;
    public int numRequired = 5;
    public int numCurrent = 0;
    
    
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
        //robot = TaskEnvironment.instances[TaskEnvironment.currentIndex].getObjectListByKey("robots")[robotIndex];
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf)
        {
            foreach (GameObject obj in fufiller)
            {
                double distance = Vector3.Distance(obj.transform.position, transform.position);
                if (distance < radius)
                {
                    numCurrent++;
                    if(numCurrent >= numRequired)
                    {
                        Complete();
                        gameObject.SetActive(false);
                        break;
                    }
                }
            }
        }
    }

    public override void Activate()
    {
        gameObject.SetActive(true);
    }
}
