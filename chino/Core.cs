using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;

namespace chino
{
    /// <summary>
    /// 競技用の画面。
    /// </summary>
    public partial class Core : Form
    {
        /// <summary>
        /// 問題情報
        /// </summary>
        private String _problemUrl, _problemID, _submitUrl, _token;

        /// <summary>
        /// 使用するSoterアルゴリズムの番号を保持する
        /// </summary>
        private int _sorterNum;

        /// <summary>
        /// ファイルの保存用ディレクトリ
        /// </summary>
        private String _workingDirectoryPath;

        ///
        /// 並び替え後の位置としてデータが保持されているもの:
        ///     _pBoxs , _pBoxsGraphics  _answerMap , _answerMapSelected , _answerMapFixed , _answerMapStatusChanged
        /// 
        ///   /\
        ///   ||    (並び替え後のものから元画像への対応は _answerMap[] で得られる)
        ///   \/
        /// 
        /// 元画像の位置としてデータが保持されているもの: 
        ///     Problem.* ,  _bm
        /// 

        /// <summary>
        /// パズル表示用PictureBox. i列j行目には (i * column + j) でアクセス
        /// </summary>
        private PictureBox[] _pBoxs;

        /// <summary>
        /// _pBoxの要素それぞれに対応するGraphics
        /// </summary>
        private Graphics[] _pBoxsGraphics;

        /// <summary>
        /// _pBoxsの横と縦サイズ. 高速化と簡易記述のために変数として置く.
        /// </summary>
        private int _pBoxsWidth = -1;
        private int _pBoxsHeight = -1;

        /// <summary>
        /// パズル全体の左上と右下を表す
        /// </summary>
        private int _topLeftX = -1;
        private int _topLeftY = -1;
        private int _bottomRightX = -1;
        private int _bottomRightY = -1;

        /// <summary>
        /// PictureBoxに表示するためのビットマップ. 二度目以降の表示のために初期化以降保持する.
        /// Problem.imgMapsの第一インデクサ同様にアクセスする.
        /// </summary>
        private Bitmap[] _bm;

        /// <summary>
        /// 並び替え後の順番を格納する.
        /// 答えのi列j行目に対応する元の画像の位置が (i * column + j) で得られる.
        /// </summary>
        private int[] _answerMap;

        /// <summary>
        /// 人力操作用: その位置のパズルが固定されているならtrue
        /// </summary>
        private bool[] _answerMapFixed;

        /// <summary>
        /// 人力操作用: その位置のパズルが選択されているならtrue
        /// </summary>
        private bool[] _answerMapSelected;

        /// <summary>
        /// 人力操作用: その位置のパズルの状態が変わったならtrue
        /// </summary>
        /// <remarks>
        /// 高速化のため, 状態が変わったときのみ表示を変更したい場合に使える.
        /// </remarks>
        private bool[] _answerMapStatusChanged;

        /// <summary>
        /// PictureBoxのボーダーが有効ならtrue
        /// </summary>
        private bool _borderEnabled;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Core()
        {
            InitializeComponent();
#if DEBUG 
            // DEBUG実行時のみにTopMostを外す
            this.TopMost = false;
#endif
        }
       
        /// <summary>
        /// Startから呼び出されるコンストラクタ
        /// </summary>
        /// <param name="problemUrl"></param>
        /// <param name="problemID"></param>
        /// <param name="submitUrl"></param>
        /// <param name="token"></param>
        /// <param name="sorterNum"></param>
        public Core(String serverAddress, String problemID, String token, int sorterNum)
            : this()
        {
            this._problemUrl = "http://" + serverAddress + "/problem/";
            this._submitUrl = "http://" + serverAddress + "/SubmitAnswer";
            this._problemID = ((problemID.Length == 1) ? "0" : "") + problemID;
            this._token = token;
            this._sorterNum = sorterNum;
        }

        /// <summary>
        /// Shownイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Core_Shown(object sender, EventArgs e)
        {
            run();
        }

        /// <summary>
        /// 処理の中心
        /// </summary>
        private void run() 
        {
            // 保存用ディレクトリ作成
            _workingDirectoryPath = createWorkingDirectory();
            
            // 問題ファイルのダウンロード
            String problemPath = downloadProblem();

            // ダウンロードに失敗した場合
            if (problemPath == null)
            {
                this.Close();
                return; // CloseはShownの実行が完了してから行われるため, returnが必要.
            }

            // staticクラスの初期化
            Problem.init(problemPath, _workingDirectoryPath);
            Poster.init(_submitUrl, _problemID, _token, this);
            Sorter.init(_sorterNum);

            // 答えマップの初期化
            _answerMap = new int[Problem.partNum];
            for (int i = 0; i < Problem.partNum; i++)
            {
                _answerMap[i] = i;
            }

            // パズルの並び替え (画像処理)
            sortPuzzles();
                
            // パズルの配置
            showPuzzles();
        }

        /// <summary>
        /// txtSortStatus.Textを更新
        /// </summary>
        /// <param name="status"></param>
        private void updateSortStatus(String status)
        {
            txtSortStatus.Text = status;
            txtSortStatus.Update();
        }

        /// <summary>
        /// txtSolverStatus.Textを更新
        /// </summary>
        /// <param name="status"></param>
        public void updatePostStatus(String status)
        {
            txtPostStatus.Text = status;
            txtPostStatus.Update();
        }

        /// <summary>
        /// 保存用ディレクトリの作成
        /// </summary>
        /// <returns>ディレクトリパス</returns>
        private String createWorkingDirectory()
        {
            updateSortStatus("ディレクトリの作成中");


            String parent = Path.GetFullPath(@".\temp\");
            String ret = parent + DateTime.Now.ToString("MM_dd_HH_mm_ss");

            if (!Directory.Exists(parent))
            {
                Directory.CreateDirectory(parent);
            }
            Directory.CreateDirectory(ret);


            updateSortStatus("ディレクトリの作成完了");
            return ret;
        }

        /// <summary>
        /// 問題ファイルのダウンロード
        /// </summary>
        /// <returns>
        /// 問題ファイルへのパスを返す. 失敗した場合はnullを返す.
        /// </returns>
        private String downloadProblem()
        {
            updateSortStatus("問題ファイルのダウンロード中");

            WebClient wc = new WebClient();
            String problemFile = "prob" + _problemID + ".ppm";
            String url = _problemUrl + problemFile;
            String ret = _workingDirectoryPath + @"\" + problemFile; // ダウンロード先
            // ダウンロードが成功するまで接続を続ける
            bool failure = true;
            while (failure)
            {
                try
                {
                    wc.DownloadFile(url, ret);
                    failure = false;
                }
                catch (WebException wex)
                {
                    if (MessageBox.Show(
                        "[DOWNLOADER ERROR: " + wex.Message + "]\n" + "続行しますか?"
                        , ""
                        , MessageBoxButtons.YesNo)
                        == DialogResult.No)
                    {
                        // 終了する
                        return null;
                    }
                }
            }

            //
            // For Debugging
            // http://procon2014-practice.oknct-ict.org/ を流用
            // String ret = Path.GetFullPath(@".\..\..\img\prob12.ppm"); // prob01 ~ prob16
            //

            updateSortStatus("問題ファイルのダウンロード完了");
            return ret;
        }

        /// <summary>
        /// パズルブロックの配置
        /// </summary>
        private void showPuzzles()
        {
            updateSortStatus("パズル配置中");

            var margin = 30;
            var remain = 0;
            var usableHeight = (this.Height - (margin * 2) - 55) / Problem.row; // height of what a pazzle can use
            var usableWidth = (this.Width - (margin * 2)) / Problem.column;
            double partRatio = ((double)Problem.partHeight) / Problem.partWidth;

            if (partRatio <= ((double)usableHeight) / usableWidth) {
                // fit in width
                _pBoxsWidth = usableWidth;
                _pBoxsHeight = (int)(usableWidth * partRatio);
            } else {
                // fit in height
                _pBoxsHeight = usableHeight;
                _pBoxsWidth = (int)(usableHeight / partRatio);
                remain = (usableWidth - _pBoxsWidth) * Problem.column / 2;
            }

            _topLeftX = margin + remain;
            _topLeftY = margin;
            _bottomRightX = _topLeftX + _pBoxsWidth * Problem.column;
            _bottomRightY = _topLeftY + _pBoxsHeight * Problem.row;

            _pBoxs = new PictureBox[Problem.partNum];
            _bm = new Bitmap[Problem.partNum];
            _answerMapFixed = new bool[Problem.partNum];
            _answerMapSelected = new bool[Problem.partNum];
            _answerMapStatusChanged = new bool[Problem.partNum];
            _pBoxsGraphics = new Graphics[Problem.partNum];
            _borderEnabled = true;
            for (int i = 0; i < Problem.row; i++) {
                for (int j = 0; j < Problem.column; j++) {
                    var addr = i * Problem.column + j;
                    _pBoxs[addr] = new PictureBox();
                    _answerMapFixed[addr] = _answerMapSelected[addr] = _answerMapStatusChanged[addr] = false;

                    // _pBoxsのプロパティ設定 & 表示
                    _pBoxs[addr].Location = new System.Drawing.Point(margin + j * _pBoxsWidth + remain, margin + i * _pBoxsHeight);
                    _pBoxs[addr].Size = new System.Drawing.Size(_pBoxsWidth, _pBoxsHeight);
                    _pBoxs[addr].Enabled = false;
                    Controls.Add(_pBoxs[addr]);

                    // Graphicsの設定
                    _pBoxsGraphics[addr] = _pBoxs[addr].CreateGraphics();
                    _pBoxsGraphics[addr].SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                    _pBoxsGraphics[addr].CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                    _pBoxsGraphics[addr].PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;

                    // 画像表示
                    _bm[_answerMap[addr]] = new Bitmap(Problem.imgPaths[_answerMap[addr]]);
                    _pBoxs[addr].Refresh();
                    drawImage(addr);
                }
            }

            updateSortStatus("パズル配置完了");
        }

        /// <summary>
        /// PictureBoxのボーダーを切り替える
        /// </summary>
        private void toggleBorder()
        {
            updateSortStatus("ボーダー切り替え中");

            _borderEnabled = !_borderEnabled;
            reshowPuzzlesAll();

            updateSortStatus("ボーダー切り替え完了");
        }

        Pen fixingPen = new Pen(Color.Red, 2);
        SolidBrush selectionBrush = new SolidBrush(Color.FromArgb(100, Color.Yellow));

        /// <summary>
        /// 固定されている場合のマーク(赤枠)を描画
        /// </summary>
        /// <param name="addr"></param>
        private void drawFixingMark(int addr)
        {
            int pad = 3;
            int right = _pBoxsWidth - pad;
            int bottom = _pBoxsHeight - pad;

            _pBoxsGraphics[addr].DrawLine(fixingPen, pad, pad, right, pad);
            _pBoxsGraphics[addr].DrawLine(fixingPen, pad, pad, pad, bottom);
            _pBoxsGraphics[addr].DrawLine(fixingPen, right, pad, right, bottom);
            _pBoxsGraphics[addr].DrawLine(fixingPen, pad, bottom, right, bottom);
        }

        /// <summary>
        /// 選択時に表示するマーク(半透明黄色の四角形)を描画
        /// </summary>
        /// <param name="addr"></param>
        private void drawSelectionMark(int addr)
        {
            int pad = 2;
            int width = _pBoxsWidth - pad * 2;
            int height = _pBoxsHeight - pad * 2;
            _pBoxsGraphics[addr].FillRectangle(selectionBrush, pad, pad, width, height);
        }

        /// <summary>
        /// addrのpieceに画像を描画
        /// </summary>
        /// <param name="addr"></param>
        private void drawImage(int addr)
        {
            _pBoxsGraphics[addr].DrawImage(_bm[_answerMap[addr]], 0, 0, _pBoxsWidth, _pBoxsHeight);
            if (_borderEnabled)
            {
                ControlPaint.DrawBorder3D(_pBoxsGraphics[addr], 0, 0, _pBoxsWidth, _pBoxsHeight);
            }
        }

        /// <summary>
        /// パズルブロックの再配置
        /// </summary>
        private void reshowPuzzles()
        {
            updateSortStatus("パズル再配置中");

            for (int i = 0; i < Problem.row; i++)
            {
                for (int j = 0; j < Problem.column; j++)
                {
                    var addr = i * Problem.column + j;

                    /// 固定されており, 選択もされていなかった場合には再描画の必要がない
                    /// => 固定されていない または 選択されていた場合に再描画する

                    // 固定されていない場合
                    if (_answerMapFixed[addr] == false)
                    {
                        drawImage(addr);
                    }
                    // 固定されていて, 選択されている場合
                    else if (_answerMapSelected[addr])
                    {
                        drawImage(addr);
                        drawFixingMark(addr);
                    }
                    _answerMapSelected[addr] = false; // 再配置前の選択を全て解除
                }
            }

            updateSortStatus("パズル再配置完了");
        }

        /// <summary>
        /// 全てのパズルブロックの再配置
        /// </summary>
        /// <returns></returns>
        private void reshowPuzzlesAll()
        {
            updateSortStatus("全パズル再配置中");

            for (int i = 0; i < Problem.row; i++)
            {
                for (int j = 0; j < Problem.column; j++)
                {
                    var addr = i * Problem.column + j;

                    drawImage(addr); 
                    if (_answerMapFixed[addr]) /// 固定されているならば赤枠を引く
                    {
                        drawFixingMark(addr);
                    }
                    if (_answerMapSelected[addr]) /// 選択されているならば半透明黄色マークを描画
                    {
                        drawSelectionMark(addr);
                    }
                }
            }

            updateSortStatus("全パズル再配置完了");
        }

        /// <summary>
        /// 各ピースの状態を表示
        /// </summary>
        private void showStatus()
        {
            for (int i = 0; i < Problem.row; i++)
            {
                for (int j = 0; j < Problem.column; j++)
                {
                    var addr = i * Problem.column + j;
                    if (_answerMapStatusChanged[addr]) 
                    {
                        drawImage(addr);
                        if (_answerMapFixed[addr]) /// 固定されているならば赤色枠を引く
                        {
                            drawFixingMark(addr);
                        }
                        if (_answerMapSelected[addr]) /// 選択されているならば半透明黄色マークを描画
                        {
                            drawSelectionMark(addr);
                        }
                        _answerMapStatusChanged[addr] = false;
                    }
                }
            }
        }

        /// <summary>
        /// パズルの並び替え (画像処理)
        /// </summary>
        private void sortPuzzles()
        {
            updateSortStatus("パズル並び替え中");
            Sorter.run(_answerMap);
            updateSortStatus("パズル並び替え完了");
        }

        /// <summary>
        /// パズルの再並び替え (並び替える前に_answerMapをランダマイズする)
        /// </summary>
        private void resortPuzzles()
        {
            updateSortStatus("パズル再並び替え中");
            Sorter.run(_answerMap, _answerMapFixed);
            updateSortStatus("パズル再並び替え完了");
        }

        /// <summary>
        /// Resort & Reshow
        /// </summary>
        private void resortReshow()
        {
            resortPuzzles();
            reshowPuzzles();
        }

        /// <summary>
        /// もう一度パズルを並び替えて表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnResort_Click(object sender, EventArgs e)
        {
            resortReshow();
            // フォーカスを外す
            btnResort.Enabled = false;
            btnResort.Enabled = true;
        }



        ///
        /// 以下, パズル人力操作用コード
        /// 

        /// <summary>
        /// _answerMapのi番地を固定
        /// </summary>
        /// <param name="i"></param>
        private void fix(int addr)
        {
            _answerMapFixed[addr] = true;
            _answerMapStatusChanged[addr] = true;
        }

        /// <summary>
        /// _answerMapのi番地を選択
        /// </summary>
        /// <param name="addr"></param>
        private void select(int addr)
        {
            _answerMapSelected[addr] = true;
            _answerMapStatusChanged[addr] = true;
        }

        /// <summary>
        /// _answerMapのi番地をunfix
        /// </summary>
        /// <param name="addr"></param>
        private void unfix(int addr)
        {
            _answerMapFixed[addr] = false;
            _answerMapStatusChanged[addr] = true;
        }

        /// <summary>
        /// _answerMapのi番地を選択から外す
        /// </summary>
        /// <param name="addr"></param>
        private void deselect(int addr)
        {
            _answerMapSelected[addr] = false;
            _answerMapStatusChanged[addr] = true;
        }

        /// MouseDown時のマウスポジション (UIではX,Yの順. 座標データではY,Xの順)
        int _downX = 0;
        int _downY = 0;

        private void Core_MouseDown(object sender, MouseEventArgs e)
        {
            _downX = e.X;
            _downY = e.Y;
        }

        /// <summary>
        /// Shiftキーを押下中ならtrue
        /// </summary>
        /// <returns></returns>
        private bool pushingShift()
        {
            return ((Control.ModifierKeys & Keys.Shift) == Keys.Shift);
        }

        /// <summary>
        /// Ctrlキーを押下中ならtrue
        /// </summary>
        /// <returns></returns>
        private bool pushingCtrl()
        {
            return ((Control.ModifierKeys & Keys.Control) == Keys.Control);
        }

        /// <summary>
        /// deselectAll
        /// </summary>
        /// <returns></returns>
        private void deselectAll()
        {
            for (int i = 0; i < Problem.partNum; i++)
            {
                if (_answerMapSelected[i]) // 画像を無駄に再表示させないためにifで囲む
                {
                    deselect(i);
                }
            }
        }

        /// <summary>
        /// UIのY軸座標を受け取り, マップの縦座標の対応を返す
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        private int correspondToMapRow(int y)
        {
            return
                (_topLeftY > y) ? -1
                : (_bottomRightY < y) ? Problem.row
                : (int)Math.Floor((double)(y - _topLeftY) / _pBoxsHeight);
        }

        /// <summary>
        /// UIのX軸座標を受け取り, マップの横座標の対応を返す
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private int correspondToMapColumn(int x)
        {
            return
                (_topLeftX > x) ? -1
                : (_bottomRightX < x) ? Problem.column
                : (int)Math.Floor((double)(x - _topLeftX) / _pBoxsWidth);
        }

        /// <summary>
        /// startX,YからendX,Yまでの四角形領域の中のピースを全て選択する.
        /// ただし, 選択されてるピースを一つのみクリックした場合には反転(選択解除)する.
        /// (deselectionがtrueならばその範囲の選択を外すようにする)
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="endX"></param>
        /// <param name="endY"></param>
        private void selectRange(int startX, int startY, int endX, int endY, bool deselection = false)
        {
            if (startX > endX)
            {
                Util.swap(ref startX, ref endX);
            }
            if (startY > endY)
            {
                Util.swap(ref startY, ref endY);
            }

            // i, j: マップのインデックス
            int i0 = correspondToMapRow(startY);
            int i1 = correspondToMapRow(endY);
            int j0 = correspondToMapColumn(startX);
            int j1 = correspondToMapColumn(endX);

            // 範囲外
            if ((i0 == i1 && (i0 == -1 || i0 == Problem.row))
                || (j0 == j1 && (j0 == -1 || j0 == Problem.column)))
            {
                return;
            }

            // 一つのみ 選択 or 選択解除
            if (i0 == i1 && j0 == j1)
            {
                int addr = i0 * Problem.column + j0;
                if (_answerMapSelected[addr])
                {
                    deselect(addr);
                }
                else
                {
                    select(addr);
                }
                return;
            }

            // 複数選択
            if (i0 == -1) i0++;
            if (j0 == -1) j0++;
            if (i1 != Problem.row) i1++;
            if (j1 != Problem.column) j1++;
            if (deselection) // 指定した範囲の選択を外す
            {
                for (int i = i0; i < i1; i++)
                {
                    for (int j = j0; j < j1; j++)
                    {
                        int addr = i * Problem.column + j;
                        if (_answerMapSelected[addr]) // 画像を無駄に再表示させないためにifで囲む
                        {
                            deselect(addr);
                        }
                    }
                }
            }
            else // 指定した範囲を選択する
            {
                for (int i = i0; i < i1; i++)
                {
                    for (int j = j0; j < j1; j++)
                    {
                        int addr = i * Problem.column + j;
                        if (!_answerMapSelected[addr])
                        {
                            select(addr);
                        }
                    }
                }
            }
        }

        private void fixSelected()
        {
            for (int i = 0; i < Problem.partNum; i++)
            {
                if (_answerMapSelected[i])
                {
                    fix(i);
                    deselect(i);
                }
            }
        }

        private void unfixSelected()
        { 
            for (int i = 0; i < Problem.partNum; i++)
            {
                if (_answerMapSelected[i])
                {
                    unfix(i);
                    deselect(i);
                }
            }
        }

        /// <summary>
        /// addr0とaddr1が両方とも固定されていないなら状態共々入れ替える.
        /// </summary>
        /// <param name="addr0"></param>
        /// <param name="addr1"></param>
        private void swapPiece(int addr0, int addr1)
        { 
            if (_answerMapFixed[addr0] == false
                && _answerMapFixed[addr1] == false)
            {
                Util.swap(ref _answerMap[addr0], ref _answerMap[addr1]);
                Util.swap(ref _answerMapSelected[addr0], ref _answerMapSelected[addr1]);
                _answerMapStatusChanged[addr0] = _answerMapStatusChanged[addr1] = true;
            }
        }

        /// <summary>
        /// startX,Yのパズルが選択中のものであれば, endX,Yまで移動する.
        /// ただし, 固定されているピースは移動しない.
        /// 
        /// (今は移動元と移動先を入れ替える処理のみなので, ずらす処理にしても良い)
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="endX"></param>
        /// <param name="endY"></param>
        private void movePuzzles(int startX, int startY, int endX, int endY)
        {
            // i, j: マップのインデックス
            int i0 = correspondToMapRow(startY);
            int i1 = correspondToMapRow(endY);
            int j0 = correspondToMapColumn(startX);
            int j1 = correspondToMapColumn(endX);

            // 選択されたものでなければ終了
            if (i0 < 0
                || i0 >= Problem.row
                || j0 < 0
                || j0 >= Problem.column
                || _answerMapSelected[i0 * Problem.column + j0] == false)
            {
                return;
            }

            if (i1 == -1) i1++;
            else if (i1 == Problem.row) i1--;
            if (j1 == -1) j1++;
            else if (j1 == Problem.column) j1--;

            // 縦
            if (i0 > i1) // 上への移動
            {
                int dist = i0 - i1;

                i0 = -1; // use the top of selected pieces
                for (int i = 0; i < Problem.row; i++)
                {
                    for (int j = 0; j < Problem.column; j++)
                    {
                        int addr = i * Problem.column + j;
                        if (_answerMapSelected[addr])
                        {
                            i0 = i;
                            break;
                        }
                    }
                    if (i0 != -1)
                    {
                        break;
                    }
                }

                if (i0 < dist) // i0 - 0 < dist
                {
                    dist = i0;
                }

                for (int j = 0; j < Problem.column; j++)
                {
                    for (int i = i0; i < Problem.row; i++)
                    {
                        int addr0 = i * Problem.column + j;
                        int addr1 = (i - dist) * Problem.column + j;
                        if (_answerMapSelected[addr0]) 
                        {
                            swapPiece(addr0, addr1);
                        }
                    }
                }
            }
            else if (i0 < i1) // 下への移動
            {
                int dist = i1- i0;

                i0 = -1; // use the bottom of selected pieces
                for (int i = Problem.row - 1; i >= 0; i--)
                {
                    for (int j = 0; j < Problem.column; j++)
                    {
                        int addr = i * Problem.column + j;
                        if (_answerMapSelected[addr])
                        {
                            i0 = i;
                            break;
                        }
                    }
                    if (i0 != -1)
                    {
                        break;
                    }
                }
                
                int tmp;
                if ((tmp = Problem.row - 1 - i0) < dist)
                {
                    dist = tmp;
                }

                for (int j = 0; j < Problem.column; j++)
                {
                    for (int i = i0; i >= 0; i--)
                    {
                        int addr0 = i * Problem.column + j;
                        int addr1 = (i + dist) * Problem.column + j;
                        if (_answerMapSelected[addr0])
                        {
                            swapPiece(addr0, addr1);
                        }
                    }
                }
            }

            // 横
            if (j0 > j1) // 左に移動
            {
                int dist = j0 - j1;

                j0 = -1; // use the right-most selected pieces
                for (int j = 0; j < Problem.column; j++)
                {
                    for (int i = 0; i < Problem.row; i++)
                    {
                        int addr = i * Problem.column + j;
                        if (_answerMapSelected[addr])
                        {
                            j0 = j;
                            break;
                        }
                    }
                    if (j0 != -1)
                    {
                        break;
                    }
                }

                
                if (j0 < dist) // j0 - 0 < dist
                {
                    dist = j0;
                }

                for (int i = 0; i < Problem.row; i++)
                {
                    for (int j = j0; j < Problem.column; j++)
                    {
                        int addr0 = i * Problem.column + j;
                        int addr1 = i * Problem.column + (j - dist);
                        if (_answerMapSelected[addr0]) 
                        {
                            swapPiece(addr0, addr1);
                        }
                    }
                }
            }
            else if (j0 < j1) // 右に移動
            {
                int dist = j1- j0;

                j0 = -1; // use the left-most selected pieces
                for (int j = Problem.column - 1; j >= 0; j--)
                {
                    for (int i = 0; i < Problem.row; i++)
                    {
                        int addr = i * Problem.column + j;
                        if (_answerMapSelected[addr])
                        {
                            j0 = j;
                            break;
                        }
                    }
                    if (j0 != -1)
                    {
                        break;
                    }
                }
                
                int tmp;
                if ((tmp = Problem.column - 1 - j0) < dist)
                {
                    dist = tmp;
                }

                for (int j = j0; j >= 0; j--)
                {
                    for (int i = 0; i < Problem.row; i++)
                    {
                        int addr0 = i * Problem.column + j;
                        int addr1 = i * Problem.column + (j + dist);
                        if (_answerMapSelected[addr0])
                        {
                            swapPiece(addr0, addr1);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 選択しているピースをランダマイズする
        /// </summary>
        private void randomizeSelected()
        {
            List<int> selectedIndex = new List<int>();
            List<int> selectedNum = new List<int>();

            for (int i = 0; i < Problem.partNum; i++)
            {
                if (_answerMapSelected[i] && !_answerMapFixed[i])
                {
                    selectedIndex.Add(i);
                    selectedNum.Add(_answerMap[i]);
                }
            }

            Util.randomizeList(selectedNum);

            for (int i = 0; i < selectedIndex.Count; i++)
            {
                _answerMap[selectedIndex[i]] = selectedNum[i];
                _answerMapStatusChanged[selectedIndex[i]] = true;
            }
        }

        /// <summary>
        /// 全選択を行う
        /// </summary>
        private void selectAll()
        {
            for (int i = 0; i < Problem.partNum; i++)
            {
                if (!_answerMapSelected[i])
                {
                    select(i);
                }
            }
        }

        ///
        /// Sorter-UIドキュメント 
        ///
        ///   [ピースをクリック]
        ///     そのピースを選択 (選択されたピースは黄色っぽくなる)
        ///     すでに選択されている場合は, 選択が解除される
        ///     
        ///   [一つのピースの上でクリックして, クリックしたまま別のピースの上でクリックをやめる] 
        ///     その区間のピース, 全てを選択 (Ctrlを押下中であれば, 選択が外される)
        ///     
        ///   [cキーを押下]
        ///     選択を解除
        ///     
        ///   [xキーを押下]
        ///     選択中のピースを固定する (固定されたピースの周りには赤色ボーダーが描かれる)
        ///     
        ///   [qキーを押下]
        ///     選択中のピースを固定状態から外す    
        /// 
        ///   [Resortボタン | rキーを押下]
        ///     再度画像処理による並び替えを行う. 固定されたピースはその位置のまま
        ///     
        ///   [Shiftキーを押しながら, 選択したピースの上でクリックをして移動したいところでクリックをやめる]
        ///     クリックをやめたところに, 選択しているピースが移動する
        ///     
        ///   [Postボタン | pキーを押下]
        ///     現在のマップをソルバーにかけ, サーバーに投稿する
        ///     (すでに投稿中ならば, それを破棄して現在のものを投稿する)
        ///     (!!注意!!: 1問につき回答回数は10回迄に制限されている)
        ///     
        ///   [Ctrlキーを押しながら, rキーを押下]
        ///     選択されたピースをランダマイズする. (固定されたピースはそのまま)
        ///
        ///   [Ctrlキーを押しながら, aキーを押下]
        ///     全選択
        ///     
        ///   [Ctrlキーを押しながら, tキーを押下]
        ///     ボーダーのオンオフを切り替える
        ///


        /// コツ
        /// 
        ///   基本的にrキーとxキーを使っていく. rキーで変化しなくなってもxキーで固定する数を増やば
        ///  どんどん別の配置になる. 初期段階で一つも固定できるピースがないときにShiftキー+マウスで移動を行う.
        ///

        private void Core_MouseUp(object sender, MouseEventArgs e)
        {
            if (pushingShift())
            {
                movePuzzles(_downX, _downY, e.X, e.Y);
            }
            else
            {
                selectRange(_downX, _downY, e.X, e.Y, pushingCtrl());
            }
            showStatus();
        }

        private void Core_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.C:
                    deselectAll();
                    break;

                case Keys.X:
                    fixSelected();
                    break;

                case Keys.Q:
                    unfixSelected();
                    break;

                case Keys.R:
                    if (pushingCtrl())
                    {
                        randomizeSelected();
                        break;
                    }
                    resortReshow();
                    return;

                case Keys.P:
                    post();
                    return;

                case Keys.A:
                    if (!pushingCtrl())
                    {
                        return;
                    }
                    selectAll();
                    break;

                case Keys.T:
                    if (!pushingCtrl())
                    {
                        return;
                    }
                    toggleBorder();
                    return;

                default:
                    return; // このメソッドから抜ける
            }
            showStatus();
        }

        /// <summary>
        /// 現在のマップをソルバーにかけ, 問題サーバーに投稿する
        /// </summary>
        private static Thread _postingThread = null;
        private void post()
        {
            // 他の投稿が行われているならば, それを破棄する
            if (Util.isRunning(_postingThread))
            {
                _postingThread.Abort();
                Poster.close();
            }
            _postingThread = Util.makeThread(() => Poster.post(_answerMap));
            _postingThread.Start();
        }
            
        private void btnPost_Click(object sender, EventArgs e)
        {
            post();
            // フォーカスを外す
            btnPost.Enabled = false;
            btnPost.Enabled = true;
        }

        /// <summary>
        /// WndProcをオーバーライドして, 
        /// * タイトルバーへのダブルクリックによる最小化
        /// * 右クリックによるシステムメニューの表示
        /// * フォームの移動
        /// を禁止する.
        /// ref:
        ///   http://blog.livedoor.jp/cyberbridge/archives/761937.html
        ///   http://dobon.net/vb/dotnet/form/fixformposition.html
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            const int WM_NCLBUTTONDBLCLK = 0x00A3;
            const int WM_NCRBUTTONDOWN = 0x00A4;
            const int WM_SYSCOMMAND = 0x0112;
            const long SC_MOVE = 0xF010L;

            switch (m.Msg)
            {
                case WM_NCLBUTTONDBLCLK: // 最小化を禁止
                case WM_NCRBUTTONDOWN:   // システムメニューの表示を禁止
                    return;
                case WM_SYSCOMMAND: 
                    // フォームの移動を禁止
                    if ((m.WParam.ToInt64() & 0xFFF0L) == SC_MOVE)
                    {
                        m.Result = IntPtr.Zero;
                        return;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        private void Core_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Util.isRunning(_postingThread))
            {
                _postingThread.Abort();
                Poster.close();
            }
        }
    }
}
