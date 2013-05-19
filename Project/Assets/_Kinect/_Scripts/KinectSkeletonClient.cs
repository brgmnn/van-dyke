using UnityEngine;
using System.IO;
using System.Diagnostics;
using System.Text;

using System.Net;
using System.Net.Sockets;

public class KinectSkeletonClient {
	private static int port_base = 31000; 
	
	public string read_data(int player, int data_type) {
		int offset = data_type*2 - 2 + player;
		
		UdpClient listener = new UdpClient(port_base+offset);
		listener.Client.ReceiveTimeout = 50;
		IPEndPoint address = new IPEndPoint(IPAddress.Any, port_base+offset);
		byte[] recb;
		
		try {
			recb = listener.Receive( ref address );
			listener.Close();
			return Encoding.ASCII.GetString(recb);
		} catch ( SocketException e ) {
			e.ToString();
		}
		
		listener.Close();
		return "silent";
	}
}