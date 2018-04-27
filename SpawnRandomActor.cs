using UnityEngine;
using System;
using System.Collections;
using Necro;
using Partiality.Modloader;
using HBS;
using System.Collections.Generic;
using HBS.Text;
using HBS.Collections;
using HBS.Logging;
using HBS.DebugConsole;
using HBS.Pooling;
using MonoMod.ModInterop;
using ZekesNewThrowables;

class SpawnRandomActor : PartialityMod
{
    Dictionary<string, Action<TextFieldParser, EffectDef>> methodParsers;
    private static readonly System.Random getrandom = new System.Random();
    


    public override void Init()
    {
        typeof(Utils).ModInterop();
        On.Necro.DataManager.Awake += (orig, instance) =>
        {
            methodParsers = Utils.GetMethodParsers();
            methodParsers["SpawnRandomZeke"] = new Action<TextFieldParser, EffectDef>(this.ParseGameEffect_SpawnRandomZeke);
            methodParsers["SpawnMinionThrow"] = new Action<TextFieldParser, EffectDef>(this.ParseGameEffect_SpawnMinionThrow);
            orig(instance);
        };
        //this.name = "Zekes New Throwables";
        //this.version = "1.0";
        base.Init();
    }

    public override void OnLoad()
    {
        //this.guiWindowProperties.windowRect.size = new Vector2(500, 500);
        base.OnLoad();
    }

    public static int GetRandomNumber(int min, int max)
    {
        lock(getrandom) //synchronize
        {
            return getrandom.Next(min, max);
        }
    }

    /*
    public void LoadParsers(object[] funStuff)
    {
        methodParsers = Utils.GetMethodParsers();
        methodParsers["SpawnRandomZeke"] = new Action<TextFieldParser, EffectDef>(this.ParseGameEffect_SpawnRandomZeke);
        methodParsers["SpawnMinionThrow"] = new Action<TextFieldParser, EffectDef>(this.ParseGameEffect_SpawnMinionThrow);
    }*/

    private void ParseGameEffect_SpawnRandomZeke(TextFieldParser parser, EffectDef def)
    {
        Debug.Log("Hey are you parsing SpawnRandomZeke?");
        def.worldMethod = CreateMethod_SpawnRandomZeke();
    }

    private void ParseGameEffect_SpawnMinionThrow(TextFieldParser parser, EffectDef def)
    {
        Debug.Log("Hey are you parsing SpawnMinonThrow?");
        TagWeights tagWeights = Utils.ParseTagWeights(parser, "Params", true);
        if (tagWeights == null)
        {
            def.worldMethod = GameEffectManager.CreateMethod_LogMessage(LogLevel.Error, "Broken SpawnMinionThrow GameEffect for: " + def.id);
        }
        else
        {
            int level;
            string tagAndWeight = tagWeights.GetTagAndWeight(0, out level);
            def.worldMethod = CreateMethod_SpawnMinionThrow(tagAndWeight, level, Utils.TryParseFloat(parser, "Radius", null), Utils.TryParseFloat(parser, "Duration", null));
        }
    }

    private static GameEffectWorldMethod CreateMethod_SpawnRandomZeke()
    {
        return delegate (EffectContext context)
        {
            if (!LazySingletonBehavior<NetworkManager>.Instance.IsSimulationServer)
            {
                return;
            }
            string actorId;
            int randomActor = GetRandomNumber(1, 11);
            if (randomActor == 11)
                actorId = "CrystalMantis";
            else if (randomActor == 10)
                actorId = "GemeaterWelpMinion";
            else if (randomActor == 9)
                actorId = "GemeaterWelp";
            else if (randomActor == 8)
                actorId = "DropSpiderlingMinion";
            else if (randomActor == 7)
            {
                int randomScrounge = GetRandomNumber(1, 3);
                if(randomScrounge == 1)
                    actorId = "ScroungeNormal";
                else if(randomScrounge == 2)
                    actorId = "ScroungeFull";
                else
                    actorId = "ScroungeBomb";
            }
            else if (randomActor == 6)
                actorId = "DropSpiderlingMinion";
            else if (randomActor == 5)
                actorId = "DropSpiderling";
            else if (randomActor == 4)
                actorId = "ShadowBornJuvenile";
            else if (randomActor == 3)
                actorId = "ShadowBornMinion";
            else if (randomActor == 2)
                actorId = "BoneEffigy";
            else
                actorId = "BoneMinion";
            LazySingletonBehavior<ActorManager>.Instance.Spawn(actorId, 0, Actor.Faction.Enemy, context.hitPoint, context.hitRot, null);
            Debug.Log("Thrown Mystery Spawn -" + actorId);

            Debug.Log("Hey you did it!");
        };
    }

    private static GameEffectWorldMethod CreateMethod_SpawnMinionThrow(string actorDefId, int level, float range, float amount)
    {
        return delegate (EffectContext context)
        {
            Actor sourceActor = context.sourceActor;
            WeaponBody sourceWeapon = context.sourceWeapon;
            if (!LazySingletonBehavior<NetworkManager>.Instance.IsSimulationServer)
            {
                return;
            }
            SpawnMinions spawnMinions = sourceWeapon as SpawnMinions;
            for (int i = 0; i < (int)amount; i++)
            {
                Vector3 zero = Vector3.zero;
                Quaternion rotation = sourceActor.Rotation;
                if (!SpatialUtil.TryFindRandomSpotCircle(context.hitPoint, context.Radius, range, sourceActor.Height, out zero)) 
                {
                    Debug.Log("Couln't find valid spawn spot");
                }
                else
                {
                    LazySingletonBehavior<ActorManager>.Instance.Spawn(actorDefId, level, Actor.Faction.Player, /*context.hitPoint*/ zero, /*context.hitRot*/ rotation, delegate (Actor actor)
                    {
                        if (spawnMinions != null && spawnMinions.minionPrefab != null)
                        {
                            GameObject gameObject = spawnMinions.minionPrefab.Spawn(actor.transform);
                            Minion component = gameObject.GetComponent<Minion>();
                            if (component != null)
                            {
                                spawnMinions.AttachMinion(component);
                            }
                        }
                            Debug.Log(string.Concat(new object[]
                            {
                                    "Spawned ",
                                    actor.DebugName,
                                    " at ",
                                    actor.transform.position
                            }), actor);
                    });
                }
            }
        };
    }

    /*
    public override void OnGUI(int windowID)
    {
        GUILayout.Label("Why hello there Adventurer! I'm glad my message has been able to reach you successfully. My name is Zeke, creator of all things strange yet throwable! " +
            Environment.NewLine + Environment.NewLine + " I know you're probably wondering, 'Why has the great and mystical Zeke sent for my attention?' Well it just so happens that that my apprentice has managed to sneak in my" +
            " new wares past Brazen's all seeing eye, so I'm here to spread the good news to you! You should start to see a few new recipies and throwables around, such as: "
            + Environment.NewLine + Environment.NewLine + "Zeke's Brash Hatch Jar" + Environment.NewLine + "Zeke's Mystery Hatch Jar");
    }
    */




}





