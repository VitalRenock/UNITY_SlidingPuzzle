using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using PuzzleGameHelper;

public enum Direction { Right, Up, Left, Down }

public class Cell
{
    private Cell[] cells;
    private GridSettings gridSettings;

    #region Paramètres d'une cellule

    public GameObject _cellGo;
    public bool _isEmpty;
    public int2 _gridPosition;
    public GridGenerator _gridManager;
    public Cell[] _adjacentCells;
    string _name;

    #endregion

    #region Constructeurs

    public Cell(GameObject cellGo, int2 gridPosition, Cell[] cells, GridSettings gridSettings)
    {
        if (cellGo)
            _name = cellGo.name;
        else
            _name = "empty";

        _isEmpty = cellGo == null; // Test la condition et renvoie 'vrai' ou 'faux'
        _cellGo = cellGo;
        _gridPosition = gridPosition;
        this.cells = cells;
        this.gridSettings = gridSettings;
    }

    #endregion

    public bool IsCellAtInitialPosition()
    {
        if (_cellGo == null && _name == "empty") { return true; }
        else if (_cellGo != null && _cellGo.name == _name) { return true; }
        else { return false; }
    }

    public void GetAdjacentCells()
    {
        _adjacentCells = new Cell[4];
        _adjacentCells[(int)Direction.Right] = GridHelper.GetCellFromPosition(cells, gridSettings, _gridPosition.x + 1, _gridPosition.y);
        _adjacentCells[(int)Direction.Up] = GridHelper.GetCellFromPosition(cells, gridSettings, _gridPosition.x, _gridPosition.y + 1);
        _adjacentCells[(int)Direction.Left] = GridHelper.GetCellFromPosition(cells, gridSettings, _gridPosition.x - 1, _gridPosition.y);
        _adjacentCells[(int)Direction.Down] = GridHelper.GetCellFromPosition(cells, gridSettings, _gridPosition.x, _gridPosition.y - 1);
    }
}
