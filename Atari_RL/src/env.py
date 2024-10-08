from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper
import random
import numpy as np

env = UnityToGymWrapper(UnityEnvironment(),allow_multiple_obs=True)
env.reset()
terminated = False

actions = [0,1,2]
while not terminated:  
    observation, reward, terminated, info = env.step(random.choice(actions)) 
    imageObs = observation[0];
    time_sec = observation[1];
    
