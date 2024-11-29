using System;

public class CellData
{
    public enum CellType
    {
        hole = -1,
        blank = 0,
        spiral = 1,
        star = 2,
        eyes = 3,
        pyramid = 4,
        fire = 5,
        thunder = 6,
        sheet = 7,
        water = 8
    }

    public CellType cellType;
    public Point point;
    private Cell _cell;

    public CellData(CellType cellType, Point point)
    {
        this.cellType = cellType;
        this.point = point;
    }

    public Cell GetCell()
    { 
        return _cell;
    }

    public void SetCell(Cell newCell)
    {
        _cell = newCell;
        if(_cell == null)
            cellType= CellType.blank;
        else
        {
            cellType = newCell.CellType;
            _cell.SetCellPoint(point);
        }
    }
}

