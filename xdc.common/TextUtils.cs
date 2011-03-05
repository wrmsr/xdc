using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace xdc.common {
	static public class TextUtils {
		private enum FileType : uint {
			FileTypeChar = 0x0002,
			FileTypeDisk = 0x0001,
			FileTypePipe = 0x0003,
			FileTypeRemote = 0x8000,
			FileTypeUnknown = 0x0000,
		}

		[DllImport("kernel32.dll")]
		static private extern FileType GetFileType(IntPtr hFile);

		private const int STD_INPUT_HANDLE = -10;
		private const int STD_OUTPUT_HANDLE = -11;
		private const int STD_ERROR_HANDLE = -12;

		[DllImport("kernel32.dll", SetLastError = true)]
		static private extern IntPtr GetStdHandle(int nStdHandle);

		static public bool IsOutputRedirected() {
			FileType ft = GetFileType(GetStdHandle(STD_OUTPUT_HANDLE));
		
			return ft != FileType.FileTypeChar;
		}

		static public void DrawTextProgressBar(int progress, int total) {
			//draw empty progress bar
			Console.CursorLeft = 0;
			Console.Write("["); //start
			Console.CursorLeft = 32;
			Console.Write("]"); //end
			Console.CursorLeft = 1;
			float onechunk = 30.0f / total;

			//draw filled part
			int position = 1;
			for(int i = 0; i < onechunk * progress; i++) {
				Console.BackgroundColor = ConsoleColor.Gray;
				Console.CursorLeft = position++;
				Console.Write(" ");
			}

			//draw unfilled part
			for(int i = position; i <= 31; i++) {
				Console.BackgroundColor = ConsoleColor.Black;
				Console.CursorLeft = position++;
				Console.Write(" ");
			}

			//draw totals
			Console.CursorLeft = 35;
			Console.BackgroundColor = ConsoleColor.Black;
			Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
		}

		static public void ClearLine() {
			Console.CursorLeft = 0;
			Console.Write(new String(' ', Console.WindowWidth - 1));
			Console.CursorLeft = 0;
		}

		static public string Indent(string str, int ct) {
			return Indent(str, new string(' ', ct));
		}

		static public string Indent(string str, string i) {
			StringBuilder sb = new StringBuilder();

			int c = 0;
			using(StringReader sr = new StringReader(str))
			for(string line; (line = sr.ReadLine()) != null;) {
				if(c++ > 0)
					sb.AppendLine();

				if(!string.IsNullOrEmpty(line))
					sb.Append(i + line);
			}

			return sb.ToString();
		}
	}
}
