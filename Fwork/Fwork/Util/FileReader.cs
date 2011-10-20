using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Fwork.Util {
	
	public class FileReader {
		
		private string filepath;
		
		public FileReader(string filepath) {
			this.filepath = filepath;
		}
		
		public string Read() {
			StreamReader sr = new StreamReader(this.filepath);
			try {
				string str = sr.ReadToEnd();
				return str;
			} finally {
				sr.Close();
				sr.Dispose();
			}
		}
		
	}
	
}
