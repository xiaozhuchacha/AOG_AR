#include <tacopie/tacopie>

#include <iostream>
#include <stdio.h>
#include <fstream>
#include "ros/ros.h"
#include <tf2_msgs/TFMessage.h>
#include <math.h>
#include <ros/console.h>
#include <tf/transform_listener.h>
#include <boost/thread.hpp>
#include <tf/tf.h>
#include "TCPPackageConstants.h"
#include <image_transport/image_transport.h>

#include "baxter_core_msgs/EndpointState.h"
#include "action_manager/execAction.h"

using namespace std;

// flags
int PROCESS_TF_DATA = 1;
int PROCESS_GRIPPER_DATA = 1;
int PROCESS_IMAGE_DATA = 0;

tacopie::tcp_client client;

// to count the seq of data points
// use the first 100 to find offset
const int num_to_find_offset = 40;
int left_data_offset_counter = 0;

//store the last 4 data points, running average
double last_n_left_data_average[6] = {0,0,0,0,0,0};


// use the average of data from the 1st second to calculate offset (100 data points)
// store the offset in a array
// left: force.x, force.y, force.z, torque.x, torque.y, torque.z
double force_torque_offset_left[6] = {0,0,0,0,0,0};

int imageCounter = 0;

ros::ServiceClient action_client;

// ros::WallTime begin;
// ros::WallTime now;

void write_cb(const tacopie::tcp_client::write_result& res) {
    if(res.success) {
        // cout << "sent package " << res.size << " bytes" << endl;
    } else {
        cout << "sending failed!" << endl;
    }
}

void on_new_message(const tacopie::tcp_client::read_result& res) {
    if (res.success){
    	// here print the action name
        for (auto i: res.buffer){
            cout << i ;
        }
        cout << endl;
        cout << res.buffer.size() << endl;

        const char* p = res.buffer.data(); // p
        if (res.buffer.size() <= 3){
            //const char* p = res.buffer.data(); // p
            switch(p[0]){
                case 'i':
                case 'I':
                    if (PROCESS_IMAGE_DATA){
                        PROCESS_IMAGE_DATA = 0;
                    } else {
                        PROCESS_IMAGE_DATA = 1;
                    }
                    break;

                case 'f':
                case 'F':
                    if (PROCESS_GRIPPER_DATA){
                        PROCESS_GRIPPER_DATA = 0;
                        left_data_offset_counter = 0;
                        for (int i = 0; i < 6; i ++){
                            last_n_left_data_average[i] = 0;
                            force_torque_offset_left[i] = 0;
                        }
                    } else {
                        PROCESS_GRIPPER_DATA = 1;
                    }
                    break;

                case 't':
                case 'T':
                    if (PROCESS_TF_DATA){
                        PROCESS_TF_DATA = 0;
                    } else {
                        PROCESS_TF_DATA = 1;
                    }
                    break;
            }            
        }
        else {
        	if (p[0] == 'a') {
        		std::string action_string(p+1);
                //cout << str(p[1]) << endl; // get action name
        		// to add service call function
        		//action_srv.request.action = str(p[1]);
        		//action_client.call(action_srv);
        		//sleep(1.5);

                action_manager::execAction action_srv;

                if(action_string == "grasp") {
                    action_string = "grasp_right";
                    action_srv.request.action = action_string;
                }
                else if(action_string == "ungrasp") {
                    action_string = "ungrasp_right";
                    action_srv.request.action = action_string;                    
                }
                else if(action_string == "push") {
                    action_string = "push";
                    action_client.call(action_srv);
                    action_string = "twist";
                }
                else {
                    action_srv.request.action = action_string;
                }
                action_client.call(action_srv);
        	}
        }

    }
    else {
        cout << "read failed, client disconnected" << endl;
    }
    client.async_read({30, bind(&on_new_message, placeholders::_1)});
}

void startConnection() {
	client.connect("192.168.1.176", 12345);
    cout << "connection established" << endl;
    
}


void leftEndPointCallBack(const baxter_core_msgs::EndpointState end_point_state) {
    if (!PROCESS_GRIPPER_DATA){
        return;
    }
    double fx = end_point_state.wrench.force.x;
    double fy = end_point_state.wrench.force.y;
    double fz = end_point_state.wrench.force.z;
    double tx = end_point_state.wrench.torque.x;
    double ty = end_point_state.wrench.torque.y;
    double tz = end_point_state.wrench.torque.z;
    if (left_data_offset_counter < num_to_find_offset)
    {
        force_torque_offset_left[0] += fx;
        force_torque_offset_left[1] += fy;
        force_torque_offset_left[2] += fz;
        force_torque_offset_left[3] += tx;
        force_torque_offset_left[4] += ty;
        force_torque_offset_left[5] += tz;
        left_data_offset_counter ++;
    } 
    else if (left_data_offset_counter == num_to_find_offset) 
    {
        for (int i = 0; i < 6; i++){
            force_torque_offset_left[i] /= num_to_find_offset;
        }
        left_data_offset_counter ++;
        // now=ros::WallTime::now();
        // ros::WallDuration diff=now-begin;

        // cout<<"diff: "<<diff.toSec() <<endl;
    } 
    else 
    {
        // because sensor feels the force/torque on the robot
        // the force/torque robot exerts is in the other direction
        fx = force_torque_offset_left[0] - fx;
        fy = force_torque_offset_left[1] - fy;
        fz = force_torque_offset_left[2] - fz;
        tx = force_torque_offset_left[3] - tx;
        ty = force_torque_offset_left[4] - ty;
        tz = force_torque_offset_left[5] - tz;

        // calculate new running average
        last_n_left_data_average[0] = ((last_n_left_data_average[0] * 3) + fx)/4;
        last_n_left_data_average[1] = ((last_n_left_data_average[1] * 3) + fy)/4;
        last_n_left_data_average[2] = ((last_n_left_data_average[2] * 3) + fz)/4;
        last_n_left_data_average[3] = ((last_n_left_data_average[3] * 3) + tx)/4;
        last_n_left_data_average[4] = ((last_n_left_data_average[4] * 3) + ty)/4;
        last_n_left_data_average[5] = ((last_n_left_data_average[5] * 3) + tz)/4;

        //calculate the magnitude for force and torque
        double force_mag = sqrt(pow(last_n_left_data_average[0],2.0) +
                                pow(last_n_left_data_average[1],2.0) +
                                pow(last_n_left_data_average[2],2.0));
        double torque_mag = sqrt(pow(last_n_left_data_average[3],2.0) +
                                pow(last_n_left_data_average[4],2.0) +
                                pow(last_n_left_data_average[5],2.0));

        int num_of_elements = 10;
        float* msg = new float[num_of_elements];
        size_t msg_length = sizeof(float) * num_of_elements;

        // first 4 byte encodes the size of the following package
        ((int*) msg)[0] = msg_length - 4;
        ((int*) msg)[1] = LEFT_FORCE_TORQUE;
        // to change
        for (int i = 0; i < 6; i++){
            msg[i+2] = (float) last_n_left_data_average[i];
        }
        msg[8] = (float) force_mag;
        msg[9] = (float) torque_mag;

        char* c_ptr = reinterpret_cast<char*>(msg);
        vector<char> vec(c_ptr, c_ptr + msg_length);
        client.async_write({vec, bind(&write_cb, placeholders::_1)});

        delete[] msg;
    }
}

void getTransform(tf::TransformListener& listener, tf::StampedTransform& transform, 
                const int target_frame, float* msg, int index) {
    try {
        listener.waitForTransform("screen", frame_name_arr[target_frame], ros::Time(0), ros::Duration(5.0));
        listener.lookupTransform("screen", frame_name_arr[target_frame], ros::Time(0), transform);
    }
    catch (tf::TransformException ex){
        ROS_ERROR("%s",ex.what());
        // ros::Duration(1.0).sleep();
    }

    tf::Vector3 translation = transform.getOrigin();
    tf::Quaternion rotation = transform.getRotation();

    ((int*) msg)[index] = target_frame;
    msg[index + 1] = translation.x();
    msg[index + 2] = translation.y();
    msg[index + 3] = translation.z();
    msg[index + 4] = rotation.getX();
    msg[index + 5] = rotation.getY();
    msg[index + 6] = rotation.getZ();
    msg[index + 7] = rotation.getW();
}


void tf_thread()
{
    tf::TransformListener listener;

    ros::Rate rate(15.0);

    while (ros::ok()){
        rate.sleep();

        if (!PROCESS_TF_DATA){
            continue;
        }
        tf::StampedTransform transform;
        int num_of_elements = TOTAL_NUM_TF * 8 + 1;
        size_t msg_length = sizeof(float) * num_of_elements;


        float* msg = new float[num_of_elements];
        ((int*) msg)[0] = msg_length - 4;
        
        int index = 1;

        for (int i = 0; i < TOTAL_NUM_TF; i++) {
            getTransform(listener, transform, i, msg, index);
            index += 8;
        }



        char* c_ptr = reinterpret_cast<char*>(msg);
        vector<char> vec(c_ptr, c_ptr + msg_length);
        client.async_write({vec, bind(&write_cb, placeholders::_1)});

        delete[] msg;
    }
}

// void tcpListener() {
//     size_t read_size = 20;
//     client.async_read({read_size, bind(&on_new_message, placeholders::_1)});
//     boost::this_thread::interruption_point();
//     // while (ros::ok()){
//     //     boost::this_thread::interruption_point();
//     //     client.async_read({read_size, bind(&on_new_message, placeholders::_1)});
//     // }
// }

void imageCallback(const sensor_msgs::ImageConstPtr& msg) {
    if (!PROCESS_IMAGE_DATA){
        return;
    }
    if (imageCounter < 3){
        imageCounter ++;
        return;
    }
    imageCounter = 0;

    // cout << "height: " << msg->height << endl;
    // cout << "width: " << msg->width << endl;
    // cout << typeid(msg->data).name() << endl;   // it is vector<unsigned char>
    int msg_length = (msg->data).size();
    // cout << msg_length << endl;

    vector<char> vec((char*)&msg_length, (char*)&msg_length + 4);
    vec.insert(vec.end(), reinterpret_cast<const char*>((msg->data).data()), reinterpret_cast<const char*>((msg->data).data()) + msg_length);

    client.async_write({vec, bind(&write_cb, placeholders::_1)});

}

int main(int argc, char **argv) {
    ros::init(argc,argv,"send_to_hololens");
    ros::NodeHandle n;

    startConnection();

    if (ros::console::set_logger_level(ROSCONSOLE_DEFAULT_NAME, ros::console::levels::Debug)) 
    {
        // Change the level to fit your needs
        ros::console::notifyLoggerLevelsChanged();
    }

    image_transport::ImageTransport it(n);

    ros::Subscriber subLeft = n.subscribe("/robot/limb/left/endpoint_state", 1, leftEndPointCallBack);
    // image_transport::Subscriber subImage = it.subscribe("/cameras/right_hand_camera/image", 1, imageCallback);
    image_transport::Subscriber subImage = it.subscribe("/simtrack/image", 1, imageCallback);
    action_client = n.serviceClient<action_manager::execAction>("/action_manager/exec");

    client.async_read({30, bind(&on_new_message, placeholders::_1)});


    boost::thread t1(tf_thread);
    // boost::thread t2(tcpListener);

    ros::Rate r1(15);

    // begin = ros::WallTime::now();


    while (ros::ok()){
        ros::spinOnce();
        r1.sleep();
    }

    t1.join();
    // t2.interrupt();
 
 	// close(sock);

	return 0;
}
