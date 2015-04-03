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
	using System.IO;
	using Utility;

	public static class ChannelParser {
		public static void Parse(String logDir) {
			AppLog.WriteLine(1, "STATUS", "Entering ChannelParser.Parse('" + logDir + "')");
			String[] logList = Directory.GetFiles(logDir);
			foreach (String curLog in logList) {
				ParseLog(curLog);
			}
		}

		private static void ParseLog(String logFile) {
			String curLine;
			DateTime logDate;
			DateTime.TryParseExact(
				Path.GetFileNameWithoutExtension(logFile),
				"yyyy-MM-dd",
				null,
				System.Globalization.DateTimeStyles.None,
				out logDate);
			StreamReader logSR = new StreamReader(logFile);
			while ((curLine = logSR.ReadLine()) != null) {
				ParseLine(curLine, logDate);
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
									AppLog.WriteLine(5, "DEBUG", "Unknown Line: " + line);
									break;
							}
							break;
						default:
							AppLog.WriteLine(5, "DEBUG", "Unknown Line: " + line);
							break;
					}
				}
			}
		}
	}
}
