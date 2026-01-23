using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Configuration;

namespace GlobalFunctions_NS
{
    // naponta uj file-t nyit
    // log nev szerkezete : yyyy_mm_dd__name.TXT
    // a program teszi az idot a nev ele!
    public static class FileLog0
    {
        const string LOG0NameConst = @"d:\log\LOG0.TXT";
        const string DateTimeFormat = "yyyy.MM.dd HH:mm:ss.fff";
        const string TimeFormat = "HH:mm:ss.fff";
        static string LOG0Name = null;
        static DateTime LastLogNameDate;
        public static bool NeedDebugText { get; set; } = false;

        static StreamWriter Writer = null;
        static readonly ConcurrentQueue<string> Lines = new ConcurrentQueue<string>();
        static readonly System.Timers.Timer MyTimer = new System.Timers.Timer(1000)
        {
            Enabled = true
        };
        static Stopwatch LastWrite = new Stopwatch();

        static FileLog0()
        {
            OpenStream();
            //..Lines = new ConcurrentQueue<string>()
            //..MyTimer = new System.Timers.Timer(1000)
            MyTimer.Elapsed += MyTimer_Tick;
            //..MyTimer.Enabled = true
        }

        private static void OpenStream()
        {
            fillLOG0Name();
            Writer = new StreamWriter(LOG0Name, true)
            {
                AutoFlush = true
            };
        }

        private static void CloseStream()
        {
            if (Writer != null)
            {
                Writer.Flush();
                Writer.Close();
                Writer.Dispose();
                Writer = null;
            }
        }


        public static void LogTextFile(string moduleID, string txt, string info1, string info2)
        {
            GetSourceInfo(2, out string fileName, out string fileLineNumber);
            Lines.Enqueue(string.Format("{0} ::{1}({2})", createLogLine(moduleID, txt, info1, info2), fileName, fileLineNumber));
        }

        public static void LogTextFileNL(string moduleID, string txt, string info1, string info2)
        {
            GetSourceInfo(2, out string fileName, out string fileLineNumber);
            Lines.Enqueue(string.Format("{0} ::{1}({2})", Environment.NewLine + createLogLine(moduleID, txt, info1, info2), fileName, fileLineNumber));

        }

        private static string createLogLine(string moduleID, string txt, string info1, string info2)
        {
            return string.Format("{0} ({1:X8}) - {2} {3} {4} {5}", DateTime.Now.ToString(DateTimeFormat),
                                                    (Task.CurrentId.HasValue) ? Task.CurrentId : Thread.CurrentThread.ManagedThreadId,
                                                    moduleID,
                                                    txt,
                                                    info1,
                                                    info2
                                                    );
        }

        public static void LogTextFileEx(string moduleID, string txt, string info1, string info2)
        {
            GetSourceInfo(2, out string fileName, out string fileLineNumber);
            Lines.Enqueue(string.Format("{0} ::{1}({2})", createLogLine(string.Format(" *EXCEPTION* {0}", moduleID), txt, info1, info2), fileName, fileLineNumber));
        }

        private static void fillLOG0Name()
        {
            // nev letrehozasa/eloallitasa
            string tmpFileName;
            try
            {
                tmpFileName = WebConfigurationManager.AppSettings["LOG0Name"];
                if (string.IsNullOrEmpty(tmpFileName))
                {
                    tmpFileName = LOG0NameConst;
                }
            }
            catch (Exception ex)
            {
                tmpFileName = LOG0NameConst;
                HELP(ex.Message);
            }
            LastLogNameDate = DateTime.Now.Date;
            LOG0Name = Path.Combine(Path.GetDirectoryName(tmpFileName), $"{LastLogNameDate.Year,4:D4}_{LastLogNameDate.Month,2:D2}_{LastLogNameDate.Day,2:D2}__{Path.GetFileName(tmpFileName)}");
        }

        private static void HELP(string txt)
        {
            string ws = @"\log\ALMA.TXT";
            string fileName = (Environment.MachineName.ToUpperInvariant().Equals("CESZESP")) ? "c:"+ws : "d:"+ws;
            System.IO.File.AppendAllText(fileName, txt);
        }


        public static string MakeExceptionMessages(Exception ex)
        {
            Exception wex = ex;
            StringBuilder rc = new StringBuilder();
            int i = 0;
            while (wex != null)
            {
                i++;
                if (i != 1)
                {
                    rc.Append(Environment.NewLine);
                }
                rc.Append(string.Format("{0}   {1:00} {2} {3}{4}{5}", Environment.NewLine, i, wex.GetType().Name, wex.Message, Environment.NewLine, wex.StackTrace));
                wex = wex.InnerException;
            }
            return rc.ToString();
        }

        // csak a Message-ek konkatenalodnak
        public static string MakeExceptionMessages2(Exception ex)
        {
            Exception wex = ex;
            StringBuilder rc = new StringBuilder();
            int i = 0;
            while (wex != null)
            {
                i++;
                if  (i != 1)
                {
                    rc.Append(" ]][[ ");    
                }
                rc.Append(String.Format("{0:00} {1} {2}", i, wex.GetType().Name, wex.Message));
                wex = wex.InnerException;
            }
            return rc.ToString();
        }



        public static void DebugTxt(string info1, string info2, string info3, string info4)
        {
            string tmpDbg;
            try
            {
                tmpDbg = WebConfigurationManager.AppSettings["DebugTxt"];
                if ((!string.IsNullOrEmpty(tmpDbg)) && (tmpDbg.ToUpperInvariant().Equals("Y")))
                {
                    NeedDebugText = true;
                }
                else
                {
                    NeedDebugText = false;
                }
            }
            catch (Exception)
            {
                NeedDebugText = false;
            }

            if (NeedDebugText)
            {
                GetSourceInfo(2, out string fileName, out string fileLineNumber);
                Lines.Enqueue(string.Format("{0} ::{1}({2})", createDebugLine(info1, info2, info3, info4), fileName, fileLineNumber));
            }
        }

        private static string createDebugLine(string info1, string info2, string info3, string info4)
        {
            return string.Format("~~~~{0,2:D2}{1,2:D2}{2,2:D2} {3} ({4:X8}), {5}, {6}, {7} {8}", DateTime.Now.Year - 2000, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.ToString(TimeFormat),
                (Task.CurrentId.HasValue) ? Task.CurrentId : Thread.CurrentThread.ManagedThreadId,
                info1, info2, info3, info4);
        }


        public static void WriteStack()
        {
            string ws = "";
            int i = 1;
            StackTrace st = new StackTrace(true);
            StackFrame sf;
            try
            {
                while (i < st.FrameCount)
                {
                    sf = st.GetFrame(i);
                    string methodName = (sf.GetMethod() != null) ? sf.GetMethod().ToString() : "";
                    string fileName = sf.GetFileName() ?? "";
                    string fileLineNumber = (sf.GetFileLineNumber() > 0) ? sf.GetFileLineNumber().ToString() : "";

                    ws = String.Format("{0}{1}                {2:00} {3}   {4}({5})",
                                        ws,
                                        Environment.NewLine,
                                        i,
                                        methodName,
                                        Path.GetFileName(fileName),
                                        fileLineNumber);
                    i++;
                }
            }
            catch (Exception ex)
            {
                LogTextFileEx("FileLog0.WriteStack", MakeExceptionMessages(ex), null, null);
            }
            Lines.Enqueue(createLogLine("FileLog0.WriteStack", ws, null, null));
        }

        [SuppressMessage("Style", "IDE0060")]
        public static void GetSourceInfo(int level, out string fileName, out string fileLineNumber)
        {
            //..StackFrame stackFrame = new StackFrame(level, true)
            //..fileName = stackFrame.GetFileName() != null ? stackFrame.GetFileName() : ""
            //..fileLineNumber = (stackFrame.GetFileLineNumber() > 0) ? stackFrame.GetFileLineNumber().ToString() : ""
            fileName = "";
            fileLineNumber = "";
        }


        private static void MyTimer_Tick__while()
        {
            while (Lines.Count > 0)
            {
                if (Lines.TryDequeue(out string ws))
                {
                    if (Writer == null)
                    {
                        OpenStream();
                    }
                    Writer.WriteLine(ws);
                    LastWrite.Start();
                }
            }
        }


        private static void MyTimer_Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                MyTimer.Stop();
                //-------------------------
                // log nevben datum valtas
                if (!DateTime.Now.Date.Equals(LastLogNameDate))
                {
                    CloseStream();
                    OpenStream();
                }
                //-------------------------
                MyTimer_Tick__while();
                //-------------------------
                // ha 10 sec-ig nem jon irni valo, engedje el file-t, majd ujra megnyitja
                LastWrite.Stop();
                TimeSpan ts = LastWrite.Elapsed;
                LastWrite = Stopwatch.StartNew();
                int deltaSeconds = Convert.ToInt32(ts.TotalSeconds);
                if (deltaSeconds > 10)
                {
                    CloseStream();
                }
            }
            finally
            {
                MyTimer.Start();
            }
        }


    }




}
