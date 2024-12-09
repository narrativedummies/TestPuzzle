using System.Collections.Generic;
using UnityEngine;

// Позволяет создать объект ScriptableObject для хранения данных уровня в Unity Editor.
[CreateAssetMenu(fileName = "Level", menuName = "Level")]
public class Level : ScriptableObject
{
    public int Row; // Количество строк на уровне.
    public int Col; // Количество столбцов на уровне.
    public List<int> Data; // Список чисел, определяющий конфигурацию уровня (например, заблокированные ячейки).
}
