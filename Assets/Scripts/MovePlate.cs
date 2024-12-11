using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{

    public GameObject controller;

    GameObject reference = null;

    int matrixX;
    int matrixY;

    public bool attack = false;

    public void Start()
    {
        if (attack)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
    }

    public void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");

        if (attack)
        {
            GameObject cp = controller.GetComponent<Game>().GetPosition(matrixX, matrixY);

            if (cp.name == "white_king") controller.GetComponent<Game>().Winner("black");
            if (cp.name == "black_king") controller.GetComponent<Game>().Winner("white");

            Chessman chessman = reference.GetComponent<Chessman>();
            if (chessman.name.EndsWith("_pawn"))
            {
                string capturedPieceName = cp.name;
                string playerPrefix = chessman.name.StartsWith("white") ? "white_" : "black_";
                string newPieceType = capturedPieceName.Split('_')[1]; 

                chessman.name = playerPrefix + newPieceType;
                chessman.Activate();
            }
            // Destroy the captured piece
            Destroy(cp);

    
        }

        controller.GetComponent<Game>().SetPositionEmpty(reference.GetComponent<Chessman>().GetXBoard(), 
            reference.GetComponent<Chessman>().GetYBoard());

        reference.GetComponent<Chessman>().SetXBoard(matrixX);
        reference.GetComponent<Chessman>().SetYBoard(matrixY);
        reference.GetComponent<Chessman>().SetCoords();

        controller.GetComponent<Game>().SetPosition(reference);

        // 检查 Pawn 是否到达底部行
        Chessman chessman1 = reference.GetComponent<Chessman>();
        if (chessman1.name.EndsWith("_pawn") && (matrixY == 0 || matrixY == 7))
        {
            SpawnRandomPawn(chessman1.name.StartsWith("white") ? "white_pawn" : "black_pawn");
        }

        controller.GetComponent<Game>().NextTurn();

        reference.GetComponent<Chessman>().DestroyMovePlates();
        
        
    }



    // 在随机空位置生成一个新的 pawn
    private void SpawnRandomPawn(string pawnName)
    {
        Game controllerGame = controller.GetComponent<Game>();
        int randomX, randomY;

        // 随机生成一个空位置
        do
        {
            randomX = Random.Range(0, 8);
            randomY = Random.Range(0, 8);
        } while (controllerGame.GetPosition(randomX, randomY) != null); // 确保位置为空

        // 创建并放置新的 Pawn
        GameObject newPawn = controllerGame.Create(pawnName, randomX, randomY);
        controllerGame.SetPosition(newPawn);

    }

    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }

    public void SetReference(GameObject obj)
    {
        reference = obj;
    }

    public GameObject GetReference()
    {
        return reference;
    }
}
