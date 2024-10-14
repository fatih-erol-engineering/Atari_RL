from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper
import numpy as np
from collections import deque
import random
import torch
from model import Linear_QNet, QTrainer
from helper import plot

MAX_MEMORY = 100_000
BATCH_SIZE = 1000
LR = 0.001
actions = [0,1,2]
class Agent:
    
    def __init__(self):
        self.n_games = 0
        self.epsilon = 0.1 # randomness
        self.gamma = 0.9 # discount rate
        self.memory = deque(maxlen=MAX_MEMORY)
        self.model = Linear_QNet(20,256,3)
        self.trainer = QTrainer(self.model, lr=LR, gamma=self.gamma)
        self.observation = np.array([])
    def get_state(self,game):
        if  self.observation.size == 0:
            self.observation = game.reset()[0]
            
        time_sec = self.observation[0]
        game_score = self.observation[1]
        ball_position = self.observation[2:5]
        paddle_position = self.observation[5:8]
        ball_velocity = self.observation[8]
        ball_angle_rad = self.observation[9]
        isBrickHitted = self.observation[10:]
        state = np.array([float(ball_position[0]),
                float(ball_position[1]), 
                float(ball_position[2]), 
                float(paddle_position[0]), 
                float(paddle_position[1]), 
                float(paddle_position[2]), 
                float(ball_velocity/10), 
                float(ball_angle_rad), 
                float(isBrickHitted[0]),
                float(isBrickHitted[1]),
                float(isBrickHitted[2]),
                float(isBrickHitted[3]),
                float(isBrickHitted[4]),
                float(isBrickHitted[5]),
                float(isBrickHitted[6]),
                float(isBrickHitted[7]),
                float(isBrickHitted[8]),
                float(isBrickHitted[9]),
                float(isBrickHitted[10]),
                float(isBrickHitted[11])],dtype = np.float32)
                # MAL GIBI YAPTIM
        return state
        
    
    def remember(self,state,action,reward,next_state,done):
        self.memory.append([state,action,reward,next_state,done]) #popleft if MAX_MEMORY is reached    
        
    def train_long_memory(self):
        if len(self.memory) > BATCH_SIZE:
            mini_sample = random.sample(self.memory, BATCH_SIZE) # list of tuples
        else:
            mini_sample = self.memory            
            
        states,actions,rewards,next_states,dones = zip(*mini_sample)        
        self.trainer.train_step(states,actions,rewards,next_states,dones)        
        
    def train_short_memory(self,state,action,reward,next_state,done):
        self.trainer.train_step(state,action,reward,next_state,done)
    
    def get_action(self,state):
        self.epsilon = 80 - self.n_games
        final_move = [0,0,0]        
        if random.randint(0,200) < self.epsilon:
            move = random.randint(0,2)
            final_move[move] = 1
        else:
            state0 = torch.tensor(state, dtype = torch.float)
            prediction = self.model(state0)   
            move = torch.argmax(prediction).item()
            final_move[move] = 1
        return final_move
    
    
def train():
    plot_scores = []
    plot_mean_scores = []
    total_score = 0
    record = 0
    agent = Agent()
    game = UnityToGymWrapper(UnityEnvironment(),allow_multiple_obs=True)
    
    while True:
        state_old = agent.get_state(game)
        
        final_move = agent.get_action(state_old)
        
        observation, reward, terminated, info = game.step(np.argmax(final_move).item()) 
        agent.observation = observation[0]
        done = terminated
        score = agent.observation[1]
        
        state_new = agent.get_state(game)
        
        agent.train_short_memory(state_old,final_move,reward,state_new,done)
        
        agent.remember(state_old, final_move, reward, state_new, done)
        
        if done:
            game.reset()
            agent.n_games += 1
            agent.train_long_memory()
            
            if  score > record:
                if not plot_scores:
                    pass
                else:
                    record = max(plot_scores)
                agent.model.save()
            print('Game: ', agent.n_games, 'Score: ,', score, 'Record: ', record)
            
            # plot_scores.append(score)
            # total_score += score 
            # mean_score = total_score / agent.n_games
            # plot_mean_scores.append(mean_score)
            # plot(plot_scores,plot_mean_scores)
            
        
    
    
if __name__ =="__main__":
    train()
