/// <copyright file="ChannelDataMan.cs" company="SpectralCoding.com">
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
	using MySql.Data.MySqlClient;
	using Utility;

	public static class ChannelDataMan {
		public static Boolean ChannelExists(String channelName) {
			AppLog.WriteLine(5, "DEBUG", "Checking if Channel \"" + channelName + "\" exists.");
			MySqlCommand checkCmd = new MySqlCommand("SELECT * FROM `_global$channel_list` WHERE `channel` = @channel LIMIT 1;", DBManager.DbConnection);
			checkCmd.Parameters.AddWithValue("@channel", channelName);
			if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0) {
				return true;
			}
			return false;
		}

		public static void AddChannel(String channelName) {
			AppLog.WriteLine(5, "DEBUG", "Adding Channel \"" + channelName + "\".");
			MySqlCommand insertCmd = new MySqlCommand(
				@"INSERT INTO
				`_global$channel_list` (
					`channel`
				) VALUES (
					@channel
				);",
				DBManager.DbConnection);
			insertCmd.Parameters.AddWithValue("@channel", channelName);
			insertCmd.ExecuteNonQuery();
		}

		public static Int32 GetChannelID(String channelName) {
			AppLog.WriteLine(5, "DEBUG", "Getting ID for Channel \"" + channelName + "\".");
			MySqlCommand selectCmd = new MySqlCommand("SELECT * FROM `_global$channel_list` WHERE `channel` = @channel LIMIT 1;", DBManager.DbConnection);
			selectCmd.Parameters.AddWithValue("@channel", channelName);
			using (MySqlDataReader reader = selectCmd.ExecuteReader()) {
				while (reader.Read()) {
					return reader.GetInt32("id");
				}
			}
			return -1;
		}
	}
}
