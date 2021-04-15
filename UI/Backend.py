
import socket
import time
import math
#from nuscenes.nuscenes import NuScenes 
import json
import numpy as np
from pyquaternion import Quaternion

#seting up socket
host, port = "127.0.0.1", 54955
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect((host, port))

#nusc = NuScenes(version='v1.0-mini', dataroot='data', verbose=True)

max_height=5
max_length=20
max_width=20

def rotate_around_point_lowperf(point, radians, origin=(0, 0)):
      """Rotate a point around a given point.
      
      I call this the "low performance" version since it's recalculating
      the same values more than once [cos(radians), sin(radians), x-ox, y-oy).
      It's more readable than the next function, though.
      """
      x, y = point
      ox, oy = origin

      qx = ox + math.cos(radians) * (x - ox) + math.sin(radians) * (y - oy)
      qy = oy + -math.sin(radians) * (x - ox) + math.cos(radians) * (y - oy)

      return qx, qy

def quaternion_yaw(q: Quaternion) -> float:
      """
      Calculate the yaw angle from a quaternion.
      See https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles.
      :param q: Quaternion of interest.
      :return: Yaw angle in radians.
      """

      a = 2.0 * (q[0] * q[3] + q[1] * q[2])
      b = 1.0 - 2.0 * (q[2] ** 2 + q[3] ** 2)

      return np.arctan2(a, b)

def convert_to_top_corner(point):
      """Convert the position with respect to the top left corner"""
      point[0] = max_length + point[0]
      point[1] = max_width - point[1]
      return point


with open('data/combined_samples.json') as f:
    samples = json.load(f)
for i in range(394):
    ego_pose = samples[i]['ego_pose']
    sock.sendall("FRAME".encode("UTF-8"))
    ToSendString = ""
    for annos in samples[i]['anns']:
        annotation_metadata =  annos
        
        ego_yaw = quaternion_yaw(ego_pose['rotation']) - math.pi/2
        cordinates = [annotation_metadata['translation'][i] - ego_pose['translation'][i] for i in range(3)]
        cordinates[0], cordinates[1] = rotate_around_point_lowperf(cordinates[:2], ego_yaw, origin=(0, 0))
        #cordinates = convert_to_top_corner(cordinates)
        if cordinates[0] > max_width or cordinates[0] < - max_width or cordinates[1] > max_length or cordinates[1] < -max_length:# or (self.augment and self.check_cameraregion() == 0):
            continue

        rotation = [annotation_metadata['rotation'][i] - ego_pose['rotation'][i] for i in range(4)]

        #converting list to string then to byte and sending to c#
        transString = ','.join(map(str, cordinates)) #Converting translation list to a string, example "0,0,0"
        sizeString = ','.join(map(str, annotation_metadata['size'])) #Converting size list to a string, example "0,0,0"
        rotationString = ','.join(map(str, rotation)) #Converting rotation list to a string, example "0,0,0"
        
        ToSendString = ToSendString + "$" + sizeString + "/" + rotationString + "/" + annotation_metadata['category_name'] + "/" + transString

    print(ToSendString)
    sock.sendall(ToSendString.encode("UTF-8"))
    time.sleep(0.5) #sleep 0.5 sec
    #sock.sendall("DONE".encode("UTF-8")) #please delete this for final testing
    #break #use this break while testing
    #sock.sendall("SCENE".encode("UTF-8"))
sock.sendall("DONE".encode("UTF-8"))
