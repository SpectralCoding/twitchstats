/// <copyright file="LineParser.cs" company="SpectralCoding.com">
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
	using StackExchange.Redis;

	public static class LineParser {
		public static void Message(DateTime date, String channelName, String username, String message, List<Task> taskList) {
			Int32[] accuracies = { 1, 5, 15, 30, 60, 360, 720, 1440 };
			AddMessageChannel(date, "_global", accuracies, taskList);
			AddMessageChannel(date, channelName, accuracies, taskList);
		}

		public static void Action(DateTime date, String channelName, String username, String message, List<Task> taskList) {
			Int32[] accuracies = { 1, 5, 15, 30, 60, 360, 720, 1440 };
			AddActionChannel(date, "_global", accuracies, taskList);
			AddActionChannel(date, channelName, accuracies, taskList);
		}

		public static void Join(DateTime date, String channelName, String username, List<Task> taskList) {
			Int32[] accuracies = { 1, 5, 15, 30, 60, 360, 720, 1440 };
			AddJoinChannel(date, "_global", accuracies, taskList);
			AddJoinChannel(date, channelName, accuracies, taskList);
		}

		public static void Part(DateTime date, String channelName, String username, List<Task> taskList) {
			Int32[] accuracies = { 1, 5, 15, 30, 60, 360, 720, 1440 };
			AddPartChannel(date, "_global", accuracies, taskList);
			AddPartChannel(date, channelName, accuracies, taskList);
		}

		private static void AddMessageChannel(DateTime date, String channelName, Int32[] accuracies, List<Task> taskList) {
			var db = DataStore.Redis.GetDatabase();
			Int32 timeID;
			foreach (Int32 curAcc in accuracies) {
				timeID = GetTimeID(date, curAcc);
				String htName = "Line:" + channelName + "|" + curAcc + "|" + timeID;
				taskList.Add(db.HashDecrementAsync(htName, "Messages"));
				taskList.Add(db.HashDecrementAsync(htName, "Total"));
				taskList.Add(db.SetAddAsync("Lines", htName));
			}
		}

		private static void AddActionChannel(DateTime date, String channelName, Int32[] accuracies, List<Task> taskList) {
			var db = DataStore.Redis.GetDatabase();
			Int32 timeID;
			foreach (Int32 curAcc in accuracies) {
				timeID = GetTimeID(date, curAcc);
				String htName = "Line:" + channelName + "|" + curAcc + "|" + timeID;
				////db.HashIncrement(htName, "Actions");
				////db.HashIncrement(htName, "Total");
				////db.SetAdd("Lines", htName);
				taskList.Add(db.HashDecrementAsync(htName, "Actions"));
				taskList.Add(db.HashDecrementAsync(htName, "Total"));
				taskList.Add(db.SetAddAsync("Lines", htName));
			}
		}

		private static void AddJoinChannel(DateTime date, String channelName, Int32[] accuracies, List<Task> taskList) {
			var db = DataStore.Redis.GetDatabase();
			Int32 timeID;
			foreach (Int32 curAcc in accuracies) {
				timeID = GetTimeID(date, curAcc);
				String htName = "Line:" + channelName + "|" + curAcc + "|" + timeID;
				////db.HashIncrement(htName, "Joins");
				////db.HashIncrement(htName, "Total");
				////db.SetAdd("Lines", htName);
				taskList.Add(db.HashDecrementAsync(htName, "Joins"));
				taskList.Add(db.HashDecrementAsync(htName, "Total"));
				taskList.Add(db.SetAddAsync("Lines", htName));
			}
		}

		private static void AddPartChannel(DateTime date, String channelName, Int32[] accuracies, List<Task> taskList) {
			var db = DataStore.Redis.GetDatabase();
			Int32 timeID;
			foreach (Int32 curAcc in accuracies) {
				timeID = GetTimeID(date, curAcc);
				String htName = "Line:" + channelName + "|" + curAcc + "|" + timeID;
				////db.HashIncrement(htName, "Parts");
				////db.HashIncrement(htName, "Total");
				////db.SetAdd("Lines", htName);
				taskList.Add(db.HashDecrementAsync(htName, "Parts"));
				taskList.Add(db.HashDecrementAsync(htName, "Total"));
				taskList.Add(db.SetAddAsync("Lines", htName));
			}
		}

		private static Int32 GetTimeID(DateTime date, Int32 accuracyMinutes) {
			DateTime fakeEpoch = new DateTime(2015, 1, 1, 0, 0, 0);
			return Convert.ToInt32(Math.Floor((date - fakeEpoch).TotalMinutes / accuracyMinutes));
		}
	}
}
