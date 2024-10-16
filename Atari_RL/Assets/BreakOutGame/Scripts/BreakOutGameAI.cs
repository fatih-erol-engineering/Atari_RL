using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Rendering;

public class BreakOutGameAI : Agent
{
    public BreakOutGame game;
    public Camera renderCamera;
    private float prevScore;
    public float[] isBrickHitted;
    public override void OnEpisodeBegin()
    {
        //Time.timeScale = 2f;
        prevScore = game.score;
        game.Reset();
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(game.timeSec);
        //sensor.AddObservation(game.score);
        //sensor.AddObservation(game.ball.transform.localPosition);
        //sensor.AddObservation(game.paddle.transform.localPosition);
        //sensor.AddObservation(game.ballVelocity);
        //sensor.AddObservation(game.ballAngle_deg*Mathf.Deg2Rad);
        //isBrickHitted = new float[game.numberOfBricks[0]* game.numberOfBricks[1]];
        //for (int i = 0; i < isBrickHitted.Length; i++)
        //{
        //    if (game.bricks[i] == null)
        //    {
        //        isBrickHitted[i] = 1f;
        //    }
        //    else
        //    {
        //        isBrickHitted[i] = 0f;
        //    }
        //}
        //sensor.AddObservation(isBrickHitted);

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


        SetReward(game.score - prevScore);

        if (game.gameStatus == GameStatus.Lose)
        {
            EndEpisode();
            SetReward(-10f);
        }
        else if (game.gameStatus == GameStatus.Win)
        {
            EndEpisode();
            SetReward(50f);
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

    public void FixedUpdate()
    {
        WaitTimeInference();
    }

    void WaitTimeInference()
    {
        if (renderCamera != null && SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null)
        {
            renderCamera.Render();
        }

        if (Academy.Instance.IsCommunicatorOn)
        {
            RequestDecision();
        }
    }

}
