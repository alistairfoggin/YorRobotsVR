using RosMessageTypes.Nav;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var rosConnection = ROSConnection.GetOrCreateInstance();
        rosConnection.Subscribe<OccupancyGridMsg>("/map", MapUpdate);
    }

    private void MapUpdate(OccupancyGridMsg obj)
    {
        print(obj.header.stamp.sec);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
