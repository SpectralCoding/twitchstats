/// <copyright file="TwitchNetwork.cs" company="SpectralCoding.com">
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
	using System.Configuration;
	using System.IO;
	using Utility;

	public static class TwitchNetwork {
		public static void Parse(string logDir) {
			AppLog.WriteLine(1, "STATUS", "Entered ParseEngine.TwitchNetwork.Parse().");
			AppLog.WriteLine(5, "DEBUG", "   P1: " + logDir);
			ParseChannels(logDir);
		}

		private static void ParseChannels(string logDir) {
			AppLog.WriteLine(1, "STATUS", "Entered ParseEngine.TwitchNetwork.ParseChannels().");
			AppLog.WriteLine(5, "DEBUG", "   P1: " + logDir);
			String[] channelList = Directory.GetDirectories(logDir);
			foreach (String curChannel in channelList) {
				String channelName = Path.GetFileName(curChannel);
				if (channelName.Substring(0, 1) == "#") {
					ChannelParser.Parse(Path.Combine(logDir, channelName));
				}
			}
		}
	}
}
