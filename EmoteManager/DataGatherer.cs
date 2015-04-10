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
	using System.Net;
	using DataManager;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using StackExchange.Redis;
	using Utility;

	public static class DataGatherer {
		public static void Download() {
			AppLog.WriteLine(1, "STATUS", "Entered EmoteManager.DataGatherer.Download().");
			////GetGlobalEmotes();
			////GetAllEmotes();
			UpdateEmotes();
		}

		private static void UpdateEmotes() {
			AppLog.WriteLine(1, "STATUS", "Entered EmoteManager.DataGatherer.UpdateEmotes().");
			var db = DataStore.Redis.GetDatabase();
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
							AppLog.WriteLine(5, "DEBUG", "   Updating Emote Code \"" + jsonEmoteProperty.Value["code"] + "\".");
							HashEntry[] emoteHash = new HashEntry[] {
								new HashEntry("image_id", jsonEmoteProperty.Name),
								new HashEntry("channel", (jsonEmoteProperty.Value["channel"] ?? String.Empty).ToString()),
								new HashEntry("set_id", (jsonEmoteProperty.Value["set"] ?? String.Empty).ToString()),
								new HashEntry("description", (jsonEmoteProperty.Value["description"] ?? String.Empty).ToString()),
							};
							db.HashSet("Emote:" + jsonEmoteProperty.Value["code"], emoteHash, CommandFlags.FireAndForget);
							db.SetAdd("Emotes", jsonEmoteProperty.Value["code"].ToString(), CommandFlags.FireAndForget);
						}
					}
				}
			}
		}
	}
}
