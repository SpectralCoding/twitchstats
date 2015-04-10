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
	using System.Diagnostics;
	using System.IO;
	using System.Threading.Tasks;
	using DataManager;
	using MySql.Data.MySqlClient;
	using Utility;

	public static class ChannelParser {
		public static void Parse(String logDir, String channelName) {
			AppLog.WriteLine(4, "INFO", "Parse Channel: " + channelName);
			AppLog.WriteLine(5, "DEBUG", "   LogDir: " + logDir);
			AppLog.WriteLine(5, "DEBUG", "   ChannelName: " + channelName);
			LineParser.Accuracies = new Int32[] { 1, 5, 15, 30, 60, 360, 720, 1440 };
			////if (!ChannelDataMan.ChannelExists(channelName)) {
			////	ChannelDataMan.AddChannel(channelName);
			////}
			////Int32 channelID = ChannelDataMan.GetChannelID(channelName);
			Dictionary<String, LogRecord> parseList = GetLogsToParse(logDir, channelName);
			foreach (KeyValuePair<String, LogRecord> curKVP in parseList) {
				// Add all the commands from this log into the list.
				ParseLog(logDir, channelName, curKVP.Value);
			}
		}

		public static Dictionary<String, LogRecord> GetLogsToParse(String logDir, String channelName) {
			Dictionary<String, LogRecord> channelLogs = LogDataMan.GetLogs(channelName);
			// Something broken here. pick up here tomorrow.
			Dictionary<String, LogRecord> returnLogs = new Dictionary<String, LogRecord>();
			String[] logList = Directory.GetFiles(Path.Combine(logDir, channelName));
			foreach (String curLog in logList) {
				if (!channelLogs.ContainsKey(Path.GetFileName(curLog))) {
					LogRecord newLogRecord = new LogRecord();
					newLogRecord.ChannelName = channelName;
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

		private static void ParseLog(String logDir, String channelName, LogRecord logRecord) {
			AppLog.WriteLine(4, "INFO", "   Parsing Log: " + logRecord.Filename);
			AppLog.WriteLine(4, "INFO", "      Line: " + logRecord.LastLine);
			Int32 lineNumber = 0;
			String curLine;
			DateTime logDate;
			List<Task> taskList = new List<Task>();
			DateTime.TryParseExact(
				Path.GetFileNameWithoutExtension(logRecord.Filename),
				"yyyy-MM-dd",
				null,
				System.Globalization.DateTimeStyles.None,
				out logDate);
			StreamReader logSR = new StreamReader(logRecord.CurrentInfo.FullName);
			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();
			while ((curLine = logSR.ReadLine()) != null) {
				if (logRecord.LastLine < lineNumber) {
					////deltaList.AddRange(ParseLine(curLine, logDate, channelName));
					ParseLine(curLine, logDate, channelName, taskList);
					if ((lineNumber % 1000) == 0) {
						AppLog.WriteLine(5, "DEBUG", "            At Line: " + lineNumber);
					}
				}
				lineNumber++;
			}
			// Make sure we have the latest size.
			logRecord.CurrentInfo = new FileInfo(logRecord.CurrentInfo.FullName);
			stopWatch.Stop();
			TimeSpan ts = stopWatch.Elapsed;
			String speedText = (lineNumber - logRecord.LastLine) + " lines in " + ts.TotalSeconds.ToString("N2") +
					"s (" + ((lineNumber - logRecord.LastLine) / ts.TotalSeconds).ToString("N0") + " lines/s)";
			AppLog.WriteLine(4, "INFO", "      Speed: " + speedText);
			// Update the database with new metric counts here.
			if (logRecord.LastSize == 0) {
				LogDataMan.UpdateLog(
					new LogRecord {
						ChannelName = channelName,
						Filename = logRecord.Filename,
						IsClosed = false,
						LastLine = 0,
						CurrentInfo = logRecord.CurrentInfo,
						LastSize = logRecord.CurrentInfo.Length
					},
					lineNumber);
			} else if (logRecord.LastLine < lineNumber) {
				LogDataMan.UpdateLog(logRecord, lineNumber);
			}
		}

		private static void ParseLine(String line, DateTime date, String channelName, List<Task> taskList) {
			TimeSpan tempTS;
			if (line.Length < 12) {
				// Our line is too short, so skip it.
				// [HH:MM:SS] *
				return;
			}
			if (!TimeSpan.TryParseExact(line.Substring(1, 8), @"hh\:mm\:ss", null, out tempTS)) {
				// Out line doesn't begin with a valid timestamp so skip it.
				return;
			}
			date = date.Add(tempTS);
			String username;
			if (line.Substring(11, 1) == "<") {
				// This is a regular chat message
				Int32 endNickIndex = line.IndexOf('>');
				username = line.Substring(12, endNickIndex - 12);
				String message = line.Substring(endNickIndex + 2);
				LineParser.Message(date, channelName, username, message, taskList);
				LineParser.ScanEmotes(date, channelName, username, message, taskList);
			} else {
				// If it's not a '<' then it HAS to be a '*'. No point in testing, it'll slow down processing.
				// This is an action, join, part, or mode.
				if (line.Substring(12, 1) == " ") {
					Int32 endNickIndex = line.IndexOf(' ', 13);
					username = line.Substring(13, endNickIndex - 13);
					String message = line.Substring(endNickIndex + 1);
					LineParser.Action(date, channelName, username, message, taskList);
					LineParser.ScanEmotes(date, channelName, username, message, taskList);
				} else {
					// We don't care about these yet and they just slow stuff down.
					////String beforeColon = line.Substring(15, line.IndexOf(':', 15) - 15);
					////switch (beforeColon) {
					////	case "Joins":
					////		username = line.Substring(22, line.IndexOf(" ", 22) - 22);
					////		LineParser.Join(date, channelName, username, taskList);
					////		break;
					////	case "Parts":
					////		username = line.Substring(22, line.IndexOf(" ", 22) - 22);
					////		LineParser.Part(date, channelName, username, taskList);
					////		break;
					////	case "jtv sets mode":
					////		switch (line.Substring(30, 2)) {
					////			case "+o":
					////				username = line.Substring(33);
					////				break;
					////			case "-o":
					////				username = line.Substring(33);
					////				break;
					////			default:
					////				AppLog.WriteLine(3, "WARNING", "      Unknown Line: " + line);
					////				break;
					////		}
					////		break;
					////	default:
					////		AppLog.WriteLine(3, "WARNING", "      Unknown Line: " + line);
					////		break;
					////}
				}
			}
			if (taskList.Count > 100000) {
				Stopwatch stopWatch = new Stopwatch();
				stopWatch.Start();
				Task.WaitAll(taskList.ToArray());
				taskList.Clear();
				stopWatch.Stop();
				TimeSpan ts = stopWatch.Elapsed;
				AppLog.WriteLine(5, "DEBUG", "         Waiting for Redis: " + ts.TotalMilliseconds + "ms");
			}
		}
	}
}
