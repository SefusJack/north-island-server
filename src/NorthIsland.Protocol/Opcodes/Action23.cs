namespace NorthIsland.Protocol.Opcodes;

/// <summary>
/// Sub codes of action <c>23</c>: bag, equipment, ground items, stalls, warehouse, house life and the many item-use packets. The second packet
/// byte selects one.
/// </summary>
public static class Action23
{
    /// <summary>Server to client: asks the player to confirm destroying a bag item; the answer returns as <see cref="Discard"/>.</summary>
    public const byte DiscardConfirm = 212;

    /// <summary>The action byte <c>23</c> that starts every packet of this group.</summary>
    public const byte Code = 23;

    /// <summary>Client to server: teleports with a map item, naming the place, the bag slot and the item mode.</summary>
    public const byte MagicMapUse = 89;

    /// <summary>Server to client: the new wear value of one bag or equipment piece.</summary>
    public const byte WearGroup = 140;

    /// <summary>Server to client: unlocks the bag slot that the teleport form was holding.</summary>
    public const byte MagicMapRelease = 169;

    /// <summary>Client to server: uses a companion voucher or card from a bag slot.</summary>
    public const byte UseVoucher = 96;

    /// <summary>Client to server: gift card request; option 1 asks for the card lists, option 2 picks the gift.</summary>
    public const byte GiftCard = 133;

    /// <summary>Server to client: answers <see cref="GiftCard"/> with card lists, a "used" message or the order to open the form.</summary>
    public const byte GiftCardReply = 108;

    /// <summary>Client to server: opens a mall gift box from a bag slot.</summary>
    public const byte OpenGiftBox = 120;

    /// <summary>Client to server: the player sat at a piano, naming the furniture index.</summary>
    public const byte PianoSit = 99;

    /// <summary>Server to client: a note (1 to 7) another player played on the piano.</summary>
    public const byte PianoNote = 161;

    /// <summary>Client to server: uses the Stopwatch to pause the double experience, or continues it.</summary>
    public const byte Stopwatch = 110;

    /// <summary>Client to server: uses a Pet Lethe or Return Scroll to move one attribute point of a pet.</summary>
    public const byte PetAbilitySwap = 85;

    /// <summary>Server to client: announces the start or end of a seasonal event.</summary>
    public const byte EventNotice = 57;

    /// <summary>Server to client: state of the Stopwatch pause of the double experience, at login and after use or cancel.</summary>
    public const byte ExpPauseNotice = 204;

    /// <summary>Client to server: body shift requests: use a Riceball, ask the time left, switch the shift on or off.</summary>
    public const byte BodyShift = 114;

    /// <summary>Server to client: body shift state change, minutes left, or a refusal code.</summary>
    public const byte BodyShiftNotice = 207;

    /// <summary>Both ways: a thrown event item (Water Ball, snowball, Lantern Cannon) with its target position in map pixels.</summary>
    public const byte Throw = 117;

    /// <summary>Server to client: shows a fireworks effect on a player.</summary>
    public const byte FireworkShow = 148;

    /// <summary>Client to server: uses a Potential Pill on the player or on a pet.</summary>
    public const byte Potential = 126;

    /// <summary>Client to server: uses the Anti-Potential Pill on the player or on a pet.</summary>
    public const byte PotentialReturn = 130;

    /// <summary>Server to client: the new potential value of a pet after a Potential Pill.</summary>
    public const byte PotentialResult = 213;

    /// <summary>Server to client: a pet's potential was set back to 0.</summary>
    public const byte PotentialReturned = 216;

    /// <summary>Client to server: repairs a piece of bag equipment with a repair tool from another bag slot.</summary>
    public const byte Repair = 84;

    /// <summary>Server to client: shows the item worn by a player's out pet; a weapon is drawn in its hand.</summary>
    public const byte PetItemShown = 69;

    /// <summary>Server to client: a player's out pet puts its weapon away.</summary>
    public const byte PetItemHidden = 70;

    /// <summary>Server to client: the new wear of a worn piece; 250 means destroyed.</summary>
    public const byte EquipWear = 27;

    /// <summary>Server to client: a worn piece of the player or of a pet was destroyed.</summary>
    public const byte EquipBroken = 35;

    /// <summary>Server to client: the new wear of a piece worn by a pet.</summary>
    public const byte PetEquipWear = 28;

    /// <summary>Client to server: the Remote Control item was double clicked in a bag slot.</summary>
    public const byte AutoPlayOpen = 76;

    /// <summary>Client to server: starts auto play with the client's settings blob.</summary>
    public const byte AutoPlayStart = 78;

    /// <summary>Client to server: stops auto play.</summary>
    public const byte AutoPlayStop = 79;

    /// <summary>Client to server: fuel bank request: put, take or use one row.</summary>
    public const byte FuelBank = 134;

    /// <summary>Both ways: the bag sort request of the QoL client and the server's replies (see <c>SortHandlers</c>).</summary>
    public const byte QolSort = 250;

    /// <summary>Server to client: the fuel bank row as it is after a put or a take.</summary>
    public const byte FuelBankNotice = 219;

    /// <summary>Client to server: leaves a vehicle from a bag slot in a parking lot cell.</summary>
    public const byte Park = 92;

    /// <summary>Client to server: takes a vehicle back out of the parking lot.</summary>
    public const byte Redeem = 93;

    /// <summary>Client to server: repairs the vehicle in a bag slot.</summary>
    public const byte VehicleRepair = 95;

    /// <summary>Server to client: result of a vehicle repair: form opened, new wear, failure or not enough points.</summary>
    public const byte VehicleRepairResult = 81;

    /// <summary>Server to client: the four parking lot records.</summary>
    public const byte ParkList = 153;

    /// <summary>Server to client: one parking lot record.</summary>
    public const byte ParkSlot = 154;

    /// <summary>Server to client: the outcome code of a parking action, followed by the vehicle item id for some codes.</summary>
    public const byte ParkResult = 155;

    /// <summary>Parking lot sub code; nothing in the server sends or handles it.</summary>
    public const byte ParkClear = 156;

    /// <summary>Server to client: opens the parking lot form.</summary>
    public const byte ParkOpen = 157;

    /// <summary>Client to server: the Remote Control drinks a potion, a bag item use without the usual messages.</summary>
    public const byte AutoPlaySupply = 82;

    /// <summary>Client to server: auto play stopped itself because no revive item was left.</summary>
    public const byte AutoPlayGaveUp = 81;

    /// <summary>Client to server: fuses a piece of equipment with a fusion scroll.</summary>
    public const byte Reinforce = 121;

    /// <summary>Server to client: result of a fusion: the new fusion level, or failure.</summary>
    public const byte ReinforceResult = 210;

    /// <summary>Server to client: result of the sewing kit: new look set, failed, reverted or revert failed.</summary>
    public const byte Pose = 105;

    /// <summary>Client to server: raises the mounted stone of a piece of equipment with a Chaos Crystal.</summary>
    public const byte ConfuseStone = 127;

    /// <summary>Server to client: result of <see cref="ConfuseStone"/>: new step, bag slot and the mounted stone.</summary>
    public const byte ConfuseResult = 214;

    /// <summary>Client to server: takes the mounted stone out of a piece of equipment with a plier item.</summary>
    public const byte Dismantle = 86;

    /// <summary>Server to client: the wear record of the dismantled piece; the client empties its stone.</summary>
    public const byte DismantleResult = 143;

    /// <summary>Client to server: the Space Capsule form confirms; the packet names no slot.</summary>
    public const byte Capsule = 125;

    /// <summary>Client to server: sends a pet to find a given item with a dog bone.</summary>
    public const byte Forage = 88;

    /// <summary>Client to server: cancels the pet's search.</summary>
    public const byte ForageCancel = 91;

    /// <summary>Server to client: the pet found an item, with item id and count.</summary>
    public const byte ForageFound = 146;

    /// <summary>Server to client: the search ended, with the bone slot, the pet slot and a failed or finished code.</summary>
    public const byte ForageEnd = 147;

    /// <summary>Server to client: the dog bone in that bag slot is in use.</summary>
    public const byte ForageBone = 152;

    /// <summary>Client to server: searches the reference material, paid with a glass or with points.</summary>
    public const byte Refer = 103;

    /// <summary>Server to client: result of <see cref="Refer"/>: the found codes, or "can't be searched".</summary>
    public const byte ReferResult = 166;

    /// <summary>Client to server: uses the Win Plate from a bag slot in a battle.</summary>
    public const byte WinPlate = 106;

    /// <summary>Client to server: asks for the Training Island time effect state, or teleports to the island.</summary>
    public const byte TrainingIsland = 111;

    /// <summary>Server to client: Training Island time effect: started, over, already running or time limit.</summary>
    public const byte TrainingState = 106;

    /// <summary>Client to server: magnet form requests: ask a target, agree, refuse or report a reason it cannot.</summary>
    public const byte Magnet = 90;

    /// <summary>Server to client: magnet form messages for the requester and the target, and the outcome code.</summary>
    public const byte MagnetNotice = 80;

    /// <summary>Server to client: how full a potion bottle is, or the used amount of a bag potion.</summary>
    public const byte BottleFill = 208;

    /// <summary>Client to server: Picnic Box requests: put an item in a cell, take it out or use it.</summary>
    public const byte PicnicBox = 131;

    /// <summary>Client to server: experience bag requests, the counterpart of <see cref="PicnicBox"/>.</summary>
    public const byte ExpBag = 132;

    /// <summary>Server to client: a Picnic Box cell was filled, emptied or used.</summary>
    public const byte PicnicNotice = 217;

    /// <summary>Server to client: an experience bag cell changed, like <see cref="PicnicNotice"/> without the used amounts.</summary>
    public const byte ExpBagNotice = 218;

    /// <summary>Client to server: the message sent with a Passion Bouquet to a target player.</summary>
    public const byte FlowerMessage = 118;

    /// <summary>Client to server: the message sent with a Bliss Lamp, like <see cref="FlowerMessage"/>.</summary>
    public const byte BlessMessage = 129;

    /// <summary>Server to client: outcome of a Passion Bouquet for the sender, or the target and the scene.</summary>
    public const byte FlowerResult = 209;

    /// <summary>Server to client: outcome of a Bliss Lamp: the blessing for the target, or a refusal code.</summary>
    public const byte BlessResult = 215;

    /// <summary>Client to server: opens a Sweets Pack from a bag slot.</summary>
    public const byte OpenSweets = 74;

    /// <summary>Client to server: opens a pack or box from a bag slot with a key item from another slot.</summary>
    public const byte OpenBox = 75;

    /// <summary>Client to server: opens a mall box.</summary>
    public const byte OpenBox2 = 128;

    /// <summary>Client to server: bathes in a tub of a house, naming the house owner and the furniture index.</summary>
    public const byte Bathe = 55;

    /// <summary>Client to server: leaves the tub.</summary>
    public const byte StopBathe = 56;

    /// <summary>Client to server: the player sat on a chair or started on a training machine.</summary>
    public const byte UseFurniture = 71;

    /// <summary>Client to server: the player got up from the furniture.</summary>
    public const byte StopFurniture = 72;

    /// <summary>Client to server: starts auto collecting at a map NPC; the server has nothing to answer.</summary>
    public const byte AutoCollect = 73;

    /// <summary>Server to client: these players stopped collecting automatically.</summary>
    public const byte AutoCollectStopped = 137;

    /// <summary>Server to client: these players began collecting automatically.</summary>
    public const byte AutoCollectStarted = 138;

    /// <summary>Server to client: that player bathes in the tub at the given furniture and seat.</summary>
    public const byte Bathing = 73;

    /// <summary>Server to client: that player left the tub.</summary>
    public const byte BatheEnd = 74;

    /// <summary>Server to client: the pets of the riders in the scene, sent as a player's scene loads.</summary>
    public const byte RidePetList = 116;

    /// <summary>Server to client: that player uses the furniture at the given index and seat.</summary>
    public const byte FurnitureUsed = 75;

    /// <summary>Server to client: that player got up from the furniture.</summary>
    public const byte FurnitureLeft = 76;

    /// <summary>Server to client: Treadmill and Bicep Exerciser training sub-packets.</summary>
    public const byte Training = 162;

    /// <summary>Client to server: picks up the ground item with that index.</summary>
    public const byte PickUp = 2;

    /// <summary>Server to client: removes a ground item, optionally with the pick-up animation.</summary>
    public const byte GroundRemove = 2;

    /// <summary>Server to client: a new ground item with its position.</summary>
    public const byte GroundAdd = 3;

    /// <summary>Server to client: the ground items of the scene, as fixed-size records.</summary>
    public const byte GroundList = 4;

    /// <summary>Server to client: the whole bag, as fixed-size records.</summary>
    public const byte BagList = 5;

    /// <summary>Server to client: tells the player which item and how many were obtained, followed by the bag record fields.</summary>
    public const byte ItemObtained = 6;

    /// <summary>Client to server: drops a bag item onto the ground.</summary>
    public const byte Drop = 3;

    /// <summary>Client to server: throws an amount of gold (at least 10) onto the ground.</summary>
    public const byte ThrowMoney = 16;

    /// <summary>Server to client: a gold pile appears on the ground, optionally with the drop animation.</summary>
    public const byte GroundMoneyAdd = 18;

    /// <summary>Server to client: the item cannot be picked up; no payload.</summary>
    public const byte CannotPickUp = 25;

    /// <summary>Client to server: uses an item or a skill from a hotkey, with the caster and the target.</summary>
    public const byte HotkeyUse = 20;

    /// <summary>Client to server: uses a Forgotten or Lethe Scroll to take an attribute point back from the player or a pet.</summary>
    public const byte ForgetAbility = 83;

    /// <summary>Both ways: stat buff items: the client asks the minutes left or uses one, the server reports the minutes or an effect.</summary>
    public const byte StatBuff = 119;

    /// <summary>Client to server: uses a bag item on the player or a pet.</summary>
    public const byte Use = 15;

    /// <summary>Server to client: plays the "use item" sound; no payload.</summary>
    public const byte UsedSound = 15;

    /// <summary>Client to server: destroys a bag item after the confirmation asked by <see cref="DiscardConfirm"/>.</summary>
    public const byte Discard = 124;

    /// <summary>Server to client: removes a count of an item from the bag by item id.</summary>
    public const byte BagRemoveById = 7;

    /// <summary>Server to client: adds one bag record.</summary>
    public const byte BagAdd = 8;

    /// <summary>Server to client: lowers the count in one bag slot.</summary>
    public const byte BagSlotDecrease = 9;

    /// <summary>Both ways: moves a count of items from one bag slot to another.</summary>
    public const byte BagMove = 10;

    /// <summary>Client to server: wears the item in a bag slot.</summary>
    public const byte Equip = 11;

    /// <summary>Server to client: the worn equipment, as fixed-size records.</summary>
    public const byte EquipList = 11;

    /// <summary>Client to server: takes a worn piece off into a bag slot.</summary>
    public const byte Unequip = 12;

    /// <summary>Client to server: destroys a worn piece.</summary>
    public const byte DiscardEquipped = 13;

    /// <summary>Client to server: puts a bag item on a pet.</summary>
    public const byte PetEquip = 17;

    /// <summary>Client to server: takes a piece off a pet.</summary>
    public const byte PetUnequip = 18;

    /// <summary>Client to server: destroys a piece worn by a pet.</summary>
    public const byte PetDiscardEquipped = 19;

    /// <summary>Server to client: a pet's equipment slot is now empty.</summary>
    public const byte PetEquipCleared = 21;

    /// <summary>Server to client: a pet's piece moved into the given bag slot.</summary>
    public const byte PetUnequipToBag = 22;

    /// <summary>Server to client: a bag item moved onto a pet.</summary>
    public const byte PetEquipFromBag = 23;

    /// <summary>Both ways: opens the player's stall (client request, server confirmation with the stall name and sign).</summary>
    public const byte StallOpen = 30;

    /// <summary>Server to client: the stalls open in the scene.</summary>
    public const byte StallList = 31;

    /// <summary>Server to client: that player's stall closed.</summary>
    public const byte StallClosed = 32;

    /// <summary>Server to client: the items offered in a stall the player is browsing.</summary>
    public const byte StallItems = 33;

    /// <summary>Server to client: an item of the player's stall was sold.</summary>
    public const byte StallSale = 34;

    /// <summary>Client to server: the player stands on water; no data.</summary>
    public const byte EnterWater = 51;

    /// <summary>Client to server: the player left the water.</summary>
    public const byte LeaveWater = 52;

    /// <summary>Client to server: starts fishing with a rod from a bag slot.</summary>
    public const byte FishStart = 53;

    /// <summary>Client to server: stops fishing.</summary>
    public const byte FishStop = 54;

    /// <summary>Server to client: that player stopped fishing.</summary>
    public const byte FishingStopped = 122;

    /// <summary>Server to client: that player started fishing.</summary>
    public const byte FishingStarted = 123;

    /// <summary>Client to server: opens a stall of large items and vehicles.</summary>
    public const byte UtensilStallOpen = 65;

    /// <summary>Client to server: closes the large-item stall.</summary>
    public const byte UtensilStallClose = 66;

    /// <summary>Client to server: browses another player's large-item stall.</summary>
    public const byte UtensilStallBrowse = 67;

    /// <summary>Client to server: buys items from a large-item stall.</summary>
    public const byte UtensilStallBuy = 68;

    /// <summary>Server to client: confirms the player's own large-item stall.</summary>
    public const byte UtensilStallOpened = 130;

    /// <summary>Server to client: the large-item stalls open in the scene.</summary>
    public const byte UtensilStallList = 131;

    /// <summary>Server to client: that player's large-item stall closed.</summary>
    public const byte UtensilStallClosed = 132;

    /// <summary>Server to client: the items offered by a large-item stall the player is browsing.</summary>
    public const byte UtensilStallOffer = 133;

    /// <summary>Server to client: an item of the player's large-item stall was sold.</summary>
    public const byte UtensilStallSale = 134;

    /// <summary>Client to server: opens a pet stall with the pets and their prices.</summary>
    public const byte PetStallOpen = 60;

    /// <summary>Client to server: closes the pet stall.</summary>
    public const byte PetStallClose = 61;

    /// <summary>Client to server: browses another player's pet stall.</summary>
    public const byte PetStallBrowse = 62;

    /// <summary>Client to server: buys a pet from a pet stall.</summary>
    public const byte PetStallBuy = 63;

    /// <summary>Server to client: confirms the player's own pet stall.</summary>
    public const byte PetStallOpened = 110;

    /// <summary>Server to client: the pet stalls open in the scene.</summary>
    public const byte PetStallList = 111;

    /// <summary>Server to client: that player's pet stall closed.</summary>
    public const byte PetStallClosed = 112;

    /// <summary>Server to client: the pets offered by a pet stall the player is browsing.</summary>
    public const byte PetStallOffer = 113;

    /// <summary>Server to client: a pet of the player's stall was sold.</summary>
    public const byte PetStallSale = 114;

    /// <summary>Client to server: closes the player's stall.</summary>
    public const byte StallClose = 31;

    /// <summary>Client to server: browses another player's stall.</summary>
    public const byte StallBrowse = 32;

    /// <summary>Client to server: buys items from a stall.</summary>
    public const byte StallBuy = 33;

    /// <summary>Client to server: combines a main item with sub items in the compound form.</summary>
    public const byte Compound = 14;

    /// <summary>Client to server: the compound request in its junior alchemy form; handled like <see cref="Compound"/>.</summary>
    public const byte CompoundJunior = 87;

    /// <summary>Client to server: the compound request in its high alchemy form; handled like <see cref="Compound"/>.</summary>
    public const byte CompoundHigh = 101;

    /// <summary>Client to server: the compound request in its blessing form; handled like <see cref="Compound"/>.</summary>
    public const byte CompoundBless = 115;

    /// <summary>Client to server: dyes a bag item with red, green and blue levels from 0 to 9.</summary>
    public const byte Dye = 38;

    /// <summary>Server to client: the first colour code of a player.</summary>
    public const byte ColourCode1 = 53;

    /// <summary>Server to client: the second colour code of a player.</summary>
    public const byte ColourCode2 = 103;

    /// <summary>Server to client: a stone is set in the piece of equipment in that bag slot.</summary>
    public const byte CrystalMounted = 77;

    /// <summary>Server to client: result items of a compound, each flying into a bag cell.</summary>
    public const byte CompoundResult = 13;

    /// <summary>Server to client: a worn slot is now empty.</summary>
    public const byte EquipSlotCleared = 19;

    /// <summary>Server to client: a worn piece moved into the given bag slot.</summary>
    public const byte UnequipToBag = 16;

    /// <summary>Server to client: a bag item moved onto the player, with the bag slot that became free.</summary>
    public const byte EquipFromBag = 17;

    /// <summary>Client to server: uses a tent palette item.</summary>
    public const byte TentStyle = 94;

    /// <summary>Client to server: moves bag items into the city warehouse.</summary>
    public const byte WareDeposit = 34;

    /// <summary>Client to server: moves warehouse items back into the bag.</summary>
    public const byte WareWithdraw = 35;

    /// <summary>Server to client: warehouse items, as fixed-size records added to their slots.</summary>
    public const byte WareList = 43;

    /// <summary>Server to client: a warehouse slot is now empty.</summary>
    public const byte WareSlotClear = 45;

    /// <summary>Server to client: opens the warehouse form; no data.</summary>
    public const byte WareOpen = 46;

    /// <summary>Server to client: opens the ATM deposit and withdraw form, or the transfer form.</summary>
    public const byte AtmForm = 165;

    /// <summary>Client to server: exchanges IM points for gold.</summary>
    public const byte PointExchange = 109;

    /// <summary>Server to client: gold paid per point; shows the exchange form.</summary>
    public const byte ExchangeRate = 200;

    /// <summary>Server to client: the gold after an exchange and the amount gained.</summary>
    public const byte ExchangeGold = 201;

    /// <summary>Server to client: a tent palette was refused, with a reason code.</summary>
    public const byte TentStyleRefused = 158;

    /// <summary>Both ways: Memory Card and puppet training packets.</summary>
    public const byte Puppet = 104;

    /// <summary>Client to server: uses the Delay Clock from a bag slot in a battle.</summary>
    public const byte DelayClock = 107;

    /// <summary>Client to server: uses the Time Mantle from a bag slot at home while an item is being made.</summary>
    public const byte TimeMantle = 108;

    /// <summary>Client to server: Present Case requests: pack an item or open a packed box.</summary>
    public const byte Packing = 112;

    /// <summary>Server to client: packing finished.</summary>
    public const byte PackingResult = 107;

    /// <summary>Client to server: a click in the Fish Urn's water.</summary>
    public const byte FishFeed = 97;

    /// <summary>Client to server: a fish tank request.</summary>
    public const byte FishAct = 98;

    /// <summary>Server to client: fish tank state and lists, selected by a second sub byte.</summary>
    public const byte FishTank = 160;

    /// <summary>Server to client: space contract, hotel panel, tent and garage state, selected by a second sub byte.</summary>
    public const byte SpaceContract = 136;

    /// <summary>Server to client: opens the tent style chooser for a bag slot.</summary>
    public const byte TentStyleChooser = 159;

    /// <summary>Client to server: a packet whose meaning is not known.</summary>
    public const byte EnterWorld66 = 66;

    /// <summary>Client to server: the chat window mode was applied; no payload.</summary>
    public const byte TalkFormReady = 77;
}
