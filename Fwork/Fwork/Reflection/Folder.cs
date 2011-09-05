using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Fwork.Reflection {
	
	internal class Folders {
		
		internal static string Check(string folder_path) {
			if(!Directory.Exists(folder_path)) {
				Directory.CreateDirectory(folder_path);
			}
			return folder_path;
		}
		
	}
	
}
