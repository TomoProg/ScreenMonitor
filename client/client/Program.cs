using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace client
{
    class Program
    {
        /// <summary>
        /// 接続先IP(server.pyが起動しているホストのIPを指定する)
        /// </summary>
        static readonly string HOST_IP = "ここにIPアドレスを入れてね";

        /// <summary>
        /// 接続先ポート(server.pyが起動しているホストのポートを指定する)
        /// </summary>
        static readonly int PORT = 20000;

        /// <summary>
        /// ログファイルパス
        /// </summary>
        static readonly string LOG_FILE_PATH = "log.txt";

        static void Main(string[] args)
        {
            TcpClient tc = null;
            NetworkStream ns = null;

            while (true)
            {

                while (true)
                {
                    try
                    {
                        //------------------------------------------------------
                        // サーバへ接続
                        //------------------------------------------------------
                        tc = new TcpClient(HOST_IP, PORT);
                        ns = tc.GetStream();
                        break;
                    }
                    catch (Exception ex)
                    {
                        WriteLog(string.Format("サーバとの接続に失敗 接続先IP[{0}] ポート[{1}]", HOST_IP, PORT));
                        WriteExceptionLog(ex);
                        if (tc != null)
                        {
                            tc.Close();
                        }
                        if (ns != null)
                        {
                            ns.Close();
                        }
                        Thread.Sleep(3000);
                    }
                }

                while(true)
                {
                    try
                    {
                        //------------------------------------------------------
                        // スクリーン画像取得
                        //------------------------------------------------------
                        MemoryStream ms = GetImageStream();
                        if (ms == null)
                        {
                            Thread.Sleep(3000);
                            continue;
                        }

                        //------------------------------------------------------
                        // バイト配列へ変換
                        //------------------------------------------------------
                        byte[] bytesImg = ms.GetBuffer();
                        ms.Dispose();

                        //------------------------------------------------------
                        // 画面送信
                        //------------------------------------------------------
                        ns.Write(bytesImg, 0, bytesImg.Length);

                        //------------------------------------------------------
                        // 終端文字設送信
                        //------------------------------------------------------
                        // [Memo]
                        //   終端文字を固定で送信することで、
                        //   画像ファイルの終端をサーバ側で検知する
                        //------------------------------------------------------
                        byte[] bytesFin = { 0xff, 0xff, 0xff };
                        ns.Write(bytesFin, 0, bytesFin.Length);
                        WriteLog("送信完了");

                        Thread.Sleep(300);

                    }
                    catch (Exception ex)
                    {
                        WriteLog(string.Format("画面画像の送信に失敗"));
                        WriteExceptionLog(ex);
                        Thread.Sleep(3000);
                        break;
                    }
                }
                ns.Dispose();
                tc.Close();
            }

        }

        /// <summary>
        /// 画面画像用のメモリストリームを取得
        /// </summary>
        /// <returns>画像のメモリストリーム(nullの場合は取得失敗)</returns>
        static MemoryStream GetImageStream()
        {
            MemoryStream ms = null;

            try
            {
                using (Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(new Point(0, 0), new Point(0, 0), bmp.Size);
                        ms = new MemoryStream();
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }
            }
            catch(Exception ex)
            {
                WriteLog(string.Format("画面画像用のメモリストリームの取得に失敗"));
                WriteExceptionLog(ex);
                ms = null;
            }

            return ms;
        }
        
        /// <summary>
        /// ログ書き込み
        /// </summary>
        /// <param name="log">ログファイルに書き込む文字列を指定する</param>
        static void WriteLog(string log)
        {
            using (StreamWriter swLog = File.AppendText(LOG_FILE_PATH))
            {
                swLog.WriteLine(DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff") + " " + log);
            }
        }

        /// <summary>
        /// 例外ログ書き込み
        /// </summary>
        /// <param name="ex">例外クラス</param>
        static void WriteExceptionLog(Exception ex)
        {
            using (StreamWriter swLog = File.AppendText(LOG_FILE_PATH))
            {
                if (ex == null)
                {
                    return;
                }
                swLog.WriteLine("message:" + ex.Message);
                swLog.WriteLine("stack trace:");
                swLog.WriteLine(ex.StackTrace);
            }
        }
    }
}
