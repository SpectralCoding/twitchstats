/// <copyright file="ChannelParser.cs" company="SpectralCoding.com">
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

namespace ParseEngine {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using Utility;
	using DataManager;

	public static class ChannelParser {
		public static void Parse(String logDir, String channelName) {
			AppLog.WriteLine(1, "STATUS", "Entered ParseEngine.ChannelParser.Parse().");
			AppLog.WriteLine(5, "DEBUG", "   LogDir: " + logDir);
			AppLog.WriteLine(5, "DEBUG", "   ChannelName: " + channelName);
			if (!ChannelDataMan.ChannelExists(channelName)) {
				ChannelDataMan.AddChannel(channelName);
			}
			Int32 channelID = ChannelDataMan.GetChannelID(channelName);
			Dictionary<String, LogRecord> parseList = GetLogsToParse(logDir, channelName, channelID);
			foreach (KeyValuePair<String, LogRecord> curKVP in parseList) {
				ParseLog(logDir, channelName, curKVP.Value, channelID);
			}
		}

		public static Dictionary<String, LogRecord> GetLogsToParse(String logDir, String channelName, Int32 channelID) {
			Dictionary<String, LogRecord> channelLogs = LogDataMan.GetLogs(channelID);
			Dictionary<String, LogRecord> returnLogs = new Dictionary<String, LogRecord>();
			String[] logList = Directory.GetFiles(Path.Combine(logDir, channelName));
			foreach (String curLog in logList) {
				if (!channelLogs.ContainsKey(Path.GetFileName(curLog))) {
					LogRecord newLogRecord = new LogRecord();
					newLogRecord.ChannelID = channelID;
					newLogRecord.CurrentInfo = new FileInfo(curLog);
					newLogRecord.Filename = Path.GetFileName(curLog);
					newLogRecord.IsClosed = false;
					newLogRecord.LastSize = 0;
					newLogRecord.LastLine = 0;
					returnLogs.Add(newLogRecord.Filename, newLogRecord);
                }
			}
			foreach (KeyValuePair<String, LogRecord> curKVP in channelLogs) {
				LogRecord tempLogRecord = curKVP.Value;
				tempLogRecord.CurrentInfo = new FileInfo(Path.Combine(logDir, channelName, curKVP.Key));
				if (tempLogRecord.CurrentInfo.Length > tempLogRecord.LastSize) {
					returnLogs.Add(curKVP.Key, tempLogRecord);
				}
			}
			return returnLogs;
		}

		private static void ParseLog(String logDir, String channelName, LogRecord logRecord, Int32 channelID) {
			AppLog.WriteLine(1, "STATUS", "Entered ParseEngine.ChannelParser.ParseLog().");
			AppLog.WriteLine(5, "DEBUG", "   Parsing: " + logRecord.Filename);
			AppLog.WriteLine(5, "DEBUG", "      Starting at Line " + logRecord.LastLine + ".");
			Int32 lineNumber = 0;
			String curLine;
			DateTime logDate;
			DateTime.TryParseExact(
				Path.GetFileNameWithoutExtension(logRecord.Filename),
				"yyyy-MM-dd",
				null,
				System.Globalization.DateTimeStyles.None,
				out logDate);
			StreamReader logSR = new StreamReader(logRecord.CurrentInfo.FullName);
			while ((curLine = logSR.ReadLine()) != null) {
				if (logRecord.LastLine < lineNumber) {
					ParseLine(curLine, logDate);
				}
				lineNumber++;
			}
			// Make sure we have the latest size.
			logRecord.CurrentInfo = new FileInfo(logRecord.CurrentInfo.FullName);
            if (logRecord.LastSize == 0) {
				LogDataMan.AddLog(logRecord.Filename, channelID, false, logRecord.CurrentInfo.Length, lineNumber);
			} else if (logRecord.LastLine < lineNumber) {
				LogDataMan.UpdateLog(logRecord, lineNumber);
			}
        }

		private static void ParseLine(String line, DateTime date) {
			TimeSpan tempTS;
			TimeSpan.TryParseExact(line.Substring(1, 8), @"hh\:mm\:ss", null, out tempTS);
			date = date.Add(tempTS);
			String username;
			LineType lineType;
			if (line.Substring(11, 1) == "<") {
				// This is a regular chat message
				lineType = LineType.Message;
				Int32 endNickIndex = line.IndexOf('>');
				username = line.Substring(12, endNickIndex - 12);
				String message = line.Substring(endNickIndex + 2);
			} else {
				// If it's not a '<' then it HAS to be a '*'. No point in testing, it'll slow down processing.
				// This is an action, join, part, or mode.
				if (line.Substring(12, 1) == " ") {
					lineType = LineType.Action;
					Int32 endNickIndex = line.IndexOf(' ', 13);
					username = line.Substring(13, endNickIndex - 13);
					String message = line.Substring(endNickIndex + 1);
				} else {
					String beforeColon = line.Substring(15, line.IndexOf(':', 15) - 15);
					switch (beforeColon) {
						case "Joins":
							username = line.Substring(22, line.IndexOf(" ", 22) - 22);
							break;
						case "Parts":
							username = line.Substring(22, line.IndexOf(" ", 22) - 22);
							break;
						case "jtv sets mode":
							switch (line.Substring(30, 2)) {
								case "+o":
									lineType = LineType.ModePlusOperator;
									username = line.Substring(33);
									break;
								case "-o":
									lineType = LineType.ModeMinusOperator;
									username = line.Substring(33);
									break;
								default:
									AppLog.WriteLine(3, "WARNING", "Unknown Line: " + line);
									break;
							}
							break;
						default:
							AppLog.WriteLine(3, "WARNING", "Unknown Line: " + line);
							break;
					}
				}
			}
		}
	}
}
