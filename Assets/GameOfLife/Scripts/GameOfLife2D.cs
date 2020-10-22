using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOfLife2D : MonoBehaviour
{
    public int gridSizeX; // x size of the grid
    public int gridSizeY; // y size of the grid

    public Sprite cellSprite; // sprite used to represent a cell on the grid
    public Text gameRunningText; // text used to display whether the game is running
    public int[,] gameArray = new int[0, 0]; // an array to hold the state data for each cell in the grid. int 0 represents dead state, int 1 represents alive state
    public int[,] gameArrayNextGen = new int[0, 0]; // an array to hold the state data for each cell in the grid for the next generation of the game
    public GameObject[,] cellGameObjects = new GameObject[0, 0]; // an array to hold references to each gameobject that make up grid
    public SpriteRenderer[,] cellSpriteRenderers = new SpriteRenderer[0, 0]; // an array to hold references to the sprite renderer component attached to each gameobject
    public bool gameRunning = false; // bool controlling whether the game is currently running

    [Range(0.01f, 3f)]
    public float updateRate; // used to define how often will the game update (in seconds)
    private float _gameTimer; // a variable that will be used detect when the game should update 

    private Camera gameCamera; // the game camera pointing at the board

    // Awake is a built-in Unity function that is only called once, before the Start function
    private void Awake()
    {
        // find the camera in the scene and store it for later
        gameCamera = FindObjectOfType<Camera>();
    }

    // Start is a built-in Unity function that is called before the first frame update
    private void Start()
    {
        CreateGrid(gridSizeX, gridSizeY);
        PauseGame(true);
        UpdateGridVisuals();
    }

    // this function controls whether or not to pause the game
    private void PauseGame(bool setGamePause)
    {
        // if setGamePause is true the game should stop running
        if (setGamePause)
        {
            gameRunning = false;
            gameRunningText.text = "Game Paused";
            gameRunningText.color = Color.red;
        }
        else // else if setGamePause is false unpause the game
        {
            gameRunning = true;
            gameRunningText.text = "Game Running";
            gameRunningText.color = Color.green;
        }
    }

    // Update is a built-in Unity function that is called once per frame 
    private void Update()
    {
        //Check for left click to switch the state of the cell being clicked on
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(gameCamera.ScreenPointToRay(Input.mousePosition), out hit, 100.0f))
            {
                GameObject cell = hit.collider.gameObject;

                // search through every cell in the cell array of gameObjects until the one that was clicked is found
                for (int xCount = 0; xCount < gridSizeX; xCount++)
                {
                    for (int yCount = 0; yCount < gridSizeY; yCount++)
                    {
                        // when the cell is found switch its state
                        if (cell == cellGameObjects[xCount, yCount])
                        {
                            SwitchCellState(xCount, yCount);
                        }
                    }
                }
            }
        }

        // check if the spacebar key has been pressed. this key will toggle between whether the game is currently running or paused
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // if the gameRunning is true, pause the game
            if (gameRunning)
            {
                PauseGame(true);
            }
            else // if the gameRunning is false, unpause the game
            {
                PauseGame(false);
            }
        }

        // check if the C key has been pressed. this key will clear the grid and pause the game
        if (Input.GetKeyDown(KeyCode.C))
        {
            PauseGame(true);

            // iterate through every cell in the cell in the grid and set its state to dead
            for (int xCount = 0; xCount < gridSizeX; xCount++)
            {
                for (int yCount = 0; yCount < gridSizeY; yCount++)
                {
                    gameArray[xCount, yCount] = 0;
                }
            }
        }

        // check if the C key has been pressed. this key will clear the grid and pause the game
        if (Input.GetKeyDown(KeyCode.R))
        {
            // iterate through every cell in the cell in the grid and set its state to dead
            for (int xCount = 0; xCount < gridSizeX; xCount++)
            {
                for (int yCount = 0; yCount < gridSizeY; yCount++)
                {
                    gameArray[xCount, yCount] = UnityEngine.Random.Range(0, 2);
                }
            }
        }

        // update the visual state of each cell
        UpdateGridVisuals();

        // if the game is not running, return here to prevent the rest of the code in this Update function from running    
        if (gameRunning == false)
            return;

        if (_gameTimer < updateRate)
        {
            _gameTimer += Time.deltaTime;
        }
        else
        {
            UpdateCells();
            _gameTimer = 0f;
        }
    }

    // set the alive or dead state of a specified cell
    public void SwitchCellState(int x, int y)
    {
        // if the cell is alive, switch it to dead
        if (gameArray[x, y] == 1)
        {
            gameArray[x, y] = 0;
        }
        else // the cell must be dead, switch it to alive
        {
            gameArray[x, y] = 1;
        }
    }

    // update the status of each cell on grid according to the rules of the game
    private void UpdateCells()
    {
        // iterate through each cell in the rows and columns
        for (int xCount = 0; xCount < gridSizeX; xCount++)
        {
            for (int yCount = 0; yCount < gridSizeY; yCount++)
            {
                // find out the number of alive neighbours this cell has
                int aliveNeighbourCells = CountAliveNeighbourCells(xCount, yCount);

                if (gameArray[xCount, yCount] == 1) // if the cell is currently alive
                {
                    // Any live cell with fewer than two live neighbours dies, as if by underpopulation.
                    // Any live cell with more than three live neighbours dies, as if by overpopulation.
                    if (aliveNeighbourCells < 2 || aliveNeighbourCells > 3)
                    {
                        // cell die from under population or over population 
                        gameArrayNextGen[xCount, yCount] = 0;
                    }
                    else // if the cell doesn't die from underpopulation or overpopulation, assign it to be alive for the next generation of the game
                    {
                        gameArrayNextGen[xCount, yCount] = 1;
                    }
                }
                else // the cell is currently dead
                {
                    // Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
                    if (aliveNeighbourCells == 3)
                    {
                        gameArrayNextGen[xCount, yCount] = 1;
                    }
                    else // if the cell is dead, keep it dead for the next generation of the game
                    {
                        gameArrayNextGen[xCount, yCount] = 0;
                    }
                }
            }
        }

        // now the state of each cell has been calculated, apply the results by setting the current game array values to that of the next generation
        for (int xCount = 0; xCount < gridSizeX; xCount++)
        {
            for (int yCount = 0; yCount < gridSizeY; yCount++)
            {
                gameArray[xCount, yCount] = gameArrayNextGen[xCount, yCount];
            }
        }
    }

    // count the alive cells surrounding a specified cell on the grid 
    private int CountAliveNeighbourCells(int cellPositionX, int cellPositionY)
    {
        // create local variable to keep track of alive neighbour cells
        int aliveNeighbourCells = 0;

        // the code below tries to iterate through the neighbour cells immediately surrounding the specified cell on the grid as well as the specified cell
        //
        // On the grid the it would look like this
        //
        //  N N N
        //  N C N
        //  N N N
        //
        // N = neighbour C = cell that's neighbours are being counted

        for (int xPosition = cellPositionX - 1; xPosition < cellPositionX + 2; xPosition++)
        {
            for (int yPosition = cellPositionY - 1; yPosition < cellPositionY + 2; yPosition++)
            {
                // only continue if the neighbour is valid
                if (IsNeighbourValid(xPosition, yPosition))
                {
                    // if the cell is currently alive (1), increase the count of alive neighbours by one
                    if (gameArray[xPosition, yPosition] == 1)
                    {
                        aliveNeighbourCells++;

                        // we don't want to check if the specified cell is alive, only its neighbours so it was added, subtract it
                        if (xPosition == cellPositionX && yPosition == cellPositionY)
                        {
                            aliveNeighbourCells--;
                        }
                    }
                }
            }
        }

        // return the number of alive neighbour cells
        return aliveNeighbourCells;
    }

    // make sure that the cell we are trying to count is not beyond the range of the game grid (edges of the game board)
    private bool IsNeighbourValid(int cellPositionX, int cellPositionY)
    {
        if (cellPositionX < 0 || cellPositionY < 0)
            return false;

        if (cellPositionX >= gridSizeX || cellPositionY >= gridSizeY)
            return false;

        return true;
    }

    // this function creates the grid of the game
    private void CreateGrid(int sizeX, int sizeY)
    {
        // initialise the game array to the size of grid
        gameArray = new int[sizeX, sizeY];

        // initialise the game array that will contain the state for each cell in the next generation fo the game
        gameArrayNextGen = new int[sizeX, sizeY];

        // initialise the array of gameobjects that will hold the sprite renderers on the grid
        cellGameObjects = new GameObject[sizeX, sizeY];

        // initialise the array of sprite renderers that will visualise the grid
        cellSpriteRenderers = new SpriteRenderer[sizeX, sizeY];

        for (int xCount = 0; xCount < sizeX; xCount++)
        {
            for (int yCount = 0; yCount < sizeY; yCount++)
            {
                // create cell object and name it according to its position
                GameObject newCell = new GameObject("cell " + xCount + " " + yCount);

                //position the cell on the grid, spacing them out using the x and y count as coordinates 
                newCell.transform.position = new Vector3(xCount, yCount, 0);

                // add a sprite renderer to the cell object and assign the sprite it will use
                newCell.AddComponent<SpriteRenderer>().sprite = cellSprite;

                // add a reference of this sprite renderer to the array so we can change it later quickly
                cellSpriteRenderers[xCount, yCount] = newCell.GetComponent<SpriteRenderer>();

                // the size of the sprite is quite small, so increase the scale so there are no visable gaps in the grid
                newCell.transform.localScale = new Vector3(7.5f, 7.5f, 0f);

                // add a box collider to the cell so we can detect clicks from the mouse
                newCell.AddComponent<BoxCollider>();

                // add the gameobject of the cell to the array that stores references of the cell sprites
                cellGameObjects[xCount, yCount] = newCell;
            }
        }
    }

    // udpate the grid sprites colours according to their current state
    // this function will be called every frame of the game so the grid is always up to date 
    private void UpdateGridVisuals()
    {
        // iterate through each cell in the rows and columns
        for (int xCount = 0; xCount < gridSizeX; xCount++)
        {
            for (int yCount = 0; yCount < gridSizeY; yCount++)
            {
                // check if the state of the cell is 1 (alive)
                if (gameArray[xCount, yCount] == 1)
                {
                    cellSpriteRenderers[xCount, yCount].color = Color.white;
                }
                else if (gameArray[xCount, yCount] == 0) // if the cell is not alive, check if it's dead (0)
                {
                    cellSpriteRenderers[xCount, yCount].color = Color.black;
                }
                else // if the value op cell is 0 or 1 then something has gone wrong, display an error message
                {
                    Debug.LogError("Cell state is not either 1 or 0, check code for errors");
                }
            }
        }
    }
}