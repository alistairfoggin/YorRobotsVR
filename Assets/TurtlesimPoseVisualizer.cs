using Unity.Robotics.Visualizations;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;
using UnityEngine;
using RosMessageTypes.Nav;

public class TurtlesimPoseVisualizer : DrawingVisualizer<RosMessageTypes.Turtlesim.PoseMsg>
{
	// these settings will appear as configurable parameters in the Unity editor.
	[SerializeField]
	float m_Size = 0.1f;
	[SerializeField]
	Color m_Color;
	[SerializeField]
	string m_Label;

	[SerializeField]
	[Tooltip("If ticked, draw the axis lines for Unity coordinates. Otherwise, draw the axis lines for ROS coordinates (FLU).")]
	bool m_DrawUnityAxes;

	public override void Draw(Drawing3d drawing, RosMessageTypes.Turtlesim.PoseMsg msg, MessageMetadata meta)
	{
		// If the user hasn't specified a color, SelectColor helpfully picks one
		// based on the message topic.
		Color finalColor = VisualizationUtils.SelectColor(m_Color, meta);

		// Similarly, if the user leaves the label blank, SelectLabel will use the
		// topic name as a label.
		string finalLabel = VisualizationUtils.SelectLabel(m_Label, meta);

		Quaternion angle = Quaternion.Euler(0.0f, msg.theta, 0.0f);
		// Most of the default visualizers offer static drawing functions
		// so that your own visualizers can easily send work to them.
		RosMessageTypes.Geometry.PoseMsg poseMsg = new RosMessageTypes.Geometry.PoseMsg(new PointMsg(msg.x, msg.y, 0.0), new QuaternionMsg(angle.x, angle.y, angle.z, angle.w));
		PoseDefaultVisualizer.Draw<FLU>(poseMsg, drawing, m_Size, m_DrawUnityAxes);


        // You can also directly use the drawing functions provided by the Drawing class
        drawing.DrawLabel(finalLabel, new Vector3(msg.x, msg.y), finalColor, m_Size);
	}

	public override System.Action CreateGUI(RosMessageTypes.Turtlesim.PoseMsg msg, MessageMetadata meta)
	{
		// this code runs each time a new message is received.
		// If you want to preprocess the message or declare any state variables for
		// the GUI to use, you can do that here.
		string text = $"[{msg.x}, {msg.y}], {msg.theta}";

		return () =>
		{
			// this code runs once per UI event, like a normal Unity OnGUI function.
			GUILayout.Label(text);
		};
	}
}
