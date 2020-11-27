using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


public class GridGenerator : MonoBehaviour
{
    #region Paramètres de la grille
    [Tooltip("Test Tooltip Info")]
    public float _gridXSize;
    public float _gridYSize;
    public int _columnCount;
    public int _rowCount;

    Vector2 _cellSize;
    Vector2 _cellExtents;
    Vector2 _gridExtents;
    #endregion

    public Cell[] _cells;

    Material _currentMaterial;
    public Material[] _tableOfMaterial;

    Camera _camera;

    public AnimationCurve _switchAnimation;

    public float _switchDuration = 0.5f;

    // 1) Lancement de CreateGrid()
    private void Start()
    {
        RandomMaterial();
        CreateGrid();
        _camera = Camera.main;
        StartCoroutine(UpDating());
    }

    IEnumerator UpDating()
    {
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(Shuffling(100, 4));
        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);

                Cell cellToMove = GetCellFromPosition(ClampPositionToGrid(WorldToGridPosition(mouseWorldPosition)));

                yield return StartCoroutine(Switching(cellToMove, _switchDuration));
            }
            yield return null;
        }
    }

    IEnumerator Shuffling(int iterationCount, float shuffleDuration)
    {
        int iteration = 0;
        float iterationDuration = shuffleDuration / iterationCount;
        Cell lastemptyCell = null; ;
        Cell emptyCell;

        while (iteration < iterationCount)
        {
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i]._isEmpty == true)
                {
                    emptyCell = _cells[i];
                    yield return StartCoroutine(Switching(GetRandomAdjacent(emptyCell, lastemptyCell), iterationDuration));
                    lastemptyCell = emptyCell;
                    break;
                }
            }
            iteration++;
            yield return null;
        }
    }

    IEnumerator Switching(Cell tempCell, float switchDuration)
    {

        for (int i = 0; i < tempCell._adjacentCells.Length; i++)
        {
            if (tempCell._adjacentCells[i] != null && tempCell._adjacentCells[i]._isEmpty)
            {
                float t = 0f;
                float tRatio;
                float tAnimated;
                Vector3 startPosition = tempCell._cellGo.transform.position;
                Vector3 endPosition = GridPositionToWorldPosition(tempCell._adjacentCells[i]._gridPosition);
                while (t < switchDuration)
                {
                    tRatio = t / switchDuration;
                    tAnimated = _switchAnimation.Evaluate(tRatio);
                    tempCell._cellGo.transform.position = Vector3.Lerp(startPosition, endPosition, tAnimated);

                    t += Time.deltaTime;
                    yield return null;
                }
                tempCell._cellGo.transform.position = endPosition;

                GameObject tempGo = tempCell._cellGo;
                tempCell._cellGo = tempCell._adjacentCells[i]._cellGo;
                tempCell._adjacentCells[i]._cellGo = tempGo;
                tempGo.transform.position = tempCell._gridManager.GridPositionToWorldPosition(tempCell._adjacentCells[i]._gridPosition);

                bool tempIsEmpty = tempCell._isEmpty;
                tempCell._isEmpty = tempCell._adjacentCells[i]._isEmpty;
                tempCell._adjacentCells[i]._isEmpty = tempIsEmpty;


                Debug.Log("I moved", tempGo);
                break;
            }
        }
    }

    Cell GetRandomAdjacent(Cell cell, Cell cellToExclude = null)
    {
        List<Cell> validCell = new List<Cell>();
        for (int i = 0; i < cell._adjacentCells.Length; i++)
        {
            if (cell._adjacentCells[i] != null) { validCell.Add(cell._adjacentCells[i]); }
        }
        if (cellToExclude != null) { validCell.Remove(cellToExclude); }

        return validCell[UnityEngine.Random.Range(0, validCell.Count)];
    }

    // 2) Création d'une grille de cellules
    void CreateGrid()
    {
        InitializeCellSize();

        _cells = new Cell[_rowCount * _columnCount];

        for (int x = 0; x < _columnCount; x++)
        {
            for (int y = 0; y < _rowCount; y++)
            {
                //bool randomTrueFalse = UnityEngine.Random.Range(0,2) == 0;
                bool isBlankCell = (y == 0 && x == _columnCount - 1);
                CreateTile(x, y, isBlankCell);
            }
        }
        for (int i = 0; i < _cells.Length; i++)
        {
            _cells[i].GetAdjacentCells();
        }
    }

    // 3) Calcule de la taille d'une cellule
    void InitializeCellSize()
    {
        _cellSize.x = _gridXSize / _columnCount;
        _cellSize.y = _gridYSize / _rowCount;
        _cellExtents = _cellSize * 0.5f;
        _gridExtents = new Vector2(_gridXSize, _gridXSize) * 0.5f;
    }

    // 4) Création d'une tuile
    void CreateTile(int x, int y, bool isEmpty = false /* Par défaut, est sur faux */)
    {
        // ???
        int tileIndex = GridPositionToIndex(x, y);

        // 4.1) Création du gameObject 'Tile'
        GameObject go = null;

        if (!isEmpty)
        {
            // ???
            Vector2 uvXMinMax = new Vector2(((x * _cellSize.x) / _gridXSize), (((x + 1) * _cellSize.x) / _gridXSize));
            Vector2 uvYMinMax = new Vector2(((y * _cellSize.y) / _gridYSize), (((y + 1) * _cellSize.y) / _gridYSize));

            go = new GameObject("Tile " + tileIndex);

            // 4.2) Ajout d'un MeshFilter et création d'un Quad à l'interieur
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = MeshGenerator.CreateQuad(_cellExtents, uvXMinMax, uvYMinMax);

            // 4.3) Ajout d'un MeshRenderer au gameObject
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = _currentMaterial;

            // 4.4) Positionement du gameObject
            go.transform.position = GridPositionToWorldPosition(x, y);

        }

        _cells[tileIndex] = new Cell(go, new int2(x, y), this);

    }

    public int GridPositionToIndex(int xPos, int yPos)
    {
        return (xPos * _rowCount + yPos);
    }

    public int GridPositionToIndex(int2 position)
    {
        return GridPositionToIndex(position.x, position.y);
    }

    public int2 IndexToGridPosition(int index)
    {
        return new int2(Mathf.FloorToInt(index / _rowCount), index % _columnCount);
    }

    public Cell GetCellFromPosition(int xPos, int yPos)
    {
        if (xPos >= 0 && xPos < _columnCount && yPos >= 0 && yPos < _rowCount)
        {
            return _cells[GridPositionToIndex(xPos, yPos)];
        }
        return null;
    }

    public Cell GetCellFromPosition(int2 position)
    {
        return GetCellFromPosition(position.x, position.y);
    }

    public Vector3 GridPositionToWorldPosition(int xPosition, int yPosition)
    {
        return (Vector3)((new Vector2(xPosition, yPosition) * _cellSize) + _cellExtents - _gridExtents) + transform.position;
    }

    public Vector3 GridPositionToWorldPosition(int2 position)
    {
        return GridPositionToWorldPosition(position.x, position.y);
    }

    int2 WorldToGridPosition(Vector2 position)
    {
        Vector2 localPosition = (Vector2)transform.InverseTransformPoint(position) + _gridExtents;
        return new int2(Mathf.FloorToInt(localPosition.x / _cellSize.x), Mathf.FloorToInt(localPosition.y / _cellSize.y));
    }

    int2 ClampPositionToGrid(int xPosition, int yPosition)
    {
        return new int2(Mathf.Clamp(xPosition, 0, _columnCount - 1), Mathf.Clamp(yPosition, 0, _rowCount - 1));
    }

    int2 ClampPositionToGrid(int2 position)
    {
        return ClampPositionToGrid(position.x, position.y);
    }

    void RandomMaterial()
    {
        _currentMaterial = _tableOfMaterial[UnityEngine.Random.Range(0, _tableOfMaterial.Length)];
    }
}
