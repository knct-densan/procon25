using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chino.Solvers.y1rInternal
{
    struct MoveStatus
    {
        public List<Puzzle.Position> answer;
        public string pos;
    }

    class Puzzle : ICloneable, IComparable<Puzzle>
    {
        private int _leftOffset;
        private int _upOffset;

        /// <summary>
        /// 選択コスト.
        /// </summary>
        private int _choiceCost;

        /// <summary>
        /// 移動コスト.
        /// </summary>
        private int _moveCost;

        /// <summary>
        /// 最大選択回数.
        /// </summary>
        private int _maxChoice;
        public int maxChoice
        {
            get
            {
                return _maxChoice;
            }
        }

        /// <summary>
        /// puzzleを格納するint型配列.
        /// </summary>
        private int[] _puzzle;
        public int[] Data
        {
            get
            {
                return _puzzle;
            }
        }

        /// <summary>
        /// パズルの幅.
        /// </summary>
        private int _column;
        public int Column
        {
            get
            {
                return _column;
            }
        }

        /// <summary>
        /// パズルの高さ.
        /// </summary>
        private int _row;
        public int Row
        {
            get
            {
                return _row;
            }
        }

        /// <summary>
        /// パズルの初期の幅.
        /// </summary>
        private int _columnOrig;
        public int ColumnOrig
        {
            get
            {
                return _columnOrig;
            }
        }

        private int _rowOrig;
        public int RowOrig
        {
            get
            {
                return _rowOrig;
            }
        }

        private int _MD;
        public int MD
        {
            get
            {
                return _MD;
            }
        }

        /// <summary>
        /// 現在ブランク扱いされているタイルの値.
        /// </summary>
        private int _blank;
        public int Blank
        {
            get
            {
                return _blank;
            }
            set
            {
                _blank = value;
            }
        }

        /// <summary>
        /// 現在のブランク扱いされているタイルの位置.
        /// </summary>
        private int _blankPos;
        public int BlankPos
        {
            get
            {
                return _blankPos;
            }
        }

        /// <summary>
        /// 現在の移動の状態.
        /// これをもとにprocon-formatに変換し,
        /// 解答文字列を得る.
        /// </summary>
        private MoveStatus _nowStatus;
        public MoveStatus NowStatus
        {
            get
            {
                return _nowStatus;
            }
        }

        /// <summary>
        /// 移動が確定したもののコスト.
        /// これに移動状態のコストを加えると,現在のコストとなる.
        /// </summary>
        private int _cost = 0;

        /// <summary>
        /// 確定した移動のprocon_format
        /// </summary>
        private List<String> _proconFormat;

        /// <summary>
        /// 位置を示す列挙型
        /// </summary>
        public enum Position : byte
        {
            Up, Right, Left, Down
        };

        public enum Position2 : byte
        {
            UL, UR, LU, LD, RU, RD, DL, DR
        };

        /// <summary>
        /// コンストラクタ. Clone時にのみ使う.
        /// </summary>
        private Puzzle()
        {

        }

        /// <summary>
        /// コンストラクタ. puzzleマップの配列と,column, rowを必要とする.
        /// どのピースを抜いて解くことができるか検索し,解けない場合例外を送出する.(注意せよ)
        /// </summary>
        /// <param name="puzzle">puzzle map</param>
        /// <param name="column">column(1から数える)</param>
        /// <param name="row">row(1から数える)</param>
        public Puzzle(int[] puzzle, int column, int row, int maxChoice, int choiceCost, int moveCost)
        {
            // important!
            this._blank = -1;
            // important!

            this._MD = -1;
            this._choiceCost = choiceCost;
            this._column = column;
            this._columnOrig = column;
            this._cost = 0;
            this._maxChoice = maxChoice;
            this._moveCost = moveCost;
            this._nowStatus = new MoveStatus();
            this._nowStatus.answer = new List<Position>();
            this._proconFormat = new List<String>(this._maxChoice);
            this._puzzle = (int[])puzzle.Clone();
            this._row = row;
            this._rowOrig = row;

            this._leftOffset = 0;
            this._upOffset = 0;
        }

        /// <summary>
        /// CompareToメソッド.
        /// 最適化済みの解答を元に,解答コストを比較します.
        /// </summary>
        /// <param name="otherPuzzle"></param>
        /// <returns>int</returns>
        public int CompareTo(Puzzle otherPuzzle)
        {
            if (otherPuzzle != null)
            {
                int thisMoveCost = this.CalculateCost();
                int otherMoveCost = otherPuzzle.CalculateCost();
                if (this.MD == -1)
                    this.CalculateMD();
                if (otherPuzzle.MD == -1)
                    otherPuzzle.CalculateMD();

                return (thisMoveCost + this.MD * this._moveCost).CompareTo(
                    otherMoveCost + otherPuzzle.MD * otherPuzzle._moveCost);
                
                /*
                if ((status = thisMoveCost.CompareTo(otherMoveCost)) == 0)
                {
                    if (this.MD == -1)
                        this.CalculateMD();
                    if (otherPuzzle.MD == -1)
                        otherPuzzle.CalculateMD();
                    return this.MD.CompareTo(otherPuzzle.MD);
                }
                else
                    return status;
                */
                /*
                int thisMoveCost = this.CalculateCost();
                int otherMoveCost = otherPuzzle.CalculateCost();

                if (this.MD == -1)
                    this.CalculateMD();
                int thisMD = this.MD;

                if (otherPuzzle.MD == -1)
                    otherPuzzle.CalculateMD();
                int otherMD = otherPuzzle.MD;

                if (thisMD.CompareTo(otherMD) == 0)
                    return thisMoveCost.CompareTo(otherMoveCost);
                else
                    return thisMD.CompareTo(otherMD);
                */
            }

            return 0;
        }

        /// <summary>
        /// PuzzleをCloneします.
        /// </summary>
        /// <returns>clone_puzzle</returns>
        public object Clone()
        {
            Puzzle clonePuzzle = new Puzzle();
            clonePuzzle._MD = this._MD;
            clonePuzzle._blank = this._blank;
            clonePuzzle._blankPos = this._blankPos;
            clonePuzzle._choiceCost = this._choiceCost;
            clonePuzzle._column = this._column;
            clonePuzzle._columnOrig = this._columnOrig;
            clonePuzzle._cost = this._cost;
            clonePuzzle._maxChoice = this._maxChoice;
            clonePuzzle._moveCost = this._moveCost;
            clonePuzzle._nowStatus = new MoveStatus();
            clonePuzzle._nowStatus.answer = new List<Position>(this._nowStatus.answer);
            if (_nowStatus.pos == null)
                clonePuzzle._nowStatus.pos = null;
            else
                clonePuzzle._nowStatus.pos = (String)this._nowStatus.pos.Clone();
            clonePuzzle._proconFormat = new List<String>(this._proconFormat.Count);
            for (int i = 0; i < this._proconFormat.Count; i++)
                clonePuzzle._proconFormat.Add((String)this._proconFormat[i].Clone());
            clonePuzzle._puzzle = (int[])this._puzzle.Clone();
            clonePuzzle._row = this._row;
            clonePuzzle._rowOrig = this._rowOrig;
            clonePuzzle._leftOffset = this._leftOffset;
            clonePuzzle._upOffset = this._upOffset;

            return clonePuzzle;
        }

        /// <summary>
        /// 位置を反転します.
        /// </summary>
        /// <param name="pos">Position 列挙型</param>
        /// <returns>反転したPosition</returns>
        public Position ReversePosition(Position pos)
        {
            switch (pos)
            {
                case Position.Down:
                    return Position.Up;

                case Position.Up:
                    return Position.Down;

                case Position.Left:
                    return Position.Right;

                case Position.Right:
                    return Position.Left;

            }

            throw new Exception();
        }

        /// <summary>
        /// positionを用いてパズルにアクセスする
        /// </summary>
        /// <param name="pos">パズルの位置</param>
        /// <returns></returns>
        public int this[int pos]
        {
            get
            {
                return _puzzle[pos];
            }

            private set
            {
                _puzzle[pos] = value;
            }

        }

        /// <summary>
        /// 座標を用いてパズルにアクセスする
        /// </summary>
        /// <param name="column">横</param>
        /// <param name="row">縦</param>
        /// <returns></returns>
        public int this[int column, int row]
        {
            get
            {
                return _puzzle[CoordToPosition(column, row)];
            }

            private set
            {
                _puzzle[CoordToPosition(column, row)] = value;
            }
        }

        /// <summary>
        /// 座標を位置に変換する
        /// </summary>
        /// <param name="pos">ref 位置を格納する</param>
        /// <param name="column">横(1から数える)</param>
        /// <param name="row">縦(1から数える)</param>
        public void CoordToPosition(ref int pos, int column, int row)
        {
            pos = CoordToPosition(column, row);
        }

        /// <summary>
        /// 座標を位置に変換する
        /// </summary>
        /// <param name="column">横(1から数える)</param>
        /// <param name="row">縦(1から数える)</param>
        /// <returns>位置</returns>
        public int CoordToPosition(int column, int row)
        {
            return (row - 1) * _column + (column - 1);
        }

        /// <summary>
        /// 位置を座標に変換する
        /// </summary>
        /// <param name="pos">位置</param>
        /// <param name="column">横(1から数える)</param>
        /// <param name="row">縦(1から数える)</param>
        public void PositionToCoord(int pos, ref int column, ref int row)
        {
            column = pos % _column + 1;
            row = pos / _column + 1;
        }

        /// <summary>
        /// タイルの数値から位置を求めます.
        /// </summary>
        /// <param name="tile">タイルの数値</param>
        /// <returns>位置</returns>
        public int TileToPosition(int tile)
        {
            for (int i = 0; i < _puzzle.Length; i++)
                if (_puzzle[i] == tile)
                    return i;

            throw new Exception();
        }

        /// <summary>
        /// ブランクのポジションと現在の位置を同期します.
        /// </summary>
        public void SyncBlankPosition()
        {
            _blankPos = TileToPosition(_blank);
        }

        /// <summary>
        /// 位置を文字に変換します.
        /// </summary>
        /// <param name="pos">位置</param>
        /// <returns>U,D,L,R</returns>
        public char PositionToChar(Position pos)
        {
            switch (pos)
            {
                case Position.Up:
                    return 'U';
                case Position.Down:
                    return 'D';
                case Position.Left:
                    return 'L';
                case Position.Right:
                    return 'R';
            }

            throw new Exception();
        }

        /// <summary>
        /// 文字を位置に変換します.
        /// </summary>
        /// <param name="pos">U,D,L,R</param>
        /// <returns>位置</returns>
        public Position CharToPosition(char pos)
        {
            switch (pos)
            {
                case 'U':
                    return Position.Up;
                case 'D':
                    return Position.Down;
                case 'L':
                    return Position.Left;
                case 'R':
                    return Position.Right;
            }

            throw new Exception();
        }

        /// <summary>
        /// pos位置を並べて,パズルをサイズダウンすることができるか判定します.
        /// CanCutメソッドと異なり,並んでいるかどうかの判定は行いません.
        /// </summary>
        /// <param name="pos">Position 列挙型</param>
        /// <returns>bool</returns>
        public bool CanUse(Position pos)
        {
            int column = -1, row = -1;
            bool status = false;
            PositionToCoord(_blank, ref column, ref row);

            switch (pos)
            {
                case Position.Up:
                    status = row != 1;
                    break;

                case Position.Down:
                    status = row != _row;
                    break;

                case Position.Left:
                    status = column != 1;
                    break;

                case Position.Right:
                    status = column != _column;
                    break;
            }

            return status;
        }

        /// <summary>
        /// 移動ができるかを判定します.
        /// </summary>
        /// <param name="pos">Position 列挙型</param>
        /// <returns>bool</returns>
        public bool CanMove(Position pos)
        {
            bool status = false;
            int column = -1, row = -1;
            PositionToCoord(BlankPos, ref column, ref row);

            switch (pos)
            {
                case Position.Up:
                    status = row != 1;
                    break;
                case Position.Down:
                    status = row != _row;
                    break;
                case Position.Left:
                    status = column != 1;
                    break;
                case Position.Right:
                    status = column != _column;
                    break;
            }

            return status;
        }

        /// <summary>
        /// posで切断可能かを判定します.
        /// ブランクが切り取り範囲に入っている場合や,まだ揃っていない場合にfalseを返します.
        /// </summary>
        /// <param name="pos">Position 列挙型</param>
        /// <returns></returns>
        public bool CanCut(Position pos)
        {
            int position;

            if (!CanUse(pos))
                return false;

            switch (pos)
            {
                case Position.Down:
                    for (int i = 1; i <= _column; i++)
                    {
                        position = CoordToPosition(i, _row);
                        if (_puzzle[position] != position)
                            return false;
                    }
                    break;

                case Position.Up:
                    for (int i = 1; i <= _column; i++)
                    {
                        position = CoordToPosition(i, 1);
                        if (_puzzle[position] != position)
                            return false;
                    }
                    break;

                case Position.Left:
                    for (int i = 1; i <= _row; i++)
                    {
                        position = CoordToPosition(1, i);
                        if (_puzzle[position] != position)
                            return false;
                    }
                    break;

                case Position.Right:
                    for (int i = 1; i <= _row; i++)
                    {
                        position = CoordToPosition(_column, i);
                        if (_puzzle[position] != position)
                            return false;
                    }
                    break;
            }

            return true;
        }


        public bool CanMove(int column, int row, int goal, Puzzle.Position2 forbid)
        {
            bool status = true;
            int l_column = -1;
            int l_row = -1;

            PositionToCoord(goal, ref l_column, ref l_row);
            switch (forbid)
            {
                case Puzzle.Position2.UR:

                    l_row = 1;
                    // 端を入れている時
                    if (l_column == this.Column)
                        l_column = this.Column - 2;
                    else
                        l_column--;

                    /*
                    l_row = 1;
                    l_column--;
                    */
                    break;

                case Puzzle.Position2.UL:

                    l_row = 1;
                    // 端を入れている時
                    if (l_column == 1)
                        l_column = 3;
                    else
                        l_column++;
                    /*
                    l_row = 1;
                    l_column++;
                    */
                    break;

                case Puzzle.Position2.DR:

                    l_row = this.Row;
                    // 端を入れている時
                    if (l_column == this.Column)
                        l_column = this.Column - 2;
                    else
                        l_column--;
                    /*
                    l_row = this.Row;
                    l_column--;
                    */
                    break;

                case Puzzle.Position2.DL:
                    l_row = this.Row;
                    if (l_column == 1)
                        l_column = 3;
                    else
                        l_column++;
                    break;

                case Puzzle.Position2.LD:

                    l_column = 1;
                    //端を入れている時
                    if (l_row == this.Row)
                        l_row = this.Row - 2;
                    else
                        l_row--;
                    /*
                    l_column = 1;
                    l_row--;
                    */
                    break;

                case Puzzle.Position2.LU:
                    l_column = 1;
                    if (l_row == 1)
                        l_row = 3;
                    else
                        l_row++;
                    break;

                case Puzzle.Position2.RD:

                    l_column = this.Column;
                    //端を入れている時
                    if (l_row == this.Row)
                        l_row = this.Row - 2;
                    else
                        l_row--;
                    /*
                    l_column = this.Column;
                    l_row--;
                    */
                    break;

                case Puzzle.Position2.RU:
                    /*
                    l_column = this.Column;
                    l_row++;
                    */
                    l_column = this.Column;
                    if (l_row == 1)
                        l_row = 3;
                    else
                        l_row++;
                    break;
            }

            switch (forbid)
            {
                case Puzzle.Position2.UR:
                    status = status && ((l_row != row) || (l_column < column));
                    break;

                case Puzzle.Position2.DR:
                    status = status && ((l_row != row) || (l_column < column));
                    break;

                case Puzzle.Position2.LD:
                    status = status && ((l_column != column) || (l_row < row));
                    break;

                case Puzzle.Position2.RD:
                    status = status && ((l_column != column) || (l_row < row));
                    break;

                case Puzzle.Position2.UL:
                    status = status && ((l_row != row) || (column < l_column));
                    break;

                case Puzzle.Position2.DL:
                    status = status && ((l_row != row) || (column < l_column));
                    break;

                case Puzzle.Position2.LU:
                    status = status && ((l_column != column) || (row < l_row));
                    break;

                case Puzzle.Position2.RU:
                    status = status && ((l_column != column) || (row < l_row));
                    break;

            }

            return status;
        }

        /// <summary>
        /// posで切断します.
        /// 切断できない場合,falseを返します.
        /// </summary>
        /// <param name="pos">Position 列挙型</param>
        /// <returns>bool</returns>
        public bool DoCut(Position pos)
        {
            int column = -1, row = -1;
            int[] new_puzzle = null;
            int newBlank = -1;
            int j = 0;

            /*
            Console.WriteLine("DoCut-START");
            for (int a = 1; a <= this.Row; a++)
            {
                for (int b = 1; b <= this.Column; b++)
                    Console.Write(this._puzzle[this.CoordToPosition(b, a)] + "\t");
                Console.WriteLine();
            }
            Console.WriteLine();
            */
            

            if (!CanCut(pos))
                return false;

            switch (pos)
            {
                case Position.Up:
                    new_puzzle = new int[(_row - 1) * _column];
                    j = 0;
                    for (int i = 0; i < _puzzle.Length; i++)
                    {
                        PositionToCoord(i, ref column, ref row);
                        if (row != 1)
                        {
                            if (_puzzle[i] == this._blank)
                                newBlank = _puzzle[i] - _column;
                            new_puzzle[j] = _puzzle[i] - _column;
                            j++;
                        }
                    }
                    _row--;
                    _upOffset++;
                    break;

                case Position.Down:
                    new_puzzle = new int[(_row - 1) * _column];
                    j = 0;
                    for (int i = 0; i < _puzzle.Length; i++)
                    {
                        PositionToCoord(i, ref column, ref row);
                        if (row != _row)
                        {
                            if (_puzzle[i] == this._blank)
                                newBlank = _puzzle[i];
                            new_puzzle[j] = _puzzle[i];
                            j++;
                        }
                    }
                    _row--;
                    break;

                case Position.Left:
                    new_puzzle = new int[(_column - 1) * _row];
                    j = 0;
                    for (int i = 0; i < _puzzle.Length; i++)
                    {
                        PositionToCoord(i, ref column, ref row);
                        if (column != 1)
                        {
                            if (_puzzle[i] == this._blank)
                                newBlank = _puzzle[i] - (_puzzle[i] / _column + 1);
                            new_puzzle[j] = _puzzle[i] - (_puzzle[i] / _column + 1);
                            j++;
                        }
                    }
                    _column--;
                    _leftOffset++;
                    break;

                case Position.Right:
                    new_puzzle = new int[(_column - 1) * _row];
                    j = 0;
                    for (int i = 0; i < _puzzle.Length; i++)
                    {
                        PositionToCoord(i, ref column, ref row);
                        if (column != _column)
                        {
                            if (_puzzle[i] == this._blank)
                                newBlank = _puzzle[i] - _puzzle[i] / _column;
                            new_puzzle[j] = _puzzle[i] - _puzzle[i] / _column;
                            j++;
                        }
                    }
                    _column--;
                    break;
            }

            /*
            for (int i = 0; i < CanBeBlank.Count; i++)
                if (new_puzzle.Length <= CanBeBlank[i])
                    CanBeBlank.RemoveAt(i);
             */
            
            /*
            Console.WriteLine();
            for (int a = 1; a <= this.Row; a++)
            {
                for (int b = 1; b <= this.Column; b++)
                    Console.Write(new_puzzle[this.CoordToPosition(b, a)] + "\t");
                Console.WriteLine();
            }
            Console.WriteLine("DoCut-END");
            */            

            if (new_puzzle == null)
                throw new Exception();

            _puzzle = new_puzzle;
            this._blank = newBlank;
            SyncBlankPosition();

            return true;
        }

        /// <summary>
        /// posに従って,moveを行います.
        /// 移動ができない場合,falseを返します.
        /// </summary>
        /// <param name="pos">Position 列挙型</param>
        /// <returns>bool</returns>
        public bool DoMove(Position pos)
        {
            if (CanMove(pos))
            {
                switch (pos)
                {
                    case Position.Up:
                        _puzzle[BlankPos] = _puzzle[BlankPos - _column];
                        _puzzle[BlankPos - _column] = _blank;
                        _blankPos -= _column;
                        break;

                    case Position.Down:
                        _puzzle[BlankPos] = _puzzle[BlankPos + _column];
                        _puzzle[BlankPos + _column] = _blank;
                        _blankPos += _column;
                        break;

                    case Position.Right:
                        _puzzle[BlankPos] = _puzzle[BlankPos + 1];
                        _puzzle[BlankPos + 1] = _blank;
                        _blankPos += 1;
                        break;

                    case Position.Left:
                        _puzzle[BlankPos] = _puzzle[BlankPos - 1];
                        _puzzle[BlankPos - 1] = _blank;
                        _blankPos -= 1;
                        break;
                    default:
                        throw new Exception();
                }
//                Console.WriteLine(PositionToChar(pos));
                _nowStatus.answer.Add(pos);
//                Console.Write(MoveStatusToProconFormat(_nowStatus));


                this.DumpPuzzle();
//                this.CalculateMD();
                return true;
            }

            return false;
        }

        public bool DoMoves(Position[] pos, Puzzle.Position2 forbid, int goal)
        {
            int i;
            int l_column = -1;
            int l_row = -1;
            int b_column = -1;
            int b_row = -1;

            PositionToCoord(goal, ref l_column, ref l_row);
            switch (forbid)
            {
                case Puzzle.Position2.UR:
                    l_row = 1;
                    // 端を入れている時
                    if (l_column == this.Column)
                        l_column = this.Column - 2;
                    else
                        l_column--;
                    break;

                case Puzzle.Position2.DR:
                    l_row = this.Row;
                    // 端を入れている時
                    if (l_column == this.Column)
                        l_column = this.Column - 2;
                    else
                        l_column--;
                    break;

                case Puzzle.Position2.LD:
                    l_column = 1;
                    //端を入れている時
                    if (l_row == this.Row)
                        l_row = this.Row - 2;
                    else
                        l_row--;
                    break;

                case Puzzle.Position2.RD:
                    l_column = this.Column;
                    //端を入れている時
                    if (l_row == this.Row)
                        l_row = this.Row - 2;
                    else
                        l_row--;
                    break;

                case Puzzle.Position2.UL:
                    // 左へ上を並べる
                    l_row = 1;
                    // 端を入れている時
                    if (l_column == 1)
                        l_column = 3;
                    else
                        l_column++;

                    break;

                case Puzzle.Position2.LU:
                    //上へ左を並べる
                    l_column = 1;
                    //端を入れている時
                    if (l_row == 1)
                        l_row = 3;
                    else
                        l_row++;
                    break;

                case Puzzle.Position2.RU:
                    l_column = this.Column;
                    //端を入れている時
                    if (l_row == 1)
                        l_row = 3;
                    else
                        l_row++;
                    //上へ右を並べる
                    break;

                case Puzzle.Position2.DL:
                    //左へ下を並べる
                    l_row = this.Row;
                    // 端を入れている時
                    if (l_column == 1)
                        l_column = 3;
                    else
                        l_column++;
                    break;

            }

            bool moveStatus = true;
            bool positionStatus = true;
            for (i = 0; i < pos.Length; i++)
            {
                moveStatus = DoMove(pos[i]);

                PositionToCoord(this.BlankPos, ref b_column, ref b_row);

                switch (forbid)
                {
                    case Puzzle.Position2.UR:
                        positionStatus = ((l_row != b_row) || (l_column < b_column));
                        break;

                    case Puzzle.Position2.DR:
                        positionStatus = ((l_row != b_row) || (l_column < b_column));
                        break;

                    case Puzzle.Position2.LD:
                        positionStatus = ((l_column != b_column) || (l_row < b_row));
                        break;

                    case Puzzle.Position2.RD:
                        positionStatus = ((l_column != b_column) || (l_row < b_row));
                        break;

                    case Puzzle.Position2.UL:
                        positionStatus = ((l_row != b_row) || (b_column < l_column));
                        break;

                    case Puzzle.Position2.DL:
                        positionStatus = ((l_row != b_row) || (b_column < l_column));
                        break;

                    case Puzzle.Position2.LU:
                        positionStatus = ((l_column != b_column) || (b_row < l_row));
                        break;

                    case Puzzle.Position2.RU:
                        positionStatus = ((l_column != b_column) || (b_row < l_row));
                        break;


                }

                if (!moveStatus)
                {
                    CancelMoves(i);
                    break;
                }
                if (!positionStatus)
                {
                    CancelMoves(i + 1);
                    break;
                }
            }

            if (i == pos.Length)
                return true;
            else
                return false;

        }

        /// <summary>
        /// posに従って,複数回のmoveを行います.
        /// 移動ができない場合,falseを返します.
        /// </summary>
        /// <param name="pos">Position 列挙型の配列</param>
        /// <returns>bool</returns>
        public bool DoMoves(Position[] pos)
        {
            int i;
            bool status = true;

            for (i = 0; i < pos.Length; i++)
            {
                status = DoMove(pos[i]);
                if (!status)
                {
                    CancelMoves(i);
                    break;
                }
            }

            if (i == pos.Length)
                return true;
            else
                return false;
        }

        /// <summary>
        /// stringに従って,複数回のmoveを行います
        /// </summary>
        /// <param name="moves"></param>
        /// <returns></returns>
        public bool DoMoves(string moves)
        {
            Position[] pos = new Position[moves.Length];

            for (int i = 0; i < moves.Length; i++)
            {
                pos[i] = CharToPosition(moves[i]);
            }

            return DoMoves(pos);

        }

        public bool DoMoves(string moves, Puzzle.Position2 forbid, int goal)
        {
            Position[] pos = new Position[moves.Length];

            for (int i = 0; i < moves.Length; i++)
                pos[i] = CharToPosition(moves[i]);

            return DoMoves(pos, forbid, goal);
        }

        /// <summary>
        /// 直前の移動をキャンセルします.
        /// </summary>
        public void CancelMove()
        {
            Position move = _nowStatus.answer[_nowStatus.answer.Count - 1];
            Position revMove = ReversePosition(move);

            if (!DoMove(revMove))
                throw new Exception();

            _nowStatus.answer.RemoveRange(_nowStatus.answer.Count - 2, 2);
        }

        /// <summary>
        /// 直前の移動を複数回キャンセルします.
        /// </summary>
        /// <param name="count">回数</param>
        public void CancelMoves(int count)
        {
            for (int i = 0; i < count; i++)
                CancelMove();
        }

        private string intToHex(int i)
        {
            if (0 <= i && i <= 9)
                return i.ToString();

            return ((char)((int)'A' + (i - 10))).ToString();
        }

        public void Choice(int tile)
        {
            if (_blank != tile)
            {
                this._maxChoice--;
                int pos = TileToPosition(tile);
                int column = -1, row = -1;
                PositionToCoord(pos, ref column, ref row);
                MoveStatus newStatus = new MoveStatus();
                newStatus.answer = new List<Position>();
                newStatus.pos = intToHex(_leftOffset + column - 1) + intToHex(_upOffset + row - 1);
                if( this.NowStatus.answer.Count != 0 )
                    this._proconFormat.Add(MoveStatusToProconFormat(_nowStatus));
                if (_nowStatus.answer.Count != 0)
                {
                    this._cost += _nowStatus.answer.Count * this._moveCost;
                    this._cost += this._choiceCost;
                }

                _nowStatus = newStatus;
                this._blank = tile;
                SyncBlankPosition();
            }
            else
                return;
        }

        private String MoveStatusToProconFormat(MoveStatus status)
        {
            StringBuilder line = new StringBuilder(status.answer.Count);
            foreach(var move in status.answer)
            {
                line.Append(PositionToChar(move));
            }

            String newline = "\r\n";
            String answer = Common.optimizeLine(line.ToString());
            StringBuilder ret = new StringBuilder(status.pos).Append(newline);
            ret.Append(answer.Length).Append(newline);
            ret.Append(answer).Append(newline);
            return ret.ToString();
        }

        /// <summary>
        /// パズルが解かれているか調べます.
        /// </summary>
        /// <returns>bool</returns>
        public bool IsPuzzleSolved()
        {
            for (int i = 0; i < _puzzle.Length; i++)
            {
                if (_puzzle[i] != i)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 最適化済みのパズルの解答を返します.
        /// パズルが完成していなくても,現在の最適化済みの解答を返します.
        /// </summary>
        /// <returns>解答</returns>
        public string GetSolution()
        {
            List<String> solution = new List<String>(this._proconFormat.Count + 1);
            StringBuilder answer = new StringBuilder();

            for (int i = 0; i < this._proconFormat.Count; i++)
                solution.Add(this._proconFormat[i]);

            solution.Add(MoveStatusToProconFormat(_nowStatus));

            answer.AppendLine(solution.Count.ToString());
            for (int i = 0; i < solution.Count; i++)
                answer.Append(solution[i]);

            return answer.ToString();
        }

        /// <summary>
        /// パズルの内容をダンプします.
        /// </summary>
        public void DumpPuzzle()
        {
            /*
            Console.WriteLine();
            for (int i = 1; i <= this.Row; i++)
            {
                for (int j = 1; j <= this.Column; j++)
                {
                    Console.Write(this[j, i].ToString() + '\t');
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            */                                                            
        }


        public int CalculateCost()
        {
            int cost = this._cost;
            if (this._nowStatus.answer.Count != 0)
                cost += this._choiceCost;
            cost += this._nowStatus.answer.Count * this._moveCost;

            return cost;
        }

        /// <summary>
        /// Manhattan Distanceを計算します
        /// </summary>
        public void CalculateMD()
        {
            int MD = 0;
            int column1 = 0, column2 = 0, row1 = 0, row2 = 0;
            for (int i = 0; i < this.Row * this.Column; i++)
            {
                if (this[i] == this.Blank)
                    continue;
                this.PositionToCoord(this[i], ref column1, ref row1);
                this.PositionToCoord(i, ref column2, ref row2);
                MD += Math.Abs(column1 - column2) + Math.Abs(row1 - row2);
            }

            this._MD = MD;
        }

    }
}
