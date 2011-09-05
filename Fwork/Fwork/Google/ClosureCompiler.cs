using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;
using Fwork.Util;

namespace Fwork.Google {
	
	public struct GCSLinks {
		public const string compilerLink = "http://closure-compiler.appspot.com/compile";
	}
	
	public struct GCSCompilationOptions {
		public const string WHITESPACE_ONLY = "WHITESPACE_ONLY";
		public const string SIMPLE_OPTIMIZATIONS = "SIMPLE_OPTIMIZATIONS";
		public const string ADVANCED_OPTIMIZATIONS = "ADVANCED_OPTIMIZATIONS";
	}
	
	public struct GCSOutputInfo {
		public const string compiled_code = "compiled_code";
		public const string warnings = "warnings";
		public const string errors = "errors";
		public const string statistics = "statistics";
	}
	
	public class ClosureCompiler {

		private string compilation_level = GCSCompilationOptions.SIMPLE_OPTIMIZATIONS;
		private List<string> code_url;
		
		public ClosureCompiler() {
			this.code_url = new List<string>();
		}
		
		public void AddCodeUrl(string url) {
			if(!this.code_url.Contains(url)) {
				this.code_url.Add(url);
			}
		}
		
		public string GetScript() {
			string output_info = GCSOutputInfo.compiled_code;
			string output_format = "text";
			string data = "compilation_level=%clevel&output_format=%oformat&output_info=%oinfo";
			data = data.Replace("%clevel", this.compilation_level).Replace("%oformat", output_format).Replace("%oinfo", output_info);
			if(this.code_url.Count > 0) {
				foreach(string curl in this.code_url) {
					data += "&code_url=" + curl;
				}
			}
			WebRequest request = WebRequest.Create(GCSLinks.compilerLink);
			byte[] byteArray = Encoding.UTF8.GetBytes(data);
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = byteArray.Length;
			Stream dataStream = request.GetRequestStream();
			try {
				//Writing data
				dataStream.Write(byteArray, 0, byteArray.Length);
				dataStream.Close();
				//Getting server response
				WebResponse response = request.GetResponse();
				dataStream = response.GetResponseStream();
				StreamReader reader = new StreamReader(dataStream);
				try {
					string responseFromServer = reader.ReadToEnd();
					return responseFromServer;
				} finally {
					reader.Close();
				}
			} finally {
				dataStream.Close();
			}
		}
		
		public void CreateScript(string path) {
			FileWriter F = new FileWriter(path, false);
			try {
				string source = this.GetScript();
				F.Write(source);
			} finally {
				F = null;
			}
		}
		
	}
}
