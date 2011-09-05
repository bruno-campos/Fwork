using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Configuration;

namespace Fwork.Xsl {
	
	public class XslCompiler {
		
		private string templatesPath = "";
		private string templatesCompPath = "";
		
		public XslCompiler() {
			this.templatesPath = ConfigurationSettings.AppSettings["Fwork:folder__xsl_templates"].ToString();
			this.templatesCompPath = ConfigurationSettings.AppSettings["Fwork:folder__xsl_compiled"].ToString();
		}
		public XslCompiler(string templatesPath, string templatesCompPath) {
			this.templatesPath = templatesPath;
			this.templatesCompPath = templatesCompPath;
		}
		
		public void Compile() {
			//Checking output directory
			if(!Directory.Exists(templatesCompPath)) {
				Directory.CreateDirectory(templatesCompPath);
			}
			//Compiling templates
			this.compileDir(this.templatesPath);
			foreach(string dir in Directory.GetDirectories(this.templatesPath)) {
				this.compileDir(dir);
			}
		}
		
		private void compileDir(string dir) {
			foreach(string file in Directory.GetFiles(dir)) {
				this.compileFile(file);
			}
		}
		
		private void compileFile(string file) {
			string fileFolder = file.Substring(file.IndexOf(this.templatesPath) + this.templatesPath.Length, (file.IndexOf(Path.GetFileName(file))) - (file.IndexOf(this.templatesPath) + this.templatesPath.Length));
			string dir = this.templatesCompPath + fileFolder;
			string compiledOutput = dir + Path.GetFileName(file);
			//Checking output directory
			if(!Directory.Exists(dir)) {
				Directory.CreateDirectory(dir);
			}
			//Writing compiled file
			StreamWriter sw = new StreamWriter(compiledOutput, false);
			try {
				string newfilestr = "";
				newfilestr += "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n";
				newfilestr += "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">";
				newfilestr += this.getFileStr(file);
				newfilestr += "</xsl:stylesheet>";
				sw.Write(newfilestr);
				newfilestr = null;
			} finally {
				sw.Close();
				sw.Dispose();
			}
		}
		
		private string getFileStr(string file) {
			StreamReader sr = new StreamReader(file);
			try {
				string filestr = sr.ReadToEnd();
				//Removing header
				filestr = filestr.Substring(filestr.IndexOf("xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">") + "xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">".Length);
				//Writing includes
				List<string> filesToParse = this.parseIncluded(filestr);
				foreach(string includedfile in filesToParse) {
					string compiledInclude = this.getFileStr(HttpContext.Current.Server.MapPath(includedfile));
					filestr = filestr.Replace("<xsl:include href=\"" + includedfile + "\"></xsl:include>", compiledInclude);
				}
				//Removing footer
				filestr = filestr.Replace("</xsl:stylesheet>", "");
				return filestr;
			} finally {
				sr.Close();
				sr.Dispose();
			}
		}
		
		private List<string> parseIncluded(string str) {
			List<string> files = new List<string>();
			while(str.Contains("<xsl:include href=\"")) {
				files.Add(str.Substring(str.IndexOf("<xsl:include href=\"") + "<xsl:include href=\"".Length, str.IndexOf("\"></xsl:include>") - (str.IndexOf("<xsl:include href=\"") + "<xsl:include href=\"".Length)));
				str = str.Substring(str.IndexOf("\"></xsl:include>") + "\"></xsl:include>".Length);
			}
			return files;
		}
		
	}
	
}