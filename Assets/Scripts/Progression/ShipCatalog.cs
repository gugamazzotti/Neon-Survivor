using UnityEngine;

namespace NeonSurvivor
{
    public sealed class ShipDefinition
    {
        public readonly string Id;
        public readonly string Art;
        public readonly int Price;
        public readonly Color Tint;

        public ShipDefinition(string id, string art, int price, Color tint)
        {
            Id = id;
            Art = art;
            Price = price;
            Tint = tint;
        }
    }

    public static class ShipCatalog
    {
        public const string StarterId = "vertex";

        public static readonly ShipDefinition[] Ships =
        {
            new ShipDefinition("vertex", "player-ship", 0, new Color(0.35f, 0.95f, 1f, 1f)),
            new ShipDefinition("blade", "ship-blade", 24, new Color(0.78f, 0.95f, 1f, 1f)),
            new ShipDefinition("bastion", "ship-bastion", 60, new Color(1f, 0.62f, 0.28f, 1f)),
            new ShipDefinition("comet", "ship-comet", 110, new Color(1f, 0.45f, 0.85f, 1f))
        };

        public static int Count => Ships.Length;

        public static int IndexOf(string id)
        {
            for (int i = 0; i < Ships.Length; i++)
            {
                if (Ships[i].Id == id)
                    return i;
            }

            return 0;
        }

        public static bool Owns(int index)
        {
            if (index < 0 || index >= Ships.Length)
                return false;
            if (Ships[index].Price <= 0)
                return true;
            return PlayerPrefs.GetInt(SaveProfile.Prefix + "shipown." + Ships[index].Id, 0) == 1;
        }

        public static int SelectedIndex()
        {
            string id = PlayerPrefs.GetString(SaveProfile.Prefix + "ship", StarterId);
            int index = IndexOf(id);
            if (!Owns(index))
                return 0;
            return index;
        }

        public static ShipDefinition Selected => Ships[SelectedIndex()];

        public static bool TryBuy(int index)
        {
            if (!Application.isPlaying || index <= 0 || index >= Ships.Length || Owns(index))
                return false;
            if (!SaveProfile.TrySpend(Ships[index].Price))
                return false;

            PlayerPrefs.SetInt(SaveProfile.Prefix + "shipown." + Ships[index].Id, 1);
            PlayerPrefs.Save();
            return true;
        }

        public static void Select(int index)
        {
            if (!Application.isPlaying || !Owns(index))
                return;

            PlayerPrefs.SetString(SaveProfile.Prefix + "ship", Ships[index].Id);
            PlayerPrefs.Save();
        }

        public static bool CanPurchase(int price, int coins, bool owned)
        {
            return !owned && price > 0 && coins >= price;
        }
    }
}
