//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.RclInterfaces
{
    [Serializable]
    public class SetParametersRequest : Message
    {
        public const string k_RosMessageName = "rcl_interfaces/SetParameters";
        public override string RosMessageName => k_RosMessageName;

        //  A list of parameters to set.
        public ParameterMsg[] parameters;

        public SetParametersRequest()
        {
            this.parameters = new ParameterMsg[0];
        }

        public SetParametersRequest(ParameterMsg[] parameters)
        {
            this.parameters = parameters;
        }

        public static SetParametersRequest Deserialize(MessageDeserializer deserializer) => new SetParametersRequest(deserializer);

        private SetParametersRequest(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.parameters, ParameterMsg.Deserialize, deserializer.ReadLength());
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.WriteLength(this.parameters);
            serializer.Write(this.parameters);
        }

        public override string ToString()
        {
            return "SetParametersRequest: " +
            "\nparameters: " + System.String.Join(", ", parameters.ToList());
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
