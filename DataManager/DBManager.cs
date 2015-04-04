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
	using System.Data.SQLite;
	using System.IO;
	using Utility;

	public static class DBManager {
		private static SQLiteConnection s_dbConnection;

		public static SQLiteConnection DbConnection {
			get { return s_dbConnection; }
			set { s_dbConnection = value; }
		}

		public static void OpenDatabase(String filePath) {
			AppLog.WriteLine(1, "STATUS", "Entered DataManager.DBManager.OpenDatabase().");
			if (!File.Exists(filePath)) {
				SQLiteConnection.CreateFile(filePath);
				DbConnection = new SQLiteConnection("Data Source=" + filePath + ";Version=3;Journal Mode=Off");
				DbConnection.Open();
				CreateChannelTables("_global");
			} else {
				DbConnection = new SQLiteConnection("Data Source=" + filePath + ";Version=3;Journal Mode=Off");
				DbConnection.Open();
			}
		}

		private static void CreateChannelTables(String channelName) {
			String sql =
				@"CREATE TABLE [main].[_global$emote_info] (
					[image_id] INTEGER,
					[code] TEXT NOT NULL,
					[channel] TEXT,
					[set] TEXT,
					[description] TEXT);";
			SQLiteCommand command = new SQLiteCommand(sql, DbConnection);
			command.ExecuteNonQuery();
		}
	}
}
