namespace RealmRpgBot.Models.Inventory
{
	using System.Collections.Generic;

	using Character;

	/// <summary>
	/// Item definition
	/// </summary>
	public class Item
	{
		public string Id { get; set; }
		public string DisplayName { get; set; }
		public string Description { get; set; }
		public string UsedResponse { get; set; }
		public ItemTypes Type { get; set; }
		public List<ItemEffect> Effects { get; set; }

		// Equipable Items
		public string EquipmentSlot { get; set; }
		public int AttackBonus { get; set; }
		public int DefenceBonus { get; set; }

		public enum ItemTypes
		{
			Junk,
			Recipe,
			Resource,
			Consumable,
			Equipment
		}

		public enum EquipmentSlots
		{
			Head,
			Chest,
			Hands,
			Shoulders,
			Pants,
			Feet,
			MainHand,
			OffHand
		}

		public void UseOnSelf(Player player)
		{
			var log = Serilog.Log.ForContext<Item>();

			if (Type != ItemTypes.Consumable) return;

			foreach (var eff in Effects)
			{
				if (int.TryParse(eff.Parameters["Amount"].ToString(), out int amount) == false)
				{
					log.Warning("Effect {effname} for item {id} has no (correct) amount declared", eff.Effect, Id);
					continue;
				}

				switch (eff.Effect)
				{
					case ItemEffect.ItemEffects.Restore:
						var targetResource = eff.GetParameter<string>("TargetResource");

						if (targetResource == null)
						{
							log.Warning("Effect {effname} for item {id} has no (correct) target resource declared", eff.Effect, Id);
							continue;
						}

						switch (targetResource.ToLower())
						{
							case "hp": player.HealHpAsync(amount).ConfigureAwait(false); break;
							case "mp": player.RestoreMpAsync(amount).ConfigureAwait(false); break;
							default:
								log.Warning("No valid resource target found for effect {eff} on item {id} (current: {val})",
									eff.Effect, Id, targetResource);
								break;
						}
						break;
					default:
						log.Warning("Bad effect definition in item {id}", Id);
						break;
				}
			}
		}
	}
}
