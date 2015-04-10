/// <copyright file="LogDataMan.cs" company="SpectralCoding.com">
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
	using StackExchange.Redis;
	using Utility;

	public static class LogDataMan {
		public static void UpdateLog(LogRecord logRecord, Int32 lastLine) {
			AppLog.WriteLine(5, "DEBUG", "   Updating Log DB Entry: " + logRecord.Filename);
			AppLog.WriteLine(5, "DEBUG", "         Channel: " + logRecord.ChannelName);
			AppLog.WriteLine(5, "DEBUG", "       Is Closed: " + logRecord.IsClosed);
			AppLog.WriteLine(5, "DEBUG", "       Last Size: " + logRecord.LastSize + " -> " + logRecord.CurrentInfo.Length);
			AppLog.WriteLine(5, "DEBUG", "       Last Line: " + logRecord.LastLine + " -> " + lastLine);
			var db = DataStore.Redis.GetDatabase();
			List<HashEntry> hashList = new List<HashEntry>();
			hashList.Add(new HashEntry("channel_name", logRecord.ChannelName));
			hashList.Add(new HashEntry("filename", logRecord.Filename));
			hashList.Add(new HashEntry("is_closed", logRecord.IsClosed));
			hashList.Add(new HashEntry("last_size", logRecord.LastSize));
			hashList.Add(new HashEntry("last_line", lastLine));
			db.HashSet("Log:" + logRecord.ChannelName + "|" + logRecord.Filename, hashList.ToArray(), CommandFlags.FireAndForget);
			db.SetAdd("Logs", logRecord.ChannelName + "|" + logRecord.Filename, CommandFlags.FireAndForget);
		}

		public static Dictionary<String, LogRecord> GetLogs(String channelName) {
			Dictionary<String, LogRecord> returnDict = new Dictionary<String, LogRecord>();
			var db = DataStore.Redis.GetDatabase();
			RedisValue[] logList = db.SetMembers("Logs");
			foreach (RedisValue curLog in logList) {
				if (curLog.ToString().Substring(0, curLog.ToString().IndexOf('|')) == channelName) {
					HashEntry[] logAttrs = db.HashGetAll("Log:" + curLog);
					LogRecord temp = new LogRecord();
					foreach (HashEntry curAttr in logAttrs) {
						switch (curAttr.Name) {
							case "channel_name": temp.ChannelName = Convert.ToString(curAttr.Value); break;
							case "filename": temp.Filename = Convert.ToString(curAttr.Value); break;
							case "is_closed": temp.IsClosed = (curAttr.Value == 1); break;
							case "last_size": temp.LastSize = Convert.ToInt64(curAttr.Value); break;
							case "last_line": temp.LastLine = Convert.ToInt32(curAttr.Value); break;
						}
					}
					returnDict.Add(temp.Filename, temp);
				}
			}
			return returnDict;
		}
	}
}
