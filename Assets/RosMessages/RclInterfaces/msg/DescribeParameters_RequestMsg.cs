//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.RclInterfaces
{
    [Serializable]
    public class DescribeParameters_RequestMsg : Message
    {
        public const string k_RosMessageName = "rcl_interfaces/DescribeParameters_Request";
        public override string RosMessageName => k_RosMessageName;

        //  A list of parameters of which to get the descriptor.
        public string[] names;

        public DescribeParameters_RequestMsg()
        {
            this.names = new string[0];
        }

        public DescribeParameters_RequestMsg(string[] names)
        {
            this.names = names;
        }

        public static DescribeParameters_RequestMsg Deserialize(MessageDeserializer deserializer) => new DescribeParameters_RequestMsg(deserializer);

        private DescribeParameters_RequestMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.names, deserializer.ReadLength());
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.WriteLength(this.names);
            serializer.Write(this.names);
        }

        public override string ToString()
        {
            return "DescribeParameters_RequestMsg: " +
            "\nnames: " + System.String.Join(", ", names.ToList());
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}