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

namespace DataManager {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using StackExchange.Redis;

	public static class DataStore {
		private static ConnectionMultiplexer s_redis;

		public static ConnectionMultiplexer Redis {
			get { return s_redis; }
			set { s_redis = value; }
		}

		public static void Connect(String connectString) {
			s_redis = ConnectionMultiplexer.Connect(connectString);
		}
	}
}
