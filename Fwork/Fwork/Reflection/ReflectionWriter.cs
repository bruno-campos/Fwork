using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fwork.Reflection {
	
	public class ReflectionWriter {
	
		private string[] namespaces, assemblies;
		
		public ReflectionWriter() {
		}
		public ReflectionWriter(string[] namespaces, string[] assemblies) {
			this.namespaces = namespaces;
			this.assemblies = assemblies;
		}
		
		public void Write() {
			ServiceWriter SW = new ServiceWriter(this.namespaces, this.assemblies);
			SW.CreateFiles();
			JsWriter JW = new JsWriter(this.namespaces, this.assemblies);
			JW.CreateFile();
		}
		
	}
	
}
