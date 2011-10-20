using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fwork.Util {
	
	public class FileConcat {
		
		private List<string> files;
		
		public FileConcat() {
			this.files = new List<string>();
		}
		
		public void AddFile(string filename) {
			this.files.Add(filename);
		}
		
		public void RemoveFile(string filename) {
			this.files.Remove(filename);
		}
		
		public void CreateFile(string filename) {
			string filecode = "";
			foreach(string file in this.files) {
				filecode += this.getCode(file) + "\r\n";
			}
			FileWriter FW = new FileWriter(filename);
			try {
				FW.Write(filecode);
			} finally {
				FW = null;
			}
		}
		
		private string getCode(string filename) {
			string code;
			FileReader FR = new FileReader(filename);
			try {
				code = FR.Read();
				return code;
			} finally {
				FR = null;
			}
		}
		
	}
	
}
