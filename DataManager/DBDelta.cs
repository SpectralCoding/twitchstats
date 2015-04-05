/// <copyright file="DBDelta.cs" company="SpectralCoding.com">
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

	public class DBDelta {
		private String m_table;
		private Int32 m_timeID;
		private String m_column;
		private Int32 m_delta;

		public String Table {
			get { return this.m_table; }
			set { this.m_table = value; }
		}

		public Int32 TimeID {
			get { return this.m_timeID; }
			set { this.m_timeID = value; }
		}

		public String Column {
			get { return this.m_column; }
			set { this.m_column = value; }
		}

		public Int32 Delta {
			get { return this.m_delta; }
			set { this.m_delta = value; }
		}
	}
}
