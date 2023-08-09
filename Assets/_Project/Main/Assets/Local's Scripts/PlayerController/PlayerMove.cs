using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMove : NetworkBehaviour
{
    [Header("Other Scripts")]
    //scripts
    [SerializeField]
    private GameController _gameControllerScript;

    [SerializeField] private PlayerAnimator _playerAnimator;
    [SerializeField] private CameraShaking _camShaking;

    [Header("Per move speed")]
    //move speed
    public float moveToGridSpeed = 5f;

    [Header("Grids")]
    //current grid and grids around player
    public GameObject currentGrid;
    public GameObject leftGrid;
    public GameObject rightGrid;
    public GameObject upperGrid;
    public GameObject upperLeftGrid;
    public GameObject upperRightGrid;
    public GameObject lowerGrid;
    public GameObject lowerLeftGrid;
    public GameObject lowerRightGrid;

    [Header("Where character looking")]
    //where the character looking at
    public bool facingUp;

    public bool facingDown;
    public bool facingLeft;
    public bool facingRight;

    [Header("Where can move")] public bool canMoveUp;
    public bool canMoveDown;
    public bool canMoveLeft;
    public bool canMoveRight;

    [Header("Offline move")]
    //Only offline _ableToPressMove and myturn
    public bool diceRollPhase;
    public bool movePhase;
    public bool ableToPressMove;

    [Header("Move indicators")]
    //player direction indicator
    public GameObject _indiUp;
    public GameObject _indiDown;
    public GameObject _indiLeft;
    public GameObject _indiRight;
    void Awake()
    {
        _gameControllerScript = GameObject.Find("Game Controller").GetComponent<GameController>();
        _playerAnimator = GetComponent<PlayerAnimator>();
        _camShaking = GameObject.Find("Main Camera").GetComponent<CameraShaking>();
    }

    // Start is called before the first frame update
    void Start()
    {
        ableToPressMove = false;

        //Check Where Character Facing (Just for game start state)
        CheckMyGrid();
        if (currentGrid.name == _gameControllerScript.mapTopLeftGrid.name)
        {
            facingRight = true;
            facingDown = facingUp = facingLeft = false;
        }
        else if (currentGrid.name == _gameControllerScript.mapTopRightGrid.name)
        {
            facingDown = true;
            facingLeft = facingRight = facingUp = false;
        }
        else if (currentGrid.name == _gameControllerScript.mapBottomLeftGrid.name)
        {
            facingUp = true;
            facingDown = facingLeft = facingRight = false;
        }
        else if (currentGrid.name == _gameControllerScript.mapBottomRightGrid.name)
        {
            facingLeft = true;
            facingDown = facingRight = facingUp = false;
        }

        _gameControllerScript.CheckPlayersGrid();
        _playerAnimator.CheckFacing();
        CheckGridsAroundPlayer();
        CheckWhereCanMove();
    }

    // Update is called once per frame
    void Update()
    {
        if (movePhase && ableToPressMove)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                if (canMoveUp)
                {
                    MoveUp();
                }
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                if (canMoveDown)
                {
                    MoveDown();
                }
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                if (canMoveLeft)
                {
                    MoveLeft();
                }
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                if (canMoveRight)
                {
                    MoveRight();
                }
            }
        }
    }

    //Move after pressed
    private void MoveUp()
    {
        StartCoroutine(Moving(upperGrid.transform.position));
        facingUp = true;
        facingDown = facingLeft = facingRight = false;
        _playerAnimator.ChangeAnim("Up_Walk");
        _indiUp.SetActive(false);
        _indiDown.SetActive(false);
        _indiLeft.SetActive(false);
        _indiRight.SetActive(false);
    }

    private void MoveDown()
    {
        StartCoroutine(Moving(lowerGrid.transform.position));
        facingDown = true;
        facingUp = facingLeft = facingRight = false;
        _playerAnimator.ChangeAnim("Down_Walk");
        _indiUp.SetActive(false);
        _indiDown.SetActive(false);
        _indiLeft.SetActive(false);
        _indiRight.SetActive(false);
    }

    private void MoveLeft()
    {
        StartCoroutine(Moving(leftGrid.transform.position));
        facingLeft = true;
        facingDown = facingUp = facingRight = false;
        _playerAnimator.ChangeAnim("Left_Walk");
        _indiUp.SetActive(false);
        _indiDown.SetActive(false);
        _indiLeft.SetActive(false);
        _indiRight.SetActive(false);
    }

    private void MoveRight()
    {
        StartCoroutine(Moving(rightGrid.transform.position));
        facingRight = true;
        facingUp = facingDown = facingLeft = false;
        _playerAnimator.ChangeAnim("Right_Walk");
        _indiUp.SetActive(false);
        _indiDown.SetActive(false);
        _indiLeft.SetActive(false);
        _indiRight.SetActive(false);
    }

    IEnumerator Moving(Vector2 target)
    {
        Vector2 current = transform.position;
        Vector2 direction = target - current;
        ableToPressMove = false;

        while (Vector2.Distance(current, target) >= 0.01)
        {
            current = transform.position;
            transform.position = Vector2.MoveTowards(transform.position, target, moveToGridSpeed * Time.deltaTime);
            yield return new WaitForSeconds(0.001f);
        }

        CheckMyGrid();
        CheckGridsAroundPlayer();
        _playerAnimator.CheckFacing();
        _gameControllerScript.CheckPlayersGrid();
        CheckWhereCanMove();
        AfterMove();
    }

    IEnumerator Knockback(Vector2 target)
    {
        Vector2 current = transform.position;
        Vector2 direction = target - current;
        _camShaking.ShakeTheScreen(0.5f, 0.3f);

        _indiUp.GetComponent<SpriteRenderer>().enabled = _indiDown.GetComponent<SpriteRenderer>().enabled =
            _indiLeft.GetComponent<SpriteRenderer>().enabled =
                _indiRight.GetComponent<SpriteRenderer>().enabled = false;
        while (Vector2.Distance(current, target) >= 0.01)
        {
            current = transform.position;
            transform.position = Vector2.MoveTowards(transform.position, target, 5 * Time.deltaTime);
            yield return new WaitForSeconds(0.001f);
        }
        
        CheckMyGrid();

        if (!currentGrid.CompareTag("Hole"))
        {
            CheckGridsAroundPlayer();
            _gameControllerScript.CheckPlayersGrid();
            CheckWhereCanMove();
            _indiUp.GetComponent<SpriteRenderer>().enabled = _indiDown.GetComponent<SpriteRenderer>().enabled =
                _indiLeft.GetComponent<SpriteRenderer>().enabled =
                    _indiRight.GetComponent<SpriteRenderer>().enabled = true;
        }
        else if (currentGrid.CompareTag("Hole"))
        {
            if (this.gameObject.name == "Player1")
            {
                _gameControllerScript.PlayerDefeated(1, "Hit Hole");
            }
            else if (this.gameObject.name == "Player2")
            {
                _gameControllerScript.PlayerDefeated(2,"Hit Hole");
            }
        }
    }

    public void AfterMove()
    {
        _gameControllerScript.DicePointChange(-1);

        if (_gameControllerScript.currentDicePoint != 0)
        {
            ableToPressMove = true;
        }
        //after last move
        else if (_gameControllerScript.currentDicePoint == 0)
        {
            ableToPressMove = false;
            if (this.gameObject.name == "Player1")
            {
                if (facingUp && _gameControllerScript.player2CurrentGrid == upperGrid)
                {
                    //Hit
                    //HP decrease
                    _gameControllerScript.PlayerHealthChange(2, -1);

                    if (_gameControllerScript.player2PlayerMove.upperGrid.tag != "Wall")
                    {
                        //Knockback
                        _gameControllerScript.player2PlayerMove.StartCoroutine(
                            _gameControllerScript.player2PlayerMove.Knockback(_gameControllerScript.player2PlayerMove
                                .upperGrid.transform.position));
                    }
                }
                else if (facingDown && _gameControllerScript.player2CurrentGrid == lowerGrid)
                {
                    //Hit
                    //HP decrease
                    _gameControllerScript.PlayerHealthChange(2, -1);

                    if (_gameControllerScript.player2PlayerMove.lowerGrid.tag != "Wall")
                    {
                        //Knockback
                        _gameControllerScript.player2PlayerMove.StartCoroutine(
                            _gameControllerScript.player2PlayerMove.Knockback(_gameControllerScript.player2PlayerMove
                                .lowerGrid.transform.position));
                    }
                }
                else if (facingLeft && _gameControllerScript.player2CurrentGrid == leftGrid)
                {
                    //Hit
                    //HP decrease
                    _gameControllerScript.PlayerHealthChange(2, -1);

                    if (_gameControllerScript.player2PlayerMove.leftGrid.tag != "Wall")
                    {
                        //Knockback
                        _gameControllerScript.player2PlayerMove.StartCoroutine(
                            _gameControllerScript.player2PlayerMove.Knockback(_gameControllerScript.player2PlayerMove
                                .leftGrid.transform.position));
                    }
                }
                else if (facingRight && _gameControllerScript.player2CurrentGrid == rightGrid)
                {
                    //Hit
                    //HP decrease
                    _gameControllerScript.PlayerHealthChange(2, -1);

                    if (_gameControllerScript.player2PlayerMove.rightGrid.tag != "Wall")
                    {
                        //Knockback
                        _gameControllerScript.player2PlayerMove.StartCoroutine(
                            _gameControllerScript.player2PlayerMove.Knockback(_gameControllerScript.player2PlayerMove
                                .rightGrid.transform.position));
                    }
                }
            }
            else if (this.gameObject.name == "Player2")
            {
                if (facingUp && _gameControllerScript.player1CurrentGrid == upperGrid)
                {
                    //Hit
                    //HP decrease
                    _gameControllerScript.PlayerHealthChange(1, -1);

                    if (_gameControllerScript.player1PlayerMove.upperGrid.tag != "Wall")
                    {
                        //Knockback
                        _gameControllerScript.player1PlayerMove.StartCoroutine(
                            _gameControllerScript.player1PlayerMove.Knockback(_gameControllerScript.player1PlayerMove
                                .upperGrid.transform.position));
                    }
                }
                else if (facingDown && _gameControllerScript.player1CurrentGrid == lowerGrid)
                {
                    //Hit
                    //HP decrease
                    _gameControllerScript.PlayerHealthChange(1, -1);

                    if (_gameControllerScript.player1PlayerMove.lowerGrid.tag != "Wall")
                    {
                        //Knockback
                        _gameControllerScript.player1PlayerMove.StartCoroutine(
                            _gameControllerScript.player1PlayerMove.Knockback(_gameControllerScript.player1PlayerMove
                                .lowerGrid.transform.position));
                    }
                }
                else if (facingLeft && _gameControllerScript.player1CurrentGrid == leftGrid)
                {
                    //Hit
                    //HP decrease
                    _gameControllerScript.PlayerHealthChange(1, -1);

                    if (_gameControllerScript.player1PlayerMove.leftGrid.tag != "Wall")
                    {
                        //Knockback
                        _gameControllerScript.player1PlayerMove.StartCoroutine(
                            _gameControllerScript.player1PlayerMove.Knockback(_gameControllerScript.player1PlayerMove
                                .leftGrid.transform.position));
                    }
                }
                else if (facingRight && _gameControllerScript.player1CurrentGrid == rightGrid)
                {
                    //Hit
                    //HP decrease
                    _gameControllerScript.PlayerHealthChange(1, -1);

                    if (_gameControllerScript.player1PlayerMove.rightGrid.tag != "Wall")
                    {
                        //Knockback
                        _gameControllerScript.player1PlayerMove.StartCoroutine(
                            _gameControllerScript.player1PlayerMove.Knockback(_gameControllerScript.player1PlayerMove
                                .rightGrid.transform.position));
                    }
                }
            }
            if (_gameControllerScript.player1Alive && _gameControllerScript.player2Alive)
            {
                _gameControllerScript.SwitchPlayer();
            }
        }
    }

    public void CheckMyGrid()
    {
        RaycastHit2D hitGrid =
            Physics2D.Raycast(transform.position, transform.TransformDirection(Vector3.forward), 10f);

        //Check Hit Grid
        if (hitGrid)
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward), Color.green);
            Debug.Log("HIT : " + hitGrid.collider.name);
            currentGrid = (GameObject.Find(hitGrid.collider.name));
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward), Color.red);
            Debug.Log("Don't Hit");
        }
    }

    public void CheckGridsAroundPlayer()
    {
        string current;
        var num = currentGrid.name.Substring(1);
        int i = int.Parse(num);

        if (currentGrid.name.Contains("A"))
        {
            current = ("A" + i);
            upperGrid = GameObject.Find("-A" + i);
            upperLeftGrid = GameObject.Find(("-A") + (i - 1));
            upperRightGrid = GameObject.Find(("-A") + (i + 1));
            leftGrid = GameObject.Find(("A") + (i - 1));
            rightGrid = GameObject.Find(("A") + (i + 1));
            lowerGrid = GameObject.Find(("B") + i);
            lowerLeftGrid = GameObject.Find(("B") + (i - 1));
            lowerRightGrid = GameObject.Find("B" + (i + 1));
        }
        else if (currentGrid.name.Contains("B"))
        {
            current = ("B" + i);
            upperGrid = GameObject.Find("A" + i);
            upperLeftGrid = GameObject.Find("A" + (i - 1));
            upperRightGrid = GameObject.Find("A" + (i + 1));
            leftGrid = GameObject.Find(("B") + (i - 1));
            rightGrid = GameObject.Find(("B") + (i + 1));
            lowerGrid = GameObject.Find(("C") + i);
            lowerLeftGrid = GameObject.Find(("C") + (i - 1));
            lowerRightGrid = GameObject.Find("C" + (i + 1));
        }
        else if (currentGrid.name.Contains("C"))
        {
            current = ("C" + i);
            upperGrid = GameObject.Find("B" + i);
            upperLeftGrid = GameObject.Find("B" + (i - 1));
            upperRightGrid = GameObject.Find("B" + (i + 1));
            leftGrid = GameObject.Find(("C") + (i - 1));
            rightGrid = GameObject.Find(("C") + (i + 1));
            lowerGrid = GameObject.Find(("D") + i);
            lowerLeftGrid = GameObject.Find(("D") + (i - 1));
            lowerRightGrid = GameObject.Find("D" + (i + 1));
        }
        else if (currentGrid.name.Contains("D"))
        {
            current = ("D" + i);
            upperGrid = GameObject.Find("C" + i);
            upperLeftGrid = GameObject.Find("C" + (i - 1));
            upperRightGrid = GameObject.Find("C" + (i + 1));
            leftGrid = GameObject.Find(("D") + (i - 1));
            rightGrid = GameObject.Find(("D") + (i + 1));
            lowerGrid = GameObject.Find(("E") + i);
            lowerLeftGrid = GameObject.Find(("E") + (i - 1));
            lowerRightGrid = GameObject.Find("E" + (i + 1));
        }
        else if (currentGrid.name.Contains("E"))
        {
            current = ("E" + i);
            upperGrid = GameObject.Find("D" + i);
            upperLeftGrid = GameObject.Find("D" + (i - 1));
            upperRightGrid = GameObject.Find("D" + (i + 1));
            leftGrid = GameObject.Find(("E") + (i - 1));
            rightGrid = GameObject.Find(("E") + (i + 1));
            lowerGrid = GameObject.Find(("F") + i);
            lowerLeftGrid = GameObject.Find(("F") + (i - 1));
            lowerRightGrid = GameObject.Find("F" + (i + 1));
        }
        else if (currentGrid.name.Contains("F"))
        {
            current = ("F" + i);
            upperGrid = GameObject.Find("E" + i);
            upperLeftGrid = GameObject.Find("E" + (i - 1));
            upperRightGrid = GameObject.Find("E" + (i + 1));
            leftGrid = GameObject.Find(("F") + (i - 1));
            rightGrid = GameObject.Find(("F") + (i + 1));
            lowerGrid = GameObject.Find(("G") + i);
            lowerLeftGrid = GameObject.Find(("G") + (i - 1));
            lowerRightGrid = GameObject.Find("G" + (i + 1));
        }
        else if (currentGrid.name.Contains("G"))
        {
            current = ("G" + i);
            upperGrid = GameObject.Find("F" + i);
            upperLeftGrid = GameObject.Find("F" + (i - 1));
            upperRightGrid = GameObject.Find("F" + (i + 1));
            leftGrid = GameObject.Find(("G") + (i - 1));
            rightGrid = GameObject.Find(("G") + (i + 1));
            lowerGrid = GameObject.Find(("H") + i);
            lowerLeftGrid = GameObject.Find(("H") + (i - 1));
            lowerRightGrid = GameObject.Find("H" + (i + 1));
        }
        else if (currentGrid.name.Contains("H"))
        {
            current = ("H" + i);
            upperGrid = GameObject.Find("G" + i);
            upperLeftGrid = GameObject.Find("G" + (i - 1));
            upperRightGrid = GameObject.Find("G" + (i + 1));
            leftGrid = GameObject.Find(("H") + (i - 1));
            rightGrid = GameObject.Find(("H") + (i + 1));
            lowerGrid = GameObject.Find(("I") + i);
            lowerLeftGrid = GameObject.Find(("I") + (i - 1));
            lowerRightGrid = GameObject.Find("I" + (i + 1));
        }
        else if (currentGrid.name.Contains("I"))
        {
            current = ("I" + i);
            upperGrid = GameObject.Find("H" + i);
            upperLeftGrid = GameObject.Find("H" + (i - 1));
            upperRightGrid = GameObject.Find("H" + (i + 1));
            leftGrid = GameObject.Find(("I") + (i - 1));
            rightGrid = GameObject.Find(("I") + (i + 1));
            lowerGrid = GameObject.Find(("J") + i);
            lowerLeftGrid = GameObject.Find(("J") + (i - 1));
            lowerRightGrid = GameObject.Find("J" + (i + 1));
        }
        else if (currentGrid.name.Contains("J"))
        {
            current = ("J" + i);
            upperGrid = GameObject.Find("I" + i);
            upperLeftGrid = GameObject.Find("I" + (i - 1));
            upperRightGrid = GameObject.Find("I" + (i + 1));
            leftGrid = GameObject.Find(("J") + (i - 1));
            rightGrid = GameObject.Find(("J") + (i + 1));
            lowerGrid = GameObject.Find(("K") + i);
            lowerLeftGrid = GameObject.Find(("K") + (i - 1));
            lowerRightGrid = GameObject.Find("K" + (i + 1));
        }
    }

    public void CheckWhereCanMove()
    {
        if (facingUp)
        {
            if (upperGrid != null && _gameControllerScript.player1CurrentGrid.name != upperGrid.name &&
                _gameControllerScript.player2CurrentGrid.name != upperGrid.name && upperGrid.tag != "Hole" &&
                upperGrid.tag != "Wall")
            {
                canMoveUp = true;
            }
            else if (upperGrid == null || _gameControllerScript.player1CurrentGrid.name == upperGrid.name ||
                     _gameControllerScript.player2CurrentGrid.name == upperGrid.name || upperGrid.tag == "Hole" ||
                     upperGrid.tag != "Wall")
            {
                canMoveUp = false;
            }

            canMoveDown = false;

            if (leftGrid != null && _gameControllerScript.player1CurrentGrid.name != leftGrid.name &&
                _gameControllerScript.player2CurrentGrid.name != leftGrid.name && leftGrid.tag != "Hole" &&
                leftGrid.tag != "Wall")
            {
                canMoveLeft = true;
            }
            else if (leftGrid == null || _gameControllerScript.player1CurrentGrid.name == leftGrid.name ||
                     _gameControllerScript.player2CurrentGrid.name == leftGrid.name || leftGrid.tag == "Hole" ||
                     leftGrid.tag != "Wall")
            {
                canMoveLeft = false;
            }

            if (rightGrid != null && _gameControllerScript.player1CurrentGrid.name != rightGrid.name &&
                _gameControllerScript.player2CurrentGrid.name != rightGrid.name && rightGrid.tag != "Hole" &&
                rightGrid.tag != "Wall")
            {
                canMoveRight = true;
            }
            else if (rightGrid == null || _gameControllerScript.player1CurrentGrid.name == rightGrid.name ||
                     _gameControllerScript.player2CurrentGrid.name == rightGrid.name || rightGrid.tag == "Hole" ||
                     rightGrid.tag != "Wall")
            {
                canMoveRight = false;
            }

            if (!canMoveUp && !canMoveDown && !canMoveLeft && !canMoveRight)
            {
                canMoveDown = true;
            }

            _indiUp.SetActive(true);
            _indiDown.SetActive(false);
            _indiLeft.SetActive(false);
            _indiRight.SetActive(false);
        }
        else if (facingDown)
        {
            canMoveUp = false;

            if (lowerGrid != null && _gameControllerScript.player1CurrentGrid.name != lowerGrid.name &&
                _gameControllerScript.player2CurrentGrid.name != lowerGrid.name && lowerGrid.tag != "Hole" &&
                lowerGrid.tag != "Wall")
            {
                canMoveDown = true;
            }
            else if (lowerGrid == null || _gameControllerScript.player1CurrentGrid.name == lowerGrid.name ||
                     _gameControllerScript.player2CurrentGrid.name == lowerGrid.name || lowerGrid.tag == "Hole" ||
                     lowerGrid.tag != "Wall")
            {
                canMoveDown = false;
            }

            if (leftGrid != null && _gameControllerScript.player1CurrentGrid.name != leftGrid.name &&
                _gameControllerScript.player2CurrentGrid.name != leftGrid.name && leftGrid.tag != "Hole" &&
                leftGrid.tag != "Wall")
            {
                canMoveLeft = true;
            }
            else if (leftGrid == null || _gameControllerScript.player1CurrentGrid.name == leftGrid.name ||
                     _gameControllerScript.player2CurrentGrid.name == leftGrid.name || leftGrid.tag == "Hole" ||
                     leftGrid.tag != "Wall")
            {
                canMoveLeft = false;
            }

            if (rightGrid != null && _gameControllerScript.player1CurrentGrid.name != rightGrid.name &&
                _gameControllerScript.player2CurrentGrid.name != rightGrid.name && rightGrid.tag != "Hole" &&
                rightGrid.tag != "Wall")
            {
                canMoveRight = true;
            }
            else if (rightGrid == null || _gameControllerScript.player1CurrentGrid.name == rightGrid.name ||
                     _gameControllerScript.player2CurrentGrid.name == rightGrid.name || rightGrid.tag == "Hole" ||
                     rightGrid.tag != "Wall")
            {
                canMoveRight = false;
            }

            if (!canMoveUp && !canMoveDown && !canMoveLeft && !canMoveRight)
            {
                canMoveUp = true;
            }

            _indiUp.SetActive(false);
            _indiDown.SetActive(true);
            _indiLeft.SetActive(false);
            _indiRight.SetActive(false);
        }
        else if (facingLeft)
        {
            if (upperGrid != null && _gameControllerScript.player1CurrentGrid.name != upperGrid.name &&
                _gameControllerScript.player2CurrentGrid.name != upperGrid.name && upperGrid.tag != "Hole" &&
                upperGrid.tag != "Wall")
            {
                canMoveUp = true;
            }
            else if (upperGrid == null || _gameControllerScript.player1CurrentGrid.name == upperGrid.name ||
                     _gameControllerScript.player2CurrentGrid.name == upperGrid.name || upperGrid.tag == "Hole" ||
                     upperGrid.tag != "Wall")
            {
                canMoveUp = false;
            }

            if (lowerGrid != null && _gameControllerScript.player1CurrentGrid.name != lowerGrid.name &&
                _gameControllerScript.player2CurrentGrid.name != lowerGrid.name && lowerGrid.tag != "Hole" &&
                lowerGrid.tag != "Wall")
            {
                canMoveDown = true;
            }
            else if (lowerGrid == null || _gameControllerScript.player1CurrentGrid.name == lowerGrid.name ||
                     _gameControllerScript.player2CurrentGrid.name == lowerGrid.name || lowerGrid.tag == "Hole" ||
                     lowerGrid.tag != "Wall")
            {
                canMoveDown = false;
            }

            if (leftGrid != null && _gameControllerScript.player1CurrentGrid.name != leftGrid.name &&
                _gameControllerScript.player2CurrentGrid.name != leftGrid.name && leftGrid.tag != "Hole" &&
                leftGrid.tag != "Wall")
            {
                canMoveLeft = true;
            }
            else if (leftGrid == null || _gameControllerScript.player1CurrentGrid.name == leftGrid.name ||
                     _gameControllerScript.player2CurrentGrid.name == leftGrid.name || leftGrid.tag == "Hole" ||
                     leftGrid.tag != "Wall")
            {
                canMoveLeft = false;
            }

            canMoveRight = false;

            if (!canMoveUp && !canMoveDown && !canMoveLeft && !canMoveRight)
            {
                canMoveRight = true;
            }

            _indiUp.SetActive(false);
            _indiDown.SetActive(false);
            _indiLeft.SetActive(true);
            _indiRight.SetActive(false);
        }
        else if (facingRight)
        {
            if (upperGrid != null && _gameControllerScript.player1CurrentGrid.name != upperGrid.name &&
                _gameControllerScript.player2CurrentGrid.name != upperGrid.name && upperGrid.tag != "Hole" &&
                upperGrid.tag != "Wall")
            {
                canMoveUp = true;
            }
            else if (upperGrid == null || _gameControllerScript.player1CurrentGrid.name == upperGrid.name ||
                     _gameControllerScript.player2CurrentGrid.name == upperGrid.name || upperGrid.tag == "Hole" ||
                     upperGrid.tag != "Wall")
            {
                canMoveUp = false;
            }

            if (lowerGrid != null && _gameControllerScript.player1CurrentGrid.name != lowerGrid.name &&
                _gameControllerScript.player2CurrentGrid.name != lowerGrid.name && lowerGrid.tag != "Hole" &&
                lowerGrid.tag != "Wall")
            {
                canMoveDown = true;
            }
            else if (lowerGrid == null || _gameControllerScript.player1CurrentGrid.name == lowerGrid.name ||
                     _gameControllerScript.player2CurrentGrid.name == lowerGrid.name || lowerGrid.tag == "Hole" ||
                     lowerGrid.tag != "Wall")
            {
                canMoveDown = false;
            }

            canMoveLeft = false;

            if (rightGrid != null && _gameControllerScript.player1CurrentGrid.name != rightGrid.name &&
                _gameControllerScript.player2CurrentGrid.name != rightGrid.name && rightGrid.tag != "Hole" &&
                rightGrid.tag != "Wall")
            {
                canMoveRight = true;
            }
            else if (rightGrid == null || _gameControllerScript.player1CurrentGrid.name == rightGrid.name ||
                     _gameControllerScript.player2CurrentGrid.name == rightGrid.name || rightGrid.tag == "Hole" ||
                     rightGrid.tag != "Wall")
            {
                canMoveRight = false;
            }

            if (!canMoveUp && !canMoveDown && !canMoveLeft && !canMoveRight)
            {
                canMoveLeft = true;
            }
            _indiUp.SetActive(false);
            _indiDown.SetActive(false);
            _indiLeft.SetActive(false);
            _indiRight.SetActive(true);
        }
    }
}