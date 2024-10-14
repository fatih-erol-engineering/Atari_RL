using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class BreakOutGame : MonoBehaviour
{
    
    [Header("Settings")]
    public Vector2Int numberOfBricks = new Vector2Int(3, 4);
    public GameStatus gameStatus = GameStatus.Continue;

    [Header("Prefabs")]
    public GameObject gameArea;
    public GameObject paddle;
    public GameObject ball;
    public GameObject brickPrefab;
    public GameObject brickHolderGameObject;
    public float brickBorder;
    public GameObject[] bricks;
    

    [Header("Action")]
    public BreakOutGameActions selectedAction = BreakOutGameActions.Stay;
    public float paddleVelocity=0.5f;
    public float score = 0;

    [Header("Ball")]
    public float initialBallAngle_deg = 33;
    public float ballVelocity = 0.5f;
    public float ballAngle_deg = 33;

    [Header("Displays")]
    public TextMeshProUGUI scoreTMP;
    public TextMeshProUGUI gameStatusTMP;

    [Header("Time Info")]
    public float timeSec = 0f;

    private void Start()
    {
        Time.timeScale = 4f;

        // FPS'yi 30'da sabitle
        //Application.targetFrameRate = 15;
        Reset();        
    }
    void FixedUpdate()
    {
        if (gameStatus == GameStatus.Continue)
        {
            UpdateBrickPositions();
            MovePaddle(selectedAction);
            MoveBall();
            UpdateTMPs();
            timeSec += Time.fixedDeltaTime;
        }
    }
    public void MoveBall()
    {
        float velX = ballVelocity*Mathf.Sin(Mathf.Deg2Rad*ballAngle_deg);
        float velY = ballVelocity * Mathf.Cos(Mathf.Deg2Rad * ballAngle_deg);
        ball.transform.Translate(velX * Time.fixedDeltaTime, velY * Time.fixedDeltaTime, 0f);

        // Check Collision with Walls
        float ballWidth = ball.transform.GetComponent<SpriteRenderer>().sprite.texture.width;
        float ballHeight = ball.transform.GetComponent<SpriteRenderer>().sprite.texture.height;


        float canvasWidth = gameArea.transform.GetComponent<SpriteRenderer>().sprite.texture.width;
        float canvasHeight = gameArea.transform.GetComponent<SpriteRenderer>().sprite.texture.height;

        Vector2 limitX = new Vector2(-((canvasWidth - ballWidth) / 2 / 100), ((canvasWidth - ballWidth) / 2 / 100));
        Vector2 limitY = new Vector2(-((canvasHeight - ballHeight) / 2 / 100), ((canvasHeight - ballHeight) / 2 / 100));

        bool isAtBottomWall = (ball.transform.localPosition.y <= limitY[0]);
        bool isAtTopWall = (ball.transform.localPosition.y >= limitY[1]);
        bool isAtTopOrBottomWall = isAtTopWall || isAtBottomWall;

        bool isAtRightWall = (ball.transform.localPosition.x >= limitX[1]);
        bool isAtLeftWall = (ball.transform.localPosition.x <= limitX[0]);
        bool isAtRightOrLeftWall = isAtRightWall || isAtLeftWall;

        if (isAtTopOrBottomWall)
        {
            ballAngle_deg = 180 - ballAngle_deg;
        }

        if (isAtRightOrLeftWall)
        {
            ballAngle_deg = -ballAngle_deg;
        }

        // Check Collision with Paddle

        float paddleWidth = paddle.transform.GetComponent<SpriteRenderer>().sprite.texture.width;
        float paddleHeight = paddle.transform.GetComponent<SpriteRenderer>().sprite.texture.height;

        bool isAtPaddleX = ((paddle.transform.localPosition.x - paddleWidth / 2/100) <= ball.transform.localPosition.x) && ((paddle.transform.localPosition.x + paddleWidth / 2 / 100) >= ball.transform.localPosition.x);
        bool isAtPaddleY = ((paddle.transform.localPosition.y - paddleHeight / 2 / 100) <= ball.transform.localPosition.y) && ((paddle.transform.localPosition.y + paddleHeight/ 2 / 100) >= ball.transform.localPosition.y);
        bool isAtPaddle = isAtPaddleX && isAtPaddleY;

        if (isAtPaddle)
        {
            ballAngle_deg = 180 - ballAngle_deg;
        }

        // Check Collision with Bricks

        bool isAllNull = true;
        foreach (GameObject brick in bricks)
        {
            if (brick != null)
            {
                float brickWidth = brick.transform.GetComponent<SpriteRenderer>().sprite.texture.width;
                float brickHeight = brick.transform.GetComponent<SpriteRenderer>().sprite.texture.height;

                bool isAtBrickX = ((brick.transform.localPosition.x - brickWidth * brick.transform.localScale.x / 2 / 100)  <= ball.transform.localPosition.x) && ((brick.transform.localPosition.x + brickWidth * brick.transform.localScale.x / 2 / 100) >= ball.transform.localPosition.x);
                bool isAtBrickY = ((brick.transform.localPosition.y - brickHeight * brick.transform.localScale.y / 2 / 100)  <= ball.transform.localPosition.y) && ((brick.transform.localPosition.y + brickHeight * brick.transform.localScale.y / 2 / 100) >= ball.transform.localPosition.y);
                bool isAtBrick = isAtBrickX && isAtBrickY;

                if (isAtBrick)
                {
                    ballAngle_deg = 180 - ballAngle_deg;
                    score++;
                    Destroy(brick);
                }
                isAllNull = false;
            }
        }

        if (isAtBottomWall)
        {
            gameStatus = GameStatus.Lose;
            //gameArea.transform.GetComponent<SpriteRenderer>().color = Color.red;
        }

        if(isAllNull)
        {
            gameStatus = GameStatus.Win;
            //gameArea.transform.GetComponent<SpriteRenderer>().color = Color.green;
        }
    }
    public void Reset()
    {
        gameStatus = GameStatus.Continue;
        selectedAction = BreakOutGameActions.Stay;
        score = 0;
        timeSec = 0f;
        StartBallRandomPosAndRot();
        CreateBricks();
    }
    public void ResetPaddle()
    {
        paddle.transform.localPosition = new Vector3(0, paddle.transform.localPosition.y, paddle.transform.localPosition.z);
        selectedAction = BreakOutGameActions.Stay;
    }
    public void MovePaddle(BreakOutGameActions action)
    {
        float deltaMoveX = 0;

        // check if paddle outside of the canvas
        float paddleWidth = paddle.transform.GetComponent<SpriteRenderer>().sprite.texture.width;
        float canvasWidth = gameArea.transform.GetComponent<SpriteRenderer>().sprite.texture.width;

        Vector2 limitX = new Vector2(-((canvasWidth - paddleWidth )/ 2 / 100), ((canvasWidth - paddleWidth) / 2 / 100)) ;

        bool isAtRightLimit = false;
        bool isAtLeftLimit = false;
        


        switch (action)
        {            
            case BreakOutGameActions.Right:
                if (isAtRightLimit)
                {
                   deltaMoveX = 0;
                }
                else
                {
                    deltaMoveX = paddleVelocity*Time.fixedDeltaTime;                                    
                }
                break;
            case BreakOutGameActions.Left:
                if (isAtLeftLimit)
                {
                    deltaMoveX = 0;
                }
                else
                {
                    deltaMoveX = paddleVelocity * Time.fixedDeltaTime * (-1);
                }

                break;
            case BreakOutGameActions.Stay:
                deltaMoveX = 0;
                break;
        }
        paddle.transform.Translate(deltaMoveX, 0f, 0f);

        if (paddle.transform.localPosition.x >= limitX[1])
        {
            isAtRightLimit = true;
            paddle.transform.localPosition = new Vector3(limitX[1], paddle.transform.localPosition.y, paddle.transform.localPosition.z);
        }

        if (paddle.transform.localPosition.x <= limitX[0])
        {
            isAtLeftLimit = true;
            paddle.transform.localPosition = new Vector3(limitX[0], paddle.transform.localPosition.y, paddle.transform.localPosition.z);
        }


    }
    public void CreateBricks()
    {
        for (int i = 0; i < brickHolderGameObject.transform.childCount; i++)
        {
            Destroy(brickHolderGameObject.transform.GetChild(i).gameObject);
        }
        int brickNumber = numberOfBricks.x * numberOfBricks.y;

        bricks = new GameObject[brickNumber];

        for (int i = 0; i < bricks.Length; i++)
        {
            bricks[i] = Instantiate(brickPrefab, brickHolderGameObject.transform);
        }

        UpdateBrickPositions();
    }
    public void UpdateBrickPositions()
    {

        // Determine the size of bricks
        if (gameArea != null)
        {
            float canvasWidth = gameArea.GetComponent<SpriteRenderer>().sprite.texture.width;
            float canvasHeight = gameArea.GetComponent<SpriteRenderer>().sprite.texture.height;

            if (bricks != null)
            {
                float brickWidth  = 0; // PlaceHolder
                float brickHeight = 0; // PlaceHolder

                int counter = 0;

                brickWidth = brickPrefab.GetComponent<SpriteRenderer>().sprite.texture.width;
                brickHeight = brickPrefab.GetComponent<SpriteRenderer>().sprite.texture.height;

                float neededWidth = canvasWidth / numberOfBricks.y;
                float neededHeight = (canvasHeight / 2.5f) / numberOfBricks.x;

                float scaleWidth = neededWidth / brickWidth - brickBorder;
                float scaleHeight = neededHeight / brickHeight - brickBorder;


                float initialPosX  = (-1*canvasWidth / 2) / 100 + ((neededWidth/2) / 100);
                float initialPosY = (+1*canvasHeight / 2) / 100 - ((neededHeight / 2) / 100);

                float currentPosX = initialPosX;
                float currentPosY = initialPosY;
                for (int i = 0; i < numberOfBricks.x; i++)
                {
                    for (int j = 0; j < numberOfBricks.y; j++)
                    {
                        if (bricks[counter] != null)
                        {                        
                            bricks[counter].transform.localPosition = new Vector3(currentPosX, currentPosY, bricks[counter].transform.localPosition.z);
                            bricks[counter].transform.localScale = new Vector3(scaleWidth, scaleHeight, bricks[counter].transform.localScale.z);
                            counter++;
                            currentPosX += neededWidth / 100;                            
                        }
                    }
                    currentPosY -= neededHeight / 100;
                    currentPosX = initialPosX;
                }
            }
        }

        
    }
    public void StartBallRandomPosAndRot()
    {
        initialBallAngle_deg = Random.Range(23,43);
        ballAngle_deg = initialBallAngle_deg;
       
        float canvasWidth = gameArea.transform.GetComponent<SpriteRenderer>().sprite.texture.width;
        float ballWidth = ball.transform.GetComponent<SpriteRenderer>().sprite.texture.width;
        float ballHeight = ball.transform.GetComponent<SpriteRenderer>().sprite.texture.height;

        float paddlePosY = paddle.transform.localPosition.y;

        Vector2 limitX = new Vector2(-((canvasWidth - ballWidth) / 2 / 100), ((canvasWidth - ballWidth) / 2 / 100));
        Vector2 limitY = new Vector2(paddlePosY, (- ballHeight/100));
        float randomPosX = Random.Range(limitX[0], limitX[1]);
        float randomPosY = Random.Range(limitY[0], limitY[1]);
        ball.transform.localPosition = new Vector3(randomPosX,randomPosY,ball.transform.localPosition.z);

    }

    public void UpdateTMPs()
    {
        scoreTMP.text = "Score: " + score.ToString();
        gameStatusTMP.text = gameStatus.ToString();
    }
}

public enum BreakOutGameActions
{
    Right, Left, Stay
}
public enum GameStatus
{
    Continue,
    Pause,
    Win,
    Lose
}

