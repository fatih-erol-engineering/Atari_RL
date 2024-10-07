from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper
import random

# Unity ortamını başlat
env = UnityToGymWrapper(UnityEnvironment())
env.reset()
terminated = False


actions = [0,1,2]
while not terminated:  
    observation, reward, terminated, info = env.step(random.choice(actions))   

pass