//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Turtlesim
{
    [Serializable]
    public class KillResponse : Message
    {
        public const string k_RosMessageName = "turtlesim/Kill";
        public override string RosMessageName => k_RosMessageName;


        public KillResponse()
        {
        }
        public static KillResponse Deserialize(MessageDeserializer deserializer) => new KillResponse(deserializer);

        private KillResponse(MessageDeserializer deserializer)
        {
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
        }

        public override string ToString()
        {
            return "KillResponse: ";
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize, MessageSubtopic.Response);
        }
    }
}
