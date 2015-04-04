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
	using System.Data.SQLite;
	using System.Net;
	using DataManager;
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

		private static Boolean NeedToUpdateEmoteInDatabase(Emote existingEmote, JProperty jsonEmoteProperty) {
			// This is needed to preserve duplicate emote issues. Assuming the latest image_id is the most up to date.
			if (existingEmote.ImageID < Convert.ToInt32(jsonEmoteProperty.Name)) {
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
			temp.Set = (jsonEmoteProperty.Value["set"] ?? String.Empty).ToString();
			temp.Description = (jsonEmoteProperty.Value["description"] ?? String.Empty).ToString();
			return temp;
		}

		private static void UpdateEmotes() {
			AppLog.WriteLine(1, "STATUS", "Entered EmoteManager.DataGatherer.UpdateEmotes().");
			Dictionary<String, Emote> databaseEmotes = DumpEmotes();
			var rawJSON = new WebClient().DownloadString(@"http://twitchemotes.com/api_cache/v2/images.json");
			dynamic dynamicObj = JsonConvert.DeserializeObject(rawJSON);
			var jsonObj = (JObject)dynamicObj;
			foreach (JToken jsonTopToken in jsonObj.Children()) {
				if (jsonTopToken is JProperty) {
					var jsonTopProperty = jsonTopToken as JProperty;
					if (jsonTopProperty.Name == "images") {
						// Begin a transaction so our query is faster.
						SQLiteTransaction transaction = DBManager.DbConnection.BeginTransaction();
						foreach (JToken jsonEmoteToken in jsonTopProperty.Value.Children()) {
							var jsonEmoteProperty = jsonEmoteToken as JProperty;
							AppLog.WriteLine(5, "DEBUG", "   Checking For Emote Code \"" + jsonEmoteProperty.Value["code"] + "\".");
							if (databaseEmotes.ContainsKey(jsonEmoteProperty.Value["code"].ToString())) {
								// Emote exists in the database
								Emote existingEmote = databaseEmotes[jsonEmoteProperty.Value["code"].ToString()];
								if (NeedToUpdateEmoteInDatabase(existingEmote, jsonEmoteProperty)) {
									// Update it in the database
									AppLog.WriteLine(5, "DEBUG", "   Updating Emote Code \"" + jsonEmoteProperty.Value["code"] + "\".");
									SQLiteCommand updateCmd = new SQLiteCommand(
										@"UPDATE [_global$emote_info] SET
											[image_id] = $image_id,
											[channel] = $channel,
											[set] = $set,
											[description] = $description
										WHERE [code] = $code;",
										DBManager.DbConnection);
									updateCmd.Parameters.AddWithValue("$image_id", jsonEmoteProperty.Name);
									updateCmd.Parameters.AddWithValue("$code", jsonEmoteProperty.Value["code"]);
									updateCmd.Parameters.AddWithValue("$channel", jsonEmoteProperty.Value["channel"]);
									updateCmd.Parameters.AddWithValue("$set", jsonEmoteProperty.Value["set"]);
									updateCmd.Parameters.AddWithValue("$description", jsonEmoteProperty.Value["description"]);
									updateCmd.ExecuteNonQuery();
									// Update the in-memory Emote Dictionary so we stay in sync.
									databaseEmotes[jsonEmoteProperty.Value["code"].ToString()] =
										CreateEmoteFromJsonEmote(jsonEmoteProperty);
								}
							} else {
								// Emote doesn't exist, so add it.
								AppLog.WriteLine(5, "DEBUG", "   Adding Emote Code \"" + jsonEmoteProperty.Value["code"] + "\".");
								SQLiteCommand insertCmd = new SQLiteCommand(
									@"INSERT INTO
									[_global$emote_info] (
										[image_id],
										[code],
										[channel],
										[set],
										[description]
									) VALUES (
										$image_id,
										$code,
										$channel,
										$set,
										$description);",
									DBManager.DbConnection);
								insertCmd.Parameters.AddWithValue("$image_id", jsonEmoteProperty.Name);
								insertCmd.Parameters.AddWithValue("$code", jsonEmoteProperty.Value["code"]);
								insertCmd.Parameters.AddWithValue("$channel", jsonEmoteProperty.Value["channel"]);
								insertCmd.Parameters.AddWithValue("$set", jsonEmoteProperty.Value["set"]);
								insertCmd.Parameters.AddWithValue("$description", jsonEmoteProperty.Value["description"]);
								insertCmd.ExecuteNonQuery();
								// Update the in-memory Emote Dictionary so we stay in sync.
								databaseEmotes.Add(jsonEmoteProperty.Value["code"].ToString(), CreateEmoteFromJsonEmote(jsonEmoteProperty));
							}
						}
						transaction.Commit();
					}
				}
			}
		}

		private static Dictionary<String, Emote> DumpEmotes() {
			Dictionary<String, Emote> returnDict = new Dictionary<String, Emote>();
			SQLiteCommand selectCmd = new SQLiteCommand(
				@"SELECT * FROM [_global$emote_info];",
				DBManager.DbConnection);
			SQLiteDataReader reader = selectCmd.ExecuteReader();
			while (reader.Read()) {
				Emote temp = new Emote();
				temp.ImageID = Convert.ToInt32(reader["image_id"]);
				temp.Code = reader["code"].ToString();
				temp.Channel = reader["channel"].ToString();
				temp.Set = reader["set"].ToString();
				temp.Description = reader["description"].ToString();
				returnDict.Add(temp.Code, temp);
			}
			return returnDict;
		}
	}
}
