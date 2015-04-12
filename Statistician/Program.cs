/// <copyright file="Program.cs" company="SpectralCoding.com">
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

namespace Statistician {
	using System;
	using System.Configuration;
	using System.Reflection;
	using System.Threading;
	using DataManager;
	using EmoteManager;
	using ParseEngine;
	using Utility;

	public class Program {
		private static void Main(String[] args) {
			Console.SetWindowSize(200, 50);
			AppLog.WriteLine(
				1,
				"STATUS",
				"Entered Statistician.Program.Main(). TwitchStats v" + Assembly.GetExecutingAssembly().GetName().Version + " started.");
			AppLog.SetLogLevel(Statistician.Properties.Settings.Default.LogLevel);
			DataStore.Connect(Statistician.Properties.Settings.Default.RedisConnectionString);
			EmoteGatherer.Download(Statistician.Properties.Settings.Default.GlobalEmotesOnly);
			while (true) {
				TwitchNetwork.Parse(Statistician.Properties.Settings.Default.IRCLogDir);
				WaitForInterrupt(Statistician.Properties.Settings.Default.ParseBreakSeconds);
			}
			////AppLog.WriteLine(1, "STATUS", "Finishing Statistician.Program.Main(). Waiting for user input.");
			////Console.ReadLine();
		}

		private static void WaitForInterrupt(Int32 pauseSeconds) {
			for (Int32 i = pauseSeconds; i > 0; i--) {
				Console.Write("\rWaiting " + i + "s before processing again...   \r");
				Thread.Sleep(1000);
			}
		}
	}
}
