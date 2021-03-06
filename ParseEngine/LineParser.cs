﻿/// <copyright file="LineParser.cs" company="SpectralCoding.com">
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
	using System.Math;
	using System.Threading.Tasks;
	using DataManager;
	using EmoteManager;
	using StackExchange.Redis;
	using Utility;

	public static class LineParser {
		private static Int32[] s_accuracies;

		public static Int32[] Accuracies {
			get { return s_accuracies; }
			set { s_accuracies = value; }
		}

		public static void Message(DateTime date, String channelName, String username, String message, List<Task> taskList) {
			AddMessageChannel(date, "_global", taskList);
			AddMessageChannel(date, channelName, taskList);
		}

		public static void Action(DateTime date, String channelName, String username, String message, List<Task> taskList) {
			AddActionChannel(date, "_global", taskList);
			AddActionChannel(date, channelName, taskList);
		}

		public static void Join(DateTime date, String channelName, String username, List<Task> taskList) {
			AddJoinChannel(date, "_global", taskList);
			AddJoinChannel(date, channelName, taskList);
		}

		public static void Part(DateTime date, String channelName, String username, List<Task> taskList) {
			AddPartChannel(date, "_global", taskList);
			AddPartChannel(date, channelName, taskList);
		}

		public static void ScanEmotes(DateTime date, String channelName, String username, String message, List<Task> taskList) {
			var db = DataStore.Redis.GetDatabase();
			Int32 timeID;
			String[] words = message.Split(' ');
			for (Int32 wordIdx = 0; wordIdx < words.Length; wordIdx++) {
				if (EmoteGatherer.EmoteHashSet.Contains(words[wordIdx])) {
					foreach (Int32 curAcc in Accuracies) {
						timeID = GetTimeID(date, curAcc);
						String htField = curAcc + "|" + timeID;
						taskList.Add(db.HashIncrementAsync("Emote:_global|" + words[wordIdx], htField, 1));
						taskList.Add(db.HashIncrementAsync("Emote:" + channelName + "|" + words[wordIdx], htField, 1));
						htField = words[wordIdx] + "|" + timeID;
						taskList.Add(db.HashIncrementAsync("EmoteTime:_global|" + curAcc, htField, 1));
						taskList.Add(db.HashIncrementAsync("EmoteTime:" + channelName + "|" + curAcc, htField, 1));
					}
				}
			}
		}

		private static void AddMessageChannel(DateTime date, String channelName, List<Task> taskList) {
			var db = DataStore.Redis.GetDatabase();
			Int32 timeID;
			foreach (Int32 curAcc in Accuracies) {
				timeID = GetTimeID(date, curAcc);
				String htName = "Line:" + channelName + "|" + curAcc;
				taskList.Add(db.HashIncrementAsync(htName, timeID + "|Messages"));
				taskList.Add(db.HashIncrementAsync(htName, timeID + "|Total"));
				// Leave this off until we're sure we need it
				taskList.Add(db.SetAddAsync("Lines", htName));
			}
		}

		private static void AddActionChannel(DateTime date, String channelName, List<Task> taskList) {
			var db = DataStore.Redis.GetDatabase();
			Int32 timeID;
			foreach (Int32 curAcc in Accuracies) {
				timeID = GetTimeID(date, curAcc);
				String htName = "Line:" + channelName + "|" + curAcc;
				////db.HashIncrement(htName, "Actions");
				////db.HashIncrement(htName, "Total");
				////db.SetAdd("Lines", htName);
				taskList.Add(db.HashIncrementAsync(htName, timeID + "|Actions"));
				taskList.Add(db.HashIncrementAsync(htName, timeID + "|Total"));
				// Leave this off until we're sure we need it
				taskList.Add(db.SetAddAsync("Lines", htName));
				taskList.Add(db.SetAddAsync("Lines", htName));
			}
		}

		private static void AddJoinChannel(DateTime date, String channelName, List<Task> taskList) {
			var db = DataStore.Redis.GetDatabase();
			Int32 timeID;
			foreach (Int32 curAcc in Accuracies) {
				timeID = GetTimeID(date, curAcc);
				String htName = "Line:" + channelName + "|" + curAcc;
				////db.HashIncrement(htName, "Joins");
				////db.HashIncrement(htName, "Total");
				////db.SetAdd("Lines", htName);
				taskList.Add(db.HashIncrementAsync(htName, timeID + "|Joins"));
				taskList.Add(db.HashIncrementAsync(htName, timeID + "|Total"));
				// Leave this off until we're sure we need it
				taskList.Add(db.SetAddAsync("Lines", htName));
			}
		}

		private static void AddPartChannel(DateTime date, String channelName, List<Task> taskList) {
			var db = DataStore.Redis.GetDatabase();
			Int32 timeID;
			foreach (Int32 curAcc in Accuracies) {
				timeID = GetTimeID(date, curAcc);
				String htName = "Line:" + channelName + "|" + curAcc;
				////db.HashIncrement(htName, "Parts");
				////db.HashIncrement(htName, "Total");
				////db.SetAdd("Lines", htName);
				taskList.Add(db.HashIncrementAsync(htName, timeID + "|Parts"));
				taskList.Add(db.HashIncrementAsync(htName, timeID + "|Total"));
				// Leave this off until we're sure we need it
				taskList.Add(db.SetAddAsync("Lines", htName));
			}
		}

		private static Int32 GetTimeID(DateTime date, Int32 accuracyMinutes) {
			DateTime fakeEpoch = new DateTime(2015, 1, 1, 0, 0, 0);
			return Convert.ToInt32(Math.Floor((date - fakeEpoch).TotalMinutes / accuracyMinutes));
		}
	}
}
