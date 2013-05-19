namespace Serialize {
	public abstract class ReadableSerialize : Serializable {
		internal abstract string readable_serialize(int indentation_level);
		
		public string readable_serialize() {
			return readable_serialize(0);
		}
	}
}
