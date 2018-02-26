namespace RealmRpgBot.Models
{
	public class TrainedSkill
	{
		public string Id { get; set; }
		public int Rank { get; set; }

		public TrainedSkill() { }
		public TrainedSkill(string skillId, int rank)
		{
			Id = skillId;
			Rank = rank;
		}

		public override string ToString() => $"{Id} (rank {Rank})";
	}
}
