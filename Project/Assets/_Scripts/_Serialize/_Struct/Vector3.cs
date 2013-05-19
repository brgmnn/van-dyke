using System.Text;
using UnityEngine;

namespace Serialize {
namespace Struct {
	public static class Vector3 {
		public static string serialize(UnityEngine.Vector3 uv3) {
			return "{\"x\":"+uv3.x+",\"y\":"+uv3.y+",\"z\":"+uv3.z+"}";
		}
		
		public static string readable_serialize(UnityEngine.Vector3 uv3, int indentation_level) {
			string ser = "{\"x\": "+uv3.x+", \"y\": "+uv3.y+", \"z\": "+uv3.z+"}";
			ser.PadLeft(indentation_level, ' ');
			return ser;
		}
		
		public static UnityEngine.Vector3 deserialize(string str) {
			return new UnityEngine.Vector3();
		}
	}
}
}
