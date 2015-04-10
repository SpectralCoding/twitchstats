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
	using System.Diagnostics;
	using System.Net;
	using System.Threading.Tasks;
	using DataManager;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using StackExchange.Redis;
	using Utility;

	public static class EmoteGatherer {
		private static List<String> s_emoteList = new List<String>();
		private static String[] s_emoteArr;
		private static String[] s_emoteArrSpacesBefore;
		private static String[] s_emoteArrSpacesAfter;
		private static String[] s_emoteArrSpacesAround;

		public static List<string> EmoteList {
			get { return s_emoteList; }
			set { s_emoteList = value; }
		}

		public static String[] EmoteArr {
			get { return s_emoteArr; }
			set { s_emoteArr = value; }
		}

		public static String[] EmoteArrSpaceBefore {
			get { return s_emoteArrSpacesBefore; }
			set { s_emoteArrSpacesBefore = value; }
		}

		public static String[] EmoteArrSpaceAfter {
			get { return s_emoteArrSpacesAfter; }
			set { s_emoteArrSpacesAfter = value; }
		}

		public static String[] EmoteArrSpacesAround {
			get { return s_emoteArrSpacesAround; }
			set { s_emoteArrSpacesAround = value; }
		}

		public static void Download() {
			AppLog.WriteLine(1, "STATUS", "Entered EmoteManager.EmoteGatherer.Download().");
			////GetGlobalEmotes();
			////GetAllEmotes();
			UpdateEmotes();
			DumpEmotes();
		}

		private static void DumpEmotes() {
			var db = DataStore.Redis.GetDatabase();
			RedisValue[] emoteArr = db.SetMembers("Emotes");
			foreach (RedisValue curEmote in emoteArr) {
				EmoteList.Add(curEmote);
			}
			EmoteArr = EmoteList.ToArray();
			EmoteArrSpaceAfter = new String[EmoteArr.Length];
			EmoteArrSpaceBefore = new String[EmoteArr.Length];
			EmoteArrSpacesAround = new String[EmoteArr.Length];
			Array.Copy(EmoteArr, EmoteArrSpaceAfter, EmoteArr.Length);
			Array.Copy(EmoteArr, EmoteArrSpaceBefore, EmoteArr.Length);
			Array.Copy(EmoteArr, EmoteArrSpacesAround, EmoteArr.Length);
			for (Int32 i = 0; i < EmoteArr.Length; i++) {
				EmoteArrSpaceAfter[i] += " ";
				EmoteArrSpaceBefore[i] = " " + EmoteArrSpaceBefore[i];
				EmoteArrSpacesAround[i] = " " + EmoteArrSpacesAround[i] + " ";
			}
		}

		private static void UpdateEmotes() {
			AppLog.WriteLine(1, "STATUS", "Entered EmoteManager.EmoteGatherer.UpdateEmotes().");
			var db = DataStore.Redis.GetDatabase();
			var rawJSON = new WebClient().DownloadString(@"http://twitchemotes.com/api_cache/v2/images.json");
			dynamic dynamicObj = JsonConvert.DeserializeObject(rawJSON);
			var jsonObj = (JObject)dynamicObj;
			foreach (JToken jsonTopToken in jsonObj.Children()) {
				if (jsonTopToken is JProperty) {
					var jsonTopProperty = jsonTopToken as JProperty;
					if (jsonTopProperty.Name == "images") {
						// Begin a transaction so our query is faster.
						List<Task> taskList = new List<Task>();
						foreach (JToken jsonEmoteToken in jsonTopProperty.Value.Children()) {
							var jsonEmoteProperty = jsonEmoteToken as JProperty;
							AppLog.WriteLine(5, "DEBUG", "   Updating Emote Code \"" + jsonEmoteProperty.Value["code"] + "\".");
							HashEntry[] emoteHash = new HashEntry[] {
								new HashEntry("image_id", jsonEmoteProperty.Name),
								new HashEntry("channel", (jsonEmoteProperty.Value["channel"] ?? String.Empty).ToString()),
								new HashEntry("set_id", (jsonEmoteProperty.Value["set"] ?? String.Empty).ToString()),
								new HashEntry("description", (jsonEmoteProperty.Value["description"] ?? String.Empty).ToString()),
							};
							taskList.Add(db.HashSetAsync("Emote:" + jsonEmoteProperty.Value["code"], emoteHash));
							taskList.Add(db.SetAddAsync("Emotes", jsonEmoteProperty.Value["code"].ToString(), CommandFlags.FireAndForget));
							if (taskList.Count > 100000) {
								Stopwatch stopWatch = new Stopwatch();
								stopWatch.Start();
								Task.WaitAll(taskList.ToArray());
								taskList.Clear();
								stopWatch.Stop();
								TimeSpan ts = stopWatch.Elapsed;
								AppLog.WriteLine(5, "DEBUG", "         Waiting for Redis: " + ts.TotalMilliseconds + "ms");
							}
						}
					}
				}
			}
		}
	}
}
