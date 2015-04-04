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
			Int32 loopReader = 0;
			using (MySqlDataReader reader = cmd.ExecuteReader()) {
				while (reader.Read()) {
					tableList.Add(reader.GetString(loopReader));
					AppLog.WriteLine(5, "Debug", "   Found Table: " + tableList[tableList.Count - 1]);
					loopReader++;
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
		}
	}
}
