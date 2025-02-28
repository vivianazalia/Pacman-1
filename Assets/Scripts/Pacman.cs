﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pacman : MonoBehaviour
{
    [SerializeField] private float speed;

    private Vector2 movement;
    private Vector2 nextMovement;

    [SerializeField] private GameObject startTile;

    public int Scores { get; private set; }
    public int Lives { get; private set; }

    private Node currentNode, nextNode, previousNode;

    private Pellet startPosition;

    private void Start()
    {
        startPosition = startTile.GetComponent<Pellet>();
        transform.position = startPosition.transform.position;
        currentNode = startTile.GetComponent<Node>();
        ResetMovementValue();
        ChangePosition(movement);

        Lives = 3;
    }

    private void Update()
    {
        CheckInput();
    }

    //fungsi yang dipanggil pada Game Manager
    public void Execute()
    {
        Move();
        SetScore(1);
    }

    #region Movement
    //fungsi untuk mengatur movement Pacman
    private void Move() 
    {
        if (nextNode != currentNode && nextNode != null)
        {
            if (ReachTargetNode())
            {
                currentNode = nextNode;

                transform.position = currentNode.transform.position;

                Node targetNode = GetNextNode(nextMovement);

                if(targetNode != null)
                {
                    movement = nextMovement;
                }

                if(targetNode == null)
                {
                    targetNode = GetNextNode(movement);
                } 

                if(targetNode != null)
                {
                    nextNode = targetNode;
                    previousNode = currentNode;
                    currentNode = null;
                }
                else
                {
                    movement = Vector2.zero;
                }
            }
            else
            {
                transform.position += (Vector3)(movement * speed * Time.deltaTime);
            }
        }
    }

    //fungsi untuk mengatur input movement Pacman
    private void CheckInput() 
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            ChangePosition(Vector2.up);
        } 
        else if (Input.GetKeyDown(KeyCode.D))
        {
            ChangePosition(Vector2.right);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ChangePosition(Vector2.down);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            ChangePosition(Vector2.left);
        }
    }
   
    //untuk mengambil target node di sekitar currentNode yang akan dituju
    private Node GetNextNode(Vector2 dir)
    {
        Node targetNode = null;
        
        if(currentNode.aroundNodes.Length > 1)
        {
            for (int i = 0; i < currentNode.aroundNodes.Length; i++)
            {
                //untuk fixed vector normalized yang tidak return -1, 0, atau 1
                if(currentNode.direction[i].x > 0.5f)
                {
                    currentNode.direction[i].x = 1;
                } 
                else if(currentNode.direction[i].x < -0.5f)
                {
                    currentNode.direction[i].x = -1;
                }
                else
                {
                    currentNode.direction[i].x = 0;
                }

                if (currentNode.direction[i].y > 0.5f)
                {
                    currentNode.direction[i].y = 1;
                }
                else if (currentNode.direction[i].y < -0.5f)
                {
                    currentNode.direction[i].y = -1;
                }
                else
                {
                    currentNode.direction[i].y = 0;
                }

                if (currentNode.direction[i] == dir)
                {
                    targetNode = currentNode.aroundNodes[i];
                    break;
                }
            }
        }
        else
        {
            targetNode = currentNode.aroundNodes[0];
        }

        return targetNode;
    }

    //untuk set movement value dan node berikutnya
    private void ChangePosition(Vector2 dir)
    {
        //menyimpan nilai vector movement yang telah diinputkan untuk dieksekusi setelahnya
        if(dir != movement)
        {
            nextMovement = dir;
        }

        if (currentNode != null)
        {
            Node targetNode = GetNextNode(dir);

            if(targetNode != null)
            {
                movement = dir;
                nextNode = targetNode;
                previousNode = currentNode;
                currentNode = null;
            }
        }
    }

    //fungsi yang mengembalikan nilai panjang dari posisi node sekarang ke posisi node target
    private float LengthFromNode(Vector2 targetPos)
    {
        Vector2 length = targetPos - (Vector2)previousNode.transform.position;
        return length.sqrMagnitude;
    }

    //fungsi untuk mengecek apakah pacman telah mencapai node tujuan atau belum
    private bool ReachTargetNode()
    {
        float nodeToTarget = LengthFromNode(nextNode.transform.position);
        float nodeToSelf = LengthFromNode(transform.position);

        return nodeToSelf > nodeToTarget;
    }

    //untuk reset arah movement
    public void ResetMovementValue()
    {
        movement = Vector2.right;
    }

    public Vector2 GetMovementValue()
    {
        return movement;
    }

    //untuk restart position pacman ke posisi awal
    public void RestartPosition()
    {
        transform.position = startPosition.transform.position;

        currentNode = startPosition.GetComponent<Node>();
        ResetMovementValue();
        ChangePosition(movement);
    }
    #endregion

    //setter variable score
    public void SetScore(int s)
    {
        Scores += s;
    }

    //setter lives
    void DecreaseLives(int l)
    {
        if (Lives == 0)
        {
            Lives = 0;
        }

        Lives -= l;
    }

    //fungsi built-in untuk deteksi saat collide dengan ghost, pacman kembali ke posisi awal
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ghost"))
        {
            if(collision.gameObject.GetComponent<Ghost>().currentMode == Ghost.Mode.Chase || collision.gameObject.GetComponent<Ghost>().currentMode == Ghost.Mode.Scatter)
            {
                RestartPosition();
                DecreaseLives(1);
            } 
            else if(collision.gameObject.GetComponent<Ghost>().currentMode == Ghost.Mode.Frightened)
            {
                Ghost g = collision.gameObject.GetComponent<Ghost>();
                g.ResetPosition();
                Scores += 500;
            }
        }
    }
}
