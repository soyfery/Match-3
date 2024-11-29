using StaticData;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CellFactory))]

public class BoardService : MonoBehaviour
{
    public ArrayLayout boardLayout;
    [SerializeField] private Sprite[] _cellSprites;
    [SerializeField] private ScoreService _scoreService;
    [SerializeField] private ScoreService _scoreService2;
    [SerializeField] private HodService _hodService;
    public GameObject objectToEnable;

    public Sprite[] CellSprites;

    private int move;
    private int score;
    private int scet;
    public int sc;

    private CellData[,] _board;
    private CellFactory _cellFactory;
    private MatchMachine _matchMachine;
    private CellMover _cellMover;
    private HodService _hod;

    private readonly int[] _fillingCellCountByColumn = new int[Config.BoardWidth];
    private readonly List<Cell> _updatingCells = new List<Cell>();
    private readonly List<Cell> _deadCells = new List<Cell>();
    private readonly List<CellFlip> _flippedCells = new List<CellFlip>();

    private void Awake()
    {
        _matchMachine = new MatchMachine(this);
        _cellFactory = GetComponent<CellFactory>();
        _cellMover = new CellMover(this);
        move = 1;
        score = 15;
        scet = 30;
    }

    private void Start()
    {
        InitializeBoard();
        VerifyBoardOnMatches();
        _cellFactory.InstantiateBoard(this, _cellMover);
    }

    public void Update()
    {
        _cellMover.Update();

        var finishedUpdating = new List<Cell>();
        foreach (var cell in _updatingCells)
        {
            if (!cell.UpdateCell())
                finishedUpdating.Add(cell);
        }
        foreach (var cell in finishedUpdating)
        {
            var x = cell.Point.x;
            _fillingCellCountByColumn[x] = Mathf.Clamp(_fillingCellCountByColumn[x] - 1, 0, Config.BoardWidth);

            var flip = GetFlip(cell);
            var connectedPoints = _matchMachine.GetMatchPoints(cell.Point, true);
            Cell flippedCell = null;

            if (flip != null)
            {
                flippedCell = flip.GetOtherCell(cell);
                MatchMachine.AddPoints(
                    ref connectedPoints,
                    _matchMachine.GetMatchPoints(flippedCell.Point, true)
                    );
                _hodService.DelHod(move);
                scet -= 1;

                if (scet == 0)
                {
                    objectToEnable.SetActive(true);
                }
            }

            if (connectedPoints.Count == 0)
            {
                if (flippedCell != null)
                    FlipCells(cell.Point, flippedCell.Point, false);
            }
            else
            {
                foreach (var connectedPoint in connectedPoints)
                {
                    _cellFactory.KillCell(connectedPoint);
                    var cellAtPoint = GetCellAtPoint(connectedPoint);
                    var connectedCell = cellAtPoint.GetCell();
                    if (connectedCell != null)
                    {
                        connectedCell.gameObject.SetActive(false);
                        _deadCells.Add(connectedCell);
                    }
                    cellAtPoint.SetCell(null);
                }

                _scoreService.AddScore(score);
                _scoreService2.AddScore(score);


                ApplyGravityToBoard();
            }

            _flippedCells.Remove(flip);
            _updatingCells.Remove(cell);
        }
    }

    private void ApplyGravityToBoard()
    {
        for (int x = 0; x < Config.BoardWidth; x++)
        {
            for (int y = Config.BoardHeight - 1; y >= 0; y--)
            {
                var point = new Point(x, y);
                var cellData = GetCellAtPoint(point);
                var cellTypeAtPoint = GetCellTypeAtPoint(point);

                if (cellTypeAtPoint != 0)
                    continue;

                for (int newY = y - 1; newY >= -1; newY--)
                {
                    var nextPoint = new Point(x, newY);
                    var nextCellType = GetCellTypeAtPoint(nextPoint);
                    if (nextCellType == 0)
                        continue;
                    if (nextCellType != CellData.CellType.hole)
                    {
                        var cellAtPoint = GetCellAtPoint(nextPoint);
                        var cell = cellAtPoint.GetCell();

                        cellData.SetCell(cell);
                        _updatingCells.Add(cell);
                        cellAtPoint.SetCell(null);
                    }
                    else
                    {
                        var cellType = GetRandomCellType();
                        var fallPoint = new Point(x, -1 - _fillingCellCountByColumn[x]);
                        Cell cell;
                        if (_deadCells.Count > 0)
                        {
                            var revivedCell = _deadCells[0];
                            revivedCell.gameObject.SetActive(true);
                            cell = revivedCell;
                            _deadCells.RemoveAt(0);
                        }
                        else
                        {
                            cell = _cellFactory.InstantiateCell();
                        }

                        cell.Initialize(new CellData(cellType, point), _cellSprites[(int)(cellType - 1)], _cellMover);
                        cell.rect.anchoredPosition = GetBoardPositionFromPoint(fallPoint);

                        var holeCell = GetCellAtPoint(point);
                        holeCell.SetCell(cell);
                        ResetCell(cell);
                        _fillingCellCountByColumn[x]++;
                    }
                    break;
                }
            }
        }
    }

    public void FlipCells(Point firstPoint, Point secondPoint, bool main)
    {
        if (GetCellTypeAtPoint(firstPoint) < 0)
            return;

        var firstCellData = GetCellAtPoint(firstPoint);
        var firstCell = firstCellData.GetCell();
        if (GetCellTypeAtPoint(secondPoint) > 0)
        {
            var secondCellData = GetCellAtPoint(secondPoint);
            var secondCell = secondCellData.GetCell();
            firstCellData.SetCell(secondCell);
            secondCellData.SetCell(firstCell);

            if (main)
                _flippedCells.Add(new CellFlip(firstCell, secondCell));

            _updatingCells.Add(firstCell);
            _updatingCells.Add(secondCell);
        }
        else
        {
            ResetCell(firstCell);
        }
    }

    private CellFlip GetFlip(Cell cell)
    {
        foreach (var flip in _flippedCells)
            if (flip.GetOtherCell(cell) != null)
                return flip;
        return null;
    }

    public void ResetCell(Cell cell)
    {
        cell.ResetPosition();
        _updatingCells.Add(cell);
    }

    private void VerifyBoardOnMatches()
    {
        for (int y = 0; y < Config.BoardHeight; y++)
        {
            for (int x = 0; x < Config.BoardWidth; x++)
            {
                var point = new Point(x, y);
                var cellTypeAtPoint = GetCellTypeAtPoint(point);
                if (cellTypeAtPoint <= 0)
                    continue;

                var removeCellTypes = new List<CellData.CellType>();
                while (_matchMachine.GetMatchPoints(point, true).Count > 0)
                {
                    if (removeCellTypes.Contains(cellTypeAtPoint) == false)
                        removeCellTypes.Add(cellTypeAtPoint);
                    SetCellTypeAtPoint(point, GetNewCellType(ref removeCellTypes));
                }
            }
        }
    }

    private void SetCellTypeAtPoint(Point point, CellData.CellType newCellType)
    {
        _board[point.x, point.y].cellType = newCellType;
    }

    private CellData.CellType GetNewCellType(ref List<CellData.CellType> removeCellTypes)
    {
        var availableCellTypes = new List<CellData.CellType>();
        for (int i = 0; i < CellSprites.Length; i++)
        {
            availableCellTypes.Add((CellData.CellType)i + 1);
        }
        foreach (var removeCellType in removeCellTypes)
        {
            availableCellTypes.Remove(removeCellType);
        }
        return availableCellTypes.Count <= 0
            ? CellData.CellType.blank
            : availableCellTypes[UnityEngine.Random.Range(0, availableCellTypes.Count)];
    }

    public CellData.CellType GetCellTypeAtPoint(Point point)
    {
        if (point.x < 0 || point.x >= Config.BoardWidth
            || point.y < 0 || point.y >= Config.BoardHeight)
            return CellData.CellType.hole;
        return _board[point.x, point.y].cellType;
    }

    private void InitializeBoard()
    {
        _board = new CellData[Config.BoardWidth, Config.BoardHeight];
        for (int y = 0; y < Config.BoardHeight; y++)
        {
            for (int x = 0; x < Config.BoardWidth; x++)
            {
                _board[x, y] = new CellData(
                    boardLayout.rows[y].row[x] ? CellData.CellType.hole : GetRandomCellType(),
                    new Point(x, y));
            }
        }
    }
    public CellData GetCellAtPoint(Point point)
        => _board[point.x, point.y];

    public CellData.CellType GetRandomCellType()
    {
        return (CellData.CellType)(UnityEngine.Random.Range(0, _cellSprites.Length) + 1);
    }

    public static Vector2 GetBoardPositionFromPoint(Point point)
    {
        return new Vector2(Config.PieceSize / 2 + Config.PieceSize * point.x,
                            -Config.PieceSize / 2 - Config.PieceSize * point.y);
    }

}
