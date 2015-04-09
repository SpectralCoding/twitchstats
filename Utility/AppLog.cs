/// <copyright file="AppLog.cs" company="SpectralCoding.com">
///     Copyright (c) 2015 SpectralCoding
/// </copyright>
/// <license>
/// This file is part of TwitchStats.
///
/// IncreBuild is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// IncreBuild is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with TwitchStats.  If not, see <http://www.gnu.org/licenses/>.
/// </license>
/// <author>Caesar Kabalan</author>

namespace Utility {
	using System;
	using System.Diagnostics;
	using System.IO;

	public static class AppLog {
		private static Int32 s_logLevel = 5;
		private static FileStream s_logFile;
		private static StreamWriter s_logStream;
		private static Boolean s_enabled = true;

		public static Boolean Enabled {
			get { return s_enabled; }
			set { s_enabled = value; }
		}

		/// <summary>
		/// Sets the verbosity of logging.
		/// </summary>
		/// <param name="newLogLevel">
		/// 1 = Minimum. Just Errors and major status changes. 2 = Non-Critical Errors/Warnings.
		/// 3 = Unused. 4 = Configuration Output. 5 = Everything.</param>
		public static void SetLogLevel(Int32 newLogLevel) {
			// 1 = Minimum. Just Errors and major status changes
			// 2 = Non-Critical Errors/Warnings
			// 3 = Unused
			// 4 = Configuration Output
			// 5 = Most Verbose. Everything.
			s_logLevel = newLogLevel;
			AppLog.WriteLine(1, "STATUS", "Logging level set to " + s_logLevel + ".");
		}

		/// <summary>
		/// Closes an opened log for program output.
		/// </summary>
		public static void CloseLog() {
			s_logStream.Close();
			s_logFile.Close();
		}

		[DebuggerStepThrough]
		public static void WriteLine(Int32 level, String logType, String lineToAdd, Boolean firstLine = true) {
			if (s_enabled) {
				if (level <= s_logLevel) {
					if (s_logFile == null) {
						OpenLog();
					}
					String output;
					try {
						if (firstLine) {
							output = String.Format("{0:HH:mm:ss.fff} [{1,-6}]\t{2}", DateTime.UtcNow, logType, lineToAdd);
							s_logStream.WriteLine(output);
							Console.WriteLine(output);
						} else {
							s_logStream.WriteLine("\t{0}", lineToAdd);
							Console.WriteLine("\t{0}", lineToAdd);
						}
					} catch (ArgumentOutOfRangeException) {
					}
				}
			}
		}

		/// <summary>
		/// Creates and Opens a log file for program output.
		/// </summary>
		private static void OpenLog() {
			if (!Directory.Exists("logs/")) {
				Directory.CreateDirectory("logs/");
			}
			s_logFile = new FileStream(
				@"logs/" + DateTime.UtcNow.ToString("yyMMdd-HHmmss") + ".log",
				FileMode.Create,
				FileAccess.ReadWrite);
			s_logStream = new StreamWriter(s_logFile);
			s_logStream.AutoFlush = true;
		}
	}
}
