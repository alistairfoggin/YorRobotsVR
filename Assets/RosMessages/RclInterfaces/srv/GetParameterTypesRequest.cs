//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.RclInterfaces
{
    [Serializable]
    public class GetParameterTypesRequest : Message
    {
        public const string k_RosMessageName = "rcl_interfaces/GetParameterTypes";
        public override string RosMessageName => k_RosMessageName;

        //  A list of parameter names.
        //  TODO(wjwwood): link to parameter naming rules.
        public string[] names;

        public GetParameterTypesRequest()
        {
            this.names = new string[0];
        }

        public GetParameterTypesRequest(string[] names)
        {
            this.names = names;
        }

        public static GetParameterTypesRequest Deserialize(MessageDeserializer deserializer) => new GetParameterTypesRequest(deserializer);

        private GetParameterTypesRequest(MessageDeserializer deserializer)
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
            return "GetParameterTypesRequest: " +
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
