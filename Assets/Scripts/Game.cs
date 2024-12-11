using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking; // 添加必要的库


public class Game : MonoBehaviour
{   
    public GameObject chesspiece;

    private GameObject[,] positions = new GameObject[8, 8];
    private GameObject[] playerBlack = new GameObject[16];
    private GameObject[] playerWhite = new GameObject[16];

    private string currentPlayer = "white";

    private bool gameOver = false;

    public void Start()
    {   
        int randomIntInRange_black_x = Random.Range(0, 8);
        while (randomIntInRange_black_x == 3 || randomIntInRange_black_x == 4)
        {
            randomIntInRange_black_x = Random.Range(0, 8);
        }
        int randomIntInRange_black_y = Random.Range(3, 5);
        int randomIntInRange_white_x = 7 - randomIntInRange_black_x;
        int randomIntInRange_white_y = 7 - randomIntInRange_black_y;
        playerWhite = new GameObject[] { Create("white_rook", 0, 0), Create("white_knight", 1, 0),
            Create("white_bishop", 2, 0), Create("white_queen", 3, 0), Create("white_king", randomIntInRange_white_x, randomIntInRange_white_y),
            Create("white_bishop", 5, 0), Create("white_knight", 6, 0), Create("white_rook", 7, 0),
            Create("white_pawn", 0, 1), Create("white_pawn", 1, 1), Create("white_pawn", 2, 1),
            Create("white_pawn", 3, 1), Create("white_pawn", 4, 1), Create("white_pawn", 5, 1),
            Create("white_pawn", 6, 1), Create("white_pawn", 7, 1) };
        playerBlack = new GameObject[] { Create("black_rook", 0, 7), Create("black_knight",1,7),
            Create("black_bishop",2,7), Create("black_queen",3,7), Create("black_king",randomIntInRange_black_x,randomIntInRange_black_y),
            Create("black_bishop",5,7), Create("black_knight",6,7), Create("black_rook",7,7),
            Create("black_pawn", 0, 6), Create("black_pawn", 1, 6), Create("black_pawn", 2, 6),
            Create("black_pawn", 3, 6), Create("black_pawn", 4, 6), Create("black_pawn", 5, 6),
            Create("black_pawn", 6, 6), Create("black_pawn", 7, 6) };

        for (int i = 0; i < playerBlack.Length; i++)
        {
            SetPosition(playerBlack[i]);
            SetPosition(playerWhite[i]);
        }
    }

    public GameObject Create(string name, int x, int y)
    {
        GameObject obj = Instantiate(chesspiece, new Vector3(0, 0, -1), Quaternion.identity);
        Chessman cm = obj.GetComponent<Chessman>(); 
        cm.name = name; 
        cm.SetXBoard(x);
        cm.SetYBoard(y);
        cm.Activate(); 
        return obj;
    }

    public void SetPosition(GameObject obj)
    {
        Chessman cm = obj.GetComponent<Chessman>();

        positions[cm.GetXBoard(), cm.GetYBoard()] = obj;
    }

    public void SetPositionEmpty(int x, int y)
    {
        positions[x, y] = null;
    }

    public GameObject GetPosition(int x, int y)
    {
        return positions[x, y];
    }

    public bool PositionOnBoard(int x, int y)
    {
        if (x < 0 || y < 0 || x >= positions.GetLength(0) || y >= positions.GetLength(1)) return false;
        return true;
    }

    public string GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public bool IsGameOver()
    {
        return gameOver;
    }

    public void NextTurn()
    {
        if (currentPlayer == "white")
        {
            currentPlayer = "black";

            // // 获取棋盘状态并调用接口
            // string boardState = GetBoardState();
            // Debug.Log("GetBoardState: " + boardState);
            // StartCoroutine(GetMoveFromAPI(boardState));
        }
        else
        {
            currentPlayer = "white";
        }
    }

    public void Update()
    {
        if (gameOver == true && Input.GetMouseButtonDown(0))
        {
            gameOver = false;

            SceneManager.LoadScene("Game");
        }
    }
    
    public void Winner(string playerWinner)
    {
        gameOver = true;

        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().enabled = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().text = playerWinner + " is the winner";

        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().enabled = true;
    }
    public IEnumerator GetMoveFromAPI(string boardState)
    {
        string url = "https://api-inference.huggingface.co/models/meta-llama/Llama-3.2-3B-Instruct/v1/chat/completions";
        string hfToken = "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"; // 替换为你的 Hugging Face Token
        string sanitizedBoardState = boardState.Replace("\n", "\\n").Replace("\r", "").Replace("\"", "\\\"");
        // 手动构建 JSON 请求体
        string json = $@"
        {{
            ""model"": ""mistralai/Mistral-7B-Instruct-v0.3"",
            ""messages"": [
                {{
                    ""role"": ""user"",
                    ""content"": ""The format of the board is a1, a2 ....Here is the board state:\n{sanitizedBoardState}\nThe left bottom corner is white_rook. What is the best move for black?Your response should follow a strict example format and do not provide any reason \nexample\nMove from e2 to e4.\n""
                }}
            ],
            ""temperature"": 0.5,
            ""max_tokens"": 2048,
            ""top_p"": 0.7,
            ""stream"": false
        }}";
        Debug.Log("Generated JSON: " + json);
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", $"Bearer {hfToken}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("API Response: " + responseText);
            ProcessBlackMove(responseText);
        }
        else
        {
            Debug.LogError("Failed to get move from API: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }


    private void ProcessBlackMove(string moveData)
    {
        // 假设返回数据包含一条消息内容，提取建议的棋步
        var response = JsonUtility.FromJson<ResponseFormat>(moveData);
        string moveContent = response.choices[0].message.content;

        Debug.Log("Move content: " + moveContent);

        // 解析棋步（e.g., "Move from e2 to e4"）
        string[] parts = moveContent.Split(' ');
        if (parts.Length >= 5)
        {
            string from = parts[2];
            string to = parts[4];

            int fromX = from[0] - 'a'; // 将字母转换为棋盘 x 坐标
            int fromY = from[1] - '1'; // 将数字转换为棋盘 y 坐标
            int toX = to[0] - 'a';
            int toY = to[1] - '1';

            GameObject piece = GetPosition(fromX, fromY);
            if (piece != null)
            {
                SetPositionEmpty(fromX, fromY);
                piece.GetComponent<Chessman>().SetXBoard(toX);
                piece.GetComponent<Chessman>().SetYBoard(toY);
                piece.GetComponent<Chessman>().SetCoords();
                SetPosition(piece);

                NextTurn(); // 切换回合
            }
        }
        else
        {
            Debug.LogError("Invalid move format from API");
        }
    }

    // 定义用于解析 JSON 的类
    [System.Serializable]
    private class ResponseFormat
    {
        public Choice[] choices;
    }

    [System.Serializable]
    private class Choice
    {
        public Message message;
    }

    [System.Serializable]
    private class Message
    {
        public string role;
        public string content;
    }
    private string GetBoardState()
    {
        string boardState = "";

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                GameObject piece = GetPosition(x, y);
                if (piece != null)
                {
                    boardState += piece.name + ",";
                }
                else
                {
                    boardState += "empty,";
                }
            }
            boardState = boardState.TrimEnd(',') + "\n";
        }

        return boardState.TrimEnd('\n');
    }
}
