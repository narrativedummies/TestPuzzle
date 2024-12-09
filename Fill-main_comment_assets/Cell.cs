using UnityEngine;

// Компонент для управления состоянием ячейки на уровне.
public class Cell : MonoBehaviour
{
    [HideInInspector] public bool Blocked; // Определяет, является ли ячейка заблокированной.
    [HideInInspector] public bool Filled; // Определяет, заполнена ли ячейка.

    [SerializeField] private Color _blockedColor; // Цвет для заблокированных ячеек.
    [SerializeField] private Color _emptyColor; // Цвет для пустых ячеек.
    [SerializeField] private Color _filledColor; // Цвет для заполненных ячеек.
    [SerializeField] private SpriteRenderer _cellRenderer; // SpriteRenderer для изменения цвета ячейки.

    // Инициализирует ячейку на основе данных из уровня.
    public void Init(int fill)
    {
        Blocked = fill == 1; // Устанавливаем заблокированное состояние, если значение равно 1.
        Filled = Blocked; // Заполненная ячейка тоже считается заблокированной.
        _cellRenderer.color = Blocked ? _blockedColor : _emptyColor; // Устанавливаем цвет в зависимости от состояния.
    }

    // Заполняет ячейку и изменяет ее цвет.
    public void Add()
    {
        Filled = true;
        _cellRenderer.color = _filledColor;
    }

    // Очищает ячейку и восстанавливает цвет для пустой ячейки.
    public void Remove()
    {
        Filled = false;
        _cellRenderer.color = _emptyColor;
    }

    // Изменяет состояние блокировки ячейки и ее цвет.
    public void ChangeState()
    {
        Blocked = !Blocked; // Переключаем блокировку.
        Filled = Blocked; // Если ячейка заблокирована, то она считается заполненной.
        _cellRenderer.color = Blocked ? _blockedColor : _emptyColor; // Изменяем цвет.
    }
}
