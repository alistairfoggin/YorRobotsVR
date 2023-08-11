# Virtual Reality Interface for Robot Teleoperation and Environment Visualisation
## Setup
Set up SteamVR room scale (or standing but that is untested) It is a lot easier to set up in Windows.
Install Unity 2020.2 or higher (This project is 2022.2.3 but I would recommended latest LTS for your own projects.)
If in windows, set up Ubuntu Virtual Machine (VM). Alternatively you can run Ubuntu natively on another machine.
Follow this guide for installing Ubuntu 22.04 in VirtualBox: <https://ubuntu.com/tutorials/how-to-run-ubuntu-desktop-on-a-virtual-machine-using-virtualbox>

Once installed, you can optionally install virtualbox utils in Ubuntu to allow for shared folders:
```sh
sudo apt install virtualbox-guest-utils
```

Install ROS2 Humble: Ubuntu (Debian packages) — [ROS 2 Documentation: Humble documentation](https://docs.ros.org/en/humble/Installation/Ubuntu-Install-Debians.html)
### Network Settings
Power off the VM, and go to network settings. Change it to be attached to "Bridged Adapter", set Name to be the adapter connected to your network. Open up advanced, and set Promiscuous Mode to be "Allow All"
![image](https://github.com/codexpro88/YorRobotsVR/assets/20808504/7cd22373-834b-46bc-92e2-792bdda0627b)

### ROS Packages and Setup
Start up the VM again, and create a new ROS 2 Workspace
Creating a workspace — ROS 2 Documentation: Humble documentation

Install the Turtlebot3, Navigation2, SLAM_Toolbox, Compressed_Image_Transport packages
```sh
sudo apt update
sudo apt install ros-humble-turtlebot3 
sudo apt install ros-humble-turtlebot3-msgs
sudo apt install ros-humble-turtlebot3-gazebo

sudo apt install ros-humble-navigation2
sudo apt install ros-humble-nav2-bringup

sudo apt install ros-humble-slam-toolbox

sudo apt install ros-humble-compressed-image-transport
```

Also add the following to the ~/.bashrc file and restart the terminal
```sh
source /opt/ros/humble/setup.bash
export ROS_DOMAIN_ID=30 #TURTLEBOT3
export TURTLEBOT3_MODEL=burger
```

To install the ROS-TCP Endpoint
```sh
cd ~/ros2_ws/src
git clone -b main-ros2 https://github.com/Unity-Technologies/ROS-TCP-Endpoint.git
cd ..
```
### Unity Setup
To run this project, clone the repository, and open the project in Unity. Open the scene `Assets\_VR Robotics\Scenes\Base Scene.unity`
To connect it to the ROS VM, you have to tell the ROS TCP Connector the IP address which corresponds to the Ubuntu VM. To find that out run `hostname -I` in the VM.
Then in Unity under Robotics->ROS Settings set the protocol to ROS2 and set the IP address. Within the Base Scene, open the ROS Tools gameobject and select the ROSConnectionPrefab.
Then in the inspector set the ROS IP Address.

> **_NOTE:_** To set up your own project that can connect to the VM, you need to install the [ROS-TCP-Connector](https://github.com/Unity-Technologies/ROS-TCP-Connector)
> I would also recommend installing the Visualisations package in the same repository. Then you can follow similar steps as above to set the IP address. For setting up VR,
> you can follow the [Unity Manual VR guide](https://docs.unity3d.com/Manual/VROverview.html), and I would recommend installing the
> [XR Interaction Toolkit](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.3/manual/index.html) too.

## Running the Interface
