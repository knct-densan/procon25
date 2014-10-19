using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;

namespace chino
{
    /// <summary>
    /// サーバーへ投稿を行うためのメソッド群
    /// </summary>
    static class Poster
    {
        //
        //  == Poster API == 
        //
        //  1. 初期化       [ init(submitUrl, problemID, token, Core) ]
        //  2. 投稿を行う    [ post(answerMap) ]
        //  3. 終了         [ close() ]
        //

        private static String _submitUrl, _problemID, _token;
        private static Core _window;
        private static Thread _lastSolverThread;

        public static void init(String submitUrl, String problemID, String token, Core win)
        {
            _submitUrl = submitUrl;
            _problemID = problemID;
            _token = token;
            _window = win;
        }

        private static void updateStatus(String status)
        {
            _window.Invoke((Action)(() => _window.updatePostStatus(status)));
        }

        private static String sendAnswer(String answer)
        {
            String param
                = "playerid=" + Uri.EscapeDataString(_token)
                + "&problemid=" + Uri.EscapeDataString(_problemID)
                + "&answer=" + Uri.EscapeDataString(answer);
            byte[] data = Encoding.ASCII.GetBytes(param);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_submitUrl);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;

            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
            }
            WebResponse response = req.GetResponse();
            String ret;
            using (Stream resStream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(resStream, Encoding.ASCII))
                {
                    ret = reader.ReadToEnd();
                }
            }
            return ret;
        }

        public static void post(int[] answerMap)
        {
            updateStatus("ソルバー実行開始");
            String answer = Solver.run(answerMap, 0);
            String lastAnswer = null;
            _lastSolverThread = Util.makeThread(() => { lastAnswer = Solver.run(answerMap, 2); });
            _lastSolverThread.Start();

            updateStatus("サーバーへの投稿開始");
            try
            {
                String response = sendAnswer(answer);
                updateStatus("投稿完了: " + response);
                
                // 完全復元されれば
                if (response == "ACCEPTED 0")
                {
                    updateStatus("ラストソルバー待機中");
                    _lastSolverThread.Join();
                    response = sendAnswer(lastAnswer);
                    updateStatus("ラストソルバー投稿完了: " + response);
                }
                else
                {
                    close();
                }
            } 
            catch (WebException wex)
            {
                updateStatus("投稿失敗: " + wex.Message);
            }
        }

        public static void close()
        {
            if (Util.isRunning(_lastSolverThread))
            {
                _lastSolverThread.Abort();
            }
        }
    }
}
