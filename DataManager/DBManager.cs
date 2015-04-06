/// <copyright file="DBManager.cs" company="SpectralCoding.com">
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
	using System.IO;
	using MySql.Data.MySqlClient;
	using Utility;

	public static class DBManager {
		private static MySqlConnection s_dbConnection;

		public static MySqlConnection DbConnection {
			get { return s_dbConnection; }
			set { s_dbConnection = value; }
		}

		public static void OpenDatabase(String connectionString) {
			AppLog.WriteLine(1, "STATUS", "Entered DataManager.DBManager.OpenDatabase().");
			DbConnection = new MySqlConnection(connectionString);
			DbConnection.Open();
			CheckTables();
		}

		private static void CheckTables() {
			AppLog.WriteLine(1, "STATUS", "Entered DataManager.DBManager.CheckTables().");
			MySqlCommand cmd = new MySqlCommand(
				@"SHOW TABLES",
				s_dbConnection);
			List<String> tableList = new List<String>();
			using (MySqlDataReader reader = cmd.ExecuteReader()) {
				while (reader.Read()) {
					tableList.Add(reader.GetString(0));
					AppLog.WriteLine(5, "Debug", "   Found Table: " + tableList[tableList.Count - 1]);
				}
			}
			if (!tableList.Contains("_global$emote_list")) {
				AppLog.WriteLine(2, "WARNING", "   Missing Table \"_global$emote_list\". Creating...");
				String sql =
					@"CREATE TABLE IF NOT EXISTS `_global$emote_list` (
						`id` int(11) NOT NULL,
						`image_id` int(11) DEFAULT NULL,
						`code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
						`channel` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
						`set_id` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL,
						`description` text COLLATE utf8mb4_bin
					) ENGINE=InnoDB AUTO_INCREMENT=26620 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;
					ALTER TABLE `_global$emote_list` ADD PRIMARY KEY (`id`);
					ALTER TABLE `_global$emote_list` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;";
				MySqlCommand command = new MySqlCommand(sql, DbConnection);
				command.ExecuteNonQuery();
			}
			if (!tableList.Contains("_global$log_list")) {
				AppLog.WriteLine(2, "WARNING", "   Missing Table \"_global$log_list\". Creating...");
				String sql =
					@"CREATE TABLE `_global$log_list` (
						`id` int(11) NOT NULL,
						`channel_id` int(11) NOT NULL,
						`filename` varchar(50) COLLATE utf8mb4_bin NOT NULL,
						`is_closed` tinyint(1) NOT NULL,
						`last_size` int(11) NOT NULL,
						`last_line` int(11) NOT NULL
					) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;
					ALTER TABLE `_global$log_list` ADD PRIMARY KEY (`id`);
					ALTER TABLE `_global$log_list` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;";
				MySqlCommand command = new MySqlCommand(sql, DbConnection);
				command.ExecuteNonQuery();
			}
			if (!tableList.Contains("_global$channel_list")) {
				AppLog.WriteLine(2, "WARNING", "   Missing Table \"_global$channel_list\". Creating...");
				String sql =
					@"CREATE TABLE IF NOT EXISTS `_global$channel_list` (
						`id` int(11) NOT NULL,
						`channel` varchar(50) COLLATE utf8mb4_bin NOT NULL
					) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;
					ALTER TABLE `_global$channel_list` ADD PRIMARY KEY (`id`);
					ALTER TABLE `_global$channel_list` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;";
				MySqlCommand command = new MySqlCommand(sql, DbConnection);
				command.ExecuteNonQuery();
			}
			foreach (String curTime in new List<String>() { "1min", "5min", "15min", "30min", "1hr", "6hr", "12hr", "1day" }) {
				if (!tableList.Contains(@"_global$" + curTime + @"_line_stats")) {
					AppLog.WriteLine(2, "WARNING", "   Missing Table \"_global$" + curTime + "_line_stats\". Creating...");
					MySqlCommand createTable = new MySqlCommand(
							@"CREATE TABLE IF NOT EXISTS `_global$" + curTime + @"_line_stats` (
							`id` int(11) NOT NULL,
							`time_id` int(11) NOT NULL,
							`messages` int(11) NOT NULL,
							`actions` int(11) NOT NULL,
							`joins` int(11) NOT NULL,
							`parts` int(11) NOT NULL
						) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin;
						ALTER TABLE `_global$" + curTime + @"_line_stats` ADD PRIMARY KEY (`id`);
						ALTER TABLE `_global$" + curTime + @"_line_stats` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;",
						DBManager.DbConnection);
					createTable.ExecuteNonQuery();
				}
			}
		}
	}
}
