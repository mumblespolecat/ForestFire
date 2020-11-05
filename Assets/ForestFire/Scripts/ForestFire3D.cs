using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// class that controls the forest fire cellular automaton
public class ForestFire3D : MonoBehaviour
{
    public int gridSizeX; // x size of the grid
    public int gridSizeY; // y size of the grid
    public int nlight; // the number of trees to set alight at the start of the game
    public int xC, yC; // used for picking random x, y points
    public int windDirection; // new variable that holds the wind direction - goes from 1 (N) to 8 (NW)
    public int windSpeed; // new variable for wind speed - takes values 0 (no wind), 1 (gentle wind), 2 (strong wind)
    public int riverStartSide; // used to hold the random side for starting the river generation
    public int riverStartPosition; // variable to hold how long along the start side to start the river
    public int rXC, rYC; // variables to hold the current x and y co-ord for generating the river
    public int directionOfRiver; // variable to hold the direction the river goes in on generation
    public int oldDirectionOfRiver; // variable to hold where the river has come from so it doesn't keep going back on itself
    public int edgeBounce; // number of times river tries to fall off grid. It gets 15 chances

    public int rockChance; // the percentage chance a cell is assigned as rock
    public int grassChance; // the percentage chance a cell is assigned as grass

    public GameObject cellPrefab; // gameobject prefab used to represent a cell on the grid   

    public ForestFireCell[,] forestFireCells = new ForestFireCell[0, 0]; // array of ForestFireCell objects
    public ForestFireCell.State[,] forestFireCellsNextGenStates = new ForestFireCell.State[0,0]; // array of cell states to be used in the next generation of the game 

    public GameObject[,] cellGameObjects = new GameObject[0, 0]; // an array to hold references to each gameobject that make up grid
    public bool gameRunning = false; // bool controlling whether the game is currently running

    [Range(0.01f, 3f)]
    public float updateRate; // used to define how often will the game update (in seconds)
    private float _gameTimer; // a variable that will be used detect when the game should update 

    private Camera gameCamera; // the camera that is players viewport

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
        RandomiseGrid();
        PauseGame(true);
        UpdateGridVisuals();
        windDirection = UnityEngine.Random.Range(1, 8); // set a random initial value to windDirection
        windSpeed = UnityEngine.Random.Range(0, 2); // set a random initial value to windSpeed
    }

    // this function controls whether or not to pause the game
    private void PauseGame(bool setGamePause)
    {
        // if setGamePause is true the game should stop running
        if (setGamePause)
        {
            gameRunning = false;           
        }
        else // else if setGamePause is false unpause the game
        {
            gameRunning = true;          
        }
    }

    // Update is a built-in Unity function that is called once per frame 
    private void Update()
    {
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

        // check if the R key has been pressed. this key will clear the grid and pause the game
        if (Input.GetKeyDown(KeyCode.R))
        {
            RandomiseGrid();
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

    private void RandomiseGrid()
    {
        nlight = 2; // how many trees to set on fire
                      // iterate through every cell in the cell in the grid and set its state to dead, decide what type of object is present and if flammable assign an amount of fuel

        for (int xCount = 0; xCount < gridSizeX; xCount++)
        {
            for (int yCount = 0; yCount < gridSizeY; yCount++)
            {
                xC = UnityEngine.Random.Range(0, 100); // generate a random number between 0 and 100

                if (xC < rockChance) // if the random value is less than rock chance, assign cell as rock
                {                   
                    forestFireCells[xCount, yCount].SetRock();
                }
                else if (xC < grassChance) // if the random value is less than grass chance, assign cell as grass and set cell fuel
                {
                    forestFireCells[xCount, yCount].SetGrass();
                    forestFireCells[xCount, yCount].cellFuel = UnityEngine.Random.Range(1, 5);
                }
                else // if the random value is higher than rock and grass chance, assign as tree and set cell fuel
                {
                    forestFireCells[xCount, yCount].SetTree();
                    forestFireCells[xCount, yCount].cellFuel = UnityEngine.Random.Range(15, 25);
                }
            }
        }

        do
        {
            xC = UnityEngine.Random.Range(0, gridSizeX);// now pick some trees at random and set them alight
            yC = UnityEngine.Random.Range(0, gridSizeY);

            if (forestFireCells[xC, yC].cellState == ForestFireCell.State.Tree)
            {
                forestFireCells[xC, yC].SetAlight();

                nlight = nlight - 1;     // decrease number of trees to light by 1
            }
        } while (nlight > 0);  // when you've lit them all exit this loop

        // here is the right place to set the river up
        riverStartSide = UnityEngine.Random.Range(1, 5); // 1 is North, 2 is East, 3 is South, 4 is West
        riverStartPosition = UnityEngine.Random.Range(5, 35); // picks how far along the side to start generating the river, missing off 10 slots each end so it isn't right on the edge

        // this set of if statements looks to see which side the river is starting on and sets the coordinates
        if (riverStartSide == 1) // north
        {
            rXC = riverStartPosition;
            rYC = 39;
        }
        else if (riverStartSide == 2) // east
        {
            rXC = 39;
            rYC = riverStartPosition;
        }
        else if (riverStartSide == 3) // south
        {
            rXC = riverStartPosition;
            rYC = 0;
        }
        else if (riverStartSide == 4) // west
        {
            rXC = 0;
            rYC = riverStartPosition;
        }

        forestFireCells[rXC, rYC].SetWater();
        Debug.Log("edge set to " + rXC + " and " + rYC);
        edgeBounce = 0; // set edgeBounce to 0 - edgebounce is the number of times the river can fall off the edge (and be put back on)

        // this loop, loops to build the river. It takes a random direction, and checks to see if that would take it off the edge. Bounces it if it does (up to a max of 15 times) and moves in that direction if not.
        // stays looping whlie the river / water feature is still on the grid and the max number of bounces hasn't been reached.
        do
        {
            directionOfRiver = UnityEngine.Random.Range(1, 5); // 1 is North, 2 is East, 3 is South, 4 is West

            if (directionOfRiver == 1) // move 1 cell north
            {
                if (rYC == 39)
                {
                    edgeBounce++;
                }
                else
                {
                    rYC++;
                }
            }
            else if (directionOfRiver == 2) // move 1 cell east
            {
                if (rXC == 39)
                {
                    edgeBounce++;
                }
                else
                {
                    rXC++;
                }
            }
            else if (directionOfRiver == 3) // move 1 cell south
            {
                if (rYC == 0)
                {
                    edgeBounce++;
                }
                else
                {
                    rYC--;
                }
            }
            else if (directionOfRiver == 4) // move 1 cell west
            {
                if (rXC == 0)
                {
                    edgeBounce++;
                }
                else
                {
                    rXC--;
                }
            }

            forestFireCells[rXC, rYC].SetWater(); // set that cell to river, so I can see it



        } while (rXC <= 49 && rXC >= 0 && rYC <= 49 && rYC >= 0 && edgeBounce <= 15);

        // set the middle cell as grass which is where the player is placed
        forestFireCells[20, 20].SetGrass();
    }


    // update the status of each cell on grid according to the rules of the game
    private void UpdateCells()
    {
        // change the wind direction randomly - change 30% of the time as this is a bit more constant - anecdotally
        xC = UnityEngine.Random.Range(0, 100); // generate a random number between 0 and 100
        if (xC > 70)
        {
            windDirection = windDirection + UnityEngine.Random.Range(-1, 2); // change windDirection by -1, 0 or 1

            if (windDirection < 1)
            {
                windDirection = 8;
            }
            else if (windDirection > 8)
            {
                windDirection = 1;
            }
            // windDirection = 4; // Setting windDirection here for testing to see if it actually has an impact
        }

        // change the wind speed randomly - change 50% of the time as this is a bit more volatile - also anecdotally !
        xC = UnityEngine.Random.Range(0, 100); // generate a random number between 0 and 100
        if (xC > 50)
        {
            windSpeed = windSpeed + UnityEngine.Random.Range(-1, 2); // change windSpeed by -1, 0 or 1

            if (windSpeed < 0)
            {
                windSpeed = 0;
            }
            else if (windSpeed > 2)
            {
                windSpeed = 2;
            }
            // windSpeed = 2; // setting windSpeed here for testing to see if it actually has an impact
        }

        // iterate through each cell in the rows and columns
        for (int xCount = 0; xCount < gridSizeX; xCount++)
        {
            for (int yCount = 0; yCount < gridSizeY; yCount++)
            {
                // find out the number of alight neighbours this cell has
                int alightNeighbourCells = CountAlightNeighbourCells(xCount, yCount);

                if (forestFireCells[xCount, yCount].cellState == ForestFireCell.State.Alight) // if the cell is currently alight let it burn but reduce it's fuel and see if it goes out
                {
                     forestFireCells[xCount, yCount].cellFuel--; // reduce fuel by 1 (-- operater reduces an integer by 1)


                    if (forestFireCells[xCount, yCount].cellFuel <= 0) // has it burned all its fuel?
                    {
                        // first, check it isn't water
                        if (forestFireCells[xCount, yCount].cellState == ForestFireCell.State.Water)
                        {
                            forestFireCells[xCount, yCount].SetWater();  // just do that again, explicitly - see iuf that works
                        }
                        else
                        {
                            // cell has no fuel so is burned out 
                            forestFireCellsNextGenStates[xCount, yCount] = ForestFireCell.State.Burnt;
                        }
                        
                    }
                    else // cell still has fuel so carries on burning
                    {
                        forestFireCellsNextGenStates[xCount, yCount] = forestFireCells[xCount, yCount].cellState;
                    }
                }
                else if (forestFireCells[xCount, yCount].cellFuel > 0)  // the cell has fuel but is not alight yet
                {
                    // A dead cell with an alight neighbour which has fuel has a probablility of becoming an alight cell
                    if (alightNeighbourCells > 0)
                    {
                        xC = UnityEngine.Random.Range(0, 100); // generate a random number between 0 and 100

                        if (xC < 10 * alightNeighbourCells) // the more alight neighbours the greater the probability of catching light
                                                                      // e.g. 1 alight neighbour = 10 * 1 = 10% chance of catching fire, 2 alight neighbours = 10 * 2 = 20% chance of catching fire, etc.
                        {
                            forestFireCellsNextGenStates[xCount, yCount] = ForestFireCell.State.Alight;  // a 10% chance of catching fire
                        }
                    }
                    else // no neighbours on fire, keep it dead for the next generation of the game
                    {
                        forestFireCellsNextGenStates[xCount, yCount] = forestFireCells[xCount, yCount].cellState;
                    }
                }
            }
        }

        // now the state of each cell has been calculated, apply the results by setting the current game array values to that of the next generation
        for (int xCount = 0; xCount < gridSizeX; xCount++)
        {
            for (int yCount = 0; yCount < gridSizeY; yCount++)
            {
                forestFireCells[xCount, yCount].cellState = forestFireCellsNextGenStates[xCount, yCount];
            }
        }
    }

    // count the alight cells surrounding a specified cell on the grid 
    private int CountAlightNeighbourCells(int cellPositionX, int cellPositionY)
    {
        // create local variable to keep track of alight neighbour cells
        int alightNeighbourCells = 0;

        // the code below tries to iterate through the neighbour cells immediately surrounding the specified cell on the grid as well as the specified cell
        //
        // On the grid the it would like this
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
                    // if the cell is currently alight (1), increase the count of alight neighbours by one
                    if (forestFireCells[xPosition, yPosition].cellState == ForestFireCell.State.Alight)
                    {
                        alightNeighbourCells++;

                        // this set of if statements increases the alightNeighbourCells count by 1 if the wind is coming fromm the appropriate direction - the initial set up 
                        // and the first of the if statements was written with Steve helping.  The rest of the implementation, I did.

                        if (xPosition == cellPositionX && yPosition == cellPositionY - 1 && windDirection == 1) // if statement to increase the value of alightNeighbourCells by 1 if the windDirection is set to 1
                        {
                            alightNeighbourCells = AddForWind(alightNeighbourCells);
                        }

                        else if (xPosition == cellPositionX - 1 && yPosition == cellPositionY - 1 && windDirection == 2) // if statement to increase the value of alightNeighbourCells by 1 if the windDirection is set to 2
                        {
                            alightNeighbourCells = AddForWind(alightNeighbourCells);
                        }

                        else if (xPosition == cellPositionX - 1 && yPosition == cellPositionY && windDirection == 3) // if statement to increase the value of alightNeighbourCells by 1 if the windDirection is set to 3
                        {
                            alightNeighbourCells = AddForWind(alightNeighbourCells);
                        }

                        else if (xPosition == cellPositionX - 1 && yPosition == cellPositionY + 1 && windDirection == 4) // if statement to increase the value of alightNeighbourCells by 1 if the windDirection is set to 4
                        {
                            alightNeighbourCells = AddForWind(alightNeighbourCells);
                        }

                        else if (xPosition == cellPositionX && yPosition == cellPositionY + 1 && windDirection == 5) // if statement to increase the value of alightNeighbourCells by 1 if the windDirection is set to 5
                        {
                            alightNeighbourCells = AddForWind(alightNeighbourCells);
                        }

                        else if (xPosition == cellPositionX + 1 && yPosition == cellPositionY + 1 && windDirection == 6) // if statement to increase the value of alightNeighbourCells by 1 if the windDirection is set to 6
                        {
                            alightNeighbourCells = AddForWind(alightNeighbourCells);
                        }

                        else if (xPosition == cellPositionX + 1 && yPosition == cellPositionY && windDirection == 7) // if statement to increase the value of alightNeighbourCells by 1 if the windDirection is set to 7
                        {
                            alightNeighbourCells = AddForWind(alightNeighbourCells);
                        }

                        else if (xPosition == cellPositionX + 1 && yPosition == cellPositionY - 1 && windDirection == 8) // if statement to increase the value of alightNeighbourCells by 1 if the windDirection is set to 8
                        {
                            alightNeighbourCells = AddForWind(alightNeighbourCells);
                        }

                        // we don't want to check if the specified cell is alight, only its neighbours so it was added, subtract it
                        if (xPosition == cellPositionX && yPosition == cellPositionY)
                        {
                            alightNeighbourCells--;
                        }
                    }
                }
            }
        }

        // return the number of alight neighbour cells
        return alightNeighbourCells;
    }

    // Add to the number of alight cells after looking at the wind
    private int AddForWind(int alightNeighbourCells)
    {
        if (windSpeed == 1)
        {
            alightNeighbourCells++;
        }
        else if (windSpeed == 2)
        {
            alightNeighbourCells++;
            alightNeighbourCells++;
        }

        return alightNeighbourCells;
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
        // initialise the game array and array containing their states for the next generation to the size of grid
        forestFireCells = new ForestFireCell[sizeX, sizeY];
        forestFireCellsNextGenStates = new ForestFireCell.State[sizeX, sizeY];

        // initialise the array of gameobjects that will hold the game objects on the grid
        cellGameObjects = new GameObject[sizeX, sizeY];

        int xSpacing = 0;
        int ySpacing = 0;

        for (int xCount = 0; xCount < sizeX; xCount++)
        {
            for (int yCount = 0; yCount < sizeY; yCount++)
            {
                // create cell object and name it according to its position (3 adds extra space between cells)
                GameObject newCell = Instantiate(cellPrefab, new Vector3(xCount, 0, yCount), Quaternion.identity);
                newCell.name = "cell " + xCount + " " + yCount;

                //position the cell on the grid, spacing them out using the x and y count as coordinates 
                newCell.transform.position = new Vector3(xCount + xSpacing, 0, yCount + ySpacing);

                // add the gameobject of the cell to the array that stores references of the cell gameObjects
                cellGameObjects[xCount, yCount] = newCell;

                // add to array
                ForestFireCell forestFireCell = newCell.GetComponent<ForestFireCell>();
                forestFireCells[xCount, yCount] = forestFireCell;
                
                 // add reference to camera
                forestFireCell.playerCamera = gameCamera.gameObject;                

                ySpacing += 3;
            }

            ySpacing = 0;
            xSpacing += 3;
        }
    }

    // udpate the grid cells according to their current state
    // this function will be called every frame of the game so the grid is always up to date 
    private void UpdateGridVisuals()
    {
        // iterate through each cell in the rows and columns
        for (int xCount = 0; xCount < gridSizeX; xCount++)
        {
            // check current state of cell an update visual
            for (int yCount = 0; yCount < gridSizeY; yCount++)
            {
                if (forestFireCells[xCount, yCount].cellState == ForestFireCell.State.Alight)
                {
                    forestFireCells[xCount, yCount].SetAlight();
                }
                else if (forestFireCells[xCount, yCount].cellState == ForestFireCell.State.Rock)
                {
                    forestFireCells[xCount, yCount].SetRock();
                }
                else if (forestFireCells[xCount, yCount].cellState != ForestFireCell.State.Rock && forestFireCells[xCount, yCount].cellFuel <= 0)// it's not a rock but it's fuel is zero, therefore it must be burnt out grass or tree
                {
                    if (forestFireCells[xCount, yCount].cellState != ForestFireCell.State.Water)
                    {
                        forestFireCells[xCount, yCount].SetBurnt();
                    }
                }
                else if (forestFireCells[xCount, yCount].cellState == ForestFireCell.State.Grass)
                {
                    forestFireCells[xCount, yCount].SetGrass();
                }
                else if (forestFireCells[xCount, yCount].cellState == ForestFireCell.State.Water)
                {
                    forestFireCells[xCount, yCount].SetWater();
                }
                else if (forestFireCells[xCount, yCount].cellState == ForestFireCell.State.Tree)
                {
                    forestFireCells[xCount, yCount].SetTree();
                }
            }
        }
    }
}