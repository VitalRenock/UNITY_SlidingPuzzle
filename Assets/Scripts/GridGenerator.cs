using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using PuzzleGameHelper;


public class GridGenerator : MonoBehaviour
{
	#region Paramètres de la grille

	public GridSettings GridSettings;
    public Cell[] _cells;

	#endregion

	Camera _camera;

    Material _currentMaterial;
    public Material[] _tableOfMaterial;

    public AnimationCurve _switchAnimation;
    public float _switchDuration = 0.5f;


    // 1) Lancement de CreateGrid()
    private void Start()
    {
        RandomMaterial();
        _cells = GridHelper.CreateGrid(transform, GridSettings, _currentMaterial);
        _camera = Camera.main;
        StartCoroutine(UpDating());
    }

    IEnumerator UpDating()
    {
        // On mets la Coroutine en pause pendant un laps de temps.
        yield return new WaitForSeconds(1f);

        // On lance le mélange des pièces et on attends la fin de son execution.
        yield return StartCoroutine(Shuffling(_cells, 100, 4, _switchAnimation, GridSettings));

        // Le jeu commence.
        while (true)
        {
            // Si le joueur clique sur la souris...
            if (Input.GetMouseButtonDown(0))
            {
                // On enregistre la position du click joueur.
                Vector3 mouseWorldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);

                Cell cellToMove = GridHelper.GetCellFromPosition(_cells, GridSettings, GridHelper.ClampPositionToGrid(GridSettings, GridHelper.WorldToGridPosition(transform, GridSettings, mouseWorldPosition)));
                yield return StartCoroutine(Switching(cellToMove, _switchDuration, _switchAnimation, GridSettings));
            }
            
            yield return null;
        }
    }

    public IEnumerator Shuffling(Cell[] gridToShuffle, int iterationCount, float shuffleDuration, AnimationCurve switchAnimation, GridSettings gridSettings)
    {
        int iteration = 0;
        float iterationDuration = shuffleDuration / iterationCount;
        Cell lastemptyCell = null; ;
        Cell emptyCell;

        while (iteration < iterationCount)
        {
            for (int i = 0; i < gridToShuffle.Length; i++)
                if (gridToShuffle[i]._isEmpty == true)
                {
                    emptyCell = gridToShuffle[i];
                    yield return StartCoroutine(Switching(GridHelper.GetRandomAdjacent(emptyCell, lastemptyCell), iterationDuration, switchAnimation, gridSettings));
                    lastemptyCell = emptyCell;
                    break;
                }

            iteration++;
            yield return null;
        }
    }

    public IEnumerator Switching(Cell tempCell, float switchDuration, AnimationCurve switchAnimation, GridSettings gridSettings)
    {
        for (int i = 0; i < tempCell._adjacentCells.Length; i++)
            if (tempCell._adjacentCells[i] != null && tempCell._adjacentCells[i]._isEmpty)
            {
                float t = 0f;
                float tRatio;
                float tAnimated;
                Vector3 startPosition = tempCell._cellGo.transform.position;
                Vector3 endPosition = GridHelper.GridPositionToWorldPosition(transform, tempCell._adjacentCells[i]._gridPosition, gridSettings);
                while (t < switchDuration)
                {
                    tRatio = t / switchDuration;
                    tAnimated = switchAnimation.Evaluate(tRatio);
                    tempCell._cellGo.transform.position = Vector3.Lerp(startPosition, endPosition, tAnimated);

                    t += Time.deltaTime;
                    yield return null;
                }
                tempCell._cellGo.transform.position = endPosition;

                GameObject tempGo = tempCell._cellGo;
                tempCell._cellGo = tempCell._adjacentCells[i]._cellGo;
                tempCell._adjacentCells[i]._cellGo = tempGo;
                tempGo.transform.position = GridHelper.GridPositionToWorldPosition(transform, tempCell._adjacentCells[i]._gridPosition, gridSettings);

                bool tempIsEmpty = tempCell._isEmpty;
                tempCell._isEmpty = tempCell._adjacentCells[i]._isEmpty;
                tempCell._adjacentCells[i]._isEmpty = tempIsEmpty;

                Debug.Log("I moved", tempGo);
                break;
            }
    }


    void RandomMaterial()
    {
        _currentMaterial = _tableOfMaterial[UnityEngine.Random.Range(0, _tableOfMaterial.Length)];
    }
}
