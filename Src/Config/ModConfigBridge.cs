using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace EnchantedOfferings;

internal static class ModConfigBridge
{
    private static bool _available;
    private static bool _registered;
    private static Type? _apiType;
    private static Type? _entryType;
    private static Type? _configTypeEnum;

    internal static bool IsAvailable => _available;

    internal static void DeferredRegister()
    {
        var tree = (SceneTree)Engine.GetMainLoop();
        tree.ProcessFrame += OnNextFrame;
    }

    private static void OnNextFrame()
    {
        var tree = (SceneTree)Engine.GetMainLoop();
        tree.ProcessFrame -= OnNextFrame;
        Detect();
        if (_available) Register();
    }

    private static void Detect()
    {
        try
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Type.EmptyTypes; }
                })
                .ToArray();

            _apiType        = allTypes.FirstOrDefault(t => t.FullName == "ModConfig.ModConfigApi");
            _entryType      = allTypes.FirstOrDefault(t => t.FullName == "ModConfig.ConfigEntry");
            _configTypeEnum = allTypes.FirstOrDefault(t => t.FullName == "ModConfig.ConfigType");
            _available      = _apiType != null && _entryType != null && _configTypeEnum != null;
        }
        catch
        {
            _available = false;
        }
    }

    private static void Register()
    {
        if (_registered) return;
        _registered = true;

        try
        {
            var entries = BuildEntries();

            var displayNames = new Dictionary<string, string>
            {
                ["en"] = "Enchanted Offerings",
            };

            var registerMethod = _apiType!.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == "Register")
                .OrderByDescending(m => m.GetParameters().Length)
                .First();

            if (registerMethod.GetParameters().Length == 4)
                registerMethod.Invoke(null, new object[] { "EnchantedOfferings", displayNames["en"], displayNames, entries });
            else
                registerMethod.Invoke(null, new object[] { "EnchantedOfferings", displayNames["en"], entries });
        }
        catch (Exception e)
        {
            GD.PrintErr($"[EnchantedOfferings] ModConfig registration failed: {e}");
        }
    }

    internal static T GetValue<T>(string key, T fallback)
    {
        if (!_available) return fallback;
        try
        {
            var result = _apiType!.GetMethod("GetValue", BindingFlags.Public | BindingFlags.Static)
                ?.MakeGenericMethod(typeof(T))
                ?.Invoke(null, new object[] { "EnchantedOfferings", key });
            return result != null ? (T)result : fallback;
        }
        catch { return fallback; }
    }

    internal static void SetValue(string key, object value)
    {
        if (!_available) return;
        try
        {
            _apiType!.GetMethod("SetValue", BindingFlags.Public | BindingFlags.Static)
                ?.Invoke(null, new object[] { "EnchantedOfferings", key, value });
        }
        catch { }
    }

    private static Array BuildEntries()
    {
        var list = new List<object>();

        list.Add(Entry(cfg =>
        {
            Set(cfg, "Key", "enabled");
            Set(cfg, "Label", "Enable Card Augments");
            Set(cfg, "Type", EnumVal("Toggle"));
            Set(cfg, "DefaultValue", (object)true);
            Set(cfg, "OnChanged", new Action<object>(v => Settings.Enabled = Convert.ToBoolean(v)));
        }));

        list.Add(Entry(cfg =>
        {
            Set(cfg, "Key", "modChance");
            Set(cfg, "Label", "Mod Percent Chance (Default 10)");
            Set(cfg, "Type", EnumVal("Slider"));
            Set(cfg, "DefaultValue", (object)10f);
            Set(cfg, "Min", 0f);
            Set(cfg, "Max", 100f);
            Set(cfg, "Step", 1f);
            Set(cfg, "Format", "F0");
            Set(cfg, "OnChanged", new Action<object>(v => Settings.ModChance = Convert.ToSingle(v)));
        }));

        foreach (var (key, label, def) in new[]
        {
            ("commonWeight",   "Common Mod Weight (Default 3)",      3f),
            ("uncommonWeight", "Uncommon Mod Weight (Default 2)",     2f),
            ("rareWeight",     "Rare Mod Weight (Default 1)",         1f),
            ("rarityBias",     "Card Rarity Weight Bias (Default 1)", 1f),
        })
        {
            var (k, l, d) = (key, label, def);
            list.Add(Entry(cfg =>
            {
                Set(cfg, "Key", k);
                Set(cfg, "Label", l);
                Set(cfg, "Type", EnumVal("Slider"));
                Set(cfg, "DefaultValue", (object)d);
                Set(cfg, "Min", 1f);
                Set(cfg, "Max", 20f);
                Set(cfg, "Step", 1f);
                Set(cfg, "Format", "F0");
                Set(cfg, "OnChanged", new Action<object>(v => Settings.Set(k, Convert.ToSingle(v))));
            }));
        }

        foreach (var (key, label) in new[]
        {
            ("modifyStarter", "Modify Starter Cards"),
            ("modifyInstant", "Modify Instant Obtain Cards"),
            ("modifyShop",    "Modify Shop Cards"),
        })
        {
            var (k, l) = (key, label);
            list.Add(Entry(cfg =>
            {
                Set(cfg, "Key", k);
                Set(cfg, "Label", l);
                Set(cfg, "Type", EnumVal("Toggle"));
                Set(cfg, "DefaultValue", (object)true);
                Set(cfg, "OnChanged", new Action<object>(v => Settings.Set(k, Convert.ToBoolean(v))));
            }));
        }

        var result = Array.CreateInstance(_entryType!, list.Count);
        for (int i = 0; i < list.Count; i++) result.SetValue(list[i], i);
        return result;
    }

    private static object Entry(Action<object> configure)
    {
        var inst = Activator.CreateInstance(_entryType!)!;
        configure(inst);
        return inst;
    }

    private static void Set(object obj, string name, object value)
        => obj.GetType().GetProperty(name)?.SetValue(obj, value);

    private static Dictionary<string, string> L(string en, string zhs)
        => new() { ["en"] = en, ["zhs"] = zhs };

    private static object EnumVal(string name)
        => Enum.Parse(_configTypeEnum!, name);
}
