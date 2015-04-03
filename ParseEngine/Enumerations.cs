/// <copyright file="ChannelParser.cs" company="SpectralCoding.com">
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
	public enum LineType {
		/// <summary>
		/// Format of [##:##:##] &lt;nick&gt; Message Text
		/// </summary>
		Message,

		/// <summary>
		/// Format of [##:##:##] * nick Action Text
		/// </summary>
		Action,

		/// <summary>
		/// Format of [##:##:##] *** Joins: nick (ident@host)
		/// </summary>
		Join,

		/// <summary>
		/// Format of [##:##:##] *** Parts: nick (ident@host)
		/// </summary>
		Part,

		/// <summary>
		/// Format of [##:##:##] *** jtv sets mode: +o nick
		/// </summary>
		ModePlusOperator,

		/// <summary>
		/// Format of [##:##:##] *** jtv sets mode: -o nick
		/// </summary>
		ModeMinusOperator
	}
}
