using System;
using System.IO;

namespace Serialize {
	public static class File {
		// write a string to a file
		public static void write_string_to_file(string path, string str) {
			StreamWriter sw = new StreamWriter(path);
			sw.Write(str);
			sw.Close();
		}
		
		// read a file into a string
		public static string read_file_as_string(string path) {
			try {
	            using (StreamReader sr = new StreamReader(path)) {
	                string contents = sr.ReadToEnd();
	                return contents;
	            }
	        } catch (Exception e) {
				return "Error: The file could not be read:"+e.Message;
	        }
		}
	}
}
