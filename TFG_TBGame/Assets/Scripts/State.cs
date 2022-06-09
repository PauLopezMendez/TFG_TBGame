// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 1.0.34
// 

using Colyseus.Schema;

public partial class State : Schema {
	[Type(0, "map", typeof(MapSchema<Player>))]
	public MapSchema<Player> players = new MapSchema<Player>();

	[Type(1, "string")]
	public string phase = default(string);

	[Type(2, "int16")]
	public short playerTurn = default(short);

	[Type(3, "int16")]
	public short winningPlayer = default(short);

	[Type(4, "array", typeof(ArraySchema<bool>))]
	public ArraySchema<bool> shop = new ArraySchema<bool>();

	[Type(5, "int16")]
	public short playersSkipped = default(short);
}

