using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class creates a mini map which is attached to the players left arm. 
// the code is similar to the ForestFire2D script, a grid of 2D sprites is created which represent each cell
// the state of each cell updated in the Update function via a reference to the ForestFire3D object
public class MiniMap : MonoBehaviour
{
    public ForestFire3D forestFire3D; // reference to the main forest fire 3D script

    public GameObject cellSprite; // sprite used to represent a cell on the grid

    public Transform spawnPosition; // initial spawn position
    public SpriteRenderer[,] cellSpriteRenderers = new SpriteRenderer[0, 0]; // an array to hold references to the sprite renderer component attached to each gameobject


    // Start is a built-in Unity function that is called before the first frame update
    private void Start()
    {
        CreateGrid(forestFire3D.gridSizeX, forestFire3D.gridSizeY);
    }

    private void CreateGrid(int sizeX, int sizeY)
    {
        // initialise the array of sprite renderers that will visualise the grid
        cellSpriteRenderers = new SpriteRenderer[sizeX, sizeY];

        for (int xCount = 0; xCount < sizeX; xCount++)
        {
            for (int yCount = 0; yCount < sizeY; yCount++)
            {
                // create cell sprite for each cell in the grid
                GameObject newCell = Instantiate(cellSprite); 

                newCell.transform.SetParent(spawnPosition, true);
                newCell.transform.localPosition = Vector3.zero;
                newCell.transform.localRotation = Quaternion.identity;
                newCell.transform.localScale = new Vector3(0.035f, 0.035f, 0.035f);

                //position the cell on the grid, spacing them out using the x and y count as coordinates with a small offset
                newCell.transform.localPosition = new Vector3(xCount * 0.005f, yCount * 0.005f, 0.0f);

                // add a reference of this sprite renderer to the array so we can change it later quickly
                cellSpriteRenderers[xCount, yCount] = newCell.GetComponent<SpriteRenderer>();     
            }
        }
    }

    // Update is a built-in Unity function that is called once per frame 
    private void Update()
    {
        // go through every sprite in the mini map grid and assign the colour based on the state of each cell in the forest fire 3D script   
        for (int xCount = 0; xCount < forestFire3D.gridSizeX; xCount++)
        {
            for (int yCount = 0; yCount < forestFire3D.gridSizeY; yCount++)
            {
                if (forestFire3D.forestFireCells[xCount, yCount].cellState == ForestFireCell.State.Alight)
                {
                    cellSpriteRenderers[xCount, yCount].color = Color.red;
                }
                else if (forestFire3D.forestFireCells[xCount, yCount].cellState == ForestFireCell.State.Rock) 
                {
                    cellSpriteRenderers[xCount, yCount].color = Color.grey;
                }
                else if (forestFire3D.forestFireCells[xCount, yCount].cellState == ForestFireCell.State.Water) // river - it does set it to blue at the start, but changes to burnt too, like the 3d forest fire ...
                {
                    cellSpriteRenderers[xCount, yCount].color = Color.blue;
                }
                else if (forestFire3D.forestFireCells[xCount, yCount].cellState != ForestFireCell.State.Rock && forestFire3D.forestFireCells[xCount, yCount].cellFuel <= 0) 
                {
                    cellSpriteRenderers[xCount, yCount].color = Color.black;
                }
                else if (forestFire3D.forestFireCells[xCount, yCount].cellState == ForestFireCell.State.Grass) 
                {
                    cellSpriteRenderers[xCount, yCount].color = Color.yellow;
                }
                else if (forestFire3D.forestFireCells[xCount, yCount].cellState == ForestFireCell.State.Tree) 
                {
                    cellSpriteRenderers[xCount, yCount].color = Color.green;
                }
                else // something has gone wrong, display an error message
                {
                    Debug.LogError("objectArray is not 0, 1 or 2, check code for errors");
                }
            }
        }
    }
}