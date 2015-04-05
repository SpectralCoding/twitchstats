/// <copyright file="DataGatherer.cs" company="SpectralCoding.com">
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

namespace EmoteManager {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.Linq;
	using System.Linq;
	using System.Net;
	using DataManager;
	using MySql.Data.MySqlClient;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using Utility;

	public static class DataGatherer {
		public static void Download() {
			AppLog.WriteLine(1, "STATUS", "Entered EmoteManager.DataGatherer.Download().");
			////GetGlobalEmotes();
			////GetAllEmotes();
			UpdateEmotes();
		}

		private static Boolean NeedToUpdateEmoteInDatabase(DataRow existingRow, JProperty jsonEmoteProperty) {
			// This is needed to preserve duplicate emote issues. Assuming the latest image_id is the most up to date.
			if (Convert.ToInt32(existingRow["image_id"]) < Convert.ToInt32(jsonEmoteProperty.Name)) {
				// Emote in the Database is a lower ID than the one we're getting from json, so update it.
				return true;
			} else {
				// Emote in the DB is the same or higher than the ID from json.
				// Don't ever update it.
				return false;
			}
		}

		private static Emote CreateEmoteFromJsonEmote(JProperty jsonEmoteProperty) {
			Emote temp = new Emote();
			temp.ImageID = Convert.ToInt32(jsonEmoteProperty.Name);
			temp.Code = jsonEmoteProperty.Value["code"].ToString();
			temp.Channel = (jsonEmoteProperty.Value["channel"] ?? String.Empty).ToString();
			temp.SetID = (jsonEmoteProperty.Value["channel"] ?? String.Empty).ToString();
			temp.Description = (jsonEmoteProperty.Value["description"] ?? String.Empty).ToString();
			return temp;
		}

		private static void UpdateEmotes() {
			AppLog.WriteLine(1, "STATUS", "Entered EmoteManager.DataGatherer.UpdateEmotes().");

			MySqlDataAdapter dataAdapter = new MySqlDataAdapter(@"SELECT * FROM `_global$emote_list`;", DBManager.DbConnection);
			dataAdapter.InsertCommand = new MySqlCommand(
				@"INSERT INTO `_global$emote_list` (`image_id`, `code`, `channel`, `set_id`, `description`
				) VALUES (@image_id, @code, @channel, @set_id, @description);",
				DBManager.DbConnection);
			dataAdapter.InsertCommand.Parameters.Add("@image_id", MySqlDbType.Int32, 4, "image_id");
			dataAdapter.InsertCommand.Parameters.Add("@code", MySqlDbType.VarChar, 50, "code");
			dataAdapter.InsertCommand.Parameters.Add("@channel", MySqlDbType.VarChar, 50, "channel");
			dataAdapter.InsertCommand.Parameters.Add("@set_id", MySqlDbType.VarChar, 20, "set_id");
			dataAdapter.InsertCommand.Parameters.Add("@description", MySqlDbType.Text, 1000, "description");

			dataAdapter.UpdateCommand = new MySqlCommand(
				@"UPDATE `_global$emote_list` SET
					`image_id` = @image_id,
					`channel` = @channel,
					`set_id` = @set_id,
					`description` = @description
				WHERE `code` = @code;",
				DBManager.DbConnection);
			dataAdapter.UpdateCommand.Parameters.Add("@image_id", MySqlDbType.Int32, 4, "image_id");
			dataAdapter.UpdateCommand.Parameters.Add("@channel", MySqlDbType.VarChar, 50, "channel");
			dataAdapter.UpdateCommand.Parameters.Add("@set_id", MySqlDbType.VarChar, 20, "set_id");
			dataAdapter.UpdateCommand.Parameters.Add("@description", MySqlDbType.Text, 1000, "description");
			dataAdapter.UpdateCommand.Parameters.Add("@code", MySqlDbType.VarChar, 50, "code");

			DataSet dataSet = new DataSet();
			dataAdapter.Fill(dataSet, "_global$emote_list");
			DataTable emoteTable = dataSet.Tables["_global$emote_list"];
			////Dictionary<String, DataRow> emoteTableDictionary = emoteTable.AsEnumerable().ToDictionary(row => row.Field<String>("code"));


			var rawJSON = new WebClient().DownloadString(@"http://twitchemotes.com/api_cache/v2/images.json");
			dynamic dynamicObj = JsonConvert.DeserializeObject(rawJSON);
			var jsonObj = (JObject)dynamicObj;
			foreach (JToken jsonTopToken in jsonObj.Children()) {
				if (jsonTopToken is JProperty) {
					var jsonTopProperty = jsonTopToken as JProperty;
					if (jsonTopProperty.Name == "images") {
						// Begin a transaction so our query is faster.
						foreach (JToken jsonEmoteToken in jsonTopProperty.Value.Children()) {
							var jsonEmoteProperty = jsonEmoteToken as JProperty;
							AppLog.WriteLine(5, "DEBUG", "   Checking For Emote Code \"" + jsonEmoteProperty.Value["code"] + "\".");
							////var results = from myRow in emoteTable.AsEnumerable()
							////			  where myRow.Field<String>("code") == jsonEmoteProperty.Value["code"].ToString()
							////			  select myRow;
							////String testStr = jsonEmoteProperty.Value["code"].ToString();
							////var results = emoteTable.AsEnumerable()
							////	.Where(myRow => myRow.Field<String>("code") == testStr)
							////	.Take(1);
							String searchCode = jsonEmoteProperty.Value["code"].ToString();
							////var results = emoteTable.AsEnumerable()
							////	.Where(myRow => myRow.Field<String>("code") == searchCode)
							////	.Take(1);
							var result = emoteTable.AsEnumerable().FirstOrDefault(myRow => myRow.Field<String>("code") == searchCode);
							if (result != null) {
								// Performance issues here....????
								//DataRow[] results = emoteTable.Select("code = '" + jsonEmoteProperty.Value["code"].ToString() + "'");
								//if (results.Any()) {
								if (NeedToUpdateEmoteInDatabase(result, jsonEmoteProperty)) {
									AppLog.WriteLine(5, "DEBUG", "   Updating Emote Code \"" + jsonEmoteProperty.Value["code"] + "\".");
									//emoteTableDictionary[testStr]["code"] = jsonEmoteProperty.Value["code"].ToString();
									result["image_id"] = jsonEmoteProperty.Name;
									result["channel"] = (jsonEmoteProperty.Value["channel"] ?? String.Empty).ToString();
									result["set_id"] = (jsonEmoteProperty.Value["set"] ?? String.Empty).ToString();
									result["description"] = (jsonEmoteProperty.Value["description"] ?? String.Empty).ToString();
									////emoteTableDictionary = emoteTable.AsEnumerable().ToDictionary(row => row.Field<String>("code"));
								}
								////	// Emote exists in the database
								////	Emote existingEmote = databaseEmotes[jsonEmoteProperty.Value["code"].ToString()];
								////	if (NeedToUpdateEmoteInDatabase(existingEmote, jsonEmoteProperty)) {
								////		// Update it in the database
								////		AppLog.WriteLine(5, "DEBUG", "   Updating Emote Code \"" + jsonEmoteProperty.Value["code"] + "\".");
								////		MySqlCommand updateCmd = new MySqlCommand(
								////			@"UPDATE `_global$emote_list` SET
								////				`image_id` = @image_id,
								////				`channel` = @channel,
								////				`set_id` = @set_id,
								////				`description` = @description
								////			WHERE `code` = @code;",
								////			DBManager.DbConnection);
								////		updateCmd.Parameters.AddWithValue("@image_id", jsonEmoteProperty.Name);
								////		updateCmd.Parameters.AddWithValue("@code", jsonEmoteProperty.Value["code"].ToString());
								////		updateCmd.Parameters.AddWithValue(
								////			"@channel",
								////			(jsonEmoteProperty.Value["channel"] ?? String.Empty).ToString());
								////		updateCmd.Parameters.AddWithValue(
								////			"@set_id",
								////			(jsonEmoteProperty.Value["set"] ?? String.Empty).ToString());
								////		updateCmd.Parameters.AddWithValue(
								////			"@description",
								////			(jsonEmoteProperty.Value["description"] ?? String.Empty).ToString());
								////		updateCmd.ExecuteNonQuery();
								////		// Update the in-memory Emote Dictionary so we stay in sync.
								////		databaseEmotes[jsonEmoteProperty.Value["code"].ToString()] =
								////			CreateEmoteFromJsonEmote(jsonEmoteProperty);
								////	}
							} else {
								// Emote doesn't exist, so add it.
								AppLog.WriteLine(5, "DEBUG", "   Adding Emote Code \"" + jsonEmoteProperty.Value["code"] + "\".");
								DataRow newEmoteRow = emoteTable.NewRow();
								newEmoteRow["image_id"] = jsonEmoteProperty.Name;
								newEmoteRow["code"] = jsonEmoteProperty.Value["code"].ToString();
								newEmoteRow["channel"] = (jsonEmoteProperty.Value["channel"] ?? String.Empty).ToString();
								newEmoteRow["set_id"] = (jsonEmoteProperty.Value["set"] ?? String.Empty).ToString();
								newEmoteRow["description"] = (jsonEmoteProperty.Value["description"] ?? String.Empty).ToString();
								emoteTable.Rows.Add(newEmoteRow);
								// Update the in-memory Emote Dictionary so we stay in sync.
								////emoteTableDictionary = emoteTable.AsEnumerable().ToDictionary(row => row.Field<String>("code"));
							}
						}
						dataAdapter.Update(dataSet, "_global$emote_list");
					}
				}
			}
		}

		private static DataTable GetEmoteTable() {
			MySqlDataAdapter dataAdapter = new MySqlDataAdapter(@"SELECT * FROM `_global$emote_list`;", DBManager.DbConnection);
			DataSet dataSet = new DataSet();

			dataAdapter.Fill(dataSet, "_global$emote_list");
			return dataSet.Tables["_global$emote_list"];
		}
	}
}
