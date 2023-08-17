# Virtual Reality Interface for Robot Teleoperation and Environment Visualisation
A Virtual Reality interface implemented in Unity which is used to connect to a mobile robot running the Robot Operating System. 
It provides visualisations of sensor data as well as controls for teleoperation. It was created as part of a 
[YorRobots Venables Internship](https://www.york.ac.uk/yorrobots/news-events/news/2023/venables-internships-2023/) in the summer of 2023.

## Setup
- Ubuntu 22.04 VM with ROS2 Humble
    - Turtlebot3 Nodes
    - SLAM Toolbox / Cartographer
    - Navigation 2
    - ROS TCP Endpoint
- Windows host with Unity 2022.2 and SteamVR
    - HTC Vive Pro Eye (Or other HTC Vive headset)
    - Clone project and open with Unity 2022.2

[Full Setup Guide](https://github.com/alistairfoggin/YorRobotsVR/wiki/Setup)

## Running the Interface with Simulation
Start up the Ubuntu VM if it is not already running.
There are four commands to run for simulating the robot with slam and navigation

To run the ROS TCP Endpoint run the following replacing `<IP ADDRESS>` with the IP address found above using `hostname -I`.
```sh
cd ~/ros2_ws
source install/setup.bash (May not exist so ignore it on first run)
colcon build
source install/setup.bash
ros2 run ros_tcp_endpoint default_server_endpoint –ros-args -p ROS_IP:=<IP ADDRESS>
```

Launch the Gazebo Simulation of the turtlebot with either of the following commands in a new terminal:
```
ros2 launch turtlebot3_gazebo turtlebot3_house.launch.py
```
or
```
ros2 launch turtlebot3_gazebo turtlebot3_world.launch.py
```

Launch SLAM Toolbox in a new terminal:
```
ros2 launch slam_toolbox online_async_launch.py use_sim_time:=True
```
Or launch Cartographer
```
ros2 launch turtlebot3_cartographer cartographer.launch.py use_sim_time:=True
```

Launch Navigation2 in a new terminal:
```
ros2 launch turtlebot3_navigation2 navigation2.launch.py use_sim_time:=True
```

To launch the unity project, open `Assets/_VR Robotics/Scenes/Base Scene.unity`. Make sure the ROS Connector Script in the `ROS Tools->ROSConnectionPrefab` GameObject has the right IP address, connect the VR headset and click play.

## Running the Interface with an Actual Turtlebot3
### Turtlebot3 
Either in seperate terminals or in seperate tmux panes run the following commands:
```sh
ros2 launch turtlebot3_bringup robot.launch.py
ros2 launch ./picamera.launch.py
```
### VM or Remote PC
Launch SLAM Toolbox in a new terminal:
```
ros2 launch slam_toolbox online_async_launch.py use_sim_time:=True
```
Or launch Cartographer
```
ros2 launch turtlebot3_cartographer cartographer.launch.py use_sim_time:=True
```

Launch Navigation2 in a new terminal:
```
ros2 launch turtlebot3_navigation2 navigation2.launch.py use_sim_time:=True
```

To run the ROS TCP Endpoint in a new terminal run the following replacing `<IP ADDRESS>` with the IP address found above using `hostname -I`.
```sh
cd ~/ros2_ws
source install/setup.bash (May not exist so ignore it on first run)
colcon build
source install/setup.bash
ros2 run ros_tcp_endpoint default_server_endpoint –ros-args -p ROS_IP:=<IP ADDRESS>
```
### Windows
To launch the unity project, open `Assets/_VR Robotics/Scenes/Base Scene.unity`. Make sure the ROS Connector Script in the `ROS Tools -> ROSConnectionPrefab` GameObject has the right IP address, connect the VR headset and click play.
