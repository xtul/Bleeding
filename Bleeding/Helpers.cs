using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bleeding {
	public static class Helpers {
		public static void Say(string text) {
			InformationManager.DisplayMessage(new InformationMessage(text, new Color(134, 114, 250)));
		}
	}
}
