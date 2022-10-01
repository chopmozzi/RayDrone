using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using PA_DronePack;
using UnityEngine;


public class DroneAgent : Agent
{
    private PA_DroneController dcoScript;
    public DroneSetting area;
    public GameObject goal;
    public RayPerceptionSensorComponent3D m_rayPerceptionSensorcomponent3D;
    float preDist;
    List<float> list = new List<float>();
    List<float> check = new List<float>();
    private Transform agentTrans;
    private Transform goalTrans;

    private Rigidbody agent_Rigidbody;



    public override void Initialize()
    {
        base.Initialize();
        m_rayPerceptionSensorcomponent3D = GetComponent<RayPerceptionSensorComponent3D>();
        dcoScript = gameObject.GetComponent<PA_DroneController>();

        agentTrans = gameObject.transform;
        goalTrans = goal.transform;

        agent_Rigidbody = gameObject.GetComponent<Rigidbody>();

        Academy.Instance.AgentPreStep += WaitTimeInference;
    }

    private void RayCastInfo(RayPerceptionSensorComponent3D rayComponent)
    {
        var rayOutputs = RayPerceptionSensor.Perceive(rayComponent.GetRayPerceptionInput()).RayOutputs;

        if (rayOutputs != null)
        {
            var lengthOfRayOutputs = RayPerceptionSensor.Perceive(rayComponent.GetRayPerceptionInput()).RayOutputs.Length;

            for (int i = 0; i < lengthOfRayOutputs; i++)
            {
                GameObject goHit = rayOutputs[i].HitGameObject;
                if (goHit != null)
                {
                    var rayDirection = rayOutputs[i].EndPositionWorld - rayOutputs[i].StartPositionWorld;
                    var scaledRayLength = rayDirection.magnitude;
                    float rayHitDistance = rayOutputs[i].HitFraction * scaledRayLength;

                    string dispStr;
                    dispStr = "__RayPerceptionSensor - HitInfo__:\r\n";
                    dispStr = dispStr + "GameObject name: " + goHit.name + "\r\n";
                    dispStr = dispStr + "GameObject tag: " + goHit.tag + "\r\n";
                    dispStr = dispStr + "Hit distance of Ray: " + rayHitDistance + "\r\n";
                    Debug.Log(dispStr);
                }
            }
        }
    }

    private GameObject RayHit(RayPerceptionSensorComponent3D rayComponent)
    {
        //GameObject goHit = null;
        var rayOutputs = RayPerceptionSensor.Perceive(rayComponent.GetRayPerceptionInput()).RayOutputs;
        if (rayOutputs != null)
        {
            var lengthOfRayOutputs = RayPerceptionSensor.Perceive(rayComponent.GetRayPerceptionInput()).RayOutputs.Length;

            for (int i = 0; i < lengthOfRayOutputs; i++)
            {
                GameObject goHit = rayOutputs[i].HitGameObject;
                if (goHit != null)
                {
                    var rayDirection = rayOutputs[i].EndPositionWorld - rayOutputs[i].StartPositionWorld;
                    var scaledRayLength = rayDirection.magnitude;
                    float rayHitDistance = rayOutputs[i].HitFraction * scaledRayLength;

                    string dispStr;
                    dispStr = "__RayPerceptionSensor - HitInfo__:\r\n";
                    dispStr = dispStr + "GameObject name: " + goHit.name + "\r\n";
                    dispStr = dispStr + "GameObject tag: " + goHit.tag + "\r\n";
                    dispStr = dispStr + "Hit distance of Ray: " + rayHitDistance + "\r\n";
                    Debug.Log(dispStr);
                    return goHit;
                }
            }
        }
        return null;
    }
    private float CalPredist()
    {
        float data = 50f;
        float distance = Vector3.Magnitude(goalTrans.position - agentTrans.position);
        list.Add(distance);
        if (list.Count > 1)
        {
            data = list[list.Count - 2] - list[list.Count - 1];
        }
        return data;
    }
    private float CheckDis()
    {
        float data = 0;
        float point = Vector3.Magnitude(agentTrans.position);
        check.Add(point);
        int num = check.Count;
        if(num>1)
        {
            data = check[num - 1] - check[num];
            if (data < 0)
                data *= -1;
        }
        return data;

    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(agentTrans.position - goalTrans.position);

        sensor.AddObservation(agent_Rigidbody.velocity);

        sensor.AddObservation(agent_Rigidbody.angularVelocity);
    }

    public override void OnActionReceived(ActionBuffers actionsBuffers)
    {
        AddReward(-0.01f);
        var actions = actionsBuffers.ContinuousActions;

        float moveX = Mathf.Clamp(actions[0], -1, 1f);
        float moveY = Mathf.Clamp(actions[1], -1, 1f);
        float moveZ = Mathf.Clamp(actions[2], -1, 1f);

        dcoScript.DriveInput(moveX);
        dcoScript.StrafeInput(moveY);
        dcoScript.LiftInput(moveZ);
        //Debug.DrawRay(agentTrans.position + Vector3.up, Vector3.forward * 10, Color.red);
        float distance = Vector3.Magnitude(goalTrans.position - agentTrans.position);
        /*GameObject test = RayHit(m_rayPerceptionSensorcomponent3D);
        if(test!=null)
            Debug.Log(test.name);*/
        Debug.Log(distance);
        /*if (RayHit(m_rayPerceptionSensorcomponent3D) != null)
        {
            Debug.Log("?");
            //if(RayHit(m_rayPerceptionSensorcomponent3D).CompareTag("DEAD_ZONE"))
            //EndEpisode();
            SetReward(-1f);
            EndEpisode();
        }
        else
        {
            Debug.Log("NULL");
        }*/
        //Debug.Log("Âý¸ðÂî : " + CalPredist());
        /*if(CheckDis()<1f)
        {
            AddReward(-1f);
        }
        if (agentTrans.position.y > 8)
        {
            SetReward(-1f);
            EndEpisode();
        }*/
        if (distance <= 1f)
        {
            SetReward(1f);
            EndEpisode();
        }
        else if (distance > 30f)
        {
            SetReward(-1f);
            EndEpisode();
        }
        else
        {
            float reward = preDist - distance;
            AddReward(reward);
            preDist = distance;
        }
    }

    public override void OnEpisodeBegin()
    {
        area.AreaSetting();
        preDist = Vector3.Magnitude(goalTrans.position - agentTrans.position);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
        continuousActionsOut[2] = Input.GetAxis("Mouse ScrollWheel");

        //Debug.DrawRay(agentTrans.position + Vector3.up, Vector3.forward * 10, Color.red);
        //RayCastInfo(m_rayPerceptionSensorcomponent3D);
    }

    public float DecisionWaitingTime = 5f;
    float m_currentTime = 0f;

    public void WaitTimeInference(int action)
    {
        if (Academy.Instance.IsCommunicatorOn)
        {
            RequestDecision();
        }
        else
        {
            if (m_currentTime >= DecisionWaitingTime)
            {
                m_currentTime = 0f;
                RequestDecision();
            }
            else
            {
                m_currentTime += Time.fixedDeltaTime;
            }
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("DEAD_ZONE"))
        {
            SetReward(-1.0f);
            EndEpisode();
        }
        if (collision.collider.CompareTag("Floor"))
        {
            SetReward(-1.0f);
            EndEpisode();
        }
        /*
        if(collision.gameObject.CompareTag("DEAD_ZONE"))
        {
            EndEpisode();
        }*/

    }
}