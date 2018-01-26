using System;
using System.Collections.Generic;
using System.Text;

namespace CheeseCake
{
	public class NuGetTarget
	{
		private string _apiKey = string.Empty;

		public string Source { get; set; }

		public List<ProjectDescriptor> ProjectsToPublish { get; }
			= new List<ProjectDescriptor>();

		public string ApiKey
		{
			get => _apiKey;
			set => _apiKey = value?.Trim() ?? string.Empty;
		}

	}
}
