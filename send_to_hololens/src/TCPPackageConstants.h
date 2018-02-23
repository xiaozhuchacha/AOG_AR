using namespace std;

#if !defined(TCP_PACKAGE_CONSTANTS)
#define TCP_PACKAGE_CONSTANTS 1

const int TOTAL_NUM_TF = 19;

const int HEAD = 0;
const int BASE = 1;

const int LEFT_HAND = 2;
const int LEFT_UPPER_ELBOW = 3;
const int LEFT_LOWER_ELBOW = 4;
const int LEFT_UPPER_FOREARM = 5;
const int LEFT_LOWER_FOREARM = 6;
const int LEFT_UPPER_SHOULDER = 7;
const int LEFT_LOWER_SHOULDER = 8;
const int LEFT_WRIST = 9;

const int RIGHT_HAND = 10;
const int RIGHT_UPPER_ELBOW = 11;
const int RIGHT_LOWER_ELBOW = 12;
const int RIGHT_UPPER_FOREARM = 13;
const int RIGHT_LOWER_FOREARM = 14;
const int RIGHT_UPPER_SHOULDER = 15;
const int RIGHT_LOWER_SHOULDER = 16;
const int RIGHT_WRIST = 17;

const int LEFT_GRIPPER = 18;

const int LEFT_FORCE_TORQUE = 19;

const string frame_name_arr[] = {"head", "base", "left_hand", "left_upper_elbow", "left_lower_elbow", 
                        "left_upper_forearm", "left_lower_forearm", "left_upper_shoulder", "left_lower_shoulder", 
                        "left_wrist", "right_hand", "right_upper_elbow", "right_lower_elbow", "right_upper_forearm",
                        "right_lower_forearm", "right_upper_shoulder", "right_lower_shoulder", "right_wrist", "left_gripper"};


#endif