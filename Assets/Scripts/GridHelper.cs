using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics; // for int2

namespace PuzzleGameHelper
{
	public delegate void Switcher(Transform transform, Cell tempCell, float switchDuration, AnimationCurve switchAnimation, GridSettings gridSettings);
	
	public static class GridHelper
	{
		#region Méthodes pour la création de grille

		/// <summary>
		/// Création d'une grille de cellules
		/// </summary>
		/// <param name="gridSettings"></param>
		/// <returns></returns>
		public static Cell[] CreateGrid(Transform transform, GridSettings gridSettings, Material material)
		{
			gridSettings.InitializeCellSize();

			Cell[] cellsToReturn = new Cell[gridSettings.rowCount * gridSettings.columnCount];

			for (int x = 0; x < gridSettings.columnCount; x++)
				for (int y = 0; y < gridSettings.rowCount; y++)
				{
					bool isBlankCell = (y == 0 && x == gridSettings.columnCount - 1);
					CreateTile(transform, cellsToReturn, gridSettings, material, x, y, isBlankCell);
				}

			for (int i = 0; i < cellsToReturn.Length; i++)
				cellsToReturn[i].GetAdjacentCells();

			return cellsToReturn;
		}

		/// <summary>
		/// Méthode d'extension qui calcule de la taille des cellules
		/// pour le 'GridSettings' appelant.
		/// </summary>
		/// <param name="gridSettings"></param>
		static void InitializeCellSize(this GridSettings gridSettings)
		{
			gridSettings.cellSize.x = gridSettings.gridXSize / gridSettings.columnCount;
			gridSettings.cellSize.y = gridSettings.gridYSize / gridSettings.rowCount;
			gridSettings.cellExtents = gridSettings.cellSize * 0.5f;
			gridSettings.gridExtents = new Vector2(gridSettings.gridXSize, gridSettings.gridXSize) * 0.5f;
		}

		/// <summary>
		/// Création d'une tuile
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="cells"></param>
		/// <param name="gridSettings"></param>
		/// <param name="material"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="isEmpty"></param>
		static void CreateTile(Transform transform, Cell[] cells, GridSettings gridSettings, Material material, int x, int y, bool isEmpty = false)
		{
			// ???
			int tileIndex = GridPositionToIndex(gridSettings, x, y);

			// 4.1) Création du gameObject 'Tile'
			GameObject go = null;

			if (!isEmpty)
			{
				// ???
				Vector2 uvXMinMax = new Vector2(((x * gridSettings.cellSize.x) / gridSettings.gridXSize), (((x + 1) * gridSettings.cellSize.x) / gridSettings.gridXSize));
				Vector2 uvYMinMax = new Vector2(((y * gridSettings.cellSize.y) / gridSettings.gridYSize), (((y + 1) * gridSettings.cellSize.y) / gridSettings.gridYSize));

				go = new GameObject("Tile " + tileIndex);

				// 4.2) Ajout d'un MeshFilter et création d'un Quad à l'interieur
				MeshFilter mf = go.AddComponent<MeshFilter>();
				mf.mesh = MeshGenerator.CreateQuad(gridSettings.cellExtents, uvXMinMax, uvYMinMax);

				// 4.3) Ajout d'un MeshRenderer au gameObject
				MeshRenderer mr = go.AddComponent<MeshRenderer>();
				mr.material = material;

				// 4.4) Positionement du gameObject
				go.transform.position = GridPositionToWorldPosition(transform, x, y, gridSettings);

			}

			cells[tileIndex] = new Cell(go, new int2(x, y), cells, gridSettings);
		}
		
		#endregion

		#region Helpers

		/// <summary>
		/// Recherche une cellule aléatoire parmis les cellules adjacentes
		/// à la cellule cible.
		/// Possibilité de fournir une cellule à exclure de la recherche.
		/// </summary>
		/// <param name="targetCell">Cellule ciblé pour la recherche.</param>
		/// <param name="cellToExclude">Cellule à exclure de la recherche.</param>
		/// <returns>Cellule adjacente aléatoire</returns>
		public static Cell GetRandomAdjacent(Cell targetCell, Cell cellToExclude = null)
		{
			List<Cell> validCell = new List<Cell>();

			for (int i = 0; i < targetCell._adjacentCells.Length; i++)
				if (targetCell._adjacentCells[i] != null)
					validCell.Add(targetCell._adjacentCells[i]);

			if (cellToExclude != null)
				validCell.Remove(cellToExclude);

			return validCell[UnityEngine.Random.Range(0, validCell.Count)];
		}

		public static Vector3 GridPositionToWorldPosition(Transform transform, int xPosition, int yPosition, GridSettings gridSettings)
		{
			return (Vector3)((new Vector2(xPosition, yPosition) * gridSettings.cellSize) + gridSettings.cellExtents - gridSettings.gridExtents) + transform.position;
		}
		public static Vector3 GridPositionToWorldPosition(Transform transform, int2 position, GridSettings gridSettings)
		{
			return GridPositionToWorldPosition(transform, position.x, position.y, gridSettings);
		}

		public static Cell GetCellFromPosition(Cell[] cells, GridSettings gridSettings, int xPos, int yPos)
		{
			if (xPos >= 0 && xPos < gridSettings.columnCount && yPos >= 0 && yPos < gridSettings.rowCount)
				return cells[GridPositionToIndex(gridSettings, xPos, yPos)];

			return null;
		}
		public static Cell GetCellFromPosition(Cell[] cells, GridSettings gridSettings, int2 position)
		{
			return GetCellFromPosition(cells, gridSettings, position.x, position.y);
		}

		public static int GridPositionToIndex(GridSettings gridSettings, int xPos, int yPos)
		{
			return (xPos * gridSettings.rowCount + yPos);
		}
		public static int GridPositionToIndex(GridSettings gridSettings, int2 position)
		{
			return GridPositionToIndex(gridSettings, position.x, position.y);
		}

		public static int2 IndexToGridPosition(GridSettings gridSettings, int index)
		{
			return new int2(Mathf.FloorToInt(index / gridSettings.rowCount), index % gridSettings.columnCount);
		}

		/// <summary>
		/// Méthode 2D:
		/// Transforme une position donnée en position locale pour l'objet appelant
		/// sous la forme d'un int2
		/// </summary>
		/// <param name="position">Position à transformer</param>
		/// <returns>Position transformée en int2</returns>
		public static int2 WorldToGridPosition(Transform transform, GridSettings gridSettings, Vector2 position)
		{
			// On transform la position du click joueur + un offset en position locale pour la grille.
			Vector2 localPosition = (Vector2)transform.InverseTransformPoint(position) + gridSettings.gridExtents;
			// On retourne en int2 la nouvelle position arrondie.
			return new int2(Mathf.FloorToInt(localPosition.x / gridSettings.cellSize.x), Mathf.FloorToInt(localPosition.y / gridSettings.cellSize.y));
		}


		/// <summary>
		/// Limite 2 entiers donnés à la taille du tableau de jeu
		/// et retourne le résultat en int2.
		/// </summary>
		/// <param name="xPosition">'x' à transformer</param>
		/// <param name="yPosition">'y' à transformer</param>
		/// <returns>Les nouveaux entiers transformés sous la forme d'un int2</returns>
		public static int2 ClampPositionToGrid(GridSettings gridSettings, int xPosition, int yPosition)
		{
			return new int2(Mathf.Clamp(xPosition, 0, gridSettings.columnCount - 1), Mathf.Clamp(yPosition, 0, gridSettings.rowCount - 1));
		}
		/// <summary>
		/// Limite un int2 donné à la taille du tableau de jeu
		/// et retourne le résultat en int2.
		/// </summary>
		/// <param name="position">int2 à transformer</param>
		/// <returns>Le int2 transformé.</returns>
		public static int2 ClampPositionToGrid(GridSettings gridSettings, int2 position)
		{
			return ClampPositionToGrid(gridSettings, position.x, position.y);
		}

		#endregion
	}

	[System.Serializable]
	public struct GridSettings
	{
		[Tooltip("Test Tooltip Info")]
		public float gridXSize;
		public float gridYSize;
		public int columnCount;
		public int rowCount;

		public Vector2 cellSize;
		public Vector2 cellExtents;
		public Vector2 gridExtents;
	}
}
