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
	public string phase = "";

	[Type(2, "int16")]
	public short playerTurn = 0;

	[Type(3, "int16")]
	public short winningPlayer = 0;

	[Type(4, "array", "string")]
	public ArraySchema<string> cards = new ArraySchema<string>();

	[Type(5, "int16")]
	public short playersSkipped = 0;

	[Type(6, "array", "string")]
	public ArraySchema<string> recruitsToDestroy = new ArraySchema<string>();

	[Type(7, "boolean")]
	public bool firstTurn = true;
}

