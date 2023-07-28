//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.RclInterfaces
{
    [Serializable]
    public class ParameterMsg : Message
    {
        public const string k_RosMessageName = "rcl_interfaces/Parameter";
        public override string RosMessageName => k_RosMessageName;

        //  This is the message to communicate a parameter. It is an open struct with an enum in
        //  the descriptor to select which value is active.
        //  The full name of the parameter.
        public string name;
        //  The parameter's value which can be one of several types, see
        //  `ParameterValue.msg` and `ParameterType.msg`.
        public ParameterValueMsg value;

        public ParameterMsg()
        {
            this.name = "";
            this.value = new ParameterValueMsg();
        }

        public ParameterMsg(string name, ParameterValueMsg value)
        {
            this.name = name;
            this.value = value;
        }

        public static ParameterMsg Deserialize(MessageDeserializer deserializer) => new ParameterMsg(deserializer);

        private ParameterMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.name);
            this.value = ParameterValueMsg.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.name);
            serializer.Write(this.value);
        }

        public override string ToString()
        {
            return "ParameterMsg: " +
            "\nname: " + name.ToString() +
            "\nvalue: " + value.ToString();
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
