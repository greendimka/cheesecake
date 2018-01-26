using Cake.Core.Annotations;
using Cake.Core.IO;
using Cake.Core.Diagnostics;
using System;
using System.Linq;

namespace CheeseCake
{
	[CakeAliasCategory("CheeseCake")]
	public static class Aliases
	{

		public static string GetProjectFile(string projectName, string dirName = null)
		{
			string rv = null;

			

			if (dirName == null) dirName = projectName;

			// get only C# project files
			var csProjs = GlobberExtensions.GetFiles((rootDir + Directory(dirName)).ToString() + $@"/{projectName}.csproj");
			// get only VB.NET project files
			var vbProjs = GlobberExtensions.GetFiles((rootDir + Directory(dirName)).ToString() + $@"/{projectName}.vbproj");
			// merge project files into single collection
			var projs = Enumerable.Concat(csProjs, vbProjs);

			rv = projs.SingleOrDefault()?.FullPath;

			return rv;
		}


		public static Version IncrementVersion(Version version, byte item)
		{
			Version rv;

			switch (item)
			{
				case 0:
				case 1:
					rv = new Version(
						Math.Max(0, version.Major) + 1,
						0);
					break;
				case 2:
					rv = new Version(
						version.Major,
						Math.Max(0, version.Minor) + 1
					);
					break;
				case 3:
					rv = new Version(
						version.Major,
						version.Minor,
						Math.Max(0, version.Build) + 1
					);
					break;
				default:
					rv = new Version(
						version.Major,
						version.Minor,
						version.Build,
						Math.Max(0, version.Revision) + 1
					);
					break;
			}

			return rv;
		}


		public static string GetLastPublishedVersionAsString(string packageName)
		{
			string rv = null;

			Information("Retrieving top published version for package '{0}'.", packageName);
			var package = NuGetList(packageName, new NuGetListSettings
			{
				Prerelease = true,
				Source = new string[] { "local", "DiwareTFS" }
			})
			.Where(x => x.Name == packageName)
			.FirstOrDefault();

			if (package != null)
			{
				rv = package.Version;
				Information("Top published version for package '{0}' is {1}.", packageName, rv);
			}
			else
			{
				Information("Package '{0}' is not published.", packageName);
			}

			return rv;
		}


		public static Version SemVerToVersion(string semVer)
		{
			if (semVer == null) return null;

			Version rv = null;

			while (semVer.Length > 0)
			{
				if (Version.TryParse(semVer, out rv)) break;

				semVer = semVer.Substring(0, semVer.Length - 1);
			}

			return rv;
		}


		public static Version ReadTopVersion(string filePath)
		{
			var proj = XElement.Load(filePath, LoadOptions.PreserveWhitespace);

			var topVer = proj
				.Descendants("Version")
				.Select(node => new Version(node.Value))
				.DefaultIfEmpty(null)
				.Max();

			return topVer;
		}


		public static void WriteVersion(string filePath, Version newVer, Version oldVer = null)
		{
			string sXml = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
			var lines = sXml.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
			int addLines = 0;
			for (var i = lines.Length - 1; i >= 0; i--)
			{
				if (lines[i] == string.Empty)
				{
					addLines++;
				}
				else
				{
					break;
				}
			}

			// var proj = XElement.Load(filePath, LoadOptions.PreserveWhitespace);
			var proj = XElement.Parse(sXml, LoadOptions.PreserveWhitespace);

			IEnumerable<XElement> oldVersionNodes;
			if (oldVer != null)
			{
				oldVersionNodes = proj
					.Descendants("Version")
					.Where(node => node.Value == oldVer.ToString())
					.ToList();
			}
			else
			{
				oldVersionNodes = proj
					.Descendants("Version")
					.ToList();
			}

			foreach (var vNode in oldVersionNodes)
			{
				vNode.Value = newVer.ToString();
			}

			//proj.Save(filePath, SaveOptions.DisableFormatting);

			string s = proj.ToString(SaveOptions.DisableFormatting);

			for (var i = 0; i < addLines; i++)
			{
				s += Environment.NewLine + string.Empty;
			}

			System.IO.File.WriteAllText(filePath, s, Encoding.UTF8);
		}

	}
}
