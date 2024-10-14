from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper
import random
import numpy as np
from collections import deque

env = UnityToGymWrapper(UnityEnvironment(),allow_multiple_obs=False)
env.reset()
terminated = False
actions = [0,1,2]
observation, reward, terminated, info = env.step(random.choice(actions)) 
time_sec =observation[0]
game_score =observation[1]
ball_position =observation[2:5]
paddle_position =observation[5:8]
ball_velocity =observation[8]
ball_angle_rad =observation[9]
isBrickHitted =observation[10:]
state = [ball_position[0], 
         ball_position[1], 
         ball_position[2], 
         paddle_position[0], 
         paddle_position[1], 
         paddle_position[2], 
         ball_velocity, 
         ball_angle_rad, 
         isBrickHitted]
pass

