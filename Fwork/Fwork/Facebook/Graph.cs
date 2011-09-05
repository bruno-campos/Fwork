using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace Fwork.Facebook {
	
	public struct FBGraphSearchLink {
		public const string Token = "https://graph.facebook.com/oauth/access_token?client_id=%appid&client_secret=%appsecret&code=%code";
		public const string Friends = "https://graph.facebook.com/me/friends?";
		public const string News = "https://graph.facebook.com/me/home?";
		public const string Wall = "https://graph.facebook.com/me/feed?";
		public const string Likes = "https://graph.facebook.com/me/likes?";
		public const string Movies = "https://graph.facebook.com/me/movies?";
		public const string Music = "https://graph.facebook.com/me/music?";
		public const string Books = "https://graph.facebook.com/me/books?";
		public const string Notes = "https://graph.facebook.com/me/notes?";
		public const string Permissions = "https://graph.facebook.com/me/permissions?";
		public const string PhotoTags = "https://graph.facebook.com/me/photos?";
		public const string PhotoAlbums = "https://graph.facebook.com/me/albums?";
		public const string VideoTags = "https://graph.facebook.com/me/videos?";
		public const string VideoUploads = "https://graph.facebook.com/me/videos/uploaded?";
		public const string Events = "https://graph.facebook.com/me/events?";
		public const string Groups = "https://graph.facebook.com/me/groups?";
		public const string Checkins = "https://graph.facebook.com/me/checkins?";
	}
	
	public struct FBGraphPublishLink {
		public const string Feed = "https://graph.facebook.com/%id/feed";
		public const string Comments = "https://graph.facebook.com/%id/comments";
		public const string Likes = "https://graph.facebook.com/%id/likes";
		public const string Notes = "https://graph.facebook.com/%id/notes";
		public const string Links = "https://graph.facebook.com/%id/links";
		public const string Events = "https://graph.facebook.com/%id/events";
		public const string Attending = "https://graph.facebook.com/%id/attending";
		public const string Maybe = "https://graph.facebook.com/%id/maybe";
		public const string Declined = "https://graph.facebook.com/%id/declined";
		public const string Albums = "https://graph.facebook.com/%id/albums";
		public const string Photos = "https://graph.facebook.com/%id/photos";
		public const string Checkins = "https://graph.facebook.com/%id/videos/checkins";
	}
	
	public class Graph {
		
		private string access_token;
		
		public Graph() { }
		
		public Graph(string acess_token) {
			this.access_token = acess_token;
		}
		
		// TOKEN FUNCTIONS
		
		public string GetToken(string appid, string appsecret, string code) {
			string url = FBGraphSearchLink.Token.Replace("%appid", appid).Replace("%appsecret", appsecret).Replace("%code", code);
			this.access_token = this.getData(url);
			return this.access_token;
		}
		
		//SEARCH FUNCTIONS
		
		public XmlNode GetNode(string url, string node_name) {
			return this.getNode(this.getData(this.getLink(url)), node_name);
		}
		
		public string GetData(string url) {
			return this.getData(this.getLink(url));
		}
		
		private string getLink(string url) {
			return url + this.access_token;
		}
		
		private XmlNode getNode(string json, string node_name) {
			return JsonConvert.DeserializeXmlNode(json, node_name);
		}
		
		private string getData(string url) {
			string sContents = string.Empty;
			System.Net.WebClient wc = new System.Net.WebClient();
			try {
				byte[] response = wc.DownloadData(url);
				sContents = System.Text.Encoding.ASCII.GetString(response);
				return sContents;
			} catch { 
				throw new Exception("Unable to connect");
			} finally {
				wc.Dispose();
				wc = null;
			}
		}
		
		// PUBLISH FUNCTIONS
		
		public string Publish(string link, string id, string data) {
			link = link.Replace("%id", id);
			data += "&access_token=" + this.access_token;
			WebRequest request = WebRequest.Create(link);
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
		
	}
	
}