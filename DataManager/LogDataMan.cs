/// <copyright file="LogDataMan.cs" company="SpectralCoding.com">
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

namespace DataManager {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using MySql.Data.MySqlClient;
	using Utility;

	public static class LogDataMan {

		public static void AddLog(String fileName, Int32 channelID, Boolean isClosed, Int64 lastSize, Int32 lastLine) {
			AppLog.WriteLine(5, "DEBUG", "   Adding Log DB Entry: " + fileName);
			AppLog.WriteLine(5, "DEBUG", "      Channel ID: " + channelID);
			AppLog.WriteLine(5, "DEBUG", "       Is Closed: " + isClosed);
			AppLog.WriteLine(5, "DEBUG", "       Last Size: " + lastSize);
			AppLog.WriteLine(5, "DEBUG", "       Last Line: " + lastLine);
			MySqlCommand insertCmd = new MySqlCommand(
				@"INSERT INTO `_global$log_list` (
					`filename`, `channel_id`, `is_closed`, `last_size`, `last_line`
				) VALUES (
					@filename, @channel_id, @is_closed, @last_size, @last_line
				)",
				DBManager.DbConnection);
			insertCmd.Parameters.AddWithValue("@filename", fileName);
			insertCmd.Parameters.AddWithValue("@channel_id", channelID);
			insertCmd.Parameters.AddWithValue("@is_closed", isClosed);
			insertCmd.Parameters.AddWithValue("@last_size", lastSize);
			insertCmd.Parameters.AddWithValue("@last_line", lastLine);
			insertCmd.ExecuteNonQuery();
		}

		public static void UpdateLog(LogRecord logRecord, Int32 lastLine) {
			AppLog.WriteLine(5, "DEBUG", "   Updating Log DB Entry: " + logRecord.Filename);
			AppLog.WriteLine(5, "DEBUG", "      Channel ID: " + logRecord.ChannelID);
			AppLog.WriteLine(5, "DEBUG", "       Is Closed: " + logRecord.IsClosed);
			AppLog.WriteLine(5, "DEBUG", "       Last Size: " + logRecord.LastSize + " -> " + logRecord.CurrentInfo.Length);
			AppLog.WriteLine(5, "DEBUG", "       Last Line: " + logRecord.LastLine + " -> " + lastLine);
			MySqlCommand insertCmd = new MySqlCommand(
				@"UPDATE `_global$log_list` SET
					`is_closed` = @is_closed,
					`last_size` = @last_size,
					`last_line` = @last_line
				WHERE `id` = @id ;",
				DBManager.DbConnection);
			insertCmd.Parameters.AddWithValue("@id", logRecord.ID);
			insertCmd.Parameters.AddWithValue("@is_closed", logRecord.IsClosed);
			insertCmd.Parameters.AddWithValue("@last_size", logRecord.CurrentInfo.Length);
			insertCmd.Parameters.AddWithValue("@last_line", lastLine);
			insertCmd.ExecuteNonQuery();
		}


		public static Dictionary<String, LogRecord> GetLogs(Int32 channelID) {
			Dictionary<String, LogRecord> returnDict = new Dictionary<String, LogRecord>();
			MySqlCommand selectCmd = new MySqlCommand(@"SELECT * FROM `_global$log_list` WHERE `channel_id` = @channel_id;", DBManager.DbConnection);
			selectCmd.Parameters.AddWithValue("@channel_id", channelID);
			using (MySqlDataReader reader = selectCmd.ExecuteReader()) {
				while (reader.Read()) {
					LogRecord temp = new LogRecord();
					temp.ID = reader.GetInt32("id");
					temp.ChannelID = reader.GetInt32("channel_id");
					temp.Filename = reader.GetString("filename");
					temp.IsClosed = reader.GetBoolean("is_closed");
					temp.LastSize = reader.GetInt64("last_size");
					temp.LastLine = reader.GetInt32("last_line");
					returnDict.Add(temp.Filename, temp);
				}
			}
			return returnDict;
		}


	}
}
