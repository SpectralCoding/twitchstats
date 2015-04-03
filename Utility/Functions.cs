/// <copyright file="Functions.cs" company="SpectralCoding.com">
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

namespace Utility {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;

	public static class Functions {
		public static string CombineAfterIndex(string[] inputArr, string glue, int startIndex) {
			string outputStr = String.Empty;
			for (int i = startIndex; i < inputArr.Length; i++) {
				outputStr += glue + inputArr[i];
			}
			return outputStr.Substring(1);
		}

		public static List<string> Parameterize(string parameters) {
			List<string> returnList = new List<string>();
			while (parameters.Length > 0) {
				if (parameters.Substring(0, 1) == "\"") {
					parameters = parameters.Substring(1);
					if (parameters.Contains("\"")) {
						returnList.Add(parameters.Substring(0, parameters.IndexOf('"')));
					} else {
						return null;
					}
				} else {
					if (parameters.Contains(" ")) {
						returnList.Add(parameters.Substring(0, parameters.IndexOf(' ')));
					} else {
						returnList.Add(parameters);
						break;
					}
				}
				parameters = parameters.Substring(returnList[returnList.Count - 1].Length);
				if (parameters.Substring(0, 1) == "\"") {
					parameters = parameters.Substring(1);
				}
				parameters = parameters.TrimStart();
			}
			return returnList;
		}

		/// <summary>
		/// Counts the number of occurances of Needle in Haystack.
		/// </summary>
		/// <param name="haystack">String being search</param>
		/// <param name="needle">String being searched for</param>
		/// <returns>Integer representing the number of occurances of Needle in Haystack</returns>
		public static int OccurancesInString(String haystack, String needle) {
			return Regex.Matches(haystack, needle).Count;
		}

		/// <summary>
		/// Prints an ArrayList object to the console.
		/// </summary>
		/// <param name="inputArray">ArrayList to print.</param>
		public static void PrintArrayList(ArrayList inputArray) {
			Console.WriteLine("Array Output: ");
			for (int i = 0; i < inputArray.Count; i++) {
				Console.WriteLine("[" + i + "] " + inputArray[i]);
			}
		}

		/// <summary>
		/// Dumps a byte[] Array to a file.
		/// </summary>
		/// <param name="inputBytes">byte[] Array to export.</param>
		/// <param name="fileName">Filename to export to.</param>
		public static void DumpByteArrayToFile(byte[] inputBytes, String fileName) {
			File.Delete(fileName);
			FileStream tempFS = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
			if (tempFS.CanWrite) {
				tempFS.Write(inputBytes, 0, inputBytes.Length);
			}
			tempFS.Close();
		}

		/// <summary>
		/// Dumps a String to a file.
		/// </summary>
		/// <param name="inputString">String to export.</param>
		/// <param name="fileName">Filename to export to.</param>
		public static void DumpStringToFile(String inputString, String fileName) {
			File.Delete(fileName);
			FileStream tempFS = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
			if (tempFS.CanWrite) {
				tempFS.Write(Encoding.UTF8.GetBytes(inputString), 0, Encoding.UTF8.GetBytes(inputString).Length);
			}
			tempFS.Close();
		}

		/// <summary>
		/// Prints the ASCII values of every character in a character array delimited by a pipe character to the Console.
		/// </summary>
		/// <param name="inputCharArr">String to be split.</param>
		public static void PrintCharArrASCII(char[] inputCharArr) {
			Console.Write("|");
			for (int i = 0; i < inputCharArr.Length; i++) {
				Console.Write(((int)inputCharArr[i]).ToString() + "|");
			}
			Console.WriteLine();
		}

		/// <summary>
		/// Prints the ASCII values of every character in a string delimited by a pipe character to the Console.
		/// </summary>
		/// <param name="inputStr">String to be split.</param>
		public static void PrintStringASCII(String inputStr) {
			char[] charArr = inputStr.ToCharArray();
			Console.Write("|");
			for (int i = 0; i < charArr.Length; i++) {
				Console.Write(((int)charArr[i]).ToString() + "|");
			}
			Console.WriteLine();
		}

		/// <summary>
		/// Prints the ASCII values of every character in a character array delimited by a pipe character to the Console.
		/// </summary>
		/// <param name="inputByteArr">String to be split.</param>
		public static void PrintByteArrASCII(byte[] inputByteArr) {
			char[] tempChar = Encoding.ASCII.GetChars(inputByteArr);
			Console.Write("|");
			for (int i = 0; i < tempChar.Length; i++) {
				Console.Write(((int)tempChar[i]).ToString() + "|");
			}
			Console.WriteLine();
		}

		public static string ByteArrToASCII(byte[] inputByteArr) {
			StringBuilder tempSB = new StringBuilder();
			char[] tempChar = Encoding.ASCII.GetChars(inputByteArr);
			tempSB.Append("|");
			for (int i = 0; i < tempChar.Length; i++) {
				tempSB.Append(((int)tempChar[i]).ToString() + "|");
			}
			return tempSB.ToString();
		}

		/// <summary>
		/// Removes all newline characters from a string (ASCII 10/13).
		/// </summary>
		/// <param name="inputStr">String to remove characters from.</param>
		/// <returns>String lacking ASCII 10/13.</returns>
		public static String RemoveNewLineChars(String inputStr) {
			if (inputStr != null) {
				return inputStr.Replace(Convert.ToChar(13).ToString(), String.Empty).Replace(Convert.ToChar(10).ToString(), String.Empty);
			}
			return null;
		}

		/// <summary>
		/// Returns a string containing the inputStrArr String Array with elemented delimited by Delimiter.
		/// </summary>
		/// <param name="inputStrArr">String Array to enumerate.</param>
		/// <param name="delimiter">Delimiter to interleave between elements.</param>
		/// <returns>String delimited by Delimiter.</returns>
		public static String StringArrToDelimitedStr(String[] inputStrArr, String delimiter) {
			String returnStr = String.Empty;
			foreach (String tempStr in inputStrArr) {
				returnStr += tempStr + delimiter;
			}
			if (returnStr.Length > delimiter.Length) {
				returnStr = returnStr.Substring(0, returnStr.Length - delimiter.Length);
			}
			return returnStr;
		}

		/// <summary>
		/// Converts bytes into a human readable format rounded to an accuracy.
		/// </summary>
		/// <param name="bytes">Number of bytes to be converted.</param>
		/// <param name="accuracy">Decimal accuracy. Use 0 for no decimal point.</param>
		/// <returns>String in the format of [Number][Unit]</returns>
		public static String BytesToHumanReadable(long bytes, int accuracy) {
			if (bytes < 1073741824) {
				return Math.Round(bytes / 1048576.0, accuracy).ToString("0.0") + "MB";
			} else if (bytes < 1099511627776) {
				return Math.Round(bytes / 1073741824.0, accuracy).ToString("0.0") + "GB";
			} else {
				return Math.Round(bytes / 1099511627776.0, accuracy).ToString("0.0") + "TB";
			}
		}

		/// <summary>
		/// Converts milliseconds to a clock-like format.
		/// </summary>
		/// <param name="milliseconds">Number of milliseconds to convert</param>
		/// <returns>Returns a hh:mm:ss formated string.</returns>
		public static String MillisecondsToClockFormat(double milliseconds) {
			TimeSpan tempTS = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(milliseconds));
			if (tempTS.Hours > 0) {
				return tempTS.Hours.ToString() + ":" + tempTS.Minutes.ToString("00") + ":" + tempTS.Seconds.ToString("00");
			} else {
				return tempTS.Minutes.ToString() + ":" + tempTS.Seconds.ToString("00");
			}
		}

		/// <summary>
		/// Converts milliseconds to a human readable format.
		/// </summary>
		/// <param name="milliseconds">Number of milliseconds to convert</param>
		/// <returns>Returns a #d #h #m #s string.</returns>
		public static String MillisecondsToHumanReadable(double milliseconds) {
			TimeSpan tempTS;
			if (milliseconds > Int32.MaxValue) {
				tempTS = new TimeSpan(0, 0, 0, Convert.ToInt32((milliseconds - Int32.MaxValue) / 1000), Int32.MaxValue);
			} else {
				tempTS = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(milliseconds));
			}
			if (tempTS.Days > 0) {
				return tempTS.Days + "d " + tempTS.Hours.ToString() + "h "
					+ tempTS.Minutes.ToString() + "m " + tempTS.Seconds.ToString() + "s";
			} else if (tempTS.Hours > 0) {
				return tempTS.Hours.ToString() + "h " + tempTS.Minutes.ToString() + "m " + tempTS.Seconds.ToString() + "s";
			} else if (tempTS.Minutes > 0) {
				return tempTS.Minutes.ToString() + "m " + tempTS.Seconds.ToString() + "s";
			} else {
				return tempTS.Seconds.ToString() + "s";
			}
		}
	}
}
