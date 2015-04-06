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
	using DataManager;
	using MySql.Data.MySqlClient;
	using Utility;

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
				// Add all the commands from this log into the list.
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
			List<DBDelta> deltaList = new List<DBDelta>();
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
					deltaList.AddRange(ParseLine(curLine, logDate, channelName));
				}
				lineNumber++;
			}
			// Make sure we have the latest size.
			logRecord.CurrentInfo = new FileInfo(logRecord.CurrentInfo.FullName);
			// Reduce the deltas for this log into individual changes for each row and apply them.
			ApplyDeltas(ConsolidateDeltas(deltaList));
			// Update the database with new metric counts here.
			if (logRecord.LastSize == 0) {
				LogDataMan.AddLog(logRecord.Filename, channelID, false, logRecord.CurrentInfo.Length, lineNumber);
			} else if (logRecord.LastLine < lineNumber) {
				LogDataMan.UpdateLog(logRecord, lineNumber);
			}
		}

		private static List<DBDeltaRow> ConsolidateDeltas(List<DBDelta> deltas) {
			List<DBDeltaRow> returnDeltaRows = new List<DBDeltaRow>();
			foreach (DBDelta curBulkDelta in deltas) {
				Boolean hasBeenAdded = false;
				foreach (DBDeltaRow curReturnDeltaRow in returnDeltaRows) {
					if (curReturnDeltaRow.Table == curBulkDelta.Table) {
						if (curReturnDeltaRow.TimeID == curBulkDelta.TimeID) {
							if (curReturnDeltaRow.Values.ContainsKey(curBulkDelta.Column)) {
								// We found an item in our returnDelta list which matches the table, the time id, and the column.
								curReturnDeltaRow.Values[curBulkDelta.Column] += curBulkDelta.Delta;
							} else {
								curReturnDeltaRow.Values.Add(curBulkDelta.Column, curBulkDelta.Delta);
							}
							hasBeenAdded = true;
						}
					}
				}
				if (!hasBeenAdded) {
					// We never found a matching item so we'll add one now.
					DBDeltaRow temp = new DBDeltaRow() { Table = curBulkDelta.Table, TimeID = curBulkDelta.TimeID };
					temp.Values.Add(curBulkDelta.Column, curBulkDelta.Delta);
					returnDeltaRows.Add(temp);
				}
			}
			return returnDeltaRows;
		}

		private static void ApplyDeltas(List<DBDeltaRow> deltas) {
			Dictionary<String, List<DBDeltaRow>> tableDeltas = new Dictionary<string, List<DBDeltaRow>>();
			// Split the delta list into per-table lists
			foreach (DBDeltaRow curDeltaRow in deltas) {
				if (tableDeltas.ContainsKey(curDeltaRow.Table)) {
					tableDeltas[curDeltaRow.Table].Add(curDeltaRow);
				} else {
					tableDeltas.Add(curDeltaRow.Table, new List<DBDeltaRow> { curDeltaRow });
				}
			}
			foreach (KeyValuePair<String, List<DBDeltaRow>> curTableKVP in tableDeltas) {
				// For each table, get the time_ids from the table so we can figure out which ones to update.
				List<Int32> timeIdList = new List<Int32>();
				List<DBDeltaRow> insertList = new List<DBDeltaRow>();
				MySqlCommand selectCmd = new MySqlCommand(@"SELECT time_id FROM `" + curTableKVP.Key + @"`;", DBManager.DbConnection);
				using (MySqlDataReader reader = selectCmd.ExecuteReader()) {
					while (reader.Read()) {
						timeIdList.Add(reader.GetInt32(0));
					}
				}
				foreach (DBDeltaRow curDeltaRow in curTableKVP.Value) {
					if (timeIdList.Contains(curDeltaRow.TimeID)) {
						// Update the time entry.
						String updateSql = @"UPDATE `" + curDeltaRow.Table + @"` SET ";
						foreach (String curColumn in curDeltaRow.Values.Keys) {
							updateSql += "`" + curColumn + "` = `" + curColumn + "` + @" + curColumn + ", ";
						}
						updateSql = updateSql.Substring(0, updateSql.Length - 2);
						updateSql += " WHERE `time_id` = @time_id LIMIT 1";
						MySqlCommand updateCmd = new MySqlCommand(updateSql, DBManager.DbConnection);
						foreach (KeyValuePair<String, Int32> curColumn in curDeltaRow.Values) {
							updateCmd.Parameters.AddWithValue("@" + curColumn.Key, curColumn.Value);
						}
						updateCmd.Parameters.AddWithValue("@time_id", curDeltaRow.TimeID);
						updateCmd.ExecuteNonQuery();
					} else {
						// Add the delta to be added later because there is no time_id to update.
						insertList.Add(curDeltaRow);
					}
				}
				// Holy shit what the fuck is this line?
				Dictionary<String, Dictionary<String, List<List<object>>>> insertFormats =
					new Dictionary<String, Dictionary<String, List<List<object>>>>();
				foreach (DBDeltaRow curNewRow in insertList) {
					// Add all the new time entries.
					String curFormat = @"(`time_id`, ";
					foreach (String curColumn in curNewRow.Values.Keys) {
						curFormat += "`" + curColumn + "`, ";
					}
					curFormat = curFormat.Substring(0, curFormat.Length - 2) + @")";
					List<object> curValues = new List<object>();
					curValues.Add(curNewRow.TimeID);
					foreach (KeyValuePair<String, Int32> curColumn in curNewRow.Values) {
						curValues.Add(curColumn.Value);
					}
					if (!insertFormats.ContainsKey(curNewRow.Table)) {
						insertFormats.Add(curNewRow.Table, new Dictionary<string, List<List<object>>>());
					}
					if (insertFormats[curNewRow.Table].ContainsKey(curFormat)) {
						insertFormats[curNewRow.Table][curFormat].Add(curValues);
					} else {
						insertFormats[curNewRow.Table].Add(curFormat, new List<List<object>>() { curValues });
					}
				}
				foreach (KeyValuePair<String, Dictionary<String, List<List<object>>>> curInsertTable in insertFormats) {
					foreach (KeyValuePair<String, List<List<object>>> curInsertFormat in curInsertTable.Value) {
						String insertSql = @"INSERT INTO `" + curInsertTable.Key + @"` " + curInsertFormat.Key + @" VALUES ";
						Int32 rowCount = 0;
						Int32 paramCount = 0;
						MySqlCommand insertCmd = new MySqlCommand();
						insertCmd.Connection = DBManager.DbConnection;
						foreach (List<object> curValueList in curInsertFormat.Value) {
							insertSql += "(";
							foreach (object curValue in curValueList) {
								insertSql += "@value_" + paramCount + @", ";
								insertCmd.Parameters.AddWithValue("@value_" + paramCount, curValue);
								paramCount++;
							}
							insertSql = insertSql.Substring(0, insertSql.Length - 2) + @"), ";
							rowCount++;
							if ((rowCount % 100) == 0) {
								// Our row count is a multiple of 100 so submit.
								insertSql = insertSql.Substring(0, insertSql.Length - 2);
								insertCmd.CommandText = insertSql;
								insertCmd.ExecuteNonQuery();
								insertCmd = new MySqlCommand();
								insertCmd.Connection = DBManager.DbConnection;
								insertSql = @"INSERT INTO `" + curInsertTable.Key + @"` " + curInsertFormat.Key + @" VALUES ";
								rowCount = 0;
								paramCount = 0;
							}
						}
						if (rowCount > 0) {
							// Submit the remainder of the rows.
							insertSql = insertSql.Substring(0, insertSql.Length - 2);
							insertCmd.CommandText = insertSql;
							insertCmd.ExecuteNonQuery();
						}
					}
				}
			}
		}

		private static List<DBDelta> ParseLine(String line, DateTime date, String channelName) {
			List<DBDelta> returnList = new List<DBDelta>();
			TimeSpan tempTS;
			TimeSpan.TryParseExact(line.Substring(1, 8), @"hh\:mm\:ss", null, out tempTS);
			date = date.Add(tempTS);
			String username;
			if (line.Substring(11, 1) == "<") {
				// This is a regular chat message
				Int32 endNickIndex = line.IndexOf('>');
				username = line.Substring(12, endNickIndex - 12);
				String message = line.Substring(endNickIndex + 2);
				returnList.AddRange(LineParser.Message(date, channelName, username, message));
			} else {
				// If it's not a '<' then it HAS to be a '*'. No point in testing, it'll slow down processing.
				// This is an action, join, part, or mode.
				if (line.Substring(12, 1) == " ") {
					Int32 endNickIndex = line.IndexOf(' ', 13);
					username = line.Substring(13, endNickIndex - 13);
					String message = line.Substring(endNickIndex + 1);
					returnList.AddRange(LineParser.Action(date, channelName, username, message));
				} else {
					String beforeColon = line.Substring(15, line.IndexOf(':', 15) - 15);
					switch (beforeColon) {
						case "Joins":
							username = line.Substring(22, line.IndexOf(" ", 22) - 22);
							returnList.AddRange(LineParser.Join(date, channelName, username));
							break;
						case "Parts":
							username = line.Substring(22, line.IndexOf(" ", 22) - 22);
							returnList.AddRange(LineParser.Part(date, channelName, username));
							break;
						case "jtv sets mode":
							switch (line.Substring(30, 2)) {
								case "+o":
									username = line.Substring(33);
									break;
								case "-o":
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
			return returnList;
		}
	}
}
