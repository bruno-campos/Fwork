using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web;

namespace Fwork.Database {
	
	public struct DbParamFlags {
		public static int Nullable = Convert.ToInt32(Math.Pow(2, 1));
	}
	
	public class DbCmd: IDisposable {
		
		private bool disposed = false;
		
		private SqlConnection conn;
		private CommandType type;
		private SqlCommand cmd;
		private string strCmd;
		
		public SqlCommand Cmd {
			get{return this.cmd;}
		}
		public SqlConnection Conn {
			get{return this.conn;}
			set{this.conn = value;}
		}
		
		public DbCmd() {
		}
		
		public DbCmd(string command) {
			this.conn = Db.Conn;
			this.strCmd = command;
			this.type = CommandType.StoredProcedure;
			this.cmd = new SqlCommand(command, this.conn);
			this.cmd.CommandType = this.type;
			this.cmd.CommandTimeout = 300;
		}
		
		public DbCmd(string command, CommandType type) {
			this.conn = Db.Conn;
			this.strCmd = command;
			this.type = type;
			this.cmd = new SqlCommand(command, this.conn);
			this.cmd.CommandType = this.type;
			this.cmd.CommandTimeout = 300;
		}
		
		public DbCmd(string command, SqlConnection conn) {
			this.conn = conn;
			this.strCmd = command;
			this.type = CommandType.StoredProcedure;
			this.cmd = new SqlCommand(command, this.conn);
			this.cmd.CommandType = this.type;
			this.cmd.CommandTimeout = 300;
		}
		
		public void AddParameter(SqlParameter param, object value) {
			this.cmd.Parameters.Add(param).Value = value;
		}
		
		public void AddParameter(string paramname, SqlDbType paramtype, int paramsize, object value) {
			this.cmd.Parameters.Add(new SqlParameter(paramname, paramtype, paramsize)).Value = value;
		}
		
		public void AddParameter(string paramname, SqlDbType paramtype, object value) {
			this.cmd.Parameters.Add(new SqlParameter(paramname, paramtype)).Value = value;
		}
		
		public void AddParameter(string paramname, object value, int flags) {
			if((flags & DbParamFlags.Nullable) >= DbParamFlags.Nullable) {
				if(value.ToString() == "" || value == null) {
					return;
				}
			}
			this.cmd.Parameters.Add(new SqlParameter(paramname, value));
		}
		
		public void AddParameter(string paramname, object value) {
			this.cmd.Parameters.Add(new SqlParameter(paramname, value));
		}
		
		public void SetPageInfo(int pag_number, int pag_size) {
			this.cmd.Parameters.Add(new SqlParameter("@pag_number", pag_number));
			this.cmd.Parameters.Add(new SqlParameter("@pag_size", pag_size));
		}
		public void SetPageInfo(int pag_number, int pag_size, string sort_column) {
			this.SetPageInfo(pag_number, pag_size);
			this.cmd.Parameters.Add(new SqlParameter("@sort_column", sort_column));
		}
		
		private void prepare() {
			Db.Open(this.conn);
		}
		
		public int ExecuteNonQuery() {
			this.prepare();
			return this.cmd.ExecuteNonQuery();
		}
		public SqlDataReader ExecuteReader() {
			this.prepare();
			return this.cmd.ExecuteReader();
		}
		public SqlDataAdapter ExecuteAdapter() {
			this.prepare();
			return new SqlDataAdapter(cmd);
		}
		public object ExecuteScalar() {
			this.prepare();
			return this.cmd.ExecuteScalar();
		}
		public XmlReader ExecuteXmlReader() {
			this.prepare();
			return this.cmd.ExecuteXmlReader();
		}
		public string ExecuteXmlString() {
			this.prepare();
			XmlDocument xmldoc = new XmlDocument();
			try {
				xmldoc.Load(this.cmd.ExecuteXmlReader());
				return xmldoc.OuterXml;
			} finally {
				xmldoc = null;
			}
		}
		
		
		#region Dispose functions
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		private void Dispose(bool disposing) {
			if(!this.disposed) {
				if(disposing) {
					this.Cmd.Dispose();
					Db.Close(this.conn);
				}
				// Note disposing has been done.
				disposed = true;
			}
		}
		~DbCmd() {
			Dispose(false);
		}
		#endregion
		
	}
	
}