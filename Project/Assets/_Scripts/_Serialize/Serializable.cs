namespace Serialize {
	public abstract class Serializable {
		public abstract string serialize();
		public abstract void deserialize(string str);
	}
}
