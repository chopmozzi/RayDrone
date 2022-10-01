using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneSetting : MonoBehaviour
{
    public GameObject DroneAgent;
    public GameObject Goal;

    private Vector3 areaInitPos;
    private Vector3 droneInitPos;
    private Vector3 goalInitPos;
    private Quaternion droneInitRot;

    private Transform AreaTrans;
    private Transform DroneTrans;
    private Transform GoalTrans;

    private Rigidbody DroneAgent_Rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        AreaTrans = gameObject.transform;
        DroneTrans = DroneAgent.transform;
        GoalTrans = Goal.transform;

        areaInitPos = AreaTrans.position;
        droneInitPos = DroneTrans.position;
        droneInitRot = DroneTrans.rotation;
        goalInitPos = GoalTrans.position;
        DroneAgent_Rigidbody = DroneAgent.GetComponent<Rigidbody>();
    }

    public void AreaSetting()
    {
        DroneAgent_Rigidbody.velocity = Vector3.zero;
        DroneAgent_Rigidbody.angularVelocity = Vector3.zero;

        DroneTrans.position = droneInitPos;
        DroneTrans.rotation = droneInitRot;

        GoalTrans.position = goalInitPos;
    }
}
