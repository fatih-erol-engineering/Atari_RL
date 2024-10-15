# Source: https://www.youtube.com/watch?v=tsy1mgB7hB0

import os
from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper
from torch import nn
import torch
from torch.utils.tensorboard import SummaryWriter

from collections import deque
import itertools
import numpy as np
import random
import msgpack
from msgpack_numpy import patch as msgpack_numpy_patch

msgpack_numpy_patch()
GAMMA=0.99
BATCH_SIZE=32
BUFFER_SIZE=50000
MIN_REPLAY_SIZE=1000
EPSILON_START=1.0
EPSILON_END=0.02
EPSILON_DECAY=10000
TARGET_UPDATE_FREQ = 1000
SAVE_PATH = './results/atari_model.pack'
SAVE_INTERVAL = 10_000
LOG_DIR = './logs/atari_vanilla'
LOG_INTERVAL = 1000

class Network(nn.Module):
    def __init__(self, env):
        super().__init__()

        in_features = int(np.prod(env.observation_space.shape))

        self.net = nn.Sequential(
            nn.Linear(in_features, 128),
            nn.Tanh(),
            nn.Linear(128, 256),
            nn.Tanh(),
            nn.Linear(256, env.action_space.n)
        )

    def forward(self, x):
        return self.net(x)

    def act(self, obs):
        obs_t = torch.as_tensor(obs, dtype=torch.float32)
        q_values = self(obs_t.unsqueeze(0))
        max_q_index = torch.argmax(q_values, dim=1)[0]
        action = max_q_index.detach().item()

        return action
    def load(self):
        pass
    def save(self,save_path):
        params = {k: t.detach().cpu().numpy() for k,t in self.state_dict().items()}
        params_data = msgpack.dumps(params)
        
        os.makedirs(os.path.dirname(save_path), exist_ok = True)
        with open(save_path, 'wb') as f:
            f.write(params_data)
        
    def load(self,load_path):
        if not os.path_exists(load_path):
            raise FileNotFoundError(load_path)
        with open(load_path,'rb') as f:
            params_numpy = msgpack.loads(f.read())
        params = {k: torch.as_tensor(v) for k,v in params_numpy.items()}
        self.load_state_dict(params)
        
env = UnityToGymWrapper(UnityEnvironment(),allow_multiple_obs=False)

replay_buffer = deque(maxlen=BUFFER_SIZE)
rew_buffer = deque([0.0], maxlen=100)

episode_reward = 0
episode_count = 0
summary_writer = SummaryWriter(LOG_DIR)

online_net = Network(env)
target_net = Network(env)

target_net.load_state_dict(online_net.state_dict())

optimizer = torch.optim.Adam(online_net.parameters(), lr=5e-4)

# Initialize replay buffer
obs = env.reset()
for _ in range(MIN_REPLAY_SIZE):
    action = env.action_space.sample()

    new_obs, rew, done, _ = env.step(action)
    transition = (obs, action, rew, done, new_obs)
    replay_buffer.append(transition)
    obs = new_obs

    if done:
        obs = env.reset()


# Main Training Loop
obs = env.reset()
for step in itertools.count():
    epsilon = np.interp(step, [0, EPSILON_DECAY], [EPSILON_START, EPSILON_END])

    rnd_sample = random.random()
    if rnd_sample <= epsilon:
        action = env.action_space.sample()
    else:
        action = online_net.act(obs)

    new_obs, rew, done, _ = env.step(action)
    transition = (obs, action, rew, done, new_obs)
    replay_buffer.append(transition)
    obs = new_obs

    episode_reward += rew

    if done:
        obs = env.reset()

        rew_buffer.append(episode_reward)
        episode_reward = 0

    # If we solved it lets just watch it play, put in last
    if len(rew_buffer) >= 100:
        if np.mean(rew_buffer) >= 195:
            while True:
                action = online_net.act(obs)

                obs, _, done, _ = env.step(action)
                env.render()
                if done: 
                    env.reset()

    transitions = random.sample(replay_buffer, BATCH_SIZE)

    obses = np.asarray([t[0] for t in transitions])
    actions = np.asarray([t[1] for t in transitions])
    rews = np.asarray([t[2] for t in transitions])
    dones = np.asarray([t[3] for t in transitions])
    new_obses = np.asarray([t[4] for t in transitions])

    obses_t = torch.as_tensor(obses, dtype=torch.float32)
    actions_t = torch.as_tensor(actions, dtype=torch.int64).unsqueeze(-1)
    rews_t = torch.as_tensor(rews, dtype=torch.float32).unsqueeze(-1)
    dones_t = torch.as_tensor(dones, dtype=torch.float32).unsqueeze(-1)
    new_obses_t = torch.as_tensor(new_obses, dtype=torch.float32)

    # Compute Targets
    # targets = r + gamma * target q vals * (1 - dones)
    target_q_values = target_net(new_obses_t)
    max_target_q_values = target_q_values.max(dim=1, keepdim=True)[0]

    targets = rews_t + GAMMA * (1 - dones_t) * max_target_q_values

    # Compute Loss
    q_values = online_net(obses_t)
    action_q_values = torch.gather(input=q_values, dim=1, index=actions_t)

    loss = nn.functional.smooth_l1_loss(action_q_values, targets)

    # Gradient Descent
    optimizer.zero_grad()
    loss.backward()
    optimizer.step()
    episode_count += 1
    # Update Target Net
    if step % TARGET_UPDATE_FREQ == 0:
        target_net.load_state_dict(online_net.state_dict())

    # Logging
    if step % LOG_INTERVAL == 0:
        print()
        print('Step:', step)
        print('Avg Rew:', np.mean(rew_buffer))
        print('Episodes:', episode_count)
        
        # summary_writer.add_scalar('AvgRew',rew_buffer, global_step=step)
        # summary_writer.add_scalar('Episodes', episode_count, global_step=step)
    if step % SAVE_INTERVAL == 0 and step != 0:
        print('Saving...')
        online_net.save(SAVE_PATH)










