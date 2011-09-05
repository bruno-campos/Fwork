using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Fwork.Util {
	
	public class FileWriter {
		
		private string filepath;
		private bool append = true;
		
		public FileWriter(string filepath) {
			this.filepath = filepath;
		}
		public FileWriter(string filepath, bool append) {
			this.filepath = filepath;
			this.append = append;
		}
		
		public void Write(string str) {
			StreamWriter sw = new StreamWriter(this.filepath, this.append);
			try {
				sw.Write(str);
			} finally {
				sw.Flush();
				sw.Close();
				sw.Dispose();
			}
		}
		
		public void WriteLine(string str) {
			StreamWriter sw = new StreamWriter(this.filepath, this.append);
			try {
				sw.WriteLine(str);
			} finally {
				sw.Flush();
				sw.Close();
				sw.Dispose();
			}
		}
		
	}
	
}
