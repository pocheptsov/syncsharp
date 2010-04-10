﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SyncSharp.Storage
{
	public static class Logger
	{
		#region attributes

        public enum LogType { CopySRC, CopyTGT, DeleteSRC, DeleteTGT, RenameSRC, RenameTGT, CreateSRC, CreateTGT };

        private static readonly string CopyStatusSRC = "[COPY OK] from SOURCE";
        private static readonly string DeleteStatusSRC = "[DELETE OK] from SOURCE";
        private static readonly string RenameStatusSRC = "[RENAME OK] from SOURCE";
        private static readonly string CreateStatusSRC = "[CREATE FOLDER OK] on SOURCE";

        private static readonly string CopyStatusTGT = "[COPY OK] from TARGET";
        private static readonly string DeleteStatusTGT = "[DELETE OK] from TARGET";
        private static readonly string RenameStatusTGT = "[RENAME OK] from TARGET";
        private static readonly string CreateStatusTGT = "[CREATE FOLDER OK] on TARGET";

		private static FileStream _currFile;
		private static StreamWriter _sw;

		private static string _logFileName;
		private static string _logFileLocation;

		private static int _filesRenamedCntSRC;
		private static int _filesCopiedCntSRC;
		private static int _filesDeletedCntSRC;

		private static long _filesRenamedSizeSRC;
		private static long _filesCopiedSizeSRC;
		private static long _filesDeletedSizeSRC;

        private static int _foldersCreatedCntSRC;

		private static int _filesRenamedCntTGT;
		private static int _filesCopiedCntTGT;
		private static int _filesDeletedCntTGT;

		private static long _filesRenamedSizeTGT;
		private static long _filesCopiedSizeTGT;
		private static long _filesDeletedSizeTGT;
        
        private static int _foldersCreatedCntTGT;

		#endregion

		#region properties

		#endregion

		#region methods

        /// <summary>
        /// Sync preview details log
        /// </summary>
        /// <param name="metaDataDir"></param>
        /// <param name="syncTaskName"></param>
        /// <param name="srcCopyTotal"></param>
        /// <param name="srcCopySize"></param>
        /// <param name="srcDeleteTotal"></param>
        /// <param name="srcDeleteSize"></param>
        /// <param name="srcRenameTotal"></param>
        /// <param name="srcRenameSize"></param>
        /// <param name="tgtCopyTotal"></param>
        /// <param name="tgtCopySize"></param>
        /// <param name="tgtDeleteTotal"></param>
        /// <param name="tgtDeleteSize"></param>
        /// <param name="tgtRenameTotal"></param>
        /// <param name="tgtRenameSize"></param>
        /// <returns>successful</returns>
        public static bool WritePreviewLog(string metaDataDir, string syncTaskName, int srcCopyTotal, long srcCopySize, int srcDeleteTotal, long srcDeleteSize, int srcRenameTotal, long srcRenameSize, int tgtCopyTotal, long tgtCopySize, int tgtDeleteTotal, long tgtDeleteSize, int tgtRenameTotal, long tgtRenameSize)
		{
            if (metaDataDir == null) throw new ArgumentNullException("metaDataDir");
			if (syncTaskName == null) throw new ArgumentNullException("syncTaskName");
            if (metaDataDir.Equals(string.Empty)) throw new ArgumentException("Empty string passed", "metaDataDir");
			if (syncTaskName.Equals(string.Empty)) throw new ArgumentException("Empty string passed", "syncTaskName");

			try
			{
                Directory.CreateDirectory(_logFileLocation);

				_currFile = new FileStream(_logFileName, FileMode.Append, FileAccess.Write, FileShare.Read);
				_sw = new StreamWriter(_currFile);

				WritePreviewLogEntry(srcCopyTotal, srcCopySize, tgtCopyTotal, tgtCopySize, srcDeleteTotal, srcDeleteSize, tgtDeleteTotal, tgtDeleteSize, srcRenameTotal, srcRenameSize, tgtRenameTotal, tgtRenameSize);
			}
			catch (IOException e)
			{
				WriteErrorLog(e.Message);
				return false;
			}
			finally
			{
				_sw.Close();
				_currFile.Close();
			}
			return true;
		}

	    private static void WritePreviewLogEntry(int srcCopyTotal, long srcCopySize, int tgtCopyTotal, long tgtCopySize, int srcDeleteTotal, long srcDeleteSize, int tgtDeleteTotal, long tgtDeleteSize, int srcRenameTotal, long srcRenameSize, int tgtRenameTotal, long tgtRenameSize)
	    {
	        _sw.WriteLine("*** Sync Plan ***");
	        _sw.WriteLine("ACTION          \tsource\t\ttarget");
	        _sw.WriteLine("Files to COPY  :\t{0}\t{1}\t{2}\t{3}\t", srcCopyTotal, srcCopySize, tgtCopyTotal, tgtCopySize);
	        _sw.WriteLine("Files to DELETE:\t{0}\t{1}\t{2}\t{3}\t", srcDeleteTotal, srcDeleteSize, tgtDeleteTotal, tgtDeleteSize);
	        _sw.WriteLine("Files to RENAME:\t{0}\t{1}\t{2}\t{3}\t", srcRenameTotal, srcRenameSize, tgtRenameTotal, tgtRenameSize);
	        _sw.WriteLine("*****************");
	        _sw.WriteLine("#Sync executing#");
	        _sw.WriteLine("#Format:|date|time|status|source source|size|source dest|size|target source|size|target dest|size|error msg|");
	        _sw.WriteLine("#");
	    }

        /// <summary>
        /// Set begin or end of log recording for a sync task
        /// </summary>
        /// <param name="metaDataDir"></param>
        /// <param name="syncTaskName"></param>
        /// <param name="start">true: begin, false: end</param>
        /// <returns>successful</returns>
	    public static bool WriteSyncLog(string metaDataDir, string syncTaskName, bool start)
		{
            if (metaDataDir == null) throw new ArgumentNullException("metaDataDir");
			if (syncTaskName == null) throw new ArgumentNullException("syncTaskName");
            if (metaDataDir.Equals(string.Empty)) throw new ArgumentException("Empty string passed", "metaDataDir");
			if (syncTaskName.Equals(string.Empty)) throw new ArgumentException("Empty string passed", "syncTaskName");

			_logFileName = metaDataDir + @"\" + syncTaskName + ".log";
			_logFileLocation = metaDataDir + @"\";

			try
			{
                Directory.CreateDirectory(_logFileLocation);

				_currFile = new FileStream(_logFileName, FileMode.Append, FileAccess.Write, FileShare.Read);
				_sw = new StreamWriter(_currFile);

				if (start)
					WriteLogHeader();
				else
					WriteSyncResultsLogEntry();	
			}
			catch (IOException e)
			{
				WriteErrorLog(e.Message);
				return false;
			}
			finally
			{
				_sw.Close();
				_currFile.Close();
			}
			return true;
		}

	    private static void WriteLogHeader()
	    {
            _sw.WriteLine("==============================START==============================");
	        _sw.WriteLine("Sync started on {0}\t{1}", DateTime.Now.ToShortDateString(),
	                      DateTime.Now.ToLongTimeString());
					
	        _filesRenamedCntSRC = 0;
	        _filesCopiedCntSRC = 0;
	        _filesDeletedCntSRC = 0;

	        _filesRenamedSizeSRC = 0;
	        _filesCopiedSizeSRC = 0;
	        _filesDeletedSizeSRC = 0;

	        _foldersCreatedCntSRC = 0;

	        _filesRenamedCntTGT = 0;
	        _filesCopiedCntTGT = 0;
	        _filesDeletedCntTGT = 0;

	        _filesRenamedSizeTGT = 0;
	        _filesCopiedSizeTGT = 0;
	        _filesDeletedSizeTGT = 0;

	        _foldersCreatedCntTGT = 0;
	    }

	    private static void WriteSyncResultsLogEntry()
	    {
            _sw.WriteLine();
	        _sw.WriteLine("*** Sync Results ***");
	        _sw.WriteLine("ACTION          \t|FROM SOURCE\t\t|FROM TARGET");

	        _sw.WriteLine("Files Copied :  \t[{0}\t({1} bytes)]\t\t[{2}\t({3} bytes)]", _filesCopiedCntSRC, _filesCopiedSizeSRC, _filesCopiedCntTGT, _filesCopiedSizeTGT);
	        _sw.WriteLine("Files Deleted:  \t[{0}\t({1} bytes)]\t\t[{2}\t({3} bytes)]", _filesDeletedCntSRC, _filesDeletedSizeSRC, _filesDeletedCntTGT, _filesDeletedSizeTGT);
	        _sw.WriteLine("Files Renamed:  \t[{0}\t({1} bytes)]\t\t[{2}\t({3} bytes)]", _filesRenamedCntSRC, _filesRenamedSizeSRC, _filesRenamedCntTGT, _filesRenamedSizeTGT);
	        _sw.WriteLine("Folders Created:  \t[{0}]\t\t\t[{1}]", _foldersCreatedCntSRC, _foldersCreatedCntTGT);
	        _sw.WriteLine("*********************");
	        _sw.WriteLine("Sync ended on {0}\t{1}", DateTime.Now.ToShortDateString(),
	                      DateTime.Now.ToLongTimeString());
            _sw.WriteLine("==============================END==============================");
	        _sw.WriteLine();
	        _sw.WriteLine();
	    }

        /// <summary>
        /// Write log for copy, rename, delete files or create folders.
        ///  </summary>
        /// <param name="logType"></param>
        /// <param name="srcPath"></param>
        /// <param name="srcSize"></param>
        /// <param name="tgtPath"></param>
        /// <param name="tgtSize"></param>
        /// <returns>successful</returns>
	    public static bool WriteLog(LogType logType, string srcPath, long srcSize, string tgtPath, long tgtSize)
		{
			try
			{
			    string status;
			    string oriPath = "";
			    long oriSize = 0;
			    string destPath = "";
                long destSize = 0;

                Directory.CreateDirectory(_logFileLocation);
				_currFile = new FileStream(_logFileName, FileMode.Append, FileAccess.Write, FileShare.Read);
				_sw = new StreamWriter(_currFile);

				switch (logType)
				{
                    case LogType.DeleteSRC:

                        status = DeleteStatusSRC;
                        _filesDeletedCntSRC++;
                        _filesDeletedSizeSRC += srcSize;
                        oriPath = srcPath;
                        oriSize = srcSize;
                        WriteLogEntry(status, oriPath, oriSize);

				        break;

                    case LogType.DeleteTGT:
      
                        status = DeleteStatusTGT;
                        _filesDeletedCntTGT++;
                        _filesDeletedSizeTGT += srcSize;
                        oriPath = tgtPath;
                        oriSize = tgtSize;
                        WriteLogEntry(status, oriPath, oriSize);

                        break;
                    
				    case LogType.CreateSRC:
                     
                        status = CreateStatusSRC;
                        _foldersCreatedCntSRC++;
                        oriPath = srcPath;
                        WriteLogEntry(status, oriPath);
                        
                        break;
                     
				    case LogType.CreateTGT:
                     
                        status = CreateStatusTGT;
                        _foldersCreatedCntTGT++;
                        oriPath = tgtPath;
                        WriteLogEntry(status, oriPath);
                        
                        break;                     				        
    
					case LogType.CopySRC:
                   
                        status = CopyStatusSRC;
                        _filesCopiedCntSRC++;
                        _filesCopiedSizeSRC += srcSize;
                        oriPath = srcPath;
                        oriSize = srcSize;
                        destPath = tgtPath;
                        destSize = tgtSize;
                        WriteLogEntry(status, oriPath, oriSize, destPath, destSize);

				        break;
                    
					case LogType.RenameSRC:

                        status = RenameStatusSRC;
                        _filesRenamedCntSRC++;
                        _filesRenamedSizeSRC += srcSize;
                        oriPath = srcPath;
                        oriSize = srcSize;
                        destPath = tgtPath;
                        destSize = tgtSize;
                        WriteLogEntry(status, oriPath, oriSize, destPath, destSize);

                        break;

                    case LogType.CopyTGT:

                        status = CopyStatusTGT;
                        _filesCopiedCntTGT++;
                        _filesCopiedSizeTGT += tgtSize;
                        oriPath = tgtPath;
                        oriSize = tgtSize;
                        destPath = srcPath;
                        destSize = srcSize;
                        WriteLogEntry(status, oriPath, oriSize, destPath, destSize);

                        break;

					case LogType.RenameTGT:

                        status = RenameStatusTGT;
                        _filesRenamedCntTGT++;
                        _filesRenamedSizeTGT += tgtSize;
                        oriPath = tgtPath;
                        oriSize = tgtSize;
                        destPath = srcPath;
                        destSize = srcSize;
                        WriteLogEntry(status, oriPath, oriSize, destPath, destSize);

                        break;                                                              
				}
			}
			catch (IOException e)
			{
				WriteErrorLog(e.Message);
				return false;
			}
			finally
			{
				_sw.Close();
				_currFile.Close();
			}
			return true;
		}

	    private static void WriteLogEntry(string status, string oriPath, long oriSize, string destPath, long destSize)
        {
            //file copy or Rename entry format:
            //|date|time|status|oriPath|oriSize|destPath|destSize|
	        _sw.WriteLine("{0}\t{1}\t" + status + "\t[{2}\t({3} bytes)] =-> [{4}\t({5} bytes)]" ,
	                      DateTime.Now.ToShortDateString(),
	                      DateTime.Now.ToLongTimeString(), oriPath, oriSize,
	                      destPath, destSize);
	    }

	    private static void WriteLogEntry(string status, string oriPath)
	    {
            //file create Entry format:
            //|date|time|status|oriPath|
	        _sw.WriteLine("{0}\t{1}\t" + status + "\t[{2}]",
	                      DateTime.Now.ToShortDateString(),
	                      DateTime.Now.ToLongTimeString(), oriPath);
	    }

	    private static void WriteLogEntry(string status, string oriPath, long oriSize)
	    {
            //file delete Entry format:
            //|date|time|status|oriPath|oriSize|
	        _sw.WriteLine("{0}\t{1}\t" + status + "\t[{2}\t({3} bytes)]",
	                      DateTime.Now.ToShortDateString(),
	                      DateTime.Now.ToLongTimeString(), oriPath, oriSize);
	    }

        /// <summary>
        /// delete log file
        /// </summary>
        /// <param name="metaDataDir"></param>
        /// <param name="syncTaskName"></param>
        /// <returns>successful</returns>
	    public static bool DeleteLog(string metaDataDir, string syncTaskName)
		{
            if (metaDataDir == null) throw new ArgumentNullException("metaDataDir");
            if (syncTaskName == null) throw new ArgumentNullException("syncTaskName");

            string logFileToDeletePath = metaDataDir + @"\" + syncTaskName + ".log";

            if (File.Exists(logFileToDeletePath))
            {
                try
                {
                    File.Delete(logFileToDeletePath);
                }
                catch (IOException e)
                {
                    WriteErrorLog(e.Message);
                    WriteErrorLog(e.StackTrace);
                    return false;
                }
            }
            return true;
		}

        /// <summary>
        /// read log file contents
        /// </summary>
        /// <param name="metaDataDir"></param>
        /// <param name="syncTaskName"></param>
        /// <returns>log contents</returns>
		public static string ReadLog(string metaDataDir, string syncTaskName)
		{
            StreamReader sr;

            if (metaDataDir == null) throw new ArgumentNullException("metaDataDir");
            if (syncTaskName == null) throw new ArgumentNullException("syncTaskName");

            // *** Read from file ***
            string retrievedLogEntry;
            try
            {
                _currFile = new FileStream(metaDataDir + @".\" + syncTaskName + ".log", FileMode.Open, FileAccess.Read);
                sr = new StreamReader(_currFile);

                retrievedLogEntry = sr.ReadToEnd();

                sr.Close();
                _currFile.Close();
            }

            catch (IOException e)
            {
                WriteErrorLog(e.Message);
                return null;
            }

            return retrievedLogEntry;
		}

        /// <summary>
        /// general program error log
        /// </summary>
        /// <param name="errorMsg"></param>
        /// <returns>successful</returns>
		public static bool WriteErrorLog(string errorMsg)
		{
			if (errorMsg == null) throw new ArgumentNullException("errorMsg");
			if (errorMsg.Equals(string.Empty)) throw new ArgumentException("Empty string passed", "errorMsg");

			try
			{
                Directory.CreateDirectory(_logFileLocation); 
                
                _currFile = new FileStream(_logFileName, FileMode.Append, FileAccess.Write, FileShare.Read);
				_sw = new StreamWriter(_currFile);

				//log entry format:
				//|date|time|errorMsg
				_sw.WriteLine("{0}\t{1}\t[***ERROR***]: {2}",
				DateTime.Now.ToShortDateString(),
				DateTime.Now.ToLongTimeString(), errorMsg);
			}
			catch (IOException e)
			{
				Console.WriteLine(e.Message);
				return false;
			}
			finally
			{
				_sw.Close();
				_currFile.Close();
			}
			return true;
		}
		#endregion
	}
}