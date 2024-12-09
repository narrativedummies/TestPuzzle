using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Управляет процессом игры, включая создание уровня и взаимодействие пользователя.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Синглтон для глобального доступа к GameManager.

    [SerializeField] private Level _level; // Ссылка на данные текущего уровня.
    [SerializeField] private Cell _cellPrefab; // Префаб ячейки для создания игрового поля.
    [SerializeField] private Transform _edgePrefab; // Префаб линии для соединения ячеек.

    private bool hasGameFinished; // Флаг завершения игры.
    private Cell[,] cells; // Двумерный массив для хранения всех ячеек на уровне.
    private List<Vector2Int> filledPoints; // Список заполненных точек.
    private List<Transform> edges; // Список линий, соединяющих ячейки.
    private Vector2Int startPos, endPos; // Начальная и конечная точки для соединения.
    private List<Vector2Int> directions = new List<Vector2Int>()
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right // Возможные направления для соединения ячеек.
    };

    private void Awake()
    {
        Instance = this; // Устанавливаем синглтон.
        hasGameFinished = false; // Инициализируем флаг завершения.
        filledPoints = new List<Vector2Int>(); // Создаем список заполненных точек.
        cells = new Cell[_level.Row, _level.Col]; // Создаем массив ячеек на основе данных уровня.
        edges = new List<Transform>(); // Создаем список линий.
        SpawnLevel(); // Создаем уровень.
    }

    // Создает уровень, размещая ячейки на экране.
    private void SpawnLevel()
    {
        Vector3 camPos = Camera.main.transform.position; // Получаем позицию камеры.
        camPos.x = _level.Col * 0.5f; // Смещаем камеру по оси X для центрирования уровня.
        camPos.y = _level.Row * 0.5f; // Смещаем камеру по оси Y.
        Camera.main.transform.position = camPos; // Устанавливаем новую позицию камеры.
        Camera.main.orthographicSize = Mathf.Max(_level.Row, _level.Col) + 2f; // Настраиваем размер камеры.

        // Генерируем ячейки на основе данных уровня.
        for (int i = 0; i < _level.Row; i++)
        {
            for (int j = 0; j < _level.Col; j++)
            {
                cells[i, j] = Instantiate(_cellPrefab); // Создаем новую ячейку.
                cells[i, j].Init(_level.Data[i * _level.Col + j]); // Инициализируем ячейку данными уровня.
                cells[i, j].transform.position = new Vector3(j + 0.5f, i + 0.5f, 0); // Устанавливаем позицию ячейки.
            }
        }
    }

    private void Update()
    {
        if (hasGameFinished) return; // Если игра завершена, не обрабатываем ввод.

        // Обработка нажатия левой кнопки мыши.
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Преобразуем координаты мыши в мировые координаты.
            startPos = new Vector2Int(Mathf.FloorToInt(mousePos.y), Mathf.FloorToInt(mousePos.x)); // Запоминаем начальную позицию.
            endPos = startPos; // Изначально конечная позиция совпадает с начальной.
        }
        else if (Input.GetMouseButton(0)) // Удержание левой кнопки мыши.
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Получаем текущую позицию мыши.
            endPos = new Vector2Int(Mathf.FloorToInt(mousePos.y), Mathf.FloorToInt(mousePos.x)); // Определяем конечную позицию.

            if (!IsNeighbour()) return; // Если ячейки не соседние, ничего не делаем.

            // Проверяем различные сценарии взаимодействия с ячейками.
            if (AddEmpty()) // Начинаем новую цепочку заполнения.
            {
                filledPoints.Add(startPos);
                filledPoints.Add(endPos);
                cells[startPos.x, startPos.y].Add();
                cells[endPos.x, endPos.y].Add();
                CreateEdge(startPos, endPos);
            }
            else if (AddToEnd()) // Продолжаем цепочку в конец.
            {
                filledPoints.Add(endPos);
                cells[endPos.x, endPos.y].Add();
                CreateEdge(startPos, endPos);
            }
            else if (AddToStart()) // Продолжаем цепочку в начало.
            {
                filledPoints.Insert(0, endPos);
                cells[endPos.x, endPos.y].Add();
                CreateEdge(startPos, endPos, true);
            }
            else if (RemoveFromEnd()) // Удаляем ячейку из конца цепочки.
            {
                RemoveLastEdge();
            }
            else if (RemoveFromStart()) // Удаляем ячейку из начала цепочки.
            {
                RemoveFirstEdge();
            }

            RemoveEmpty(); // Убираем пустые ячейки, если это требуется.
            CheckWin(); // Проверяем, завершена ли игра.
            startPos = endPos; // Обновляем начальную позицию для следующей итерации.
        }
    }

    // Создает соединение (линию) между ячейками.
    private void CreateEdge(Vector2Int start, Vector2Int end, bool insertAtStart = false)
    {
        Transform edge = Instantiate(_edgePrefab); // Создаем новый объект линии.
        if (insertAtStart)
            edges.Insert(0, edge); // Добавляем линию в начало списка.
        else
            edges.Add(edge); // Добавляем линию в конец списка.

        // Рассчитываем позицию линии между ячейками.
        edge.transform.position = new Vector3(
            start.y * 0.5f + 0.5f + end.y * 0.5f,
            start.x * 0.5f + 0.5f + end.x * 0.5f,
            0f
        );

        // Определяем ориентацию линии.
        bool horizontal = (end.y - start.y) != 0;
        edge.transform.eulerAngles = new Vector3(0, 0, horizontal ? 90f : 0);
    }

    // Удаляет последнюю линию и ячейку из цепочки.
    private void RemoveLastEdge()
    {
        Transform removeEdge = edges[edges.Count - 1]; // Получаем последнюю линию.
        edges.RemoveAt(edges.Count - 1); // Удаляем ее из списка.
        Destroy(removeEdge.gameObject); // Уничтожаем объект линии.
        filledPoints.RemoveAt(filledPoints.Count - 1); // Удаляем последнюю заполненную точку.
        cells[startPos.x, startPos.y].Remove(); // Очищаем ячейку.
    }

    // Удаляет первую линию и ячейку из цепочки.
    private void RemoveFirstEdge()
    {
        Transform removeEdge = edges[0]; // Получаем первую линию.
        edges.RemoveAt(0); // Удаляем ее из списка.
        Destroy(removeEdge.gameObject); // Уничтожаем объект линии.
        filledPoints.RemoveAt(0); // Удаляем первую заполненную точку.
        cells[startPos.x, startPos.y].Remove(); // Очищаем ячейку.
    }

    // Проверяет, являются ли две ячейки соседними.
    private bool IsNeighbour()
    {
        return IsValid(startPos) && IsValid(endPos) && directions.Contains(startPos - endPos);
    }

    // Проверяет, находится ли точка внутри уровня.
    private bool IsValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < _level.Row && pos.y < _level.Col;
    }

    // Проверяет, заполнены ли все ячейки, чтобы завершить игру.
    private void CheckWin()
    {
        for (int i = 0; i < _level.Row; i++)
        {
            for (int j = 0; j < _level.Col; j++)
            {
                if (!cells[i, j].Filled) // Если найдена незаполненная ячейка, игра продолжается.
                    return;
            }
        }

        hasGameFinished = true; // Устанавливаем флаг завершения игры.
        StartCoroutine(GameFinished()); // Запускаем процесс завершения игры.
    }

    // Выводит сообщение о победе и перезапускает сцену.
    private IEnumerator GameFinished()
    {
        Debug.Log("WIN!!!"); // Вывод сообщения о победе в консоль.
        yield return new WaitForSeconds(2f); // Задержка перед перезапуском.
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Перезагрузка текущей сцены.
    }
}

