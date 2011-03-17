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

		static public string RenderTextProgressBar(int progress, int total, int bars) {
			int c = (int)((float)progress / (float)total * (float)bars);

			return string.Format("[{0}{1}]",
				new string('=', c),
				new string(' ', bars - c));
		}

		static readonly char[] dancers = new char[] { '-', '/', '|', '\\' };
		static public char RenderTextDancer(int i, int stride) {
			return dancers[(i / stride) % dancers.Length];
		}

		public class ReservedLineConsole : TextWriter {
			private int reservedLines;

			public int ReservedLines {
				get { return reservedLines; }
			}

			public override Encoding Encoding {
				get { return Console.Out.Encoding; }
			}

			static public string EmptyLine {
				get { return new string(' ', Console.WindowWidth - 1); }
			}

			public ReservedLineConsole(int _reservedLines) {
				reservedLines = _reservedLines;
			}

			public void WriteReservedLine(int i, string s) {
				if(i < 0 || i >= ReservedLines)
					throw new ArgumentOutOfRangeException();

				int l = Console.CursorLeft;
				Console.CursorTop += i + 1;

				Console.CursorLeft = 0;
				Console.Write(EmptyLine);

				Console.CursorLeft = 0;
				Console.Write(s);

				Console.CursorTop -= i + 1;
				Console.CursorLeft = l;
			}

			protected void LineUp() {
				if(Console.CursorTop < (Console.BufferHeight - ReservedLines - 1)) {
					Console.MoveBufferArea(
						0, Console.CursorTop + 1,
						Console.BufferWidth, ReservedLines,
						0, Console.CursorTop + 2);

					Console.CursorTop++;
					Console.CursorLeft = 0;

					Console.Write(EmptyLine);
					Console.CursorLeft = 0;

					Console.CursorTop += ReservedLines;
					Console.CursorTop -= ReservedLines;
				}
				else {
					int stride = 100; //0x8000 / (Console.BufferWidth + 2);

					int ofs = Console.CursorTop % stride;

					Console.MoveBufferArea(
						0, 1,
						Console.BufferWidth, ofs,
						0, 0);

					for(int i = ofs; i < Console.CursorTop; i += stride)
						Console.MoveBufferArea(
							0, i + 1,
							Console.BufferWidth, stride,
							0, i);
				}
			}

			public override void Write(char value) {
				if(value == 10) {
					LineUp();
					return;
				}

				Console.Write(value);
			}
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
