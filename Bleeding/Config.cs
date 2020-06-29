using System;
using System.Xml.Serialization;

namespace Bleeding {
	[Serializable()]
	[XmlRoot(ElementName = "Config")]
	public class Config {
		[XmlElement(ElementName = "PercentageBled")]
		public double PercentageBled { get; set; }
		[XmlElement(ElementName = "MinimumDamage")]
		public float MinimumDamage { get; set; }
		[XmlElement(ElementName = "BleedRate")]
		public double BleedRate { get; set; }
		[XmlElement(ElementName = "SecondsBetweenTicks")]
		public int SecondsBetweenTicks { get; set; }
		[XmlElement(ElementName = "CutMultiplier")]
		public double CutMultiplier { get; set; }
		[XmlElement(ElementName = "BluntMultiplier")]
		public double BluntMultiplier { get; set; }
		[XmlElement(ElementName = "PierceMultiplier")]
		public double PierceMultiplier { get; set; }
		[XmlElement(ElementName = "InvalidMultiplier")]
		public double InvalidMultiplier { get; set; }
		[XmlElement(ElementName = "SlowOnBleed")]
		public SlowOnBleed SlowOnBleed { get; set; }
		[XmlElement(ElementName = "StaggerOnTick")]
		public StaggerOnTick StaggerOnTick { get; set; }
		[XmlElement(ElementName = "ReducedForNPCs")]
		public ReducedForNPCs ReducedForNPCs { get; set; }
		[XmlElement(ElementName = "DisabledForPlayer")]
		public bool DisabledForPlayer { get; set; }
		[XmlElement(ElementName = "DisplayPlayerEffects")]
		public bool DisplayPlayerEffects { get; set; }
		[XmlElement(ElementName = "BodyMultipliers")]
		public BodyMultipliers BodyMultipliers { get; set; }
		[XmlElement(ElementName = "Bandages")]
		public Bandages Bandages { get; set; }
		[XmlElement(ElementName = "Debug")]
		public bool Debug { get; set; }
	}

	[XmlRoot(ElementName = "ReducedForNPCs")]
	public class ReducedForNPCs {
		[XmlAttribute(AttributeName = "enabled")]
		public bool Enabled { get; set; }
		[XmlText]
		public double Value { get; set; }
	}

	[XmlRoot(ElementName = "Bandages")]
	public class Bandages {
		[XmlAttribute(AttributeName = "enabled")]
		public bool Enabled { get; set; }
		[XmlElement(ElementName = "PlayerCount")]
		public int PlayerCount { get; set; }
		[XmlElement(ElementName = "NpcCount")]
		public int NpcCount { get; set; }
		[XmlElement(ElementName = "MinimumMedicine")]
		public int MinimumMedicine { get; set; }
		[XmlElement(ElementName = "KeyCode")]
		public int KeyCode { get; set; }
	}

	[Serializable()]
	[XmlRoot(ElementName = "StaggerOnTick")]
	public class StaggerOnTick {
		[XmlAttribute(AttributeName = "enabled")]
		public bool Enabled { get; set; }
		[XmlText]
		public double Value { get; set; }
	}

	[Serializable()]
	[XmlRoot(ElementName = "SlowOnBleed")]
	public class SlowOnBleed {
		[XmlAttribute(AttributeName = "enabled")]
		public bool Enabled { get; set; }
		[XmlText]
		public float Value { get; set; }
	}

	[XmlRoot(ElementName = "BodyMultipliers")]
	public class BodyMultipliers {
		[XmlElement(ElementName = "Head")]
		public Head Head { get; set; }
		[XmlElement(ElementName = "Neck")]
		public Neck Neck { get; set; }
		[XmlElement(ElementName = "Arms")]
		public Arms Arms { get; set; }
		[XmlElement(ElementName = "Shoulders")]
		public Shoulders Shoulders { get; set; }
		[XmlElement(ElementName = "Legs")]
		public Legs Legs { get; set; }
	}


	[Serializable()]
	[XmlRoot(ElementName = "Head")]
	public class Head {
		[XmlAttribute(AttributeName = "mult")]
		public double Mult { get; set; }
	}

	[Serializable()]
	[XmlRoot(ElementName = "Neck")]
	public class Neck {
		[XmlAttribute(AttributeName = "mult")]
		public double Mult { get; set; }
	}

	[Serializable()]
	[XmlRoot(ElementName = "Arms")]
	public class Arms {
		[XmlAttribute(AttributeName = "mult")]
		public double Mult { get; set; }
	}

	[Serializable()]
	[XmlRoot(ElementName = "Shoulders")]
	public class Shoulders {
		[XmlAttribute(AttributeName = "mult")]
		public double Mult { get; set; }
	}

	[Serializable()]
	[XmlRoot(ElementName = "Legs")]
	public class Legs {
		[XmlAttribute(AttributeName = "mult")]
		public double Mult { get; set; }
	}
}