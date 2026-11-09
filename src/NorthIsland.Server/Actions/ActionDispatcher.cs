using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Actions;

/// <summary>Creates every action and lets it register its packet routes.</summary>
public static class ActionDispatcher
{
    /// <summary>The action classes, in the order of their action numbers.</summary>
    public static readonly IReadOnlyList<Type> Actions =
        [
            typeof(Action00Hello),
            typeof(Action01System),
            typeof(Action02Chat),
            typeof(Action03PlayerEnters),
            typeof(Action04PlayerPresent),
            typeof(Action05Status),
            typeof(Action06Walk),
            typeof(Action07Jump),
            typeof(Action08Attribute),
            typeof(Action09CreateCharacter),
            typeof(Action10Names),
            typeof(Action11Battle),
            typeof(Action12MapChange),
            typeof(Action13Team),
            typeof(Action14Friends),
            typeof(Action15Pets),
            typeof(Action16Gm),
            typeof(Action18Rename),
            typeof(Action19ActivePet),
            typeof(Action20Event),
            typeof(Action21IdAndPw),
            typeof(Action22MapNpc),
            typeof(Action23Items),
            typeof(Action24Marks),
            typeof(Action25Trade),
            typeof(Action26Gold),
            typeof(Action27Shop),
            typeof(Action28Skills),
            typeof(Action29BankGold),
            typeof(Action30BankItems),
            typeof(Action31Inn),
            typeof(Action32Emotions),
            typeof(Action33Switches),
            typeof(Action34ImPoints),
            typeof(Action35Password),
            typeof(Action36Boards),
            typeof(Action37ServerEcho),
            typeof(Action39Guild),
            typeof(Action40HotKeys),
            typeof(Action41Facility),
            typeof(Action43Alliance),
            typeof(Action44Rebirth),
            typeof(Action45Marriage),
            typeof(Action50BattleActions),
            typeof(Action51MapSkill),
            typeof(Action54Christmas),
            typeof(Action55Cafe),
            typeof(Action57Sports),
            typeof(Action58BiDaXiao),
            typeof(Action59ShiBaDou),
            typeof(Action60Chess),
            typeof(Action61Lotto),
            typeof(Action62Furniture),
            typeof(Action63Account),
            typeof(Action64Manufacture),
            typeof(Action65Tents),
            typeof(Action66Reborn),
            typeof(Action67Prizes),
            typeof(Action68ItemBag),
            typeof(Action69Trips),
            typeof(Action70Castle),
            typeof(Action71Machines),
            typeof(Action72BlackBoard),
            typeof(Action74Gps),
            typeof(Action75Mall),
            typeof(Action76Maid),
            typeof(Action77FleeKill),
            typeof(Action78Computer),
            typeof(Action79Career),
            typeof(Action80PointStore),
            typeof(Action81SpecialFloor),
            typeof(Action82Notices),
            typeof(Action84Top100),
            typeof(Action85Instances),
            typeof(Action86Property),
            typeof(Action87MobileShop),
            typeof(Action88Discussion),
            typeof(Action89ActivityList),
            typeof(Action90GiftCard),
            typeof(Action91LuckyBag),
            typeof(Action104Lottery),
            typeof(Action199Gm),
        ];

    /// <summary>Creates each action from the service provider and has it register its routes on the router.</summary>
    /// <param name="services">The service provider that supplies the actions' constructor arguments.</param>
    /// <param name="router">The router that receives the routes.</param>
    public static void Register(IServiceProvider services, PacketRouter router)
    {
        foreach (var type in Actions)
        {
            var action = (IAction)ActivatorUtilities.CreateInstance(services, type);
            action.Register(new ActionRoutes(router, action.Code));
        }
    }
}
