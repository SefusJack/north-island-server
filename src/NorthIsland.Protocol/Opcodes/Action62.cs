namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 62: house furniture, storage, shops and house fun items.</summary>
public static class Action62
{
    /// <summary>Action id of this packet group, 62.</summary>
    public const byte Code = 62;

    /// <summary>Server to client: a furniture piece placed in the house; client to server: the request to place a piece.</summary>
    public const byte Place = 1;

    /// <summary>Client to server: takes a furniture piece back; server to client: a piece was removed.</summary>
    public const byte TakeBack = 2;

    /// <summary>Client to server: moves a furniture piece; server to client: the piece's new position.</summary>
    public const byte Move = 3;

    /// <summary>Client to server: puts a tool on a work platform.</summary>
    public const byte PlaceTool = 4;

    /// <summary>Client to server: recycles a bag item.</summary>
    public const byte Recycle = 5;

    /// <summary>Client to server: deposits gas into the big gas storage.</summary>
    public const byte GasDeposit = 62;

    /// <summary>Client to server: deposits gas into the small gas storage.</summary>
    public const byte SmallGasDeposit = 63;

    /// <summary>Client to server: the fish urn was clicked.</summary>
    public const byte FishUrnClick = 56;

    /// <summary>Client to server: a picture frame was clicked.</summary>
    public const byte PortraitClick = 66;

    /// <summary>Server to client: the picture frame portrait data.</summary>
    public const byte PortraitReply = 62;

    /// <summary>Client to server: the New Year Cracker was clicked.</summary>
    public const byte CrackerClick = 64;

    /// <summary>Client to server: the clock was clicked.</summary>
    public const byte ClockClick = 65;

    /// <summary>Server to client: plays the New Year Cracker on a floor and piece.</summary>
    public const byte CrackerPlay = 60;

    /// <summary>Server to client: the clock chimes, or reports the game time.</summary>
    public const byte ClockChime = 61;

    /// <summary>Server to client: the elevator stops its animation.</summary>
    public const byte LiftStop = 255;

    /// <summary>Client to server: stairs, an elevator or a rented door was clicked.</summary>
    public const byte StairsClick = 14;

    /// <summary>Server to client: a piece goes to or leaves the pick-up area.</summary>
    public const byte Retain = 16;

    /// <summary>Server to client: a pick-up piece now stands in the house.</summary>
    public const byte Placed = 17;

    /// <summary>Server to client: a new piece arrives in the pick-up area.</summary>
    public const byte Delivered = 52;

    /// <summary>Server to client: a furniture rent notice or refusal.</summary>
    public const byte RentNotice = 56;

    /// <summary>Client to server: a bag item is dropped on the house floor.</summary>
    public const byte FloorDrop = 10;

    /// <summary>Client to server: a bag item is dropped on the house wall.</summary>
    public const byte WallDrop = 11;

    /// <summary>Server to client: the house floor item.</summary>
    public const byte FloorSet = 14;

    /// <summary>Server to client: the house wall item.</summary>
    public const byte WallSet = 15;

    /// <summary>Client to server: puts bag items into the storeroom (same value as <c>StoreList</c>).</summary>
    public const byte StorePut = 7;

    /// <summary>Server to client: the storeroom contents (same value as <c>StorePut</c>).</summary>
    public const byte StoreList = 7;

    /// <summary>Client to server: moves items inside the storeroom.</summary>
    public const byte StoreMove = 8;

    /// <summary>Client to server: takes items out of the storeroom.</summary>
    public const byte StoreTake = 9;

    /// <summary>Server to client: items added to a storeroom cell.</summary>
    public const byte StoreAdd = 11;

    /// <summary>Server to client: items removed from a storeroom cell.</summary>
    public const byte StoreRemove = 13;

    /// <summary>Client to server: the super handbag was double-clicked; server to client: opens the storeroom form.</summary>
    public const byte Handbag = 50;

    /// <summary>Client to server: puts a vehicle into a garage cell; server to client: the cell's vehicle.</summary>
    public const byte GaragePut = 47;

    /// <summary>Client to server: takes a vehicle out of a garage cell; server to client: the cell is emptied.</summary>
    public const byte GarageTake = 48;

    /// <summary>Client to server: puts a garage vehicle on show or takes it down.</summary>
    public const byte PlatformSet = 49;

    /// <summary>Client to server: opens a garage; server to client: the garage form.</summary>
    public const byte GarageOpen = 51;

    /// <summary>Client to server: starts a game on a TV console (same value as <c>PlatformForm</c>).</summary>
    public const byte TvGame = 54;

    /// <summary>Server to client: shows the show platform form (same value as <c>TvGame</c>).</summary>
    public const byte PlatformForm = 54;

    /// <summary>Client to server: opens a show platform (same value as <c>PlatformShown</c>).</summary>
    public const byte PlatformOpen = 55;

    /// <summary>Server to client: the vehicle shown on a platform (same value as <c>PlatformOpen</c>).</summary>
    public const byte PlatformShown = 55;

    /// <summary>Stall case: orders from the client (open, add, take, price) and case updates from the server.</summary>
    public const byte CaseOwner = 58;

    /// <summary>Client to server: buys from a stall case.</summary>
    public const byte CaseBuy = 59;

    /// <summary>Client to server: puts an item on a shelf for sale; server to client: the shelf offer.</summary>
    public const byte ShelfPut = 21;

    /// <summary>Client to server: stops a shelf sale; server to client: the offer is removed.</summary>
    public const byte ShelfStop = 22;

    /// <summary>Client to server: buys from a shelf.</summary>
    public const byte ShelfBuy = 23;

    /// <summary>Client to server: puts an item into a vending machine (same value as <c>ShelfSold</c>).</summary>
    public const byte VendingPut = 24;

    /// <summary>Server to client: a shelf offer was sold (same value as <c>VendingPut</c>).</summary>
    public const byte ShelfSold = 24;

    /// <summary>Client to server: stops a vending machine sale.</summary>
    public const byte VendingStop = 25;

    /// <summary>Client to server: buys from a vending machine.</summary>
    public const byte VendingBuy = 26;

    /// <summary>Client to server: puts an egg into the egg machine; server to client: the cell content.</summary>
    public const byte EggPut = 31;

    /// <summary>Client to server: sets the egg machine price; server to client: the price.</summary>
    public const byte EggPrice = 32;

    /// <summary>Client to server: plays the egg machine (same value as <c>EggPlayed</c>).</summary>
    public const byte EggPlay = 33;

    /// <summary>Server to client: the egg machine cell that was played (same value as <c>EggPlay</c>).</summary>
    public const byte EggPlayed = 33;

    /// <summary>Client to server: feeds a token into the egg machine; server to client: the egg count.</summary>
    public const byte EggFeed = 34;

    /// <summary>Client to server: takes an egg back from the egg machine.</summary>
    public const byte EggRetake = 35;

    /// <summary>Client to server: opens the egg machine.</summary>
    public const byte EggOpen = 36;

    /// <summary>Server to client: a shop or egg machine action was refused, with a reason code.</summary>
    public const byte VendRefused = 25;

    /// <summary>Client to server: puts a pet to bed; server to client: the stored pet of a bed.</summary>
    public const byte BedStore = 43;

    /// <summary>Client to server: takes a pet from a bed; server to client: the pet leaves the bed.</summary>
    public const byte BedTake = 44;

    /// <summary>Client to server: stores a bag item in a cupboard; server to client: the cupboard slot content.</summary>
    public const byte CupboardStore = 41;

    /// <summary>Client to server: takes an item from a cupboard; server to client: the slot is emptied.</summary>
    public const byte CupboardTake = 42;

    /// <summary>Server to client: clears a player's doorplate.</summary>
    public const byte DoorplateClear = 45;

    /// <summary>Client to server: sets the doorplate; server to client: a player's doorplate text.</summary>
    public const byte Doorplate = 46;

    /// <summary>Client to server: a piano key (note 1 to 7) was played.</summary>
    public const byte PianoKey = 57;

    /// <summary>Client to server: puts items into or takes them out of a house box.</summary>
    public const byte BoxItems = 52;

    /// <summary>Server to client: a house box slot now holds an item or is empty.</summary>
    public const byte BoxUpdate = 57;

    /// <summary>Server to client: the house music to play.</summary>
    public const byte HouseMusic = 59;

    /// <summary>Client to server: the song chosen at the music center.</summary>
    public const byte MusicCenter = 61;

    /// <summary>Client to server: uses the magic sandglass; server to client: its state.</summary>
    public const byte Sandglass = 53;

    /// <summary>Client to server: recycles a placed furniture piece.</summary>
    public const byte RecyclePiece = 6;

    /// <summary>Server to client: the furniture list of a house.</summary>
    public const byte List = 4;

    /// <summary>Server to client: a tool is linked to its parent piece.</summary>
    public const byte ToolLinked = 5;
}
