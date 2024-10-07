using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class BreakOutGameAI : Agent
{
    public BreakOutGame game;
    public override void OnEpisodeBegin()
    {
        game.Reset();
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(game.ball.transform.localPosition);
        sensor.AddObservation(game.paddle.transform.localPosition);
        sensor.AddObservation(game.ballVelocity);
        sensor.AddObservation(game.ballAngle_deg*Mathf.Deg2Rad);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (actions.DiscreteActions[0] == 0)
        {
            game.selectedAction = BreakOutGameActions.Left;
        }
        else if (actions.DiscreteActions[0] == 1)
        {
            game.selectedAction = BreakOutGameActions.Stay;
        }
        else
        {
            game.selectedAction = BreakOutGameActions.Right;
        }


        SetReward(game.score);


        if (game.gameStatus == GameStatus.Lose)
        {
            EndEpisode();
        }
        else if (game.gameStatus == GameStatus.Win)
        {
            EndEpisode();
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 1;
        if (Input.GetKey(KeyCode.A))
        {
            discreteActions[0] = 0;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActions[0] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActions[0] = 2;
        }
    }

}
