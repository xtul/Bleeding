﻿using System;
using System.Xml.Serialization;

namespace Bleeding {
	[Serializable()]
	[XmlRoot(ElementName = "Config")]
	public class Config {
		[XmlElement(ElementName = "PercentageBled")]
		public decimal PercentageBled { get; set; }
		[XmlElement(ElementName = "MinimumDamage")]
		public float MinimumDamage { get; set; }
		[XmlElement(ElementName = "BleedRate")]
		public decimal BleedRate { get; set; }
		[XmlElement(ElementName = "SecondsBetweenTicks")]
		public int SecondsBetweenTicks { get; set; }
		[XmlElement(ElementName = "CutMultiplier")]
		public decimal CutMultiplier { get; set; }
		[XmlElement(ElementName = "BluntMultiplier")]
		public decimal BluntMultiplier { get; set; }
		[XmlElement(ElementName = "PierceMultiplier")]
		public decimal PierceMultiplier { get; set; }
		[XmlElement(ElementName = "InvalidMultiplier")]
		public decimal InvalidMultiplier { get; set; }
		[XmlElement(ElementName = "SlowOnBleed")]
		public SlowOnBleed SlowOnBleed { get; set; }
		[XmlElement(ElementName = "DisplayPlayerEffects")]
		public bool DisplayPlayerEffects { get; set; }
		[XmlElement(ElementName = "Debug")]
		public bool Debug { get; set; }
	}

	[Serializable()]
	[XmlRoot(ElementName = "SlowOnBleed")]
	public class SlowOnBleed {
		[XmlAttribute(AttributeName = "enabled")]
		public bool Enabled { get; set; }
		[XmlText]
		public float Value { get; set; }
	}
}