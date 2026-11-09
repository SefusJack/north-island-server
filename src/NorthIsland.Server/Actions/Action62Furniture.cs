using NorthIsland.Protocol.Messages.World;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 62: house furniture.</summary>
/// <param name="bed">The <c>BedHandlers</c> that serve the packets of this action.</param>
/// <param name="eggMachine">The <c>EggMachineHandlers</c> that serve the packets of this action.</param>
/// <param name="fishTank">The <c>FishTankHandlers</c> that serve the packets of this action.</param>
/// <param name="gas">The <c>GasHandlers</c> that serve the packets of this action.</param>
/// <param name="homeShop">The <c>HomeShopHandlers</c> that serve the packets of this action.</param>
/// <param name="houseDecor">The <c>HouseDecorHandlers</c> that serve the packets of this action.</param>
/// <param name="houseFun">The <c>HouseFunHandlers</c> that serve the packets of this action.</param>
/// <param name="houseFurniture">The <c>HouseFurnitureHandlers</c> that serve the packets of this action.</param>
/// <param name="houseRoom">The <c>HouseRoomHandlers</c> that serve the packets of this action.</param>
/// <param name="manufacture">The <c>ManufactureHandlers</c> that serve the packets of this action.</param>
/// <param name="storeroom">The <c>StoreroomHandlers</c> that serve the packets of this action.</param>
/// <param name="tent">The <c>TentHandlers</c> that serve the packets of this action.</param>
/// <param name="gobang">The <c>GobangHandlers</c> that serve the packets of this action.</param>
/// <param name="stallCase">The <c>StallCaseHandlers</c> that serve the packets of this action.</param>
/// <param name="garage">The <c>GarageHandlers</c> that serve the packets of this action.</param>
public sealed class Action62Furniture(
    BedHandlers bed,
    EggMachineHandlers eggMachine,
    FishTankHandlers fishTank,
    GasHandlers gas,
    HomeShopHandlers homeShop,
    HouseDecorHandlers houseDecor,
    HouseFunHandlers houseFun,
    HouseFurnitureHandlers houseFurniture,
    HouseRoomHandlers houseRoom,
    ManufactureHandlers manufacture,
    StoreroomHandlers storeroom,
    TentHandlers tent,
    GobangHandlers gobang,
    StallCaseHandlers stallCase,
    GarageHandlers garage
) : IAction
{
    public byte Code => Action62.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action62.Place, "place furniture", tent.OnPlace) // 62/1: put a bag item down as furniture in their own house
        .On(Action62.TakeBack, "take back furniture", tent.OnTakeBack) // 62/2: take a placed furniture piece back into the bag
        .On(Action62.Move, "move furniture", tent.OnMove) // 62/3: move a placed furniture piece, and its tools, to a new spot
        .On(Action62.PlaceTool, "put a tool on a work platform", tent.OnPlaceTool) // 62/4: put a tool from the bag on a work platform
        .On(Action62.Recycle, "recycle an item", tent.OnRecycle) // 62/5: recycle a bag item at a recycling bin
        .On(Action62.RecyclePiece, "recycle a furniture piece", tent.OnRecyclePiece) // 62/6: recycle a placed furniture piece, its product goes to the bag
        .On(Action62.StorePut, "storeroom put", (s, b) => storeroom.Order(s, b, 7)) // 62/7: put a bag stack into the storeroom
        .On(Action62.StoreMove, "storeroom move", (s, b) => storeroom.Order(s, b, 8)) // 62/8: move a stack inside the storeroom
        .On(Action62.StoreTake, "storeroom take", (s, b) => storeroom.Order(s, b, 9)) // 62/9: take a stack from the storeroom into the bag
        .On(Action62.FloorDrop, "house floor", (s, b) => houseDecor.Drop(s, b, floor: true)) // 62/10: lay a bag item as the house floor
        .On(Action62.WallDrop, "house wallpaper", (s, b) => houseDecor.Drop(s, b, floor: false)) // 62/11: lay a bag item as the house wallpaper
        .On(Action62.StairsClick, "stairs or door click", houseRoom.OnClick) // 62/14: click stairs, elevator or rented door to change floor or room
        .On(Action62.ShelfPut, "shelf put", (s, b) => homeShop.OnPut(s, b, vending: false)) // 62/21: put a bag stack on a shelf for sale at a unit price
        .On(Action62.ShelfStop, "shelf stop", (s, b) => homeShop.OnStop(s, b, vending: false)) // 62/22: take a shelf offer down
        .On(Action62.ShelfBuy, "shelf buy", (s, b) => homeShop.OnBuy(s, b, vending: false)) // 62/23: buy from a shelf offer, the owner is paid at once
        .On(Action62.VendingPut, "vending put", (s, b) => homeShop.OnPut(s, b, vending: true)) // 62/24: put a bag stack in a vending machine for sale
        .On(Action62.VendingStop, "vending stop", (s, b) => homeShop.OnStop(s, b, vending: true)) // 62/25: take a vending machine offer down
        .On(Action62.VendingBuy, "vending buy", (s, b) => homeShop.OnBuy(s, b, vending: true)) // 62/26: buy from a vending machine offer
        .On(Action62.EggPut, "egg machine put", eggMachine.OnPut) // 62/31: owner puts a bag stack in an egg machine cell
        .On(Action62.EggPrice, "egg machine price", eggMachine.OnPrice) // 62/32: owner sets the price of an egg machine
        .On(Action62.EggPlay, "egg machine play", eggMachine.OnPlay) // 62/33: visitor pays and gets an item from a random filled cell
        .On(Action62.EggFeed, "egg machine token", eggMachine.OnFeed) // 62/34: owner puts a token egg in the first machine with room
        .On(Action62.EggRetake, "egg machine retake", eggMachine.OnRetake) // 62/35: owner takes the items back out of an egg machine
        .On(Action62.EggOpen, "egg machine open", eggMachine.OnOpen) // 62/36: open the buy form of an egg machine
        .On(Action62.CupboardStore, "store in cupboard", houseFurniture.OnStore) // 62/41: owner puts a bag stack in a cupboard slot
        .On(Action62.CupboardTake, "take from cupboard", houseFurniture.OnTake) // 62/42: owner takes a cupboard slot back into the bag
        .On(Action62.BedStore, "pet to bed", bed.OnStore) // 62/43: owner puts a pet in a bed
        .On(Action62.BedTake, "pet from bed", bed.OnTake) // 62/44: owner takes a pet out of a bed
        .On(Action62.Doorplate, "doorplate", houseFurniture.OnDoorplate) // 62/46: owner sets or clears the doorplate text
        .On(Action62.GaragePut, "garage put", garage.OnPut) // 62/47: owner puts a vehicle from the bag in a garage cell
        .On(Action62.GarageTake, "garage take", garage.OnTake) // 62/48: owner takes a garage vehicle back into the bag
        .On(Action62.PlatformSet, "show platform set", garage.OnSet) // 62/49: put a garage vehicle on a show platform or take it down
        .On(Action62.Handbag, "super handbag", StoreroomHandlers.OnHandbag) // 62/50: put, move or take stacks in a super handbag
        .On(Action62.GarageOpen, "garage open", garage.OnOpenGarage) // 62/51: ask to open a garage
        .On(Action62.BoxItems, "house box", houseFurniture.OnBox) // 62/52: owner puts a stack into, or takes one out of, a house box
        .On(Action62.Sandglass, "magic sandglass", manufacture.OnSandglass) // 62/53: use the magic sandglass
        .On(Action62.TvGame, "tv game", gobang.OnTvGame) // 62/54: start the TV game hall from the console
        .On(Action62.PlatformOpen, "show platform open", garage.OnOpenPlatform) // 62/55: ask to open a show platform
        .On(Action62.FishUrnClick, "fish urn", fishTank.OnClick) // 62/56: show the fish tank to its owner or a visitor
        .On(Action62.PianoKey, "piano key", houseFurniture.OnPianoKey) // 62/57: play a piano note for the others in the house
        .On(Action62.CaseOwner, "stall case", stallCase.OnOwner) // 62/58: owner commands on a stall case: open, add, take, reprice
        .On(Action62.CaseBuy, "stall case buy", stallCase.OnBuy) // 62/59: buy from a stall case
        .On(Action62.MusicCenter, "music center", houseFurniture.OnMusic) // 62/61: owner picks a song at the music center
        .On(Action62.GasDeposit, "big gas storage", (s, b) => gas.Deposit(s, b, 1)) // 62/62: move marked gas stacks into the big gas storage
        .On(Action62.SmallGasDeposit, "gas storage", (s, b) => gas.Deposit(s, b, 2)) // 62/63: move marked gas stacks into the small gas storage
        .On(Action62.CrackerClick, "new year cracker", (s, b) => houseFun.Click(s, b, HouseFunHandlers.Cracker)) // 62/64: click the new year cracker
        .On(Action62.ClockClick, "gugu clock", (s, b) => houseFun.Click(s, b, HouseFunHandlers.Clock)) // 62/65: click the gugu clock
        .On(Action62.PortraitClick, "picture frame", houseFun.OnPortrait) // 62/66: ask for the picture frame portrait
        .Clears(Action62.MusicCenter, () => [HouseLifeMessages.HouseMusic(0, "")], whenSilent: true); // 62/61: sends back an empty music center song
}
